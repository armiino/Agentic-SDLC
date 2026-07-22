namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class CoreCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Core (die lebende Projektwahrheit): einmaliger Seed aus einem ProjectState-Rebuild.
        // Bootstrap / Start bei null (B1): erstes Transkript, noch kein Core. Modus-Erkennung + deterministische
        // Orchestrierung (project-state-build -> core-seed -> core-baseline). Danach agentische Backlog-Stufe (mit Gates).
        map["core-bootstrap-first-transcript"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreBootstrapRunner.RunAsync(args, repoRoot);

        map["core-seed"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreSeedRunner.RunAsync(args, repoRoot);

        // Inc 1b: Core -> L4-Applied-Triplet, damit re-clarify (cluster->PBIs->issues) den lebenden Core konsumiert.
        map["core-baseline"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreBaselineRunner.RunAsync(args, repoRoot);

        // Inc 1c-1: Cluster (Features) + PBIs eines re-clarify-Laufs als persistente Core-Entitaeten in den Core heben.
        map["core-seed-backlog"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreSeedBacklogRunner.RunAsync(args, repoRoot);

        // Inc 1c-2: arbeitsfaehige Views auf den Core (active-backlog | archive | github-sync).
        map["core-view"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreViewRunner.RunAsync(args, repoRoot);

        // Tor 3 / T3.1: PBI<->Issue-Mapping als Core-Relation (implemented_by_issue) pflegen = Dedup-Basis fuer den
        // naechsten GitHub-Delta-Lauf (nicht mehr nur Run-Artefakt). Deterministisch, kein LLM, kein GitHub-Call.
        map["github-map"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreGithubMapRunner.RunAsync(args, repoRoot);
    }
}
