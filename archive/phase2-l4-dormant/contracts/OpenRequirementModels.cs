using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record OpenRequirementHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<OpenRequirementHumanDecision> Decisions);

public sealed record OpenRequirementHumanDecision(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("clarificationType")] string? ClarificationType,
    [property: JsonPropertyName("priority")] string? Priority,
    [property: JsonPropertyName("issueTitle")] string? IssueTitle,
    [property: JsonPropertyName("question")] string? Question,
    [property: JsonPropertyName("reason")] string? Reason);

public sealed record OpenRequirementApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourceAuditPath")] string SourceAuditPath,
    [property: JsonPropertyName("sourceOpenRequirements")] int SourceOpenRequirements,
    [property: JsonPropertyName("reviewed")] int Reviewed,
    [property: JsonPropertyName("missingDecisions")] IReadOnlyList<string> MissingDecisions,
    [property: JsonPropertyName("decisionsByType")] IReadOnlyDictionary<string, int> DecisionsByType,
    [property: JsonPropertyName("clarificationItems")] int ClarificationItems,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc);

public sealed record ClarificationPlanningInput(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceAuditPath")] string SourceAuditPath,
    [property: JsonPropertyName("items")] IReadOnlyList<ClarificationPlanningInputItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record ClarificationPlanningInputItem(
    [property: JsonPropertyName("clarificationId")] string ClarificationId,
    [property: JsonPropertyName("sourceRequirementId")] string SourceRequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("clarificationType")] string ClarificationType,
    [property: JsonPropertyName("priority")] string Priority,
    [property: JsonPropertyName("readiness")] string? Readiness,
    [property: JsonPropertyName("requirementTitle")] string RequirementTitle,
    [property: JsonPropertyName("rationale")] string? Rationale);
