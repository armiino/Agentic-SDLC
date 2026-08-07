using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

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
    // v2: identityKey + history (Core/Ingestion). v3: feature/pbi-Payloads (Inc 1c-1). v4: §5 Statusmodell — typisierte
    // Status-Achsen additiv am Item (validity/progress/blocker/confirmedBy/confirmedInRun/decision). Abwaertskompatibel:
    // aeltere Dateien deserialisieren (fehlende Felder -> null). S7/Option A: die typisierten Achsen SIND die Quelle;
    // `status` ist eine berechnete get-only-Projektion daraus (kein gespeichertes Feld mehr).
    public const int CurrentSchemaVersion = 4;
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
    // §5-S7 (Option A): `Status` ist KEIN gespeichertes Feld mehr — s. berechnete Projektion unten (Record-Body).
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
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata,
    // Core-Erweiterung (SchemaVersion 2, abwaertskompatibel): identityKey = deterministischer Such-ANKER
    // (kein Identitaets-Urteil); history = fruehere Fassungen (Ingestion macht den Core versioniert).
    [property: JsonPropertyName("identityKey")] string? IdentityKey = null,
    [property: JsonPropertyName("history")] IReadOnlyList<ProjectStateItemVersion>? History = null,
    // Core-Erweiterung (SchemaVersion 3, Inc 1c-1): typisierte Backlog-Payloads. Nur das zu itemType passende
    // Feld ist gesetzt (feature -> feature, pbi -> pbi); requirements nutzen die flachen Felder.
    [property: JsonPropertyName("feature")] FeaturePayload? Feature = null,
    [property: JsonPropertyName("pbi")] PbiPayload? Pbi = null,
    // R-11 A2: arch-Konsum-Rollen (WhenWritingNull: der Bestand ändert sich erst, wenn klassifiziert wird).
    [property: JsonPropertyName("architecture"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ArchitecturePayload? Architecture = null,
    // Core-Erweiterung (SchemaVersion 4, §5 Statusmodell-Refactor): der Item-Lifecycle-Status als getrennte, typisierte
    // Achsen (siehe CoreStatus.cs), die das eine rohe `status`-Feld ABGELÖST haben (S7/Option A). Diese Achsen SIND die
    // Quelle; `Status` ist eine berechnete Projektion daraus (get-only). Nullable nur zur Deserialisierungs-Toleranz — ein
    // valides Item trägt sie IMMER (fehlende Achsen = Konstruktions-Fehler → ReadStatus wirft). WhenWritingNull hält die
    // JSON schlank; Enums serialisieren als String (JsonConverter an der Enum-Definition).
    [property: JsonPropertyName("validity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Validity? Validity = null,
    [property: JsonPropertyName("progress"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Progress? Progress = null,
    [property: JsonPropertyName("blocker"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Blocker? Blocker = null,
    [property: JsonPropertyName("confirmedBy"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Confirmation? ConfirmedBy = null,
    [property: JsonPropertyName("confirmedInRun"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ConfirmedInRun = null,
    [property: JsonPropertyName("decision"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DecisionState? Decision = null)
{
    /// <summary>§5-S7 (Option A): der Legacy-Status-String als BERECHNETE get-only-Projektion aus den typisierten Achsen —
    /// KEINE gespeicherte Wahrheit mehr, nur noch Ausgabeformat für Grenzen (History/GitHub/LLM). Wird in die JSON
    /// geschrieben (`status`), beim Deserialisieren aber IGNORIERT (kein Setter) → die Achsen sind die einzige Quelle.</summary>
    [JsonInclude, JsonPropertyName("status")]
    public string Status => this.ReadStatus().ToLegacyString();
}

/// <summary>Fruehere Fassung eines Items (Ingestion-Historie). Die aktuelle Fassung steht im Item selbst.</summary>
public sealed record ProjectStateItemVersion(
    [property: JsonPropertyName("versionId")] int VersionId,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("sourceRunId")] string? SourceRunId,
    [property: JsonPropertyName("sourceClaimIds")] IReadOnlyList<string> SourceClaimIds,
    [property: JsonPropertyName("recordedUtc")] DateTime RecordedUtc,
    [property: JsonPropertyName("note")] string? Note);

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
