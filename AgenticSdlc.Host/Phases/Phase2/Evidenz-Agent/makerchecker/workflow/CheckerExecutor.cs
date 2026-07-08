using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;

/// <summary>
/// Stufe „Checker" des MAF-Workflows: prüft das eingehende Artefakt gegen den Ledger und leitet die Weiter-
/// Entscheidung DETERMINISTISCH ab. Kapselt die beiden bestehenden Prüfer — das deterministische
/// <see cref="ContractChecker"/> (MC0: Citation-/Dispositions-Integrität) und den bounded k-Vote-
/// <see cref="ContractCritic"/> (C7: Evidence-Support je Zeile). Sendet ein <see cref="CheckVerdictMessage"/>,
/// auf das die zwei konditionalen Ausgangskanten (Repair vs. Finalize) filtern.
/// </summary>
/// <remarks>
/// Der Executor prüft, entscheidet und protokolliert — er repariert NICHT (Trennung Checker/Repair). Je
/// Iteration wird <c>step-check-&lt;iter&gt;/report.json</c> geschrieben (MC0 + Vote + Entscheidung) — die
/// Audit-Spur der Schleife. Der C7-Critic ruft das LLM; sein Client ist im Runner mit der Observability-
/// Pipeline (Executor-Name <see cref="ExecutorName"/>) umhüllt, daher landen seine Calls unter
/// <c>logs/agents/MakerCheckerChecker/</c> — wie jede andere MAF-Stufe.
/// </remarks>
[SendsMessage(typeof(CheckVerdictMessage))]
internal sealed class CheckerExecutor : Executor<CheckArtifactMessage>
{
    public const string ExecutorName = "MakerCheckerChecker";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly ContractCritic _critic;
    private readonly ConsumableLedger _ledger;
    private readonly string _artifactDisposition;
    private readonly int _k;
    private readonly int _minVotes;
    private readonly int _maxIterations;
    private readonly RunContext _run;

    public CheckerExecutor(
        ContractCritic critic, ConsumableLedger ledger, string artifactDisposition,
        int k, int minVotes, int maxIterations, RunContext run)
        : base(ExecutorName)
    {
        _critic = critic;
        _ledger = ledger;
        _artifactDisposition = artifactDisposition;
        _k = k;
        _minVotes = minVotes;
        _maxIterations = maxIterations;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        CheckArtifactMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // MC0: deterministische Struktur-Prüfung (Citation/Disposition) — kein LLM.
        var structural = ContractChecker.Check(
            message.Markdown, _ledger, message.Iteration, _maxIterations, _artifactDisposition);

        // C7: bounded k-Vote-Evidence-Support (LLM). Nur mehrheitlich bestätigte Zeilen zählen.
        var evidence = await _critic
            .CheckWithVoteAsync(message.Markdown, _ledger, _k, _minVotes, cancellationToken)
            .ConfigureAwait(false);

        var decision = Decide(structural, evidence, message.Iteration, _maxIterations);

        WriteStepReport(message.Iteration, structural, evidence, decision);

        _run.AppendEvent(new
        {
            type = "CHECK_COMPLETED",
            runId = _run.RunId,
            workflow = CheckerRepairWorkflow.WorkflowName,
            iteration = message.Iteration,
            mc0Pass = structural.Pass,
            mc0Errors = structural.Violations.Count(v => v.Severity == ContractSeverity.Error),
            c7Violations = evidence.Violations.Count,
            decision = decision.ToString(),
            timestampUtc = DateTime.UtcNow
        });

        await context
            .SendMessageAsync(new CheckVerdictMessage(message.Markdown, message.Iteration, decision, structural, evidence))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Kombinierte, deterministische Weiter-Entscheidung (Contract.md §3, hier über beide Prüfer). Reihenfolge:
    /// <list type="number">
    ///   <item><description><b>Pass</b> — keine blockierenden MC0-Fehler UND C7-Vote sauber.</description></item>
    ///   <item><description><b>MaxIterationsReached</b> — Schranke erreicht: kein weiterer Repair, Rest → Mensch.</description></item>
    ///   <item><description><b>Repair</b> — es gibt reparierbare C7-Verstöße (EUD/FO) und noch Iterationen übrig.</description></item>
    ///   <item><description><b>HumanReview</b> — nur nicht-reparierbare Struktur-Fehler bleiben (MC0), die der
    ///     zeilenweise Repair nicht beheben kann → Mensch.</description></item>
    /// </list>
    /// </summary>
    private static ContractDecision Decide(
        ContractCheckReport structural, CriticVoteReport evidence, int iteration, int maxIterations)
    {
        var mc0Errors = structural.Violations.Count(v => v.Severity == ContractSeverity.Error);
        var repairableC7 = evidence.Violations.Count; // k-Vote liefert ausschließlich reparierbare EUD/FO

        if (mc0Errors == 0 && evidence.Pass) return ContractDecision.Pass;
        if (iteration >= maxIterations) return ContractDecision.MaxIterationsReached;
        if (repairableC7 > 0) return ContractDecision.Repair;
        return ContractDecision.HumanReview;
    }

    private void WriteStepReport(
        int iteration, ContractCheckReport structural, CriticVoteReport evidence, ContractDecision decision)
    {
        var dir = Path.Combine(_run.RunDir, $"step-check-{iteration:D2}");
        Directory.CreateDirectory(dir);
        var payload = new
        {
            iteration,
            decision = decision.ToString(),
            mc0 = structural,
            c7 = evidence
        };
        File.WriteAllText(Path.Combine(dir, "report.json"), JsonSerializer.Serialize(payload, Json));
    }
}
