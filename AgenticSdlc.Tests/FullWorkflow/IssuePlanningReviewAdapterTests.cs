using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/IssuePlanning). Stufen-Besonderheiten, die hier festgeschrieben werden:
// (1) Resolved verlangt Entscheidung {accept|edit|reject|revise} UND eine nicht-leere Begruendung.
// (2) ItemId = IssuePlanId. (3) scope "needs-human" filtert; edit transportiert das editierte Item als JSON.
public sealed class IssuePlanningReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static IssuePlanItem Item(string id = "IP-1", string op = "CREATE", bool needsHuman = false) => new(
        IssuePlanId: id, Operation: op, Title: $"Titel {id}", Description: "Beschreibung",
        SourceRequirementIds: ["REQ-1"], AcceptanceCriteria: ["AC-1"], Labels: [], Dependencies: [],
        Rationale: "weil", RequiresHumanReview: needsHuman, Metadata: new Dictionary<string, object?>());

    private static IssuePlanDocument Plan(params IssuePlanItem[] items)
        => new(1, "plan-1", "p1", "b1", T, "input.json", items);

    private static IssuePlanningInput Input() => new(1, "b1", "p1", T, []);

    private static IssuePlanGateReport Gate() => new(true, "pass", [], [], new Dictionary<string, object>());

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_scope_all_nimmt_alles_needs_human_filtert()
    {
        var plan = Plan(Item("IP-1", "CREATE"), Item("IP-2", "NEEDS_REVIEW"), Item("IP-3", "CREATE", needsHuman: true));
        var all = IssuePlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        Assert.Equal(["IP-1", "IP-2", "IP-3"], all.Items.Select(i => i.ItemId));

        var needsHuman = IssuePlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "needs-human");
        Assert.Equal(["IP-2", "IP-3"], needsHuman.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Resolved_verlangt_Entscheidung_UND_Begruendung()
    {
        var session = IssuePlanningReviewAdapter.BuildSession("r1", Input(), Plan(Item()), Gate(), "all");
        var it = session.Items[0];
        Set(it, IssuePlanningReviewAdapter.FieldDecision, "accept");
        Set(it, IssuePlanningReviewAdapter.FieldReason, "");
        Assert.False(IssuePlanningReviewAdapter.Resolved(it));

        Set(it, IssuePlanningReviewAdapter.FieldReason, "passt fachlich");
        Assert.True(IssuePlanningReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var plan = Plan(Item("IP-1"), Item("IP-2"));
        var session = IssuePlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        Set(session.Items[0], IssuePlanningReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], IssuePlanningReviewAdapter.FieldReason, "ok");
        Set(session.Items[1], IssuePlanningReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], IssuePlanningReviewAdapter.FieldReason, "Duplikat");

        var file = IssuePlanningReviewAdapter.Apply("r1", session, plan);
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));
        Assert.All(file.Decisions, d => Assert.Null(d.EditedIssuePlanItemJson)); // kein edit -> kein JSON

        var fresh = IssuePlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        IssuePlanningReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], IssuePlanningReviewAdapter.FieldDecision));
        Assert.Equal("Duplikat", FieldOf(fresh.Items[1], IssuePlanningReviewAdapter.FieldReason));
        Assert.True(IssuePlanningReviewAdapter.Resolved(fresh.Items[1]));
    }

    [Fact]
    public void Merge_mit_edit_Entscheidung_stellt_editierte_Felder_wieder_her()
    {
        var plan = Plan(Item("IP-1"));
        var edited = Item("IP-1") with { Title = "Neuer Titel" };
        var file = new IssuePlanningHumanDecisionsFile("r1", "x",
            [new IssuePlanningHumanDecision("IP-1", "edit", JsonSerializer.Serialize(edited, JsonFiles.Json), "editiert")]);

        var session = IssuePlanningReviewAdapter.BuildSession("r1", Input(), plan, Gate(), "all");
        IssuePlanningReviewAdapter.MergeExistingDecisions(session, file);

        Assert.Equal("edit", FieldOf(session.Items[0], IssuePlanningReviewAdapter.FieldDecision));
        Assert.Equal("Neuer Titel", FieldOf(session.Items[0], IssuePlanningReviewAdapter.FieldEditTitle));
    }
}
