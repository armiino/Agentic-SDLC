using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Erster Adapter-Roundtrip-Test (Blocker-Auflage vor der Adapter-Basis-Konsolidierung):
// BuildSession -> Merge -> Apply muessen Entscheidungen verlustfrei tragen.
public sealed class GithubForwardReviewAdapterTests
{
    private static GithubForwardOp Op(string kind, string pbi) => new(
        Kind: kind, PbiId: pbi, TargetIssueNumber: 7, Title: "T", Body: null, Labels: null,
        SearchedQueries: null, SearchEvidence: null, Anchor: "req-anchor", Rationale: "weil", Origin: "agent");

    private static GithubForwardPlanDocument Plan(params GithubForwardOp[] ops) => new(
        SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourcePbiUpdateRun: "src", Repository: "o/n",
        Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    [Fact]
    public void BuildSession_erzeugt_ein_Item_pro_Op_mit_Default_apply()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("CREATE_ISSUE", "PBI-2")));
        Assert.Equal(2, session.Items.Count);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("apply", FieldOf(i, GithubForwardReviewAdapter.FieldDecision)));
        Assert.All(session.Items, i => Assert.True(i.Resolved));
    }

    [Fact]
    public void Apply_traegt_Entscheidungen_und_Reason_in_die_Datei()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("LINK", "PBI-2")));
        var op1 = session.Items[1];
        op1.FieldValues.RemoveAll(f => f.FieldKey == GithubForwardReviewAdapter.FieldDecision);
        op1.FieldValues.Add(new ReviewFieldValue(GithubForwardReviewAdapter.FieldDecision, "skip"));
        op1.FieldValues.RemoveAll(f => f.FieldKey == GithubForwardReviewAdapter.FieldReason);
        op1.FieldValues.Add(new ReviewFieldValue(GithubForwardReviewAdapter.FieldReason, "Duplikat"));

        var file = GithubForwardReviewAdapter.Apply("r1", session);
        Assert.Equal("r1", file.RunId);
        Assert.Equal(2, file.Decisions.Count);
        Assert.Equal("apply", file.Decisions[0].Decision);
        Assert.Equal("skip", file.Decisions[1].Decision);
        Assert.Equal("Duplikat", file.Decisions[1].Reason);
        Assert.Null(file.Decisions[0].Reason);
    }

    [Fact]
    public void MergeExistingDecisions_stellt_gespeicherte_Entscheidungen_wieder_her()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1"), Op("LINK", "PBI-2")));
        var existing = new GithubForwardDecisionsFile("r1", "human (review-ui)",
            [new GithubForwardDecision("op-0", "skip", "war falsch"), new GithubForwardDecision("op-99", "apply", null)]);

        GithubForwardReviewAdapter.MergeExistingDecisions(session, existing);

        Assert.Equal("skip", FieldOf(session.Items[0], GithubForwardReviewAdapter.FieldDecision));
        Assert.Equal("war falsch", FieldOf(session.Items[0], GithubForwardReviewAdapter.FieldReason));
        Assert.True(session.Items[0].Resolved);
        Assert.Equal("apply", FieldOf(session.Items[1], GithubForwardReviewAdapter.FieldDecision)); // op-99 ignoriert
    }

    [Fact]
    public void Leere_Entscheidung_gilt_als_nicht_resolved()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r1", Plan(Op("LINK", "PBI-1")));
        GithubForwardReviewAdapter.MergeExistingDecisions(session, new GithubForwardDecisionsFile("r1", "x",
            [new GithubForwardDecision("op-0", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }
}
