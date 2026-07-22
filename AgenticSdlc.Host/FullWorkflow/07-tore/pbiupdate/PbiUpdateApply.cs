using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

public sealed record PbiUpdateApplyReport(
    IReadOnlyList<string> NewPbis,
    IReadOnlyList<string> UpdatedPbis,
    IReadOnlyDictionary<string, string> FinalStatus,
    int RelationsAdded,
    int RelationsRemoved,
    IReadOnlyList<string> Skipped);

// Deterministischer Update-by-Identity der akzeptierten PBI-Operationen (plan-increment1c3 §7). Mehrere Ops pro PBI
// werden geordnet zusammengefuehrt (MULTI_CAUSE_MERGE): EINE Version++, Status per Praezedenz, alle Gruende in History.
// Unberuehrte PBIs bleiben unangetastet. NEW_PBI praegt eine stabile PBI-<n>. SUPERSEDE_PBI swappt das Requirement.
public static class PbiUpdateApply
{
    public static (ProjectStateDocument Core, PbiUpdateApplyReport Report) Apply(
        ProjectStateDocument core, PbiStateChangePlanDocument plan, ISet<int> acceptedIndices, string sourceRun)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var nextPbi = MaxSuffix(order, "PBI") + 1;
        int relAdded = 0, relRemoved = 0;
        var newPbis = new List<string>();
        var updated = new List<string>();
        var skipped = new List<string>();

        var accepted = plan.Operations.Where((_, i) => acceptedIndices.Contains(i)).ToList();

        // 1) NEW_PBI: neue Core-PBI-Entitaet je Operation.
        foreach (var op in accepted.Where(o => string.Equals(o.Kind, PbiUpdateKind.NewPbi, StringComparison.Ordinal)))
        {
            if (op.FeatureId is null || !byId.ContainsKey(op.FeatureId)) { skipped.Add($"NEW_PBI {op.RequirementId}: feature '{op.FeatureId}' unbekannt"); continue; }
            var id = $"PBI-{nextPbi++:D3}";
            var title = byId.TryGetValue(op.RequirementId, out var rq) ? Truncate(rq.Text, 90) : op.RequirementId;
            var payload = new PbiPayload(
                Goal: null, Title: title, AcceptanceCriteria: [], LinkedRequirementIds: [op.RequirementId],
                OpenDecisionRefs: [], PriorityRank: null, Readiness: PbiStatus.NeedsClarify, Mvp: null, Trace: null);
            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            { ["sourceRunId"] = sourceRun, ["createdFromRequirement"] = op.RequirementId, ["featureId"] = op.FeatureId };
            AddItem(order, byId, new ProjectStateItem(
                ItemId: id, ItemType: "pbi", Text: title, Status: PbiStatus.NeedsClarify, Origin: "pbi-update", Stage: null, Version: 1,
                SourceRunId: sourceRun, SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                SourceClaimIds: [], SourceArtifactItemIds: [], Metadata: meta, IdentityKey: null, History: [], Feature: null, Pbi: payload));
            relations.Add(new ProjectStateRelation(id, op.FeatureId, "part_of_feature", "pbi-update", new Dictionary<string, string>())); relAdded++;
            relations.Add(RequirementSwap.Covers(id, op.RequirementId, "pbi-update")); relAdded++;
            newPbis.Add(id);
        }

        // 2) Bestehende PBIs: mehrere Ursachen geordnet zusammenfuehren.
        foreach (var g in accepted.Where(o => PbiUpdateKind.RequirePbi.Contains(o.Kind) && o.PbiId is not null)
                     .GroupBy(o => o.PbiId!, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(g.Key, out var pbi) || pbi.Pbi is null) { skipped.Add($"{g.Key}: kein PBI"); continue; }

            var links = pbi.Pbi.LinkedRequirementIds.ToList();
            var decRefs = pbi.Pbi.OpenDecisionRefs.ToList();
            var status = pbi.Status;
            var reasons = new List<string>();

            foreach (var op in g)
            {
                reasons.Add($"{op.Kind}: {op.Rationale}");
                switch (op.Kind)
                {
                    case PbiUpdateKind.ExtendPbi:
                        if (!links.Contains(op.RequirementId)) { links.Add(op.RequirementId); relations.Add(RequirementSwap.Covers(pbi.ItemId, op.RequirementId, "pbi-update")); relAdded++; }
                        status = PbiStatus.Max(status, PbiStatus.NeedsClarify);
                        break;
                    case PbiUpdateKind.MarkChanged:
                        status = PbiStatus.Max(status, PbiStatus.NeedsClarify);
                        break;
                    case PbiUpdateKind.BlockPbi:
                        if (op.OpenDecisionRef is not null && !decRefs.Contains(op.OpenDecisionRef)) decRefs.Add(op.OpenDecisionRef);
                        status = PbiStatus.Max(status, PbiStatus.BlockedByDecision);
                        break;
                    case PbiUpdateKind.SupersedePbi:
                        // Geteilter Wahrheitsuebergang (T2.0) — identisch fuer pbi-update und Tor 2 ADOPT_NEW.
                        var (swAdded, swRemoved) = RequirementSwap.SwapCoverage(
                            pbi.ItemId, op.RequirementId, op.ReplacementRequirementId, links, relations, "pbi-update");
                        relAdded += swAdded; relRemoved += swRemoved;
                        status = PbiStatus.Max(status, PbiStatus.NeedsClarify);
                        break;
                }
            }

            var history = (pbi.History ?? []).Append(new ProjectStateItemVersion(
                pbi.Version, pbi.Text, pbi.Status, pbi.Origin, pbi.SourceRunId, pbi.SourceClaimIds, DateTime.UtcNow, string.Join(" | ", reasons))).ToList();
            byId[pbi.ItemId] = pbi with
            {
                Status = status,
                Version = pbi.Version + 1,
                History = history,
                Pbi = pbi.Pbi with { LinkedRequirementIds = links, OpenDecisionRefs = decRefs }
            };
            updated.Add(pbi.ItemId);
        }

        var items = order.Select(id => byId[id]).ToList();
        var finalStatus = newPbis.Concat(updated).ToDictionary(id => id, id => byId[id].Status, StringComparer.Ordinal);
        var coreUpdated = core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items, Relations = relations };
        return (coreUpdated, new PbiUpdateApplyReport(newPbis, updated, finalStatus, relAdded, relRemoved, skipped));
    }

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    { byId[item.ItemId] = item; order.Add(item.ItemId); }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal)).Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit)).Select(int.Parse).DefaultIfEmpty(0).Max();
    }

    private static string Truncate(string value, int max)
    {
        var t = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return t.Length <= max ? t : t[..max] + "...";
    }
}
