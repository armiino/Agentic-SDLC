using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-36 v2: die REQ→Feature-Kante wird DETERMINISTISCH aus der bestaetigten Deckung abgeleitet — die Seed-Regel
// („requirement→part_of_feature aus den PBIs abgeleitet", CoreBacklogSeeder) auch im Betrieb (pbi-update-Apply).
// Nie agent-benannt, idempotent: NEW_PBI und EXTEND schreiben die Kante mit, Doppel-Kanten entstehen nicht.
public sealed class PbiUpdateReqFeatureDerivationTests
{
    private static ProjectStateItem FeatureItem(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Pbi(string id, string title) => new ProjectStateItem(
        id, "pbi", title, "test", null, 1,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "Ziel", Title: title, AcceptanceCriteria: [], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "active", Mvp: null, Trace: null)).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(IReadOnlyList<ProjectStateRelation> relations, params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], relations, [], []);

    private static ProjectStateRelation Rel(string from, string to, string type)
        => new(from, to, type, "test", new Dictionary<string, string>());

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(1, "p1", DateTime.UnixEpoch, "run-x", [.. ops], []);

    [Fact]
    public void NewPbi_leitet_ReqFeature_Kante_aus_der_Deckung_ab()
    {
        var core = Core([], FeatureItem("FC-01", "Feature"), Req("REQ-2", "neue Anforderung"));
        var plan = Plan(new PbiStateChangeOperation("NEW_PBI", "REQ-2", null, "FC-01", null, null, "eigenes PBI"));

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run-x");

        var pbiId = Assert.Single(report.NewPbis);
        Assert.Contains(result.Relations, r => r.FromId == pbiId && r.ToId == "FC-01" && r.RelationType == "part_of_feature");
        Assert.Contains(result.Relations, r => r.FromId == "REQ-2" && r.ToId == "FC-01" && r.RelationType == "part_of_feature");
    }

    [Fact]
    public void ExtendPbi_Req_erbt_das_Feature_des_PBI()
    {
        var core = Core(
            [Rel("PBI-1", "FC-01", "part_of_feature"), Rel("PBI-1", "REQ-1", "covers")],
            FeatureItem("FC-01", "Feature"), Pbi("PBI-1", "Bestehendes PBI"), Req("REQ-1", "alt"), Req("REQ-2", "erweitert"));
        var plan = Plan(new PbiStateChangeOperation("EXTEND_PBI", "REQ-2", "PBI-1", null, null, null, "erweitert PBI-1"));

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run-x");

        Assert.Contains(result.Relations, r => r.FromId == "PBI-1" && r.ToId == "REQ-2" && r.RelationType == "covers");
        Assert.Contains(result.Relations, r => r.FromId == "REQ-2" && r.ToId == "FC-01" && r.RelationType == "part_of_feature");
    }

    [Fact]
    public void Ableitung_ist_idempotent_keine_Doppel_Kante()
    {
        // REQ-2 hat die Kante bereits (z.B. aus dem Seed) -> EXTEND fuegt keine zweite hinzu.
        var core = Core(
            [Rel("PBI-1", "FC-01", "part_of_feature"), Rel("PBI-1", "REQ-1", "covers"), Rel("REQ-2", "FC-01", "part_of_feature")],
            FeatureItem("FC-01", "Feature"), Pbi("PBI-1", "Bestehendes PBI"), Req("REQ-1", "alt"), Req("REQ-2", "erweitert"));
        var plan = Plan(new PbiStateChangeOperation("EXTEND_PBI", "REQ-2", "PBI-1", null, null, null, "erweitert PBI-1"));

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run-x");

        Assert.Single(result.Relations, r => r.FromId == "REQ-2" && r.ToId == "FC-01" && r.RelationType == "part_of_feature");
    }
}
