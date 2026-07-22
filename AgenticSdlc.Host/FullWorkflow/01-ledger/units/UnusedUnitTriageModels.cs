using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

public sealed record UnusedUnitTriageFixture(
    [property: JsonPropertyName("items")] IReadOnlyList<UnusedUnitTriageItem> Items);

public sealed record UnusedUnitTriageItem(
    [property: JsonPropertyName("unitId")] string UnitId,
    [property: JsonPropertyName("triage")] string Triage,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("keywords")] IReadOnlyList<string> Keywords);

public sealed record UnusedUnitLedgerCompareFixture(
    [property: JsonPropertyName("items")] IReadOnlyList<UnusedUnitLedgerCompareItem> Items);

public sealed record UnusedUnitLedgerCompareItem(
    [property: JsonPropertyName("unitId")] string UnitId,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("suggestedAction")] string SuggestedAction,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("suggestedProposition")] string? SuggestedProposition,
    [property: JsonPropertyName("relatedCandidateIds")] IReadOnlyList<string> RelatedCandidateIds);
