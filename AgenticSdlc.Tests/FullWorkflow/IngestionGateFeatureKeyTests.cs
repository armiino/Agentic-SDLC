using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O1 (Variante c, deterministisch, kein LLM): IngestionGate prüft, dass ein bei NEW_RELATED gesetzter
// featureKey auf ein bestehendes Core-Feature zeigt — sonst schriebe IngestionApply eine part_of_feature-
// Relation ins Leere (stille Core-Inkonsistenz). Fehlender featureKey bleibt bewusst unverändert; die härtere
// Regel („NEW_RELATED braucht ein Feature") kommt erst mit Feature-Placement/O4, wenn der Reparaturpfad steht.
public sealed class IngestionGateFeatureKeyTests
{
    private static ProjectStateItem Feature(string id, string label) => new(
        id, "feature", label, "active", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>());

    private static ProjectStateItem Req(string id, string text) => new(
        id, "requirement", text, "accepted", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>());

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static StateChangeOperation NewRelated(string incoming, string? featureKey) =>
        new(incoming, StateChangeKind.NewRelated, "neue Anforderung im Feature", null, featureKey, ["claim-1"], "im selben Feature");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", DateTime.UnixEpoch, "delta-path", [.. ops]);

    [Fact]
    public void NewRelated_mit_gueltigem_featureKey_passt()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte"), Req("REQ-1", "bestehend")),
            Plan(NewRelated("REQ-9", "FC-01")));

        Assert.True(report.Pass);
        Assert.DoesNotContain(report.Errors, e => e.Code == "UNKNOWN_FEATURE");
    }

    [Fact]
    public void NewRelated_mit_unbekanntem_featureKey_faellt_deterministisch()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte")),
            Plan(NewRelated("REQ-9", "FC-99")));

        Assert.False(report.Pass);
        var issue = Assert.Single(report.Errors, e => e.Code == "UNKNOWN_FEATURE");
        Assert.Equal("repairable", issue.Repairability);
        Assert.Equal("REQ-9", issue.IncomingItemId);
        Assert.Equal("FC-99", issue.TargetEntityId);
    }

    [Fact]
    public void NewRelated_ohne_featureKey_bleibt_unveraendert()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte")),
            Plan(NewRelated("REQ-9", null)));

        Assert.DoesNotContain(report.Errors, e => e.Code == "UNKNOWN_FEATURE");
        Assert.True(report.Pass);
    }

    [Fact]
    public void Unbekannter_featureKey_ist_reparierbar_Gate_entscheidet_Repair()
    {
        var report = IngestionGate.Check(
            Doc(Req("REQ-9", "neue Anforderung")),
            Doc(Feature("FC-01", "Zugang und Rechte")),
            Plan(NewRelated("REQ-9", "FC-99")));

        Assert.Equal(GateDecision.Repair, IngestionGate.Decide(report, attempt: 1, maxAttempts: 3));
    }
}
