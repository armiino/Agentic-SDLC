using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Tor 1): Ingestion-Entscheidungen verlustfrei tragen.
// Besonderheit dieser Stufe: ItemId = IncomingItemId (nicht op-{i}).
public sealed class IngestionReviewAdapterTests
{
    private static readonly ProjectStateDocument Empty =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static StateChangeOperation Op(string incoming = "M1-REQ-001", string kind = "NEW") => new(
        IncomingItemId: incoming, Kind: kind, Statement: "Der Nutzer kann X.",
        TargetEntityId: null, FeatureKey: null, ClaimIds: [], Rationale: "nichts Passendes im Core");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops) => new(
        SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch,
        SourceMeetingDeltaPath: "runs/manual/demo/delta.json", Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    [Fact]
    public void BuildSession_nutzt_IncomingItemId_als_ItemId()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002", "REFINE")), Empty, Empty);
        Assert.Equal(["M1-REQ-001", "M1-REQ-002"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("apply", FieldOf(i, IngestionReviewAdapter.FieldDecision)));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002")), Empty, Empty);
        var it = session.Items[1];
        it.FieldValues.RemoveAll(f => f.FieldKey == IngestionReviewAdapter.FieldDecision);
        it.FieldValues.Add(new ReviewFieldValue(IngestionReviewAdapter.FieldDecision, "skip"));

        var file = IngestionReviewAdapter.Apply("r1", session);
        Assert.Equal(["M1-REQ-001", "M1-REQ-002"], file.Decisions.Select(d => d.IncomingItemId));
        Assert.Equal(["apply", "skip"], file.Decisions.Select(d => d.Decision));

        var fresh = IngestionReviewAdapter.BuildSession("r1", Plan(Op("M1-REQ-001"), Op("M1-REQ-002")), Empty, Empty);
        IngestionReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], IngestionReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[1].Resolved);
    }

    [Fact]
    public void AcceptedFromDecisions_liefert_nur_applizierte_IncomingItemIds()
    {
        var accepted = IngestionApplyExec.AcceptedFromDecisions(
        [
            new IngestionHumanDecision("M1-REQ-001", "apply", null),
            new IngestionHumanDecision("M1-REQ-002", "skip", null)
        ]);
        Assert.Contains("M1-REQ-001", accepted);
        Assert.DoesNotContain("M1-REQ-002", accepted);
    }
}
