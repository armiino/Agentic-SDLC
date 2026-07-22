using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent;

// R3b (2026-07-22): geteilte UI-Review-Orchestrierung — das CollectViaUi-Muster existierte 4x in den *-hitl-
// Runnern (und aehnlich in den -review-Runnern): Resolved setzen, Review-Server mit Autosave-Persist fahren,
// final persistieren, Entscheidungs-Datei zurueckgeben. Die ADAPTER bleiben pro Stufe (Session-Bau, Merge,
// Resolved-Regel, Kontext-Aufloesung, Apply) — das ist Fachlogik.
public static class ReviewUiFlow
{
    public static async Task<(TFile Decisions, ReviewOutcome Outcome)> RunAsync<TFile>(
        ReviewSession session, string decisionsPath,
        Func<ReviewItem, bool> resolved,
        Func<string, string, Task<string>>? resolveContext,
        Func<ReviewSession, TFile> apply,
        bool openBrowser)
    {
        foreach (var it in session.Items) it.Resolved = resolved(it);
        async Task Persist() => await JsonFiles.SaveAsync(decisionsPath, apply(session)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = resolved,
            ResolveContext = resolveContext,
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = openBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        return (apply(session), result.Outcome);
    }
}
