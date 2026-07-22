using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

[SendsMessage(typeof(CandidateLedgerMessage))]
internal sealed class UnusedUnitReviewExecutor : Executor<UnitCoverageReviewRequestMessage>
{
    public const string ExecutorName = "LedgerUnusedUnitReview";

    private readonly UnusedUnitReviewer _reviewer;
    private readonly RunContext _run;

    public UnusedUnitReviewExecutor(UnusedUnitReviewer reviewer, RunContext run)
        : base(ExecutorName)
    {
        _reviewer = reviewer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        UnitCoverageReviewRequestMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var unusedIds = message.Coverage.UnusedUnits.ToHashSet(StringComparer.Ordinal);
        var unusedUnits = message.Units.Where(u => unusedIds.Contains(u.Id)).ToList();
        var review = await _reviewer.ReviewAsync(unusedUnits, message.Entries, cancellationToken).ConfigureAwait(false);
        var reviewedIds = review.Select(i => i.UnitId).ToHashSet(StringComparer.Ordinal);
        var missing = unusedUnits
            .Where(u => !reviewedIds.Contains(u.Id))
            .Select(u => new UnusedUnitReviewItem(
                UnitId: u.Id,
                Verdict: "needs_human",
                SuggestedAction: "human_review",
                Reason: "The LLM reviewer did not return a classification for this unused unit.",
                SuggestedProposition: u.Text,
                RelatedCandidateIds: []))
            .ToList();
        var completedReview = review.Concat(missing).ToList();

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01c-unused-unit-review",
            output: new UnusedUnitReviewFixture(completedReview),
            metrics: new
            {
                stage = "unused-unit-review",
                unusedUnits = unusedUnits.Count,
                reviewedByModel = review.Count,
                autoCompletedNeedsHuman = missing.Count,
                reviewed = completedReview.Count,
                missingReviewItems = 0,
                missingClaim = completedReview.Count(i => i.Verdict == "missing_claim"),
                needsHuman = completedReview.Count(i => i.Verdict == "needs_human"),
                alreadyCoveredIndirectly = completedReview.Count(i => i.Verdict == "already_covered_indirectly"),
                lowSignal = completedReview.Count(i => i.Verdict == "low_signal"),
                irrelevant = completedReview.Count(i => i.Verdict == "irrelevant")
            });

        await context.SendMessageAsync(new CandidateLedgerMessage(message.Entries)).ConfigureAwait(false);
    }
}
