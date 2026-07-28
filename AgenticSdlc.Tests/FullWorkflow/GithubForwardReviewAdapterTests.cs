using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Erster Adapter-Roundtrip-Test (Blocker-Auflage vor der Adapter-Basis-Konsolidierung):
// BuildSession -> Merge -> Apply muessen Entscheidungen verlustfrei tragen.
// E0.2c (27.07.): Items starten OHNE Vorentscheid; Begruendung ist nur beim skip Pflicht.
public sealed class GithubForwardReviewAdapterTests
{
    private static GithubForwardOp Op(string kind, string pbi, string? body = null) => new(
        Kind: kind, PbiId: pbi, TargetIssueNumber: 7, Title: "T", Body: body, Labels: null,
        SearchedQueries: null, SearchEvidence: null, Anchor: "req-anchor", Rationale: "weil", Origin: "agent");

    private static GithubForwardPlanDocument Plan(params GithubForwardOp[] ops) => new(
        SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourcePbiUpdateRun: "src", Repository: "o/n",
        Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_ein_Item_pro_Op_ohne_Vorentscheid()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("CREATE_ISSUE", "PBI-2")));
        Assert.Equal(2, session.Items.Count);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("", FieldOf(i, GithubForwardReviewAdapter.FieldDecision)));
        Assert.All(session.Items, i => Assert.False(i.Resolved));
    }

    [Fact]
    public void Resolved_apply_ohne_Begruendung_skip_nur_mit()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1")));
        var it = session.Items[0];

        Set(it, GithubForwardReviewAdapter.FieldDecision, "apply");
        Assert.True(GithubForwardReviewAdapter.Resolved(it));

        Set(it, GithubForwardReviewAdapter.FieldDecision, "skip");
        Assert.False(GithubForwardReviewAdapter.Resolved(it));
        Set(it, GithubForwardReviewAdapter.FieldReason, "Duplikat");
        Assert.True(GithubForwardReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_traegt_Entscheidungen_und_Reason_in_die_Datei()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("LINK", "PBI-2")));
        Set(session.Items[0], GithubForwardReviewAdapter.FieldDecision, "apply");
        Set(session.Items[1], GithubForwardReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], GithubForwardReviewAdapter.FieldReason, "Duplikat");

        var file = GithubForwardReviewAdapter.Apply("r1", session);
        Assert.Equal("r1", file.RunId);
        Assert.Equal(2, file.Decisions.Count);
        Assert.Equal("apply", file.Decisions[0].Decision);
        Assert.Equal("skip", file.Decisions[1].Decision);
        Assert.Equal("Duplikat", file.Decisions[1].Reason);
        Assert.Null(file.Decisions[0].Reason);
    }

    [Fact]
    public void MergeExistingDecisions_stellt_gespeicherte_Entscheidungen_wieder_her()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("LINK", "PBI-2")));
        var existing = new GithubForwardDecisionsFile("r1", "human (review-ui)",
            [new GithubForwardDecision("op-0", "skip", "war falsch"), new GithubForwardDecision("op-99", "apply", null)]);

        GithubForwardReviewAdapter.MergeExistingDecisions(session, existing);

        Assert.Equal("skip", FieldOf(session.Items[0], GithubForwardReviewAdapter.FieldDecision));
        Assert.Equal("war falsch", FieldOf(session.Items[0], GithubForwardReviewAdapter.FieldReason));
        Assert.True(session.Items[0].Resolved);
        Assert.Equal("", FieldOf(session.Items[1], GithubForwardReviewAdapter.FieldDecision)); // op-99 ignoriert
    }

    [Fact]
    public void Leere_Entscheidung_gilt_als_nicht_resolved()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1")));
        GithubForwardReviewAdapter.MergeExistingDecisions(session, new GithubForwardDecisionsFile("r1", "x",
            [new GithubForwardDecision("op-0", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }

    // E0.2a (Autor-Nachschärfung 2): UPDATE zeigt den SEMANTISCHEN Diff (Deckung/Status/Readiness/Titel) —
    // Format-Neuschrieb des Bodys ist EINE Hinweiszeile, kein Zeilen-Rauschen. Unveraendertes wird nicht
    // angezeigt; Label-Ersetzung (R-30) warnt separat; ohne Snapshot warnt das Item und zeigt den Vorschlag.
    [Fact]
    public void Update_zeigt_semantischen_Diff_statt_Format_Rauschen()
    {
        var issues = new Dictionary<int, GithubIssueSnapshot>
        {
            [7] = new(7, null, "T",
                "Ziel\nText alt\n- REQ-61: Uebergabe-Notiz\nStatus: needs_clarify · Readiness: needs_clarify",
                "open", ["initial-sync", "pbi"], null, null)
        };
        var session = GithubForwardReviewAdapter.BuildSession("r1",
            Plan(Op("UPDATE_ISSUE", "PBI-1",
                body: "Abgedeckte Requirements: REQ-61, REQ-77\n---\nSync-Metadaten: Status needs_clarify · Readiness needs_clarify")
                with { Labels = ["REQ-61", "REQ-77"] }), issues);

        var diff = Assert.Single(session.Items[0].Notes, n => n.Label.StartsWith("Was aendert sich an Issue #7"));
        Assert.Contains("+ deckt jetzt zusaetzlich REQ-77 ab", diff.Text);
        Assert.DoesNotContain("Status:", diff.Text);          // unveraendert -> nicht anzeigen
        Assert.DoesNotContain("Readiness:", diff.Text);
        Assert.Contains("komplett neu aus dem aktuellen Core-Stand", diff.Text);

        var labelWarnung = Assert.Single(session.Items[0].Notes, n => n.Label.StartsWith("Labels wuerden ERSETZT"));
        Assert.Equal(ReviewNoteKind.Warning, labelWarnung.Kind);
        Assert.Contains("initial-sync", labelWarnung.Text);

        var identisch = GithubForwardReviewAdapter.BuildSession("r1",
            Plan(Op("UPDATE_ISSUE", "PBI-1",
                body: "Ziel\nText alt\n- REQ-61: Uebergabe-Notiz\nStatus: needs_clarify · Readiness: needs_clarify")
                with { Labels = null }), issues);
        Assert.Contains(identisch.Items[0].Notes, n => n.Text.Contains("Keine fachliche Aenderung"));

        var ohneSnapshot = GithubForwardReviewAdapter.BuildSession("r1",
            Plan(Op("UPDATE_ISSUE", "PBI-1", body: "Neuer Body-Vorschlag")));
        Assert.Contains(ohneSnapshot.Items[0].Notes, n => n.Label == "Issue AKTUELL" && n.Kind == ReviewNoteKind.Warning);
        Assert.Contains(ohneSnapshot.Items[0].Notes, n => n.Label.StartsWith("VORSCHLAG NEU") && n.Text.Contains("Neuer Body-Vorschlag"));
    }

    // R-26-Beleg (E0.2): UPDATE eines ungeklaerten PBI (Status needs_clarify im Body) warnt deterministisch —
    // der Aufloese-Schritt fehlt in der Kette; bis dahin ist skip die empfohlene Entscheidung.
    [Fact]
    public void Update_eines_needs_clarify_PBI_warnt()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1",
            Plan(Op("UPDATE_ISSUE", "PBI-1", body: "Reqs: REQ-1\n---\nSync-Metadaten: Status needs_clarify · Readiness -")));
        Assert.Contains(session.Items[0].Notes,
            n => n.Label.Contains("needs_clarify") && n.Kind == ReviewNoteKind.Warning && n.Text.Contains("ueberspringen"));

        var aktiv = GithubForwardReviewAdapter.BuildSession("r1",
            Plan(Op("UPDATE_ISSUE", "PBI-1", body: "Reqs: REQ-1\n---\nSync-Metadaten: Status active · Readiness -")));
        Assert.DoesNotContain(aktiv.Items[0].Notes, n => n.Label.Contains("needs_clarify"));
    }
}
