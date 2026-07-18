using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record ClarificationPlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceClarificationPlanningInputPath")] string SourceClarificationPlanningInputPath,
    [property: JsonPropertyName("items")] IReadOnlyList<ClarificationPlanItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record ClarificationPlanItem(
    [property: JsonPropertyName("clarificationPlanId")] string ClarificationPlanId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("sourceClarificationIds")] IReadOnlyList<string> SourceClarificationIds,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("acceptanceCriteria")] IReadOnlyList<string> AcceptanceCriteria,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("priority")] string Priority,
    [property: JsonPropertyName("requiresHumanReview")] bool RequiresHumanReview,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata);

public sealed record ClarificationPlanGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<ClarificationPlanGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<ClarificationPlanGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record ClarificationPlanGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("clarificationPlanId")] string? ClarificationPlanId,
    [property: JsonPropertyName("sourceClarificationIds")] IReadOnlyList<string> SourceClarificationIds,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds);

public sealed record ClarificationPlanRunSummary(
    [property: JsonPropertyName("saved")] bool Saved,
    [property: JsonPropertyName("toolCheckRounds")] int ToolCheckRounds,
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("items")] int Items,
    [property: JsonPropertyName("errors")] int Errors,
    [property: JsonPropertyName("warnings")] int Warnings);
