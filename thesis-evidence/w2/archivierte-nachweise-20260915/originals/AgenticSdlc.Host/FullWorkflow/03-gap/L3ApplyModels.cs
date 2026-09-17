using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

// ── L3 Apply-Phase (Workflow 2, §5.3) — Datenverträge ────────────────────────────────────────────────────────────
// Deterministisch: der Mensch hat im Human-Review-Paket entschieden (accept/edit/reject); die Anwendung ist auditierbar.
// Provenienz-Kette (§7): promoted item → Human Decision → Candidate → Agentenlauf. Der NEEDS_REVISION→Reflect-Zweig ist
// noch nicht Teil von v1.

/// <summary>Eingabe: EINE menschliche Entscheidung zu einem Kandidaten.</summary>
public sealed record HumanDecision(
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("decision")] string Decision,            // accept | edit | reject
    [property: JsonPropertyName("editedText")] string? EditedText,       // nur bei edit: menschliche Fassung (autoritativ)
    [property: JsonPropertyName("finalAnchorIds")] IReadOnlyList<string>? FinalAnchorIds,  // optional: vom Menschen gesetzte Anker
    [property: JsonPropertyName("reason")] string? Reason);

/// <summary>Eingabedatei mit allen menschlichen Entscheidungen eines Review-Durchgangs.</summary>
public sealed record HumanDecisionsFile(
    [property: JsonPropertyName("runId")] string? RunId,
    [property: JsonPropertyName("reviewer")] string? Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<HumanDecision> Decisions);

/// <summary>Auditierbarer Entscheidungs-Datensatz (§7) — die neue autoritative Provenienzquelle.</summary>
public sealed record L3DecisionRecord(
    [property: JsonPropertyName("decisionId")] string DecisionId,
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedText")] string? EditedText,
    [property: JsonPropertyName("finalAnchorIds")] IReadOnlyList<string> FinalAnchorIds,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("resultingItemId")] string? ResultingItemId);

/// <summary>Promoviertes Projektartefakt mit stabiler ID + Provenienz. Eigenes Modell (nicht ArtifactItem), weil der
/// Origin „human-akzeptiert (open-world/verankert)" außerhalb des ArtifactOrigin-Enums liegt (ehrliche Herkunft).</summary>
public sealed record L3PromotedItem(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("origin")] string Origin,               // HUMAN_ACCEPTED_OPEN_WORLD | HUMAN_ACCEPTED_ANCHORED
    [property: JsonPropertyName("status")] string Status,               // ACCEPTED
    [property: JsonPropertyName("sourceDecisionId")] string SourceDecisionId,
    [property: JsonPropertyName("sourceCandidateId")] string SourceCandidateId,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds,
    [property: JsonPropertyName("version")] int Version);

/// <summary>Abgelehnter Kandidat — bleibt auditierbar, downstream ausgeschlossen.</summary>
public sealed record L3RejectedItem(
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("class")] L3Class Class,
    [property: JsonPropertyName("reason")] string? Reason);

/// <summary>Kandidaten-ID → finale Projekt-ID (referenzierbar für Issues; die Kandidaten-ID bleibt auditierbar).</summary>
public sealed record L3PromotionMapping(
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("promotedItemId")] string PromotedItemId);
