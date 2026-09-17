using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

[SendsMessage(typeof(UnitCoverageReviewRequestMessage))]
internal sealed class UnitCoverageGateExecutor : Executor<UnitAwareCandidateLedgerMessage>
{
    public const string ExecutorName = "LedgerUnitCoverageGate";

    private readonly RunContext _run;

    public UnitCoverageGateExecutor(RunContext run)
        : base(ExecutorName)
    {
        _run = run;
    }

    public override async ValueTask HandleAsync(
        UnitAwareCandidateLedgerMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var gate = UnitCoverageGate.Evaluate(message.Units, message.Entries);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01b-unit-coverage",
            output: gate,
            metrics: new
            {
                stage = "unit-coverage-gate",
                gate.Pass,
                gate.TotalUnits,
                gate.UsedUnits,
                unusedUnits = gate.UnusedUnits.Count,
                missingTrace = gate.CandidateWithoutSourceUnitIds.Count,
                unknownRefs = gate.UnknownSourceUnitIds.Count
            });

        await context
            .SendMessageAsync(new UnitCoverageReviewRequestMessage(message.Units, message.Entries, gate))
            .ConfigureAwait(false);
    }
}
