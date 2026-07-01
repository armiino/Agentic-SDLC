using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Baut den Ledger-Builder-Workflow als MAF-<see cref="Workflow"/> aus typisierten Custom-Executoren
/// (Muster wie Phase2B, aber Transport = Message-Passing statt Shared State).
/// </summary>
/// <remarks>
/// L3-Stand = lineare 3-Stufen-Kette (graph-fähig, aktuell linear; später für den Gap-Audit verzweigbar):
/// <code>Transcript → CandidateExtraction → Canonicalization → FacetValidation</code>
/// L4 ergänzt das LedgerQualityGate (siehe smallVersion.md §verfeinerte Bau-Reihenfolge).
/// Logging pro Executor liefert die gemeinsame Observability-Pipeline (AgentChatPipelineBuilder), die der
/// Runner um die LLM-Clients der Executoren legt.
/// </remarks>
public static class LedgerBuilderWorkflow
{
    public static Workflow Build(
        SemanticLedgerExtractor extractor,
        SemanticLedgerCanonicalizer canonicalizer,
        FacetValidator facetValidator,
        string transcript,
        RunContext run)
    {
        var extraction = new CandidateExtractionExecutor(extractor, run);
        var canonicalization = new CanonicalizationExecutor(canonicalizer, run);
        var facetValidation = new FacetValidationExecutor(facetValidator, transcript, run);

        var builder = new WorkflowBuilder(extraction)
            .WithName("LedgerBuilder")
            .WithDescription("Transcript → Candidate → Canonical → FacetValidation (evidence-first Ledger-Bau, L3).");

        builder.AddEdge(extraction, canonicalization);
        builder.AddEdge(canonicalization, facetValidation);

        return builder.Build();
    }
}
