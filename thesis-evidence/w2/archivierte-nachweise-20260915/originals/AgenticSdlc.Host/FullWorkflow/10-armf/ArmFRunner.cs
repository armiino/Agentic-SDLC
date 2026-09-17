using System.Diagnostics;
using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.ArmF;

/// <summary>
/// W2 Arm F — der freie Ziel-Agent auf der Ledger-Aufgabe (Evaluationskonzept §11, Mess-Arm,
/// KEIN Kettenglied, kein Gate, kein Wahrheits-Write). Fairness-Verdrahtung (§12.0):
/// dieselbe kanonisch nummerierte Quelle wie der Ledger (AU-Locators aus dem GETEILTEN
/// deterministischen Segmenter), dasselbe Modell (jury.judgeModel-Präferenz wie die
/// Ledger-Runner — Konfigurations-Falle!), großzügiges Werkzeug-Budget, keine vorgegebenen
/// Stufen. Artefakte: runs/arm-f/&lt;id&gt;/{source-units.json, output.json,
/// process-profile.json, metrics.json} + die normalen logs/.
/// </summary>
public static class ArmFRunner
{
    private const int DefaultToolBudget = 40;

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3 || !string.Equals(args[1], "run", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Usage: arm-f run <transcript> [--model <id>] [--tool-budget <n>]   (W2-Mess-Arm; LLM-Kosten!)");
            return 2;
        }

        var transcriptArg = args[2];
        string? modelArg = null;
        var toolBudget = DefaultToolBudget;
        for (var i = 3; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--model", StringComparison.OrdinalIgnoreCase)) modelArg = args[i + 1];
            if (string.Equals(args[i], "--tool-budget", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(args[i + 1], out var b) && b > 0) toolBudget = b;
        }

        var transcriptPath = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, transcriptArg);
        if (!File.Exists(transcriptPath))
        { Console.Error.WriteLine($"[arm-f] Transkript fehlt: {transcriptPath}"); return 2; }

        // Modell-Präferenz WIE die Ledger-Runner (jury.judgeModel vor agentModel) — sonst verglichen
        // F und L/LCR verschiedene Modelle und der ganze Arm wäre wertlos (run-config-Falle, CLAUDE.md).
        var model = modelArg ?? settings.JuryJudgeModel ?? settings.ModelId;
        var genSettings = settings with { ModelId = model };

        var source = Path.GetFileName(transcriptPath);
        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var units = AtomicUnitSegmenter.Segment(transcript, source);

        var run = new RunContext(RunId.New(), "arm-f");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "ArmF",
            runId = run.RunId,
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            source,
            units = units.Count,
            provider = settings.LlmProvider,
            model,
            toolBudget,
            variant = ArmFAgents.Variant,
            prompt = ArmFAgents.PromptName,
            captureReasoning = settings.ReasoningCapture.ToString(),
            timestampUtc = DateTime.UtcNow
        });
        File.WriteAllText(Path.Combine(run.RunDir, "source-units.json"),
            JsonSerializer.Serialize(new AtomicUnitFixture(units), JsonFiles.Json));
        Console.WriteLine($"[arm-f] Lauf {run.RunId}: {source} ({units.Count} Units) model={model} toolBudget={toolBudget}");

        var tools = new ArmFTools(units, toolBudget);
        var stopReason = "no_submission";
        var watch = Stopwatch.StartNew();
        ModelRoundCounter rounds;

        // OTel-Exporter in eigenem Scope: erst nach Dispose ist otel-traces.jsonl geflusht und lesbar.
        var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: "AgenticSdlc.Host",
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
        try
        {
            var (agent, r) = ArmFAgents.Build(repoRoot, settings, genSettings, run, tools.Build());
            rounds = r;
            var auftrag = "Hier ist das kanonisch nummerierte Meeting-Transkript. Bearbeite die Aufgabe aus deinen Instruktionen.\n\n"
                          + ArmFTools.Render(units);
            try
            {
                await agent.RunAsync([new ChatMessage(ChatRole.User, auftrag)]).ConfigureAwait(false);
                stopReason = tools.Submitted is not null ? "submitted"
                    : tools.BudgetExhausted ? "budget" : "no_submission";
            }
            catch (Exception ex)
            {
                // R-18-Lehre: Fehler LAUT — inkl. Event, damit der Lauf als Beleg lesbar bleibt.
                stopReason = "error";
                run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
                Console.Error.WriteLine($"[arm-f] Lauf FEHLGESCHLAGEN: {ex.Message}");
            }
        }
        finally { otel?.Dispose(); }
        watch.Stop();

        if (tools.Submitted is { } statements)
            File.WriteAllText(Path.Combine(run.RunDir, "output.json"),
                JsonSerializer.Serialize(new ArmFResult(run.RunId, source, model, ArmFAgents.Variant,
                    statements, tools.SubmittedBilanz ?? []), JsonFiles.Json));

        File.WriteAllText(Path.Combine(run.RunDir, "process-profile.json"), JsonSerializer.Serialize(
            new ArmFProcessProfile(rounds.Rounds, tools.ToolCalls, tools.SourceReads, tools.ValidationCalls,
                tools.Submissions, tools.SelfRevisions, toolBudget, tools.BudgetExhausted, stopReason), JsonFiles.Json));

        var (tokensIn, tokensOut) = SumTokens(Path.Combine(run.LogsDir, "otel-traces.jsonl"));
        File.WriteAllText(Path.Combine(run.RunDir, "metrics.json"), JsonSerializer.Serialize(
            new ArmFMetrics(run.RunId, source, model, watch.ElapsedMilliseconds,
                tools.Submitted?.Count ?? 0, tokensIn, tokensOut), JsonFiles.Json));

        Console.WriteLine($"[arm-f] {(stopReason == "submitted" ? "FERTIG" : $"ENDE OHNE ERGEBNIS ({stopReason})")}: "
            + $"{tools.Submitted?.Count ?? 0} Aussagen · {rounds.Rounds} Modellrunden · {tools.ToolCalls} Tool-Calls "
            + $"({tools.SourceReads} Re-Reads, {tools.ValidationCalls} Validierungen, {tools.SelfRevisions} Selbstrevisionen) — runs/arm-f/{run.RunId}");
        return stopReason switch { "submitted" => 0, "error" => 3, _ => 2 };
    }

    /// <summary>Token-Summe aus den OTel-Spans (gen_ai.usage.*) — dasselbe Leseformat wie der
    /// pipeline-full-<c>MetricsFinalizer</c> (dort privat; hier bewusst lokal, das gemessene
    /// System bleibt unangetastet). (null, null) wenn kein OTel-Beleg existiert.</summary>
    internal static (long? In, long? Out) SumTokens(string tracesPath)
    {
        if (!File.Exists(tracesPath)) return (null, null);
        long tin = 0, tout = 0;
        var gefunden = false;
        foreach (var line in File.ReadLines(tracesPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (!doc.RootElement.TryGetProperty("name", out var n)
                    || n.GetString()?.StartsWith("chat", StringComparison.Ordinal) != true) continue;
                if (!doc.RootElement.TryGetProperty("tags", out var tags) || tags.ValueKind != JsonValueKind.Object) continue;
                if (LongTag(tags, "gen_ai.usage.input_tokens") is { } i) { tin += i; gefunden = true; }
                if (LongTag(tags, "gen_ai.usage.output_tokens") is { } o) { tout += o; gefunden = true; }
            }
            catch (JsonException) { /* fremde/abgeschnittene Zeile — überspringen */ }
        }
        return gefunden ? (tin, tout) : (null, null);
    }

    /// <summary>OTel-Tags kommen je nach Exporter als Zahl ODER String (gleiches Duo wie im MetricsFinalizer).</summary>
    private static long? LongTag(JsonElement tags, string key)
    {
        if (!tags.TryGetProperty(key, out var v)) return null;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.TryGetInt64(out var n) ? n : null,
            JsonValueKind.String => long.TryParse(v.GetString(), out var s) ? s : null,
            _ => null
        };
    }
}
