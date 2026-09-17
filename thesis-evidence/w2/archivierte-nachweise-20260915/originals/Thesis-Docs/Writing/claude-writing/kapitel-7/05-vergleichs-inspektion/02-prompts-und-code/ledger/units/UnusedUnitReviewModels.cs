using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

internal sealed record UnusedUnitReviewFixture(
    [property: JsonPropertyName("items")] IReadOnlyList<UnusedUnitReviewItem> Items);

internal sealed record UnusedUnitReviewItem(
    [property: JsonPropertyName("unitId")] string UnitId,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("suggestedAction")] string SuggestedAction,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("suggestedProposition")] string? SuggestedProposition,
    [property: JsonPropertyName("relatedCandidateIds")] IReadOnlyList<string> RelatedCandidateIds);
