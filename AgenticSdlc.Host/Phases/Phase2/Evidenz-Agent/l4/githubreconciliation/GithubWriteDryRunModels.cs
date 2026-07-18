using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record GithubWriteDryRunDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("readyForExecute")] bool ReadyForExecute,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubWriteDryRunOperation> Operations,
    [property: JsonPropertyName("summary")] GithubWriteDryRunSummary Summary)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubWriteDryRunOperation(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("wouldCall")] string WouldCall,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("blocked")] bool Blocked,
    [property: JsonPropertyName("blockReason")] string? BlockReason,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds);

public sealed record GithubWriteDryRunSummary(
    [property: JsonPropertyName("actions")] int Actions,
    [property: JsonPropertyName("create")] int Create,
    [property: JsonPropertyName("update")] int Update,
    [property: JsonPropertyName("link")] int Link,
    [property: JsonPropertyName("reopen")] int Reopen,
    [property: JsonPropertyName("noChange")] int NoChange,
    [property: JsonPropertyName("needsReview")] int NeedsReview,
    [property: JsonPropertyName("blocked")] int Blocked);
