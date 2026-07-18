using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record GithubWriteExecutionDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("repository")] string Repository,
    [property: JsonPropertyName("startedUtc")] DateTime StartedUtc,
    [property: JsonPropertyName("completedUtc")] DateTime CompletedUtc,
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubWriteExecutionOperation> Operations,
    [property: JsonPropertyName("mappings")] IReadOnlyList<GithubIssueMapping> Mappings,
    [property: JsonPropertyName("summary")] GithubWriteExecutionSummary Summary)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubWriteExecutionOperation(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("resultIssueNumber")] int? ResultIssueNumber,
    [property: JsonPropertyName("resultIssueUrl")] string? ResultIssueUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string? Message);

public sealed record GithubWriteExecutionSummary(
    [property: JsonPropertyName("actions")] int Actions,
    [property: JsonPropertyName("created")] int Created,
    [property: JsonPropertyName("updated")] int Updated,
    [property: JsonPropertyName("linked")] int Linked,
    [property: JsonPropertyName("reopened")] int Reopened,
    [property: JsonPropertyName("noChange")] int NoChange,
    [property: JsonPropertyName("skipped")] int Skipped,
    [property: JsonPropertyName("failed")] int Failed);

public sealed record GithubWriteExecuteOptions(
    string Repository,
    string Token,
    string UserAgent);

internal sealed record GithubIssueWriteResult(
    int IssueNumber,
    string? IssueUrl,
    string State,
    string Title);
