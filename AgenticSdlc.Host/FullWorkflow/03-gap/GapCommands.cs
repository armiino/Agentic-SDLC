namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class GapCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // L3 Open-World-Ableitung: Kandidaten generieren → verankern → in 4 Klassen routen → Human-Review-Paket (Prepare-Phase).
        map["l3"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Gap.L3Runner.RunAsync(args, settings, repoRoot);

        // L3 Apply-Phase (Workflow 2): menschliche Entscheidungen (accept/edit/reject) deterministisch anwenden → Promotion + Provenienz.
        map["l3-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Gap.L3ApplyRunner.RunAsync(args, settings, repoRoot);

        // L3 Reflect-Sub-Workflow (NEEDS_REVISION): Kandidaten per Feedback überarbeiten (Self-Refine) → neu klassifizieren, bounded.
        map["l3-revise"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Gap.L3ReviseRunner.RunAsync(args, settings, repoRoot);

        // L3 Human-Review (config l3.reviewMode: file|interactive): Review-UI → human-decisions.json (von apply/revise konsumiert).
        map["l3-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Gap.L3ReviewRunner.RunAsync(args, settings, repoRoot);
    }
}
