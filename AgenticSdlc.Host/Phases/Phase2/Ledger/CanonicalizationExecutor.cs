using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Stufe 2 des Ledger-Workflows: Candidate Ledger → kanonischer (gemergeter, facetten-reparierter) Ledger.
/// </summary>
/// <remarks>
/// Wie Stufe 1 ein <see cref="Executor{TInput}"/> mit EINEM schema-gebundenen LLM-Call; Logik aus
/// <see cref="SemanticLedgerCanonicalizer"/> wiederverwendet. Reicht den kanonischen Ledger als
/// <see cref="CanonicalLedgerMessage"/> an Stufe 3 (FacetValidation) weiter. Cluster-Trace
/// (candidateIds[]/assumedRelation) + ResponseFormat kamen mit L2.
/// </remarks>
[SendsMessage(typeof(CanonicalLedgerMessage))]
internal sealed class CanonicalizationExecutor : Executor<CandidateLedgerMessage>
{
    public const string ExecutorName = "LedgerCanonicalization";

    private readonly SemanticLedgerCanonicalizer _canonicalizer;
    private readonly RunContext _run;

    public CanonicalizationExecutor(SemanticLedgerCanonicalizer canonicalizer, RunContext run)
        : base(ExecutorName)
    {
        _canonicalizer = canonicalizer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        CandidateLedgerMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var canonical = await _canonicalizer
            .CanonicalizeAsync(message.Entries, cancellationToken)
            .ConfigureAwait(false);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-02-canonical",
            output: new SemanticLedgerFixture(canonical),
            metrics: new
            {
                stage = "canonicalization",
                candidate = message.Entries.Count,
                canonical = canonical.Count,
                merged = message.Entries.Count - canonical.Count
            });

        await context
            .SendMessageAsync(new CanonicalLedgerMessage(canonical))
            .ConfigureAwait(false);
    }
}
