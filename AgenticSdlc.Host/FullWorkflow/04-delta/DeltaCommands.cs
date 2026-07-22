namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class DeltaCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Project State: JSON-first fachlicher Projektzustand aus L1/L2-Artefakten + akzeptierten L3-Promotions.
        map["project-state-build"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState.ProjectStateBuildRunner.RunAsync(args, repoRoot);
    }
}
