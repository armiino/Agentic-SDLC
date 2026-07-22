namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Deterministisches, STRUKTURELLES Coverage-Gate fuer die agentische Cluster-Bildung.
// Es beweist KEINE fachliche Richtigkeit (das ist Sache von ReviewAgent + Human Review),
// sondern garantiert den RECALL: kein aktives Requirement geht still verloren.
public static class ReClarifyClusterGate
{
    public static ReClarifyGateReport Check(CanonicalRequirementsBaseline baseline, IReadOnlyList<FeatureCluster> clusters)
    {
        var errors = new List<ReClarifyGateIssue>();
        var warnings = new List<ReClarifyGateIssue>();

        var allReqIds = baseline.Requirements.Select(r => r.RequirementId).ToHashSet(StringComparer.Ordinal);
        var mustCover = baseline.Requirements
            .Where(r => !IsRetired(r.Status))
            .Select(r => r.RequirementId)
            .ToHashSet(StringComparer.Ordinal);

        var coreCount = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var c in clusters)
        {
            if (c.CoreRequirementIds.Count == 0)
                errors.Add(new("EMPTY_CLUSTER", "error", $"Cluster {c.ClusterId} hat keine coreRequirementIds.", c.ClusterId, []));

            foreach (var id in c.CoreRequirementIds)
            {
                coreCount[id] = coreCount.TryGetValue(id, out var n) ? n + 1 : 1;
                if (!allReqIds.Contains(id))
                    errors.Add(new("UNKNOWN_REQUIREMENT", "error", $"coreRequirementId {id} existiert nicht in der Baseline.", c.ClusterId, [id]));
            }
            foreach (var id in c.CrossCuttingRequirementIds)
                if (!allReqIds.Contains(id))
                    errors.Add(new("UNKNOWN_REQUIREMENT", "error", $"crossCuttingRequirementId {id} existiert nicht in der Baseline.", c.ClusterId, [id]));
        }

        // Coverage-Invariante: jedes nicht-stillgelegte Requirement genau einmal CORE.
        foreach (var id in mustCover.OrderBy(x => x, StringComparer.Ordinal))
        {
            var n = coreCount.TryGetValue(id, out var c) ? c : 0;
            if (n == 0)
                errors.Add(new("UNPLACED_REQUIREMENT", "error", $"Requirement {id} ist in keinem Cluster core (Recall-Verletzung).", null, [id]));
            else if (n > 1)
                errors.Add(new("DUPLICATE_CORE", "error", $"Requirement {id} ist in {n} Clustern core (muss genau eins sein).", null, [id]));
        }

        // Querschnitt sollte irgendwo core sein (sonst nur referenziert, aber nie geplant).
        var coreSet = coreCount.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var c in clusters)
            foreach (var id in c.CrossCuttingRequirementIds)
                if (allReqIds.Contains(id) && !coreSet.Contains(id))
                    warnings.Add(new("CROSSCUTTING_NOT_CORE", "warning", $"crossCutting {id} ist in keinem Cluster core.", c.ClusterId, [id]));

        var pass = errors.Count == 0;
        var checks = new Dictionary<string, object>
        {
            ["requirements"] = baseline.Requirements.Count,
            ["mustCover"] = mustCover.Count,
            ["clusters"] = clusters.Count,
            ["coreAssignments"] = coreCount.Values.Sum(),
        };
        return new ReClarifyGateReport(pass, pass ? "pass" : "fail", errors, warnings, checks);
    }

    private static bool IsRetired(string status)
        => string.Equals(status, "deprecated", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "superseded", StringComparison.OrdinalIgnoreCase);
}
