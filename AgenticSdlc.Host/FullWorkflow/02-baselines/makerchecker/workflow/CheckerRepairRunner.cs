using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow;

/// <summary>
/// CLI: <c>checker-repair-workflow &lt;artifact.md&gt; &lt;consumable.json&gt; [model] [--k N] [--min-votes N]
///   [--max N] [--artifact requirements|risks] [--dry-run]</c>.
/// Führt den eigenständigen <see cref="CheckerRepairWorkflow"/> als echten MAF-Workflow auf einem bereits
/// erzeugten Artefakt aus (Checker → [Repair-Loop] → Finalize) und legt die Ergebnisse unter
/// <c>runs/checker-repair/&lt;runId&gt;/</c> ab. Das ist die Workflow-Form des zuvor prozeduralen
/// <c>contract-repair</c>-Loops — der Kontroll-Fluss lebt jetzt im Graphen.
/// </summary>
/// <remarks>
/// Bewusst OHNE Generator: das Artefakt kommt aus einer Datei, geprüft wird gegen die übergebene
/// <c>consumable.json</c> (die zitierbare Quelle). Damit ist der Workflow quell-generisch und derselbe Knoten,
/// den man später per <c>BindAsExecutor</c> hinter jeden Generator-Agenten hängt. Provenienz/Logging über die
/// bestehende Infrastruktur (RunContext, AgentChatPipelineBuilder, OtelRunExporters) — pro LLM-Executor ein eigener
/// geloggter Client (Checker/Critic, Repair); MC0 ist deterministisch. <c>--dry-run</c> baut nur den zyklischen
/// Graphen (Selbsttest, kein LLM). Exit: 0 = Pass/dry-run, 1 = terminale Restverstöße, 2 = Usage/IO, 3 = Workflow-
/// Fehler, 4 = LLM-Fehler.
/// </remarks>
public static class CheckerRepairRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: checker-repair-workflow <artifact.md> <consumable.json> [model] [--k N] [--min-votes N] [--max N] [--artifact requirements|risks] [--dry-run]");
            return 2;
        }

        var artPath = Resolve(repoRoot, args[1]);
        var consPath = Resolve(repoRoot, args[2]);
        if (artPath is null || !File.Exists(artPath)) { Console.Error.WriteLine($"[checker-repair] artifact fehlt: {args[1]}"); return 2; }
        if (consPath is null || !File.Exists(consPath)) { Console.Error.WriteLine($"[checker-repair] consumable fehlt: {args[2]}"); return 2; }

        string? modelArg = null;
        int k = 3, minVotes = 0, maxIter = 3;
        var artifactDisposition = settings.EvidenceArtifact;
        var dryRun = false;
        for (var i = 3; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--k", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) k = ParseInt(args[++i], k);
            else if (string.Equals(a, "--min-votes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) minVotes = ParseInt(args[++i], minVotes);
            else if (string.Equals(a, "--max", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) maxIter = ParseInt(args[++i], maxIter);
            else if (string.Equals(a, "--artifact", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) artifactDisposition = args[++i].Trim().ToLowerInvariant();
            else if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) dryRun = true;
            else if (a.StartsWith("--", StringComparison.Ordinal)) { /* unbekanntes Flag ignorieren */ }
            else if (modelArg is null) modelArg = a;
        }
        k = Math.Max(1, k);
        maxIter = Math.Max(1, maxIter);

        var markdown = await File.ReadAllTextAsync(artPath).ConfigureAwait(false);
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0) { Console.Error.WriteLine("[checker-repair] consumable leer/nicht lesbar."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var run = new RunContext(RunId.New(), "checker-repair");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = CheckerRepairWorkflow.WorkflowName,
            runId = run.RunId,
            artifact = Path.GetRelativePath(repoRoot, artPath),
            artifactDisposition,
            consumable = Path.GetRelativePath(repoRoot, consPath),
            claims = ledger.Claims.Count,
            provider = settings.LlmProvider,
            model = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter,
            structuredOutput = settings.JuryStructuredOutput,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[checker-repair] runId={run.RunId} artifact={Path.GetRelativePath(repoRoot, artPath)} disp={artifactDisposition} claims={ledger.Claims.Count} model={judgeSettings.ModelId} k={k} max={maxIter}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Pro LLM-Executor ein eigener, geloggter Client (dieselbe Pipeline wie Ledger/Agenten-Phasen).
        var baseClient = ChatClientFactory.Create(judgeSettings);
        var checkerClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CheckerExecutor.ExecutorName, SourceName);
        var repairClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, RepairExecutor.ExecutorName, SourceName);

        var critic = new ContractCritic(checkerClient, settings.JuryStructuredOutput);
        var repair = new ContractRepair(repairClient, settings.JuryStructuredOutput);

        var scope = $"baselines/{artifactDisposition}";
        var checker = new CheckerExecutor(critic, ledger, artifactDisposition, k, minVotes, maxIter, run, scope);
        var repairExec = new RepairExecutor(repair, ledger, run, scope);
        var finalize = new FinalizeExecutor(run, artifactDisposition, scope);

        var workflow = CheckerRepairWorkflow.Build(checker, repairExec, finalize);

        if (dryRun)
        {
            Console.WriteLine("[checker-repair] --dry-run: zyklischer Graph erfolgreich Build()-bar (Checker→[Repair-Loop]/Finalize). Kein LLM-Lauf.");
            Console.WriteLine($"[checker-repair] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine("[checker-repair] running workflow (checker -> [repair-loop] -> finalize)...");
        Microsoft.Agents.AI.Workflows.Run workflowRun;
        try
        {
            workflowRun = await InProcessExecution.Default
                .RunAsync(workflow, new CheckArtifactMessage(markdown, Iteration: 1), run.RunId, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[checker-repair] Workflow-Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var (hasFailure, result) = RecordWorkflowEvents(run, workflowRun);
        var status = await workflowRun.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        run.AppendEvent(new
        {
            type = "WORKFLOW_FINISHED",
            runId = run.RunId,
            workflow = CheckerRepairWorkflow.WorkflowName,
            workflowSessionId = workflowRun.SessionId,
            status = status.ToString(),
            hasFailure,
            timestampUtc = DateTime.UtcNow
        });

        if (hasFailure)
        {
            Console.Error.WriteLine("[checker-repair] Workflow meldete ExecutorFailed/WorkflowError — siehe events.jsonl.");
            Console.WriteLine($"[checker-repair] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        // final-report.json (vom FinalizeExecutor) ist die Wahrheit. MAF surfaced YieldOutput hier nicht immer als
        // WorkflowOutputEvent in OutgoingEvents (Status Idle), daher NICHT auf das Event verlassen -> Disk lesen
        // (analog LedgerBuildRunner, der Step-Outputs von Disk liest). Event-result bleibt sekundärer Fallback.
        var final = ReadFinalReport(run, scope);
        if (final is { } f)
            Console.WriteLine($"[checker-repair] decision={f.Decision} nach {f.Iterations} Iteration(en) (MC0-Fehler={f.Mc0Errors}, C7-Restverstöße={f.C7Residual}).");
        else if (result is not null)
            Console.WriteLine($"[checker-repair] decision={result.Decision} nach {result.Iterations} Iteration(en) (MC0-Fehler={result.Mc0Errors}, C7-Restverstöße={result.C7Residual}).");
        else
            Console.Error.WriteLine("[checker-repair] WARN: kein final-report.json und kein Output-Event — Ergebnis unklar.");
        Console.WriteLine($"[checker-repair] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");

        var decision = final?.Decision ?? result?.Decision.ToString();
        return string.Equals(decision, nameof(ContractDecision.Pass), StringComparison.OrdinalIgnoreCase) ? 0 : 1;
    }

    /// <summary>Liest das Abschluss-Zertifikat von Disk (Wahrheitsquelle, unabhängig davon ob MAF das YieldOutput
    /// als Event surfaced).</summary>
    private static (string Decision, int Iterations, int Mc0Errors, int C7Residual)? ReadFinalReport(RunContext run, string scope)
    {
        var path = Path.Combine(run.OutputDir(scope), "final-report.json");
        if (!File.Exists(path)) return null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            return (
                root.GetProperty("decision").GetString() ?? "UNKNOWN",
                root.GetProperty("iterations").GetInt32(),
                root.GetProperty("mc0").GetProperty("errors").GetInt32(),
                root.GetProperty("c7").GetProperty("violations").GetInt32());
        }
        catch (Exception) { return null; }
    }

    /// <summary>Zeichnet die Workflow-Events auf (wie LedgerBuildRunner) und gibt zurück, ob ein Executor-Fehler
    /// auftrat + das terminale <see cref="CheckerRepairResult"/>.</summary>
    private static (bool HasFailure, CheckerRepairResult? Result) RecordWorkflowEvents(
        RunContext run, Microsoft.Agents.AI.Workflows.Run workflowRun)
    {
        var hasFailure = false;
        CheckerRepairResult? result = null;
        foreach (var e in workflowRun.OutgoingEvents)
        {
            switch (e)
            {
                case ExecutorCompletedEvent completed:
                    run.AppendEvent(new { type = "EXECUTOR_FINISHED", runId = run.RunId, workflow = CheckerRepairWorkflow.WorkflowName, executorId = completed.ExecutorId, timestampUtc = DateTime.UtcNow });
                    break;
                case ExecutorFailedEvent failed:
                    hasFailure = true;
                    run.AppendEvent(new { type = "EXECUTOR_FAILED", runId = run.RunId, workflow = CheckerRepairWorkflow.WorkflowName, executorId = failed.ExecutorId, error = failed.Data?.Message, timestampUtc = DateTime.UtcNow });
                    break;
                case WorkflowErrorEvent error:
                    hasFailure = true;
                    run.AppendEvent(new { type = "WORKFLOW_ERROR", runId = run.RunId, workflow = CheckerRepairWorkflow.WorkflowName, error = error.Exception?.Message, timestampUtc = DateTime.UtcNow });
                    break;
                case WorkflowOutputEvent o:
                    if (o.Data is CheckerRepairResult r) result = r;
                    run.AppendEvent(new { type = "WORKFLOW_OUTPUT", runId = run.RunId, workflow = CheckerRepairWorkflow.WorkflowName, executorId = o.ExecutorId, decision = (o.Data as CheckerRepairResult)?.Decision.ToString(), timestampUtc = DateTime.UtcNow });
                    break;
            }
        }
        return (hasFailure, result);
    }

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
