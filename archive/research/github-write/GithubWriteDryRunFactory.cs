namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class GithubWriteDryRunFactory
{
    public static GithubWriteDryRunDocument Create(GithubActionPlanDocument acceptedPlan)
    {
        var operations = acceptedPlan.Actions
            .OrderBy(a => a.ActionId, StringComparer.Ordinal)
            .Select(ToOperation)
            .ToArray();
        var summary = new GithubWriteDryRunSummary(
            Actions: operations.Length,
            Create: Count(operations, "CREATE"),
            Update: Count(operations, "UPDATE"),
            Link: Count(operations, "LINK"),
            Reopen: Count(operations, "REOPEN"),
            NoChange: Count(operations, "NO_CHANGE"),
            NeedsReview: Count(operations, "NEEDS_REVIEW"),
            Blocked: operations.Count(o => o.Blocked));

        return new GithubWriteDryRunDocument(
            SchemaVersion: GithubWriteDryRunDocument.CurrentSchemaVersion,
            SourcePlanId: acceptedPlan.PlanId,
            Repository: acceptedPlan.Repository,
            CreatedUtc: DateTime.UtcNow,
            ReadyForExecute: summary.Blocked == 0,
            Operations: operations,
            Summary: summary);
    }

    private static GithubWriteDryRunOperation ToOperation(GithubActionPlanItem action)
    {
        var operation = action.Operation.ToUpperInvariant();
        var operationBlockReason = operation switch
        {
            "CREATE" => null,
            "UPDATE" => action.TargetIssueNumber is null ? "UPDATE braucht targetIssueNumber." : null,
            "REOPEN" => action.TargetIssueNumber is null ? "REOPEN braucht targetIssueNumber." : null,
            "LINK" => action.TargetIssueNumber is null ? "LINK braucht targetIssueNumber." : null,
            "NO_CHANGE" => null,
            "NEEDS_REVIEW" => "NEEDS_REVIEW darf nicht automatisch nach GitHub geschrieben werden.",
            _ => $"Unbekannte Operation: {action.Operation}"
        };
        var qualityBlockReason = GithubIssueQualityRules.Check(action).Count == 0
            ? null
            : "IssueQualityGate: " + string.Join(" ", GithubIssueQualityRules.Check(action).Select(f => $"{f.Code}: {f.Message}"));
        var blockReason = JoinReasons(operationBlockReason, qualityBlockReason);
        var wouldCall = operation switch
        {
            "CREATE" => "POST /repos/{owner}/{repo}/issues",
            "UPDATE" => $"PATCH /repos/{{owner}}/{{repo}}/issues/{action.TargetIssueNumber}",
            "REOPEN" => $"PATCH /repos/{{owner}}/{{repo}}/issues/{action.TargetIssueNumber} state=open",
            "LINK" => "NO_GITHUB_WRITE: persistiere lokales Mapping",
            "NO_CHANGE" => "NO_OP",
            _ => "BLOCKED"
        };

        return Op(action, wouldCall, blockReason is not null, blockReason);
    }

    private static GithubWriteDryRunOperation Op(GithubActionPlanItem action, string wouldCall, bool blocked, string? blockReason)
        => new(
            ActionId: action.ActionId,
            Operation: action.Operation.ToUpperInvariant(),
            IssuePlanId: action.IssuePlanId,
            TargetIssueNumber: action.TargetIssueNumber,
            WouldCall: wouldCall,
            Title: action.Title,
            Body: action.Body,
            Labels: action.Labels,
            Blocked: blocked,
            BlockReason: blockReason,
            SourceRequirementIds: action.SourceRequirementIds);

    private static int Count(IReadOnlyList<GithubWriteDryRunOperation> operations, string operation)
        => operations.Count(o => o.Operation.Equals(operation, StringComparison.OrdinalIgnoreCase));

    private static string? JoinReasons(params string?[] reasons)
    {
        var values = reasons.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!).ToArray();
        return values.Length == 0 ? null : string.Join(" ", values);
    }
}
