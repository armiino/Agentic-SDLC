using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z4.2: Reiner Datei-/JSON-Merge — deserialisiert mehrere persistierte <see cref="ReviewResult"/>s
/// (verschiedene Achsen) und führt sie via <see cref="ReviewAggregator"/> zu einem zusammen. Kein LLM.
/// </summary>
/// <remarks>
/// Robust gegen Alt-Format: <see cref="ReviewJson"/> ignoriert unbekannte Properties (ein vor Z5
/// geschriebenes <c>review.json</c> mit altem <c>decision</c>-Feld wird sauber ohne Decision gelesen).
/// </remarks>
public static class ReviewMerge
{
    public static ReviewResult FromJson(IReadOnlyList<string> reviewJsons)
    {
        if (reviewJsons is null || reviewJsons.Count == 0)
            throw new ArgumentException("ReviewMerge.FromJson benötigt mindestens ein ReviewResult-JSON.", nameof(reviewJsons));

        var parts = reviewJsons
            .Select(j => JsonSerializer.Deserialize<ReviewResult>(j, ReviewJson.Options)
                ?? throw new InvalidOperationException("ReviewResult-JSON konnte nicht deserialisiert werden."))
            .ToList();

        return ReviewAggregator.Merge(parts);
    }
}
