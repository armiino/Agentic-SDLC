using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2d ① Baustein 1+2 (c2d-plan §3-1/§3-2): eigener comments-snapshot (Parse aus der GitHub-API-Form)
// + Kommentar-Gedächtnis als High-Water-Mark IN der Wahrheit (Mapping-Relation vor Item-Claim, monoton,
// Ablehnung stempelt auch; kein Anker = LAUT false, dokumentierte Grenze statt Zweit-Store).
public sealed class GithubCommentMetaTests
{
    private static ProjectStateItem Item(string id, string type, string text, Dictionary<string, string>? meta = null)
        => new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, [], [],
            meta ?? new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateDocument Doc(IReadOnlyList<ProjectStateItem> items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static ProjectStateDocument WithMapping(ProjectStateDocument core, string pbiId, int issue)
        => CoreGithubMapping.Apply(core, [new GithubMappingOp(pbiId, issue, Kind: GithubMappingKind.Link, Origin: "TEST")]).Core;

    private static GithubIssueCommentSnapshot Comment(int issue, long id, string body = "b")
        => new(issue, id, "alice", DateTime.UnixEpoch, body);

    // ---- Baustein 1: Parse aus der API-Form ----

    [Fact]
    public void ParseComment_liest_API_Form_und_verwirft_Kaputtes_statt_zu_raten()
    {
        var ok = GithubIssueSnapshotRunner.ParseComment(JsonDocument.Parse(
            """{"id": 42, "issue_url": "https://api.github.com/repos/o/n/issues/12", "user": {"login": "alice"}, "created_at": "2026-08-09T10:00:00Z", "body": "Bitte auch Angehörige."}""").RootElement);
        Assert.NotNull(ok);
        Assert.Equal(12, ok!.IssueNumber);
        Assert.Equal(42, ok.CommentId);
        Assert.Equal("alice", ok.Author);
        Assert.Equal("Bitte auch Angehörige.", ok.Body);

        Assert.Null(GithubIssueSnapshotRunner.ParseComment(JsonDocument.Parse("""{"body": "ohne id"}""").RootElement));
        Assert.Null(GithubIssueSnapshotRunner.ParseComment(JsonDocument.Parse("""{"id": 7, "issue_url": "kaputt"}""").RootElement));
    }

    // ---- Baustein 2: Anker lesen/filtern/stempeln ----

    [Fact]
    public void NewSince_ohne_Anker_ist_alles_neu_und_mit_Anker_nur_das_Juengere()
    {
        var core = WithMapping(Doc([Item("PBI-1", "pbi", "t")]), "PBI-1", 12);
        var comments = new[] { Comment(12, 10), Comment(12, 20), Comment(99, 5) };

        Assert.Equal(3, GithubCommentMeta.NewSince(core, comments, c => c.IssueNumber, c => c.CommentId).Count);

        var (stamped, ok) = GithubCommentMeta.Stamp(core, 12, 10);
        Assert.True(ok);
        var fresh = GithubCommentMeta.NewSince(stamped, comments, c => c.IssueNumber, c => c.CommentId);
        Assert.Equal(2, fresh.Count);                                    // 20 (nach Anker) + 5 (fremdes Issue ohne Anker)
        Assert.DoesNotContain(fresh, c => c.CommentId == 10);
    }

    [Fact]
    public void Stamp_bevorzugt_Mapping_faellt_auf_Item_Claim_zurueck_und_bleibt_monoton()
    {
        // Mapping-Anker (PBI-Issue):
        var mapped = WithMapping(Doc([Item("PBI-1", "pbi", "t")]), "PBI-1", 12);
        var (afterMapping, ok1) = GithubCommentMeta.Stamp(mapped, 12, 30);
        Assert.True(ok1);
        Assert.Equal(30, GithubCommentMeta.LastProcessedFor(afterMapping, 12));

        // Monotonie: aelterer Stempel ueberschreibt nie.
        var (afterOld, ok2) = GithubCommentMeta.Stamp(afterMapping, 12, 20);
        Assert.True(ok2);
        Assert.Equal(30, GithubCommentMeta.LastProcessedFor(afterOld, 12));

        // Item-Claim-Anker (adoptiertes Issue ohne Mapping, z. B. Frage->DEC aus 9i):
        var adopted = Doc([Item("DEC-001", "decision", "Frage?", new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "30" })]);
        var (afterItem, ok3) = GithubCommentMeta.Stamp(adopted, 30, 77);
        Assert.True(ok3);
        Assert.Equal(77, GithubCommentMeta.LastProcessedFor(afterItem, 30));

        // Kein Anker (nie adoptiert, kein Mapping) -> LAUT false, Core unveraendert (dokumentierte Grenze).
        var (unchanged, ok4) = GithubCommentMeta.Stamp(adopted, 99, 5);
        Assert.False(ok4);
        Assert.Same(adopted.Items, unchanged.Items);
    }
}
