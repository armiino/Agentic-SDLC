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
/// **Platzhalter-Policy (V1).** Einheitliche, mess-basierte Regel; die echte, konfigurierbare und
/// **pro Artefakttyp** eingefrorene Policy (Schwellen, Repair-Limits) ist Phase-3-/A-B-C-Arbeit.
/// Regel: technisch fehlgeschlagen → Failed; mind. ein bestätigter/partieller Defekt → Repair;
/// nur unverifizierte Kandidaten oder unvollständige Messung (Partial) -> HumanReview.. sonst Pass.
/// Unverifizierte Findings lösen bewusst **kein** auto-Repair aus (erst Verifikation/Mensch).
/// </remarks>
public sealed class GatePolicy
{
    public const string Version = "placeholder-v1";

    public GateResult Evaluate(ReviewResult review)
    {
        var reasons = new List<string>();

        if (review.Status == ReviewStatus.Failed)
        {
            reasons.Add("ReviewStatus=Failed: Messung technisch nicht belastbar.");
            return new GateResult(GateDecision.Failed, Version, reasons);
        }

        var actionable = review.Defects
            .Count(d => d.VerificationStatus is VerificationStatus.Confirmed or VerificationStatus.Partial);
        if (actionable > 0)
        {
            reasons.Add($"{actionable} bestätigte/partielle Defekte → Repair.");
            return new GateResult(GateDecision.Repair, Version, reasons);
        }

        var unverified = review.Defects.Count(d => d.VerificationStatus == VerificationStatus.Unverified);
        if (unverified > 0 || review.Status == ReviewStatus.Partial)
        {
            if (unverified > 0) reasons.Add($"{unverified} unverifizierte Kandidaten → erst Verifikation/Mensch.");
            if (review.Status == ReviewStatus.Partial) reasons.Add("Messung unvollständig (Partial).");
            return new GateResult(GateDecision.HumanReview, Version, reasons);
        }

        reasons.Add("Keine actionable Defekte, Messung vollständig.");
        return new GateResult(GateDecision.Pass, Version, reasons);
    }
}
