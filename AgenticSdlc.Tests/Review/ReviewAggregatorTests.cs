using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z4.1-Nachweis: Der ReviewAggregator führt disjunkte Achsen-Results (Per-Item-Grounding + Coverage)
/// zu EINEM ReviewResult zusammen — Achsen-Union, kombinierte Defects, beide Achsen-Scores erhalten,
/// Status = schlimmster.
/// </summary>
public sealed class ReviewAggregatorTests
{
    private static ReviewResult Grounding() => PerItemReviewMapper.Map(
        "requirements.md", "requirements",
        new[] { new PerItemUnit(null, "99,5% Verfügbarkeit", "fabricated", "Erfunden.") },
        coverage: null, "judge");

    private static ReviewResult Coverage(string verdict = "missing") => PerItemReviewMapper.Map(
        "requirements.md", "requirements",
        units: null,
        new[] { new PerItemCoverage("Ben", "Secrets-Management", verdict, "Fehlt.") },
        "judge");

    [Fact]
    public void Merges_Disjoint_Axes_Into_One_Result()
    {
        var merged = ReviewAggregator.Merge(new[] { Grounding(), Coverage() });

        // Achsen-Union: Grounding+Certainty (aus Grounding-Result) + Coverage.
        Assert.Equal(
            new[] { ReviewAxis.Grounding, ReviewAxis.Certainty, ReviewAxis.Coverage },
            merged.EvaluatedAxes);

        // Beide Achsen-Scores erhalten, ErrorScore null (keine Jury im Merge).
        Assert.Equal(2, merged.Metrics.GroundingScore);   // fabricated
        Assert.Equal(1, merged.Metrics.CoverageScore);     // missing
        Assert.Null(merged.Metrics.ErrorScore);

        // Kombinierte Defects (fabricated + missing).
        Assert.Equal(2, merged.Defects.Count);
        Assert.Equal(ReviewStatus.Succeeded, merged.Status);
        Assert.Equal(GateDecision.Repair, new GatePolicy().Evaluate(merged).Decision);
        Assert.Equal(5, merged.Metrics.ConfirmedDefectScore); // Critical(3) + Medium(2)
    }

    [Fact]
    public void Worst_Status_Wins()
    {
        // Coverage mit unclassified -> Partial; Grounding -> Succeeded; Merge muss Partial sein.
        var coveragePartial = PerItemReviewMapper.Map(
            "requirements.md", "requirements", units: null,
            new[] { new PerItemCoverage("Ben", "x", "unclassified", "kein Verdikt") }, "judge");

        var merged = ReviewAggregator.Merge(new[] { Grounding(), coveragePartial });
        Assert.Equal(ReviewStatus.Partial, merged.Status);
    }

    [Fact]
    public void Single_Part_Is_Returned_Equivalently()
    {
        var merged = ReviewAggregator.Merge(new[] { Grounding() });
        Assert.Equal(2, merged.Metrics.GroundingScore);
        Assert.Null(merged.Metrics.CoverageScore);
        Assert.Single(merged.Defects);
    }

    [Fact]
    public void Empty_Throws()
        => Assert.Throws<System.ArgumentException>(() => ReviewAggregator.Merge(System.Array.Empty<ReviewResult>()));
}
