using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-33 S0: die re-clarify-Gates sprechen die kanonische Loop-Sprache (Form von IngestionGate/PbiUpdateGate) —
// Issues tragen Repairability, Decide liefert typisiertes GateDecision via GateLoop. Klassifikation: alle
// bekannten Codes = Plan-Qualitaet des Agenten (repairable); unbekannte Codes fallen auf hard.
public sealed class ReClarifyGateDecisionTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static CanonicalRequirement Req(string id, string status = "accepted")
        => new(id, $"Titel {id}", $"Text {id}", status, [], "origin", 1, new Dictionary<string, string>());

    private static CanonicalRequirementsBaseline Baseline(params CanonicalRequirement[] reqs)
        => new("b1", "p1", 1, T, "state.json", reqs, [], []);

    private static FeatureCluster Cluster(string id, params string[] coreReqs)
        => new(id, $"key-{id}", $"Label {id}", coreReqs, [], null);

    private static FeatureClusterSet ClusterSet(params FeatureCluster[] clusters)
        => new(1, "cs-1", "p1", "b1", T, "baseline.json", clusters);

    private static ProductBacklogItem Pbi(string id, params string[] reqIds)
        => new ProductBacklogItem(PbiId: id, IdentityKey: $"key-{id}", Version: 1, Type: "delivery",
               Title: $"Titel {id}", RequirementIds: reqIds) with { AcceptanceCriteria = ["AK 1"] };

    private static ProductBacklogDocument Backlog(params ProductBacklogItem[] items)
        => new(1, "bl-1", "p1", "b1", T, "clusters.json", items);

    [Fact]
    public void Backlog_Fehler_sind_repairable_getaggt()
    {
        // REQ-2 ist Cluster-Core, aber in keinem PBI -> UNCOVERED_CORE.
        var report = ReClarifyBacklogGate.Check(
            ClusterSet(Cluster("FC-001", "CAN-REQ-1", "CAN-REQ-2")),
            Baseline(Req("CAN-REQ-1"), Req("CAN-REQ-2")),
            Backlog(Pbi("PBI-1", "CAN-REQ-1")));

        Assert.False(report.Pass);
        var issue = Assert.Single(report.Errors, e => e.Code == "UNCOVERED_CORE");
        Assert.Equal(Repairability.Repairable, issue.Repairability);
    }

    [Fact]
    public void Backlog_Decide_Repair_bei_reparierbaren_Fehlern_unter_Max()
    {
        var report = ReClarifyBacklogGate.Check(
            ClusterSet(Cluster("FC-001", "CAN-REQ-1", "CAN-REQ-2")),
            Baseline(Req("CAN-REQ-1"), Req("CAN-REQ-2")),
            Backlog(Pbi("PBI-1", "CAN-REQ-1")));

        Assert.Equal(GateDecision.Repair, ReClarifyBacklogGate.Decide(report, attempt: 1, maxAttempts: 2));
        Assert.Equal(GateDecision.MaxAttemptsReached, ReClarifyBacklogGate.Decide(report, attempt: 2, maxAttempts: 2));
    }

    [Fact]
    public void Backlog_Decide_Pass_bei_gruenem_Gate()
    {
        var report = ReClarifyBacklogGate.Check(
            ClusterSet(Cluster("FC-001", "CAN-REQ-1")),
            Baseline(Req("CAN-REQ-1")),
            Backlog(Pbi("PBI-1", "CAN-REQ-1")));

        Assert.True(report.Pass);
        Assert.Equal(GateDecision.Pass, ReClarifyBacklogGate.Decide(report, attempt: 1, maxAttempts: 2));
    }

    [Fact]
    public void Cluster_Recall_Fehler_ist_repairable_und_Decide_Repair()
    {
        // CAN-REQ-2 ist aktiv, aber in keinem Cluster core -> UNPLACED_REQUIREMENT (Recall-Verletzung).
        var report = ReClarifyClusterGate.Check(
            Baseline(Req("CAN-REQ-1"), Req("CAN-REQ-2")),
            [Cluster("FC-001", "CAN-REQ-1")]);

        Assert.False(report.Pass);
        var issue = Assert.Single(report.Errors, e => e.Code == "UNPLACED_REQUIREMENT");
        Assert.Equal(Repairability.Repairable, issue.Repairability);
        Assert.Equal(GateDecision.Repair, ReClarifyClusterGate.Decide(report, attempt: 1, maxAttempts: 2));
    }

    [Fact]
    public void Cluster_Warnungen_sind_getaggt_treiben_Decide_aber_nicht()
    {
        // Superseded REQ nur als crossCutting: kein UNPLACED-Error (retired), aber Warnung CROSSCUTTING_NOT_CORE
        // -> Gate bleibt pass, Decision bleibt Pass (Warnungen treiben den Loop nicht).
        var report = ReClarifyClusterGate.Check(
            Baseline(Req("CAN-REQ-1"), Req("CAN-REQ-2", status: "superseded")),
            [Cluster("FC-001", "CAN-REQ-1") with { CrossCuttingRequirementIds = ["CAN-REQ-2"] }]);

        Assert.True(report.Pass);
        var warning = Assert.Single(report.Warnings, w => w.Code == "CROSSCUTTING_NOT_CORE");
        Assert.Equal(Repairability.Repairable, warning.Repairability);
        Assert.Equal(GateDecision.Pass, ReClarifyClusterGate.Decide(report, attempt: 1, maxAttempts: 2));
    }

    [Fact]
    public void Cluster_superseded_Requirements_muessen_nicht_gedeckt_sein()
    {
        var report = ReClarifyClusterGate.Check(
            Baseline(Req("CAN-REQ-1"), Req("CAN-REQ-2", status: "superseded")),
            [Cluster("FC-001", "CAN-REQ-1")]);

        Assert.True(report.Pass);
        Assert.Equal(GateDecision.Pass, ReClarifyClusterGate.Decide(report, attempt: 1, maxAttempts: 2));
    }
}
