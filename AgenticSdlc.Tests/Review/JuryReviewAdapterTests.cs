using AgenticSdlc.Host.Phases.Phase2.Evaluation;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z2.2b-Nachweis: Der Extraktor übersetzt strukturierte <see cref="ScoredFinding"/>s (Evaluator-Hook)
/// + Score-Daten korrekt in einen <see cref="ReviewResult"/> — inkl. Verdikt-Mapping
/// (confirmed/partial/unverified → <see cref="VerificationStatus"/>).
/// </summary>
public sealed class JuryReviewAdapterTests
{
    private static readonly ScoredFinding[] Scored =
    {
        new("FALSE_CLAIM", "KRITISCH", "confirmed", "99,5% Verfügbarkeit", "nicht vorhanden", "Erfunden."),
        new("MISSING_TOPIC", "MITTEL", "partial", "nicht vorhanden", "Secrets-Management", "Teilabdeckung.")
    };

    private static readonly string[] Categories = { "FALSE_CLAIM", "FALSE_CERTAINTY", "MISSING_TOPIC" };

    private static ReviewRequest Request() =>
        new(RunId: "run-1", ArtifactType: "requirements", ArtifactId: "requirements.md",
            ArtifactVersion: 0, ArtifactText: "…", RepairAttempt: 0);

    [Fact]
    public void Maps_ScoredFindings_And_Verdicts()
    {
        var r = JuryReviewAdapter.ToReviewResult(
            Scored, Categories, errorScore: 5, evaluationStatus: "ok", needsRepair: true,
            Request(), judgeModel: "openai/gpt-4.1-mini");

        Assert.Equal(2, r.Defects.Count);
        Assert.Equal(ReviewAxis.Grounding, r.Defects[0].Axis);
        Assert.Equal(VerificationStatus.Confirmed, r.Defects[0].VerificationStatus);
        Assert.Equal(VerificationStatus.Partial, r.Defects[1].VerificationStatus);
        Assert.Equal(DefectSeverity.Critical, r.Defects[0].Severity);

        Assert.Equal(5, r.Metrics.ErrorScore);
        Assert.Null(r.Metrics.GroundingScore);   // Per-Item lief nicht
        Assert.Equal(GateDecision.Repair, r.Decision);
        Assert.Equal("requirements.md", r.ArtifactName);
    }

    [Fact]
    public void EvaluatedAxes_Come_From_Categories_Not_Findings()
    {
        // Keine Findings, aber alle 3 Kategorien liefen → alle 3 Achsen evaluiert (gegen Fake-0).
        var r = JuryReviewAdapter.ToReviewResult(
            System.Array.Empty<ScoredFinding>(), Categories, errorScore: 0, evaluationStatus: "ok",
            needsRepair: false, Request(), judgeModel: "m");

        Assert.Equal(
            new[] { ReviewAxis.Grounding, ReviewAxis.Certainty, ReviewAxis.Coverage },
            r.EvaluatedAxes);
        Assert.Empty(r.Defects);
        Assert.Equal(GateDecision.Pass, r.Decision);
    }

    [Fact]
    public void RejectedCandidateCount_Flows_Through()
    {
        // Z2.4: rejected ist kein Defect -> als Aggregat durchgereicht (FromEvaluator zieht es aus Metadata).
        var r = JuryReviewAdapter.ToReviewResult(
            Scored, Categories, 5, "ok", true, Request(), "m", rejectedCandidateCount: 3);
        Assert.Equal(3, r.Metrics.RejectedCandidateCount);
    }

    [Fact]
    public void Unknown_Verdict_Defaults_To_Unverified()
    {
        var scored = new[] { new ScoredFinding("FALSE_CLAIM", "GERING", "???", null, null, "x") };
        var r = JuryReviewAdapter.ToReviewResult(
            scored, Categories, 1, "ok", true, Request(), "m");

        Assert.Equal(VerificationStatus.Unverified, r.Defects[0].VerificationStatus);
    }
}
