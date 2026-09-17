using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record IssuePlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceIssuePlanningInputPath")] string SourceIssuePlanningInputPath,
    [property: JsonPropertyName("items")] IReadOnlyList<IssuePlanItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record IssuePlanItem(
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("acceptanceCriteria")] IReadOnlyList<string> AcceptanceCriteria,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string> Dependencies,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("requiresHumanReview")] bool RequiresHumanReview,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata)
{
    [property: JsonPropertyName("knownContext")]
    public IReadOnlyList<string> KnownContext { get; init; } = [];

    [property: JsonPropertyName("implementationHints")]
    public IReadOnlyList<string> ImplementationHints { get; init; } = [];

    [property: JsonPropertyName("openQuestions")]
    public IReadOnlyList<string> OpenQuestions { get; init; } = [];

    [property: JsonPropertyName("relatedClarificationIds")]
    public IReadOnlyList<string> RelatedClarificationIds { get; init; } = [];

    [property: JsonPropertyName("readiness")]
    public string? Readiness { get; init; }
}

public sealed record IssuePlanGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<IssuePlanGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<IssuePlanGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record IssuePlanGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("issuePlanId")] string? IssuePlanId,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds);
