namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Technischer/methodischer Hinweis zum Review (z. B. Parsefehler, abgebrochene Achse, Chunk-Ausfall).
/// KEIN fachlicher Defekt — fachliche Befunde sind <see cref="ReviewDefect"/>.
/// </summary>
public sealed record ReviewDiagnostic(
    string Code,
    string Message,
    string? Detail = null);
