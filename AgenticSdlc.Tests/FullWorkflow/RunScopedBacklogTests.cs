using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Steward;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1d Catch-up-Schalter (19.08., Block-E/L-Fund „Scope-Überraschung"): der Betriebs-Lauf bearbeitet an
// classify/adr NUR die eigenen Items (I7-sourceRunId); der Bestands-Rückstau ist ein bewusster Akt
// (--arch-catchup) und wird beim Zurückstellen LAUT gezählt (kein stiller Cap). EINE Quelle für beide Strips.
public sealed class RunScopedBacklogTests
{
    private static ProjectStateItem Item(string id, string sourceRun) => new ProjectStateItem(
        id, "architecture", "Text " + id, "MEETING", null, 1, sourceRun, null, null, null, null, [], [],
        new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    [Fact]
    public void Betriebs_Scope_nimmt_nur_die_eigenen_Items_und_zaehlt_den_Rueckstau()
    {
        IReadOnlyList<ProjectStateItem> backlog = [Item("ARCH-1", "run-alt"), Item("ARCH-2", "run-neu"), Item("ARCH-3", "run-alt")];

        var (own, deferred) = RunScopedBacklog.Scope(backlog, "run-neu", catchUp: false);
        Assert.Equal(["ARCH-2"], own.Select(i => i.ItemId));               // nur der eigene Beitrag
        Assert.Equal(2, deferred);                                          // Rückstau EHRLICH gezählt

        var (alle, keine) = RunScopedBacklog.Scope(backlog, "run-neu", catchUp: true);
        Assert.Equal(3, alle.Count);                                        // der bewusste Akt nimmt alles
        Assert.Equal(0, keine);

        Assert.Contains("--arch-catchup", RunScopedBacklog.DeferredLine("arch-classify", 2));  // Ausweg steht dabei
    }

    // 1f-② (Zombie-Fund 19.08.): die eine testbare Wächter-Regel des Leerlauf-Timeouts + der Options-Default.
    [Fact]
    public void Leerlauf_Waechter_Regel_und_Default()
    {
        var t0 = new DateTime(2026, 8, 19, 10, 0, 0, DateTimeKind.Utc);
        Assert.False(AgenticSdlc.HumanReview.LocalReviewServerHost.IdleExceeded(t0, t0.AddMinutes(119), TimeSpan.FromHours(2)));
        Assert.True(AgenticSdlc.HumanReview.LocalReviewServerHost.IdleExceeded(t0, t0.AddMinutes(121), TimeSpan.FromHours(2)));
        Assert.Equal(TimeSpan.FromHours(2), new AgenticSdlc.HumanReview.ReviewServerOptions
        { Session = new AgenticSdlc.HumanReview.ReviewSession { SessionId = "s", Title = "t", Items = [] } }.IdleTimeout);
    }

    // Kosmetik-Feil (Abnahme 4.0): async ⏸/✔-Zeilen kleben nicht mehr am wartenden du>-Prompt —
    // sie setzen sich ab und echoen den Prompt neu; ohne wartenden Prompt bleibt die Zeile schlicht.
    [Fact]
    public void Lifecycle_Zeile_setzt_sich_vom_wartenden_Prompt_ab_und_echot_ihn_neu()
    {
        var wartend = StewardRunConsole.RenderLifecycle("[steward] ⏸ Lauf X haelt.", promptWaiting: true);
        Assert.StartsWith(Environment.NewLine, wartend);
        Assert.EndsWith("du> ", wartend);

        var frei = StewardRunConsole.RenderLifecycle("[steward] ✔ Lauf X ist FERTIG.", promptWaiting: false);
        Assert.Equal("[steward] ✔ Lauf X ist FERTIG." + Environment.NewLine, frei);
    }
}
