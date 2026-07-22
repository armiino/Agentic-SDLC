using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record RequirementsReadinessReport(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("summary")] RequirementsReadinessSummary Summary,
    [property: JsonPropertyName("items")] IReadOnlyList<RequirementReadinessItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record RequirementsReadinessSummary(
    [property: JsonPropertyName("requirements")] int Requirements,
    [property: JsonPropertyName("readyForIssuePlanning")] int ReadyForIssuePlanning,
    [property: JsonPropertyName("needsDecision")] int NeedsDecision,
    [property: JsonPropertyName("needsBreakdown")] int NeedsBreakdown,
    [property: JsonPropertyName("deferredOrOptional")] int DeferredOrOptional,
    [property: JsonPropertyName("blockedByTraceability")] int BlockedByTraceability);

public sealed record RequirementReadinessItem(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("readiness")] string Readiness,
    [property: JsonPropertyName("issuePlanningAllowed")] bool IssuePlanningAllowed,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("l4OperationId")] string? L4OperationId,
    [property: JsonPropertyName("l4Operation")] string? L4Operation,
    [property: JsonPropertyName("reasons")] IReadOnlyList<RequirementReadinessReason> Reasons,
    [property: JsonPropertyName("qualityFindingCodes")] IReadOnlyList<string> QualityFindingCodes,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record RequirementReadinessReason(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message);

public sealed record IssuePlanningInput(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("items")] IReadOnlyList<IssuePlanningInputItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record IssuePlanningInputItem(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("originSummary")] string OriginSummary,
    [property: JsonPropertyName("provenance")] L4RequirementProvenance? Provenance);
