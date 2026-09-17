using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

/// <summary>
/// C5a (09.08.2026, K13-Hebung der --pending-Materialisierung): EINE Naht, die eine Registry-Nutzlast in
/// einen frischen Review-Run materialisiert (Episode/Beleg-Kopie + pending-ref.json fürs Apply-Schließen).
/// Konsumenten: pbi-update-review --pending (UI-Kanal) UND StewardGateTools (Chat-Kanal) — gleiche Governance.
/// </summary>
public static class PendingReviewMaterializer
{
    public static async Task<string> MaterializeAsync(ProjectStateProposal proposal)
    {
        var run = new RunContext(RunId.New(), "pbi-update");
        run.EnsureFolders();
        var dir = run.OutputDir("plan");
        await File.WriteAllTextAsync(Path.Combine(dir, "pbi-change-plan.json"), proposal.Payload!.Value.GetRawText()).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(dir, "pending-ref.json"),
            JsonSerializer.Serialize(new { proposalId = proposal.ProposalId }, JsonFiles.Json)).ConfigureAwait(false);
        return Path.GetFullPath(dir);   // absolut: Konsumenten (Apply/Steward) sind nicht CWD-gebunden
    }
}
