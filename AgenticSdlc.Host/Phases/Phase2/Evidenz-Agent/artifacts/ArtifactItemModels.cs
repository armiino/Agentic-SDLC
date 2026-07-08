using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;

/// <summary>
/// Q-a — das MINIMALE, produktive Artifact-Item-Modell (IST_Soll §6 minimal-first, §7.2 D1/D2). Die stabile,
/// PERSISTIERTE Item-Repräsentation, die der ID-Gate (I-a) füllt, der strukturierte Output (E-e, <c>artifact.json</c>)
/// serialisiert und die spätere Derivation- (I-b) sowie Issue-Schicht (create/comment) konsumiert.
/// </summary>
/// <remarks>
/// Bewusst NICHT der Vollvertrag: <c>assertionKind</c> (Vollvokabular), <c>derivationType</c>, Operation-Contract
/// (CreateInitial|Update|…) und Read/Write-Registry kommen erst, wenn ein echter Lauf sie verlangt (minimal-first).
/// Getrennt vom Spike-Modell <c>GeneratedArtifactClaim</c> (Evaluation/PerItem) gehalten, damit die Nachweis-/Eval-
/// Runs unangetastet bleiben — dies ist der produktive Pfad.
///
/// ZWEI Identitätsebenen (Reviewer-Präzisierung, s. issueOrchesterPlans-smaller-idea.md §6b):
/// <list type="bullet">
///   <item><description><b>ArtifactId</b> (Arbeitsobjekt, z. B. <c>REQ-0012</c>) — welches fachliche Objekt; wird
///     später der Anker für DAS GitHub-Issue (Marker <c>artifactId=…</c>).</description></item>
///   <item><description><b>ItemId</b> (Aussage, z. B. <c>REQ-0012-R01</c>) — welche konkrete Zeile; Basis für
///     Provenienz UND Idempotenz (Item schon an ein Issue angewendet → skip).</description></item>
/// </list>
/// Provenienz-Pfad ist EIN Strang: <c>ArtifactItem → sourceClaimIds → Ledger-Evidence</c> (Extraktion) bzw.
/// <c>ArtifactItem → sourceArtifactItemIds → Upstream-Item</c> (Inferenz). Kein paralleler zweiter Mapping-Pfad.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArtifactOrigin
{
    /// <summary>Aus dem Ledger extrahiert (closed-world, verankert an sourceClaimIds).</summary>
    Extracted,
    /// <summary>Aus Upstream-Artefakt-Items abgeleitet (open-world, verankert an sourceArtifactItemIds).</summary>
    Derived
}

/// <summary>Eine einzelne, stabil identifizierte Artefakt-Aussage. <c>ItemId</c> ist persistent (nicht run-lokal),
/// damit ein späterer Run/Issue-Layer sie referenzieren kann.</summary>
public sealed record ArtifactItem(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("origin")] ArtifactOrigin Origin,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceClaimIds")] IReadOnlyList<string> SourceClaimIds,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds);

/// <summary>Reproduktions-/Audit-Metadaten des erzeugenden Laufs (koppelt an das run-config-Snapshot-System).</summary>
public sealed record ProducerMetadata(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("promptVersion")] string? PromptVersion);

/// <summary>Der strukturierte Artefakt-Container (E-e, <c>artifact.json</c>): ein Arbeitsobjekt (<c>ArtifactId</c>)
/// mit seinen Items. <c>Version</c> = 1 initial; ein späterer Update-Modus erhöht sie (Operation-Contract = Future
/// Work). <c>Stage</c> trennt Baseline von Ableitung.</summary>
public sealed record ArtifactDocument(
    [property: JsonPropertyName("artifactId")] string ArtifactId,
    [property: JsonPropertyName("artifactType")] string ArtifactType,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("stage")] string Stage,
    [property: JsonPropertyName("producer")] ProducerMetadata Producer,
    [property: JsonPropertyName("items")] IReadOnlyList<ArtifactItem> Items)
{
    /// <summary>Kanonische Stages (kein Enum, um additiv erweiterbar zu bleiben).</summary>
    public const string StageEvidenceBaseline = "evidence_baseline";
    public const string StageDerivation = "derivation";
}
