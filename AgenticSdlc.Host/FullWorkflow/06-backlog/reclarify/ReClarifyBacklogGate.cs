using AgenticSdlc.Host.FullWorkflow.Core;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Deterministisches, STRUKTURELLES Gate fuer die aus Feature-Clustern geschnittenen PBIs (Definition of Ready).
// Beweist KEINE fachliche Vollstaendigkeit (das ist HumanReview) - garantiert: kein Cluster-Core geht verloren,
// und jedes PBI ist entweder pruefbar (Akzeptanzkriterien) ODER macht seine Luecken explizit (openDecisions).
public static class ReClarifyBacklogGate
{
    private static readonly HashSet<string> Types = new(StringComparer.OrdinalIgnoreCase)
        { "delivery", "clarification", "deferred", "out_of_scope" };
    private static readonly HashSet<string> Readiness = new(StringComparer.OrdinalIgnoreCase)
        { "backlog_ready", "ready_with_nonblocking_questions", "blocked_by_decision" };

    public static ReClarifyGateReport Check(FeatureClusterSet clusters, CanonicalRequirementsBaseline baseline, ProductBacklogDocument backlog)
    {
        var errors = new List<ReClarifyGateIssue>();
        var warnings = new List<ReClarifyGateIssue>();
        var allReqIds = baseline.Requirements.Select(r => r.RequirementId).ToHashSet(StringComparer.Ordinal);

        // Cluster-Cores muessen von PBIs abgedeckt sein (kein Feature-Kern faellt beim Schnitt weg).
        var coreReqs = clusters.Clusters.SelectMany(c => c.CoreRequirementIds).ToHashSet(StringComparer.Ordinal);
        var pbiReqUnion = backlog.Items.SelectMany(i => i.RequirementIds).ToHashSet(StringComparer.Ordinal);
        foreach (var id in coreReqs.OrderBy(x => x, StringComparer.Ordinal))
            if (!pbiReqUnion.Contains(id))
                errors.Add(Error("UNCOVERED_CORE", $"Cluster-Core-Requirement {id} ist in keinem PBI (Coverage-Verletzung).", null, [id]));

        var seenPbi = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pbi in backlog.Items)
        {
            if (string.IsNullOrWhiteSpace(pbi.PbiId))
                errors.Add(Error("MISSING_PBI_ID", "PBI ohne pbiId.", null, pbi.RequirementIds));
            else if (!seenPbi.Add(pbi.PbiId))
                errors.Add(Error("DUPLICATE_PBI_ID", $"pbiId {pbi.PbiId} mehrfach.", pbi.PbiId, pbi.RequirementIds));

            if (pbi.RequirementIds.Count == 0)
                errors.Add(Error("EMPTY_PBI", $"PBI {pbi.PbiId} hat keine requirementIds.", pbi.PbiId, []));
            foreach (var id in pbi.RequirementIds)
                if (!allReqIds.Contains(id))
                    errors.Add(Error("UNKNOWN_REQUIREMENT", $"requirementId {id} existiert nicht.", pbi.PbiId, [id]));

            if (string.IsNullOrWhiteSpace(pbi.Title))
                errors.Add(Error("MISSING_TITLE", $"PBI {pbi.PbiId} ohne title.", pbi.PbiId, pbi.RequirementIds));

            // Kern-Invariante "keine stille Luecke": entweder pruefbar ODER offene Punkte explizit.
            if (pbi.AcceptanceCriteria.Count == 0 && pbi.OpenDecisions.Count == 0)
                errors.Add(Error("NO_TESTABILITY",
                    $"PBI {pbi.PbiId} hat weder Akzeptanzkriterien noch offene Entscheidungen (stille Luecke).", pbi.PbiId, pbi.RequirementIds));

            if (!string.IsNullOrWhiteSpace(pbi.Type) && !Types.Contains(pbi.Type))
                warnings.Add(Warn("UNKNOWN_TYPE", $"PBI {pbi.PbiId}: unbekannter type '{pbi.Type}'.", pbi.PbiId, []));
            if (pbi.Readiness is { Length: > 0 } rd && !Readiness.Contains(rd))
                warnings.Add(Warn("UNKNOWN_READINESS", $"PBI {pbi.PbiId}: unbekannte readiness '{rd}'.", pbi.PbiId, []));

            // Scope-Konsistenz: blockierende offene Entscheidung -> nicht backlog_ready.
            var hasBlocking = pbi.OpenDecisions.Any(d => d.BlocksScope);
            if (hasBlocking && string.Equals(pbi.Readiness, "backlog_ready", StringComparison.OrdinalIgnoreCase))
                warnings.Add(Warn("READINESS_INCONSISTENT",
                    $"PBI {pbi.PbiId}: blockierende offene Entscheidung, aber readiness=backlog_ready.", pbi.PbiId, []));
        }

        var pass = errors.Count == 0;
        var checks = new Dictionary<string, object>
        {
            ["clusters"] = clusters.Clusters.Count,
            ["coreRequirements"] = coreReqs.Count,
            ["pbis"] = backlog.Items.Count,
            ["coveredCores"] = coreReqs.Count(pbiReqUnion.Contains),
        };
        return new ReClarifyGateReport(pass, pass ? "pass" : "fail", errors, warnings, checks);
    }

    // R-33 S0: Severity + Repairability werden AN DER REGEL deklariert (eine Stelle je Regel, kein Neben-
    // Register). Alle heutigen Regeln sind Plan-Qualitaet des Clarify-Agenten -> per GateFeedback fixbar;
    // eine kuenftig nicht-reparierbare Regel deklariert ihre Repairability explizit am Aufruf.
    private static ReClarifyGateIssue Error(string code, string message, string? subjectId,
        IReadOnlyList<string> requirementIds, string repairability = Repairability.Repairable)
        => new(code, "error", message, subjectId, requirementIds, repairability);

    private static ReClarifyGateIssue Warn(string code, string message, string? subjectId, IReadOnlyList<string> requirementIds)
        => new(code, "warning", message, subjectId, requirementIds, Repairability.Repairable);

    public static GateDecision Decide(ReClarifyGateReport report, int attempt, int maxAttempts)
        => GateLoop.Decide(report.Pass,
            report.Errors.Any(e => string.Equals(e.Repairability, Repairability.Repairable, StringComparison.Ordinal)),
            attempt, maxAttempts);
}
