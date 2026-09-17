using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using GateDecision = AgenticSdlc.Host.FullWorkflow.Core.GateDecision;

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
        var canonicalCheck = new CanonicalCheckExecutor(run);
        var coverageRepair = new CanonicalCoverageRepairExecutor(coverageRepairer, run);
        var facetValidation = new FacetValidationExecutor(facetValidator, transcript, run, withAdjudication);

        var builder = new WorkflowBuilder(extraction)
            .WithName("LedgerBuilder")
            .WithDescription("Transcript → Candidate → CanonicalDraft → [CanonicalCheck ⇄ CoverageRepair] → FacetValidation (evidence-first Ledger-Bau, L4)"
                + (withAdjudication ? " → HumanAdjudication." : "."));

        builder.AddEdge(extraction, canonicalization);
        // Schritt 5 ④: Maker-Checker-Repair-Loop (R-33-Form) — Verdict-typisierte Kanten-Prädikate.
        AddCanonicalLoop(builder, canonicalization, canonicalCheck, coverageRepair, facetValidation);

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
        var unusedCompareRepair = new UnusedCompareReferenceRepairExecutor(unusedUnitLedgerComparer, run);
        var canonicalization = new CanonicalizationExecutor(canonicalizer, run);
        var canonicalCheck = new CanonicalCheckExecutor(run);
        var coverageRepair = new CanonicalCoverageRepairExecutor(coverageRepairer, run);
        var facetValidation = new FacetValidationExecutor(facetValidator, transcript, run);

        var builder = new WorkflowBuilder(segmentation)
            .WithName("LedgerBuilderUnitCoverage")
            .WithDescription("Transcript -> AtomicUnits -> UnitAwareCandidate -> UnitCoverageGate -> UnusedUnitTriage -> "
                + "UnusedUnitLedgerCompare -> UnusedCompareReferenceRepair -> CanonicalDraft -> [CanonicalCheck ⇄ CoverageRepair] -> FacetValidation.");

        builder.AddEdge(segmentation, extraction);
        builder.AddEdge(extraction, unitCoverage);
        builder.AddEdge(unitCoverage, unusedTriage);
        builder.AddEdge(unusedTriage, unusedCompare);
        // Schritt 5 ④ / R-3: der Referenz-Repair ist ein SICHTBARER Knoten (vorher in der Comparer-Komponente versteckt).
        builder.AddEdge(unusedCompare, unusedCompareRepair);
        builder.AddEdge(unusedCompareRepair, canonicalization);
        // Schritt 5 ④: Maker-Checker-Repair-Loop (R-33-Form) — Verdict-typisierte Kanten-Prädikate.
        AddCanonicalLoop(builder, canonicalization, canonicalCheck, coverageRepair, facetValidation);

        // Schritt 5 ② (05.08.): der Summary-Yield der FacetValidation ist der deklarierte Workflow-Output —
        // als GEBUNDENE Kapsel im Ein-Graph wird genau dieser Yield zur Nachricht an den LedgerSummaryExecutor.
        // (Die CLI-Bahn liest weiterhin die Step-Outputs von Platte; der Output ist dort nur ein Event.)
        builder.WithOutputFrom(facetValidation);

        return builder.Build();
    }

    /// <summary>Schritt 5 ④ — die EINE Kanten-Quelle des Kanonisierungs-Loops (beide Builder-Varianten,
    /// R-33-Blaupause): Draft → Check · Repair-Verdict → Repair (Prädikat) · Repair → Check (Loop-back) ·
    /// Pass (CanonicalLedgerMessage) → FacetValidation · Terminal-Yield des Checkers als Workflow-Output.</summary>
    private static void AddCanonicalLoop(
        WorkflowBuilder builder,
        CanonicalizationExecutor canonicalization,
        CanonicalCheckExecutor canonicalCheck,
        CanonicalCoverageRepairExecutor coverageRepair,
        FacetValidationExecutor facetValidation)
    {
        builder.AddEdge(canonicalization, canonicalCheck);
        builder.AddEdge<CanonicalGateVerdict>(canonicalCheck, coverageRepair, m => m is not null && m.Decision == GateDecision.Repair);
        builder.AddEdge(coverageRepair, canonicalCheck);
        builder.AddEdge(canonicalCheck, facetValidation);
        builder.WithOutputFrom(canonicalCheck);
    }
}
