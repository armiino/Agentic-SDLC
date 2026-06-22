using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z2.1-Nachweis: Der JuryReviewMapper übersetzt strukturierte Auto-Jury-Daten deterministisch in
/// einen ReviewResult — korrektes Achsen-/Severity-Mapping, ErrorScore als Legacy-Metrik,
/// Per-Item-/Coverage-Scores null (Fake-0-Schutz), invarianten-sichere Decision.
/// </summary>
public sealed class JuryReviewMapperTests
{
    private static JuryReviewInput Input(
        IReadOnlyList<JuryFinding> findings,
        int? errorScore = 4,
        string status = "ok",
        bool needsRepair = true)
        => new(
            ArtifactName: "requirements.md",
            ArtifactType: "requirements",
            EvaluatedCategories: new[] { "FALSE_CLAIM", "FALSE_CERTAINTY", "MISSING_TOPIC" },
            Findings: findings,
            ErrorScore: errorScore,
            EvaluationStatus: status,
            NeedsRepair: needsRepair,
            JudgeModel: "openai/gpt-4.1-mini");

    private static readonly JuryFinding[] TwoFindings =
    {
        new("FALSE_CLAIM", "KRITISCH", VerificationStatus.Confirmed,
            "99,5% Verfügbarkeit", "nicht vorhanden", "Erfundene Zahl, im Transkript verneint."),
        new("MISSING_TOPIC", "MITTEL", VerificationStatus.Partial,
            "nicht vorhanden", "Secrets-Management", "Thema teilweise abgedeckt.")
    };

    [Fact]
    public void Maps_Categories_To_Axes()
    {
        var r = JuryReviewMapper.Map(Input(TwoFindings));

        Assert.Equal(ReviewAxis.Grounding, r.Defects[0].Axis);   // FALSE_CLAIM
        Assert.Equal(ReviewAxis.Coverage, r.Defects[1].Axis);    // MISSING_TOPIC
        Assert.Equal("FalseClaim", r.Defects[0].Category);
        // EvaluatedAxes deckt alle drei laufenden Kategorien ab.
        Assert.Equal(
            new[] { ReviewAxis.Grounding, ReviewAxis.Certainty, ReviewAxis.Coverage },
            r.EvaluatedAxes);
    }

    [Fact]
    public void Maps_Severity_And_Quotes()
    {
        var r = JuryReviewMapper.Map(Input(TwoFindings));

        Assert.Equal(DefectSeverity.Critical, r.Defects[0].Severity);   // KRITISCH
        Assert.Equal(DefectSeverity.Medium, r.Defects[1].Severity);     // MITTEL
        Assert.Equal("99,5% Verfügbarkeit", r.Defects[0].ArtifactQuote);
        Assert.Equal("Secrets-Management", r.Defects[1].SourceQuote);   // transcriptEvidence -> SourceQuote
        Assert.True(r.Defects[0].Repairable);
    }

    [Fact]
    public void Keeps_ErrorScore_But_Leaves_PerItem_Scores_Null()
    {
        var r = JuryReviewMapper.Map(Input(TwoFindings, errorScore: 4));

        Assert.Equal(4, r.Metrics.ErrorScore);
        Assert.Null(r.Metrics.GroundingScore);   // Per-Item lief nicht -> null, NICHT 0
        Assert.Null(r.Metrics.CoverageScore);
        // Defekt-Scores aus Findings: confirmed KRITISCH(3), partial MITTEL(2).
        Assert.Equal(3, r.Metrics.ConfirmedDefectScore);
        Assert.Equal(2, r.Metrics.PartialDefectScore);
        Assert.Equal(1, r.Metrics.CriticalDefectCount);
    }

    [Fact]
    public void Decision_Repair_When_NeedsRepair_With_Actionable_Defect()
    {
        var r = JuryReviewMapper.Map(Input(TwoFindings, needsRepair: true));
        Assert.Equal(ReviewStatus.Succeeded, r.Status);
        Assert.Equal(GateDecision.Repair, r.Decision);
    }

    [Fact]
    public void UnverifiedDefectScore_Is_Derived_From_Unverified_Defects()
    {
        // Z2.4: unverified ist ein kept-Finding -> Score wird aus den Defects abgeleitet (MITTEL=2).
        var findings = new[]
        {
            new JuryFinding("FALSE_CLAIM", "MITTEL", VerificationStatus.Unverified, "q", "e", "r")
        };
        var r = JuryReviewMapper.Map(Input(findings));
        Assert.Equal(2, r.Metrics.UnverifiedCandidateScore);
        Assert.Equal(0, r.Metrics.ConfirmedDefectScore);
    }

    [Fact]
    public void Decision_Pass_When_No_Repair_Needed()
    {
        var r = JuryReviewMapper.Map(Input(System.Array.Empty<JuryFinding>(), errorScore: 0, needsRepair: false));
        Assert.Equal(GateDecision.Pass, r.Decision);
    }

    [Fact]
    public void Failed_Status_Forces_Failed_Decision()
    {
        // Invariante: Status=Failed => Decision in {Failed, HumanReview}, nie Pass/Repair.
        var r = JuryReviewMapper.Map(Input(TwoFindings, status: "failed", needsRepair: true));
        Assert.Equal(ReviewStatus.Failed, r.Status);
        Assert.Equal(GateDecision.Failed, r.Decision);
    }
}
