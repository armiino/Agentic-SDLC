using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Projektions-Nachzug ④ (Autor-⚖ 20.08.): Labels als GEPFLEGTE Status-Projektion — EINE Berechnung
// für Create und Update, Merge statt Ersetzen (R-30), Altlasten der Juli-Wellen getilgt.
public sealed class GithubIssueLabelsTests
{
    private static GithubSyncEntry Entry(string status) =>
        new("PBI-1", "Titel", status, "ready", [], false, null);

    [Fact]
    public void Familie_ist_pbi_plus_needs_clarify_nur_bei_ungeklaertem_Status()
    {
        Assert.Equal(["pbi"], GithubIssueLabels.For(Entry("active")));
        Assert.Equal(["pbi", "needs-clarify"], GithubIssueLabels.For(Entry("needs_clarify")));
    }

    [Fact]
    public void Geloest_heisst_Label_weg_Fremd_Labels_bleiben()
    {
        // Das Issue trug needs-clarify aus einem früheren Write + ein menschliches Label:
        var vorher = new[] { "needs-clarify", "wontfix-diskussion", "pbi" };
        // Klärung gelöst (Status active) ⇒ Familie neu berechnet, Fremd-Label bleibt.
        Assert.Equal(["wontfix-diskussion", "pbi"], GithubIssueLabels.For(Entry("active"), vorher));
    }

    [Fact]
    public void Altlasten_der_Juli_Wellen_werden_getilgt()
    {
        var vorher = new[] { "initial-sync", "REQ-76", "req-12", "eigenes-label" };
        // initial-sync + REQ-nn = System-Altlast (weg); "req-12" matcht das REQ-Muster case-insensitiv? Nein:
        // das Muster verlangt REQ-<Ziffern> — "req-12" matcht (IgnoreCase, bewusst: GitHub normalisiert Labels klein).
        Assert.Equal(["eigenes-label", "pbi"], GithubIssueLabels.For(Entry("active"), vorher));
    }
}
