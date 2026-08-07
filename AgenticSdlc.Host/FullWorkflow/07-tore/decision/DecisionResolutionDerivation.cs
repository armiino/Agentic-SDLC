using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// T2.1 — deterministische Ableitung des StateChange-Plans aus den Auflösungen (kein LLM). Findet je Decision das
// Ziel-Requirement (via contradicts-Relation) und die betroffenen PBIs (blockiert durch die DEC ∪ — bei
// Wahrheitsänderung — die das Ziel-Requirement deckenden). Bei KEEP_ORIGINAL nur die blockierten PBIs.
public static class DecisionResolutionDerivation
{
    public static (IReadOnlyList<DecisionResolutionOp> Ops, IReadOnlyList<string> Problems) Derive(
        ProjectStateDocument core, DecisionResolutionInput input)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var ops = new List<DecisionResolutionOp>();
        var problems = new List<string>();

        foreach (var r in input.Resolutions)
        {
            if (!byId.TryGetValue(r.DecisionId, out var dec) || !string.Equals(dec.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
            { problems.Add($"{r.DecisionId}: keine Decision im Core"); continue; }
            if (!dec.ReadStatus().IsOpenDecision)
            { problems.Add($"{r.DecisionId}: nicht offen (status={dec.Status})"); continue; }

            // Ziel-Requirement: contradicts-Relation der DEC, sonst metadata targetEntityId.
            var targetReq = core.Relations
                .FirstOrDefault(rel => string.Equals(rel.RelationType, DecisionRelations.Contradicts, StringComparison.Ordinal)
                    && string.Equals(rel.FromId, r.DecisionId, StringComparison.Ordinal))?.ToId
                ?? dec.Metadata.GetValueOrDefault("targetEntityId");
            // 9g: ziellose DECs (Meeting-Fragen) sind mit KEEP aufloesbar ("geklaert/erledigt", keine Mutation);
            // ADOPT/REFINE brauchen weiterhin zwingend ein Ziel — ohne Ziel gibt es nichts abzuloesen/verfeinern.
            if (targetReq is not null && !byId.ContainsKey(targetReq)) targetReq = null;
            if (targetReq is null && !string.Equals(r.Outcome, DecisionOutcome.KeepOriginal, StringComparison.Ordinal))
            { problems.Add($"{r.DecisionId}: kein Ziel-Requirement — nur 'geklaert' (KEEP) oder Vertagen moeglich"); continue; }

            var blocked = PbisBlockedBy(core, r.DecisionId);
            var affected = new HashSet<string>(blocked, StringComparer.Ordinal);
            if (targetReq is not null && !string.Equals(r.Outcome, DecisionOutcome.KeepOriginal, StringComparison.Ordinal))
            {
                foreach (var p in PbisCovering(core, targetReq)) affected.Add(p);
                foreach (var p in PbisConstrainedBy(core, targetReq)) affected.Add(p);   // ② E-R4: arch-Ziele wirken ueber constrained_by
            }

            ops.Add(new DecisionResolutionOp(
                DecisionId: r.DecisionId,
                Outcome: r.Outcome,
                TargetRequirementId: targetReq,
                NewStatement: r.NewStatement,
                AffectedPbis: affected.OrderBy(x => x, StringComparer.Ordinal).ToList(),
                Rationale: string.IsNullOrWhiteSpace(r.Rationale) ? $"{r.Outcome} für {r.DecisionId}" : r.Rationale!));
        }

        return (ops, problems);
    }

    private static IReadOnlyList<string> PbisBlockedBy(ProjectStateDocument core, string decisionId)
        => core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                        && (i.Pbi?.OpenDecisionRefs.Contains(decisionId) ?? false))
            .Select(i => i.ItemId).ToList();

    private static IReadOnlyList<string> PbisConstrainedBy(ProjectStateDocument core, string archId)
        => core.Relations
            .Where(rel => string.Equals(rel.RelationType, "constrained_by", StringComparison.Ordinal)
                        && string.Equals(rel.ToId, archId, StringComparison.Ordinal))
            .Select(rel => rel.FromId).Distinct(StringComparer.Ordinal).ToList();

    private static IReadOnlyList<string> PbisCovering(ProjectStateDocument core, string requirementId)
        => core.Relations
            .Where(rel => string.Equals(rel.RelationType, "covers", StringComparison.Ordinal)
                        && string.Equals(rel.ToId, requirementId, StringComparison.Ordinal))
            .Select(rel => rel.FromId).Distinct(StringComparer.Ordinal).ToList();
}
