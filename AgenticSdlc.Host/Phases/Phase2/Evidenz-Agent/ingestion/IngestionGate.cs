using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

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
        => new(code, severity, message, incoming, target);
}
