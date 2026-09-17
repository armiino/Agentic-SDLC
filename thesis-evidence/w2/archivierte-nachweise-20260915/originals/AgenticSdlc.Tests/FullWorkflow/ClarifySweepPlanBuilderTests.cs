using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C4b (09.08., c4-plan §5 b/c/d + §8) — Antworten-Validierung LAUT, Herkunfts-Naht (kein Fake-REQ),
// und der Apply-Roundtrip: NUR akzeptierte Angleichung löst den Blocker; skip/reject ⇒ Blocker BLEIBT.
public sealed class ClarifySweepPlanBuilderTests
{
    private static ProjectStateItem Pbi(string id, string status) =>
        (new ProjectStateItem(id, "pbi", $"{id} Text", "re-clarify", null, 1, "r", null, null, null, null, [], [],
            new Dictionary<string, string>()) with
        { Pbi = new PbiPayload("Ziel", $"{id} Titel", ["AK alt"], [], [], null, "active", null, null) })
        .WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Core() => new("p", 4, DateTime.UnixEpoch, [],
        [Pbi("PBI-1", "needs_clarify"), Pbi("PBI-2", "active")], [], [], []);

    [Fact]
    public void Validierung_LAUT_und_Herkunfts_Naht_ohne_Fake_REQ()
    {
        var (plan, targets, skipped) = ClarifySweepPlanBuilder.Build(Core(),
        [
            new ClarifySweepAnswer("PBI-1", "Der Stack ist Flutter mit SQLite.", SessionName: "sweep-1"),
            new ClarifySweepAnswer("PBI-2", "egal"),                       // nicht needs_clarify
            new ClarifySweepAnswer("PBI-404", "egal"),                     // unbekannt
            new ClarifySweepAnswer("PBI-1", "   "),                        // leer
        ], "run-1");

        Assert.Equal(3, skipped.Count);
        var op = Assert.Single(plan.Operations);
        Assert.Equal("chat:sweep-1#1", op.AuthorAnswerRef);                // §8: eigene Trigger-ID, kein REQ-Namensraum
        Assert.Equal("", op.RequirementId);                                // KEIN Fake-Requirement
        Assert.Contains("author via steward-chat", op.Rationale);
        Assert.Equal("chat:sweep-1#1", Assert.Single(targets).Triggers.Single().RequirementId);
        Assert.Equal("Der Stack ist Flutter mit SQLite.", targets.Single().Triggers.Single().NewText);
    }

    [Fact]
    public void Apply_Roundtrip_accept_loest_Blocker_skip_laesst_ihn_stehen()
    {
        var core = Core();
        var (plan, _, _) = ClarifySweepPlanBuilder.Build(core,
            [new ClarifySweepAnswer("PBI-1", "Der Stack ist Flutter mit SQLite.", SessionName: "s")], "run-1");
        var align = new PbiAlignment("PBI-1", "Neuer Titel", "Als Team nutzen wir Flutter+SQLite.",
            ["Stack dokumentiert"], "aus Autor-Antwort", ["chat:s#1"]);

        // accept MIT Angleichung ⇒ Blocker fällt (bestehender Apply, unverändert).
        var (applied, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run-1", [align]);
        Assert.Equal(Blocker.None, applied.Items.Single(i => i.ItemId == "PBI-1").ReadStatus().Blocker);
        Assert.Equal("Neuer Titel", applied.Items.Single(i => i.ItemId == "PBI-1").Pbi!.Title);

        // accept OHNE Angleichung (skip der Alignments) ⇒ needs_clarify BLEIBT — kein stiller Erfolg.
        var (ohne, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run-1", null);
        Assert.Equal(Blocker.NeedsClarify, ohne.Items.Single(i => i.ItemId == "PBI-1").ReadStatus().Blocker);

        // Op gar nicht akzeptiert ⇒ unverändert.
        var (rejected, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int>(), "run-1", [align]);
        Assert.Equal(Blocker.NeedsClarify, rejected.Items.Single(i => i.ItemId == "PBI-1").ReadStatus().Blocker);
    }
}
