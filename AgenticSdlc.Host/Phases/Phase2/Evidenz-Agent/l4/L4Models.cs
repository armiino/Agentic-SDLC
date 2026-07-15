using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record CanonicalRequirementsBaseline(
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceProjectStatePath")] string SourceProjectStatePath,
    [property: JsonPropertyName("requirements")] IReadOnlyList<CanonicalRequirement> Requirements,
    [property: JsonPropertyName("openDecisions")] IReadOnlyList<CanonicalOpenDecision> OpenDecisions,
    [property: JsonPropertyName("traceLinks")] IReadOnlyList<CanonicalTraceLink> TraceLinks)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record CanonicalRequirement(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("originSummary")] string OriginSummary,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record CanonicalOpenDecision(
    [property: JsonPropertyName("decisionId")] string DecisionId,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceRequirementId")] string? SourceRequirementId,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("reason")] string? Reason);

public sealed record CanonicalTraceLink(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("sourceType")] string SourceType,
    [property: JsonPropertyName("sourceId")] string SourceId,
    [property: JsonPropertyName("relation")] string Relation,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record ConsolidationPlan(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceProjectStatePath")] string SourceProjectStatePath,
    [property: JsonPropertyName("operations")] IReadOnlyList<ConsolidationOperation> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record ConsolidationOperation(
    [property: JsonPropertyName("operationId")] string OperationId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("targets")] IReadOnlyList<ConsolidationTargetRequirement> Targets,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("requiresHumanReview")] bool RequiresHumanReview,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata);

public sealed record ConsolidationTargetRequirement(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata);

public sealed record ConsolidationGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<ConsolidationGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<ConsolidationGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record ConsolidationGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("operationId")] string? OperationId,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds);

public sealed record L4ProvenanceMap(
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("requirements")] IReadOnlyList<L4RequirementProvenance> Requirements);

public sealed record L4RequirementProvenance(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("l4OperationId")] string? L4OperationId,
    [property: JsonPropertyName("l4Operation")] string? L4Operation,
    [property: JsonPropertyName("sourceProjectItems")] IReadOnlyList<L4ProjectItemTrace> SourceProjectItems,
    [property: JsonPropertyName("ledgerClaimIds")] IReadOnlyList<string> LedgerClaimIds,
    [property: JsonPropertyName("l3CandidateIds")] IReadOnlyList<string> L3CandidateIds,
    [property: JsonPropertyName("humanDecisionIds")] IReadOnlyList<string> HumanDecisionIds,
    [property: JsonPropertyName("replacesProjectItemIds")] IReadOnlyList<string> ReplacesProjectItemIds,
    [property: JsonPropertyName("linkedClusters")] IReadOnlyList<L4LinkedClusterTrace> LinkedClusters);

public sealed record L4ProjectItemTrace(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("sourceRunId")] string? SourceRunId,
    [property: JsonPropertyName("sourceArtifactId")] string? SourceArtifactId,
    [property: JsonPropertyName("sourceArtifactType")] string? SourceArtifactType,
    [property: JsonPropertyName("sourceCandidateId")] string? SourceCandidateId,
    [property: JsonPropertyName("sourceDecisionId")] string? SourceDecisionId,
    [property: JsonPropertyName("sourceClaimIds")] IReadOnlyList<string> SourceClaimIds,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds,
    [property: JsonPropertyName("provenanceLinks")] IReadOnlyList<ProjectState.ProjectStateProvenanceLink> ProvenanceLinks);

public sealed record L4LinkedClusterTrace(
    [property: JsonPropertyName("operationId")] string OperationId,
    [property: JsonPropertyName("relation")] string Relation,
    [property: JsonPropertyName("sourceItemIds")] IReadOnlyList<string> SourceItemIds,
    [property: JsonPropertyName("rationale")] string? Rationale);
