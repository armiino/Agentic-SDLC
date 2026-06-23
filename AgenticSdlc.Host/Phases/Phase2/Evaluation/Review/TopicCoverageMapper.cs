namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z6 (DISK-COV V1): Übersetzt Topic-Coverage-Verdikte in ein <see cref="ReviewResult"/> (Coverage-Achse).
/// Statt roher Transkript-Turns werden **aggregierte, eingefrorene Topics** bewertet — mit einem
/// **Relevanz-Gate** pro Artefakttyp. Das adressiert die B34-Treiber (Over-Counting, Falschzuordnung,
/// Rauschen). Reine, deterministische Funktion (offline).
/// </summary>
/// <remarks>
/// Gate: nur Topics mit <c>artifactType ∈ RelevantFor</c> werden bewertet. <c>missing</c> + relevante
/// <c>partial</c> → <c>Coverage.*</c>-Defects; <c>covered</c>/<c>not_applicable</c> → kein Defekt;
/// <c>unclassified</c> oder fehlendes Verdikt für ein relevantes Topic → Diagnostic + <c>Partial</c>.
/// <c>CoverageScore</c> = #missing-relevant; <c>MissingCoverageRate</c> = missing/relevant (interpretierbarer
/// Nenner, Contract-#4). Einheit „topic" über <c>evaluatorVersion = topic-coverage-v1</c>.
/// </remarks>
public static class TopicCoverageMapper
{
    public static ReviewResult Map(
        string artifactName,
        string artifactType,
        IReadOnlyList<TopicItem> topics,
        IReadOnlyList<TopicVerdict> verdicts,
        string judgeModel,
        string evaluatorVersion = "topic-coverage-v1",
        DateTimeOffset? createdAt = null)
    {
        var verdictById = verdicts
            .GroupBy(v => v.TopicId)
            .ToDictionary(g => g.Key, g => g.First());

        // Relevanz-Gate: nur für diesen Artefakttyp relevante Topics.
        var relevant = topics
            .Where(t => t.RelevantFor.Any(r => string.Equals(r, artifactType, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var defects = new List<ReviewDefect>();
        var diagnostics = new List<ReviewDiagnostic>();
        var partial = false;
        var missing = 0;
        var n = 0;

        foreach (var t in relevant)
        {
            if (!verdictById.TryGetValue(t.TopicId, out var v))
            {
                partial = true;
                diagnostics.Add(new ReviewDiagnostic("TOPIC_NO_VERDICT",
                    $"Kein Verdikt für relevantes Topic {t.TopicId}.", t.Summary));
                continue;
            }

            switch (v.Verdict?.Trim().ToLowerInvariant())
            {
                case "missing":
                    defects.Add(new ReviewDefect($"COV-{++n:D3}", ReviewAxis.Coverage, "Coverage.MissingTopic",
                        DefectSeverity.Medium, VerificationStatus.Confirmed, Reason(t, v),
                        SourceQuote: t.Summary, Repairable: true));
                    missing++;
                    break;
                case "partial":
                    defects.Add(new ReviewDefect($"COV-{++n:D3}", ReviewAxis.Coverage, "Coverage.PartialTopic",
                        DefectSeverity.Low, VerificationStatus.Confirmed, Reason(t, v),
                        SourceQuote: t.Summary, Repairable: true));
                    break;
                case "unclassified":
                    partial = true;
                    diagnostics.Add(new ReviewDiagnostic("TOPIC_UNCLASSIFIED",
                        $"Topic {t.TopicId} konnte nicht klassifiziert werden.", t.Summary));
                    break;
                // covered / not_applicable → kein Defekt
            }
        }

        var relevantCount = relevant.Count;
        var metrics = new ReviewMetrics(
            ErrorScore: null,
            GroundingScore: null,
            CoverageScore: missing,
            ConfirmedDefectScore: defects.Sum(d => SeverityWeight(d.Severity)),
            PartialDefectScore: 0,
            UnverifiedCandidateScore: 0,
            CriticalDefectCount: 0,
            RejectedCandidateCount: 0,
            MissingCoverageRate: relevantCount > 0 ? (double?)((double)missing / relevantCount) : null);

        var status = partial ? ReviewStatus.Partial : ReviewStatus.Succeeded;

        return new ReviewResult(
            artifactName, artifactType, status, metrics,
            new[] { ReviewAxis.Coverage }, defects, diagnostics,
            evaluatorVersion, judgeModel, createdAt ?? DateTimeOffset.UtcNow);
    }

    private static string Reason(TopicItem t, TopicVerdict v)
        => string.IsNullOrWhiteSpace(v.Reason) ? t.Summary : v.Reason;

    private static int SeverityWeight(DefectSeverity s) => s switch
    {
        DefectSeverity.Critical => 3,
        DefectSeverity.Medium => 2,
        DefectSeverity.Low => 1,
        _ => 0
    };
}
