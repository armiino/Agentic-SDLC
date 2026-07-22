using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (E4-Stufe): Reverse-Vorschlaege muessen Entscheidungen verlustfrei tragen.
public sealed class GithubReverseReviewAdapterTests
{
    private static GithubReverseOp Op(string kind = "PBI_DONE", string pbi = "PBI-1") => new(
        Kind: kind, PbiId: pbi, IssueNumber: 60, CurrentPbiStatus: "active",
        ProposedPbiStatus: "done", RequiresVerification: true, Rationale: "Issue closed");

    private static GithubReversePlanDocument Plan(params GithubReverseOp[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, Snapshot: null, Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    [Fact]
    public void BuildSession_erzeugt_Items_je_Op()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op("FLAG_REOPENED", "PBI-2")));
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op()));
        var it = session.Items[0];
        it.FieldValues.RemoveAll(f => f.FieldKey == GithubReverseReviewAdapter.FieldDecision);
        it.FieldValues.Add(new ReviewFieldValue(GithubReverseReviewAdapter.FieldDecision, "skip"));
        it.FieldValues.RemoveAll(f => f.FieldKey == GithubReverseReviewAdapter.FieldReason);
        it.FieldValues.Add(new ReviewFieldValue(GithubReverseReviewAdapter.FieldReason, "nicht verifiziert"));

        var file = GithubReverseReviewAdapter.Apply("r1", session);

        var fresh = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op()));
        GithubReverseReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[0], GithubReverseReviewAdapter.FieldDecision));
        Assert.Equal("nicht verifiziert", FieldOf(fresh.Items[0], GithubReverseReviewAdapter.FieldReason));
        Assert.True(fresh.Items[0].Resolved);
    }

    [Fact]
    public void Leere_Entscheidung_ist_nicht_resolved()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op()));
        GithubReverseReviewAdapter.MergeExistingDecisions(session,
            new GithubReverseDecisionsFile("r1", "x", [new GithubReverseDecision("op-0", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }
}
