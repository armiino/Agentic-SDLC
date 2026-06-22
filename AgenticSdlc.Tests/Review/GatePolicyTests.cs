using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z5.1-Nachweis: Die GatePolicy leitet die Workflow-Entscheidung deterministisch aus einem
/// ReviewResult ab (getrennt von der Messung). Unverifizierte Kandidaten lösen kein Auto-Repair aus.
/// </summary>
public sealed class GatePolicyTests
{
    private static ReviewResult R(ReviewStatus status, params VerificationStatus[] verdicts)
    {
        var defects = verdicts
            .Select((v, i) => new ReviewDefect($"D{i}", ReviewAxis.Grounding, "Grounding.X",
                DefectSeverity.Medium, v, "reason"))
            .ToList();
        var metrics = new ReviewMetrics(null, null, null, 0, 0, 0, 0, 0);
        return new ReviewResult("a.md", "a", status, metrics, new[] { ReviewAxis.Grounding },
            defects, System.Array.Empty<ReviewDiagnostic>(), "v", "m", System.DateTimeOffset.UtcNow);
    }

    private static readonly GatePolicy Policy = new();

    [Fact]
    public void Failed_Status_Wins_Over_Defects()
        => Assert.Equal(GateDecision.Failed, Policy.Evaluate(R(ReviewStatus.Failed, VerificationStatus.Confirmed)).Decision);

    [Fact]
    public void Confirmed_Defect_Triggers_Repair()
        => Assert.Equal(GateDecision.Repair, Policy.Evaluate(R(ReviewStatus.Succeeded, VerificationStatus.Confirmed)).Decision);

    [Fact]
    public void Unverified_Only_Goes_To_HumanReview()
        => Assert.Equal(GateDecision.HumanReview, Policy.Evaluate(R(ReviewStatus.Succeeded, VerificationStatus.Unverified)).Decision);

    [Fact]
    public void Partial_Without_Actionable_Goes_To_HumanReview()
        => Assert.Equal(GateDecision.HumanReview, Policy.Evaluate(R(ReviewStatus.Partial)).Decision);

    [Fact]
    public void Clean_Passes()
        => Assert.Equal(GateDecision.Pass, Policy.Evaluate(R(ReviewStatus.Succeeded)).Decision);

    [Fact]
    public void Result_Carries_PolicyVersion_And_Reasons()
    {
        var g = Policy.Evaluate(R(ReviewStatus.Succeeded, VerificationStatus.Confirmed));
        Assert.Equal(GatePolicy.Version, g.PolicyVersion);
        Assert.NotEmpty(g.Reasons);
    }
}
