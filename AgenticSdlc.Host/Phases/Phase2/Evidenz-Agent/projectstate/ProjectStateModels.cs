using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

/// <summary>
/// Fachlicher Projektzustand als speicherneutrale Sicht auf L1/L2/L3-Artefakte. JSON ist nur die erste Persistenzform;
/// L4, Issue Planning und spaetere Agenten sollen gegen diese Konzepte arbeiten, nicht gegen Run-Dateipfade.
/// </summary>
public sealed record ProjectStateDocument(
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sources")] IReadOnlyList<ProjectStateSource> Sources,
    [property: JsonPropertyName("items")] IReadOnlyList<ProjectStateItem> Items,
    [property: JsonPropertyName("relations")] IReadOnlyList<ProjectStateRelation> Relations,
    [property: JsonPropertyName("provenance")] IReadOnlyList<ProjectStateProvenance> Provenance,
    [property: JsonPropertyName("proposals")] IReadOnlyList<ProjectStateProposal> Proposals)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record ProjectStateSource(
    [property: JsonPropertyName("sourceId")] string SourceId,
    [property: JsonPropertyName("sourceType")] string SourceType,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("runId")] string? RunId,
    [property: JsonPropertyName("description")] string? Description);

public sealed record ProjectStateItem(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("stage")] string? Stage,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("sourceRunId")] string? SourceRunId,
    [property: JsonPropertyName("sourceArtifactId")] string? SourceArtifactId,
    [property: JsonPropertyName("sourceArtifactType")] string? SourceArtifactType,
    [property: JsonPropertyName("sourceDecisionId")] string? SourceDecisionId,
    [property: JsonPropertyName("sourceCandidateId")] string? SourceCandidateId,
    [property: JsonPropertyName("sourceClaimIds")] IReadOnlyList<string> SourceClaimIds,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record ProjectStateRelation(
    [property: JsonPropertyName("fromId")] string FromId,
    [property: JsonPropertyName("toId")] string ToId,
    [property: JsonPropertyName("relationType")] string RelationType,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record ProjectStateProvenance(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("links")] IReadOnlyList<ProjectStateProvenanceLink> Links);

public sealed record ProjectStateProvenanceLink(
    [property: JsonPropertyName("targetType")] string TargetType,
    [property: JsonPropertyName("targetId")] string TargetId,
    [property: JsonPropertyName("relation")] string Relation,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record ProjectStateProposal(
    [property: JsonPropertyName("proposalId")] string ProposalId,
    [property: JsonPropertyName("proposalType")] string ProposalType,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("sourceRunId")] string? SourceRunId,
    [property: JsonPropertyName("payloadPath")] string? PayloadPath,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record ProjectStateItemQuery(
    IReadOnlySet<string>? ItemTypes = null,
    IReadOnlySet<string>? Statuses = null,
    IReadOnlySet<string>? Origins = null);
