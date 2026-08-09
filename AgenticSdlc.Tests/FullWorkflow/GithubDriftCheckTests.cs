using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2a-2 (08.08., c2-inbound-plan §7) — Hash-Stempel-Naht + die EINE Drift-Quelle: der Stempel entsteht am
// gated Apply (Schreib-Zweige), überlebt stempel-lose Link-Ops (COMMENT/LINK), fällt bewusst beim Remap,
// und GithubDriftCheck unterscheidet None/HumanEdited/Unknown gegen den LETZTEN Schreib-Stand.
public sealed class GithubDriftCheckTests
{
    private static ProjectStateDocument CoreWithPbi() => new("p", 4, DateTime.UnixEpoch, [],
        [new ProjectStateItem("PBI-1", "pbi", "t", "test", null, 1, "r", null, null, null, null, [], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"))],
        [], [], []);

    private static GithubIssueSnapshot Issue(string title, string body) =>
        new(12, "u", title, body, "open", [], null, null);

    [Fact]
    public void Hash_normalisiert_Zeilenenden_und_haengende_Leerzeilen()
    {
        // GitHub liefert Web-Edits mit \r\n — nur ECHTE Inhalts-Änderung darf als Drift zählen.
        Assert.Equal(GithubProjectionHash.Compute("a\nb"), GithubProjectionHash.Compute("a\r\nb\n"));
        Assert.NotEqual(GithubProjectionHash.Compute("a"), GithubProjectionHash.Compute("b"));
        Assert.Equal(16, GithubProjectionHash.Compute("x").Length);
    }

    [Fact]
    public void Stempel_fliesst_durch_den_Apply_und_ueberlebt_stempellose_Link_Ops()
    {
        var (core, _) = CoreGithubMapping.Apply(CoreWithPbi(),
            [new GithubMappingOp("PBI-1", 12, Kind: GithubMappingKind.Link, Origin: "CREATE",
                ProjectedTitleHash: "t-hash", ProjectedBodyHash: "b-hash")]);
        var m = CoreGithubMapping.CurrentMappings(core).Single();
        Assert.Equal("t-hash", m.ProjectedTitleHash);
        Assert.Equal("b-hash", m.ProjectedBodyHash);

        // COMMENT/LINK-Ops tragen keinen Hash — der Drift-Anker darf dabei NICHT verloren gehen (Preserve).
        (core, _) = CoreGithubMapping.Apply(core, [new GithubMappingOp("PBI-1", 12, Origin: "COMMENT")]);
        m = CoreGithubMapping.CurrentMappings(core).Single();
        Assert.Equal("b-hash", m.ProjectedBodyHash);

        // Remap auf ANDERES Issue: alter Stempel gehört zum alten Issue → bewusst weg (Drift = Unknown).
        (core, _) = CoreGithubMapping.Apply(core, [new GithubMappingOp("PBI-1", 99, Origin: "CREATE")]);
        m = CoreGithubMapping.CurrentMappings(core).Single();
        Assert.Equal(99, m.IssueNumber);
        Assert.Null(m.ProjectedBodyHash);
    }

    [Fact]
    public void DriftCheck_unterscheidet_None_HumanEdited_Unknown()
    {
        var entry = new GithubSyncEntry("PBI-1", "Titel", "active", "active", ["REQ-01"], false, null,
            AcceptanceCriteria: ["AK 1"], Statement: "Als Nutzer möchte ich X.");
        var body = GithubIssueTemplate.Render(entry, "test");

        var (core, _) = CoreGithubMapping.Apply(CoreWithPbi(),
            [new GithubMappingOp("PBI-1", 12, Kind: GithubMappingKind.Link, Origin: "CREATE",
                ProjectedTitleHash: GithubProjectionHash.Compute("Titel"),
                ProjectedBodyHash: GithubProjectionHash.Compute(body))]);
        var mapping = CoreGithubMapping.CurrentMappings(core).Single();

        // None: Snapshot == letzter Schreib-Stand — auch wenn GitHub \r\n zurückliefert (Normalisierung).
        Assert.Equal(GithubDrift.None, GithubDriftCheck.Check(mapping, Issue("Titel", body.Replace("\n", "\r\n"))));

        // HumanEdited: ein Mensch hat den Body angefasst (Zusatz-Zeile) bzw. den Titel geändert.
        Assert.Equal(GithubDrift.HumanEdited, GithubDriftCheck.Check(mapping, Issue("Titel", body + "\nBitte Darkmode!")));
        Assert.Equal(GithubDrift.HumanEdited, GithubDriftCheck.Check(mapping, Issue("Neuer Titel", body)));

        // Unknown: Alt-Mapping ohne Stempel (vor C2a-2) — warnen, nicht blocken; erster Apply heilt.
        var (altCore, _) = CoreGithubMapping.Apply(CoreWithPbi(), [new GithubMappingOp("PBI-1", 12)]);
        var alt = CoreGithubMapping.CurrentMappings(altCore).Single();
        Assert.Equal(GithubDrift.Unknown, GithubDriftCheck.Check(alt, Issue("Titel", body)));
    }
}
