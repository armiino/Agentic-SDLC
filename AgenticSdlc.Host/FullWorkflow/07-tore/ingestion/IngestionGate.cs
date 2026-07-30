using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Deterministisches Gate der Requirement-Ingestion (plan-increment1 §1.4): kein stiller Fehler,
// kein Halluzinations-Match. Prueft die Operationen gegen MeetingDelta (Coverage) und Core (Ziele).
public static class IngestionGate
{
    public static IngestionGateReport Check(ProjectStateDocument meetingDelta, ProjectStateDocument core, StateChangePlanDocument plan)
    {
        var errors = new List<IngestionGateIssue>();
        var warnings = new List<IngestionGateIssue>();

        var incomingIds = meetingDelta.Items
            .Where(i => IsRequirement(i))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        var coreReqIds = core.Items
            .Where(i => IsRequirement(i))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        var coreDecisionIds = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        var coreFeatureIds = core.Items
            .Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!incomingIds.Contains(op.IncomingItemId))
                errors.Add(Issue("UNKNOWN_INCOMING", "error", $"Operation referenziert unbekanntes incoming Item '{op.IncomingItemId}'.", op.IncomingItemId, op.TargetEntityId));

            if (!StateChangeKind.All.Contains(op.Kind))
                errors.Add(Issue("UNKNOWN_KIND", "error", $"Unbekannte Operation '{op.Kind}' fuer '{op.IncomingItemId}'.", op.IncomingItemId, null));

            var requiresReqTarget = StateChangeKind.RequireTarget.Contains(op.Kind);
            var requiresDecisionTarget = StateChangeKind.RequireDecisionTarget.Contains(op.Kind);
            var forbidsTarget = StateChangeKind.ForbidTarget.Contains(op.Kind);
            var hasTarget = !string.IsNullOrWhiteSpace(op.TargetEntityId);

            if ((requiresReqTarget || requiresDecisionTarget) && !hasTarget)
                errors.Add(Issue("TARGET_REQUIRED", "error", $"'{op.Kind}' braucht targetEntityId ('{op.IncomingItemId}').", op.IncomingItemId, null));
            if (forbidsTarget && hasTarget)
                errors.Add(Issue("TARGET_FORBIDDEN", "error", $"'{op.Kind}' darf kein targetEntityId haben ('{op.IncomingItemId}').", op.IncomingItemId, op.TargetEntityId));
            if (hasTarget && requiresDecisionTarget && !coreDecisionIds.Contains(op.TargetEntityId!))
                errors.Add(Issue("UNKNOWN_TARGET", "error", $"targetEntityId '{op.TargetEntityId}' ist keine bestehende Open Decision.", op.IncomingItemId, op.TargetEntityId));
            else if (hasTarget && !requiresDecisionTarget && !coreReqIds.Contains(op.TargetEntityId!))
                errors.Add(Issue("UNKNOWN_TARGET", "error", $"targetEntityId '{op.TargetEntityId}' existiert nicht im Core.", op.IncomingItemId, op.TargetEntityId));

            // O1 (Variante c): NEW_RELATED verweist per featureKey auf ein bestehendes Feature. Ein gesetzter
            // featureKey MUSS im Core existieren, sonst schreibt IngestionApply eine part_of_feature-Relation ins
            // Leere. Fehlender featureKey bleibt hier bewusst unveraendert (haerter erst mit Feature-Placement/O4).
            if (string.Equals(op.Kind, StateChangeKind.NewRelated, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(op.FeatureKey)
                && !coreFeatureIds.Contains(op.FeatureKey!))
                errors.Add(Issue("UNKNOWN_FEATURE", "error", $"featureKey '{op.FeatureKey}' ist kein bestehendes Core-Feature ('{op.IncomingItemId}').", op.IncomingItemId, op.FeatureKey));

            if (op.ClaimIds.Count == 0)
                warnings.Add(Issue("MISSING_EVIDENCE", "warning", $"Operation '{op.IncomingItemId}' ohne claimIds (Beleg).", op.IncomingItemId, op.TargetEntityId));
        }

        // Coverage: jedes eingehende Requirement genau eine Operation.
        var opsByIncoming = plan.Operations
            .GroupBy(o => o.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        foreach (var (id, count) in opsByIncoming.Where(kv => kv.Value > 1))
            errors.Add(Issue("DUPLICATE_OP", "error", $"incoming Item '{id}' hat {count} Operationen (genau eine erlaubt).", id, null));

        foreach (var id in incomingIds.Where(id => !opsByIncoming.ContainsKey(id)))
            errors.Add(Issue("UNPLACED_INCOMING", "error", $"incoming Requirement '{id}' hat keine Operation.", id, null));

        // Mehrere Operationen auf dasselbe Ziel -> Warnung (nicht zwingend falsch, aber pruefenswert).
        foreach (var g in plan.Operations
                     .Where(o => !string.IsNullOrWhiteSpace(o.TargetEntityId))
                     .GroupBy(o => o.TargetEntityId!, StringComparer.Ordinal)
                     .Where(g => g.Count() > 1))
            warnings.Add(Issue("MULTIPLE_OPS_SAME_TARGET", "warning", $"{g.Count()} Operationen zeigen auf '{g.Key}'.", null, g.Key));

        var pass = errors.Count == 0;
        return new IngestionGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static bool IsRequirement(ProjectStateItem i) => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase);

    private static IngestionGateIssue Issue(string code, string severity, string message, string? incoming, string? target)
        => new(code, severity, message, incoming, target, RepairabilityOf(code));

    // R7: reparierbar = Plan-Qualitaet des Resolvers (per GateFeedback fixbar); UNKNOWN_KIND = needs_human.
    private static readonly IReadOnlyDictionary<string, string> Classification = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["UNPLACED_INCOMING"] = Core.Repairability.Repairable,
        ["DUPLICATE_OP"] = Core.Repairability.Repairable,
        ["MULTIPLE_OPS_SAME_TARGET"] = Core.Repairability.Repairable,
        ["UNKNOWN_TARGET"] = Core.Repairability.Repairable,
        ["TARGET_REQUIRED"] = Core.Repairability.Repairable,
        ["TARGET_FORBIDDEN"] = Core.Repairability.Repairable,
        ["UNKNOWN_INCOMING"] = Core.Repairability.Repairable,
        ["UNKNOWN_FEATURE"] = Core.Repairability.Repairable,
        ["UNKNOWN_KIND"] = Core.Repairability.NeedsHuman,
    };

    private static string RepairabilityOf(string code) => Classification.GetValueOrDefault(code, Core.Repairability.Hard);

    public static Core.GateDecision Decide(IngestionGateReport report, int attempt, int maxAttempts)
        => Core.GateLoop.Decide(report.Pass, report.Errors.Any(e => string.Equals(e.Repairability, Core.Repairability.Repairable, StringComparison.Ordinal)), attempt, maxAttempts);
}
