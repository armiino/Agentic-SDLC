using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O5a (Apply) + O5b (Forward-Projektion), deterministisch: ein Meeting mit MEHREREN Fällen zugleich —
// MARK_CHANGED/align + NEW_PBI/create + NEW_FEATURE/create — läuft durch EINEN PbiUpdateApply.Apply-Aufruf
// (mit akzeptierten Alignments/Drafts, NICHT accept-all). Erwartung: ein konsistentes Core; danach projiziert
// CoreViews.GithubSync (gefiltert auf berührte PBIs, wie PbiUpdateApplyExec) genau die berührten PBIs — keine
// fehlende, keine doppelte Projektion.
public sealed class PbiUpdateMixedDeltaTests
{
    private static ProjectStateItem FeatureItem(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Pbi(string id, string title) => new ProjectStateItem(
        id, "pbi", title, "test", null, 3,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "altes Ziel", Title: title, AcceptanceCriteria: ["alt AK"], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "needs_clarify", Mvp: null, Trace: null)).WithStatus(CoreStatus.From("needs_clarify"));

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    // Der gemischte Plan (Fall 1/B/C zugleich), einmal appliziert. Geteilt von O5a + O5b.
    private static (ProjectStateDocument Result, PbiUpdateApplyReport Report) MixedApply()
    {
        var core = Core(
            FeatureItem("FC-01", "Bestehendes Feature"),
            Pbi("PBI-1", "Altes PBI"),
            Req("REQ-1", "verfeinerte Anforderung"),
            Req("REQ-2", "neue Anforderung im Feature"),
            Req("REQ-3", "neues eigenständiges Thema"));

        var markAlign = new PbiAlignment("PBI-1", "Angeglichenes PBI", "Neues Statement", ["neue AK"], "align an REQ-1", ["REQ-1"]);
        var createPbi = new PbiAlignment(null, "Neues PBI im Feature", "Statement B", ["AK create B"], "create B", ["REQ-2"], TargetRequirementId: "REQ-2");
        var createFeat = new PbiAlignment(null, "Neues-Feature-PBI", "Statement C", ["AK create C"], "create C", ["REQ-3"], TargetRequirementId: "REQ-3");

        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "mixed-run",
            [
                new PbiStateChangeOperation("MARK_CHANGED", "REQ-1", "PBI-1", null, null, null, "verfeinert"),
                new PbiStateChangeOperation("NEW_PBI", "REQ-2", null, "FC-01", null, null, "eigenes PBI"),
                new PbiStateChangeOperation("NEW_FEATURE", "REQ-3", null, null, null, null, "neues Thema", ProposedFeatureLabel: "Neues Feature"),
            ],
            [markAlign, createPbi, createFeat]);

        return PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0, 1, 2 }, "mixed-run", [markAlign, createPbi, createFeat]);
    }

    [Fact]
    public void O5a_Gemischter_Plan_ergibt_ein_konsistentes_Core()
    {
        var (result, report) = MixedApply();

        // 1) MARK_CHANGED + align: PBI-1 angeglichen und active.
        var pbi1 = result.Items.First(i => i.ItemId == "PBI-1");
        Assert.Equal("active", pbi1.Status);
        Assert.Equal("Angeglichenes PBI", pbi1.Pbi!.Title);
        Assert.Contains("PBI-1", report.UpdatedPbis);

        // 2) Genau ZWEI neue PBIs (NEW_PBI + NEW_FEATURE-PBI).
        Assert.Equal(2, report.NewPbis.Count);

        // 3) NEW_PBI im bestehenden Feature FC-01, volles PBI + active.
        var inFeature = result.Items.First(i => report.NewPbis.Contains(i.ItemId) && i.Pbi!.Title == "Neues PBI im Feature");
        Assert.Equal("active", inFeature.Status);
        Assert.Contains(result.Relations, r => r.FromId == inFeature.ItemId && r.ToId == "FC-01" && r.RelationType == "part_of_feature");

        // 4) NEW_FEATURE: genau EIN neues Feature (nicht FC-01) + sein PBI + die drei Relationen.
        var newFeature = Assert.Single(result.Items, i => i.ItemType == "feature" && i.ItemId != "FC-01");
        Assert.Equal("Neues Feature", newFeature.Feature!.Label);
        var featPbi = result.Items.First(i => report.NewPbis.Contains(i.ItemId) && i.Pbi!.Title == "Neues-Feature-PBI");
        Assert.Contains(result.Relations, r => r.FromId == featPbi.ItemId && r.ToId == newFeature.ItemId && r.RelationType == "part_of_feature");
        Assert.Contains(result.Relations, r => r.FromId == featPbi.ItemId && r.ToId == "REQ-3" && r.RelationType == "covers");
        Assert.Contains(result.Relations, r => r.FromId == "REQ-3" && r.ToId == newFeature.ItemId && r.RelationType == "part_of_feature");

        // 5) Ein konsistentes Core-Ergebnis: keine Duplikate von Bestand.
        Assert.Single(result.Items, i => i.ItemId == "FC-01");
        Assert.Single(result.Items, i => i.ItemId == "PBI-1");
    }

    [Fact]
    public void O5b_Forward_Projektion_enthaelt_genau_die_beruehrten_PBIs()
    {
        var (result, report) = MixedApply();

        // Wie PbiUpdateApplyExec: github-sync-Delta = GithubSync(core), gefiltert auf berührte (neu+aktualisiert) PBIs.
        var touched = report.NewPbis.Concat(report.UpdatedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(result).Entries.Where(e => touched.Contains(e.PbiId)).ToList();

        // Genau die drei berührten PBIs (PBI-1 aktualisiert + 2 neue), keine fehlende, keine doppelte.
        Assert.Equal(3, syncDelta.Count);
        Assert.Contains(syncDelta, e => e.PbiId == "PBI-1");
        Assert.All(report.NewPbis, id => Assert.Contains(syncDelta, e => e.PbiId == id));

        // Keine Duplikate und keine unberührten PBIs projiziert.
        Assert.Equal(syncDelta.Select(e => e.PbiId).Distinct().Count(), syncDelta.Count);
        Assert.All(syncDelta, e => Assert.Contains(e.PbiId, touched));
    }
}
