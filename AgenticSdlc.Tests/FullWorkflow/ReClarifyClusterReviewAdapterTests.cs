using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/ReClarify-Cluster): die vom ReviewAgent vorgeschlagenen Cluster-KORREKTUREN
// werden apply/skip-freigegeben (Items aus review.Operations, ItemId = OpId).
public sealed class ReClarifyClusterReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ClusterOperation Op(string id = "op-1", string kind = "add_crosscutting")
        => new(OpId: id, Kind: kind, Rationale: "Querschnitt fehlt") { RequirementId = "CAN-REQ-1", ToClusterId = "CL-1" };

    private static ClusterReviewReport Review(params ClusterOperation[] ops)
        => new(1, "rev-1", "revise", "Korrekturen noetig", []) { Operations = ops };

    private static FeatureClusterSet Clusters()
        => new(1, "cs-1", "p1", "b1", T, "baseline.json",
               [new FeatureCluster("CL-1", "key-cl1", "No-Go-Management", ["CAN-REQ-1"], [], null)]);

    private static CanonicalRequirementsBaseline Baseline() => new("b1", "p1", 1, T, "state.json", [], [], []);

    private static ReClarifyGateReport Gate() => new(true, "pass", [], [], new Dictionary<string, object>());

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_aus_Review_Operationen()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op("op-1"), Op("op-2", "move_core")), "all");
        Assert.Equal(["op-1", "op-2"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var review = Review(Op("op-1"), Op("op-2"));
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), review, "all");
        Set(session.Items[1], ReClarifyClusterReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], ReClarifyClusterReviewAdapter.FieldReason, "Cluster passt schon");

        var file = ReClarifyClusterReviewAdapter.Apply("r1", session);
        Assert.Equal(["op-1", "op-2"], file.Decisions.Select(d => d.OpId));
        Assert.Equal("skip", file.Decisions[1].Decision);

        var fresh = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), review, "all");
        ReClarifyClusterReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], ReClarifyClusterReviewAdapter.FieldDecision));
    }

    [Fact]
    public void Leere_Entscheidung_ist_nicht_resolved()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op()), "all");
        ReClarifyClusterReviewAdapter.MergeExistingDecisions(session,
            new ClusterHumanDecisionsFile("r1", "x", [new ClusterHumanDecision("op-1", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }
}
