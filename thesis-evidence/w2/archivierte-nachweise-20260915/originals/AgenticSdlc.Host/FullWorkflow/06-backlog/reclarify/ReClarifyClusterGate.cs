using AgenticSdlc.Host.FullWorkflow.Core;

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
                errors.Add(Error("EMPTY_CLUSTER", $"Cluster {c.ClusterId} hat keine coreRequirementIds.", c.ClusterId, []));

            foreach (var id in c.CoreRequirementIds)
            {
                coreCount[id] = coreCount.TryGetValue(id, out var n) ? n + 1 : 1;
                if (!allReqIds.Contains(id))
                    errors.Add(Error("UNKNOWN_REQUIREMENT", $"coreRequirementId {id} existiert nicht in der Baseline.", c.ClusterId, [id]));
            }
            foreach (var id in c.CrossCuttingRequirementIds)
                if (!allReqIds.Contains(id))
                    errors.Add(Error("UNKNOWN_REQUIREMENT", $"crossCuttingRequirementId {id} existiert nicht in der Baseline.", c.ClusterId, [id]));
        }

        // Coverage-Invariante: jedes nicht-stillgelegte Requirement genau einmal CORE.
        foreach (var id in mustCover.OrderBy(x => x, StringComparer.Ordinal))
        {
            var n = coreCount.TryGetValue(id, out var c) ? c : 0;
            if (n == 0)
                errors.Add(Error("UNPLACED_REQUIREMENT", $"Requirement {id} ist in keinem Cluster core (Recall-Verletzung).", null, [id]));
            else if (n > 1)
                errors.Add(Error("DUPLICATE_CORE", $"Requirement {id} ist in {n} Clustern core (muss genau eins sein).", null, [id]));
        }

        // Querschnitt sollte irgendwo core sein (sonst nur referenziert, aber nie geplant).
        var coreSet = coreCount.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var c in clusters)
            foreach (var id in c.CrossCuttingRequirementIds)
                if (allReqIds.Contains(id) && !coreSet.Contains(id))
                    warnings.Add(Warn("CROSSCUTTING_NOT_CORE", $"crossCutting {id} ist in keinem Cluster core.", c.ClusterId, [id]));

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

    // R-33 S0: Severity + Repairability werden AN DER REGEL deklariert (eine Stelle je Regel, kein Neben-
    // Register). Alle heutigen Regeln sind Plan-Qualitaet des Cluster-Agenten (Gruppierungs-Fehler) -> per
    // GateFeedback fixbar; eine kuenftig nicht-reparierbare Regel deklariert das explizit am Aufruf.
    private static ReClarifyGateIssue Error(string code, string message, string? subjectId,
        IReadOnlyList<string> requirementIds, string repairability = Repairability.Repairable)
        => new(code, "error", message, subjectId, requirementIds, repairability);

    private static ReClarifyGateIssue Warn(string code, string message, string? subjectId, IReadOnlyList<string> requirementIds)
        => new(code, "warning", message, subjectId, requirementIds, Repairability.Repairable);

    public static GateDecision Decide(ReClarifyGateReport report, int attempt, int maxAttempts)
        => GateLoop.Decide(report.Pass,
            report.Errors.Any(e => string.Equals(e.Repairability, Repairability.Repairable, StringComparison.Ordinal)),
            attempt, maxAttempts);

    private static bool IsRetired(string status)
        => string.Equals(status, "deprecated", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "superseded", StringComparison.OrdinalIgnoreCase);
}
