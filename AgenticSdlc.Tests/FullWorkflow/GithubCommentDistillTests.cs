using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2d ① Baustein 3+4 (c2d-plan §1–§3): deterministischer Kommentar-Collector (Anker-Filter, Core-Kontext,
// NiC-Opt-out), Save-Zaum des Destillat-Agenten (Beleg-Pflicht commentIds, clarify_answer braucht targetPbiId),
// clarify_answer-Adapter in die C4-Bahn, Mehrfach-Drafts je Issue im Delta (eindeutige IDs, EINE Quelle)
// und der Apply-Stempel-Kreis (Anker reist als Metadata, gated Apply stempelt — auch bei Ablehnung).
public sealed class GithubCommentDistillTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "active",
        Dictionary<string, string>? meta = null) => new ProjectStateItem(
        id, type, text, "test", null, 1, "r", null, null, null, null, [], [],
        meta ?? new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static GithubIssueSnapshot Issue(int n, string title = "Titel", string[]? labels = null)
        => new(n, $"u/{n}", title, "body", "open", labels ?? [], null, null);

    private static GithubIssueCommentSnapshot Comment(int issue, long id, string body = "Diskussion")
        => new(issue, id, "alice", DateTime.UnixEpoch, body);

    [Fact]
    public void Collect_filtert_Anker_und_NiC_und_liefert_Core_Kontext()
    {
        var core = CoreGithubMapping.Apply(
            Doc(Item("PBI-1", "pbi", "Medikamenten-Erinnerung", "needs_clarify")),
            [new GithubMappingOp("PBI-1", 12, Kind: GithubMappingKind.Link, Origin: "TEST")]).Core;
        var (stamped, _) = GithubCommentMeta.Stamp(core, 12, 10);

        var finds = GithubCommentDistill.Collect(stamped,
            [Issue(12), Issue(50, "NiC: Teamausflug"), Issue(60)],
            [Comment(12, 10), Comment(12, 20), Comment(50, 30), Comment(60, 40), Comment(99, 50)]);

        Assert.Equal(2, finds.Count);                                   // 12 (nur Neues) + 60; NiC 50 raus, 99 unbekannt raus
        var mapped = Assert.Single(finds, f => f.IssueNumber == 12);
        Assert.Equal("PBI-1", mapped.CoreItemId);                       // Kontext: gemapptes PBI
        Assert.True(mapped.NeedsClarify);                               // wartet auf Klärung → clarify_answer-Kandidat
        Assert.Equal([20L], mapped.Comments.Select(c => c.CommentId));  // Anker 10 gefiltert
        Assert.Null(Assert.Single(finds, f => f.IssueNumber == 60).CoreItemId);
    }

    [Fact]
    public async Task SaveZaum_erzwingt_Beleg_Coverage_und_clarify_answer_Ziel()
    {
        var tools = new GithubCommentDistillTools([
            new GithubCommentFind(12, "T", "PBI-1", "Text", true, [Comment(12, 20)]),
            new GithubCommentFind(60, "T2", null, null, false, [Comment(60, 40)]),
        ]);
        var save = tools.Build().OfType<Microsoft.Extensions.AI.AIFunction>().First(t => t.Name == "save_comment_drafts");

        // Fehlerfälle in EINEM Aufruf: fremde commentId, clarify_answer ohne Ziel, Fund 60 ohne Draft.
        var bad = (await save.InvokeAsync(new Microsoft.Extensions.AI.AIFunctionArguments
        {
            ["drafts"] = new[] { new GithubCommentDraft(12, [999], GithubCommentDisposition.ClarifyAnswer, "Antwort", "r") }
        }))!.ToString()!;
        Assert.Contains("Beleg-Pflicht", bad);
        Assert.Contains("targetPbiId", bad);
        Assert.Contains("#60", bad);

        var ok = (await save.InvokeAsync(new Microsoft.Extensions.AI.AIFunctionArguments
        {
            ["drafts"] = new[]
            {
                new GithubCommentDraft(12, [20], GithubCommentDisposition.ClarifyAnswer, "Nur Lese-Zugriff.", "beantwortet Klärung", "PBI-1"),
                new GithubCommentDraft(60, [40], GithubCommentDisposition.Noise, "", "Geplauder"),
            }
        }))!.ToString()!;
        Assert.StartsWith("OK", ok);
        Assert.True(tools.Saved);
    }

    [Fact]
    public void ToClarifyAnswers_baut_C4_Antworten_mit_gh_comment_Ref()
    {
        var answers = GithubCommentDistill.ToClarifyAnswers([
            new GithubCommentDraft(12, [20, 21], GithubCommentDisposition.ClarifyAnswer, "Nur Lese-Zugriff.", "r", "PBI-1"),
            new GithubCommentDraft(60, [40], GithubCommentDisposition.Requirement, "PDF-Export.", "r"),
        ]);

        var a = Assert.Single(answers);
        Assert.Equal("PBI-1", a.PbiId);
        Assert.Equal("gh-comment:12#21", a.AnswerRef);                  // höchster tragender Kommentar
        Assert.Equal("github-comment gh#12", a.Quelle);
    }

    [Fact]
    public void DeltaBuilder_traegt_MehrfachDrafts_eindeutig_mit_Anker_und_einer_Quelle()
    {
        var issues = new[] { Issue(60) };
        var finds = new[] { new GithubInboundFind(GithubInboundCategory.UnmappedNew, 60, "T", null, ["neu"]) };
        var delta = GithubInboundDeltaBuilder.Build([
            new GithubInboundDraft(60, "requirement", "PDF-Export wird angeboten.", "r", CommentAnchor: 40),
            new GithubInboundDraft(60, "open_question", "Welche Rollen brauchen Export?", "r", CommentAnchor: 41),
        ], finds, issues, "run-1");

        Assert.Equal(["GH-60", "GH-60-2"], delta.Items.Select(i => i.ItemId));
        Assert.Equal("40", delta.Items[0].Metadata[GithubCommentMeta.AnchorKey]);
        Assert.Single(delta.Sources);                                   // EINE Quelle je Issue, kein Duplikat
    }

    [Fact]
    public void ApplyStempel_verarbeitet_Annahme_UND_Ablehnung_ueber_den_Anker()
    {
        // Adoptiertes Issue 60 (Item claimt via GithubOriginMeta) + Delta-Incoming mit Kommentar-Anker.
        var core = Doc(Item("REQ-9", "requirement", "bestehend", meta: new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "60" }));
        var delta = Doc(Item("GH-60", "requirement", "PDF-Export wird angeboten.", "baseline",
            new Dictionary<string, string>
            {
                [GithubOriginMeta.IssueNumber] = "60",
                [GithubCommentMeta.AnchorKey] = "40",
            }));
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "p1",
            DateTime.UnixEpoch, "d", [new StateChangeOperation("GH-60", StateChangeKind.New, "PDF-Export wird angeboten.", null, null, ["c"], "r")]);

        // Kern-Semantik (ohne Datei-I/O): der Stempel gilt fuer PLAN-abgedeckte Incomings — Annahme wie
        // Ablehnung; hier direkt ueber die geteilte Naht wie im IngestionApplyExec-Haken.
        var (applied, _, _) = IngestionApply.Apply(core, delta, plan, new HashSet<string>(StringComparer.Ordinal), "run-x");
        var (stamped, ok) = GithubCommentMeta.Stamp(applied, 60, 40);

        Assert.True(ok);                                                // Anker fand das adoptierende Item
        Assert.Equal(40, GithubCommentMeta.LastProcessedFor(stamped, 60));
        Assert.Empty(GithubCommentMeta.NewSince(stamped, new[] { Comment(60, 40) }, c => c.IssueNumber, c => c.CommentId));
    }
}
