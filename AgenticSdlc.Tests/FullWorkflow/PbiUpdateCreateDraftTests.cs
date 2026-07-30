using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

// O3b (create-Modus, Variante 2): NEW_PBI erzeugt bei akzeptiertem Draft ein VOLLES PBI (Titel/Goal/AK) + active
// statt eines Skeletts. Das Draft ist über TargetRequirementId adressiert (KEIN Fake-PbiId); PbiId bleibt null.
// Der UI-Fluss trägt den DraftKey (= TargetRequirementId für create) im PbiId-Feld der Decision.
namespace AgenticSdlc.Tests.FullWorkflow;

public sealed class PbiUpdateCreateDraftTests
{
    private static ProjectStateItem Feature(string id, string label) => new(
        id, "feature", label, "active", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>());

    private static ProjectStateItem Req(string id, string text) => new(
        id, "requirement", text, "accepted", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>());

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation NewPbi(string req, string feature)
        => new("NEW_PBI", req, null, feature, null, null, $"neues PBI für {req}");

    private static PbiStateChangePlanDocument Plan(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment>? aligns = null)
        => new(1, "p1", DateTime.UnixEpoch, "create-run", ops, aligns);

    private static PbiAlignment CreateDraft(string req, string feature, string title, string statement, IReadOnlyList<string> ak)
        => new(null, title, statement, ak, $"neues PBI aus {req}", [req], TargetRequirementId: req, TargetFeatureId: feature);

    [Fact]
    public void Collect_sammelt_NewPbi_als_create_Ziel()
    {
        var core = Core(Feature("FC-01", "Export"), Req("REQ-9", "PDF-Export der Bewohnerdaten"));
        var targets = PbiAlignTargets.Collect(Plan([NewPbi("REQ-9", "FC-01")]), core);

        var t = Assert.Single(targets);
        Assert.Null(t.PbiId);                              // kein bestehendes PBI
        Assert.Equal("REQ-9", t.TargetRequirementId);
        Assert.Equal("FC-01", t.TargetFeatureId);
        Assert.Equal("Export", t.FeatureLabel);
        var trig = Assert.Single(t.Triggers);
        Assert.Equal("PDF-Export der Bewohnerdaten", trig.NewText);
    }

    [Fact]
    public void Akzeptierter_Create_Draft_erzeugt_volles_PBI_active()
    {
        var core = Core(Feature("FC-01", "Export"), Req("REQ-9", "PDF-Export"));
        var draft = CreateDraft("REQ-9", "FC-01", "PDF-Export der Bewohnerdaten",
            "Als Pflegekraft will ich Daten als PDF exportieren, damit ich sie teilen kann",
            ["Export als PDF möglich", "enthält alle Profilfelder"]);
        var plan = Plan([NewPbi("REQ-9", "FC-01")], [draft]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "create-run", [draft]);
        var newId = Assert.Single(report.NewPbis);
        var pbi = result.Items.First(i => i.ItemId == newId);

        Assert.Equal("active", pbi.Status);                                     // volles PBI, kein Skelett
        Assert.Equal("PDF-Export der Bewohnerdaten", pbi.Pbi!.Title);
        Assert.Equal("Als Pflegekraft will ich Daten als PDF exportieren, damit ich sie teilen kann", pbi.Pbi.Goal);
        Assert.Equal(["Export als PDF möglich", "enthält alle Profilfelder"], pbi.Pbi.AcceptanceCriteria);
        Assert.Contains("REQ-9", pbi.Pbi.LinkedRequirementIds);
    }

    [Fact]
    public void NewPbi_ohne_Draft_bleibt_Skelett_needs_clarify()
    {
        var core = Core(Feature("FC-01", "Export"), Req("REQ-9", "PDF-Export der Bewohnerdaten"));
        var plan = Plan([NewPbi("REQ-9", "FC-01")]);   // keine Alignments

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "create-run");
        var pbi = result.Items.First(i => i.ItemId == Assert.Single(report.NewPbis));

        Assert.Equal("needs_clarify", pbi.Status);     // ehrliches Skelett wie bisher
        Assert.Empty(pbi.Pbi!.AcceptanceCriteria);
        Assert.Null(pbi.Pbi.Goal);
    }

    [Fact]
    public void AcceptedAlignments_reicht_Create_Draft_ueber_DraftKey_durch()
    {
        var draft = CreateDraft("REQ-9", "FC-01", "Titel", "Statement", ["AK"]);
        var plan = Plan([NewPbi("REQ-9", "FC-01")], [draft]);
        // Der Review trägt den DraftKey (= TargetRequirementId) im PbiId-Feld der Decision.
        var decision = new PbiAlignmentDecision("REQ-9", "accept", null, null, null, null);

        var a = Assert.Single(PbiUpdateApplyExec.AcceptedAlignments(plan, [decision]));
        Assert.Null(a.PbiId);
        Assert.Equal("REQ-9", a.TargetRequirementId);
        Assert.Equal("Titel", a.ProposedTitle);
    }
}
