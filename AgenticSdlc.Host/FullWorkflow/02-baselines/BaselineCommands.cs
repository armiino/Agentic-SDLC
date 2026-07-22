namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class BaselineCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // MC0 (Maker-Checker): deterministischer Contract-Checker requirements.md + consumable.json -> contract-report.json.
        map["contract-check"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.MakerChecker.ContractCheckRunner.RunAsync(args, settings, repoRoot);

        // C7 (MC3): bounded Evidence-Support-Critic (LLM) — Detail-Deckung je Zeile gegen das zitierte Claim-Paket.
        map["contract-critic"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.MakerChecker.ContractCriticRunner.RunAsync(args, settings, repoRoot);

        // MC2: bounded Repair-Loop (k-Vote-Critic -> Repair -> re-check, max N) auf einem Artefakt.
        map["contract-repair"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.MakerChecker.ContractRepairRunner.RunAsync(args, settings, repoRoot);

        // Volle MAF-Komposition: Ledger -> [Fan-out] -> SelectBaseline -> [Derivation] (mehrstufig BindAsExecutor).
        map["evidence-chain"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Chain.EvidenceChainRunner.RunAsync(args, settings, repoRoot);

        // Rezept-Assembler (§10): deklaratives Rezept -> Graph zur Laufzeit (Baseline build|load -> 0..N Ableitungen).
        map["recipe"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Recipes.RecipeRunner.RunAsync(args, settings, repoRoot);

        // Derivation-Familie (verallgemeinert): config-gesteuerter Ableitungs-Workflow (Generate[Agent]->Anchor->Check).
        map["derive"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Derivation.DerivationRunner.RunAsync(args, settings, repoRoot);

        // B0: Ableitungsgüte-Aggregator über MEHRERE Läufe (Mittel + Spannweite je Modus; --judge = R3 Scope-Creep).
        map["derive-metrics"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Derivation.DerivationMetricsAggregator.RunAsync(args, settings, repoRoot);

        // A2 (Demonstration): Nicht-dekorativ-Beleg — supported-Rate der ledger-geerdeten requirements.md gegen den consumable.
        map["ledger-cite-fidelity"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Fidelity.LedgerCiteFidelityRunner.RunAsync(args, settings, repoRoot);

        // I-d: Human-Review der abgeleiteten Risiken (generisches HumanReview-UI) -> approved-derived-risks.json.
        map["derive-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Derivation.DerivedRisksReviewRunner.RunAsync(args, settings, repoRoot);

        // I-c: Inference-Checker — semantischer Relevanz-/Nicht-Widerspruchs-Check der abgeleiteten Risiken gegen ihre Anker.
        map["inference-check"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Derivation.InferenceCheckRunner.RunAsync(args, settings, repoRoot);

        // I-b: erster Derivation-Agent — leitet aus der geprüften Requirements-Baseline neue, verankerte Risiken ab.
        map["derive-risks"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Derivation.DerivedRisksRunner.RunAsync(args, settings, repoRoot);

        // E-d: Fan-out des Ledgers auf mehrere Artefakt-Zweige (parallel) -> Fan-in-Barrier -> Verified Baseline Set.
        map["baseline-fanout"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.FanOut.BaselineFanOutRunner.RunAsync(args, settings, repoRoot);

        // E-c: EIN komponierter Artefakt-Zweig (EvidenceBaselineAgent -> [CheckerRepair via BindAsExecutor] -> AssignIds).
        map["artifact-branch"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Branch.ArtifactBranchRunner.RunAsync(args, settings, repoRoot);

        // I-a: deterministisches ID-Gate — geprüftes Baseline-Artefakt -> ArtifactDocument mit stabilen Item-IDs (artifact.json).
        map["assign-artifact-ids"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Artifacts.ArtifactIdGateRunner.RunAsync(args, settings, repoRoot);

        // MC1/C3: eigenständiger Checker-Repair als echter MAF-Workflow (Checker -> [Repair-Loop] -> Finalize).
        // Quell-generisch, per BindAsExecutor als Knoten hinter jeden Generator-Agenten einhängbar (Kapitel C).
        map["checker-repair-workflow"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow.CheckerRepairRunner.RunAsync(args, settings, repoRoot);
    }
}
