using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.MakerChecker;
using AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Branch;

/// <summary>
/// E-c-CLI: <c>artifact-branch [consumable.json] [model] [--artifact requirements|risks] [--k N] [--min-votes N]
///   [--max N] [--dry-run]</c>.
/// Führt EINEN komponierten Artefakt-Zweig aus (EvidenceBaselineAgent → [CheckerRepair] → AssignIds) und legt die
/// Ergebnisse unter <c>runs/artifact-branch/&lt;runId&gt;/</c> ab. Der erste echte <c>BindAsExecutor</c>-Einsatz:
/// der CheckerRepair-Zyklus wird als EIN Subworkflow-Knoten gebunden. <c>--dry-run</c> baut nur den Graphen
/// (deterministischer Selbsttest, kein LLM).
/// </summary>
/// <remarks>
/// Arm-B-only (Maker konsumiert den Ledger). Pro LLM-Stufe ein eigener geloggter Client (Maker, Checker/Critic,
/// Repair) über die bestehende Pipeline. Exit: 0 = Pass/dry-run, 1 = terminale Restverstöße, 2 = Usage/IO,
/// 3 = Workflow-Fehler, 4 = Konfig/LLM-Fehler.
/// </remarks>
public static class ArtifactBranchRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string SharedCorePrompt = "_shared-core";

    private static readonly IReadOnlyDictionary<string, (string Agent, string Disposition)> ArtifactMap =
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
        var artifact = settings.EvidenceArtifact;
        var dryRun = false;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--k", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) k = ParseInt(args[++i], k);
            else if (string.Equals(a, "--min-votes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) minVotes = ParseInt(args[++i], minVotes);
            else if (string.Equals(a, "--max", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) maxIter = ParseInt(args[++i], maxIter);
            else if (string.Equals(a, "--artifact", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) artifact = args[++i].Trim().ToLowerInvariant();
            else if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) dryRun = true;
            else if (a.StartsWith("--", StringComparison.Ordinal)) { /* ignorieren */ }
            else if (consumableArg is null) consumableArg = a;
            else if (modelArg is null) modelArg = a;
        }
        k = Math.Max(1, k);
        maxIter = Math.Max(1, maxIter);

        if (!string.Equals(settings.EvidenceSource, "ledger", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("[artifact-branch] Arm B: setze evidenceAgent.source=ledger (+ Ledger-Prompt) in run-config.json.");
            return 4;
        }
        if (!ArtifactMap.TryGetValue(artifact, out var map))
        {
            Console.Error.WriteLine($"[artifact-branch] artifact='{artifact}' nicht unterstützt (verfügbar: {string.Join(", ", ArtifactMap.Keys)}).");
            return 2;
        }
        var (agentName, dispositionKey) = map;

        var consPath = Resolve(repoRoot, consumableArg ?? settings.EvidenceLedgerRun);
        if (consPath is null || !File.Exists(consPath))
        {
            Console.Error.WriteLine($"[artifact-branch] consumable.json fehlt/ungesetzt: '{consumableArg ?? settings.EvidenceLedgerRun}'.");
            return 2;
        }
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0)
        {
            Console.Error.WriteLine("[artifact-branch] consumable.json leer/nicht lesbar.");
            return 2;
        }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var run = new RunContext(RunId.New(), "artifact-branch");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = $"ArtifactBranch-{artifact}",
            runId = run.RunId,
            arm = "ledger",
            artifact,
            consumable = Path.GetRelativePath(repoRoot, consPath),
            claims = ledger.Claims.Count,
            provider = settings.LlmProvider,
            makerModel = settings.ModelId,
            checkerModel = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter,
            structuredOutput = settings.JuryStructuredOutput,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[artifact-branch] runId={run.RunId} artifact={artifact} claims={ledger.Claims.Count} makerModel={settings.ModelId} checkerModel={judgeSettings.ModelId} k={k} max={maxIter}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Maker läuft auf dem Config-Modell (Generator); Checker/Repair auf dem Judge-Modell. Je Stufe ein geloggter Client.
        var makerBase = ChatClientFactory.Create(settings);
        var judgeBase = ChatClientFactory.Create(judgeSettings);
        var makerClient = AgentChatPipelineBuilder.Build(makerBase, settings, run, $"BranchMaker-{artifact}", SourceName);
        var checkerClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, CheckerExecutor.ExecutorName, SourceName);
        var repairClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, RepairExecutor.ExecutorName, SourceName);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var header = PromptProvider.Load(repoRoot, Phase, agentName, settings.GetPromptName(agentName), vars);
        var core = PromptProvider.Load(repoRoot, Phase, agentName, SharedCorePrompt, vars);
        var instructions = header.TrimEnd() + "\n\n" + core;
        var agent = BuildAgent(makerClient, instructions, agentName, run);

        var critic = new ContractCritic(checkerClient, settings.JuryStructuredOutput);
        var repair = new ContractRepair(repairClient, settings.JuryStructuredOutput);

        var branch = ArtifactBranchWorkflow.Build(
            agent, critic, repair, ledger, artifact, dispositionKey, k, minVotes, maxIter, settings.ModelId, run);

        if (dryRun)
        {
            Console.WriteLine("[artifact-branch] --dry-run: Zweig-Graph Build()-bar (Maker → [CheckerRepair via BindAsExecutor] → AssignIds). Kein LLM-Lauf.");
            Console.WriteLine($"[artifact-branch] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        var sourceBlock = "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + EvidenceLedgerProjection.Project(ledger.Claims, dispositionKey);

        Console.WriteLine("[artifact-branch] running branch (maker -> [checker-repair] -> assign-ids)...");
        Microsoft.Agents.AI.Workflows.Run branchRun;
        try
        {
            branchRun = await InProcessExecution.Default.RunAsync(branch, sourceBlock, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[artifact-branch] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var hasFailure = RecordEvents(run, branchRun);
        var status = await branchRun.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        run.AppendEvent(new { type = "WORKFLOW_FINISHED", runId = run.RunId, workflow = $"ArtifactBranch-{artifact}", status = status.ToString(), hasFailure, timestampUtc = DateTime.UtcNow });

        if (hasFailure)
        {
            Console.Error.WriteLine("[artifact-branch] Executor/Workflow-Fehler — siehe events.jsonl.");
            Console.WriteLine($"[artifact-branch] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        // Wahrheit von Disk: baselines/{artifact}/ mit artifact.json + CheckerRepair-final-report.json.
        var artifactJson = Path.Combine(run.RunDir, "baselines", artifact, "artifact.json");
        var items = -1; string decision = "UNKNOWN";
        if (File.Exists(artifactJson))
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(artifactJson).ConfigureAwait(false));
            items = doc.RootElement.GetProperty("items").GetArrayLength();
        }
        var finalReport = Path.Combine(run.RunDir, "baselines", artifact, "final-report.json");
        if (File.Exists(finalReport))
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(finalReport).ConfigureAwait(false));
            decision = doc.RootElement.GetProperty("decision").GetString() ?? "UNKNOWN";
        }
        Console.WriteLine($"[artifact-branch] checkerDecision={decision}  artifact.json items={items}");
        Console.WriteLine($"[artifact-branch] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return string.Equals(decision, nameof(ContractDecision.Pass), StringComparison.OrdinalIgnoreCase) ? 0 : 1;
    }

    private static AIAgent BuildAgent(IChatClient chat, string instructions, string agentName, RunContext run)
    {
        AIAgent baseAgent = chat.AsAIAgent(instructions: instructions, name: agentName, tools: []);
        var toolLogger = new ToolCallLoggerMiddleware(run);
        return baseAgent.AsBuilder().Use(toolLogger.InvokeAsync).Build();
    }

    private static bool RecordEvents(RunContext run, Microsoft.Agents.AI.Workflows.Run branchRun)
    {
        var hasFailure = false;
        foreach (var e in branchRun.OutgoingEvents)
        {
            switch (e)
            {
                case ExecutorCompletedEvent c:
                    run.AppendEvent(new { type = "EXECUTOR_FINISHED", runId = run.RunId, executorId = c.ExecutorId, timestampUtc = DateTime.UtcNow });
                    break;
                case ExecutorFailedEvent f:
                    hasFailure = true;
                    run.AppendEvent(new { type = "EXECUTOR_FAILED", runId = run.RunId, executorId = f.ExecutorId, error = f.Data?.Message, timestampUtc = DateTime.UtcNow });
                    break;
                case WorkflowErrorEvent err:
                    hasFailure = true;
                    run.AppendEvent(new { type = "WORKFLOW_ERROR", runId = run.RunId, error = err.Exception?.Message, timestampUtc = DateTime.UtcNow });
                    break;
            }
        }
        return hasFailure;
    }

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;
    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
