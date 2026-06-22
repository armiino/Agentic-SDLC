using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z3.1-Nachweis: Der PerItemReviewMapper bildet die Per-Item-Achsen auf den ReviewResult-Vertrag ab —
/// Spiegelbild des Jury-Adapters (GroundingScore/CoverageScore gesetzt, ErrorScore null), mit
/// unclassified → Diagnostic + Status=Partial (B29).
/// </summary>
public sealed class PerItemReviewMapperTests
{
    private static readonly PerItemUnit[] Units =
    {
        new(null, "99,5% Verfügbarkeit zugesichert", "fabricated", "Im Transkript verneint."),
        new("NFR", "OAuth wird im MVP genutzt", "overstated", "Im Transkript nur als Option."),
        new(null, "Web-first, Mobile später", "grounded", "Steht im Transkript."),
        new("Traceability", "Thema | Stakeholder", "not_a_claim", "Metadaten."),
        new(null, "irgendwas Unklares", "unclassified", "kein Verdikt erhalten")
    };

    private static readonly PerItemCoverage[] Coverage =
    {
        new("Ben", "CI/CD braucht Secrets-Management", "missing", "Im Artefakt nicht erfasst."),
        new("Anna", "Guten Morgen", "not_actionable", "Smalltalk."),
        new("Eva", "unklarer Turn", "unclassified", "kein Verdikt")
    };

    [Fact]
    public void Maps_Both_Axes_With_PerItem_Scores()
    {
        var r = PerItemReviewMapper.Map("requirements.md", "requirements", Units, Coverage, "judge");

        // ErrorScore null (Jury lief nicht), Per-Item-Scores gesetzt.
        Assert.Null(r.Metrics.ErrorScore);
        Assert.Equal(3, r.Metrics.GroundingScore);   // fabricated(2) + overstated(1)
        Assert.Equal(1, r.Metrics.CoverageScore);     // ein missing

        Assert.Equal(new[] { ReviewAxis.Grounding, ReviewAxis.Certainty, ReviewAxis.Coverage }, r.EvaluatedAxes);

        // 3 Defects: fabricated(Grounding/Critical), overstated(Certainty/Medium), missing(Coverage/Medium).
        Assert.Equal(3, r.Defects.Count);
        Assert.Equal(ReviewAxis.Grounding, r.Defects[0].Axis);
        Assert.Equal(DefectSeverity.Critical, r.Defects[0].Severity);
        Assert.Equal(ReviewAxis.Certainty, r.Defects[1].Axis);
        Assert.Equal(ReviewAxis.Coverage, r.Defects[2].Axis);
        Assert.Equal(1, r.Metrics.CriticalDefectCount);
        Assert.Equal(7, r.Metrics.ConfirmedDefectScore); // 3 + 2 + 2
    }

    [Fact]
    public void Unclassified_Surfaces_As_Diagnostic_And_Partial()
    {
        var r = PerItemReviewMapper.Map("requirements.md", "requirements", Units, Coverage, "judge");

        Assert.Equal(ReviewStatus.Partial, r.Status);   // wegen unclassified
        Assert.Equal(2, r.Diagnostics.Count);            // 1 Unit + 1 Turn unclassified
        Assert.Equal(GateDecision.Repair, r.Decision);   // es gibt Defekte
    }

    [Fact]
    public void CoverageScore_Null_When_Coverage_Not_Run()
    {
        var units = new[] { new PerItemUnit(null, "x", "fabricated", "r") };
        var r = PerItemReviewMapper.Map("risks.md", "risks", units, coverage: null, "judge");

        Assert.Equal(2, r.Metrics.GroundingScore);
        Assert.Null(r.Metrics.CoverageScore);            // Coverage-Achse lief nicht -> null (Fake-0-Schutz)
        Assert.DoesNotContain(ReviewAxis.Coverage, r.EvaluatedAxes);
        Assert.Equal(ReviewStatus.Succeeded, r.Status);
        Assert.Equal(GateDecision.Repair, r.Decision);
    }

    [Fact]
    public void Clean_Artifact_Passes()
    {
        var units = new[] { new PerItemUnit(null, "Web-first", "grounded", "ok") };
        var r = PerItemReviewMapper.Map("risks.md", "risks", units, coverage: null, "judge");

        Assert.Empty(r.Defects);
        Assert.Equal(0, r.Metrics.GroundingScore);
        Assert.Equal(GateDecision.Pass, r.Decision);
    }
}
