using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Placement-Stufe): Backlog-Aenderungs-Entscheidungen verlustfrei tragen.
public sealed class PbiUpdateReviewAdapterTests
{
    private static readonly ProjectStateDocument EmptyCore =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static PbiStateChangeOperation Op(string kind = "EXTEND_PBI", string req = "REQ-1", string? pbi = "PBI-1")
        => new(Kind: kind, RequirementId: req, PbiId: pbi, FeatureId: null,
               ReplacementRequirementId: null, OpenDecisionRef: null, Rationale: "weil");

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourceIngestionRun: "src", Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    [Fact]
    public void BuildSession_erzeugt_Items_mit_Default_apply()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op("NEW_PBI", "REQ-2", pbi: null)), EmptyCore);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("apply", FieldOf(i, PbiUpdateReviewAdapter.FieldDecision)));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        var it = session.Items[1];
        it.FieldValues.RemoveAll(f => f.FieldKey == PbiUpdateReviewAdapter.FieldDecision);
        it.FieldValues.Add(new ReviewFieldValue(PbiUpdateReviewAdapter.FieldDecision, "skip"));

        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        Assert.Equal(["apply", "skip"], file.Decisions.Select(d => d.Decision));

        var fresh = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], PbiUpdateReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[1].Resolved);
    }

    [Fact]
    public void Unbekannte_OpIds_in_der_Datei_werden_ignoriert()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(session,
            new PbiUpdateDecisionsFile("r1", "x", [new PbiUpdateDecision("op-99", "skip", null)]));
        Assert.Equal("apply", FieldOf(session.Items[0], PbiUpdateReviewAdapter.FieldDecision));
    }
}
