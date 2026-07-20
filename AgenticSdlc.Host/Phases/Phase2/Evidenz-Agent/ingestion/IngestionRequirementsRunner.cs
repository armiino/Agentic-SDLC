using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// CLI: ingest-requirements <meeting-delta project-state.json> [modelId] [--out <dir>] [--dry-run]
// Loest die Requirements eines neuen Meetings gegen den Core auf (Resolver-Agent -> Gate -> Plan).
// MAF-nativ (Workflow wie Cluster/Clarify). Der eigentliche Upsert in den Core folgt in Block C (Apply).
public static class IngestionRequirementsRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "RequirementIngestionAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        var maxAttempts = 2;
        string? deltaToken = null;
        string? outputDir = null;
        string? modelArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { outputDir = args[++i]; continue; }
            if (string.Equals(arg, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (arg.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[ingest-requirements] unbekanntes Argument: {arg}"); Usage(); return 2; }
            if (deltaToken is null) deltaToken = arg; else modelArg ??= arg;
        }
        if (string.IsNullOrWhiteSpace(deltaToken)) { Usage(); return 2; }

        // Core (die Wahrheit) laden.
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[ingest-requirements] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        // MeetingDelta = ProjectStateDocument des neuen Meetings (Extraktion nur des neuen Transkripts).
        var deltaPath = Path.IsPathRooted(deltaToken) ? deltaToken : Path.Combine(repoRoot, deltaToken);
        if (!File.Exists(deltaPath))
        {
            Console.Error.WriteLine($"[ingest-requirements] MeetingDelta nicht gefunden: {deltaPath}");
            return 2;
        }
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "ingestion");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("plan") : (Path.IsPathRooted(outputDir) ? outputDir : Path.Combine(repoRoot, outputDir));
        var deltaRel = Path.GetRelativePath(repoRoot, deltaPath);
        run.WriteConfig(new
        {
            workflow = RequirementIngestionWorkflow.WorkflowName,
            runId = run.RunId,
            meetingDelta = deltaRel,
            provider = settings.LlmProvider,
            model = genSettings.ModelId,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            dryRun,
            timestampUtc = DateTime.UtcNow
        });

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "RequirementIngestionAgent1", vars);
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        ICandidateRetriever retriever = new ShowAllRequirementRetriever();

        var workflow = RequirementIngestionWorkflow.Build(
            new IngestionResolveExecutor(factory, retriever, run),
            new IngestionGateExecutor(run),
            new IngestionRepairExecutor(factory, retriever, run),
            new IngestionFinalizeExecutor(run, outDir));

        var incomingReq = delta.Items.Count(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase));
        var coreReq = core.Items.Count(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase));

        if (dryRun)
        {
            Console.WriteLine("[ingest-requirements] --dry-run: Graph Build()-bar (Resolve -> Gate --[repairable]--> Repair (Loop) / Finalize). Kein LLM.");
            Console.WriteLine($"[ingest-requirements] incoming-req={incomingReq} core-req={coreReq} -> {deltaRel}");
            Console.WriteLine($"[ingest-requirements] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[ingest-requirements] running runId={run.RunId} model={genSettings.ModelId} incoming-req={incomingReq} core-req={coreReq}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new IngestionResolveInput(delta, core, deltaRel, maxAttempts), run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ingest-requirements] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var summaryPath = Path.Combine(outDir, "ingestion-summary.json");
        var planPath = Path.Combine(outDir, "plan.json");
        if (!File.Exists(summaryPath) || !File.Exists(planPath))
        {
            Console.Error.WriteLine("[ingest-requirements] Ausfuehrung unvollstaendig: plan.json/ingestion-summary.json fehlt.");
            return 4;
        }

        Console.WriteLine($"[ingest-requirements] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return 0;
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: ingest-requirements <meeting-delta project-state.json> [modelId] [--out <dir>] [--dry-run]");
}
