using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// GatePolicy-v2-Nachweis (severity-gestuft): die Entscheidung wird deterministisch aus einem ReviewResult
/// abgeleitet (getrennt von der Messung). Nur ein KRITISCHER geglaubter Defekt erzwingt Repair; Medium/Low
/// → PassWithWarnings; unverifiziert/unvollständig → HumanReview; unverifizierte lösen kein Auto-Repair aus.
/// </summary>
public sealed class GatePolicyTests
{
    private static ReviewDefect Defect(DefectSeverity sev, VerificationStatus vs, int i = 0)
        => new($"D{i}", ReviewAxis.Grounding, "Grounding.X", sev, vs, "reason");

    private static ReviewResult R(ReviewStatus status, params ReviewDefect[] defects)
    {
        var metrics = new ReviewMetrics(null, null, null, 0, 0, 0, 0, 0);
        return new ReviewResult("a.md", "a", status, metrics, new[] { ReviewAxis.Grounding },
            defects, System.Array.Empty<ReviewDiagnostic>(), "v", "m", System.DateTimeOffset.UtcNow);
    }

    private static readonly GatePolicy Policy = new();

    [Fact]
    public void Failed_Status_Wins_Over_Defects()
        => Assert.Equal(GateDecision.Failed,
            Policy.Evaluate(R(ReviewStatus.Failed, Defect(DefectSeverity.Critical, VerificationStatus.Confirmed))).Decision);

    [Fact]
    public void Critical_Confirmed_Triggers_Repair()
        => Assert.Equal(GateDecision.Repair,
            Policy.Evaluate(R(ReviewStatus.Succeeded, Defect(DefectSeverity.Critical, VerificationStatus.Confirmed))).Decision);

    [Fact]
    public void Critical_Wins_Over_Partial_Measurement()
        => Assert.Equal(GateDecision.Repair,
            Policy.Evaluate(R(ReviewStatus.Partial, Defect(DefectSeverity.Critical, VerificationStatus.Confirmed))).Decision);

    [Fact]
    public void Medium_Or_Low_Confirmed_Is_PassWithWarnings_Not_Repair()
    {
        Assert.Equal(GateDecision.PassWithWarnings,
            Policy.Evaluate(R(ReviewStatus.Succeeded, Defect(DefectSeverity.Medium, VerificationStatus.Confirmed))).Decision);
        Assert.Equal(GateDecision.PassWithWarnings,
            Policy.Evaluate(R(ReviewStatus.Succeeded, Defect(DefectSeverity.Low, VerificationStatus.Confirmed))).Decision);
    }

    [Fact]
    public void Unverified_Only_Goes_To_HumanReview()
        => Assert.Equal(GateDecision.HumanReview,
            Policy.Evaluate(R(ReviewStatus.Succeeded, Defect(DefectSeverity.Medium, VerificationStatus.Unverified))).Decision);

    [Fact]
    public void Partial_Without_Critical_Goes_To_HumanReview()
        => Assert.Equal(GateDecision.HumanReview, Policy.Evaluate(R(ReviewStatus.Partial)).Decision);

    [Fact]
    public void Clean_Passes()
        => Assert.Equal(GateDecision.Pass, Policy.Evaluate(R(ReviewStatus.Succeeded)).Decision);

    [Fact]
    public void Result_Carries_PolicyVersion_And_Reasons()
    {
        var g = Policy.Evaluate(R(ReviewStatus.Succeeded, Defect(DefectSeverity.Critical, VerificationStatus.Confirmed)));
        Assert.Equal(GatePolicy.Version, g.PolicyVersion);
        Assert.NotEmpty(g.Reasons);
    }
}
