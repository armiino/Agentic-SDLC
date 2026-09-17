using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// A10: hebt die per Adjudikation NEU geminteten Claims (facetStatus=pending) auf Pipeline-Niveau.
///   ledger-adjudicate-refine &lt;consumable.json&gt; [transcript.txt] [model] [out.json]
/// Das Filtern der pending-Claims ist DETERMINISTISCH (keine LLM-Kosten für bereits facettierte Claims);
/// nur die pending-Claims gehen an den <see cref="FacetAssigner"/>. Alles andere läuft unverändert durch.
/// </summary>
public static class LedgerAdjudicateRefineRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ledger-adjudicate-refine <consumable.json> [transcript.txt] [model] [out.json]");
            return 2;
        }
        var consumablePath = Resolve(repoRoot, args[1]);
        if (!File.Exists(consumablePath)) { Console.Error.WriteLine($"[refine] consumable fehlt: {consumablePath}"); return 2; }

        var transcript = args.Length >= 3 && File.Exists(Resolve(repoRoot, args[2]))
            ? await File.ReadAllTextAsync(Resolve(repoRoot, args[2])).ConfigureAwait(false)
            : null;
        var modelArg = args.Length >= 4 ? args[3] : null;
        var outPath = args.Length >= 5 ? Resolve(repoRoot, args[4]) : consumablePath; // Default: in-place.

        var consumable = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consumablePath), Json);
        if (consumable is null) { Console.Error.WriteLine("[refine] consumable nicht lesbar."); return 2; }

        // DETERMINISTISCHER Filter (geteilter A10-Kern): nur pending-Claims (keine LLM-Kosten für den Rest).
        var pendingCount = AdjudicationRefine.CountPending(consumable);
        Console.WriteLine($"[refine] consumable claims={consumable.Claims.Count} pending={pendingCount}");
        if (pendingCount == 0)
        {
            Console.WriteLine("[refine] nichts zu tun (keine facetStatus=pending Claims) — kein LLM-Call.");
            return 0;
        }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        Console.WriteLine($"[refine] Facetten-Zuweisung für {pendingCount} Claim(s) mit model={judgeSettings.ModelId} (transcript={(transcript is null ? "nein" : "ja")})");

        // W1b: Judge-Call observability-verdrahtet (ChatDecisionLogger + InputContext + OTel) über AgentChatPipelineBuilder.Build.
        var run = new RunContext(RunId.New(), "ledger-adjudicate-refine");
        run.EnsureFolders();
        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
        var refineClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "FacetAssigner", SourceName);
        var assigner = new FacetAssigner(refineClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        AdjudicationRefineResult result;
        try
        {
            result = await AdjudicationRefine.RefineAsync(consumable, assigner.AssignAllAsync, transcript, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[refine] Facetten-Zuweisung fehlgeschlagen: {ex.Message}");
            return 4;
        }

        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(result.Consumable, Json)).ConfigureAwait(false);

        var pendingIds = consumable.Claims
            .Where(c => string.Equals(c.FacetStatus, "pending", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Id)
            .ToHashSet(StringComparer.Ordinal);
        Console.WriteLine($"[refine] refined={result.RefinedCount} nochPending={result.StillPending} -> {Path.GetRelativePath(repoRoot, outPath)}");
        foreach (var r in result.Consumable.Claims.Where(c => pendingIds.Contains(c.Id)))
            Console.WriteLine($"[refine]   {r.Id}: kind={r.Kind} status={r.Status} modality={r.Modality} timeScope={r.TimeScope}");
        return 0;
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
