namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z4: Führt mehrere achsen-spezifische <see cref="ReviewResult"/>s desselben Artefakts zu EINEM
/// konsolidierten Ergebnis zusammen (z. B. Per-Item-Grounding + Coverage). Reine, deterministische
/// Funktion (offline). Siehe NextStep/ReviewResult-vertrag.md (Reihenfolge Schritt 5).
/// </summary>
/// <remarks>
/// Gedacht für Producer mit **disjunkten Achsen**. Die semantische De-Duplizierung **überlappender**
/// Producer (z. B. Jury-Grounding ↔ Per-Item-Grounding) ist eine separate, P1-abhängige Aufgabe und hier
/// bewusst NICHT enthalten — der Merge konkateniert Defects und vereinigt Achsen.
/// Achsen-Scores (`ErrorScore`/`GroundingScore`/`CoverageScore`) werden als erster Nicht-null-Wert
/// übernommen (jeder stammt aus genau einem Producer); die Defekt-Scores werden aus den gemergten
/// Defects neu abgeleitet (Source of Truth = Defects).
/// </remarks>
public static class ReviewAggregator
{
    public static ReviewResult Merge(
        IReadOnlyList<ReviewResult> parts,
        string? evaluatorVersion = null,
        DateTimeOffset? createdAt = null)
    {
        if (parts is null || parts.Count == 0)
            throw new ArgumentException("ReviewAggregator.Merge benötigt mindestens ein ReviewResult.", nameof(parts));

        var first = parts[0];
        var defects = parts.SelectMany(p => p.Defects).ToList();
        var diagnostics = parts.SelectMany(p => p.Diagnostics).ToList();

        // EvaluatedAxes: Vereinigung in Reihenfolge des ersten Auftretens.
        var axes = parts.SelectMany(p => p.EvaluatedAxes).Distinct().ToList();

        var metrics = new ReviewMetrics(
            ErrorScore: FirstNonNull(parts, m => m.ErrorScore),
            GroundingScore: FirstNonNull(parts, m => m.GroundingScore),
            CoverageScore: FirstNonNull(parts, m => m.CoverageScore),
            ConfirmedDefectScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Confirmed).Sum(d => Weight(d.Severity)),
            PartialDefectScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Partial).Sum(d => Weight(d.Severity)),
            UnverifiedCandidateScore: defects.Where(d => d.VerificationStatus == VerificationStatus.Unverified).Sum(d => Weight(d.Severity)),
            CriticalDefectCount: defects.Count(d => d.Severity == DefectSeverity.Critical
                && d.VerificationStatus is VerificationStatus.Confirmed or VerificationStatus.Partial),
            RejectedCandidateCount: parts.Sum(p => p.Metrics.RejectedCandidateCount),
            UnsupportedClaimRate: FirstNonNull(parts, m => m.UnsupportedClaimRate),
            MissingCoverageRate: FirstNonNull(parts, m => m.MissingCoverageRate));

        var status = WorstStatus(parts);

        var judgeModel = string.Join("+", parts.Select(p => p.JudgeModel).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());

        return new ReviewResult(
            ArtifactName: first.ArtifactName,
            ArtifactType: first.ArtifactType,
            Status: status,
            Metrics: metrics,
            EvaluatedAxes: axes,
            Defects: defects,
            Diagnostics: diagnostics,
            EvaluatorVersion: evaluatorVersion ?? "review-merged-v1",
            JudgeModel: judgeModel,
            CreatedAt: createdAt ?? parts.Max(p => p.CreatedAt));
    }

    private static int? FirstNonNull(IReadOnlyList<ReviewResult> parts, Func<ReviewMetrics, int?> sel)
        => parts.Select(p => sel(p.Metrics)).FirstOrDefault(v => v is not null);

    private static double? FirstNonNull(IReadOnlyList<ReviewResult> parts, Func<ReviewMetrics, double?> sel)
        => parts.Select(p => sel(p.Metrics)).FirstOrDefault(v => v is not null);

    private static ReviewStatus WorstStatus(IReadOnlyList<ReviewResult> parts)
    {
        if (parts.Any(p => p.Status == ReviewStatus.Failed)) return ReviewStatus.Failed;
        if (parts.Any(p => p.Status == ReviewStatus.Partial)) return ReviewStatus.Partial;
        return ReviewStatus.Succeeded;
    }

    private static int Weight(DefectSeverity s) => s switch
    {
        DefectSeverity.Critical => 3,
        DefectSeverity.Medium => 2,
        DefectSeverity.Low => 1,
        _ => 0
    };
}
