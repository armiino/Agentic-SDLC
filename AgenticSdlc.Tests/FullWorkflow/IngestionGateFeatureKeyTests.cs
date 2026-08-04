using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-36 v2: der featureKey bei NEW_RELATED ist ein reines HINWEIS-Metadatum fuer die Placement-Stufe —
// IngestionApply schreibt KEINE part_of_feature-Relation mehr (der alte Direkt-Write mit Agent-Freitext
// als Relationsziel war der Verursacher der 12 Audit-Defekte). Die REQ→Feature-Kante entsteht deterministisch
// im pbi-update-Apply (Seed-Regel). Ein nicht aufloesbarer Hinweis ist darum kein Blocker mehr, bleibt aber
// als Gate-WARNUNG sichtbar (Review).
public sealed class IngestionGateFeatureKeyTests
{
    private static ProjectStateItem Feature(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static StateChangeOperation NewRelated(string incoming, string? featureKey) =>
        new(incoming, StateChangeKind.NewRelated, "neue Anforderung im Feature", null, featureKey, ["claim-1"], "im selben Feature");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", DateTime.UnixEpoch, "delta-path", [.. ops]);

    [Fact]
    public void NewRelated_mit_gueltigem_featureKey_passt_ohne_Warnung()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte"), Req("REQ-1", "bestehend")),
            Plan(NewRelated("REQ-9", "FC-01")));

        Assert.True(report.Pass);
        Assert.DoesNotContain(report.Warnings, w => w.Code == "UNKNOWN_FEATURE");
    }

    [Fact]
    public void NewRelated_mit_unbekanntem_featureKey_ist_Warnung_kein_Blocker()
    {
        // R-36 v2: der Key ist kein Relationsziel mehr -> unaufloesbarer Hinweis blockt nicht, wird aber gemeldet.
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte")),
            Plan(NewRelated("REQ-9", "FC-99")));

        Assert.True(report.Pass);
        Assert.DoesNotContain(report.Errors, e => e.Code == "UNKNOWN_FEATURE");
        var issue = Assert.Single(report.Warnings, w => w.Code == "UNKNOWN_FEATURE");
        Assert.Equal("REQ-9", issue.IncomingItemId);
        Assert.Equal("FC-99", issue.TargetEntityId);
        Assert.Equal(GateDecision.Pass, IngestionGate.Decide(report, attempt: 1, maxAttempts: 3));
    }

    [Fact]
    public void NewRelated_ohne_featureKey_bleibt_unveraendert()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte")),
            Plan(NewRelated("REQ-9", null)));

        Assert.DoesNotContain(report.Warnings, w => w.Code == "UNKNOWN_FEATURE");
        Assert.True(report.Pass);
    }

    [Fact]
    public void Apply_NewRelated_schreibt_Hinweis_Metadatum_aber_KEINE_Relation()
    {
        // R-36 v2 Kern: kein Code-Pfad mehr, auf dem Agent-Freitext zur Graph-Kante wird.
        var core = Doc(Feature("FC-01", "Zugang und Rechte"), Req("REQ-1", "bestehend"));
        var delta = Doc(Req("REQ-9", "neue Anforderung"));
        var (updated, report, _) = IngestionApply.Apply(core, delta, Plan(NewRelated("REQ-9", "no-go")),
            new HashSet<string>(StringComparer.Ordinal) { "REQ-9" }, "ingest-run");

        var added = Assert.Single(report.Applied);
        Assert.Equal("added", added.Outcome);
        var newReq = updated.Items.Single(i => i.ItemId == added.EntityId);
        Assert.Equal("no-go", newReq.Metadata.GetValueOrDefault("featureKey"));
        Assert.DoesNotContain(updated.Relations, r => r.RelationType == "part_of_feature" && r.FromId == newReq.ItemId);
    }
}
