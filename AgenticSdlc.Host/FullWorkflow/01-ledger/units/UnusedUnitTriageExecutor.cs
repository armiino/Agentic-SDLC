using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

[SendsMessage(typeof(UnusedUnitTriageResultMessage))]
internal sealed class UnusedUnitTriageExecutor : Executor<UnitCoverageReviewRequestMessage>
{
    public const string ExecutorName = "LedgerUnusedUnitTriage";

    private readonly UnusedUnitTriageReviewer _reviewer;
    private readonly RunContext _run;

    public UnusedUnitTriageExecutor(UnusedUnitTriageReviewer reviewer, RunContext run)
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
        var triage = await _reviewer.TriageAsync(unusedUnits, cancellationToken).ConfigureAwait(false);
        var reviewedIds = triage.Select(i => i.UnitId).ToHashSet(StringComparer.Ordinal);
        var missing = unusedUnits
            .Where(u => !reviewedIds.Contains(u.Id))
            .Select(u => new UnusedUnitTriageItem(
                UnitId: u.Id,
                Triage: "potentially_relevant",
                Reason: "The LLM triage did not return a classification for this unused unit.",
                Keywords: []))
            .ToList();
        var completed = triage.Concat(missing).ToList();

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01c-unused-unit-triage",
            output: new UnusedUnitTriageFixture(completed),
            metrics: new
            {
                stage = "unused-unit-triage",
                unusedUnits = unusedUnits.Count,
                reviewedByModel = triage.Count,
                autoCompletedPotentiallyRelevant = missing.Count,
                reviewed = completed.Count,
                potentiallyRelevant = completed.Count(i => i.Triage == "potentially_relevant"),
                lowSignal = completed.Count(i => i.Triage == "low_signal"),
                repetition = completed.Count(i => i.Triage == "repetition"),
                acknowledgement = completed.Count(i => i.Triage == "acknowledgement"),
                smalltalk = completed.Count(i => i.Triage == "smalltalk"),
                trash = completed.Count(i => i.Triage == "trash")
            });

        await context
            .SendMessageAsync(new UnusedUnitTriageResultMessage(message.Units, message.Entries, message.Coverage, completed))
            .ConfigureAwait(false);
    }
}
