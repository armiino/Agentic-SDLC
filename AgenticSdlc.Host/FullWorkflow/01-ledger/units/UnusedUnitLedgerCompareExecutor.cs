using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>Step 01d (Compare-Teil): vergleicht potenziell relevante unused Units gegen den Candidate-Ledger,
/// sanitisiert unbekannte Referenzen und vervollständigt fehlende Urteile deterministisch zu needs_human.
/// Seit Schritt 5 ④ / R-3: der Referenz-Repair-Pass ist ein EIGENER sichtbarer Folge-Knoten
/// (<see cref="UnusedCompareReferenceRepairExecutor"/>) — DER schreibt step-01d; dieser Knoten sendet den Draft.</summary>
[SendsMessage(typeof(UnusedCompareDraftMessage))]
internal sealed class UnusedUnitLedgerCompareExecutor : Executor<UnusedUnitTriageResultMessage>
{
    public const string ExecutorName = "LedgerUnusedUnitLedgerCompare";

    private readonly UnusedUnitLedgerComparer _comparer;
    private readonly RunContext _run;

    public UnusedUnitLedgerCompareExecutor(UnusedUnitLedgerComparer comparer, RunContext run)
        : base(ExecutorName)
    {
        _comparer = comparer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        UnusedUnitTriageResultMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var relevantIds = message.Triage
            .Where(i => i.Triage == "potentially_relevant")
            .Select(i => i.UnitId)
            .ToHashSet(StringComparer.Ordinal);
        var relevantUnits = message.Units.Where(u => relevantIds.Contains(u.Id)).ToList();
        var comparisons = await _comparer.CompareAsync(relevantUnits, message.Entries, cancellationToken).ConfigureAwait(false);
        var comparedIds = comparisons.Select(i => i.UnitId).ToHashSet(StringComparer.Ordinal);
        var candidateIds = message.Entries.Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        var unknownRelatedCandidateIds = comparisons
            .SelectMany(i => i.RelatedCandidateIds)
            .Where(id => !candidateIds.Contains(id))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var sanitizedComparisons = comparisons
            .Select(i => i with
            {
                RelatedCandidateIds = i.RelatedCandidateIds
                    .Where(candidateIds.Contains)
                    .Distinct(StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();
        var missing = relevantUnits
            .Where(u => !comparedIds.Contains(u.Id))
            .Select(u => new UnusedUnitLedgerCompareItem(
                UnitId: u.Id,
                Verdict: "needs_human",
                SuggestedAction: "human_review",
                Reason: "The LLM ledger-compare did not return a classification for this potentially relevant unused unit.",
                SuggestedProposition: u.Text,
                RelatedCandidateIds: []))
            .ToList();
        var completed = sanitizedComparisons.Concat(missing).ToList();

        await context.SendMessageAsync(new UnusedCompareDraftMessage(
            RelevantUnits: relevantUnits,
            Candidates: message.Entries,
            Items: completed,
            ComparedByModel: comparisons.Count,
            AutoCompletedNeedsHuman: missing.Count,
            RemovedRelatedCandidateIds: unknownRelatedCandidateIds)).ConfigureAwait(false);
    }
}

/// <summary>
/// Schritt 5 ④ / R-3 (05.08.) — der Referenz-Repair-Pass als EIGENER, sichtbarer Graph-Knoten
/// (vorher: verstecktes Anhängsel in <c>CompareAsync</c>, im Event-Stream unsichtbar — der R-3-Fund).
/// Fachlogik unverändert (gezielter LLM-Nachfrage-Pass, danach deterministisches Downgrade zu needs_human);
/// schreibt step-01d exakt wie zuvor und reicht den Candidate-Ledger an die Kanonisierung.
/// </summary>
[SendsMessage(typeof(CandidateLedgerMessage))]
internal sealed class UnusedCompareReferenceRepairExecutor : Executor<UnusedCompareDraftMessage>
{
    public const string ExecutorName = "LedgerUnusedCompareReferenceRepair";

    private readonly UnusedUnitLedgerComparer _comparer;
    private readonly RunContext _run;

    public UnusedCompareReferenceRepairExecutor(UnusedUnitLedgerComparer comparer, RunContext run)
        : base(ExecutorName)
    {
        _comparer = comparer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        UnusedCompareDraftMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var completed = await _comparer
            .RepairCoverageReferencesAsync(message.Items, message.RelevantUnits, message.Candidates, cancellationToken)
            .ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "LEDGER_UNUSED_REFERENCE_REPAIR",
            runId = _run.RunId,
            items = completed.Count,
            downgraded = completed.Count(r => r.Reason.StartsWith(UnusedUnitCompareRepair.DowngradePrefix, StringComparison.Ordinal)),
            timestampUtc = DateTime.UtcNow
        });

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01d-unused-unit-ledger-compare",
            output: new UnusedUnitLedgerCompareFixture(completed.ToList()),
            metrics: new
            {
                stage = "unused-unit-ledger-compare",
                potentiallyRelevant = message.RelevantUnits.Count,
                comparedByModel = message.ComparedByModel,
                autoCompletedNeedsHuman = message.AutoCompletedNeedsHuman,
                compared = completed.Count,
                unknownRelatedCandidateIds = message.RemovedRelatedCandidateIds.Count,
                removedRelatedCandidateIds = message.RemovedRelatedCandidateIds,
                alreadyCoveredIndirectly = completed.Count(i => i.Verdict == "already_covered_indirectly"),
                attachAsEvidence = completed.Count(i => i.Verdict == "attach_as_evidence"),
                missingClaim = completed.Count(i => i.Verdict == "missing_claim"),
                needsHuman = completed.Count(i => i.Verdict == "needs_human")
            });

        await context.SendMessageAsync(new CandidateLedgerMessage(message.Candidates)).ConfigureAwait(false);
    }
}
