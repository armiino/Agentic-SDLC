using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-14 G1-b (= E0.8): das EDIT-faehige decision-gate-Review. E0-Politik test-fixiert: leerer Default (aktive
// Entscheidung), Klartext-Optionen, Prefill = Meeting-Aussage, Pflicht-Text bei Uebernehmen/Verfeinern,
// Vertagen frei (P2a). Roundtrip: Session -> Apply -> Datei -> ToResponse (fehlende DECs => sicher vertagt).
public sealed class PipelineDecisionReviewAdapterTests
{
    private static PipelineDecisionItemView View(string id = "DEC-001") => new(
        id, $"Widerspruch zu REQ-1: Sieben Tage", "REQ-1", "Vierzehn Tage", ["PBI-1"], "Sieben Tage",
        Origin: "Meeting-Widerspruch — am Ingest-Gate bestätigt · Lauf run-x · Meeting-Item IN-9");

    private static PipelineDecisionReviewRequest Request(params PipelineDecisionItemView[] views)
        => new("r1", views);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
        it.Resolved = PipelineDecisionReviewAdapter.Resolved(it);
    }

    [Fact]
    public void Session_leerer_Default_Prefill_und_Blast_Radius()
    {
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", Request(View()));
        var it = Assert.Single(s.Items);
        Assert.Equal("", FieldOf(it, PipelineDecisionReviewAdapter.FieldDecision));      // E0: keine Durchwink-Falle
        Assert.Equal("Sieben Tage", FieldOf(it, PipelineDecisionReviewAdapter.FieldStatement));   // Prefill = Meeting-Aussage
        Assert.False(it.Resolved);
        Assert.Contains(it.Notes, n => n.Label == "Bestehende Wahrheit (Core)" && n.Text.Contains("Vierzehn Tage"));
        Assert.Contains(it.Notes, n => n.Label == "Meeting sagt dagegen" && n.Text.Contains("Sieben Tage"));   // Gegenüberstellungs-Paar
        Assert.Contains(it.Notes, n => n.Text.Contains("PBI-1"));                        // Blast-Radius sichtbar
        Assert.Contains(it.Notes, n => n.Label == "Woher stammt dieser Widerspruch?" && n.Text.Contains("Lauf run-x"));   // Herkunft sichtbar
        Assert.NotNull(s.BulkAction);                                                     // Experiment-Bulk: Alle vertagen
        Assert.Contains("vertagen", s.BulkAction!.Label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Uebernehmen_und_Verfeinern_brauchen_den_neuen_Text_Vertagen_ist_frei()
    {
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", Request(View()));
        var it = s.Items[0];

        Set(it, PipelineDecisionReviewAdapter.FieldStatement, "");
        Set(it, PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceAdopt);
        Assert.False(it.Resolved);                                        // adopt ohne Text: unvollstaendig
        Set(it, PipelineDecisionReviewAdapter.FieldStatement, "Sieben Tage, DSGVO-geprueft");
        Assert.True(it.Resolved);

        Set(it, PipelineDecisionReviewAdapter.FieldStatement, "");
        Set(it, PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceDefer);
        Assert.True(it.Resolved);                                         // vertagen: keine weiteren Pflichten (P2a)

        Set(it, PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceKeep);
        Assert.True(it.Resolved);                                         // behalten: kein Text noetig
    }

    [Fact]
    public void Apply_uebersetzt_Wahl_in_Resolutions_Roundtrip_ueber_Merge()
    {
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", Request(View("DEC-001"), View("DEC-002")));
        Set(s.Items[0], PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceRefine);
        Set(s.Items[0], PipelineDecisionReviewAdapter.FieldStatement, "Vierzehn Tage, geklaert");
        Set(s.Items[0], PipelineDecisionReviewAdapter.FieldReason, "Team-Entscheid");
        Set(s.Items[1], PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceDefer);

        var file = PipelineDecisionReviewAdapter.Apply("r1", s);
        var r1 = file.Resolutions.First(r => r.DecisionId == "DEC-001");
        Assert.Equal(DecisionStage.ActionResolve, r1.Action);
        Assert.Equal(DecisionOutcome.Refine, r1.Outcome);
        Assert.Equal("Vierzehn Tage, geklaert", r1.NewStatement);
        Assert.Equal("Team-Entscheid", r1.Reason);
        Assert.Equal(DecisionStage.ActionDefer, file.Resolutions.First(r => r.DecisionId == "DEC-002").Action);

        // Reload-Roundtrip: Merge stellt die Wahl wieder her.
        var fresh = PipelineDecisionReviewAdapter.BuildSession("r1", Request(View("DEC-001"), View("DEC-002")));
        PipelineDecisionReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal(PipelineDecisionReviewAdapter.ChoiceRefine, FieldOf(fresh.Items[0], PipelineDecisionReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[0].Resolved);
    }

    [Fact]
    public void Kontext_Bloecke_machen_Ziel_REQ_und_PBIs_anklickbar()
    {
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", Request(View()));
        var it = Assert.Single(s.Items);
        Assert.Contains(it.ContextBlocks, c => c.ResolverKey == "item:REQ-1");
        Assert.Contains(it.ContextBlocks, c => c.ResolverKey == "item:PBI-1" && c.Label.Contains("betroffenes PBI"));
    }

    [Fact]
    public void RenderItemContext_zeigt_PBI_Details_mit_Deckung_und_REQ_mit_deckenden_PBIs()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [
                new ProjectStateItem("REQ-1", "requirement", "Vierzehn Tage", "test", null, 2,
                    "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted")),
                new ProjectStateItem("PBI-1", "pbi", "Archiv", "test", null, 3,
                    "run", null, null, null, null, [], [], new Dictionary<string, string>(),
                    Pbi: new PbiPayload(Goal: "Notizen archivieren", Title: "Archivierung", AcceptanceCriteria: ["AK eins"],
                        LinkedRequirementIds: ["REQ-1"], OpenDecisionRefs: [], PriorityRank: null, Readiness: "blocked_by_decision", Mvp: null, Trace: null))
                    .WithStatus(CoreStatus.From("blocked_by_decision")),
            ],
            [new ProjectStateRelation("PBI-1", "REQ-1", "covers", "test", new Dictionary<string, string>())], [], []);

        var pbi = PipelineDecisionReviewAdapter.RenderItemContext("item:PBI-1", core);
        Assert.Contains("Archivierung", pbi);
        Assert.Contains("REQ-1", pbi);
        Assert.Contains("AK eins", pbi);

        var req = PipelineDecisionReviewAdapter.RenderItemContext("item:REQ-1", core);
        Assert.Contains("Vierzehn Tage", req);
        Assert.Contains("PBI-1", req);

        Assert.Contains("nicht im Core", PipelineDecisionReviewAdapter.RenderItemContext("item:GEIST", core));
    }

    [Fact]
    public void ToResponse_vertagt_DECs_ohne_Eintrag_sicher()
    {
        var request = Request(View("DEC-001"), View("DEC-002"));
        var file = new PipelineDecisionDecisionsFile("r1", "test",
            [new PipelineDecisionResolution("DEC-001", DecisionStage.ActionResolve, DecisionOutcome.KeepOriginal, null, null)]);

        var resp = PipelineDecisionReviewAdapter.ToResponse(file, request);
        Assert.Equal(2, resp.Resolutions.Count);
        Assert.Equal(DecisionStage.ActionResolve, resp.Resolutions.First(r => r.DecisionId == "DEC-001").Action);
        Assert.Equal(DecisionStage.ActionDefer, resp.Resolutions.First(r => r.DecisionId == "DEC-002").Action);   // fail-safe
    }
}
