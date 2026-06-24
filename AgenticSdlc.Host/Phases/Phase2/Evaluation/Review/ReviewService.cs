namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// C1: Der wiederverwendbare Review-Kern. Führt die konfigurierten <see cref="IReviewAxis"/> über EIN
/// Artefakt aus und merged die achsen-spezifischen Ergebnisse via <see cref="ReviewAggregator"/> zu
/// EINEM <see cref="ReviewResult"/>. Reine Orchestrierung — KEIN LLM, KEINE Datei-/CLI-Logik (die liegt
/// in den Achsen bzw. im Runner).
/// </summary>
/// <remarks>
/// Genau diese Einheit ist die fehlende "aufrufbare Review-Einheit" (architektur-zwei-welten.md §9): heute
/// vom <c>review</c>-Runner offline genutzt, später unverändert hinter einem ReviewExecutor inline schaltbar.
/// Deterministisch + ohne LLM testbar (Fake-Achsen). Kein stiller Pass: kann keine Achse messen, ist das
/// Ergebnis explizit <see cref="ReviewStatus.Partial"/> + Diagnostic, nicht "Succeeded".
/// </remarks>
public sealed class ReviewService
{
    public const string EvaluatorVersion = "review-c1-v1";

    private readonly IReadOnlyList<IReviewAxis> _axes;

    public ReviewService(IReadOnlyList<IReviewAxis> axes)
    {
        if (axes is null || axes.Count == 0)
            throw new ArgumentException("ReviewService benötigt mindestens eine Achse.", nameof(axes));
        _axes = axes;
    }

    /// <summary>Bewertet ein Artefakt über alle Achsen und merged zu einem konsolidierten ReviewResult.</summary>
    public async Task<ReviewResult> ReviewAsync(ReviewSubject subject, CancellationToken cancellationToken = default)
    {
        var parts = new List<ReviewResult>();
        foreach (var axis in _axes)
        {
            var result = await axis.EvaluateAsync(subject, cancellationToken).ConfigureAwait(false);
            if (result is not null) parts.Add(result);
        }

        // Keine Achse konnte messen → ehrliches "nichts gemessen" (Partial + Diagnostic), KEIN stiller Pass.
        if (parts.Count == 0)
            return NoMeasurement(subject);

        return ReviewAggregator.Merge(parts, evaluatorVersion: EvaluatorVersion);
    }

    private static ReviewResult NoMeasurement(ReviewSubject subject) => new(
        ArtifactName: subject.ArtifactName,
        ArtifactType: subject.ArtifactType,
        Status: ReviewStatus.Partial,
        Metrics: new ReviewMetrics(
            ErrorScore: null, GroundingScore: null, CoverageScore: null,
            ConfirmedDefectScore: 0, PartialDefectScore: 0, UnverifiedCandidateScore: 0,
            CriticalDefectCount: 0, RejectedCandidateCount: 0),
        EvaluatedAxes: Array.Empty<ReviewAxis>(),
        Defects: Array.Empty<ReviewDefect>(),
        Diagnostics: new[]
        {
            new ReviewDiagnostic("NO_AXIS_RESULT",
                "Keine Review-Achse konnte dieses Artefakt messen.", subject.ArtifactName)
        },
        EvaluatorVersion: EvaluatorVersion,
        JudgeModel: string.Empty,
        CreatedAt: DateTimeOffset.UtcNow);
}
