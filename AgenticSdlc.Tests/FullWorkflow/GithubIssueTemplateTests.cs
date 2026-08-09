using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2a-1 (08.08., c2-inbound-plan §12) — die EINE Struktur-Naht des kanonischen Issue-Bodys:
// Render+Parse als Paar. Der Roundtrip-Test ist der Drift-Stolperdraht zwischen Ersteller (Forward)
// und Leser (Inbound); der Byte-Pinning-Test garantiert „C2a-1 = verhaltensneutral" gegenüber dem
// bisherigen GithubIssueBodySections.Build (E0.1c/E0.2/R-23-Format).
public sealed class GithubIssueTemplateTests
{
    private static GithubSyncEntry Entry() => new(
        PbiId: "PBI-007", Title: "Medikamente erfassen", Status: "active", Readiness: "active",
        CoveredRequirementIds: ["REQ-01", "REQ-52"], BlockedByOpenDecision: false, GithubIssue: null,
        AcceptanceCriteria: ["Eingabemaske vorhanden", "Pflichtfelder validiert"],
        Statement: "Als Pflegekraft möchte ich Medikamente erfassen,\ndamit der Plan aktuell bleibt.",
        Constraints: ["Offline-first (ARCH-02)"],
        CoveredArchitecture: ["ARCH-11 Sync-Konzept"]);

    [Fact]
    public void Roundtrip_Parse_Render_ist_verlustfrei_der_Drift_Stolperdraht()
    {
        var e = Entry();
        var p = GithubIssueTemplate.Parse(GithubIssueTemplate.Render(e, "test"));

        Assert.Equal(e.Statement, p.Statement);
        Assert.Equal(e.AcceptanceCriteria, p.AcceptanceCriteria);
        Assert.Equal(e.Constraints, p.Constraints);
        Assert.Equal(e.CoveredArchitecture, p.CoveredArchitecture);
        Assert.Equal(e.CoveredRequirementIds, p.CoveredRequirementIds);
        Assert.Equal("PBI-007", p.PbiId);
        Assert.Equal("active", p.Status);
        Assert.Equal("active", p.Readiness);
        Assert.Equal("test", p.Quelle);
        Assert.Empty(p.FreeText);                                        // kanonischer Body = restlos zerlegt
    }

    [Fact]
    public void Roundtrip_minimal_ohne_optionale_Sektionen()
    {
        var e = Entry() with { Statement = null, AcceptanceCriteria = null, Constraints = null,
            CoveredArchitecture = null, CoveredRequirementIds = [], Readiness = null };
        var p = GithubIssueTemplate.Parse(GithubIssueTemplate.Render(e, "test"));

        Assert.Null(p.Statement);
        Assert.Empty(p.AcceptanceCriteria);
        Assert.Empty(p.CoveredRequirementIds);                           // "-" wird zu leer, nicht zu ["-"]
        Assert.Equal("-", p.Readiness);                                  // Render schreibt "-" für fehlend
        Assert.Empty(p.FreeText);
    }

    [Fact]
    public void Byte_Pinning_Render_ist_das_unveraenderte_R23_Format()
    {
        // Pinnt das EXAKTE Format des bisherigen GithubIssueBodySections.Build — C2a-1 ist verhaltensneutral;
        // jede bewusste Format-Änderung MUSS diesen Test anfassen (und damit Render+Parse gemeinsam).
        var e = Entry() with { Constraints = null, CoveredArchitecture = null };
        var expected =
            "Als Pflegekraft möchte ich Medikamente erfassen,\ndamit der Plan aktuell bleibt.\n\n"
            + "Akzeptanzkriterien:\n- Eingabemaske vorhanden\n- Pflichtfelder validiert\n\n"
            + "Abgedeckte Requirements: REQ-01, REQ-52\n\n"
            + "---\nSync-Metadaten: PBI PBI-007 · Status active · Readiness active · Quelle: test";
        Assert.Equal(expected, GithubIssueTemplate.Render(e, "test"));
    }

    [Fact]
    public void Menschlicher_Zusatztext_landet_in_FreeText_nie_stiller_Verlust()
    {
        // §8/§12: alles außerhalb der Sektionen = Ernte-Fläche für den Agent-Fallback (C2a-4/C2b).
        var body = GithubIssueTemplate.Render(Entry(), "test")
            .Replace("Abgedeckte Requirements:", "Bitte auch an Darkmode denken!\n\nAbgedeckte Requirements:");
        var p = GithubIssueTemplate.Parse(body);

        Assert.Contains("Bitte auch an Darkmode denken!", p.FreeText);
        Assert.Equal(2, p.AcceptanceCriteria.Count);                     // Sektionen bleiben trotzdem intakt
        Assert.Equal(["REQ-01", "REQ-52"], p.CoveredRequirementIds);
        Assert.Equal("PBI-007", p.PbiId);
    }

    [Fact]
    public void Parse_ist_robust_gegen_leeren_und_fremden_Body()
    {
        Assert.Empty(GithubIssueTemplate.Parse(null).FreeText);
        var fremd = GithubIssueTemplate.Parse("Nur ein Satz ohne jede Struktur.");
        Assert.Equal("Nur ein Satz ohne jede Struktur.", fremd.Statement);   // Statement-Zone = vor erstem Marker
        Assert.Null(fremd.PbiId);
    }
}
