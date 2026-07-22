using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Branch;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;
using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;

/// <summary>
/// E-d-CLI: <c>baseline-fanout [consumable.json] [model] [--artifacts requirements,risks] [--k N] [--min-votes N]
///   [--max N] [--dry-run]</c>.
/// Fan-out des Ledgers auf mehrere Artefakt-Zweige (E-c) parallel, Fan-in-Barrier zu einem Verified Baseline Set.
/// Runs unter <c>runs/baseline-fanout/&lt;runId&gt;/</c>. Jeder Zweig ist reiner Reuse (Evidence-Agent + CheckerRepair
/// + ID-Gate), hier per <c>BindAsExecutor</c> als EIN Fan-out-Knoten gebunden. <c>--dry-run</c> baut nur den Graphen.
/// </summary>
/// <remarks>
/// Arm-B-only. Aktuell unterstützte Artefakttypen: die mit Evidence-Prompt (requirements, risks). Pro Zweig eigene,
/// nach Artefakttyp benannte Logging-Clients (Maker/Checker/Repair) → getrennte Logs je Zweig. Exit: 0 = ok/dry-run,
/// 2 = Usage/IO, 3 = Workflow-Fehler, 4 = Konfig/LLM-Fehler.
/// </remarks>
public static class BaselineFanOutRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string SharedCorePrompt = "_shared-core";

    internal static readonly IReadOnlyDictionary<string, (string Agent, string Disposition)> ArtifactMap =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = ("EvidenceRequirementsAgent", "requirements"),
            ["risks"] = ("EvidenceRisksAgent", "risks"),
            ["architecture"] = ("EvidenceArchitectureAgent", "architecture"),
            ["open-questions"] = ("EvidenceOpenQuestionsAgent", "open-questions"),
        };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? consumableArg = null, modelArg = null;
        int k = 3, minVotes = 0, maxIter = 3;
        var artifactsArg = "requirements,risks";
        var dryRun = false;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--artifacts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) artifactsArg = args[++i];
            else if (string.Equals(a, "--k", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) k = ParseInt(args[++i], k);
            else if (string.Equals(a, "--min-votes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) minVotes = ParseInt(args[++i], minVotes);
            else if (string.Equals(a, "--max", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) maxIter = ParseInt(args[++i], maxIter);
            else if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) dryRun = true;
            else if (a.StartsWith("--", StringComparison.Ordinal)) { /* ignorieren */ }
            else if (consumableArg is null) consumableArg = a;
            else if (modelArg is null) modelArg = a;
        }
        k = Math.Max(1, k);
        maxIter = Math.Max(1, maxIter);

        if (!string.Equals(settings.EvidenceSource, "ledger", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("[baseline-fanout] Arm B: setze evidenceAgent.source=ledger (+ Ledger-Prompts) in run-config.json.");
            return 4;
        }

        var types = artifactsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant()).Distinct().ToList();
        var unknown = types.Where(t => !ArtifactMap.ContainsKey(t)).ToList();
        if (unknown.Count > 0)
        {
            Console.Error.WriteLine($"[baseline-fanout] Artefakttyp(en) ohne Evidence-Prompt: {string.Join(", ", unknown)} (verfügbar: {string.Join(", ", ArtifactMap.Keys)}).");
            return 2;
        }

        var consPath = Resolve(repoRoot, consumableArg ?? settings.EvidenceLedgerRun);
        if (consPath is null || !File.Exists(consPath))
        {
            Console.Error.WriteLine($"[baseline-fanout] consumable.json fehlt/ungesetzt: '{consumableArg ?? settings.EvidenceLedgerRun}'.");
            return 2;
        }
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0)
        {
            Console.Error.WriteLine("[baseline-fanout] consumable.json leer/nicht lesbar.");
            return 2;
        }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var run = new RunContext(RunId.New(), "baseline-fanout");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = BaselineFanOutWorkflow.WorkflowName,
            runId = run.RunId,
            arm = "ledger",
            artifacts = types,
            consumable = Path.GetRelativePath(repoRoot, consPath),
            claims = ledger.Claims.Count,
            provider = settings.LlmProvider,
            makerModel = settings.ModelId,
            checkerModel = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter,
            structuredOutput = settings.JuryStructuredOutput,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[baseline-fanout] runId={run.RunId} artifacts=[{string.Join(",", types)}] claims={ledger.Claims.Count} makerModel={settings.ModelId} checkerModel={judgeSettings.ModelId} k={k} max={maxIter}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var makerBase = ChatClientFactory.Create(settings);
        var judgeBase = ChatClientFactory.Create(judgeSettings);

        // Einen Zweig je Artefakttyp bauen (reiner Reuse: Evidence-Agent + CheckerRepair + ID-Gate).
        var branches = new List<(string ArtifactType, Microsoft.Agents.AI.Workflows.Workflow Branch)>();
        foreach (var type in types)
        {
            var (agentName, dispositionKey) = ArtifactMap[type];
            var branch = BuildBranch(
                type, agentName, dispositionKey, ledger, k, minVotes, maxIter,
                settings, judgeSettings, makerBase, judgeBase, run, repoRoot);
            branches.Add((type, branch));
        }

        var workflow = BaselineFanOutWorkflow.Build(branches, run);

        if (dryRun)
        {
            Console.WriteLine($"[baseline-fanout] --dry-run: Fan-out-Graph Build()-bar (Dispatch ─fan-out─► [{string.Join(", ", types)}] ─barrier─► Collector). Kein LLM-Lauf.");
            Console.WriteLine($"[baseline-fanout] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        var sourceBlock = "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + EvidenceLedgerProjection.Project(ledger.Claims, "requirements");
        // Hinweis: die Projektion ist disposition-annotiert; jeder Zweig-Prompt liest die für ihn relevante Disposition.
        // (Für Fan-out reicht EINE Projektion; die Zweige unterscheiden sich über Prompt + Checker-dispositionKey.)

        Console.WriteLine("[baseline-fanout] running fan-out (dispatch -> [branches] -> barrier -> collector)...");
        Microsoft.Agents.AI.Workflows.Run wfRun;
        try
        {
            wfRun = await InProcessExecution.Default.RunAsync(workflow, sourceBlock, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[baseline-fanout] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var hasFailure = RecordEvents(run, wfRun);
        var status = await wfRun.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        run.AppendEvent(new { type = "WORKFLOW_FINISHED", runId = run.RunId, workflow = BaselineFanOutWorkflow.WorkflowName, status = status.ToString(), hasFailure, timestampUtc = DateTime.UtcNow });

        if (hasFailure)
        {
            Console.Error.WriteLine("[baseline-fanout] Executor/Workflow-Fehler — siehe events.jsonl.");
            Console.WriteLine($"[baseline-fanout] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        // Wahrheit von Disk: baseline-set.json (vom Collector).
        var setPath = Path.Combine(run.RunDir, "baseline-set.json");
        if (File.Exists(setPath))
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(setPath).ConfigureAwait(false));
            foreach (var a in doc.RootElement.GetProperty("artifacts").EnumerateArray())
                Console.WriteLine($"[baseline-fanout]   {a.GetProperty("artifactType").GetString()}: artifactId={a.GetProperty("artifactId").GetString()} items={a.GetProperty("items").GetInt32()}");
            Console.WriteLine($"[baseline-fanout] Verified Baseline Set: {doc.RootElement.GetProperty("artifacts").GetArrayLength()} Artefakt(e) -> baseline-set.json");
        }
        else
        {
            Console.Error.WriteLine("[baseline-fanout] WARN: baseline-set.json fehlt — Fan-in unvollständig?");
        }
        Console.WriteLine($"[baseline-fanout] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    internal static Microsoft.Agents.AI.Workflows.Workflow BuildBranch(
        string artifactType, string agentName, string dispositionKey, ConsumableLedger ledger,
        int k, int minVotes, int maxIter, HostSettings settings, HostSettings judgeSettings,
        IChatClient makerBase, IChatClient judgeBase, RunContext run, string repoRoot)
    {
        var makerClient = AgentChatPipelineBuilder.Build(makerBase, settings, run, $"BranchMaker-{artifactType}", SourceName);
        var checkerClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"Checker-{artifactType}", SourceName);
        var repairClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"Repair-{artifactType}", SourceName);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var header = PromptProvider.Load(repoRoot, Phase, agentName, settings.GetPromptName(agentName), vars);
        var core = PromptProvider.Load(repoRoot, Phase, agentName, SharedCorePrompt, vars);
        var instructions = header.TrimEnd() + "\n\n" + core;

        IChatClient chat = makerClient;
        AIAgent baseAgent = chat.AsAIAgent(instructions: instructions, name: agentName, tools: []);
        var agent = baseAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var critic = new ContractCritic(checkerClient, settings.JuryStructuredOutput);
        var repair = new ContractRepair(repairClient, settings.JuryStructuredOutput);

        return ArtifactBranchWorkflow.Build(
            agent, critic, repair, ledger, artifactType, dispositionKey, k, minVotes, maxIter, settings.ModelId, run);
    }

    private static bool RecordEvents(RunContext run, Microsoft.Agents.AI.Workflows.Run wfRun)
    {
        var hasFailure = false;
        foreach (var e in wfRun.OutgoingEvents)
        {
            switch (e)
            {
                case ExecutorFailedEvent f:
                    hasFailure = true;
                    run.AppendEvent(new { type = "EXECUTOR_FAILED", runId = run.RunId, executorId = f.ExecutorId, error = f.Data?.Message, timestampUtc = DateTime.UtcNow });
                    break;
                case WorkflowErrorEvent err:
                    // SubworkflowErrorEvent leitet hiervon ab -> Sub-Workflow-Fehler werden hier miterfasst.
                    hasFailure = true;
                    run.AppendEvent(new { type = "WORKFLOW_ERROR", runId = run.RunId, eventType = err.GetType().Name, error = err.Exception?.Message, timestampUtc = DateTime.UtcNow });
                    break;
            }
        }
        return hasFailure;
    }

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;
    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
