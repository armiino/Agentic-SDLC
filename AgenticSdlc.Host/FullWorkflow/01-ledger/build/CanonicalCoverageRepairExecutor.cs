using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Stufe 2b — seit Schritt 5 ④ der REPAIR-Knoten des Kanonisierungs-Loops (R-33-Form): bekommt das
/// typisierte <see cref="CanonicalGateVerdict"/> (Kanten-Prädikat Decision==Repair), liest die
/// Verstoß-IDs WÖRTLICH aus dem Gate-Report (CANDIDATE_SILENTLY_DROPPED) und ruft den existierenden
/// gezielten LLM-Repair. Ergebnis geht als Attempt+1 ZURÜCK zum Checker (Loop-back) — der re-prüft;
/// Schreib-Ort von step-02 ist der Checker (Terminal), nicht mehr dieser Knoten.
/// </summary>
[SendsMessage(typeof(CandidateAndCanonicalLedgerMessage))]
internal sealed class CanonicalCoverageRepairExecutor : Executor<CanonicalGateVerdict>
{
    public const string ExecutorName = "LedgerCanonicalCoverageRepair";

    private readonly ICanonicalCoverageRepairer _repairer;
    private readonly RunContext _run;

    public CanonicalCoverageRepairExecutor(ICanonicalCoverageRepairer repairer, RunContext run)
        : base(ExecutorName)
    {
        _repairer = repairer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        CanonicalGateVerdict verdict,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = verdict.Report.Violations
            .Where(x => x.Code == CanonicalCheckExecutor.RepairableCode)
            .SelectMany(x => x.Ids)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var repaired = missing.Count == 0
            ? verdict.CanonicalDraft
            : await _repairer
                .RepairAsync(verdict.Candidates, verdict.CanonicalDraft, missing, cancellationToken)
                .ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "LEDGER_CANONICAL_REPAIR",
            runId = _run.RunId,
            fromAttempt = verdict.Attempt,
            missingCandidateIds = missing.Count,
            canonicalBefore = verdict.CanonicalDraft.Count,
            canonicalAfter = repaired.Count,
            timestampUtc = DateTime.UtcNow
        });

        await context
            .SendMessageAsync(new CandidateAndCanonicalLedgerMessage(
                verdict.Candidates, repaired, verdict.Attempt + 1, "repair", verdict.History))
            .ConfigureAwait(false);
    }
}
