using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// CLI: pbi-update <ingestion-run> [model] [--dry-run]
// Incrementeller PBI-Update als MAF-Workflow (Derive -> Maker -> Gate -> Finalize, austauschbare Executor-Knoten,
// siehe PbiUpdateWorkflow). Konsumiert das Ingestion-Delta (applied/delta.json).
public static class PbiUpdateRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "PbiPlacementAgent";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var dryRun = args.Contains("--dry-run");
        var maxAttempts = 2;
        string? token = null, modelArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pbi-update] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) { Usage(); return 2; }

        var ingestPlanDir = IngestionReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = ingestPlanDir is null ? null : Path.Combine(ingestPlanDir, "applied", "delta.json");
        if (deltaPath is null || !File.Exists(deltaPath))
        {
            Console.Error.WriteLine($"[pbi-update] Ingestion-Delta (applied/delta.json) fuer '{token}' nicht gefunden - erst ingest-apply fahren.");
            return 2;
        }
        var delta = await LoadAsync<IngestionApplyReport>(deltaPath).ConfigureAwait(false);
        var sourceIngestionRun = Path.GetFileName(Path.GetDirectoryName(ingestPlanDir!) ?? ingestPlanDir!);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pbi-update] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "pbi-update");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
            Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "PbiPlacementAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var workflow = PbiUpdateWorkflow.Build(
            new PbiUpdateDeriveExecutor(run),
            new PbiUpdateMakerExecutor(factory, run),
            new PbiUpdateGateExecutor(run),
            new PbiUpdateRepairExecutor(factory, run),
            new PbiUpdateFinalizeExecutor(run));

        var ctx = new PbiUpdateWfContext(core, delta.Applied, sourceIngestionRun, outDir, dryRun, maxAttempts);

        Console.WriteLine($"[pbi-update] running runId={run.RunId} model={genSettings.ModelId} dryRun={dryRun} maxAttempts={maxAttempts} (Derive->Maker->Gate->[Repair]/Finalize)");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, ctx, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[pbi-update] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var summaryPath = Path.Combine(outDir, "pbi-update-summary.json");
        if (!File.Exists(summaryPath)) { Console.Error.WriteLine("[pbi-update] Ausfuehrung unvollstaendig: summary fehlt."); return 4; }
        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
        var s = summary.RootElement;
        var gatePass = s.GetProperty("gatePass").GetBoolean();
        Console.WriteLine($"[pbi-update] deterministic={s.GetProperty("deterministic").GetInt32()} unplaced={s.GetProperty("unplaced").GetInt32()} placements={s.GetProperty("placements").GetInt32()} operations={s.GetProperty("operations").GetInt32()} gate={(gatePass ? "pass" : "fail")} errors={s.GetProperty("gateErrors").GetInt32()} attempts={s.GetProperty("attempts").GetInt32()} finalDecision={s.GetProperty("finalDecision").GetString()}");
        Console.WriteLine($"[pbi-update] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return gatePass ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage() => Console.Error.WriteLine("Usage: pbi-update <ingestion-run> [modelId] [--dry-run]");
}
