namespace AgenticSdlc.Host.FullWorkflow.Decision;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class DecisionCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Tor 2 / T2.1: Decision-Ingestion (deterministischer Kern). Stakeholder-Auflösung einer Open Decision -> Core
        // auflösen (DEC resolved, contradicts->contradicts_resolved) + betroffene PBIs entblocken. Maker/Review/Apply.
        map["decision-resolve"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionResolveRunner.RunAsync(args, repoRoot);

        // Tor 2 / T2.2: agentischer Resolver — freie Stakeholder-Antwort -> strukturierte Auflösung; danach dieselbe
        // deterministische T2.1-Kette (Derivation/Gate/Apply). Der 4. agentische Knoten.
        map["decision-resolve-agent"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionResolveAgentRunner.RunAsync(args, settings, repoRoot);

        // S4 (Worklist 20.07): decision (Tor 2) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
        // Additiv/parallel zum klassischen decision-resolve(-agent) / -review / -apply (die bleiben unveraendert).
        map["decision-resolve-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionHitlRunner.RunAsync(args, settings, repoRoot);

        map["decision-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionReviewRunner.RunAsync(args, settings, repoRoot);

        // R-14 G1-b (= E0.8): edit-faehige Review-UI des operativen decision-gate (pipeline-full/pipeline-hitl).
        map["decision-gate-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionGateReviewRunner.RunAsync(args, settings, repoRoot);

        map["decision-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionApplyRunner.RunAsync(args, repoRoot);

        // Tor 2 / T2.3: Kreis-Test (Tor 2 -> Tor 3). Blockiertes PBI -> decision-apply -> github-forward-Seed sieht
        // UPDATE/CREATE statt HOLD_BLOCKED. Deterministisch, kein LLM, kein GitHub, kein echter Core.
        map["decision-unblock-test"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Decision.DecisionUnblockTest.RunAsync(args, repoRoot);
    }
}
