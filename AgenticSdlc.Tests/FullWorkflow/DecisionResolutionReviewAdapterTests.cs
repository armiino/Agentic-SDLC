using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Tor 2): Auflösungs-Entscheidungen verlustfrei tragen.
public sealed class DecisionResolutionReviewAdapterTests
{
    private static readonly ProjectStateDocument EmptyCore =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static DecisionResolutionOp Op(string dec = "DEC-1", string outcome = "KEEP_ORIGINAL") => new(
        DecisionId: dec, Outcome: outcome, TargetRequirementId: "REQ-1",
        NewStatement: null, AffectedPbis: ["PBI-1"], Rationale: "Stakeholder sagt so");

    private static DecisionResolutionPlanDocument Plan(params DecisionResolutionOp[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    [Fact]
    public void BuildSession_erzeugt_Items_je_Aufloesung()
    {
        var session = DecisionResolutionReviewAdapter.BuildSession("r1", Plan(Op(), Op("DEC-2", "ADOPT_NEW")), EmptyCore);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("apply", FieldOf(i, DecisionResolutionReviewAdapter.FieldDecision)));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = DecisionResolutionReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        var it = session.Items[0];
        it.FieldValues.RemoveAll(f => f.FieldKey == DecisionResolutionReviewAdapter.FieldDecision);
        it.FieldValues.Add(new ReviewFieldValue(DecisionResolutionReviewAdapter.FieldDecision, "skip"));

        var file = DecisionResolutionReviewAdapter.Apply("r1", session);
        Assert.Equal(["skip", "apply"], file.Decisions.Select(d => d.Decision));

        var fresh = DecisionResolutionReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        DecisionResolutionReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[0], DecisionResolutionReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[0].Resolved);
    }

    [Fact]
    public void Leere_Entscheidung_ist_nicht_resolved()
    {
        var session = DecisionResolutionReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        DecisionResolutionReviewAdapter.MergeExistingDecisions(session,
            new DecisionResolutionDecisionsFile("r1", "x", [new DecisionReviewDecision("op-0", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }
}
