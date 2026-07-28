using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Placement-Stufe): Backlog-Aenderungs-Entscheidungen verlustfrei tragen.
// E0.3 (27.07., R-20): Items starten OHNE Vorentscheid; Begruendung nur beim skip Pflicht; Anreicherung
// loest IDs zu Text auf und zeigt Vorher/Nachher aus der Core-History.
public sealed class PbiUpdateReviewAdapterTests
{
    private static readonly ProjectStateDocument EmptyCore =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static PbiStateChangeOperation Op(string kind = "EXTEND_PBI", string req = "REQ-1", string? pbi = "PBI-1")
        => new(Kind: kind, RequirementId: req, PbiId: pbi, FeatureId: null,
               ReplacementRequirementId: null, OpenDecisionRef: null, Rationale: "weil");

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourceIngestionRun: "src", Operations: ops);

    private static ProjectStateItem Req(string id, string text, string? prev = null) => new(
        id, "requirement", text, "accepted", "test", null, 2,
        null, null, null, null, null, [], [], new Dictionary<string, string>(),
        History: prev is null ? null : [new ProjectStateItemVersion(1, prev, "accepted", "test", null, [], DateTime.UnixEpoch, null)]);

    private static ProjectStateItem PbiItem(string id, string title, string status = "needs_clarify") => new(
        id, "pbi", title, status, "test", null, 1,
        null, null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "Als Team will ich …", Title: title, AcceptanceCriteria: ["AK 1"], LinkedRequirementIds: [],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "backlog_ready", Mvp: null, Trace: null));

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_ohne_Vorentscheid()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op("NEW_PBI", "REQ-2", pbi: null)), EmptyCore);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("", FieldOf(i, PbiUpdateReviewAdapter.FieldDecision)));
        Assert.All(session.Items, i => Assert.False(i.Resolved));
    }

    [Fact]
    public void Resolved_apply_ohne_Begruendung_skip_nur_mit()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        var it = session.Items[0];

        Set(it, PbiUpdateReviewAdapter.FieldDecision, "apply");
        Assert.True(PbiUpdateReviewAdapter.Resolved(it));

        Set(it, PbiUpdateReviewAdapter.FieldDecision, "skip");
        Assert.False(PbiUpdateReviewAdapter.Resolved(it));
        Set(it, PbiUpdateReviewAdapter.FieldReason, "bewusst nicht uebernehmen");
        Assert.True(PbiUpdateReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        Set(session.Items[0], PbiUpdateReviewAdapter.FieldDecision, "apply");
        Set(session.Items[1], PbiUpdateReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], PbiUpdateReviewAdapter.FieldReason, "Duplikat");

        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        Assert.Equal(["apply", "skip"], file.Decisions.Select(d => d.Decision));

        var fresh = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], PbiUpdateReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[1].Resolved);
    }

    [Fact]
    public void Unbekannte_OpIds_in_der_Datei_werden_ignoriert()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(session,
            new PbiUpdateDecisionsFile("r1", "x", [new PbiUpdateDecision("op-99", "skip", null)]));
        Assert.Equal("", FieldOf(session.Items[0], PbiUpdateReviewAdapter.FieldDecision));
    }

    // E0.3: MARK_CHANGED löst IDs zu Text auf, zeigt Vorher/Nachher aus der History und warnt vor needs_clarify.
    [Fact]
    public void MarkChanged_zeigt_Text_VorherNachher_und_needs_clarify_Warnung()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [Req("REQ-1", "Archivierung nach vierzehn Tagen", prev: "Archivierung nach sieben Tagen"),
             PbiItem("PBI-1", "Automatische Archivierung")], [], [], []);

        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op("MARK_CHANGED")), core);
        var notes = session.Items[0].Notes;

        Assert.Contains(notes, n => n.Label.StartsWith("Betroffenes PBI") && n.Text.Contains("Automatische Archivierung"));
        var diff = Assert.Single(notes, n => n.Label.Contains("Vorher/Nachher"));
        Assert.Contains("− Archivierung nach sieben Tagen", diff.Text);
        Assert.Contains("+ Archivierung nach vierzehn Tagen", diff.Text);
        Assert.Contains(notes, n => n.Label == "Danach: needs_clarify" && n.Kind == ReviewNoteKind.Warning && n.Text.Contains("R-26-C"));
        // MARK_CHANGED ist deterministisch -> die Leersatz-Rationale wird NICHT als Platzierungs-Begruendung gezeigt.
        Assert.DoesNotContain(notes, n => n.Label.StartsWith("Warum diese Zuordnung"));
    }

    // E0.3: bei EXTEND/NEW (agentische Platzierung) wird die Rationale als handlungsleitende Begruendung gezeigt.
    [Fact]
    public void Extend_zeigt_agentische_Platzierungs_Begruendung()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [Req("REQ-65", "Zugriffsprotokoll nur fuer Admins"), PbiItem("PBI-2", "Interne Accounts")], [], [], []);
        var op = new PbiStateChangeOperation("EXTEND_PBI", "REQ-65", "PBI-2", null, null, null,
            "REQ-65 ist eine Rechte-Vorgabe und passt zum bestehenden Rollen-PBI statt zu einem neuen.");

        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(op), core);
        Assert.Contains(session.Items[0].Notes,
            n => n.Label.StartsWith("Warum diese Zuordnung") && n.Text.Contains("Rechte-Vorgabe"));
    }
}
