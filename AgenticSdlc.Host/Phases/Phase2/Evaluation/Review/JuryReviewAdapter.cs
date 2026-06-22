using Microsoft.Extensions.AI.Evaluation;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z2.2: Brücke vom Output der bestehenden Auto-Jury (<see cref="Evaluator"/>) zum gemeinsamen
/// <see cref="ReviewResult"/>-Vertrag. Zwei Ebenen:
/// <list type="bullet">
/// <item><see cref="ToReviewResult"/> -> reine, testbare Funktion (nur public Eingaben, kein LLM).</item>
/// <item><see cref="FromEvaluator"/> -> strukturierten Daten aus einem realen <see cref="Evaluator"/>-Lauf zieht (erst im Run belegbar).</item>
/// </list>
/// Kein Eingriff in den Jury-Pfad — der <see cref="Evaluator"/> stellt die Daten über
/// <see cref="Evaluator.LastScoredFindings"/> nur zusätzlich bereit (Z2.2a-Hook).
/// </summary>
public static class JuryReviewAdapter
{
    /// <summary>Reine Übersetzung strukturierter Jury-Daten → <see cref="ReviewResult"/> (unit-testbar).</summary>
    public static ReviewResult ToReviewResult(
        IReadOnlyList<ScoredFinding> scoredFindings,
        IReadOnlyList<string> evaluatedCategoryKeys,
        int? errorScore,
        string evaluationStatus,
        bool needsRepair,
        ReviewRequest request,
        string judgeModel,
        int rejectedCandidateCount = 0,
        string evaluatorVersion = "jury-synthesis-v1",
        DateTimeOffset? createdAt = null)
    {
        var findings = scoredFindings
            .Select(s => new JuryFinding(
                Category: s.Category ?? string.Empty,
                Severity: s.Severity ?? string.Empty,
                Verification: MapVerdict(s.Verdict),
                ArtifactQuote: s.ArtifactQuote,
                TranscriptEvidence: s.TranscriptEvidence,
                Reason: s.Reason ?? string.Empty))
            .ToList();

        var input = new JuryReviewInput(
            ArtifactName: request.ArtifactId,
            ArtifactType: request.ArtifactType,
            EvaluatedCategories: evaluatedCategoryKeys,
            Findings: findings,
            ErrorScore: errorScore,
            EvaluationStatus: evaluationStatus,
            NeedsRepair: needsRepair,
            RejectedCandidateCount: rejectedCandidateCount,
            EvaluatorVersion: evaluatorVersion,
            JudgeModel: judgeModel,
            CreatedAt: createdAt);

        return JuryReviewMapper.Map(input);
    }

    /// <summary>
    /// zieht die strukturierten Daten aus einem realen <see cref="Evaluator"/>-Lauf
    /// (<see cref="Evaluator.LastScoredFindings"/>, <see cref="Evaluator.Categories"/>, ErrorScore-Metrik)
    /// und delegiert an <see cref="ToReviewResult"/>. Erst im echten Run belegbar (kein Unit-Test).
    /// </summary>
    public static ReviewResult FromEvaluator(
        Evaluator evaluator, EvaluationResult result, ReviewRequest request, string judgeModel)
    {
        var score = EvaluatorOutput.ReadScore(result);
        var errorScore = score.ErrorScore is { } d ? (int)Math.Round(d) : (int?)null;
        var categoryKeys = evaluator.Categories.Select(c => c.Key).ToList();

        // Z2.4: rejected-Kandidaten sind verworfen (kein Defect) → nur aus der Kategorie-Metadata holbar.
        var rejected = evaluator.Categories
            .Sum(c => ReadMetaInt(result.Get<NumericMetric>(c.MetricName), "rejected"));

        return ToReviewResult(
            evaluator.LastScoredFindings,
            categoryKeys,
            errorScore,
            score.EvaluationStatus,
            score.NeedsRepair,
            request,
            judgeModel,
            rejectedCandidateCount: rejected);
    }

    private static int ReadMetaInt(NumericMetric metric, string key)
        => metric.Metadata?.TryGetValue(key, out var v) == true && int.TryParse(v, out var n) ? n : 0;

    private static VerificationStatus MapVerdict(string verdict)
        => verdict?.Trim().ToLowerInvariant() switch
        {
            "confirmed" => VerificationStatus.Confirmed,
            "partial" => VerificationStatus.Partial,
            "unverified" => VerificationStatus.Unverified,
            _ => VerificationStatus.Unverified
        };
}
