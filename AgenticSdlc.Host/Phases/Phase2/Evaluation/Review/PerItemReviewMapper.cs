namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>Eine klassifizierte Artefakt-Einheit (Per-Item-Grounding) — Eingabe für den Mapper.</summary>
public sealed record PerItemUnit(
    string? Section,
    string Text,
    string Verdict,   // grounded | overstated | fabricated | not_a_claim | unclassified
    string Reason);

/// <summary>Ein klassifizierter Transkript-Turn (Coverage-Achse) — Eingabe für den Mapper.</summary>
public sealed record PerItemCoverage(
    string Speaker,
    string Text,
    string Verdict,   // covered | missing | not_actionable | unclassified
    string Reason);

/// <summary>
/// Z3: Übersetzt die Per-Item-Achsen (Grounding/Certainty aus <c>classify-units</c>, Coverage aus
/// <c>coverage-units</c>) in den gemeinsamen <see cref="ReviewResult"/>-Vertrag — das Spiegelbild des
/// <see cref="JuryReviewMapper"/>: hier sind <see cref="ReviewMetrics.GroundingScore"/>/
/// <see cref="ReviewMetrics.CoverageScore"/> gesetzt und <see cref="ReviewMetrics.ErrorScore"/> ist null.
/// Reine, deterministische Funktion (offline, kein LLM).
/// </summary>
/// <remarks>
/// Mapping: <c>fabricated→Grounding/Critical</c>, <c>overstated→Certainty/Medium</c>,
/// <c>missing→Coverage/Medium</c>; <c>grounded</c>/<c>not_actionable</c>/<c>not_a_claim</c> → kein Defekt;
/// <c>unclassified</c> → <see cref="ReviewDiagnostic"/> + <see cref="ReviewStatus.Partial"/> (B29: kein
/// stiller Pass). Die Achsen-Scores nutzen die etablierten Per-Item-Gewichte (fabricated=2, overstated=1,
/// missing=1) → Kontinuität zu <c>classify-units</c>/<c>coverage-units</c> (B25–B28). Per-Item-Verdikte
/// sind die Entscheidung selbst → alle Defekte sind <see cref="VerificationStatus.Confirmed"/>.
/// </remarks>
public static class PerItemReviewMapper
{
    public static ReviewResult Map(
        string artifactName,
        string artifactType,
        IReadOnlyList<PerItemUnit>? units,
        IReadOnlyList<PerItemCoverage>? coverage,
        string judgeModel,
        string evaluatorVersion = "per-item-v1",
        DateTimeOffset? createdAt = null)
    {
        var defects = new List<ReviewDefect>();
        var diagnostics = new List<ReviewDiagnostic>();
        var axes = new List<ReviewAxis>();
        var partial = false;
        var n = 0;

        int? groundingScore = null;
        if (units is not null)
        {
            // Klassifizieren der Units prüft Grounding (fabricated) UND Certainty (overstated).
            axes.Add(ReviewAxis.Grounding);
            axes.Add(ReviewAxis.Certainty);
            groundingScore = 0;
            foreach (var u in units)
            {
                switch (u.Verdict)
                {
                    case "fabricated":
                        defects.Add(new ReviewDefect($"PI-{++n:D3}", ReviewAxis.Grounding, "Fabricated",
                            DefectSeverity.Critical, VerificationStatus.Confirmed, u.Reason,
                            ArtifactQuote: u.Text, TargetSection: u.Section, Repairable: true));
                        groundingScore += 2;
                        break;
                    case "overstated":
                        defects.Add(new ReviewDefect($"PI-{++n:D3}", ReviewAxis.Certainty, "Overstated",
                            DefectSeverity.Medium, VerificationStatus.Confirmed, u.Reason,
                            ArtifactQuote: u.Text, TargetSection: u.Section, Repairable: true));
                        groundingScore += 1;
                        break;
                    case "unclassified":
                        partial = true;
                        diagnostics.Add(new ReviewDiagnostic("UNIT_UNCLASSIFIED",
                            "Einheit konnte nicht klassifiziert werden.", u.Text));
                        break;
                    // grounded / not_a_claim → kein Defekt, Gewicht 0
                }
            }
        }

        int? coverageScore = null;
        if (coverage is not null)
        {
            axes.Add(ReviewAxis.Coverage);
            coverageScore = 0;
            foreach (var c in coverage)
            {
                switch (c.Verdict)
                {
                    case "missing":
                        defects.Add(new ReviewDefect($"PI-{++n:D3}", ReviewAxis.Coverage, "MissingCoverage",
                            DefectSeverity.Medium, VerificationStatus.Confirmed, c.Reason,
                            SourceQuote: c.Text, Repairable: true));
                        coverageScore += 1;
                        break;
                    case "unclassified":
                        partial = true;
                        diagnostics.Add(new ReviewDiagnostic("TURN_UNCLASSIFIED",
                            "Transkript-Turn konnte nicht klassifiziert werden.", c.Text));
                        break;
                    // covered / not_actionable → kein Defekt
                }
            }
        }

        var metrics = new ReviewMetrics(
            ErrorScore: null,                 // open-ended Jury lief nicht
            GroundingScore: groundingScore,
            CoverageScore: coverageScore,
            ConfirmedDefectScore: defects.Sum(d => SeverityWeight(d.Severity)),
            PartialDefectScore: 0,            // Per-Item kennt kein partial
            UnverifiedCandidateScore: 0,      // Klassifikation IST die Entscheidung → kein unverified
            CriticalDefectCount: defects.Count(d => d.Severity == DefectSeverity.Critical),
            RejectedCandidateCount: 0);

        var status = partial ? ReviewStatus.Partial : ReviewStatus.Succeeded;
        var decision = defects.Count > 0
            ? GateDecision.Repair
            : status == ReviewStatus.Partial
                ? GateDecision.HumanReview     // unvollständig gemessen, keine Defekte → kein sauberer Pass
                : GateDecision.Pass;

        return new ReviewResult(
            artifactName, artifactType, status, decision, metrics,
            axes, defects, diagnostics, evaluatorVersion, judgeModel, createdAt ?? DateTimeOffset.UtcNow);
    }

    private static int SeverityWeight(DefectSeverity s) => s switch
    {
        DefectSeverity.Critical => 3,
        DefectSeverity.Medium => 2,
        DefectSeverity.Low => 1,
        _ => 0
    };
}
