using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4ProvenanceBuilder
{
    public static L4ProvenanceMap Build(
        CanonicalRequirementsBaseline baseline,
        ProjectStateDocument state,
        ConsolidationPlan effectivePlan)
    {
        var itemsById = state.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var provenanceById = state.Provenance.ToDictionary(p => p.ItemId, StringComparer.Ordinal);
        var operationByTarget = effectivePlan.Operations
            .SelectMany(op => op.Targets.Select(t => (t.RequirementId, Operation: op)))
            .GroupBy(x => x.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Operation, StringComparer.Ordinal);

        var replacesByTarget = effectivePlan.Operations
            .Where(op => op.Operation.Equals("DEPRECATE", StringComparison.OrdinalIgnoreCase))
            .SelectMany(op => ExtractStringArray(op.Metadata, "replacedBy").Select(targetId => (TargetId: targetId, Operation: op)))
            .GroupBy(x => x.TargetId, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(x => x.Operation.SourceItemIds).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        var linkOps = effectivePlan.Operations
            .Where(op => op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var requirementTraces = baseline.Requirements.Select(req =>
        {
            operationByTarget.TryGetValue(req.RequirementId, out var op);
            var sourceItems = req.SourceItemIds
                .Select(id => BuildProjectItemTrace(id, itemsById, provenanceById))
                .Where(t => t is not null)
                .Select(t => t!)
                .ToList();

            var ledgerClaims = sourceItems.SelectMany(i => i.SourceClaimIds)
                .Concat(baseline.TraceLinks.Where(t => t.RequirementId == req.RequirementId && t.SourceType == "ledger_claim").Select(t => t.SourceId))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var l3Candidates = sourceItems.Select(i => i.SourceCandidateId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id!)
                .Concat(baseline.TraceLinks.Where(t => t.RequirementId == req.RequirementId && t.SourceType == "l3_candidate").Select(t => t.SourceId))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var humanDecisions = sourceItems.Select(i => i.SourceDecisionId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id!)
                .Concat(baseline.TraceLinks.Where(t => t.RequirementId == req.RequirementId && t.SourceType == "human_decision").Select(t => t.SourceId))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();

            var clusters = linkOps
                .Where(link => link.SourceItemIds.Any(id => req.SourceItemIds.Contains(id, StringComparer.Ordinal)))
                .Select(link => new L4LinkedClusterTrace(
                    OperationId: link.OperationId,
                    Relation: link.Metadata.TryGetValue("relation", out var rel) ? JsonValue(rel) : "linked",
                    SourceItemIds: link.SourceItemIds,
                    Rationale: link.Rationale))
                .ToList();

            return new L4RequirementProvenance(
                RequirementId: req.RequirementId,
                Status: req.Status,
                Title: req.Title,
                L4OperationId: op?.OperationId,
                L4Operation: op?.Operation,
                SourceProjectItems: sourceItems,
                LedgerClaimIds: ledgerClaims,
                L3CandidateIds: l3Candidates,
                HumanDecisionIds: humanDecisions,
                ReplacesProjectItemIds: replacesByTarget.GetValueOrDefault(req.RequirementId) ?? [],
                LinkedClusters: clusters);
        }).ToList();

        return new L4ProvenanceMap(
            BaselineId: baseline.BaselineId,
            ProjectId: baseline.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            Requirements: requirementTraces);
    }

    private static L4ProjectItemTrace? BuildProjectItemTrace(
        string itemId,
        IReadOnlyDictionary<string, ProjectStateItem> itemsById,
        IReadOnlyDictionary<string, ProjectStateProvenance> provenanceById)
    {
        if (!itemsById.TryGetValue(itemId, out var item)) return null;
        var provenanceLinks = provenanceById.TryGetValue(itemId, out var provenance)
            ? provenance.Links
            : [];
        return new L4ProjectItemTrace(
            ItemId: item.ItemId,
            ItemType: item.ItemType,
            Status: item.Status,
            Origin: item.Origin,
            SourceRunId: item.SourceRunId,
            SourceArtifactId: item.SourceArtifactId,
            SourceArtifactType: item.SourceArtifactType,
            SourceCandidateId: item.SourceCandidateId,
            SourceDecisionId: item.SourceDecisionId,
            SourceClaimIds: item.SourceClaimIds,
            SourceArtifactItemIds: item.SourceArtifactItemIds,
            ProvenanceLinks: provenanceLinks);
    }

    private static IReadOnlyList<string> ExtractStringArray(IReadOnlyDictionary<string, object?> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var raw) || raw is null) return [];
        if (raw is string s) return [s];
        if (raw is IEnumerable<string> strings) return strings.ToArray();
        if (raw is System.Text.Json.JsonElement element)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                return [element.GetString() ?? ""];
            if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
                return element.EnumerateArray()
                    .Where(e => e.ValueKind == System.Text.Json.JsonValueKind.String)
                    .Select(e => e.GetString() ?? "")
                    .Where(sv => !string.IsNullOrWhiteSpace(sv))
                    .ToArray();
        }
        return [];
    }

    private static string JsonValue(object? value)
        => value switch
        {
            null => "",
            string s => s,
            System.Text.Json.JsonElement el => el.ValueKind == System.Text.Json.JsonValueKind.String ? el.GetString() ?? "" : el.GetRawText(),
            _ => System.Text.Json.JsonSerializer.Serialize(value, ProjectStateJson.Options)
        };
}
