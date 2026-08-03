using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Tor 1): Ingestion-Entscheidungen verlustfrei tragen.
// Besonderheit dieser Stufe: ItemId = IncomingItemId (nicht op-{i}).
public sealed class IngestionReviewAdapterTests
{
    private static readonly ProjectStateDocument Empty =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static StateChangeOperation Op(string incoming = "M1-REQ-001", string kind = "NEW") => new(
        IncomingItemId: incoming, Kind: kind, Statement: "Der Nutzer kann X.",
        TargetEntityId: null, FeatureKey: null, ClaimIds: [], Rationale: "nichts Passendes im Core");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops) => new(
        SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch,
        SourceMeetingDeltaPath: "runs/manual/demo/delta.json", Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_nutzt_IncomingItemId_als_ItemId()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002", "REFINE")), Empty, Empty);
        Assert.Equal(["M1-REQ-001", "M1-REQ-002"], session.Items.Select(i => i.ItemId));
    }

    // E0.7: Default ist LEER (aktive Autorisierung jeder Wahrheits-Mutation, keine Durchwink-Falle).
    [Fact]
    public void Default_Entscheidung_ist_leer()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op()), Empty, Empty);
        Assert.Equal("", FieldOf(session.Items[0], IngestionReviewAdapter.FieldDecision));
        Assert.False(session.Items[0].Resolved);
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002")), Empty, Empty);
        Set(session.Items[0], IngestionReviewAdapter.FieldDecision, "apply");
        Set(session.Items[1], IngestionReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], IngestionReviewAdapter.FieldReason, "Regression — Interview-Klärung gilt");

        var file = IngestionReviewAdapter.Apply("r1", session);
        Assert.Equal(["M1-REQ-001", "M1-REQ-002"], file.Decisions.Select(d => d.IncomingItemId));
        Assert.Equal(["apply", "skip"], file.Decisions.Select(d => d.Decision));

        var fresh = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002")), Empty, Empty);
        IngestionReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], IngestionReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[1].Resolved);
    }

    // E0.7: Klartext-Optionen je Op-Art (interner Value bleibt apply/skip).
    [Fact]
    public void Entscheidungs_Optionen_sind_je_OpArt_in_Klartext()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001", "SUPERSEDE"), Op("M1-REQ-002", "CONTRADICT")), Empty, Empty);
        var sup = session.Items[0].FieldOptions[IngestionReviewAdapter.FieldDecision];
        var con = session.Items[1].FieldOptions[IngestionReviewAdapter.FieldDecision];
        Assert.Equal(["apply", "skip"], sup.Select(o => o.Value));
        Assert.Contains("Ersetzen", sup[0].Label);
        Assert.NotEqual(sup[0].Label, con[0].Label);
    }

    // E0.9-P2a: Ablehnen ohne Begründung gilt nicht als erledigt (Audit-Symmetrie).
    [Fact]
    public void Skip_ohne_Begruendung_ist_nicht_resolved()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op()), Empty, Empty);
        var it = session.Items[0];
        Set(it, IngestionReviewAdapter.FieldDecision, "skip");
        Assert.False(IngestionReviewAdapter.Resolved(it));
        Set(it, IngestionReviewAdapter.FieldReason, "doppelt zu REQ-12");
        Assert.True(IngestionReviewAdapter.Resolved(it));
    }

    // E0.7: die deklarierte Experiment-Bulk-Linie ist vorhanden und setzt apply.
    [Fact]
    public void Experiment_Bulk_Linie_ist_gesetzt()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op()), Empty, Empty);
        Assert.NotNull(session.BulkAction);
        Assert.Contains(session.BulkAction!.Set, s => s.FieldKey == IngestionReviewAdapter.FieldDecision && s.Value == "apply");
    }

    // E0.7: der Kontext ist lesbar (kein rohes JSON) und zeigt die Op-Art.
    [Fact]
    public void ResolveContext_ist_lesbar_kein_JSON()
    {
        var plan = Plan(Op("M1-REQ-001", "NEW"));
        var text = IngestionReviewAdapter.ResolveContext("op:M1-REQ-001", plan, Empty, Empty);
        Assert.Contains("Eingehend", text);
        Assert.Contains("Der Nutzer kann X.", text);
        Assert.DoesNotContain("{", text);
    }

    // E0.7 Blast-Radius: ein REFINE auf REQ-12 zeigt die PBIs, die REQ-12 abdecken (covers-Relation).
    [Fact]
    public void BlastRadius_Note_zeigt_betroffene_PBIs()
    {
        var pbi = new ProjectStateItem("PBI-001", "pbi", "Login-Screen bauen", "re-clarify", null, 1,
            null, null, null, null, null, [], [], new Dictionary<string, string>());
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [pbi],
            [new ProjectStateRelation("PBI-001", "REQ-12", "covers", "test", new Dictionary<string, string>())],
            [], []);
        var op = new StateChangeOperation("M1-REQ-002", "REFINE", "Login erweitern.", "REQ-12", null, [], "verfeinert REQ-12");

        var session = IngestionReviewAdapter.BuildSession("r1", Plan(op), Empty, core);

        Assert.Contains(session.Items[0].Notes, n => n.Text.Contains("PBI-001") && n.Label.Contains("Nachgelagert"));
    }

    [Fact]
    public void AcceptedFromDecisions_liefert_nur_applizierte_IncomingItemIds()
    {
        var accepted = IngestionApplyExec.AcceptedFromDecisions(
        [
            new IngestionHumanDecision("M1-REQ-001", "apply", null),
            new IngestionHumanDecision("M1-REQ-002", "skip", null)
        ]);
        Assert.Contains("M1-REQ-001", accepted);
        Assert.DoesNotContain("M1-REQ-002", accepted);
    }
}
