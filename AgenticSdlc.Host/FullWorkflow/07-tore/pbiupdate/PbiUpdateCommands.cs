namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class PbiUpdateCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Inc 1c-3: incrementeller PBI-Update (affected-view/Delta -> nur betroffene PBIs). Maker / Review / Apply.
        map["pbi-update"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateRunner.RunAsync(args, settings, repoRoot);

        // S4 (Worklist 20.07): pbi-update als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
        // Additiv/parallel zum klassischen pbi-update / -review / -apply (die bleiben unveraendert).
        map["pbi-update-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateHitlRunner.RunAsync(args, settings, repoRoot);

        map["pbi-update-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.RunAsync(args, settings, repoRoot);

        map["pbi-update-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateApplyRunner.RunAsync(args, repoRoot);
    }
}
