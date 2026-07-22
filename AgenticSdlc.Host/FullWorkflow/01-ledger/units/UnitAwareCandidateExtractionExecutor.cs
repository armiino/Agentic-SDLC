using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

[SendsMessage(typeof(UnitAwareCandidateLedgerMessage))]
internal sealed class UnitAwareCandidateExtractionExecutor : Executor<AtomicUnitsMessage>
{
    public const string ExecutorName = "LedgerUnitAwareCandidateExtraction";

    private readonly UnitAwareSemanticLedgerExtractor _extractor;
    private readonly RunContext _run;

    public UnitAwareCandidateExtractionExecutor(UnitAwareSemanticLedgerExtractor extractor, RunContext run)
        : base(ExecutorName)
    {
        _extractor = extractor;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        AtomicUnitsMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var entries = await _extractor.ExtractAsync(message.Units, cancellationToken).ConfigureAwait(false);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01-candidate",
            output: new SemanticLedgerFixture(entries),
            metrics: new
            {
                stage = "unit-aware-candidate-extraction",
                units = message.Units.Count,
                extracted = entries.Count,
                candidatesWithSourceUnitIds = entries.Count(e => e.SourceUnitIds is { Count: > 0 })
            });

        await context.SendMessageAsync(new UnitAwareCandidateLedgerMessage(message.Units, entries)).ConfigureAwait(false);
    }
}
