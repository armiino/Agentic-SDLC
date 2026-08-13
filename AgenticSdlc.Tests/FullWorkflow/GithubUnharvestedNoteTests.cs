using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Warn-Note „ungeerntete GitHub-Arbeit" (13.08., Auflösung des conditional-edge-Disk-Punkts): pure Funktion
// über der GETEILTEN Detect-Engine — sagt am forward-gate, ob drüben erntbare Arbeit liegt. Wächter:
// 0 Funde ⇒ null (kein leeres Warnen) · NiC zählt nie · neue fremde Issues zählen als „neu".
public sealed class GithubUnharvestedNoteTests
{
    private static ProjectStateDocument EmptyCore() => new(
        "note-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch, [], [], [], [], []);

    private static GithubIssueSnapshot Issue(int n, string title) =>
        new(n, null, title, "Body", "open", [], null, null);

    [Fact]
    public void Keine_Issues_keine_Note()
        => Assert.Null(GithubUnharvested.Compute(EmptyCore(), []));

    [Fact]
    public void Neues_fremdes_Issue_zaehlt_als_neu()
    {
        var note = GithubUnharvested.Compute(EmptyCore(), [Issue(45, "Dark Mode fuer den Nachtdienst")]);
        Assert.NotNull(note);
        Assert.Equal(1, note!.Neu);
        Assert.Equal(0, note.Geaendert);
        Assert.Contains("UNGEERNTET", note.Text);
        Assert.Contains("reproject", note.Text);                       // die saubere Sequenz steht IN der Note
    }

    [Fact]
    public void NiC_Issues_zaehlen_nie_also_keine_Note()
        => Assert.Null(GithubUnharvested.Compute(EmptyCore(), [Issue(46, "NiC: internes Orga-Issue")]));
}
