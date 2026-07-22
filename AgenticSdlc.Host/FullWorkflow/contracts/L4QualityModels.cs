using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record L4QualityReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("summary")] L4QualitySummary Summary,
    [property: JsonPropertyName("findings")] IReadOnlyList<L4QualityFinding> Findings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record L4QualitySummary(
    [property: JsonPropertyName("requirements")] int Requirements,
    [property: JsonPropertyName("openDecisions")] int OpenDecisions,
    [property: JsonPropertyName("traceLinks")] int TraceLinks,
    [property: JsonPropertyName("errors")] int Errors,
    [property: JsonPropertyName("warnings")] int Warnings,
    [property: JsonPropertyName("infos")] int Infos,
    [property: JsonPropertyName("ready")] int Ready,
    [property: JsonPropertyName("needsBreakdown")] int NeedsBreakdown,
    [property: JsonPropertyName("needsDecision")] int NeedsDecision,
    [property: JsonPropertyName("duplicateRisk")] int DuplicateRisk);

public sealed record L4QualityFinding(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("classification")] string Classification,
    [property: JsonPropertyName("requirementId")] string? RequirementId,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("evidence")] IReadOnlyDictionary<string, string> Evidence);
