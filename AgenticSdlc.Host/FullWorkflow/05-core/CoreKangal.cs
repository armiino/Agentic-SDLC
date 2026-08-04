using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// R-33 S4 — der WACHHUND am Core-Port (Autor-Taufe 04.08.: Kangal, der Herdenschutzhund). EINE Prüf-Logik,
// EINE Naht: Check läuft in JsonCoreRepository.SaveAsync VOR Snapshot+Write — damit ist es egal, welcher der
// 9 Schreiber (oder ein künftiger Steward-Pfad) schreibt: kaputte Wahrheit wird nie persistiert.
// Invarianten-Spec: docs/aktiv/core-relationen-konzept.md §3 + §5.3 (Härtegrade).
//   Fehler (Save bricht ab): I1 Struktur-Relation auf fehlendes/typfalsches Item · I5 >1 Issue-Mapping je PBI.
//   Warnung (laut, blockt nie): I2 PBI ohne Feature · I3 aktives REQ ohne Deckung (feuert BEWUSST im
//   Übergangszustand zwischen Ingest- und Pbi-Gate) · I4 covers→superseded (= R-34) · I6 contradicts-Lebenszyklus.
// Endpunkt-Matrix am realen Core verifiziert (04.08.): part_of_feature pbi/req→feature · covers pbi→req ·
// supersedes req→req · contradicts(_resolved) dec→Item · Extern-Ziele by design (Audit: 173 gewollte).
public static class CoreKangal
{
    public sealed record Issue(string Code, string Severity, string Message);

    public sealed record Report(IReadOnlyList<Issue> Errors, IReadOnlyList<Issue> Warnings)
    {
        public bool Pass => Errors.Count == 0;
    }

    // Relationen mit by-design EXTERNEM Ziel (Claims, gh#n, HDEC, CAND) — der Hund bellt keine Nachbarn an.
    private static readonly HashSet<string> ExternalTargetRelations = new(StringComparer.Ordinal)
    {
        "implemented_by_issue", "evidenced_by_ledger_claim", "accepted_by_human_decision", "promoted_from_l3_candidate",
    };

    public static Report Check(ProjectStateDocument core)
    {
        var errors = new List<Issue>();
        var warnings = new List<Issue>();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        bool Is(string id, string type) => byId.TryGetValue(id, out var it) && string.Equals(it.ItemType, type, StringComparison.OrdinalIgnoreCase);

        // I1 — jede Struktur-Relation zeigt auf existierende Items korrekten Typs.
        foreach (var r in core.Relations)
        {
            if (!byId.ContainsKey(r.FromId))
            {
                errors.Add(Error("I1_SOURCE_MISSING", $"{r.RelationType} von '{r.FromId}' nach '{r.ToId}': Quelle ist kein Core-Item."));
                continue;
            }
            if (ExternalTargetRelations.Contains(r.RelationType)) continue;

            switch (r.RelationType)
            {
                case "part_of_feature":
                    if (!Is(r.ToId, "feature"))
                        errors.Add(Error("I1_TARGET_INVALID", $"part_of_feature von '{r.FromId}': Ziel '{r.ToId}' ist kein Feature-Item."));
                    break;
                case "covers":
                    if (!Is(r.FromId, "pbi"))
                        errors.Add(Error("I1_SOURCE_TYPE", $"covers von '{r.FromId}': Quelle ist kein PBI."));
                    if (!Is(r.ToId, "requirement"))
                        errors.Add(Error("I1_TARGET_INVALID", $"covers von '{r.FromId}': Ziel '{r.ToId}' ist kein Requirement."));
                    break;
                case "supersedes":
                    if (!Is(r.ToId, "requirement"))
                        errors.Add(Error("I1_TARGET_INVALID", $"supersedes von '{r.FromId}': Ziel '{r.ToId}' ist kein Requirement."));
                    break;
                case "contradicts":
                case "contradicts_resolved":
                    if (!Is(r.FromId, "decision"))
                        errors.Add(Error("I1_SOURCE_TYPE", $"{r.RelationType} von '{r.FromId}': Quelle ist keine Decision."));
                    if (!byId.ContainsKey(r.ToId))
                        errors.Add(Error("I1_TARGET_INVALID", $"{r.RelationType} von '{r.FromId}': Ziel '{r.ToId}' ist kein Core-Item."));
                    break;
                default:
                    warnings.Add(Warn("UNKNOWN_RELATION_TYPE", $"Unbekannter Relationstyp '{r.RelationType}' ('{r.FromId}' -> '{r.ToId}') — nicht geprüft."));
                    break;
            }
        }

        // I5 — implemented_by_issue ist ein 1:1-Mapping: max. eine Issue-Relation je PBI.
        foreach (var g in core.Relations
                     .Where(r => string.Equals(r.RelationType, "implemented_by_issue", StringComparison.Ordinal))
                     .GroupBy(r => r.FromId, StringComparer.Ordinal)
                     .Where(g => g.Count() > 1))
            errors.Add(Error("I5_MULTIPLE_ISSUE_MAPPINGS", $"PBI '{g.Key}' hat {g.Count()} implemented_by_issue-Relationen (max. 1)."));

        // I2 — jedes PBI gehört zu einem Feature.
        var pbiWithFeature = core.Relations
            .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal))
            .Select(r => r.FromId).ToHashSet(StringComparer.Ordinal);
        foreach (var pbi in core.Items.Where(i => Is(i.ItemId, "pbi") && !pbiWithFeature.Contains(i.ItemId)))
            warnings.Add(Warn("I2_PBI_WITHOUT_FEATURE", $"PBI '{pbi.ItemId}' hat keine part_of_feature-Relation."));

        // I3 — jedes AKTIVE Requirement ist gedeckt. Feuert bewusst (nur Warnung) im Übergang Ingest→Pbi-Gate.
        var covered = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .Select(r => r.ToId).ToHashSet(StringComparer.Ordinal);
        foreach (var req in core.Items.Where(i => Is(i.ItemId, "requirement")
                     && i.ReadStatus().Validity == Validity.Active && !covered.Contains(i.ItemId)))
            warnings.Add(Warn("I3_ACTIVE_REQ_UNCOVERED", $"Aktives Requirement '{req.ItemId}' ist von keinem PBI gedeckt."));

        // I4 (= R-34) — kein covers auf ein superseded Requirement.
        foreach (var r in core.Relations.Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal)))
            if (byId.TryGetValue(r.ToId, out var target) && string.Equals(target.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
                && target.ReadStatus().Validity == Validity.Superseded)
                warnings.Add(Warn("I4_COVERS_SUPERSEDED", $"PBI '{r.FromId}' deckt superseded Requirement '{r.ToId}' (Swap fehlt, R-34)."));

        // I6 — contradicts-Lebenszyklus: offene DEC ↔ contradicts, aufgelöste ↔ contradicts_resolved.
        foreach (var r in core.Relations)
        {
            if (!byId.TryGetValue(r.FromId, out var from) || !string.Equals(from.ItemType, "decision", StringComparison.OrdinalIgnoreCase)) continue;
            var open = from.ReadStatus().IsOpenDecision;
            if (string.Equals(r.RelationType, "contradicts", StringComparison.Ordinal) && !open)
                warnings.Add(Warn("I6_CONTRADICTS_LIFECYCLE", $"Nicht-offene Decision '{r.FromId}' hält contradicts auf '{r.ToId}' (erwartet: contradicts_resolved)."));
            if (string.Equals(r.RelationType, "contradicts_resolved", StringComparison.Ordinal) && open)
                warnings.Add(Warn("I6_CONTRADICTS_LIFECYCLE", $"Offene Decision '{r.FromId}' hält contradicts_resolved auf '{r.ToId}'."));
        }

        return new Report(errors, warnings);
    }

    private static Issue Error(string code, string message) => new(code, "error", message);
    private static Issue Warn(string code, string message) => new(code, "warning", message);
}
