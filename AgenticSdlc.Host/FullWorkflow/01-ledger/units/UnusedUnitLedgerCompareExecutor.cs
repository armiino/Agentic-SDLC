using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

[SendsMessage(typeof(CandidateLedgerMessage))]
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

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01d-unused-unit-ledger-compare",
            output: new UnusedUnitLedgerCompareFixture(completed),
            metrics: new
            {
                stage = "unused-unit-ledger-compare",
                potentiallyRelevant = relevantUnits.Count,
                comparedByModel = comparisons.Count,
                autoCompletedNeedsHuman = missing.Count,
                compared = completed.Count,
                unknownRelatedCandidateIds = unknownRelatedCandidateIds.Count,
                removedRelatedCandidateIds = unknownRelatedCandidateIds,
                alreadyCoveredIndirectly = completed.Count(i => i.Verdict == "already_covered_indirectly"),
                attachAsEvidence = completed.Count(i => i.Verdict == "attach_as_evidence"),
                missingClaim = completed.Count(i => i.Verdict == "missing_claim"),
                needsHuman = completed.Count(i => i.Verdict == "needs_human")
            });

        await context.SendMessageAsync(new CandidateLedgerMessage(message.Entries)).ConfigureAwait(false);
    }
}
