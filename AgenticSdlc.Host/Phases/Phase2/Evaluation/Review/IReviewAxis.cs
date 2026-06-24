namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// C1: Eingabe für eine Review-Achse — EIN Artefakt plus die Quellen, gegen die es bewertet wird.
/// Bewusst LLM-/CLI-frei (reine Daten), damit der Review als <i>eine aufrufbare Einheit</i> dieselbe
/// Eingabe nutzt — heute offline (Runner), später inline im Workflow (Executor). Siehe
/// <c>architektur-zwei-welten.md</c> §9/§10.
/// </summary>
public sealed record ReviewSubject(
    string ArtifactName,                  // z. B. "requirements.md"
    string ArtifactType,                  // z. B. "requirements"
    string ArtifactText,                  // Markdown des Artefakts
    string Transcript,                    // Roh-Transkript (Quelle der Grounding-Achse)
    IReadOnlyList<TopicItem> Topics);     // eingefrorene Topic-Fixture (Coverage gated intern auf Relevanz)

/// <summary>
/// C1: Eine austauschbare Review-Achse (z. B. Grounding, Coverage). Kapselt ihren Mess-Mechanismus
/// (deterministisch ODER LLM) und liefert ein achsen-spezifisches <see cref="ReviewResult"/>.
/// Liefert <c>null</c>, wenn die Achse für dieses Subject nichts beitragen kann (z. B. keine relevanten
/// Topics) — der <see cref="ReviewService"/> merged nur Nicht-null-Ergebnisse.
/// </summary>
/// <remarks>
/// Diese Schnittstelle ist zugleich die geplante Executor-Grenze: jede Achse kann später ein eigener
/// MAF-Executor in einem Review-Sub-Workflow werden (Inline-Form b), ohne dass sich ihre Logik ändert.
/// </remarks>
public interface IReviewAxis
{
    /// <summary>Stabiler Achsenname (für Logs/Diagnostics), z. B. "grounding" / "coverage".</summary>
    string Name { get; }

    /// <summary>Bewertet das Subject auf dieser Achse. <c>null</c> = nicht anwendbar (kein Beitrag).</summary>
    Task<ReviewResult?> EvaluateAsync(ReviewSubject subject, CancellationToken cancellationToken = default);
}
