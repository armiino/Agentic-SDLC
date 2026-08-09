using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2a-4 (08.08., c2-inbound-plan §1/§8/§9) — der deterministische Ernte-Detektor: F-Fall-Klassifikation,
// Sektions-Diff gegen den Core-Stand, NiC-Opt-out, kein stilles Verwerfen (jedes Issue hat eine Kategorie
// oder einen Skip-Grund). Leit-Testfall a): „ein manueller Edit darf nie mehr stillschweigend verloren gehen."
public sealed class GithubInboundDetectTests
{
    private static ProjectStateDocument CoreWithMappedPbi(string? stampedBody, string title = "Titel")
    {
        var pbi = new ProjectStateItem("PBI-1", "pbi", "Titel", "test", null, 1, "r", null, null, null, null, [], [],
                new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"))
            with
            { Pbi = new PbiPayload(Goal: "Als Nutzer möchte ich X.", Title: "Titel", AcceptanceCriteria: ["AK 1"],
                LinkedRequirementIds: [], OpenDecisionRefs: [], PriorityRank: null, Readiness: "active", Mvp: null, Trace: null) };
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [pbi], [], [], []);
        var (mapped, _) = CoreGithubMapping.Apply(core,
            [new GithubMappingOp("PBI-1", 12, Kind: GithubMappingKind.Link, Origin: "CREATE",
                ProjectedTitleHash: stampedBody is null ? null : GithubProjectionHash.Compute(title),
                ProjectedBodyHash: stampedBody is null ? null : GithubProjectionHash.Compute(stampedBody))]);
        return mapped;
    }

    private static GithubIssueSnapshot Issue(int n, string title, string body, string state = "open", string[]? labels = null)
        => new(n, "u", title, body, state, labels ?? [], null, null);

    [Fact]
    public void F1_manueller_Edit_wird_erkannt_mit_Sektions_Diff_und_Freitext()
    {
        var core = CoreWithMappedPbi(stampedBody: "alter geschriebener Body");
        var edited = "Als Nutzer möchte ich X.\n\nAkzeptanzkriterien:\n- AK 1\n- AK 2 neu vom Menschen\n\n"
                   + "Bitte auch Darkmode beachten!\n\nAbgedeckte Requirements: -\n\n---\nSync-Metadaten: PBI PBI-1 · Status active · Readiness active · Quelle: test";

        var report = GithubInboundDetect.Detect(core, [Issue(12, "Titel", edited)]);

        var find = Assert.Single(report.Finds);
        Assert.Equal(GithubInboundCategory.MappedDrift, find.Category);
        Assert.Equal("PBI-1", find.PbiId);
        Assert.Contains(find.Details, d => d.Contains("AK NEU im Issue") && d.Contains("AK 2 neu vom Menschen"));
        Assert.Contains(find.Details, d => d.Contains("Freitext") && d.Contains("Darkmode"));
        Assert.NotNull(find.Parsed);                                    // Leit-Testfall a): nichts geht verloren
    }

    [Fact]
    public void F2_und_F6_und_Skip_jedes_Issue_hat_eine_Kategorie_oder_einen_Grund()
    {
        var core = CoreWithMappedPbi(stampedBody: "body");
        var report = GithubInboundDetect.Detect(core,
        [
            Issue(20, "Neuer Wunsch", "Bitte Export als PDF."),
            Issue(21, "NiC: Team-Ausflug planen", "..."),
            Issue(22, "Interne Liste", "...", labels: ["Not-In-Core"]),
            Issue(23, "Alt und zu", "...", state: "closed"),
        ]);

        Assert.Contains(report.Finds, f => f.Category == GithubInboundCategory.UnmappedNew && f.IssueNumber == 20 && f.Parsed is not null);
        Assert.Equal(2, report.Finds.Count(f => f.Category == GithubInboundCategory.NicOptOut));   // Präfix UND Label
        Assert.Contains(report.Skipped, s => s.Contains("#23") && s.Contains("geschlossen"));
    }

    [Fact]
    public void Unknown_ohne_Stempel_warnt_und_unveraendert_bleibt_still()
    {
        var unstamped = CoreWithMappedPbi(stampedBody: null);
        var r1 = GithubInboundDetect.Detect(unstamped, [Issue(12, "Titel", "irgendwas")]);
        Assert.Equal(GithubInboundCategory.UnknownStamp, Assert.Single(r1.Finds).Category);

        var stamped = CoreWithMappedPbi(stampedBody: "Body A\nZeile 2");
        var r2 = GithubInboundDetect.Detect(stamped, [Issue(12, "Titel", "Body A\r\nZeile 2\r\n")]);
        Assert.Empty(r2.Finds);                                          // CRLF-Snapshot = kein Falsch-Fund
        Assert.Equal(1, r2.Unchanged);
    }
}
