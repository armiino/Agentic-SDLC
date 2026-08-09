using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2a-3 (08.08., c2-inbound-plan §7) — die Forward-Drift-Sperre am Seed: menschlich editierte Issues bekommen
// FLAG_DRIFT statt UpdateIssue (kein stilles Überschreiben, DER Kollegen-Testfall g); Alt-Mappings ohne
// Stempel blocken NICHT (Unknown → Update mit lautem Hinweis, erster Write stempelt = Konvergenz-Start).
public sealed class GithubForwardDriftSperreTests
{
    private static GithubSyncEntry Entry() => new("PBI-1", "Titel", "active", "active", ["REQ-01"], false, null,
        AcceptanceCriteria: ["AK 1"], Statement: "Als Nutzer möchte ich X.");

    private static GithubIssueSnapshot Issue(string body, string title = "Titel") =>
        new(12, "u", title, body, "open", [], null, null);

    private static GithubMappingRecord Mapping(string? bodyOnGithub, string title = "Titel") => new(
        "PBI-1", 12, "u", "repo", "open",
        ProjectedTitleHash: bodyOnGithub is null ? null : GithubProjectionHash.Compute(title),
        ProjectedBodyHash: bodyOnGithub is null ? null : GithubProjectionHash.Compute(bodyOnGithub));

    private static IReadOnlyList<GithubForwardOp> Seed(GithubMappingRecord mapping, GithubIssueSnapshot issue) =>
        GithubForwardSeed.Seed([Entry()],
            new Dictionary<string, GithubMappingRecord> { ["PBI-1"] = mapping }, [issue]).DeterministicOps;

    [Fact]
    public void HumanEdited_wird_zur_DRIFT_SPERRE_statt_UpdateIssue()
    {
        var ops = Seed(Mapping(bodyOnGithub: "Zuletzt geschriebener Body"),
            Issue("Zuletzt geschriebener Body\n\nManuell ergänzt: bitte Darkmode!"));

        var op = Assert.Single(ops);
        Assert.Equal(GithubForwardKind.FlagDrift, op.Kind);
        Assert.Contains("DRIFT-SPERRE", op.Rationale);
        Assert.Contains("ernten", op.Rationale);                       // Weg raus wird am Op benannt
        Assert.DoesNotContain(ops, o => o.Kind == GithubForwardKind.UpdateIssue);
    }

    [Fact]
    public void Unveraendertes_Issue_bekommt_normales_Update_auch_bei_CRLF_Snapshot()
    {
        var ops = Seed(Mapping(bodyOnGithub: "Body A\nZeile 2"), Issue("Body A\r\nZeile 2\r\n"));

        var op = Assert.Single(ops);
        Assert.Equal(GithubForwardKind.UpdateIssue, op.Kind);
        Assert.DoesNotContain("Drift-Stempel", op.Rationale);          // kein Unknown-Hinweis bei sauberem Stempel
    }

    [Fact]
    public void Alt_Mapping_ohne_Stempel_blockt_nicht_aber_benennt_die_Luecke()
    {
        var ops = Seed(Mapping(bodyOnGithub: null), Issue("Irgendein Body"));

        var op = Assert.Single(ops);
        Assert.Equal(GithubForwardKind.UpdateIssue, op.Kind);          // Unknown → warnen, nicht blocken (§7)
        Assert.Contains("kein Drift-Stempel", op.Rationale);
        Assert.Contains("dieser Write stempelt", op.Rationale);        // Konvergenz-Versprechen am Op
    }
}
