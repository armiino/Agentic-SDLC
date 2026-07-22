using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// D1 / R0: Direct-Review als <see cref="IReviewAxis"/>. Nutzt den <see cref="DirectReviewClassifier"/>
/// (Artefakt direkt gegen Roh-Transkript, OHNE Fixture) und mappt die Befunde auf denselben
/// <see cref="ReviewResult"/>-Vertrag wie der TopicCoverage-Pfad — damit R0 und R1 fair vergleichbar sind
/// (EinsatzReviewWorkflow §4.6). Die Achse ist immer anwendbar (gibt nie <c>null</c> zurück); ein
/// Parse-Fehler wird als <see cref="ReviewStatus.Partial"/> + Diagnostic gemeldet (Fake-0-Schutz), NICHT
/// als sauberer Pass.
/// </summary>
public sealed class DirectReviewAxis : IReviewAxis
{
    private readonly DirectReviewClassifier _classifier;
    private readonly string _judgeModel;

    public DirectReviewAxis(DirectReviewClassifier classifier, string judgeModel)
    {
        _classifier = classifier;
        _judgeModel = judgeModel;
    }

    public string Name => "direct";

    public async Task<ReviewResult?> EvaluateAsync(ReviewSubject subject, CancellationToken cancellationToken = default)
    {
        var findings = await _classifier
            .ReviewAsync(subject.ArtifactType, subject.ArtifactText, subject.Transcript, cancellationToken)
            .ConfigureAwait(false);

        // Parse-Fehler ⇒ ehrliches Partial + Diagnostic (NIE stiller errorScore=0 / Pass).
        if (findings is null)
        {
            return new ReviewResult(
                subject.ArtifactName, subject.ArtifactType, ReviewStatus.Partial,
                new ReviewMetrics(null, null, null, 0, 0, 0, 0, 0),
                Array.Empty<ReviewAxis>(), Array.Empty<ReviewDefect>(),
                new[] { new ReviewDiagnostic("DIRECT_PARSE_FAILED",
                    "Direct-Review-Antwort nicht parsebar.", subject.ArtifactName) },
                "direct-review-v1", _judgeModel, DateTimeOffset.UtcNow);
        }

        var defects = new List<ReviewDefect>();
        var n = 0;
        foreach (var f in findings)
        {
            var (axis, category) = f.Axis == "coverage"
                ? (ReviewAxis.Coverage, "Coverage.MissingTopic")
                : (ReviewAxis.Grounding, "Grounding.FalseClaim");

            defects.Add(new ReviewDefect(
                $"DIR-{++n:D3}", axis, category, Severity(f.Severity), VerificationStatus.Confirmed,
                f.Description, ArtifactQuote: f.ArtifactQuote, SourceQuote: f.TranscriptQuote, Repairable: true));
        }

        var coverageScore = defects.Count(d => d.Axis == ReviewAxis.Coverage);
        var groundingScore = defects.Where(d => d.Axis == ReviewAxis.Grounding).Sum(d => Weight(d.Severity));

        var metrics = new ReviewMetrics(
            ErrorScore: null,
            GroundingScore: groundingScore,
            CoverageScore: coverageScore,
            ConfirmedDefectScore: defects.Sum(d => Weight(d.Severity)),
            PartialDefectScore: 0,
            UnverifiedCandidateScore: 0,
            CriticalDefectCount: defects.Count(d => d.Severity == DefectSeverity.Critical),
            RejectedCandidateCount: 0);

        // Direct-Review betrachtet BEIDE Achsen holistisch in einem Pass → beide als evaluiert ausweisen
        // (auch bei 0 Defects auf einer Achse: 0 ≠ null, weil die Achse gelaufen ist).
        return new ReviewResult(
            subject.ArtifactName, subject.ArtifactType, ReviewStatus.Succeeded, metrics,
            new[] { ReviewAxis.Coverage, ReviewAxis.Grounding }, defects, Array.Empty<ReviewDiagnostic>(),
            "direct-review-v1", _judgeModel, DateTimeOffset.UtcNow);
    }

    private static DefectSeverity Severity(string s) => s switch
    {
        "critical" => DefectSeverity.Critical,
        "low" => DefectSeverity.Low,
        _ => DefectSeverity.Medium
    };

    private static int Weight(DefectSeverity s) => s switch
    {
        DefectSeverity.Critical => 3,
        DefectSeverity.Medium => 2,
        DefectSeverity.Low => 1,
        _ => 0
    };
}
