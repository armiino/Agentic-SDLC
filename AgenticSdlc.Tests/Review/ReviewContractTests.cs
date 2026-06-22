using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z1-Nachweis: Der V1-ReviewResult-Vertrag ist als getipptes Datenmodell baubar und serialisiert
/// stabil ins kanonische Wire-Format (jury/*.json). Sichert insbesondere den Fake-0-Schutz
/// (nullable Achsen-Scores) und Enums-als-Strings ab.
/// </summary>
public sealed class ReviewContractTests
{
    private static ReviewResult Sample() => new(
        ArtifactName: "requirements.md",
        ArtifactType: "requirements",
        Status: ReviewStatus.Succeeded,
        Metrics: new ReviewMetrics(
            ErrorScore: null,        // synthesis-Achse lief nicht -> null, NICHT 0
            GroundingScore: 2,
            CoverageScore: 3,
            ConfirmedDefectScore: 5,
            PartialDefectScore: 1,
            UnverifiedCandidateScore: 0,
            CriticalDefectCount: 1,
            RejectedCandidateCount: 4,
            UnsupportedClaimRate: 0.03,
            MissingCoverageRate: 0.12),
        EvaluatedAxes: new[] { ReviewAxis.Grounding, ReviewAxis.Coverage },
        Defects: new[]
        {
            new ReviewDefect(
                Id: "DEF-001",
                Axis: ReviewAxis.Coverage,
                Category: "MissingCoverage",
                Severity: DefectSeverity.Critical,
                VerificationStatus: VerificationStatus.Confirmed,
                Description: "Supportprozess fehlt als Anforderung.",
                SourceQuote: "Kontaktformular ohne Ticketpersistenz?",
                TargetSection: "Functional Requirements",
                RequiredAction: "Anforderung ergänzen oder als Scope-Entscheidung markieren.",
                Repairable: true)
        },
        Diagnostics: System.Array.Empty<ReviewDiagnostic>(),
        EvaluatorVersion: "review-v1",
        JudgeModel: "openai/gpt-4.1-mini",
        CreatedAt: new System.DateTimeOffset(2026, 6, 20, 12, 0, 0, System.TimeSpan.Zero));

    [Fact]
    public void RoundTrips_Stably_ThroughCanonicalJson()
    {
        var original = Sample();

        var json1 = JsonSerializer.Serialize(original, ReviewJson.Options);
        var restored = JsonSerializer.Deserialize<ReviewResult>(json1, ReviewJson.Options);
        Assert.NotNull(restored);
        var json2 = JsonSerializer.Serialize(restored, ReviewJson.Options);

        Assert.Equal(json1, json2); // stabiler Round-Trip
    }

    [Fact]
    public void Preserves_NullAxisScore_NotZero()
    {
        var json = JsonSerializer.Serialize(Sample(), ReviewJson.Options);
        var restored = JsonSerializer.Deserialize<ReviewResult>(json, ReviewJson.Options)!;

        // Fake-0-Schutz: nicht gelaufene Achse bleibt null, nicht 0.
        Assert.Null(restored.Metrics.ErrorScore);
        Assert.Equal(2, restored.Metrics.GroundingScore);
        Assert.DoesNotContain(ReviewAxis.Validity, restored.EvaluatedAxes);
    }

    [Fact]
    public void Serializes_Enums_AsStrings()
    {
        var json = JsonSerializer.Serialize(Sample(), ReviewJson.Options);

        Assert.Contains("\"Coverage\"", json);   // ReviewAxis als String
        Assert.Contains("\"Critical\"", json);    // DefectSeverity als String
        Assert.Contains("\"Confirmed\"", json);    // VerificationStatus als String
    }

    [Fact]
    public void Preserves_DefectFields_AfterRoundTrip()
    {
        var json = JsonSerializer.Serialize(Sample(), ReviewJson.Options);
        var restored = JsonSerializer.Deserialize<ReviewResult>(json, ReviewJson.Options)!;

        var defect = Assert.Single(restored.Defects);
        Assert.Equal(ReviewAxis.Coverage, defect.Axis);
        Assert.Equal(DefectSeverity.Critical, defect.Severity);
        Assert.Equal(VerificationStatus.Confirmed, defect.VerificationStatus);
        Assert.True(defect.Repairable);
        Assert.Null(defect.ArtifactUnitId); // V1-Schnitt: noch nicht befüllt
    }
}
