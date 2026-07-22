namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record ClusterApplyResult(
    FeatureClusterSet Updated,
    ReClarifyGateReport Gate,
    IReadOnlyList<string> AppliedOpIds,
    IReadOnlyList<string> SkippedOps,
    IReadOnlyList<string> RemovedClusters);

// Deterministische Ausfuehrung der vom Menschen akzeptierten Cluster-Operationen + erneuter Coverage-Check.
// Analog l4-issuplanning-apply: kein Agent, keine Interpretation - nur die adjudizierten Aenderungen.
public static class ReClarifyClusterApply
{
    public static ClusterApplyResult Apply(
        CanonicalRequirementsBaseline baseline,
        FeatureClusterSet clusters,
        IReadOnlyList<ClusterOperation> operations,
        ISet<string> acceptedOpIds)
    {
        var order = clusters.Clusters.Select(c => c.ClusterId).ToList();
        var map = clusters.Clusters.ToDictionary(c => c.ClusterId, c => new Mutable(c), StringComparer.Ordinal);
        var applied = new List<string>();
        var skipped = new List<string>();

        foreach (var op in operations.Where(o => acceptedOpIds.Contains(o.OpId)))
        {
            var err = TryApply(op, map, order);
            if (err is null) applied.Add(op.OpId);
            else skipped.Add($"{op.OpId}: {err}");
        }

        var removed = new List<string>();
        foreach (var id in order.ToList())
            if (map[id].Core.Count == 0)
            {
                removed.Add(id);
                map.Remove(id);
                order.Remove(id);
            }

        var updatedClusters = order.Select(id => map[id].ToRecord()).ToList();
        var updated = clusters with
        {
            ClusterSetId = $"cluster-set-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            CreatedUtc = DateTime.UtcNow,
            Clusters = updatedClusters
        };
        var gate = ReClarifyClusterGate.Check(baseline, updatedClusters);
        return new ClusterApplyResult(updated, gate, applied, skipped, removed);
    }

    private static string? TryApply(ClusterOperation op, Dictionary<string, Mutable> map, List<string> order)
    {
        switch (op.Kind)
        {
            case "add_crosscutting":
                if (op.RequirementId is null || op.ToClusterId is null) return "requirementId/toClusterId fehlt";
                if (!map.TryGetValue(op.ToClusterId, out var addC)) return $"Cluster {op.ToClusterId} unbekannt";
                if (!addC.Cross.Contains(op.RequirementId)) addC.Cross.Add(op.RequirementId);
                return null;
            case "remove_crosscutting":
                if (op.RequirementId is null || op.FromClusterId is null) return "requirementId/fromClusterId fehlt";
                if (!map.TryGetValue(op.FromClusterId, out var remC)) return $"Cluster {op.FromClusterId} unbekannt";
                remC.Cross.Remove(op.RequirementId);
                return null;
            case "move_core":
                if (op.RequirementId is null || op.FromClusterId is null || op.ToClusterId is null) return "requirementId/from/to fehlt";
                if (!map.TryGetValue(op.FromClusterId, out var mFrom)) return $"Cluster {op.FromClusterId} unbekannt";
                if (!map.TryGetValue(op.ToClusterId, out var mTo)) return $"Cluster {op.ToClusterId} unbekannt";
                mFrom.Core.Remove(op.RequirementId);
                if (!mTo.Core.Contains(op.RequirementId)) mTo.Core.Add(op.RequirementId);
                return null;
            case "new_cluster":
                if (string.IsNullOrWhiteSpace(op.NewClusterId) || op.CoreRequirementIds.Count == 0) return "newClusterId/coreRequirementIds fehlt";
                if (map.ContainsKey(op.NewClusterId!)) return $"Cluster {op.NewClusterId} existiert bereits";
                var nc = new Mutable(op.NewClusterId!, op.IdentityKey ?? op.NewClusterId!, op.Label ?? op.NewClusterId!, op.Rationale);
                foreach (var rid in op.CoreRequirementIds)
                {
                    foreach (var m in map.Values) m.Core.Remove(rid);
                    if (!nc.Core.Contains(rid)) nc.Core.Add(rid);
                }
                map[op.NewClusterId!] = nc;
                order.Add(op.NewClusterId!);
                return null;
            case "merge_clusters":
                if (op.FromClusterId is null || op.ToClusterId is null) return "from/to fehlt";
                if (!map.TryGetValue(op.FromClusterId, out var gFrom)) return $"Cluster {op.FromClusterId} unbekannt";
                if (!map.TryGetValue(op.ToClusterId, out var gTo)) return $"Cluster {op.ToClusterId} unbekannt";
                foreach (var rid in gFrom.Core) if (!gTo.Core.Contains(rid)) gTo.Core.Add(rid);
                foreach (var rid in gFrom.Cross) if (!gTo.Cross.Contains(rid)) gTo.Cross.Add(rid);
                map.Remove(op.FromClusterId);
                order.Remove(op.FromClusterId);
                return null;
            default:
                return $"unbekannte Operation {op.Kind}";
        }
    }

    private sealed class Mutable
    {
        public string Id;
        public string Key;
        public string Label;
        public string? Rationale;
        public List<string> Core;
        public List<string> Cross;

        public Mutable(FeatureCluster c)
        {
            Id = c.ClusterId;
            Key = c.IdentityKey;
            Label = c.Label;
            Rationale = c.Rationale;
            Core = c.CoreRequirementIds.ToList();
            Cross = c.CrossCuttingRequirementIds.ToList();
        }

        public Mutable(string id, string key, string label, string? rationale)
        {
            Id = id;
            Key = key;
            Label = label;
            Rationale = rationale;
            Core = [];
            Cross = [];
        }

        public FeatureCluster ToRecord() => new(Id, Key, Label, Core, Cross, Rationale);
    }
}
