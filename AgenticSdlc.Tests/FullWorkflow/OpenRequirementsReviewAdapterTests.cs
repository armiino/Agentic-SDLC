using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/OpenRequirements): die STRENGSTE Resolved-Policy der Kette —
// Entscheidung + ClarificationType + Priority + IssueTitle + Question + Reason muessen ALLE gesetzt sein.
// ItemId = RequirementId; Default-Scope zeigt nur Requirements OHNE IssuePlanning-Abdeckung.
public sealed class OpenRequirementsReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static OperationalizationRequirementAudit Req(string id = "CAN-REQ-1", bool inPlanning = false, string? readiness = null) => new(
        RequirementId: id, Title: $"Titel {id}", Status: "active", Readiness: readiness,
        InIssuePlanningInput: inPlanning, IssuePlanIds: [], ClarificationPlanIds: [], GithubActionIds: [],
        GithubOperations: [], DryRunOperations: [], TargetIssueNumbers: [],
        OperationalizationStatus: "uncovered", FindingCodes: []);

    private static OperationalizationAuditDocument Audit(params OperationalizationRequirementAudit[] reqs) => new(
        SchemaVersion: 1, CreatedUtc: T, BaselineId: "b1", ProjectId: "p1",
        SourcePaths: new OperationalizationAuditSourcePaths(null, null, "in.json", "plan.json", "action.json", "dry.json", null),
        Summary: new OperationalizationAuditSummary(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            new Dictionary<string, int>(), 0, 0, new Dictionary<string, int>(), false),
        Requirements: reqs, Findings: []);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void Default_Scope_zeigt_nur_Requirements_ohne_Planning_Abdeckung()
    {
        var audit = Audit(Req("CAN-REQ-1", inPlanning: false), Req("CAN-REQ-2", inPlanning: true));
        var scoped = OpenRequirementsReviewAdapter.BuildSession("r1", audit, null, "open");
        Assert.Equal(["CAN-REQ-1"], scoped.Items.Select(i => i.ItemId));

        var all = OpenRequirementsReviewAdapter.BuildSession("r1", audit, null, "all");
        Assert.Equal(2, all.Items.Count);
    }

    [Fact]
    public void Resolved_verlangt_ALLE_sechs_Felder()
    {
        var session = OpenRequirementsReviewAdapter.BuildSession("r1", Audit(Req()), null, "all");
        var it = session.Items[0];
        Set(it, OpenRequirementsReviewAdapter.FieldDecision, "create_clarification_issue");
        Set(it, OpenRequirementsReviewAdapter.FieldClarificationType, "decision");
        Set(it, OpenRequirementsReviewAdapter.FieldPriority, "high");
        Set(it, OpenRequirementsReviewAdapter.FieldIssueTitle, "Klärung X");
        Set(it, OpenRequirementsReviewAdapter.FieldQuestion, "Wie genau?");
        Set(it, OpenRequirementsReviewAdapter.FieldReason, "");
        Assert.False(OpenRequirementsReviewAdapter.Resolved(it)); // Reason fehlt noch

        Set(it, OpenRequirementsReviewAdapter.FieldReason, "blockiert MVP");
        Assert.True(OpenRequirementsReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var audit = Audit(Req("CAN-REQ-1"));
        var session = OpenRequirementsReviewAdapter.BuildSession("r1", audit, null, "all");
        var it = session.Items[0];
        Set(it, OpenRequirementsReviewAdapter.FieldDecision, "defer");
        Set(it, OpenRequirementsReviewAdapter.FieldClarificationType, "scope");
        Set(it, OpenRequirementsReviewAdapter.FieldPriority, "low");
        Set(it, OpenRequirementsReviewAdapter.FieldIssueTitle, "Später");
        Set(it, OpenRequirementsReviewAdapter.FieldQuestion, "Brauchen wir das?");
        Set(it, OpenRequirementsReviewAdapter.FieldReason, "nicht MVP");

        var file = OpenRequirementsReviewAdapter.Apply("r1", session);
        Assert.Single(file.Decisions);
        Assert.Equal("defer", file.Decisions[0].Decision);
        Assert.Equal("scope", file.Decisions[0].ClarificationType);

        var fresh = OpenRequirementsReviewAdapter.BuildSession("r1", audit, null, "all");
        OpenRequirementsReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("defer", FieldOf(fresh.Items[0], OpenRequirementsReviewAdapter.FieldDecision));
        Assert.True(OpenRequirementsReviewAdapter.Resolved(fresh.Items[0]));
    }
}
