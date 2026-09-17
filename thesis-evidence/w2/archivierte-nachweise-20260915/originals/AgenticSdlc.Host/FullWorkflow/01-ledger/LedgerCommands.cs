namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class LedgerCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Ledger L1: Transkript -> Candidate Ledger -> Canonical Ledger als ECHTER MAF-Workflow (runs/ledger/<runId>).
        // Optional mit Fixture: matcht den kanonischen Ledger gegen die Fixture = Baseline-Reproduktion (Exit-Kriterium L1).
        map["ledger-build"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerBuildRunner.RunAsync(args, settings, repoRoot);

        // Ledger High-Coverage-MVP: Transkript -> deterministische Atomic Units -> unit-aware Candidate Ledger
        // -> bestehende Canonical/Repair/Facet-Kette. Additiv; normaler ledger-build bleibt unveraendert.
        map["ledger-build-units"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerBuildUnitsRunner.RunAsync(args, settings, repoRoot);

        // Ledger-Referenz-Vorlage (deterministisch, KEIN LLM): Transkript + Kandidaten-Ledger -> segmentweise
        // Annotations-Vorlage zum Bau eines hand-vollständigen Referenz-Ledgers (Completeness/Recall-Messung).
        map["ledger-reference-template"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerReferenceTemplateRunner.RunAsync(args, settings, repoRoot);

        // Adjudikation Schritt 1 (deterministisch, kein LLM): review_required + Misses -> Adjudikations-Queue.
        map["ledger-adjudicate"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerAdjudicateRunner.RunPrepareAsync(args, repoRoot);

        // Adjudikation apply: ausgefüllte Queue -> adjudicated-ledger + consumable + AdjudicationCompletenessGate.
        map["ledger-adjudicate-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerAdjudicateRunner.RunApplyAsync(args, repoRoot);

        // Adjudikation Interactive/Re-Launch (§9): lokale Review-UI über der queue.json (Autosave), Finish -> apply.
        map["ledger-adjudicate-ui"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerAdjudicateUiRunner.RunAsync(args, repoRoot);

        // Adjudikation A10 (Refine): neu geminteten Claims (facetStatus=pending) volle Facetten zuweisen (deterministischer
        // Filter, LLM nur auf den pending-Claims) -> consumable-Claims auf Pipeline-Niveau.
        map["ledger-adjudicate-refine"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerAdjudicateRefineRunner.RunAsync(args, settings, repoRoot);

        // L3 realer Test: bestehenden Ledger mit dem FacetValidator prüfen (evidence- oder transcript-Kontext).
        map["ledger-validate"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerValidateRunner.RunAsync(args, settings, repoRoot);

        // L3-B: Selective-Metrics für den FacetValidator gegen die autor-bestätigte Fixture (correct vs perturbed).
        map["facet-validation-eval"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.FacetValidationEvalRunner.RunAsync(args, settings, repoRoot);

        // W2 L/LCR-Capture (DETERMINISTISCH, kein LLM, rein lesend): exportiert die Messpunkte L-machine
        // (Maker-Draft vor aller maschinellen QC) und LCR-machine (nach QC, vor Adjudikation) aus einem
        // bestehenden ledger-build-units-Lauf nach capture/ (Evaluationskonzept §11/§12.0-3).
        map["ledger-capture"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerCaptureRunner.RunAsync(args, repoRoot);

        // Ledger-Referenz-Recall (DETERMINISTISCH, kein LLM): Segment-Overlap-Screen, gratis. Misses verlässlich.
        map["ledger-reference-recall-fast"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerReferenceRecallFastRunner.RunAsync(args, settings, repoRoot);

        // Ledger-Referenz-Recall: misst, ob ein Auto-Ledger die Claims einer Referenz findet (Completeness/(B)-Frage).
        map["ledger-reference-recall"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Ledger.LedgerReferenceRecallRunner.RunAsync(args, settings, repoRoot);
    }
}
