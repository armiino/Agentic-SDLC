using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 9g S3/S4: die zweite Haelfte der Fragen-Rampe — Bootstrap-Bahn (Seeder mintet DECs statt open_question-Items),
// Aufloesung zielloser Frage-DECs am decision-gate (KEEP="geklaert" mit Pflicht-Begruendung; ADOPT/REFINE geguardet)
// und die Sichtbarkeit (Parkplatz-Herkunfts-Aufschluesselung, Ingest-Note "Schon einmal geklaert").
public sealed class MeetingQuestionResolutionTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        "delta-run", null, null, null, null, ["claim-7"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items) => new("p", 4, T, [], [.. items], [], [], []);

    [Fact]
    public void Seeder_mintet_Fragen_als_DECs_statt_open_question_Items()
    {
        var source = Doc(
            Item("REQ-01", "requirement", "Angehörige erhalten Leserechte", status: "baseline"),
            Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"));

        var (core, report) = CoreSeeder.Seed(source, "bootstrap-run");

        Assert.Equal(1, report.QuestionDecs);
        Assert.DoesNotContain(core.Items, i => i.ItemType == "open_question");   // EIN Zuhause: kein zweiter Typ
        var dec = Assert.Single(core.Items, i => i.ItemType == "decision");
        Assert.True(dec.ReadStatus().IsOpenDecision);
        Assert.Equal(MeetingQuestionMint.Origin, dec.Origin);
        Assert.Equal("bootstrap-run", dec.SourceRunId);                          // I7 auch in der Bootstrap-Bahn
        Assert.True(CoreKangal.Check(core).Pass);
    }

    [Fact]
    public void Zielloses_Frage_DEC_laesst_sich_als_geklaert_aufloesen_ADOPT_REFINE_sind_geguardet()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var (minted, _, _) = MeetingQuestionMint.Mint(core, [Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline")], "run-x");
        var decId = minted.Items.Single(i => i.ItemType == "decision").ItemId;

        // ADOPT ohne Ziel -> Problem (nichts abzuloesen); KEEP -> Op ohne Ziel.
        var (badOps, badProblems) = DecisionResolutionDerivation.Derive(minted,
            DecisionStage.ToInput([new PipelineDecisionResolution(decId, DecisionStage.ActionResolve, DecisionOutcome.AdoptNew, "neu", "x")]));
        Assert.Empty(badOps);
        Assert.Contains(badProblems, p => p.Contains("kein Ziel-Requirement"));

        var (ops, problems) = DecisionResolutionDerivation.Derive(minted,
            DecisionStage.ToInput([new PipelineDecisionResolution(decId, DecisionStage.ActionResolve, DecisionOutcome.KeepOriginal, null, "beantwortet durch REQ-1: nur Leserechte")]));
        Assert.Empty(problems);
        var op = Assert.Single(ops);
        Assert.Null(op.TargetRequirementId);

        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "p1", T, ops);
        var (resolved, report) = DecisionResolutionApply.Apply(minted, plan, new HashSet<int> { 0 }, "run-y");

        var dec = resolved.Items.Single(i => i.ItemId == decId);
        Assert.False(dec.ReadStatus().IsOpenDecision);                           // geklaert
        Assert.Contains(dec.History!, h => h.Note!.Contains("freistehende Frage"));
        Assert.True(CoreKangal.Check(resolved).Pass);
    }

    [Fact]
    public void Adapter_zielloses_DEC_verlangt_Begruendung_bei_geklaert_und_verbietet_Uebernehmen()
    {
        var view = new PipelineDecisionItemView("DEC-001", "Dürfen Angehörige eintragen?", null, "", [], "Dürfen Angehörige eintragen?",
            "Offene Frage — im Meeting gestellt");
        var s = PipelineDecisionReviewAdapter.BuildSession("r1", new PipelineDecisionReviewRequest("r1", [view]));
        var it = s.Items[0];
        Assert.Equal("Offene Frage", it.Badge);

        // Saubere Form (kein Marker-Feld): die ITEM-Palette selbst traegt die Wahrheit —
        // ziellose Frage-DECs bieten NUR geklaert/vertagen an (FieldOptions, wie am ingest-Gate).
        var palette = it.FieldOptions[PipelineDecisionReviewAdapter.FieldDecision];
        Assert.Equal(2, palette.Count);
        Assert.DoesNotContain(palette, o => o.Value == PipelineDecisionReviewAdapter.ChoiceAdopt);
        Assert.DoesNotContain(palette, o => o.Value == PipelineDecisionReviewAdapter.ChoiceRefine);

        void Set(string key, string value)
        {
            it.FieldValues.RemoveAll(f => f.FieldKey == key);
            it.FieldValues.Add(new ReviewFieldValue(key, value));
            it.Resolved = PipelineDecisionReviewAdapter.Resolved(it);
        }

        Set(PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceAdopt);
        Assert.False(it.Resolved);                                               // kein Ziel -> Uebernehmen unmoeglich
        Set(PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceKeep);
        Assert.False(it.Resolved);                                               // geklaert OHNE Begruendung: unvollstaendig (P2a)
        Set(PipelineDecisionReviewAdapter.FieldReason, "beantwortet durch REQ-81 (nur Leserechte)");
        Assert.True(it.Resolved);
        Set(PipelineDecisionReviewAdapter.FieldDecision, PipelineDecisionReviewAdapter.ChoiceDefer);
        Assert.True(it.Resolved);                                                // vertagen bleibt frei
    }

    [Fact]
    public void Parkplatz_schluesselt_offene_DECs_nach_Herkunft_auf()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var (withQuestion, _, _) = MeetingQuestionMint.Mint(core, [Item("OQ-01", "open_question", "Frage?", status: "baseline")], "run-x");

        var p = CoreParkplatz.Count(withQuestion);
        Assert.Equal(1, p.OpenDecisionsByOrigin!["Meeting-Frage"]);

        var lines = CoreParkplatz.RenderLines(p, CoreKangal.Check(withQuestion));
        Assert.Contains(lines, l => l.Contains("1 Meeting-Frage"));
    }

    [Fact]
    public void Ingest_Note_warnt_bei_wortgleicher_bereits_geklaerter_Frage()
    {
        // Aufgeloeste DEC mit identischem IdentityKey im Core -> die Note erscheint am OPEN_QUESTION-Vorschlag.
        var answered = Item("DEC-001", "decision", "Dürfen Angehörige eintragen?", status: "resolved") with
        {
            IdentityKey = IdentityKey.From("Dürfen Angehörige eintragen?"),
            Metadata = new Dictionary<string, string> { ["resolutionOutcome"] = "KEEP_ORIGINAL", ["resolvedUtc"] = "2026-08-01T10:00:00Z" }
        };
        var core = Doc(Item("REQ-1", "requirement", "bestehend"), answered);
        var delta = Doc(Item("OQ-02", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"));
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", T, "delta",
            [new StateChangeOperation("OQ-02", StateChangeKind.OpenQuestion, "Dürfen Angehörige eintragen?", null, null, ["claim-7"], "aus dem Meeting")]);

        var session = IngestionReviewAdapter.BuildSession("r1", plan, delta, core);
        var note = Assert.Single(session.Items[0].Notes, n => n.Label == "Schon einmal geklärt");
        Assert.Contains("DEC-001", note.Text);
        Assert.Contains("KEEP_ORIGINAL", note.Text);
    }
}
