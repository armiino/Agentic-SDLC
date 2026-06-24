namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Workflow-Entscheidung über ein Artefakt — abgeleitet aus einem <see cref="ReviewResult"/> durch eine
/// <see cref="GatePolicy"/>. Bewusst getrennt von der Messung: dieselbe Messung kann je nach Policy
/// (offline-Report / Runtime / Repair-Experiment) anders entschieden werden, daher trägt das Ergebnis
/// die <see cref="PolicyVersion"/> mit.
/// </summary>
public sealed record GateResult(
    GateDecision Decision,
    string PolicyVersion,
    IReadOnlyList<string> Reasons);

/// <summary>
/// Leitet deterministisch eine <see cref="GateResult"/> aus einem <see cref="ReviewResult"/> ab.
/// </summary>
/// <remarks>
/// **v2 — severity-gestuft.** Motivation (EXP-REVIEW-1 Teil A): die V1-Regel „irgendein bestätigter Defekt
/// → Repair" ignorierte die Severity → da Coverage/Per-Item ubiquitär bestätigte Defekte erzeugen,
/// entschied sie für JEDES Artefakt Repair (12/12) und trug keine Information mehr. v2 macht die **Severity
/// zur Hauptachse**: nur ein **kritischer** geglaubter Defekt (z. B. Halluzination) erzwingt auto-Repair;
/// die allgegenwärtigen Medium/Low-Defekte werden sichtbar gemacht (PassWithWarnings), nicht blockierend.
///
/// Präzedenz (von oben): Failed → Repair(kritisch) → HumanReview(unverifiziert/unvollständig) →
/// PassWithWarnings(Medium/Low) → Pass. „Geglaubt" = <see cref="VerificationStatus.Confirmed"/> oder
/// <see cref="VerificationStatus.Partial"/>; <see cref="VerificationStatus.Unverified"/> löst bewusst KEIN
/// auto-Repair aus (erst Verifikation/Mensch); <see cref="DefectSeverity.Info"/> ist unter der Warn-Schwelle.
///
/// Das ist eine **Policy-Wahl, kein an Daten optimiertes Schwellenwerk** (es gibt keine Ground Truth für
/// „hätte repariert werden müssen"). Zahlen-Schwellen (z. B. „viele Mediums → eskalieren") + pro-Artefakttyp
/// eingefrorene Policy bleiben bewusst offen (Phase-3-/A-B-C-Arbeit), bis gelabelte Daten existieren.
/// </remarks>
public sealed class GatePolicy
{
    public const string Version = "v2-severity";

    public GateResult Evaluate(ReviewResult review)
    {
        var reasons = new List<string>();

        // 1. Technisch fehlgeschlagene Messung → Failed (überstimmt alles).
        if (review.Status == ReviewStatus.Failed)
        {
            reasons.Add("ReviewStatus=Failed: Messung technisch nicht belastbar.");
            return new GateResult(GateDecision.Failed, Version, reasons);
        }

        // "Geglaubt" = der Defekt wird für (mindestens teilweise) real gehalten.
        static bool IsBelieved(ReviewDefect d) =>
            d.VerificationStatus is VerificationStatus.Confirmed or VerificationStatus.Partial;

        // 2. Kritischer geglaubter Defekt (z. B. Grounding.Fabricated) → Repair (unstrittig must-fix).
        var critical = review.Defects.Count(d => IsBelieved(d) && d.Severity == DefectSeverity.Critical);
        if (critical > 0)
        {
            reasons.Add($"{critical} kritische(r) bestätigte Defekt(e) → Repair.");
            return new GateResult(GateDecision.Repair, Version, reasons);
        }

        // 3. Nicht auto-entscheidbar: unverifizierte Kandidaten ODER unvollständige Messung → HumanReview.
        var unverified = review.Defects.Count(d => d.VerificationStatus == VerificationStatus.Unverified);
        if (unverified > 0 || review.Status == ReviewStatus.Partial)
        {
            if (unverified > 0) reasons.Add($"{unverified} unverifizierte Kandidaten → erst Verifikation/Mensch.");
            if (review.Status == ReviewStatus.Partial) reasons.Add("Messung unvollständig (Partial) → Mensch.");
            return new GateResult(GateDecision.HumanReview, Version, reasons);
        }

        // 4. Nur nicht-kritische geglaubte Defekte (Medium/Low), Messung vollständig → PassWithWarnings.
        var minor = review.Defects.Count(d => IsBelieved(d)
            && d.Severity is DefectSeverity.Medium or DefectSeverity.Low);
        if (minor > 0)
        {
            reasons.Add($"{minor} nicht-kritische Defekte (Medium/Low) → PassWithWarnings.");
            return new GateResult(GateDecision.PassWithWarnings, Version, reasons);
        }

        // 5. Keine actionable Defekte (höchstens Info), Messung vollständig → Pass.
        reasons.Add("Keine actionable Defekte, Messung vollständig.");
        return new GateResult(GateDecision.Pass, Version, reasons);
    }
}
