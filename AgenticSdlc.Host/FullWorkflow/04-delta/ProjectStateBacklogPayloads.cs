using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

// Typisierte Payloads fuer Backlog-Entitaeten im Core (Increment 1c-1). ProjectStateItem bleibt die CoreItem-
// Basis; nur das zu itemType passende Payload ist gesetzt (feature -> FeaturePayload, pbi -> PbiPayload).
// Requirements nutzen weiter die flachen Item-Felder. DB-later: eigene Tabellen join by itemId.

public sealed record FeaturePayload(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("coreRequirementIds")] IReadOnlyList<string> CoreRequirementIds,
    [property: JsonPropertyName("crossCuttingRequirementIds")] IReadOnlyList<string> CrossCuttingRequirementIds);

public sealed record PbiPayload(
    [property: JsonPropertyName("goal")] string? Goal,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("acceptanceCriteria")] IReadOnlyList<string> AcceptanceCriteria,
    [property: JsonPropertyName("linkedRequirementIds")] IReadOnlyList<string> LinkedRequirementIds,
    // Verweise auf decision-Entitaeten (KEINE zweite Decision-Wahrheit im Payload). Beim Seed leer;
    // Promotion der alten PBI-openDecisions zu echten decision-Items ist ein spaeterer Schritt.
    [property: JsonPropertyName("openDecisionRefs")] IReadOnlyList<string> OpenDecisionRefs,
    [property: JsonPropertyName("priorityRank")] int? PriorityRank,
    [property: JsonPropertyName("readiness")] string? Readiness,
    [property: JsonPropertyName("mvp")] string? Mvp,
    [property: JsonPropertyName("trace")] PbiTrace? Trace);

public sealed record PbiTrace(
    [property: JsonPropertyName("canonicalRequirementIds")] IReadOnlyList<string> CanonicalRequirementIds,
    [property: JsonPropertyName("l1Req")] IReadOnlyList<string> L1Req,
    [property: JsonPropertyName("l3")] IReadOnlyList<string> L3,
    [property: JsonPropertyName("claims")] IReadOnlyList<string> Claims);

/// <summary>R-11 A2 (05.08.) — das typisierte arch-Payload (D-10): die Konsum-ROLLEN eines architecture-Items
/// (T2.14: 1..N, komponierend; Wire-Werte englisch: constraint|work|design) + Begründung des Klassifikators.
/// A5 ergänzt später ADR-Felder. KEINE zweite Wahrheit — reine Konsum-Steuerung am Item.</summary>
public sealed record ArchitecturePayload(
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName("rationale")] string? Rationale,
    // A5 (06.08., E-3): die ADR-Projektion der design-Rolle — typisiert statt Metadata (§5-Linie).
    // AdrId "ADR-0007" (Nummer aus dem CORE, nie aus Dateien) · AdrStatus proposed|accepted|deprecated|superseded.
    // Optional: Alt-Cores/Alt-Pläne bleiben lesbar; null = noch kein ADR projiziert.
    [property: JsonPropertyName("adrId")] string? AdrId = null,
    [property: JsonPropertyName("adrStatus")] string? AdrStatus = null);

