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
        IReadOnlyList<string> BlockedPbis)
    {
        public bool Empty => OpenDecisions.Count == 0 && NeedsClarifyPbis.Count == 0 && BlockedPbis.Count == 0;
    }

    public static Report Count(ProjectStateDocument core)
    {
        var openDecs = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase) && i.ReadStatus().IsOpenDecision)
            .Select(i => i.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var pbis = core.Items.Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)).ToList();
        var needsClarify = pbis.Where(p => p.ReadStatus().Blocker == Blocker.NeedsClarify)
            .Select(p => p.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var blocked = pbis.Where(p => p.ReadStatus().Blocker == Blocker.BlockedByDecision)
            .Select(p => p.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        return new Report(openDecs, needsClarify, blocked);
    }

    private const int MaxIds = 6;

    public static IReadOnlyList<string> RenderLines(Report parkplatz, CoreKangal.Report integrity)
    {
        static string Ids(IReadOnlyList<string> ids) => ids.Count == 0 ? "" :
            $" ({string.Join(", ", ids.Take(MaxIds))}{(ids.Count > MaxIds ? ", …" : "")})";

        var lines = new List<string>
        {
            $"  Parkplatz:  {parkplatz.OpenDecisions.Count} offene Entscheidung(en){Ids(parkplatz.OpenDecisions)} · "
                + $"{parkplatz.NeedsClarifyPbis.Count} Klärung(en) (needs_clarify){Ids(parkplatz.NeedsClarifyPbis)} · "
                + $"{parkplatz.BlockedPbis.Count} blockiert{Ids(parkplatz.BlockedPbis)}",
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
