using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Inc 1c-1: hebt Cluster (Features) + PBIs eines re-clarify-Laufs als persistente Core-Entitaeten in den Core.
// Deterministisch, kein LLM. Core = ID-Autoritaet: PBIs bekommen NEUE Core-IDs (PBI-<n>), Features uebernehmen
// ihre FC-<n> AB SEED als Core-ID; alte IDs bleiben als legacyId/sourceRunId (Wiedererkennungs-Anker). Requirement-
// Referenzen der PBIs kommen aus der (bereits enrichten) traceability.l1Req+l3 = Core-Requirement-IDs.
public static class CoreBacklogSeeder
{
    public sealed record Report(int FeaturesAdded, int PbisAdded, int RelationsAdded, int SkippedExisting, int RequirementRefsUnresolved);

    public static (ProjectStateDocument Core, Report Report) Seed(
        ProjectStateDocument core, FeatureClusterSet clusters, ProductBacklogDocument backlog, string sourceRunId)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var coreReqIds = core.Items.Where(IsRequirement).Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);

        // Idempotenz-Anker (primaer legacy): welche Cluster/PBIs sind schon im Core?
        var existingFeatureKeys = core.Items.Where(i => Is(i, "feature"))
            .SelectMany(i => new[] { i.ItemId, i.Metadata.GetValueOrDefault("legacyClusterId") })
            .Where(x => !string.IsNullOrEmpty(x)).ToHashSet(StringComparer.Ordinal)!;
        var existingPbiLegacy = core.Items.Where(i => Is(i, "pbi"))
            .Select(i => i.Metadata.GetValueOrDefault("legacyPbiId"))
            .Where(x => !string.IsNullOrEmpty(x)).ToHashSet(StringComparer.Ordinal)!;

        var clusterCompact = clusters.Clusters
            .GroupBy(c => c.ClusterId.Replace("-", ""), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var nextPbi = MaxSuffix(order, "PBI") + 1;
        int featuresAdded = 0, pbisAdded = 0, relAdded = 0, skipped = 0, unresolved = 0;
        var featureReqs = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        void AddRel(string from, string to, string type)
        {
            relations.Add(new ProjectStateRelation(from, to, type, "core-seed-backlog", new Dictionary<string, string>()));
            relAdded++;
        }

        // 1) PBIs -> neue Core-IDs + Relationen (pbi->feature, pbi->covers->requirement).
        foreach (var p in backlog.Items)
        {
            if (existingPbiLegacy.Contains(p.PbiId)) { skipped++; continue; }

            var featureId = ResolveFeatureId(p.PbiId, clusterCompact);
            var trace = p.Traceability;
            var coreLinked = trace is null ? []
                : trace.L1Req.Concat(trace.L3).Where(coreReqIds.Contains).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (trace is not null)
                unresolved += trace.L1Req.Concat(trace.L3).Distinct(StringComparer.Ordinal).Count(x => !coreReqIds.Contains(x));

            var newId = $"PBI-{nextPbi++:D3}";
            var payload = new PbiPayload(
                Goal: p.Goal, Title: p.Title, AcceptanceCriteria: p.AcceptanceCriteria, LinkedRequirementIds: coreLinked,
                OpenDecisionRefs: [], PriorityRank: p.PriorityRank, Readiness: p.Readiness, Mvp: p.Mvp,
                Trace: trace is null ? null : new PbiTrace(trace.CanonicalRequirementIds, trace.L1Req, trace.L3, trace.Claims));

            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["legacyPbiId"] = p.PbiId,
                ["sourceRunId"] = sourceRunId,
                ["legacyOpenDecisions"] = p.OpenDecisions.Count.ToString()
            };
            if (featureId is not null) meta["featureId"] = featureId;

            AddItem(order, byId, new ProjectStateItem(
                ItemId: newId, ItemType: "pbi", Text: p.Title, Origin: "re-clarify", Stage: null, Version: 1,
                SourceRunId: sourceRunId, SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                SourceClaimIds: trace?.Claims ?? [], SourceArtifactItemIds: [], Metadata: meta,
                IdentityKey: p.IdentityKey, History: [], Feature: null, Pbi: payload).WithStatus(CoreStatus.From("active")));   // §5-S7: Status→Achsen
            pbisAdded++;

            if (featureId is not null)
            {
                AddRel(newId, featureId, "part_of_feature");
                if (!featureReqs.TryGetValue(featureId, out var set)) { set = new(StringComparer.Ordinal); featureReqs[featureId] = set; }
                foreach (var r in coreLinked) set.Add(r);
            }
            foreach (var r in coreLinked) AddRel(newId, r, "covers");
        }

        // 2) Features -> FC-<n> als Core-ID + requirement->part_of_feature (aus den PBIs abgeleitet).
        foreach (var c in clusters.Clusters)
        {
            if (existingFeatureKeys.Contains(c.ClusterId)) { skipped++; continue; }

            var reqs = featureReqs.TryGetValue(c.ClusterId, out var s)
                ? s.OrderBy(x => x, StringComparer.Ordinal).ToList() : [];

            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["legacyClusterId"] = c.ClusterId,
                ["sourceRunId"] = sourceRunId,
                ["legacyCrossCutting"] = string.Join(",", c.CrossCuttingRequirementIds)
            };
            AddItem(order, byId, new ProjectStateItem(
                ItemId: c.ClusterId, ItemType: "feature", Text: c.Label, Origin: "re-clarify", Stage: null, Version: 1,
                SourceRunId: sourceRunId, SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                SourceClaimIds: [], SourceArtifactItemIds: [], Metadata: meta,
                IdentityKey: c.IdentityKey, History: [],
                Feature: new FeaturePayload(c.Label, c.Rationale, reqs, []), Pbi: null).WithStatus(CoreStatus.From("active")));   // §5-S7: Status→Achsen
            featuresAdded++;

            foreach (var r in reqs) AddRel(r, c.ClusterId, "part_of_feature");
        }

        var items = order.Select(id => byId[id]).ToList();
        var updated = core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items, Relations = relations };
        return (updated, new Report(featuresAdded, pbisAdded, relAdded, skipped, unresolved));
    }

    private static string? ResolveFeatureId(string pbiId, IReadOnlyDictionary<string, FeatureCluster> compact)
    {
        // "PBI-FC001-01" -> Segment "FC001" -> Cluster "FC-001".
        var parts = pbiId.Split('-');
        return parts.Length >= 2 && compact.TryGetValue(parts[1], out var c) ? c.ClusterId : null;
    }

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    {
        byId[item.ItemId] = item;
        order.Add(item.ItemId);
    }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal))
            .Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse).DefaultIfEmpty(0).Max();
    }

    private static bool IsRequirement(ProjectStateItem i) => Is(i, "requirement");
    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
}
