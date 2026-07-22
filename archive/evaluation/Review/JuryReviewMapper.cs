namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Ein gewertetes (kept) Finding der Auto-Jury in strukturierter Form .. die Eingabeeinheit für den
/// Mapper. Kategorie/Severity sind die Jury-Strings (FALSE_CLAIM/…, KRITISCH/MITTEL/GERING).
/// </summary>
public sealed record JuryFinding(
    string Category,                 // FALSE_CLAIM | FALSE_CERTAINTY | MISSING_TOPIC
    string Severity,                 // KRITISCH | MITTEL | GERING
    VerificationStatus Verification, // Confirmed (voll) | Partial (herabgestuft)
    string? ArtifactQuote,
    string? TranscriptEvidence,
    string Reason);

/// <summary>Strukturierte Jury-Daten zu EINEM Artefakt — Eingabe für <see cref="JuryReviewMapper"/>.</summary>
public sealed record JuryReviewInput(
    string ArtifactName,
    string ArtifactType,
    IReadOnlyList<string> EvaluatedCategories,   // Kategorie-Keys, die liefen (auch ohne Finding)
    IReadOnlyList<JuryFinding> Findings,         // gewertete Findings: Confirmed + Partial
    int? ErrorScore,                             // integer-wertig (Z2.2 rundet die double-Metrik)
    string EvaluationStatus,                     // "ok" | "failed"
    bool NeedsRepair,
    int RejectedCandidateCount = 0,              // Z2.4: verworfene Kandidaten (NICHT aus Defects ableitbar)
    string EvaluatorVersion = "jury-synthesis-v1",
    string JudgeModel = "",
    DateTimeOffset? CreatedAt = null);

/// <summary>
/// Z2: Übersetzt den Output der bestehenden Auto-Jury (open-ended <c>Evaluator</c>) in den gemeinsamen
/// <see cref="ReviewResult"/>-Vertrag. Reine, deterministische Funktion (offline, kein Eingriff in den
/// Jury-Pfad). Siehe NextStep/ReviewResult-vertrag.md („Mapping bestehender Ansaetze").
/// </summary>
/// <remarks>
/// Mapping-Entscheidungen (dokumentiert):
/// <list type="bullet">
/// <item>Achse: FALSE_CLAIM→Grounding, FALSE_CERTAINTY→Certainty, MISSING_TOPIC→Coverage.</item>
/// <item>Severity: KRITISCH→Critical, MITTEL→Medium, GERING→Low.</item>
/// <item><see cref="ReviewMetrics.ErrorScore"/> = Legacy-Jury-Score. <see cref="ReviewMetrics.GroundingScore"/>
///   und <see cref="ReviewMetrics.CoverageScore"/> bleiben <c>null</c> — die Jury deckt die *Achsen* über
///   ihren ErrorScore ab, aber die dedizierten Per-Item-/Coverage-Scores stammen aus anderen Producern,
///   die hier nicht liefen (Fake-0-Schutz: <c>null</c> ≠ 0).</item>
/// <item><see cref="ReviewResult.Decision"/> ist hier ein invarianten-sicherer Platzhalter, bis eine
///   echte <c>GatePolicy</c> existiert.</item>
/// </list>
/// </remarks>
public static class JuryReviewMapper
{
    public static ReviewResult Map(JuryReviewInput input)
    {
        var defects = new List<ReviewDefect>(input.Findings.Count);
        for (var i = 0; i < input.Findings.Count; i++)
        {
            var f = input.Findings[i];
            var (axis, category) = MapCategory(f.Category);
            defects.Add(new ReviewDefect(
                Id: $"JURY-{i + 1:D3}",
                Axis: axis,
                Category: category,
                Severity: MapSeverity(f.Severity),
                VerificationStatus: f.Verification,
                Description: f.Reason,
                ArtifactQuote: f.ArtifactQuote,
                SourceQuote: f.TranscriptEvidence,
                Repairable: f.Verification is VerificationStatus.Confirmed or VerificationStatus.Partial));
        }

        var evaluatedAxes = input.EvaluatedCategories
            .Select(c => MapCategory(c).Axis)
            .Distinct()
            .ToList();

        var metrics = new ReviewMetrics(
            ErrorScore: input.ErrorScore,
            GroundingScore: null,   // Per-Item-Producer lief nicht
            CoverageScore: null,    // Coverage-Producer lief nicht
            ConfirmedDefectScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Confirmed).Sum(d => Weight(d.Severity)),
            PartialDefectScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Partial).Sum(d => Weight(d.Severity)),
            UnverifiedCandidateScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Unverified).Sum(d => Weight(d.Severity)),
            CriticalDefectCount: defects.Count(d => d.Severity == DefectSeverity.Critical
                && d.VerificationStatus is VerificationStatus.Confirmed or VerificationStatus.Partial),
            RejectedCandidateCount: input.RejectedCandidateCount);

        var status = string.Equals(input.EvaluationStatus, "failed", StringComparison.OrdinalIgnoreCase)
            ? ReviewStatus.Failed
            : ReviewStatus.Succeeded;

        return new ReviewResult(
            ArtifactName: input.ArtifactName,
            ArtifactType: input.ArtifactType,
            Status: status,
            Metrics: metrics,
            EvaluatedAxes: evaluatedAxes,
            Defects: defects,
            Diagnostics: System.Array.Empty<ReviewDiagnostic>(),
            EvaluatorVersion: input.EvaluatorVersion,
            JudgeModel: input.JudgeModel,
            CreatedAt: input.CreatedAt ?? DateTimeOffset.UtcNow);
    }

    private static (ReviewAxis Axis, string Category) MapCategory(string juryCategory)
        => juryCategory?.Trim().ToUpperInvariant() switch
        {
            "FALSE_CLAIM" => (ReviewAxis.Grounding, "Grounding.FalseClaim"),
            "FALSE_CERTAINTY" => (ReviewAxis.Certainty, "Certainty.FalseCertainty"),
            "MISSING_TOPIC" => (ReviewAxis.Coverage, "Coverage.MissingTopic"),
            _ => (ReviewAxis.Validity, "Validity.Unknown")
        };

    private static DefectSeverity MapSeverity(string severity)
        => severity?.Trim().ToUpperInvariant() switch
        {
            "KRITISCH" => DefectSeverity.Critical,
            "MITTEL" => DefectSeverity.Medium,
            "GERING" => DefectSeverity.Low,
            _ => DefectSeverity.Info
        };

    private static int Weight(DefectSeverity s) => s switch
    {
        DefectSeverity.Critical => 3,
        DefectSeverity.Medium => 2,
        DefectSeverity.Low => 1,
        _ => 0
    };
}
