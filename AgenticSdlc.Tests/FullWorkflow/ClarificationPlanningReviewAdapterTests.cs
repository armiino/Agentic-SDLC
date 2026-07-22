using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/Clarification) — Schwester von IssuePlanning:
// Resolved braucht Entscheidung {accept|edit|reject|revise} + Begruendung; edit traegt das editierte Item als JSON.
public sealed class ClarificationPlanningReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ClarificationPlanItem Item(string id = "CP-1", string op = "CREATE_CLARIFICATION_ISSUE", bool needsHuman = false) => new(
        ClarificationPlanId: id, Operation: op, Title: $"Titel {id}", Question: "Wie genau?",
        Description: "Beschreibung", SourceClarificationIds: ["CL-1"], SourceRequirementIds: ["REQ-1"],
        AcceptanceCriteria: [], Labels: [], Priority: "high", RequiresHumanReview: needsHuman,
        Metadata: new Dictionary<string, object?>());

    private static ClarificationPlanDocument Plan(params ClarificationPlanItem[] items)
        => new(1, "plan-1", "p1", "b1", "audit", T, "input.json", items);

    private static ClarificationPlanningInput Input() => new(1, "p1", "b1", T, "audit.json", []);

    private static ClarificationPlanGateReport Gate() => new(true, "pass", [], [], new Dictionary<string, object>());

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_nutzt_ClarificationPlanId_als_ItemId()
    {
        var session = ClarificationPlanningReviewAdapter.BuildSession("r1", Input(), Plan(Item("CP-1"), Item("CP-2", "DEFER")), Gate(), "all");
        Assert.Equal(["CP-1", "CP-2"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Resolved_verlangt_Entscheidung_UND_Begruendung()
    {
        var session = ClarificationPlanningReviewAdapter.BuildSession("r1", Input(), Plan(Item()), Gate(), "all");
        var it = session.Items[0];
        Set(it, ClarificationPlanningReviewAdapter.FieldDecision, "accept");
        Set(it, ClarificationPlanningReviewAdapter.FieldReason, "");
        Assert.False(ClarificationPlanningReviewAdapter.Resolved(it));
        Set(it, ClarificationPlanningReviewAdapter.FieldReason, "fachlich noetig");
        Assert.True(ClarificationPlanningReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var plan = Plan(Item("CP-1"), Item("CP-2"));
        var session = ClarificationPlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        Set(session.Items[0], ClarificationPlanningReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], ClarificationPlanningReviewAdapter.FieldReason, "ok");
        Set(session.Items[1], ClarificationPlanningReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], ClarificationPlanningReviewAdapter.FieldReason, "unnoetig");

        var file = ClarificationPlanningReviewAdapter.Apply("r1", session, plan);
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));

        var fresh = ClarificationPlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        ClarificationPlanningReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], ClarificationPlanningReviewAdapter.FieldDecision));
        Assert.True(ClarificationPlanningReviewAdapter.Resolved(fresh.Items[1]));
    }

    [Fact]
    public void Merge_mit_edit_stellt_editierte_Felder_wieder_her()
    {
        var plan = Plan(Item("CP-1"));
        var edited = Item("CP-1") with { Question = "Neue Frage?" };
        var file = new ClarificationPlanningHumanDecisionsFile("r1", "x",
            [new ClarificationPlanningHumanDecision("CP-1", "edit", JsonSerializer.Serialize(edited, JsonFiles.Json), "editiert")]);

        var session = ClarificationPlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        ClarificationPlanningReviewAdapter.MergeExistingDecisions(session, file);
        Assert.Equal("edit", FieldOf(session.Items[0], ClarificationPlanningReviewAdapter.FieldDecision));
        Assert.Equal("Neue Frage?", FieldOf(session.Items[0], ClarificationPlanningReviewAdapter.FieldEditQuestion));
    }
}
