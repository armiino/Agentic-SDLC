namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Aggregierte Messwerte eines Reviews. V1-Vertrag (siehe NextStep/ReviewResult-vertrag.md).
/// </summary>
/// <remarks>
/// Die achsen-spezifischen Scores sind <c>nullable</c>: <c>null</c> = die Achse lief nicht (≠ <c>0</c>),
/// damit ein nicht gemessenes Artefakt nicht als sauberer Pass durchrutscht (Fake-0-Schutz, B8/B29).
/// <c>ReviewMetrics</c> ist abgeleitet (Source of Truth = <c>ReviewResult.Defects</c> +
/// <c>EvaluatedAxes</c>); es wird vom künftigen <c>ReviewAggregator</c> deterministisch berechnet.
/// </remarks>
public sealed record ReviewMetrics(
    int? ErrorScore,                  // null = synthesis-Achse lief nicht
    int? GroundingScore,              // null = Per-Item-Grounding lief nicht
    int? CoverageScore,               // null = Coverage-Achse lief nicht
    int ConfirmedDefectScore,
    int PartialDefectScore,
    int UnverifiedCandidateScore,
    int CriticalDefectCount,
    int RejectedCandidateCount,
    double? UnsupportedClaimRate = null,
    double? MissingCoverageRate = null);
