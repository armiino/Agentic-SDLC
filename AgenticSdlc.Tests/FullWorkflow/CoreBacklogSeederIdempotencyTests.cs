using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O2 (deterministisch, kein LLM): Regressionsnetz für die §2-Erkenntnis — CoreBacklogSeeder ist
// additiv/idempotent. Gegen einen nicht-leeren Core mit gemischtem Cluster-/PBI-Set legt er NUR Neues an
// und überspringt Bestehendes (per legacyClusterId / legacyPbiId). Genau diese Eigenschaft nutzt der
// Fall-C-Seeder-Adapter (O4, Option 3): er ruft Seed mit einem Mini-ClusterSet des NEUEN Features auf.
public sealed class CoreBacklogSeederIdempotencyTests
{
    private static ProjectStateItem Feature(string id) => new(
        id, "feature", "Label " + id, "active", "re-clarify", null, 1,
        "old-run", null, null, null, null, [], [],
        new Dictionary<string, string> { ["legacyClusterId"] = id });

    private static ProjectStateItem Pbi(string id, string legacyPbiId) => new(
        id, "pbi", "PBI " + id, "active", "re-clarify", null, 1,
        "old-run", null, null, null, null, [], [],
        new Dictionary<string, string> { ["legacyPbiId"] = legacyPbiId });

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static FeatureCluster Cluster(string clusterId, string label) =>
        new(clusterId, "ik-" + clusterId, label, [], [], "rationale");

    private static FeatureClusterSet Clusters(params FeatureCluster[] cs) =>
        new(FeatureClusterSet.CurrentSchemaVersion, "cs-1", "p", "b", DateTime.UnixEpoch, "path", [.. cs]);

    private static ProductBacklogItem Item(string pbiId, string title) =>
        new(pbiId, "ik-" + pbiId, 1, "delivery", title, []);

    private static ProductBacklogDocument Backlog(params ProductBacklogItem[] items) =>
        new(ProductBacklogDocument.CurrentSchemaVersion, "bk-1", "p", "b", DateTime.UnixEpoch, "path", [.. items]);

    [Fact]
    public void Seeder_legt_nur_Neues_an_und_ueberspringt_Bestehendes()
    {
        var core = Core(
            Feature("FC-001"),                   // bestehend
            Pbi("PBI-001", "PBI-FC001-01"));     // bestehend (legacyPbiId)

        var clusters = Clusters(
            Cluster("FC-001", "Bestehend"),      // schon im Core -> skip
            Cluster("FC-002", "Neu"));           // neu -> add

        var backlog = Backlog(
            Item("PBI-FC001-01", "Bestehend"),   // legacyPbiId schon im Core -> skip
            Item("PBI-FC002-01", "Neu"));        // neu -> add (ResolveFeatureId -> FC-002)

        var (updated, report) = CoreBacklogSeeder.Seed(core, clusters, backlog, "op-run");

        Assert.Equal(1, report.FeaturesAdded);   // nur FC-002
        Assert.Equal(1, report.PbisAdded);       // nur PBI-FC002-01
        Assert.Equal(2, report.SkippedExisting); // FC-001 + PBI-FC001-01

        // Bestehendes Feature nicht dupliziert, neues angelegt:
        Assert.Single(updated.Items, i => i.ItemId == "FC-001");
        Assert.Contains(updated.Items, i => i.ItemId == "FC-002" && i.ItemType == "feature");
        // Bestehendes PBI unverändert (genau eine Instanz mit dem legacy-Key):
        Assert.Single(updated.Items, i => i.ItemType == "pbi" && i.Metadata.GetValueOrDefault("legacyPbiId") == "PBI-FC001-01");
    }

    [Fact]
    public void Zweiter_Seed_desselben_Sets_ist_idempotent()
    {
        var core = Core(Feature("FC-001"), Pbi("PBI-001", "PBI-FC001-01"));
        var clusters = Clusters(Cluster("FC-001", "Bestehend"));
        var backlog = Backlog(Item("PBI-FC001-01", "Bestehend"));

        var (updated, report) = CoreBacklogSeeder.Seed(core, clusters, backlog, "op-run");

        Assert.Equal(0, report.FeaturesAdded);
        Assert.Equal(0, report.PbisAdded);
        Assert.Equal(2, report.SkippedExisting);
        Assert.Equal(core.Items.Count, updated.Items.Count); // nichts hinzugefügt
    }
}
