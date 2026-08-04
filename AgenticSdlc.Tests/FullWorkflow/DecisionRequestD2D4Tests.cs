using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-14 D2 („→ Entscheidung"-Knopf) + D4 (sichtbarer Parkplatz + Kangal-Integritätszeile). Der volle D2-Kreis pure:
// Antrag am pbi-Gate → Mint (offene DEC, OHNE contradicts-Kante, PBI geblockt) → Scan zeigt sie mit
// targetEntityId-Fallback + Herkunft → Auflösung KEEP → Unblock. Governance: Begründung Pflicht (= die Frage);
// accept-all beantragt nie (DecisionRequests optional/null).
public sealed class DecisionRequestD2D4Tests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string status, string text = "") => new ProjectStateItem(
        id, type, text.Length > 0 ? text : $"{id} Text", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Pbi(string id, string status) => new ProjectStateItem(
        id, "pbi", $"{id} Titel", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: null, Title: $"{id} Titel", AcceptanceCriteria: [], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: status, Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 4, T, [], [.. items], [], [], []);

    [Fact]
    public void Mint_praegt_offene_DEC_ohne_contradicts_Kante_und_blockt_das_PBI()
    {
        var core = Core(Item("REQ-1", "requirement", "accepted"), Pbi("PBI-1", "needs_clarify"), Item("DEC-004", "decision", "resolved"));
        var (updated, minted, skipped) = DecisionRequestMint.Mint(core,
            [new PbiDecisionRequest("REQ-1", "PBI-1", "Gilt die Frist auch für Bestandsdaten?")], "run-x");

        Assert.Empty(skipped);
        Assert.Equal("DEC-005", Assert.Single(minted));                       // saubere naechste ID (nach DEC-004)
        var dec = updated.Items.First(i => i.ItemId == "DEC-005");
        Assert.True(dec.ReadStatus().IsOpenDecision);
        Assert.Equal("REQ-1", dec.Metadata["targetEntityId"]);
        Assert.Equal(DecisionRequestMint.Origin, dec.Origin);
        Assert.Contains("Gilt die Frist", dec.Text);
        Assert.Empty(updated.Relations);                                       // BEWUSST keine contradicts-Kante

        var pbi = updated.Items.First(i => i.ItemId == "PBI-1");
        Assert.Equal("blocked_by_decision", pbi.Status);
        Assert.Contains("DEC-005", pbi.Pbi!.OpenDecisionRefs);
        Assert.Equal(2, pbi.Version);                                          // History-Notiz + v+1

        Assert.True(CoreKangal.Check(updated).Pass);                           // Kangal-konform (I6 unberuehrt)
    }

    [Fact]
    public void Scan_zeigt_den_Antrag_mit_Fallback_Ziel_und_pbi_Gate_Herkunft_Aufloesung_KEEP_entblockt()
    {
        var core = Core(Item("REQ-1", "requirement", "accepted", "Vierzehn Tage"), Pbi("PBI-1", "active"));
        var (minted, _, _) = DecisionRequestMint.Mint(core, [new PbiDecisionRequest("REQ-1", "PBI-1", "Stakeholder-Frage?")], "run-x");

        var views = DecisionStage.BuildViews(minted);
        var v = Assert.Single(views);
        Assert.Equal("REQ-1", v.TargetRequirementId);                          // targetEntityId-Fallback (keine Kante!)
        Assert.Equal(["PBI-1"], v.BlockedPbis);
        Assert.Contains("Klärungs-Antrag — am pbi-Gate", v.Origin);            // zweite Herkunfts-Quelle

        // Aufloesung KEEP ueber die normale Tor-2-Maschinerie (Flip-Schritt uebergeht graceful — keine Kante da).
        var input = DecisionStage.ToInput([new PipelineDecisionResolution(v.DecisionId, DecisionStage.ActionResolve, DecisionOutcome.KeepOriginal, null, "geklärt")]);
        var (ops, problems) = DecisionResolutionDerivation.Derive(minted, input);
        Assert.Empty(problems);
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "p1", T, ops);
        var (resolved, _) = DecisionResolutionApply.Apply(minted, plan, new HashSet<int> { 0 }, "run-y");

        Assert.False(resolved.Items.First(i => i.ItemId == v.DecisionId).ReadStatus().IsOpenDecision);
        Assert.Equal("active", resolved.Items.First(i => i.ItemId == "PBI-1").Status);   // entblockt — Kreis geschlossen
    }

    [Fact]
    public void Adapter_to_decision_braucht_Begruendung_und_liefert_den_Antrag()
    {
        var core = Core(Item("REQ-1", "requirement", "accepted"), Pbi("PBI-1", "needs_clarify"));
        var plan = new PbiStateChangePlanDocument(1, "plan-1", T, "ingest-run",
            [new PbiStateChangeOperation(PbiUpdateKind.MarkChanged, "REQ-1", "PBI-1", null, null, null, "geaendert")]);
        var session = PbiUpdateReviewAdapter.BuildSession("r1", plan, core);
        var it = session.Items[0];

        void Set(string key, string value)
        {
            it.FieldValues.RemoveAll(f => f.FieldKey == key);
            it.FieldValues.Add(new ReviewFieldValue(key, value));
            it.Resolved = PbiUpdateReviewAdapter.Resolved(it);
        }

        Set(PbiUpdateReviewAdapter.FieldDecision, "to_decision");
        Assert.False(it.Resolved);                                             // ohne Begruendung unvollstaendig
        Set(PbiUpdateReviewAdapter.FieldReason, "Braucht Stakeholder: gilt das auch rückwirkend?");
        Assert.True(it.Resolved);

        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        var requests = PbiUpdateReviewAdapter.DecisionRequestsFrom(plan, file.Decisions);
        var req = Assert.Single(requests);
        Assert.Equal("REQ-1", req.RequirementId);
        Assert.Equal("PBI-1", req.PbiId);
        Assert.Contains("rückwirkend", req.Question);

        Assert.DoesNotContain("op-0", PbiUpdateApplyExec.AcceptedFromDecisions(plan, file.Decisions).Select(i => $"op-{i}"));   // Antrag ≠ apply
    }

    [Fact]
    public void Parkplatz_zaehlt_und_rendert_mit_Kangal_Zeile()
    {
        var core = Core(
            Item("DEC-001", "decision", "open_decision"),
            Pbi("PBI-1", "needs_clarify"), Pbi("PBI-2", "blocked_by_decision"), Pbi("PBI-3", "active"),
            Item("REQ-1", "requirement", "accepted"));
        var p = CoreParkplatz.Count(core);

        Assert.Equal(["DEC-001"], p.OpenDecisions);
        Assert.Equal(["PBI-1"], p.NeedsClarifyPbis);
        Assert.Equal(["PBI-2"], p.BlockedPbis);
        Assert.False(p.Empty);

        var lines = CoreParkplatz.RenderLines(p, CoreKangal.Check(core));
        Assert.Contains(lines, l => l.Contains("1 offene Entscheidung(en) (DEC-001)"));
        Assert.Contains(lines, l => l.Contains("Integrität: Kangal 0 Fehler"));
        Assert.Contains(lines, l => l.Contains("decision-gate"));
    }
}
