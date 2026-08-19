using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// R-14 D4 (r14-entscheidung.md §9): der SICHTBARE Parkplatz — „unsichtbar = rottet" (Autor). Pure Zaehlung der
// fachlich UNERLEDIGTEN Zustaende in einer GUELTIGEN Wahrheit (der Kangal hat diese Saves durchgewunken):
// offene Entscheidungen (DEC) · Klaerungen (needs_clarify-PBIs) · blockierte PBIs. Konsument: pipeline-full status.
public static class CoreParkplatz
{
    public sealed record Report(
        IReadOnlyList<string> OpenDecisions,
        IReadOnlyList<string> NeedsClarifyPbis,
        IReadOnlyList<string> BlockedPbis,
        IReadOnlyDictionary<string, int>? OpenDecisionsByOrigin = null,
        // C4d (K12): wartende Gate-Vorschläge aus der Pending-Registry — Läufe/Status ZEIGEN sie nur.
        IReadOnlyList<string>? PendingReviews = null)
    {
        public bool Empty => OpenDecisions.Count == 0 && NeedsClarifyPbis.Count == 0 && BlockedPbis.Count == 0;
    }

    // Klartext je DEC-Herkunft (Autor-Wunsch 05.08.: Herkunft AUF EINEN BLICK — in der Anzeige, nie in der ID).
    private static string OriginLabel(string origin) => origin switch
    {
        "INGESTION_CONTRADICTION" => "Widerspruch",
        Decision.DecisionRequestMint.Origin => "vom pbi-Gate",
        Decision.MeetingQuestionMint.Origin => "Meeting-Frage",
        Decision.MeetingQuestionMint.OriginAuthor => "Autor-Frage",
        Decision.MeetingQuestionMint.OriginGithub => "GitHub-Frage",
        _ => origin,
    };

    public static Report Count(ProjectStateDocument core)
    {
        var openDecItems = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase) && i.ReadStatus().IsOpenDecision)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList();
        var openDecs = openDecItems.Select(i => i.ItemId).ToList();
        var byOrigin = openDecItems
            .GroupBy(i => OriginLabel(i.Origin), StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var pbis = core.Items.Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)).ToList();
        var needsClarify = pbis.Where(p => p.ReadStatus().Blocker == Blocker.NeedsClarify)
            .Select(p => p.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var blocked = pbis.Where(p => p.ReadStatus().Blocker == Blocker.BlockedByDecision)
            .Select(p => p.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var pending = PendingReviewRegistry.ListOpen(core)
            .Select(e => $"{e.ProposalId} [{e.Bahn}]{(e.Ueberholt ? $" ÜBERHOLT: {e.UeberholtGrund}" : "")} → {e.GateCommand}")
            .ToList();
        return new Report(openDecs, needsClarify, blocked, byOrigin, pending);
    }

    private const int MaxIds = 6;

    public static IReadOnlyList<string> RenderLines(Report parkplatz, CoreKangal.Report integrity)
    {
        static string Ids(IReadOnlyList<string> ids) => ids.Count == 0 ? "" :
            $" ({string.Join(", ", ids.Take(MaxIds))}{(ids.Count > MaxIds ? ", …" : "")})";

        var lines = new List<string>
        {
            $"  Parkplatz:  {parkplatz.OpenDecisions.Count} offene Entscheidung(en){Ids(parkplatz.OpenDecisions)}"
                + (parkplatz.OpenDecisionsByOrigin is { Count: > 0 } o
                    ? $" [{string.Join(" · ", o.Select(kv => $"{kv.Value} {kv.Key}"))}]" : "")
                + " · "
                + $"{parkplatz.NeedsClarifyPbis.Count} Klärung(en) (needs_clarify){Ids(parkplatz.NeedsClarifyPbis)} · "
                + $"{parkplatz.BlockedPbis.Count} blockiert{Ids(parkplatz.BlockedPbis)}"
                + (parkplatz.PendingReviews is { Count: > 0 } pr ? $" · {pr.Count} wartende(s) Review(s)" : ""),
            $"  Integrität: Kangal {integrity.Errors.Count} Fehler · {integrity.Warnings.Count} Warnung(en)"
                + (integrity.Warnings.Count > 0 ? $" — {string.Join("; ", integrity.Warnings.Take(3).Select(w => w.Code))}" : ""),
        };
        if (parkplatz.OpenDecisions.Count > 0)
            lines.Add("  Auflösen:   Entscheidungen → nächster Lauf pausiert am decision-gate (decision-gate-review)");
        if (parkplatz.NeedsClarifyPbis.Count > 0)
            lines.Add("  Auflösen:   Klärungen → Angleichung am pbi-Gate; falls Stakeholder nötig: Option ‚→ Entscheidung‘ (D2)");
        return lines;
    }
}
