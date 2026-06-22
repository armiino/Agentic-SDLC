namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Ein konkreter Defekt — die zentrale Repair-Einheit. V1-Vertrag (ReviewResult-vertrag).
/// </summary>
/// <remarks>
/// <c>Id</c>/<c>ArtifactUnitId</c> sind run-lokal, NICHT semantisch stabil → kein Cross-Run-Vergleich
/// roher IDs. <c>Severity</c> ist bewusst ein Enum (steuert Gewichtung). Felder, die im V1-Schnitt noch
/// nicht befüllt werden (<c>ArtifactUnitId</c>, <c>EvidenceItemId</c>, <c>TargetSection</c>,
/// <c>RequiredAction</c>), bleiben <c>null</c>.
/// </remarks>
public sealed record ReviewDefect(
    string Id,
    ReviewAxis Axis,
    string Category,
    DefectSeverity Severity,
    VerificationStatus VerificationStatus,
    string Description,
    string? ArtifactQuote = null,
    string? SourceQuote = null,
    string? ArtifactUnitId = null,
    string? EvidenceItemId = null,
    string? TargetSection = null,
    string? RequiredAction = null,
    bool Repairable = false);
