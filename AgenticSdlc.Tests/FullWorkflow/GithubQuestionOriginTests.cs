using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 9i + 9m (09.08.2026, w2-freeze-checkliste Schritt 4): Fragen von aussen fahren auf der bestehenden
// 9g-Schiene zum DEC-Topf, und die GitHub-Herkunft reist als GithubOriginMeta in die Wahrheit —
// Ernte-Gedächtnis (kein wiederkehrendes Rauschen) + deterministischer Forward-Link (kein Duplikat-Issue).
// Dazu der Spur-Exklusiv-Fix: die Fragen-Spur gehört GENAU dem Requirement-Strip (kein Doppel-DEC).
public sealed class GithubQuestionOriginTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted",
        Dictionary<string, string>? meta = null) => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        "delta-run", null, null, null, null, ["claim-1"], [], meta ?? new Dictionary<string, string>())
        .WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(IReadOnlyList<ProjectStateItem> items,
        IReadOnlyList<ProjectStateRelation>? relations = null)
        => new("p", 4, T, [], [.. items], relations ?? [], [], []);

    private static Dictionary<string, string> GhMeta(int issue, string title, string body) => new(StringComparer.Ordinal)
    {
        [GithubOriginMeta.IssueNumber] = issue.ToString(),
        [GithubOriginMeta.IssueUrl] = $"u/{issue}",
        [GithubOriginMeta.HarvestedTitleHash] = GithubProjectionHash.Compute(title),
        [GithubOriginMeta.HarvestedBodyHash] = GithubProjectionHash.Compute(body),
    };

    private static GithubIssueSnapshot Issue(int n, string title, string body, string state = "open")
        => new(n, $"u/{n}", title, body, state, [], null, null);

    private static StateChangeOperation Op(string incoming, string kind, string? target = null,
        string statement = "Sollen Angehörige Zugriff bekommen?")
        => new(incoming, kind, statement, target, null, ["claim-1"], "aus dem Fund");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", T, "delta-path", [.. ops]);

    // ---------- 9i Stecker 1: Frage-Drafts fahren ins Delta ----------

    [Fact]
    public void DeltaBuilder_nimmt_open_question_Drafts_als_open_question_Items_mit()
    {
        var issues = new[] { Issue(30, "Frage Titel", "Sollen Angehörige Zugriff bekommen?"), Issue(31, "Wunsch", "Export als PDF.") };
        var finds = new[]
        {
            new GithubInboundFind(GithubInboundCategory.UnmappedNew, 30, "Frage Titel", null, ["neu"]),
            new GithubInboundFind(GithubInboundCategory.UnmappedNew, 31, "Wunsch", null, ["neu"]),
        };
        var drafts = new[]
        {
            new GithubInboundDraft(30, GithubInboundDisposition.OpenQuestion, "Sollen Angehörige Zugriff bekommen?", "echte Frage"),
            new GithubInboundDraft(31, GithubInboundDisposition.Requirement, "Export als PDF wird angeboten.", "Anforderung"),
        };

        var delta = GithubInboundDeltaBuilder.Build(drafts, finds, issues, "run-1");

        Assert.Equal(2, delta.Items.Count);
        var question = Assert.Single(delta.Items, i => i.ItemType == "open_question");
        Assert.Equal("GH-30", question.ItemId);
        Assert.Equal("30", question.Metadata[GithubOriginMeta.IssueNumber]);        // Herkunft reist mit
        Assert.Contains(delta.Provenance, p => p.ItemId == "GH-30");                // Beleg-Kette wie req
    }

    // ---------- 9i/9m Stecker 2: Herkunft reist beim gated Apply in die Wahrheit ----------

    [Fact]
    public void IngestionApply_traegt_GitHub_Herkunft_bei_NEW_und_OPEN_QUESTION_in_die_Wahrheit()
    {
        var delta = Doc([
            Item("GH-31", "requirement", "Export als PDF wird angeboten.", status: "baseline", meta: GhMeta(31, "Wunsch", "Export als PDF.")),
            Item("GH-30", "open_question", "Sollen Angehörige Zugriff bekommen?", status: "baseline", meta: GhMeta(30, "Frage Titel", "Sollen Angehörige Zugriff bekommen?")),
            Item("IN-9", "requirement", "Meeting-Aussage ohne GitHub-Herkunft.", status: "baseline"),
        ]);
        var core = Doc([Item("REQ-01", "requirement", "bestehend")]);
        var plan = Plan(
            Op("GH-31", StateChangeKind.New, statement: "Export als PDF wird angeboten."),
            Op("GH-30", StateChangeKind.OpenQuestion),
            Op("IN-9", StateChangeKind.New, statement: "Meeting-Aussage ohne GitHub-Herkunft."));

        var (updated, report, _) = IngestionApply.Apply(core, delta, plan,
            new HashSet<string>(["GH-31", "GH-30", "IN-9"], StringComparer.Ordinal), "ingest-run");

        Assert.Equal(3, report.Applied.Count);
        var req = Assert.Single(updated.Items, i => i.Metadata.GetValueOrDefault(GithubOriginMeta.IssueNumber) == "31");
        Assert.Equal("requirement", req.ItemType);
        var dec = Assert.Single(updated.Items, i => i.Metadata.GetValueOrDefault(GithubOriginMeta.IssueNumber) == "30");
        Assert.Equal("decision", dec.ItemType);                                     // Frage → DEC mit Herkunft
        var meetingReq = Assert.Single(updated.Items, i => i.Metadata.GetValueOrDefault("ingestedFrom") == "IN-9");
        Assert.False(meetingReq.Metadata.ContainsKey(GithubOriginMeta.IssueNumber)); // Naht bleibt bahn-neutral
    }

    // ---------- 9i Spur-Exklusivität: Fragen gehören NUR dem Requirement-Strip ----------

    [Fact]
    public void Fragen_Spur_gehoert_nur_dem_Requirement_Strip()
    {
        var delta = Doc([
            Item("GH-40", "architecture", "PostgreSQL ist gesetzt.", status: "baseline"),
            Item("GH-30", "open_question", "Sollen Angehörige Zugriff bekommen?", status: "baseline"),
        ]);
        var core = Doc([Item("ARCH-01", "architecture", "bestehend")]);

        // arch-Strip: die Frage ist WEDER Coverage-Pflicht (kein UNPLACED_INCOMING für GH-30) ...
        var archPass = IngestionGate.Check(delta, core, Plan(
            Op("GH-40", StateChangeKind.New, statement: "PostgreSQL ist gesetzt.")), AspectIngestionProfile.Architecture);
        Assert.True(archPass.Pass);

        // ... NOCH erlaubtes Op-Ziel (OPEN_QUESTION im arch-Plan = Kategorien-Wache).
        var archMismatch = IngestionGate.Check(delta, core, Plan(
            Op("GH-40", StateChangeKind.New, statement: "PostgreSQL ist gesetzt."),
            Op("GH-30", StateChangeKind.OpenQuestion)), AspectIngestionProfile.Architecture);
        Assert.Contains(archMismatch.Errors, e => e.Code == "QUESTION_KIND_MISMATCH" && e.IncomingItemId == "GH-30");

        // req-Strip: unverändert Coverage-Pflicht (9g-Verhalten gepinnt).
        var reqUnplaced = IngestionGate.Check(delta, core, Plan(), AspectIngestionProfile.Requirement);
        Assert.Contains(reqUnplaced.Errors, e => e.Code == "UNPLACED_INCOMING" && e.IncomingItemId == "GH-30");
    }

    // ---------- 9i/9m Ernte-Gedächtnis: Detect überspringt Adoptiertes ----------

    [Fact]
    public void Detect_ueberspringt_adoptierte_Issues_und_meldet_F7_bei_erneutem_Edit()
    {
        var core = Doc([Item("DEC-001", "decision", "Sollen Angehörige Zugriff bekommen?", status: "open_decision",
            meta: GhMeta(30, "Frage Titel", "Sollen Angehörige Zugriff bekommen?"))]);

        var same = GithubInboundDetect.Detect(core, [Issue(30, "Frage Titel", "Sollen Angehörige Zugriff bekommen?")]);
        Assert.Empty(same.Finds);
        Assert.Equal(1, same.Unchanged);                                            // kein Rauschen mehr

        var edited = GithubInboundDetect.Detect(core, [Issue(30, "Frage Titel", "Präzisiert: nur Lese-Zugriff?")]);
        var find = Assert.Single(edited.Finds);
        Assert.Equal(GithubInboundCategory.AdoptedDrift, find.Category);
        Assert.Equal("DEC-001", find.AdoptedItemId);
    }

    // ---------- 9m: deterministischer Forward-Link statt Such-Match ----------

    [Fact]
    public void ForwardSeed_linkt_adoptierte_PBIs_deterministisch()
    {
        var entries = new[] { new GithubSyncEntry("PBI-1", "Titel PBI-1", "active", "ready", ["REQ-1"], false, null) };
        var none = new Dictionary<string, GithubMappingRecord>(StringComparer.Ordinal);

        var linked = GithubForwardSeed.Seed(entries, none, [Issue(31, "Wunsch", "Export als PDF.")],
            adoptedIssueByPbi: new Dictionary<string, int>(StringComparer.Ordinal) { ["PBI-1"] = 31 });
        Assert.Empty(linked.UnmappedPbis);
        var op = Assert.Single(linked.DeterministicOps);
        Assert.Equal(GithubForwardKind.Link, op.Kind);
        Assert.Equal(31, op.TargetIssueNumber);
        Assert.Equal("deterministic", op.Origin);

        var without = GithubForwardSeed.Seed(entries, none, []);
        Assert.Single(without.UnmappedPbis);                                        // proven behavior unverändert
    }

    [Fact]
    public void AdoptedIssueByPbi_liefert_nur_eindeutige_Herkuenfte()
    {
        var core = Doc(
            [
                Item("REQ-1", "requirement", "aus Issue 31", meta: GhMeta(31, "a", "b")),
                Item("REQ-2", "requirement", "aus Issue 32", meta: GhMeta(32, "c", "d")),
                Item("REQ-3", "requirement", "ohne Herkunft"),
                Item("PBI-1", "pbi", "deckt eindeutig"), Item("PBI-2", "pbi", "deckt zwei Herkünfte"),
            ],
            relations:
            [
                new ProjectStateRelation("PBI-1", "REQ-1", "covers", "test", new Dictionary<string, string>()),
                new ProjectStateRelation("PBI-1", "REQ-3", "covers", "test", new Dictionary<string, string>()),
                new ProjectStateRelation("PBI-2", "REQ-1", "covers", "test", new Dictionary<string, string>()),
                new ProjectStateRelation("PBI-2", "REQ-2", "covers", "test", new Dictionary<string, string>()),
            ]);

        var adopted = GithubOriginMeta.AdoptedIssueByPbi(core);

        Assert.Equal(31, adopted["PBI-1"]);                                         // eindeutig → Link-Quelle
        Assert.False(adopted.ContainsKey("PBI-2"));                                 // mehrdeutig → Such-Match
    }

    // ---------- 9i Stecker 3: Autor-Front diktiert Fragen ----------

    [Fact]
    public void AuthorFront_question_Disposition_wird_open_question_Item()
    {
        var (delta, errors) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("Sollen Angehörige Zugriff bekommen?", "question")], "sess-1");

        Assert.Empty(errors);
        var item = Assert.Single(delta!.Items);
        Assert.Equal("open_question", item.ItemType);                               // Autor-Sprache → 9g-Vertrag

        var (_, bad) = AuthorFrontDeltaBuilder.Build([new AuthorStatement("x", "frage")], "sess-1");
        Assert.Contains(bad, e => e.Contains("requirement|architecture|question"));
    }
}
