using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2d §3-5 + §3-7 (Rest-Moves): NOTE_COMMENT-Vertrag (kein Mapping, keine Coverage-Pflicht, Ziel+Anker-
// Wachen), deterministische Vermerk-Ableitung aus Lauf-Report (✔ nur nach Apply, ✕ nur mit Begründung,
// Unbelegtes NIE), Herkunfts-Treue im Autor-Diktat (Issue-Nummer + aufgelöste Hashes) und die
// Detect-Wache für Hash-lose Herkunft (kein Dauer-F7).
public sealed class GithubCommentVermerkTests
{
    private static ProjectStateItem Item(string id, string type, string text, Dictionary<string, string>? meta = null)
        => new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, [], [],
            meta ?? new Dictionary<string, string>()).WithStatus(CoreStatus.From("baseline"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    [Fact]
    public void Derive_vermerkt_Apply_und_Reject_aber_nie_Unbelegtes()
    {
        var delta = Doc(
            Item("GH-12", "requirement", "Erinnerung auch an Angehörige.", new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "12", [GithubCommentMeta.AnchorKey] = "20" }),
            Item("GH-12-2", "requirement", "Zweite Aussage.", new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "12", [GithubCommentMeta.AnchorKey] = "21" }),
            Item("GH-60", "requirement", "Ohne Entscheid.", new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "60", [GithubCommentMeta.AnchorKey] = "40" }),
            Item("IN-9", "requirement", "Meeting-Item ohne Kommentar-Herkunft."));

        var report = new IngestionApplyReport(
            [new AppliedOperation("GH-12", "NEW", "REQ-80", "added")], [],
            new IngestionDeltaSummary(1, 0, 0, 0, 0, 0, 0));
        var decisions = new[] { new IngestionHumanDecision("GH-12-2", "skip", "zu vage formuliert") };

        var ops = GithubCommentVermerk.Derive(delta, report, decisions, "run-77");

        var op = Assert.Single(ops);                                     // GH-60 (unbelegt) bekommt KEINEN Vermerk
        Assert.Equal(GithubForwardKind.NoteComment, op.Kind);
        Assert.Equal(12, op.TargetIssueNumber);
        Assert.Contains("✔ Eingepflegt in die Projektwahrheit: REQ-80", op.Body);
        Assert.Contains("✕ Bewusst nicht übernommen — Begründung: zu vage formuliert", op.Body);
        Assert.Contains("run-77", op.Body);
        Assert.False(string.IsNullOrWhiteSpace(op.Anchor));              // Beleg-Pflicht des Gates erfüllt
    }

    // R-62b (20.08., Nachprobe): ein Vermerk-ONLY-Plan (reine Ablehnung) braucht den GitHub-Client —
    // NOTE_COMMENT fehlte in der Schreib-Arten-Liste und der erste solche Plan starb an einer NullReference.
    [Fact]
    public void Vermerk_only_Plan_verlangt_den_GitHub_Client()
    {
        var note = new GithubForwardOp(GithubForwardKind.NoteComment, "gh#43", 43, null, "✕ …", null, null, null, "a", "r", "deterministic");
        var plan = new GithubForwardPlanDocument(1, "p1", DateTime.UnixEpoch, "src", "o/r", [note]);
        Assert.True(GithubForwardApply.RequiresClient(plan, new HashSet<string> { "op-0" }, new HashSet<string>()));
        Assert.False(GithubForwardApply.RequiresClient(plan, new HashSet<string>(), new HashSet<string>()));   // geskippt = kein Client
    }

    // R-61 (20.08., „Geister-Kommentare"): der Reproject-Stempel schrieb die Mapping-Relation FRISCH und
    // löschte dabei den Kommentar-Anker — alle jemals verarbeiteten Kommentare kamen als „neu" zurück.
    [Fact]
    public void Mapping_Refresh_erbt_den_Kommentar_Anker_statt_ihn_zu_loeschen()
    {
        var core = Doc(Item("PBI-1", "pbi", "P"));
        // Erst-Link + Anker stempeln:
        (core, _) = CoreGithubMapping.Apply(core, [new GithubMappingOp("PBI-1", 12, null, "o/r", GithubMappingKind.Link, "LINK")]);
        (core, var stamped) = GithubCommentMeta.Stamp(core, 12, 5313971856);
        Assert.True(stamped);
        // Reproject-artiger Refresh (UPDATE mit neuen Hash-Stempeln, gleiches Issue):
        (core, _) = CoreGithubMapping.Apply(core, [new GithubMappingOp("PBI-1", 12, null, "o/r", GithubMappingKind.Link, "UPDATE",
            ProjectedTitleHash: "t2", ProjectedBodyHash: "b2")]);
        Assert.Equal(5313971856, GithubCommentMeta.LastProcessedFor(core, 12));   // Anker lebt
    }

    // Echo-Schutz (Autor-Fund 20.08. spät, Lauf 165512): die EIGENEN Vermerk-Kommentare des Systems
    // sind NIE Ernte-Beute — sonst destilliert der Agent die zitierten Wahrheits-Sätze als „neu" zurück
    // (GH-7 RESTATE / GH-12 ALREADY_DECIDED waren wörtliche Echos aus „✔ Eingepflegt…").
    [Fact]
    public void Collector_ueberspringt_System_Vermerke_deterministisch()
    {
        Assert.True(GithubCommentVermerk.IsSystemVermerk("**Verarbeitungs-Vermerk** (Lauf `x`):\n\n✔ …"));
        Assert.True(GithubCommentVermerk.IsSystemVermerk("**Kommentar-Verarbeitung** (Lauf `x`):\n\n✔ …"));
        Assert.False(GithubCommentVermerk.IsSystemVermerk("Danke, schaue ich mir am Montag an 👍"));

        var core = Doc(Item("PBI-1", "pbi", "P"));
        var issues = new[] { new GithubIssueSnapshot(7, null, "T", "b", "open", [], null, null) };
        var comments = new[]
        {
            new GithubIssueCommentSnapshot(7, 20, "armiino", null, "**Verarbeitungs-Vermerk** (Lauf `r`):\n\n✔ Eingepflegt: REQ-78 — „…“"),
            new GithubIssueCommentSnapshot(7, 21, "armiino", null, "Echter menschlicher Kommentar.")
        };
        var finds = GithubCommentDistill.Collect(core, issues, comments);
        var find = Assert.Single(finds);
        var c = Assert.Single(find.Comments);                            // NUR der menschliche Kommentar
        Assert.Equal(21, c.CommentId);
    }

    // Projektions-Nachzug ② (Autor-⚖ 20.08., Beleg REJ-012/#45): auch geerntete BODY-Edits (Ernte-Drafts
    // OHNE Kommentar-Anker) bekommen nach dem Gate-Entscheid einen Vermerk — vorher blieb eine Ablehnung
    // für den Editierenden stumm (Zeile verschwand erst beim Reproject, kommentarlos).
    [Fact]
    public void Abgelehnter_Body_Edit_ohne_Kommentar_Anker_bekommt_den_Ablehnungs_Vermerk()
    {
        var delta = Doc(
            Item("GH-45", "architecture", "Push-Zustellungen binnen 60 Sekunden.", new Dictionary<string, string>
            { [GithubOriginMeta.IssueNumber] = "45" }));   // KEIN GithubCommentMeta.AnchorKey — Body-Edit-Herkunft

        var report = new IngestionApplyReport([], [], new IngestionDeltaSummary(0, 0, 0, 0, 0, 0, 0));
        var decisions = new[] { new IngestionHumanDecision("GH-45", "reject", "Abnahme-Testzeile, kein echter Rahmen") };

        var op = Assert.Single(GithubCommentVermerk.Derive(delta, report, decisions, "run-88"));
        Assert.Equal(45, op.TargetIssueNumber);
        Assert.Contains("✕ Bewusst nicht übernommen — Begründung: Abnahme-Testzeile", op.Body);
        Assert.Contains("Verarbeitungs-Vermerk", op.Body);
    }

    [Fact]
    public void Gate_nimmt_NOTE_COMMENT_ohne_Coverage_Pflicht_aber_mit_Ziel_Wache()
    {
        var entries = new[] { new GithubSyncEntry("PBI-1", "T", "active", "ready", [], false, null) };
        var update = new GithubForwardOp(GithubForwardKind.UpdateIssue, "PBI-1", 5, "T", "b", null, null, null, "a", "r", "deterministic");
        var note = new GithubForwardOp(GithubForwardKind.NoteComment, "REQ-80", 12, null, "✔ …", null, null, null, "comment-distill run-77", "r", "deterministic");
        var plan = new GithubForwardPlanDocument(1, "p1", DateTime.UnixEpoch, "src", "o/n", [update, note]);

        var ok = GithubForwardGate.Check(plan, entries,
            [new GithubIssueSnapshot(5, null, "T", "b", "open", [], null, null), new GithubIssueSnapshot(12, null, "F", "b", "open", [], null, null)]);
        Assert.True(ok.Pass, string.Join("; ", ok.Errors.Select(e => e.Code)));   // REQ-80 verlangt KEINE Coverage

        var blind = new GithubForwardOp(GithubForwardKind.NoteComment, "REQ-80", null, null, "x", null, null, null, "a", "r", "deterministic");
        var bad = GithubForwardGate.Check(plan with { Operations = [update, blind] }, entries,
            [new GithubIssueSnapshot(5, null, "T", "b", "open", [], null, null)]);
        Assert.Contains(bad.Errors, e => e.Code == "TARGET_REQUIRED");            // Ziel-Wache wirkt
    }

    [Fact]
    public void AuthorFront_traegt_Issue_Herkunft_mit_aufgeloesten_Hashes()
    {
        var (delta, errors) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("Nur Lese-Zugriff für Angehörige.", "requirement", GithubIssueNumber: 12)],
            "sess-1",
            githubOrigin: n => n == 12
                ? new Dictionary<string, string> { [GithubOriginMeta.IssueUrl] = "u/12", [GithubOriginMeta.HarvestedTitleHash] = "th", [GithubOriginMeta.HarvestedBodyHash] = "bh" }
                : null);

        Assert.Empty(errors);
        var item = Assert.Single(delta!.Items);
        Assert.Equal("12", item.Metadata[GithubOriginMeta.IssueNumber]);
        Assert.Equal("th", item.Metadata[GithubOriginMeta.HarvestedTitleHash]);   // Drift-Anker beim Diktat

        var (plain, _) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("Reines Autor-Thema.", "requirement")], "sess-1");
        Assert.False(plain!.Items[0].Metadata.ContainsKey(GithubOriginMeta.IssueNumber));
    }

    [Fact]
    public void Detect_benennt_Hash_lose_Herkunft_statt_Dauer_F7()
    {
        var core = Doc(Item("REQ-9", "requirement", "diktiert mit Herkunft, aber ohne Snapshot-Treffer",
            new Dictionary<string, string> { [GithubOriginMeta.IssueNumber] = "12" }));

        var report = GithubInboundDetect.Detect(core, [new GithubIssueSnapshot(12, null, "T", "b", "open", [], null, null)]);

        Assert.Empty(report.Finds);                                      // kein F7-Rauschen
        Assert.Contains(report.Skipped, s => s.Contains("#12") && s.Contains("Drift nicht prüfbar"));
    }
}
