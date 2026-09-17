using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// A′ Schritt 1 (steward/graph-entry-vs-werkbank.md §9/§10/§11) — der GETEILTE, deterministische Kern des
// clarify-Graph-Eingangs. Genutzt vom Standalone-Sweep (Werkbank) UND — ab Schritt 2 — vom durablen Graph-Eingang
// (Betrieb): EIN Ort für „Antworten → Plan → Alignment → PFLICHT-Checker". Bewusst frei von Host/LLM/settings —
// die LLM-Alignment-Ausführung wird als Naht INJIZIERT (Betrieb: PbiAnswerAlignment.Llm; Test: Fake). Der Verdict
// ist NIE hardcodiert: Report/Decision kommen wörtlich aus PbiUpdateGate.Check/Decide (Governance-Leitplanke A′).

/// <summary>Injizierbare LLM-Naht: Ziel-PBIs (mit Antwort-Trigger) → inhaltliche Angleichungs-Vorschläge.
/// Hält <see cref="ClarifyEntryPlan"/> settings-/LLM-frei und damit testbar.</summary>
public delegate Task<IReadOnlyList<PbiAlignment>> AnswerAlignSeam(IReadOnlyList<PbiAlignTarget> targets, CancellationToken ct);

/// <summary>A′ Schritt 2 — der typisierte GRAPH-Eingangs-Vertrag für den clarify-Weg: eine BATCH von Autor-
/// Antworten (Batch-Regel §4 — EIN Lauf, EIN Gate, EIN Apply, EIN Forward für alle betroffenen PBIs).</summary>
public sealed record ClarifySweepInput(IReadOnlyList<ClarifySweepAnswer> Answers);

/// <summary>Ergebnis der Plan-Erzeugung inkl. des ECHTEN Checker-Befunds (nie hardcodiert):
/// <see cref="Report"/>/<see cref="Decision"/> stammen wörtlich aus <see cref="PbiUpdateGate"/>.</summary>
public sealed record ClarifyValidation(PbiStateChangePlanDocument Plan, PbiUpdateGateReport Report, GateDecision Decision)
{
    public bool Pass => Report.Pass;
}

public static class ClarifyEntryPlan
{
    // clarify kennt KEINE unplaced Requirements (nur MARK_CHANGED auf bestehende PBIs) — der Checker braucht die
    // Menge trotzdem (gemeinsame Signatur mit dem operativen Placement-Pfad).
    private static readonly IReadOnlySet<string> NoUnplaced = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Antworten → deterministischer Plan (<see cref="ClarifySweepPlanBuilder"/>) → Alignment über die
    /// injizierte Naht. Ist nichts Gültiges dabei (leerer Plan), wird KEIN LLM-Schritt gefahren.</summary>
    public static async Task<(PbiStateChangePlanDocument Plan, IReadOnlyList<PbiAlignTarget> Targets, IReadOnlyList<string> Skipped)>
        AssembleAsync(ProjectStateDocument core, IReadOnlyList<ClarifySweepAnswer> answers, string runId,
                      AnswerAlignSeam align, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(align);
        var (plan, targets, skipped) = ClarifySweepPlanBuilder.Build(core, answers, runId);
        if (plan.Operations.Count == 0) return (plan, targets, skipped);   // nichts Gültiges — kein LLM-Schritt
        var alignments = await align(targets, ct).ConfigureAwait(false);
        return (plan with { Alignments = alignments.Count > 0 ? alignments : null }, targets, skipped);
    }

    /// <summary>PFLICHT-Checker (Governance-Leitplanke A′): deterministische Validierung des FERTIGEN Plans →
    /// ECHTER Befund. Kein glattgezogenes Pass — liefert der Checker Repair/HumanReview, steht das im Verdict.</summary>
    public static ClarifyValidation Validate(ProjectStateDocument core, PbiStateChangePlanDocument plan, int maxAttempts = 1)
    {
        var report = PbiUpdateGate.Check(core, plan, NoUnplaced);
        var decision = PbiUpdateGate.Decide(report, attempt: 1, maxAttempts);
        return new ClarifyValidation(plan, report, decision);
    }

    /// <summary>A′ Schritt 2 — baut den internen <c>PbiUpdateVerdict</c> für den Graph-Schwanz (pbiFinalize/pbiPort/
    /// pbiApply) NUR aus einer bereits erhobenen <see cref="ClarifyValidation"/>. **Es gibt bewusst KEINEN
    /// plan→verdict-Shortcut** — so ist die Checker-Leitplanke strukturell erzwungen (der Checker MUSS gelaufen
    /// sein, sonst gibt es keine Validation). Report/Decision werden wörtlich durchgereicht (nie neu geraten).</summary>
    internal static PbiUpdateVerdict AsVerdict(ClarifyValidation validation, PbiUpdateWfContext ctx)
    {
        var attempt = new GateAttempt(1, "clarify-entry", validation.Report.Pass, validation.Decision.ToString(),
            validation.Report.Errors.Select(e => $"{e.Code}({e.Repairability})").ToList(), DateTime.UtcNow);
        return new PbiUpdateVerdict(ctx, Unplaced: [], validation.Plan, validation.Report, validation.Decision,
            Attempt: 1, UnplacedCount: 0, History: [attempt]);
    }
}
