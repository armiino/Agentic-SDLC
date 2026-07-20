using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// T2.1 — deterministisches Gate über den Decision-Resolution-Plan (plan-tor2 §6). Kernregeln inkl. Rev-2-
// Nachvollziehbarkeit (Outcome gesetzt) und Coverage bei ADOPT_NEW (neue Aussage vorhanden → Swap kann decken).
public static class DecisionResolutionGate
{
    public static DecisionResolutionGateReport Check(ProjectStateDocument core, DecisionResolutionPlanDocument plan)
    {
        var errors = new List<DecisionResolutionGateIssue>();
        var warnings = new List<DecisionResolutionGateIssue>();

        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!DecisionOutcome.All.Contains(op.Outcome))
                errors.Add(Issue("UNKNOWN_OUTCOME", "error", $"Unbekanntes Outcome '{op.Outcome}'.", op.DecisionId));

            if (!byId.TryGetValue(op.DecisionId, out var dec) || !string.Equals(dec.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
                errors.Add(Issue("UNKNOWN_DECISION", "error", $"'{op.DecisionId}' ist keine Decision.", op.DecisionId));
            else if (!string.Equals(dec.Status, DecisionStatus.Open, StringComparison.OrdinalIgnoreCase))
                errors.Add(Issue("DECISION_NOT_OPEN", "error", $"'{op.DecisionId}' ist nicht offen (status={dec.Status}).", op.DecisionId));

            if (!byId.ContainsKey(op.TargetRequirementId))
                errors.Add(Issue("UNKNOWN_TARGET", "error", $"Ziel-Requirement '{op.TargetRequirementId}' existiert nicht.", op.DecisionId));

            if (DecisionOutcome.RequireNewStatement.Contains(op.Outcome) && string.IsNullOrWhiteSpace(op.NewStatement))
                errors.Add(Issue("NEW_STATEMENT_REQUIRED", "error", $"'{op.Outcome}' braucht eine neue Aussage.", op.DecisionId));

            // Nachvollziehbarkeit (Rev 2): das Outcome ist der dokumentierte resolutionOutcome — darf nie leer sein.
            if (string.IsNullOrWhiteSpace(op.Outcome))
                errors.Add(Issue("MISSING_OUTCOME", "error", "Auflösung ohne dokumentiertes Outcome.", op.DecisionId));
        }

        foreach (var g in plan.Operations.GroupBy(o => o.DecisionId, StringComparer.Ordinal).Where(g => g.Count() > 1))
            errors.Add(Issue("DUPLICATE_DECISION", "error", $"Decision '{g.Key}' hat {g.Count()} Auflösungen (genau eine erlaubt).", g.Key));

        var pass = errors.Count == 0;
        return new DecisionResolutionGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static DecisionResolutionGateIssue Issue(string code, string sev, string msg, string? dec)
        => new(code, sev, msg, dec, RepairabilityOf(code));

    // R7: reparierbar = Resolver-Agent kann es fixen (Outcome waehlen / neue Aussage formulieren). UNKNOWN_DECISION/
    // DECISION_NOT_OPEN/UNKNOWN_TARGET = Input-/State-Fakten (hard); MISSING_OUTCOME = needs_human.
    private static readonly IReadOnlyDictionary<string, string> Classification = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["NEW_STATEMENT_REQUIRED"] = Core.Repairability.Repairable,
        ["UNKNOWN_OUTCOME"] = Core.Repairability.Repairable,
        ["DUPLICATE_DECISION"] = Core.Repairability.Repairable,
        ["MISSING_OUTCOME"] = Core.Repairability.NeedsHuman,
    };

    private static string RepairabilityOf(string code) => Classification.GetValueOrDefault(code, Core.Repairability.Hard);

    public static Core.GateDecision Decide(DecisionResolutionGateReport report, int attempt, int maxAttempts)
        => Core.GateLoop.Decide(report.Pass, report.Errors.Any(e => string.Equals(e.Repairability, Core.Repairability.Repairable, StringComparison.Ordinal)), attempt, maxAttempts);
}
