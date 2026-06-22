namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Gemeinsamer Ergebnisvertrag des Review-Kerns — die **reine Messung**. V1
/// (siehe NextStep/ReviewResult-vertrag.md).
/// </summary>
/// <remarks>
/// Enthält bewusst **keine** <c>Decision</c>: die Workflow-Entscheidung ist policy-abhängig und wird
/// getrennt von der <c>GatePolicy</c> als <see cref="GateResult"/> abgeleitet (sonst sieht dieselbe
/// Messung je nach Policy wie eine andere fachliche Wahrheit aus). <see cref="Status"/> sagt nur, ob
/// die Messung technisch durchlief. Achsen-Scores in <see cref="Metrics"/>, die nicht in
/// <see cref="EvaluatedAxes"/> stehen, sind null (Fake-0-Schutz).
/// </remarks>
public sealed record ReviewResult(
    string ArtifactName,
    string ArtifactType,
    ReviewStatus Status,
    ReviewMetrics Metrics,
    IReadOnlyList<ReviewAxis> EvaluatedAxes,
    IReadOnlyList<ReviewDefect> Defects,
    IReadOnlyList<ReviewDiagnostic> Diagnostics,
    string EvaluatorVersion,
    string JudgeModel,
    DateTimeOffset CreatedAt);
