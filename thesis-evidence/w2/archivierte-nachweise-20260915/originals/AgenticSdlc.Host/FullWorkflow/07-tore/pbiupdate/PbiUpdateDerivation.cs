using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Deterministische Ableitung der PBI-Operationen aus dem Ingestion-Delta + Core-Relationen (kein LLM):
// REFINE -> MARK_CHANGED · CONTRADICT -> BLOCK_PBI · SUPERSEDE -> SUPERSEDE_PBI (mit Ersatz aus supersedes-Relation).
// ③ E-8 (06.08.): der Wahrheits-Link ist GENERALISIERT — ein PBI ist betroffen, wenn es das geänderte Item
// DECKT (covers, req) ODER von ihm EINGESCHRÄNKT wird (constrained_by, arch). Damit wecken Rahmen-Änderungen
// dieselben PBI-Ops wie Anforderungs-Änderungen (das Op-Feld RequirementId trägt dann die ARCH-Id).
// Neue Requirements (NEW/NEW_RELATED), die in KEINEM PBI gedeckt sind, gehen als "unplaced" an den
// Platzierungs-Agenten — NUR requirements: ein neues arch-Item ist Wahrheit, keine Arbeit (work-Rolle -> A4).
public static class PbiUpdateDerivation
{
    // A4 (06.08.): Aspect sagt dem Platzierungs-Agenten + der Review ehrlich, WAS platziert wird —
    // "requirement" (fachliche Anforderung) oder "architecture" (technische Arbeit aus einer work-Rolle).
    public sealed record UnplacedRequirement(string RequirementId, string? FeatureHint, string Text, string Aspect = "requirement");
    public sealed record Result(IReadOnlyList<PbiStateChangeOperation> DeterministicOps, IReadOnlyList<UnplacedRequirement> Unplaced);

    public static Result Derive(ProjectStateDocument core, IReadOnlyList<AppliedOperation> delta)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        var pbisLinked = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal)
                     || string.Equals(r.RelationType, ConstraintSwap.Relation, StringComparison.Ordinal))
            .GroupBy(r => r.ToId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(r => r.FromId).Distinct(StringComparer.Ordinal).ToList(), StringComparer.Ordinal);

        var contradictsTarget = core.Relations
            .Where(r => string.Equals(r.RelationType, "contradicts", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var supersedesTarget = core.Relations
            .Where(r => string.Equals(r.RelationType, "supersedes", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var featureOf = core.Relations
            .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                        && byId.TryGetValue(r.FromId, out var it) && string.Equals(it.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .GroupBy(r => r.FromId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var ops = new List<PbiStateChangeOperation>();
        var unplaced = new List<UnplacedRequirement>();

        foreach (var d in delta)
        {
            switch (d.Kind)
            {
                case StateChangeKind.Refine:
                    foreach (var pbi in Covering(pbisLinked, d.EntityId))
                        ops.Add(new(PbiUpdateKind.MarkChanged, d.EntityId!, pbi, null, null, null, $"Requirement {d.EntityId} verfeinert"));
                    break;

                case StateChangeKind.Supersede:
                    if (d.EntityId is not null && supersedesTarget.TryGetValue(d.EntityId, out var reqOld))
                        foreach (var pbi in Covering(pbisLinked, reqOld))
                            ops.Add(new(PbiUpdateKind.SupersedePbi, reqOld, pbi, null, d.EntityId, null, $"Requirement {reqOld} ersetzt durch {d.EntityId}"));
                    break;

                case StateChangeKind.Contradict:
                    if (d.EntityId is not null && contradictsTarget.TryGetValue(d.EntityId, out var reqC))
                        foreach (var pbi in Covering(pbisLinked, reqC))
                            ops.Add(new(PbiUpdateKind.BlockPbi, reqC, pbi, null, null, d.EntityId, $"Open Decision {d.EntityId} widerspricht {reqC}"));
                    break;

                case StateChangeKind.New:
                case StateChangeKind.NewRelated:
                    // ③: unplaced ist ARBEITS-Semantik — nur neue requirements gehen zum Platzierungs-Agenten.
                    if (d.EntityId is not null && !Covering(pbisLinked, d.EntityId).Any()
                        && byId.TryGetValue(d.EntityId, out var it)
                        && string.Equals(it.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
                        unplaced.Add(new(d.EntityId, featureOf.GetValueOrDefault(d.EntityId), it.Text));
                    break;

                case StateChangeKind.OpenQuestion:
                    // 9g BEWUSST: eine Meeting-Frage (DEC ohne Ziel) hat KEINE Platzierungs-Wirkung — sie blockt
                    // nichts und erzeugt keine PBI-Ops; ihr Kreislauf ist Parkplatz + decision-gate.
                    break;
            }
        }

        // A4 (06.08., work-Rolle → Arbeit): unabgedeckte work-archs gehen an DENSELBEN Platzierungs-Agenten.
        // Quell-agnostischer Rückstau-Scan (T2.21: Core, nicht Delta) — aber NUR in Läufen, die arch berührt
        // haben (Smoke-/Leer-Lauf-Schutz, dieselbe Lehre wie die Classify-Bridge 06.08.); die Grenze ist
        // dokumentiert: reine Backlog-Klassifikation ohne arch-Delta füttert das Placement erst im nächsten
        // arch-aktiven Lauf (Zwei-Bahnen-Regel, bewusste Ausnahme).
        var archActive = delta.Any(d => d.EntityId is not null
            && byId.TryGetValue(d.EntityId, out var t)
            && string.Equals(t.ItemType, "architecture", StringComparison.OrdinalIgnoreCase));
        if (archActive)
            foreach (var work in UncoveredWorkArchs(core))
                unplaced.Add(new(work.ItemId, null, work.Text, Aspect: "architecture"));

        return new Result(ops, unplaced);
    }

    /// <summary>A4: aktive arch-Items mit work-Rolle, die noch KEIN PBI deckt — Deckungs-Begriff ist NUR
    /// `covers` (eine constrained_by-Kante aus der constraint-Rolle ist Wirkung, keine Umsetzung!).
    /// Idempotent: einmal platzierte Arbeit wird nie erneut vorgelegt (dieselbe Form wie der Classify-Scan).</summary>
    public static IReadOnlyList<ProjectStateItem> UncoveredWorkArchs(ProjectStateDocument core)
    {
        var covered = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .Select(r => r.ToId)
            .ToHashSet(StringComparer.Ordinal);
        return core.Items
            .Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .Where(i => i.Architecture?.Roles.Contains("work", StringComparer.Ordinal) == true)
            .Where(i => !covered.Contains(i.ItemId))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<string> Covering(IReadOnlyDictionary<string, List<string>> map, string? requirementId)
        => requirementId is not null && map.TryGetValue(requirementId, out var l) ? l : [];
}
