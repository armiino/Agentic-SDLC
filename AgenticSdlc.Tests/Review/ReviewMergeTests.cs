using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z4.2-Nachweis: Der reine Datei-/JSON-Merge führt persistierte Achsen-ReviewResults zu EINEM zusammen
/// (kein LLM) und ist robust gegen das Alt-Format (vor Z5, mit `decision`-Feld).
/// </summary>
public sealed class ReviewMergeTests
{
    private static string Json(ReviewResult r) => JsonSerializer.Serialize(r, ReviewJson.Options);

    private static ReviewResult Grounding() => PerItemReviewMapper.Map(
        "requirements.md", "requirements",
        new[] { new PerItemUnit(null, "99,5%", "fabricated", "Erfunden.") }, coverage: null, "judge");

    private static ReviewResult Coverage() => PerItemReviewMapper.Map(
        "requirements.md", "requirements", units: null,
        new[] { new PerItemCoverage("Ben", "Secrets", "missing", "Fehlt.") }, "judge");

    [Fact]
    public void Merges_Two_Axis_Jsons_Into_One()
    {
        var merged = ReviewMerge.FromJson(new[] { Json(Grounding()), Json(Coverage()) });

        Assert.Equal(new[] { ReviewAxis.Grounding, ReviewAxis.Certainty, ReviewAxis.Coverage }, merged.EvaluatedAxes);
        Assert.Equal(2, merged.Metrics.GroundingScore);
        Assert.Equal(1, merged.Metrics.CoverageScore);
        Assert.Null(merged.Metrics.ErrorScore);
        Assert.Equal(2, merged.Defects.Count);
    }

    [Fact]
    public void Tolerates_Legacy_Json_With_Decision_Field()
    {
        // Vor Z5 trug review.json ein "decision"-Feld. ReviewJson ignoriert unbekannte Properties.
        var legacy =
            "{\"artifactName\":\"a.md\",\"artifactType\":\"a\",\"status\":\"Succeeded\",\"decision\":\"Repair\"," +
            "\"metrics\":{\"errorScore\":null,\"groundingScore\":2,\"coverageScore\":null,\"confirmedDefectScore\":0," +
            "\"partialDefectScore\":0,\"unverifiedCandidateScore\":0,\"criticalDefectCount\":0,\"rejectedCandidateCount\":0," +
            "\"unsupportedClaimRate\":null,\"missingCoverageRate\":null},\"evaluatedAxes\":[\"Grounding\"]," +
            "\"defects\":[],\"diagnostics\":[],\"evaluatorVersion\":\"v\",\"judgeModel\":\"m\"," +
            "\"createdAt\":\"2026-06-22T00:00:00+00:00\"}";

        var merged = ReviewMerge.FromJson(new[] { legacy });

        Assert.Equal(2, merged.Metrics.GroundingScore);
        Assert.Contains(ReviewAxis.Grounding, merged.EvaluatedAxes);
    }

    [Fact]
    public void Empty_Throws()
        => Assert.Throws<System.ArgumentException>(() => ReviewMerge.FromJson(System.Array.Empty<string>()));
}
