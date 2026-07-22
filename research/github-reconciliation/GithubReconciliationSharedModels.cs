using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record GithubReconciliationInput(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceAcceptedIssuePlanPath")] string SourceAcceptedIssuePlanPath,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("acceptedIssuePlan")] IssuePlanDocument AcceptedIssuePlan,
    [property: JsonPropertyName("existingIssues")] IReadOnlyList<GithubIssueSnapshot> ExistingIssues,
    [property: JsonPropertyName("existingMappings")] IReadOnlyList<GithubIssueMapping> ExistingMappings)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubIssueMapping(
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("issueUrl")] string? IssueUrl,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("createdFromPlanId")] string? CreatedFromPlanId,
    [property: JsonPropertyName("syncedUtc")] DateTime? SyncedUtc);

public sealed record GithubActionPlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceAcceptedIssuePlanPath")] string SourceAcceptedIssuePlanPath,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("actions")] IReadOnlyList<GithubActionPlanItem> Actions)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubActionPlanItem(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("acceptanceCriteria")] IReadOnlyList<string> AcceptanceCriteria,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("requiresHumanReview")] bool RequiresHumanReview,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata);

public sealed record GithubActionPlanGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<GithubActionPlanGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<GithubActionPlanGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record GithubActionPlanGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("actionId")] string? ActionId,
    [property: JsonPropertyName("issuePlanId")] string? IssuePlanId);
