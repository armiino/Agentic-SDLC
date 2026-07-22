using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// I-c-CLI: <c>inference-check &lt;derived-risks.json&gt; &lt;requirements.artifact.json&gt; [model] [out.json]</c>.
/// Prüft die abgeleiteten Risiken semantisch gegen ihre zitierten Anker-Requirements (Relevanz/Nicht-Widerspruch/
/// Scope) — der LLM-Teil hinter der deterministischen Anker-Validierung (I-b). Schreibt
/// <c>inference-check-report.json</c>. Exit: 0 = alle supported, 1 = geflaggte (contradicts/unrelated) → HumanReview,
/// 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Ehrlich: schwache Garantie (LLM-Relevanz), der Mensch bleibt tragend (I-d). Der Checker filtert die klar
/// untragbaren Ableitungen; alles andere ist mindestens review-pflichtig. KEIN Coverage-Check (open-world).
/// Middleware/Logging über die bestehende Pipeline (logs/agents/EvidenceInferenceChecker).
/// </remarks>
public static class InferenceCheckRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string AgentName = "EvidenceInferenceChecker";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: inference-check <derived-risks.json> <requirements.artifact.json> [model] [out.json]");
            return 2;
        }

        var derivedPath = Resolve(repoRoot, args[1]);
        var basePath = Resolve(repoRoot, args[2]);
        if (derivedPath is null || !File.Exists(derivedPath)) { Console.Error.WriteLine($"[inference-check] derived-risks fehlt: {args[1]}"); return 2; }
        if (basePath is null || !File.Exists(basePath)) { Console.Error.WriteLine($"[inference-check] baseline fehlt: {args[2]}"); return 2; }

        string? modelArg = args.Length >= 4 && !args[3].StartsWith("--", StringComparison.Ordinal) ? args[3] : null;
        string? outArg = args.Length >= 5 ? args[4] : null;

        var derived = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath).ConfigureAwait(false), Json);
        var baseline = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(basePath).ConfigureAwait(false), Json);
        if (derived is null || derived.Items.Count == 0) { Console.Error.WriteLine("[inference-check] derived-risks leer/nicht lesbar."); return 2; }
        if (baseline is null || baseline.Items.Count == 0) { Console.Error.WriteLine("[inference-check] baseline leer/nicht lesbar."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var run = new RunContext(RunId.New(), "derivation");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "InferenceCheck", runId = run.RunId,
            derived = Path.GetRelativePath(repoRoot, derivedPath),
            baseline = Path.GetRelativePath(repoRoot, basePath),
            derivedItems = derived.Items.Count, baselineItems = baseline.Items.Count,
            provider = settings.LlmProvider, model = judgeSettings.ModelId,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[inference-check] runId={run.RunId} derived={derived.Items.Count} baseline={baseline.Items.Count} model={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, AgentName, SourceName);
        var checker = new InferenceChecker(client, settings.JuryStructuredOutput);
        var baselineById = baseline.Items.ToDictionary(i => i.ItemId, i => i, StringComparer.Ordinal);

        InferenceCheckReport report;
        try
        {
            report = await checker.CheckAsync(derived.Items, baselineById, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[inference-check] Prüfung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var outPath = outArg is not null ? Resolve(repoRoot, outArg)! : Path.Combine(run.RunDir, "inference-check-report.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
        run.AppendEvent(new
        {
            type = "INFERENCE_CHECKED", runId = run.RunId, pass = report.Pass, total = report.Total,
            byVerdict = report.ByVerdict, flagged = report.Flagged.Count, timestampUtc = DateTime.UtcNow
        });

        Console.WriteLine($"[inference-check] pass={report.Pass}  " + string.Join("  ", report.ByVerdict.OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}")));
        foreach (var v in report.Flagged)
            Console.WriteLine($"    ! {v.ItemId} [{v.Verdict}] <- [{string.Join(",", v.Anchors)}]  {Trunc(v.Rationale)}");
        Console.WriteLine($"[inference-check] report -> {Path.GetRelativePath(repoRoot, outPath)}  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return report.Pass ? 0 : 1;
    }

    private static string Trunc(string s) => s.Length <= 100 ? s : s[..100] + "…";
    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
