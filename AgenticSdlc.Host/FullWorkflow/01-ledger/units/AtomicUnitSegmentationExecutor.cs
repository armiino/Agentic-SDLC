using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

[SendsMessage(typeof(AtomicUnitsMessage))]
internal sealed class AtomicUnitSegmentationExecutor : Executor<string>
{
    public const string ExecutorName = "LedgerAtomicUnitSegmentation";

    private readonly string _sourceName;
    private readonly RunContext _run;

    public AtomicUnitSegmentationExecutor(string sourceName, RunContext run)
        : base(ExecutorName)
    {
        _sourceName = sourceName;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        string transcript,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var units = AtomicUnitSegmenter.Segment(transcript, _sourceName);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-00-atomic-units",
            output: new AtomicUnitFixture(units),
            metrics: new { stage = "atomic-unit-segmentation", units = units.Count, mode = "deterministic-turns" });

        await context.SendMessageAsync(new AtomicUnitsMessage(units)).ConfigureAwait(false);
    }
}
