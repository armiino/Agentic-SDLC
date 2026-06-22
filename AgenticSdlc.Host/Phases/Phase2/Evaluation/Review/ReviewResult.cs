namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Gemeinsamer Ergebnisvertrag des Review-Kerns. V1 (siehe NextStep/ReviewResult-vertrag.md).
/// </summary>
/// <remarks>
/// Trennt bewusst <see cref="Status"/> (lief die Messung technisch?) von <see cref="Decision"/>
/// (was soll der Workflow tun?). <see cref="Decision"/> ist eine gecachte Projektion, die die
/// <c>GatePolicy</c> deterministisch aus <see cref="Status"/> + <see cref="Metrics"/> +
/// <see cref="Defects"/> + <see cref="EvaluatedAxes"/> berechnet.
/// Invarianten: <see cref="Status"/> = <see cref="ReviewStatus.Failed"/> ⇒ <see cref="Decision"/> ∈
/// { <see cref="GateDecision.Failed"/>, <see cref="GateDecision.HumanReview"/> };
/// <see cref="Decision"/> = <see cref="GateDecision.Repair"/> ⇒ mind. ein <see cref="ReviewDefect"/>
/// mit <see cref="VerificationStatus.Confirmed"/>/<see cref="VerificationStatus.Partial"/>.
/// Achsen-Scores in <see cref="Metrics"/>, die nicht in <see cref="EvaluatedAxes"/> stehen, sind null.
/// </remarks>
public sealed record ReviewResult(
    string ArtifactName,
    string ArtifactType,
    ReviewStatus Status,
    GateDecision Decision,
    ReviewMetrics Metrics,
    IReadOnlyList<ReviewAxis> EvaluatedAxes,
    IReadOnlyList<ReviewDefect> Defects,
    IReadOnlyList<ReviewDiagnostic> Diagnostics,
    string EvaluatorVersion,
    string JudgeModel,
    DateTimeOffset CreatedAt);
