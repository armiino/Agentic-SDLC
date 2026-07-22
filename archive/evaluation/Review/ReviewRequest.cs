namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Eingabe für einen Review eines einzelnen Artefakt-(Draft-)Stands. V1-Vertrag
/// (siehe NextStep/ReviewResult-vertrag.md). Die Runtime-/Repair-Felder (<see cref="ArtifactVersion"/>,
/// <see cref="RepairAttempt"/>) sind im aktuellen Offline-Einsatz konstant (0), aber bereits Teil des
/// Vertrags für den späteren Maker-Checker-Repair-Loop.
/// </summary>
public sealed record ReviewRequest(
    string RunId,
    string ArtifactType,
    string ArtifactId,
    int ArtifactVersion,
    string ArtifactText,
    int RepairAttempt);
