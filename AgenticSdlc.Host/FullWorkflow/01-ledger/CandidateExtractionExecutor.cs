using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Stufe 1 des Ledger-Workflows: Transkript → recall-first Candidate Semantic Ledger.
/// </summary>
/// <remarks>
/// Bewusst ein <see cref="Executor{TInput}"/> mit EINEM schema-gebundenen LLM-Call — KEIN freier
/// <c>AIAgent</c> mit Tool-Loop (Abgrenzung zu Phase2B, wo der Executor einen Agenten umhüllt). Die
/// LLM-Logik (Prompt + JSON-Schema) wird aus <see cref="SemanticLedgerExtractor"/> wiederverwendet, damit
/// der bestehende Spike-Runner als gemessene Baseline intakt bleibt (smallVersion.md §Schutzregel).
/// Eingabe = Transkripttext (Workflow-Input-String). Ausgabe = <see cref="CandidateLedgerMessage"/>.
/// </remarks>
[SendsMessage(typeof(CandidateLedgerMessage))]
internal sealed class CandidateExtractionExecutor : Executor<string>
{
    public const string ExecutorName = "LedgerCandidateExtraction";

    private readonly SemanticLedgerExtractor _extractor;
    private readonly RunContext _run;

    public CandidateExtractionExecutor(SemanticLedgerExtractor extractor, RunContext run)
        : base(ExecutorName)
    {
        _extractor = extractor;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        string transcript,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var entries = await _extractor.ExtractAsync(transcript, cancellationToken).ConfigureAwait(false);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-01-candidate",
            output: new SemanticLedgerFixture(entries),
            metrics: new { stage = "candidate-extraction", extracted = entries.Count });

        await context.SendMessageAsync(new CandidateLedgerMessage(entries)).ConfigureAwait(false);
    }
}
