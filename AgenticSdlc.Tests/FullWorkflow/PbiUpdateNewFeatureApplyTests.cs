using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O4b (Fall C, deterministisch): ein akzeptiertes NEW_FEATURE + akzeptierter O3b-Create-Draft erzeugt über den
// CoreBacklogSeeder-Adapter GENAU ein neues Feature + ein PBI + die drei Relationen (PBI→Feature, PBI→covers→
// Requirement, Requirement→Feature). Enger Scope: ein NEW_FEATURE; gemischte Deltas sind O5.
public sealed class PbiUpdateNewFeatureApplyTests
{
    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation NewFeature(string req, string label)
        => new("NEW_FEATURE", req, null, null, null, null, "eigenständiges neues Thema", ProposedFeatureLabel: label);

    private static PbiStateChangePlanDocument Plan(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment>? aligns = null)
        => new(1, "p1", DateTime.UnixEpoch, "feat-run", ops, aligns);

    private static PbiAlignment CreateDraft(string req, string title, string statement, IReadOnlyList<string> ak)
        => new(null, title, statement, ak, $"neues PBI aus {req}", [req], TargetRequirementId: req);

    [Fact]
    public void Akzeptiertes_NewFeature_mit_Draft_legt_Feature_PBI_und_drei_Relationen_an()
    {
        var core = Core(Req("REQ-9", "Übergabe-Notiz pro Schicht"));
        var draft = CreateDraft("REQ-9", "Schicht-Übergabe-Notiz",
            "Als Pflegekraft will ich eine Übergabe-Notiz je Schicht, damit nichts verloren geht",
            ["Notiz beim Schichtbeginn sichtbar", "pro Schicht genau eine Notiz"]);
        var plan = Plan([NewFeature("REQ-9", "Schichtübergabe")], [draft]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "feat-run", [draft]);

        // Genau EIN neues Feature mit dem vorgeschlagenen Label:
        var feature = Assert.Single(result.Items, i => i.ItemType == "feature");
        Assert.Equal("Schichtübergabe", feature.Feature!.Label);

        // Genau EIN neues PBI mit dem Draft-Inhalt:
        var pbiId = Assert.Single(report.NewPbis);
        var pbi = result.Items.First(i => i.ItemId == pbiId);
        Assert.Equal("Schicht-Übergabe-Notiz", pbi.Pbi!.Title);
        Assert.Equal(["Notiz beim Schichtbeginn sichtbar", "pro Schicht genau eine Notiz"], pbi.Pbi.AcceptanceCriteria);
        Assert.Contains("REQ-9", pbi.Pbi.LinkedRequirementIds);

        // Die drei Relationen:
        Assert.Contains(result.Relations, r => r.FromId == pbiId && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");
        Assert.Contains(result.Relations, r => r.FromId == pbiId && r.ToId == "REQ-9" && r.RelationType == "covers");
        Assert.Contains(result.Relations, r => r.FromId == "REQ-9" && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");

        // Der Report zählt die Seeder-Relationen mit (Audit/CLI-Sauberkeit):
        Assert.True(report.RelationsAdded >= 3);
    }

    [Fact]
    public void NewFeature_ohne_akzeptierten_Draft_legt_kein_aktives_Feature_an()
    {
        // Governance: der Seeder schreibt immer 'active' — ohne autorisierten Draft dürfte kein leeres
        // Skelett-Feature entstehen. Erwartung: skippen + klar reporten, NICHT anlegen.
        var core = Core(Req("REQ-9", "Neues Thema"));
        var plan = Plan([NewFeature("REQ-9", "Neues Feature")]);   // KEIN Create-Draft

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "feat-run");

        Assert.DoesNotContain(result.Items, i => i.ItemType == "feature");
        Assert.Empty(report.NewPbis);
        Assert.Contains(report.Skipped, s => s.Contains("NEW_FEATURE") && s.Contains("REQ-9"));
    }

    [Fact]
    public void NewFeature_bekommt_eine_freie_FC_ID_neben_bestehenden_Features()
    {
        var core = Core(
            Req("REQ-9", "Neues Thema"),
            new ProjectStateItem("FC-01", "feature", "Bestehend", "test", null, 1,
                "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active")));
        var draft = CreateDraft("REQ-9", "Titel", "Statement", ["AK"]);
        var plan = Plan([NewFeature("REQ-9", "Neues Feature")], [draft]);

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "feat-run", [draft]);

        // Bestehendes FC-01 unberührt, neues Feature mit anderer ID:
        Assert.Single(result.Items, i => i.ItemId == "FC-01");
        var newFeature = Assert.Single(result.Items, i => i.ItemType == "feature" && i.ItemId != "FC-01");
        Assert.Equal("Neues Feature", newFeature.Feature!.Label);
    }

    [Fact]
    public void Ohne_NewFeature_Op_legt_der_Apply_kein_Feature_an()
    {
        var core = Core(Req("REQ-9", "Neues Thema"));
        var plan = Plan([]);   // keine Ops

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int>(), "feat-run");

        Assert.DoesNotContain(result.Items, i => i.ItemType == "feature");
        Assert.Empty(report.NewPbis);
    }

    [Fact]
    public void Mehrere_NewFeature_mit_gleichem_Label_ergeben_EIN_Feature_mit_mehreren_PBIs()
    {
        // Kollegen-Fund: der Placement-Agent liefert pro Requirement eine eigene NEW_FEATURE-Op mit gleichem
        // Label (im E2E: 4× "Medikationsbereich"). Erwartung: EIN Feature, mehrere PBIs — nicht N Features.
        var core = Core(Req("REQ-9", "Medikamenten-Übersicht"), Req("REQ-10", "Medikamenten-Erinnerung"));
        var d1 = CreateDraft("REQ-9", "Übersicht der Medikamente", "Statement 1", ["AK1"]);
        var d2 = CreateDraft("REQ-10", "Erinnerung an Gaben", "Statement 2", ["AK2"]);
        var plan = Plan([NewFeature("REQ-9", "Medikationsbereich"), NewFeature("REQ-10", "Medikationsbereich")], [d1, d2]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0, 1 }, "feat-run", [d1, d2]);

        // GENAU EIN neues Feature (gleiches Label -> gruppiert), nicht zwei.
        var feature = Assert.Single(result.Items, i => i.ItemType == "feature");
        Assert.Equal("Medikationsbereich", feature.Feature!.Label);

        // ZWEI PBIs, beide im selben Feature.
        Assert.Equal(2, report.NewPbis.Count);
        foreach (var pbiId in report.NewPbis)
            Assert.Contains(result.Relations, r => r.FromId == pbiId && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");

        // Beide Requirements haengen am selben Feature.
        Assert.Contains(result.Relations, r => r.FromId == "REQ-9" && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");
        Assert.Contains(result.Relations, r => r.FromId == "REQ-10" && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");
    }

    [Fact]
    public void Collect_sammelt_NewFeature_als_create_Ziel()
    {
        // Vom isolierten UI-E2E aufgedeckt: ohne diese Sammlung bekommt der Drafting-Agent für NEW_FEATURE keine
        // Ziele -> 0 Drafts -> Apply skippt alles. Erwartung: ein create-Ziel (kein PbiId, kein TargetFeatureId).
        var core = Core(Req("REQ-9", "Medikamenten-Übersicht pro Bewohner"));
        var targets = PbiAlignTargets.Collect(Plan([NewFeature("REQ-9", "Medikationsbereich")]), core);

        var t = Assert.Single(targets);
        Assert.Null(t.PbiId);
        Assert.Equal("REQ-9", t.TargetRequirementId);
        Assert.Null(t.TargetFeatureId);                 // das Feature existiert noch nicht
        Assert.Equal("Medikationsbereich", t.FeatureLabel);
        Assert.Equal("Medikamenten-Übersicht pro Bewohner", Assert.Single(t.Triggers).NewText);
    }
}
