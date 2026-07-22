using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Stufe 2 des Ledger-Workflows: Candidate Ledger → kanonischer (gemergeter, facetten-reparierter) Ledger.
/// </summary>
/// <remarks>
/// Wie Stufe 1 ein <see cref="Executor{TInput}"/> mit EINEM schema-gebundenen LLM-Call; Logik aus
/// <see cref="SemanticLedgerCanonicalizer"/> wiederverwendet. Reicht den Draft zusammen mit den Candidates als
/// <see cref="CandidateAndCanonicalLedgerMessage"/> an Stufe 2b (CoverageRepair) weiter. Cluster-Trace
/// (candidateIds[]/assumedRelation) + ResponseFormat kamen mit L2.
/// </remarks>
[SendsMessage(typeof(CandidateAndCanonicalLedgerMessage))]
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
        canonical = LedgerSourceUnitTrace.ApplyFromCandidates(message.Entries, canonical);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-02-canonical-draft",
            output: new SemanticLedgerFixture(canonical),
            metrics: new
            {
                stage = "canonicalization",
                candidate = message.Entries.Count,
                canonical = canonical.Count,
                merged = message.Entries.Count - canonical.Count
            });

        await context
            .SendMessageAsync(new CandidateAndCanonicalLedgerMessage(message.Entries, canonical))
            .ConfigureAwait(false);
    }
}
