using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Baut den Ledger-Builder-Workflow als MAF-<see cref="Workflow"/> aus typisierten Custom-Executoren
/// (Muster wie Phase2B, aber Transport = Message-Passing statt Shared State).
/// </summary>
/// <remarks>
/// L4-Stand = lineare Kette mit gezielter Coverage-Reparatur:
/// <code>Transcript → CandidateExtraction → Canonicalization → CoverageRepair → FacetValidation</code>
/// Das deterministische LedgerQualityGate (L4) läuft NACH dem Workflow im Runner über die Step-Outputs
/// (kein LLM → gehört nicht in die Chat-Executor-Kette); schreibt gate/ledger-quality.json.
/// Logging pro Executor liefert die gemeinsame Observability-Pipeline (AgentChatPipelineBuilder), die der
/// Runner um die LLM-Clients der Executoren legt.
/// </remarks>
public static class LedgerBuilderWorkflow
{
    public static Workflow Build(
        SemanticLedgerExtractor extractor,
        SemanticLedgerCanonicalizer canonicalizer,
        CanonicalCoverageRepairer coverageRepairer,
        FacetValidator facetValidator,
        string transcript,
        RunContext run,
        AdjudicationMode adjudicationMode = AdjudicationMode.Skip,
        string repoRoot = "",
        bool adjudicationOpenBrowser = true)
    {
        // Skip = Baseline-neutral: FacetValidation terminiert wie bisher. Sonst hängt der
        // HumanAdjudicationExecutor als terminale Stufe an und FacetValidation forwardet den Ledger.
        var withAdjudication = adjudicationMode != AdjudicationMode.Skip;

        var extraction = new CandidateExtractionExecutor(extractor, run);
        var canonicalization = new CanonicalizationExecutor(canonicalizer, run);
        var coverageRepair = new CanonicalCoverageRepairExecutor(coverageRepairer, run);
        var facetValidation = new FacetValidationExecutor(facetValidator, transcript, run, withAdjudication);

        var builder = new WorkflowBuilder(extraction)
            .WithName("LedgerBuilder")
            .WithDescription("Transcript → Candidate → CanonicalDraft → CoverageRepair → FacetValidation (evidence-first Ledger-Bau, L4)"
                + (withAdjudication ? " → HumanAdjudication." : "."));

        builder.AddEdge(extraction, canonicalization);
        builder.AddEdge(canonicalization, coverageRepair);
        builder.AddEdge(coverageRepair, facetValidation);

        if (withAdjudication)
        {
            // Normal-Build: keine Miss-Signale (nur review_required in der Queue) -> missSignalPath=null.
            var adjudication = new HumanAdjudicationExecutor(adjudicationMode, run, repoRoot, adjudicationOpenBrowser, missSignalPath: null);
            builder.AddEdge(facetValidation, adjudication);
        }

        return builder.Build();
    }

    internal static Workflow BuildUnitCoverage(
        UnitAwareSemanticLedgerExtractor extractor,
        UnusedUnitTriageReviewer unusedUnitTriageReviewer,
        UnusedUnitLedgerComparer unusedUnitLedgerComparer,
        SemanticLedgerCanonicalizer canonicalizer,
        CanonicalCoverageRepairer coverageRepairer,
        FacetValidator facetValidator,
        string transcript,
        string sourceName,
        RunContext run)
    {
        var segmentation = new AtomicUnitSegmentationExecutor(sourceName, run);
        var extraction = new UnitAwareCandidateExtractionExecutor(extractor, run);
        var unitCoverage = new UnitCoverageGateExecutor(run);
        var unusedTriage = new UnusedUnitTriageExecutor(unusedUnitTriageReviewer, run);
        var unusedCompare = new UnusedUnitLedgerCompareExecutor(unusedUnitLedgerComparer, run);
        var canonicalization = new CanonicalizationExecutor(canonicalizer, run);
        var coverageRepair = new CanonicalCoverageRepairExecutor(coverageRepairer, run);
        var facetValidation = new FacetValidationExecutor(facetValidator, transcript, run);

        var builder = new WorkflowBuilder(segmentation)
            .WithName("LedgerBuilderUnitCoverage")
            .WithDescription("Transcript -> AtomicUnits -> UnitAwareCandidate -> UnitCoverageGate -> UnusedUnitTriage -> UnusedUnitLedgerCompare -> CanonicalDraft -> CoverageRepair -> FacetValidation.");

        builder.AddEdge(segmentation, extraction);
        builder.AddEdge(extraction, unitCoverage);
        builder.AddEdge(unitCoverage, unusedTriage);
        builder.AddEdge(unusedTriage, unusedCompare);
        builder.AddEdge(unusedCompare, canonicalization);
        builder.AddEdge(canonicalization, coverageRepair);
        builder.AddEdge(coverageRepair, facetValidation);

        return builder.Build();
    }
}
