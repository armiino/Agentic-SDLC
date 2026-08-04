using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/Consolidation): gleiche strenge Policy wie IssuePlanning
// (Decision {accept|edit|reject|revise} + Begruendung); ItemId = OperationId; scope "changes" filtert KEEP raus.
public sealed class L4ReviewAdapterTests
{
    private static readonly ProjectStateDocument EmptyState =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static ConsolidationOperation Op(string id = "OP-1", string op = "KEEP") => new(
        OperationId: id, Operation: op, SourceItemIds: ["ITEM-1"],
        Targets: [new ConsolidationTargetRequirement("CAN-REQ-1", "Titel", "Text", "active", new Dictionary<string, object?>())],
        Rationale: "weil", RequiresHumanReview: false, Metadata: new Dictionary<string, object?>());

    private static ConsolidationPlan Plan(params ConsolidationOperation[] ops)
        => new(1, "p1", DateTime.UnixEpoch, "state.json", ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_scope_changes_filtert_KEEP_raus()
    {
        var plan = Plan(Op("OP-1", "KEEP"), Op("OP-2", "DEPRECATE"));
        var all = L4ReviewAdapter.BuildSession("r1", plan, EmptyState, "all");
        Assert.Equal(["OP-1", "OP-2"], all.Items.Select(i => i.ItemId));

        var changes = L4ReviewAdapter.BuildSession("r1", plan, EmptyState, "changes");
        Assert.Equal(["OP-2"], changes.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Resolved_verlangt_Entscheidung_UND_Begruendung()
    {
        var session = L4ReviewAdapter.BuildSession("r1", Plan(Op()), EmptyState, "all");
        var it = session.Items[0];
        Set(it, L4ReviewAdapter.FieldDecision, "accept");
        Set(it, L4ReviewAdapter.FieldReason, "");
        Assert.False(L4ReviewAdapter.Resolved(it));
        Set(it, L4ReviewAdapter.FieldReason, "passt");
        Assert.True(L4ReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var plan = Plan(Op("OP-1"), Op("OP-2"));
        var session = L4ReviewAdapter.BuildSession("r1", plan, EmptyState, "all");
        Set(session.Items[0], L4ReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], L4ReviewAdapter.FieldReason, "ok");
        Set(session.Items[1], L4ReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], L4ReviewAdapter.FieldReason, "falsch");

        var file = L4ReviewAdapter.Apply("r1", session);
        Assert.Equal(["OP-1", "OP-2"], file.Decisions.Select(d => d.OperationId));
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));

        var fresh = L4ReviewAdapter.BuildSession("r1", plan, EmptyState, "all");
        L4ReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], L4ReviewAdapter.FieldDecision));
        Assert.True(L4ReviewAdapter.Resolved(fresh.Items[1]));
    }
}
