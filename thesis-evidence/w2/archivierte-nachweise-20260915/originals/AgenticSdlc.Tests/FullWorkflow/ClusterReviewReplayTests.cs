using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B3 (pipeline-full-bootstrap-plan §5/§7): das cluster-review-gate liest im replay-Modus die aufgezeichneten
// Human-Entscheide (UI-#15-Format) per OpId. Hier: Datei-Mapping + das Zusammenspiel mit dem GateResponder
// (unmatchte Ops => reject — Governance-Default).
public sealed class ClusterReviewReplayTests
{
    private static string WriteDecisions(params (string OpId, string Decision)[] decisions)
    {
        var path = Path.Combine(Path.GetTempPath(), $"cluster-replay-{Guid.NewGuid():N}.json");
        var file = new ClusterHumanDecisionsFile("run-x", "tester",
            decisions.Select(d => new ClusterHumanDecision(d.OpId, d.Decision, null)).ToList());
        File.WriteAllText(path, JsonSerializer.Serialize(file));
        return path;
    }

    [Fact]
    public void Load_mappt_apply_und_skip_per_OpId()
    {
        var path = WriteDecisions(("op-1", "apply"), ("op-2", "skip"), ("op-3", "APPLY"));
        try
        {
            var map = ClusterReviewReplay.Load(path)!;
            Assert.True(map["op-1"]);
            Assert.False(map["op-2"]);
            Assert.True(map["op-3"]); // case-insensitiv wie der CLI-Apply
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_ohne_datei_ist_null()
    {
        Assert.Null(ClusterReviewReplay.Load(null));
        Assert.Null(ClusterReviewReplay.Load(Path.Combine(Path.GetTempPath(), "gibt-es-nicht.json")));
    }

    [Fact]
    public void Replay_im_GateResponder_akzeptiert_apply_und_rejected_unmatchte()
    {
        var path = WriteDecisions(("op-1", "apply"), ("op-2", "skip"));
        try
        {
            var items = new[] { new GateItem("op-1"), new GateItem("op-2"), new GateItem("op-neu") };
            var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.Replay, path), items, ClusterReviewReplay.Load(path));

            Assert.Equal(GateOutcome.Resolved, r.Outcome);
            Assert.Equal(["op-1"], r.AcceptedItemIds);
            Assert.Contains("op-neu", r.UnmatchedItemIds); // unmatcht => reject, geloggt
        }
        finally { File.Delete(path); }
    }
}
