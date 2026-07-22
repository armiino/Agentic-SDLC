using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Deterministisches Gate (plan-increment1c3 §5). MULTI_CAUSE_MERGE: mehrere Ops pro PBI sind ERLAUBT (additiv,
// Merge im Apply). Geprueft: gueltige Ziele, Platzierungs-Coverage, Block-Decision-Ref, Supersede-Ersatz.
public static class PbiUpdateGate
{
    public static PbiUpdateGateReport Check(ProjectStateDocument core, PbiStateChangePlanDocument plan, IReadOnlySet<string> unplacedRequirementIds)
    {
        var errors = new List<PbiUpdateGateIssue>();
        var warnings = new List<PbiUpdateGateIssue>();

        var pbiIds = Ids(core, "pbi");
        var featureIds = Ids(core, "feature");
        var decisionIds = Ids(core, "decision");
        var reqIds = Ids(core, "requirement");

        foreach (var op in plan.Operations)
        {
            if (!PbiUpdateKind.All.Contains(op.Kind))
                errors.Add(Issue("UNKNOWN_KIND", "error", $"Unbekannte Operation '{op.Kind}'.", op.RequirementId, op.PbiId));

            if (PbiUpdateKind.RequirePbi.Contains(op.Kind))
            {
                if (string.IsNullOrWhiteSpace(op.PbiId)) errors.Add(Issue("TARGET_REQUIRED", "error", $"'{op.Kind}' braucht pbiId.", op.RequirementId, null));
                else if (!pbiIds.Contains(op.PbiId)) errors.Add(Issue("UNKNOWN_TARGET", "error", $"pbiId '{op.PbiId}' existiert nicht.", op.RequirementId, op.PbiId));
            }

            if (string.Equals(op.Kind, PbiUpdateKind.NewPbi, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(op.FeatureId)) errors.Add(Issue("FEATURE_REQUIRED", "error", "NEW_PBI braucht featureId.", op.RequirementId, null));
                else if (!featureIds.Contains(op.FeatureId)) errors.Add(Issue("UNKNOWN_FEATURE", "error", $"featureId '{op.FeatureId}' existiert nicht.", op.RequirementId, null));
            }

            if (string.Equals(op.Kind, PbiUpdateKind.BlockPbi, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(op.OpenDecisionRef) || !decisionIds.Contains(op.OpenDecisionRef!))
                    errors.Add(Issue("BLOCK_NEEDS_DECISION", "error", "BLOCK_PBI braucht existierende openDecisionRef.", op.RequirementId, op.PbiId));
            }

            if (string.Equals(op.Kind, PbiUpdateKind.SupersedePbi, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(op.ReplacementRequirementId) || !reqIds.Contains(op.ReplacementRequirementId!))
                    warnings.Add(Issue("SUPERSEDE_NO_REPLACEMENT", "warning", "SUPERSEDE_PBI ohne gueltiges Ersatz-Requirement — PBI verliert Abdeckung.", op.RequirementId, op.PbiId));
            }
        }

        // Coverage: jedes neue (ungedeckte) Requirement genau eine Platzierung (NEW_PBI|EXTEND_PBI).
        var placedBy = plan.Operations
            .Where(o => PbiUpdateKind.Placement.Contains(o.Kind))
            .GroupBy(o => o.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        foreach (var rid in unplacedRequirementIds)
        {
            if (!placedBy.TryGetValue(rid, out var n)) errors.Add(Issue("UNPLACED_REQUIREMENT", "error", $"Neues Requirement '{rid}' hat keine Platzierung.", rid, null));
            else if (n > 1) errors.Add(Issue("DUPLICATE_PLACEMENT", "error", $"Requirement '{rid}' hat {n} Platzierungen (genau eine erlaubt).", rid, null));
        }

        var pass = errors.Count == 0;
        return new PbiUpdateGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static HashSet<string> Ids(ProjectStateDocument core, string type)
        => core.Items.Where(i => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase)).Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);

    private static PbiUpdateGateIssue Issue(string code, string sev, string msg, string? req, string? pbi)
        => new(code, sev, msg, req, pbi, RepairabilityOf(code));

    // R7: reparierbar = Fehler der agentischen Platzierung (EXTEND/NEW_PBI); deterministische Ableitungsfehler = hard.
    private static readonly IReadOnlyDictionary<string, string> Classification = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["UNPLACED_REQUIREMENT"] = Core.Repairability.Repairable,
        ["DUPLICATE_PLACEMENT"] = Core.Repairability.Repairable,
        ["UNKNOWN_FEATURE"] = Core.Repairability.Repairable,
        ["FEATURE_REQUIRED"] = Core.Repairability.Repairable,
        ["UNKNOWN_KIND"] = Core.Repairability.NeedsHuman,
    };

    private static string RepairabilityOf(string code) => Classification.GetValueOrDefault(code, Core.Repairability.Hard);

    public static Core.GateDecision Decide(PbiUpdateGateReport report, int attempt, int maxAttempts)
        => Core.GateLoop.Decide(report.Pass, report.Errors.Any(e => string.Equals(e.Repairability, Core.Repairability.Repairable, StringComparison.Ordinal)), attempt, maxAttempts);
}
