using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// C1-Nachweis: die LLM-freie Orchestrierung des <see cref="ReviewService"/> — Achsen-Ergebnisse mergen,
/// null-Achsen überspringen, "keine Achse misst" → ehrliches Partial (kein stiller Pass), leere Achsenliste
/// abweisen. Die echten LLM-Achsen (Grounding/Coverage) sind ein Run (nicht unit-getestet).
/// </summary>
public sealed class ReviewServiceTests
{
    private sealed class FakeAxis : IReviewAxis
    {
        private readonly ReviewResult? _result;
        public FakeAxis(string name, ReviewResult? result) { Name = name; _result = result; }
        public string Name { get; }
        public Task<ReviewResult?> EvaluateAsync(ReviewSubject subject, CancellationToken ct = default)
            => Task.FromResult(_result);
    }

    private static ReviewSubject Subject() =>
        new("requirements.md", "requirements", "# Inhalt", "transcript", Array.Empty<TopicItem>());

    private static ReviewResult AxisResult(ReviewAxis axis, int? grounding = null, int? coverage = null) =>
        new("requirements.md", "requirements", ReviewStatus.Succeeded,
            new ReviewMetrics(null, grounding, coverage, 0, 0, 0, 0, 0),
            new[] { axis }, Array.Empty<ReviewDefect>(), Array.Empty<ReviewDiagnostic>(),
            "fake", "fake-model", DateTimeOffset.UtcNow);

    [Fact]
    public async Task Merges_Disjoint_Axis_Results()
    {
        var service = new ReviewService(new IReviewAxis[]
        {
            new FakeAxis("grounding", AxisResult(ReviewAxis.Grounding, grounding: 2)),
            new FakeAxis("coverage", AxisResult(ReviewAxis.Coverage, coverage: 3)),
        });

        var result = await service.ReviewAsync(Subject());

        Assert.Equal(ReviewService.EvaluatorVersion, result.EvaluatorVersion);
        Assert.Contains(ReviewAxis.Grounding, result.EvaluatedAxes);
        Assert.Contains(ReviewAxis.Coverage, result.EvaluatedAxes);
        Assert.Equal(2, result.Metrics.GroundingScore);
        Assert.Equal(3, result.Metrics.CoverageScore);
    }

    [Fact]
    public async Task Skips_Null_Axis()
    {
        var service = new ReviewService(new IReviewAxis[]
        {
            new FakeAxis("grounding", AxisResult(ReviewAxis.Grounding, grounding: 1)),
            new FakeAxis("coverage", null),   // nicht anwendbar
        });

        var result = await service.ReviewAsync(Subject());

        Assert.Contains(ReviewAxis.Grounding, result.EvaluatedAxes);
        Assert.DoesNotContain(ReviewAxis.Coverage, result.EvaluatedAxes);
        Assert.Equal(1, result.Metrics.GroundingScore);
    }

    [Fact]
    public async Task All_Null_Yields_Partial_NoMeasurement_Not_Silent_Pass()
    {
        var service = new ReviewService(new IReviewAxis[]
        {
            new FakeAxis("grounding", null),
            new FakeAxis("coverage", null),
        });

        var result = await service.ReviewAsync(Subject());

        Assert.Equal(ReviewStatus.Partial, result.Status);
        Assert.Empty(result.EvaluatedAxes);
        Assert.Contains(result.Diagnostics, d => d.Code == "NO_AXIS_RESULT");
    }

    [Fact]
    public void Empty_Axis_List_Is_Rejected()
        => Assert.Throws<ArgumentException>(() => new ReviewService(Array.Empty<IReviewAxis>()));
}
