using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Deterministische Ableitung der PBI-Operationen aus dem Ingestion-Delta + Core-Relationen (kein LLM):
// REFINE -> MARK_CHANGED · CONTRADICT -> BLOCK_PBI · SUPERSEDE -> SUPERSEDE_PBI (mit Ersatz aus supersedes-Relation).
// Neue Requirements (NEW/NEW_RELATED), die in KEINEM PBI gedeckt sind, gehen als "unplaced" an den Platzierungs-Agenten.
public static class PbiUpdateDerivation
{
    public sealed record UnplacedRequirement(string RequirementId, string? FeatureHint, string Text);
    public sealed record Result(IReadOnlyList<PbiStateChangeOperation> DeterministicOps, IReadOnlyList<UnplacedRequirement> Unplaced);

    public static Result Derive(ProjectStateDocument core, IReadOnlyList<AppliedOperation> delta)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        var pbisCovering = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
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
                    foreach (var pbi in Covering(pbisCovering, d.EntityId))
                        ops.Add(new(PbiUpdateKind.MarkChanged, d.EntityId!, pbi, null, null, null, $"Requirement {d.EntityId} verfeinert"));
                    break;

                case StateChangeKind.Supersede:
                    if (d.EntityId is not null && supersedesTarget.TryGetValue(d.EntityId, out var reqOld))
                        foreach (var pbi in Covering(pbisCovering, reqOld))
                            ops.Add(new(PbiUpdateKind.SupersedePbi, reqOld, pbi, null, d.EntityId, null, $"Requirement {reqOld} ersetzt durch {d.EntityId}"));
                    break;

                case StateChangeKind.Contradict:
                    if (d.EntityId is not null && contradictsTarget.TryGetValue(d.EntityId, out var reqC))
                        foreach (var pbi in Covering(pbisCovering, reqC))
                            ops.Add(new(PbiUpdateKind.BlockPbi, reqC, pbi, null, null, d.EntityId, $"Open Decision {d.EntityId} widerspricht {reqC}"));
                    break;

                case StateChangeKind.New:
                case StateChangeKind.NewRelated:
                    if (d.EntityId is not null && !Covering(pbisCovering, d.EntityId).Any())
                        unplaced.Add(new(d.EntityId, featureOf.GetValueOrDefault(d.EntityId),
                            byId.TryGetValue(d.EntityId, out var it) ? it.Text : ""));
                    break;

                case StateChangeKind.OpenQuestion:
                    // 9g BEWUSST: eine Meeting-Frage (DEC ohne Ziel) hat KEINE Platzierungs-Wirkung — sie blockt
                    // nichts und erzeugt keine PBI-Ops; ihr Kreislauf ist Parkplatz + decision-gate.
                    break;
            }
        }

        return new Result(ops, unplaced);
    }

    private static IEnumerable<string> Covering(IReadOnlyDictionary<string, List<string>> map, string? requirementId)
        => requirementId is not null && map.TryGetValue(requirementId, out var l) ? l : [];
}
