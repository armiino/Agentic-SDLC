using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>Erlaubte Adjudikations-Aktionen (Bauvorschlag §1.2). Per-itemType-Semantik: siehe AdjudicationCompletenessGate.</summary>
public static class AdjudicationActions
{
    public const string AcceptGap = "accept_gap";        // echte Lücke -> neuer Claim (bzw. review_required-Claim als-ist übernehmen)
    public const string MergeExisting = "merge_existing"; // gehört zu bestehendem Claim -> referenceTarget, kein neuer Claim
    public const string MarkCoveredBy = "mark_covered_by";// indirekt abgedeckt -> referenceTarget, kein neuer Claim
    public const string Reject = "reject";                // kein relevanter SDLC-Claim
    public const string ApplyRepair = "apply_repair";     // FacetIssue.suggested übernehmen -> Facette korrigiert
    public const string Defer = "defer";                  // noch keine Entscheidung -> pending, nicht final

    public static readonly string[] All = [AcceptGap, MergeExisting, MarkCoveredBy, Reject, ApplyRepair, Defer];
    public static bool IsValid(string? a) => a is not null && All.Contains(a, StringComparer.OrdinalIgnoreCase);
}

/// <summary>System-Vorschlag zu einem Item (Facet-Repair aus L3 oder Compare-Klassifikation aus U0).</summary>
public sealed record AdjudicationSuggestion(
    [property: JsonPropertyName("kind")] string Kind,                 // facet_repair | compare_classification
    [property: JsonPropertyName("facet")] string? Facet,             // bei facet_repair
    [property: JsonPropertyName("observed")] string? Observed,
    [property: JsonPropertyName("suggested")] string? Suggested,
    [property: JsonPropertyName("classification")] string? Classification); // bei compare

/// <summary>Ein Item, das eine Autor-Entscheidung braucht (modus-agnostisch, Bauvorschlag §1.1).</summary>
public sealed record AdjudicationItem(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,        // review_required_claim | coverage_miss | unit_signal
    [property: JsonPropertyName("sourceMode")] string SourceMode,    // normal | unit
    [property: JsonPropertyName("claimId")] string? ClaimId,
    [property: JsonPropertyName("unitId")] string? UnitId,
    [property: JsonPropertyName("proposition")] string Proposition,
    [property: JsonPropertyName("evidenceRefs")] IReadOnlyList<string> EvidenceRefs,
    [property: JsonPropertyName("systemSuggestion")] AdjudicationSuggestion? SystemSuggestion,
    [property: JsonPropertyName("reason")] string Reason,
    // --- vom Autor auszufüllen ---
    [property: JsonPropertyName("action")] string? Action = null,
    [property: JsonPropertyName("actionReason")] string? ActionReason = null,
    [property: JsonPropertyName("referenceTarget")] string? ReferenceTarget = null);

public sealed record AdjudicationQueue(
    [property: JsonPropertyName("sourceValidatedRunId")] string? SourceValidatedRunId,
    [property: JsonPropertyName("sourceUnitRunId")] string? SourceUnitRunId,
    [property: JsonPropertyName("generatedAt")] string GeneratedAt,
    [property: JsonPropertyName("items")] IReadOnlyList<AdjudicationItem> Items);

/// <summary>Audit-Record einer angewandten Aktion (was + warum).</summary>
public sealed record AdjudicationRecord(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemType")] string ItemType,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("resultingClaim")] SemanticLedgerEntry? ResultingClaim,
    [property: JsonPropertyName("referenceTarget")] string? ReferenceTarget,
    [property: JsonPropertyName("reason")] string Reason);

/// <summary>Unveränderliches Audit-Artefakt pro Adjudikationslauf (Bauvorschlag §1.3).</summary>
public sealed record AdjudicatedLedger(
    [property: JsonPropertyName("sourceValidatedRunId")] string? SourceValidatedRunId,
    [property: JsonPropertyName("sourceUnitRunId")] string? SourceUnitRunId,
    [property: JsonPropertyName("adjudicatedBy")] string AdjudicatedBy,
    [property: JsonPropertyName("adjudicatedAt")] string AdjudicatedAt,
    [property: JsonPropertyName("carriedApprovedClaims")] IReadOnlyList<SemanticLedgerEntry> CarriedApprovedClaims,
    [property: JsonPropertyName("records")] IReadOnlyList<AdjudicationRecord> Records,
    [property: JsonPropertyName("pendingCount")] int PendingCount);

/// <summary>Konsumierbare Projektion (Bauvorschlag §1.6): die FINALE Claim-Menge für den Artifact-Agenten.</summary>
public sealed record ConsumableLedger(
    [property: JsonPropertyName("sourceAdjudicatedAt")] string SourceAdjudicatedAt,
    [property: JsonPropertyName("claims")] IReadOnlyList<SemanticLedgerEntry> Claims,
    [property: JsonPropertyName("pendingCount")] int PendingCount,
    [property: JsonPropertyName("note")] string Note);
