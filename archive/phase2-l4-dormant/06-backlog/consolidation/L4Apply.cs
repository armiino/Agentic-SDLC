using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4Apply
{
    private static readonly HashSet<string> TargetProducingOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "KEEP", "ADD", "MERGE", "SPLIT", "REVISE", "MARK_OPEN_DECISION"
    };

    public static (ConsolidationPlan EffectivePlan, IReadOnlyList<ConsolidationOperation> AutoFallbackKeeps) BuildEffectivePlan(
        ProjectStateDocument state,
        ConsolidationPlan plan,
        L4HumanDecisionsFile decisions)
    {
        var byDecision = decisions.Decisions
            .GroupBy(d => d.OperationId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var seedKeeps = L4ConsolidationPlanFactory.CreateIdentityPlan(state, plan.SourceProjectStatePath)
            .Operations
            .Where(o => o.Operation.Equals("KEEP", StringComparison.OrdinalIgnoreCase))
            .SelectMany(o => o.SourceItemIds.Select(sourceId => (sourceId, op: o)))
            .ToDictionary(x => x.sourceId, x => x.op, StringComparer.Ordinal);

        var effective = new List<ConsolidationOperation>();
        var fallback = new List<ConsolidationOperation>();
        var existingIds = new HashSet<string>(plan.Operations.Select(o => o.OperationId), StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!byDecision.TryGetValue(op.OperationId, out var decision))
            {
                if (!op.RequiresHumanReview)
                    effective.Add(op);
                else
                    AddFallbackKeeps(op, seedKeeps, existingIds, effective, fallback);
                continue;
            }

            switch ((decision.Decision ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "accept":
                    effective.Add(op);
                    break;
                case "edit":
                    if (string.IsNullOrWhiteSpace(decision.EditedOperationJson))
                    {
                        AddFallbackKeeps(op, seedKeeps, existingIds, effective, fallback);
                        break;
                    }
                    var edited = JsonSerializer.Deserialize<ConsolidationOperation>(decision.EditedOperationJson, ProjectStateJson.Options)
                                 ?? throw new InvalidOperationException($"Edited operation konnte nicht gelesen werden: {op.OperationId}");
                    effective.Add(edited);
                    break;
                case "reject":
                case "revise":
                    AddFallbackKeeps(op, seedKeeps, existingIds, effective, fallback);
                    break;
                default:
                    if (!op.RequiresHumanReview)
                        effective.Add(op);
                    else
                        AddFallbackKeeps(op, seedKeeps, existingIds, effective, fallback);
                    break;
            }
        }

        return (plan with { CreatedUtc = DateTime.UtcNow, Operations = effective }, fallback);
    }

    public static CanonicalRequirementsBaseline BuildBaseline(
        ProjectStateDocument state,
        ConsolidationPlan effectivePlan,
        L4HumanDecisionsFile decisions)
    {
        var byItem = state.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var reviewed = decisions.Decisions.ToDictionary(d => d.OperationId, StringComparer.Ordinal);
        var requirements = new List<CanonicalRequirement>();
        var traceLinks = new List<CanonicalTraceLink>();
        var sourceToRequirementIds = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var op in effectivePlan.Operations.Where(op => TargetProducingOperations.Contains(op.Operation)))
        {
            foreach (var target in op.Targets)
            {
                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["operationId"] = op.OperationId,
                    ["operation"] = op.Operation,
                    ["requiresHumanReview"] = op.RequiresHumanReview.ToString(),
                    ["rationale"] = op.Rationale ?? ""
                };
                foreach (var (key, value) in target.Metadata)
                    metadata[$"target.{key}"] = JsonValue(value);
                foreach (var (key, value) in op.Metadata)
                    metadata[$"operation.{key}"] = JsonValue(value);
                if (reviewed.TryGetValue(op.OperationId, out var decision))
                {
                    metadata["humanReview.decision"] = decision.Decision;
                    metadata["humanReview.reason"] = decision.Reason ?? "";
                }

                var requirement = new CanonicalRequirement(
                    RequirementId: target.RequirementId,
                    Title: target.Title,
                    Text: target.Text,
                    Status: target.Status,
                    SourceItemIds: op.SourceItemIds,
                    OriginSummary: op.Operation,
                    Version: 1,
                    Metadata: metadata);
                requirements.Add(requirement);

                foreach (var sourceId in op.SourceItemIds)
                {
                    if (!sourceToRequirementIds.TryGetValue(sourceId, out var ids))
                    {
                        ids = [];
                        sourceToRequirementIds[sourceId] = ids;
                    }
                    ids.Add(requirement.RequirementId);

                    traceLinks.Add(new CanonicalTraceLink(requirement.RequirementId, "project_item", sourceId, "canonicalizes", new Dictionary<string, string>
                    {
                        ["operationId"] = op.OperationId,
                        ["operation"] = op.Operation
                    }));

                    if (!byItem.TryGetValue(sourceId, out var sourceItem)) continue;
                    foreach (var claimId in sourceItem.SourceClaimIds)
                        traceLinks.Add(new CanonicalTraceLink(requirement.RequirementId, "ledger_claim", claimId, "evidenced_by", new Dictionary<string, string>()));
                    foreach (var upstreamId in sourceItem.SourceArtifactItemIds)
                        traceLinks.Add(new CanonicalTraceLink(requirement.RequirementId, "project_item", upstreamId, "derived_from", new Dictionary<string, string>()));
                    if (!string.IsNullOrWhiteSpace(sourceItem.SourceDecisionId))
                        traceLinks.Add(new CanonicalTraceLink(requirement.RequirementId, "human_decision", sourceItem.SourceDecisionId!, "accepted_by", new Dictionary<string, string>()));
                    if (!string.IsNullOrWhiteSpace(sourceItem.SourceCandidateId))
                        traceLinks.Add(new CanonicalTraceLink(requirement.RequirementId, "l3_candidate", sourceItem.SourceCandidateId!, "promoted_from", new Dictionary<string, string>()));
                }
            }
        }

        foreach (var op in effectivePlan.Operations.Where(op => op.Operation.Equals("DEPRECATE", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var targetId in ExtractStringArray(op.Metadata, "replacedBy"))
            {
                foreach (var sourceId in op.SourceItemIds)
                {
                    traceLinks.Add(new CanonicalTraceLink(targetId, "project_item", sourceId, "replaces", new Dictionary<string, string>
                    {
                        ["operationId"] = op.OperationId,
                        ["rationale"] = op.Rationale ?? "",
                        ["humanReview"] = reviewed.TryGetValue(op.OperationId, out var d) ? d.Decision : ""
                    }));
                }
            }
        }

        foreach (var op in effectivePlan.Operations.Where(op => op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase)))
        {
            var relation = op.Metadata.TryGetValue("relation", out var rel) ? JsonValue(rel) : "linked";
            var sourceIds = string.Join(",", op.SourceItemIds);
            foreach (var sourceId in op.SourceItemIds)
            {
                if (!sourceToRequirementIds.TryGetValue(sourceId, out var reqIds)) continue;
                foreach (var reqId in reqIds)
                {
                    traceLinks.Add(new CanonicalTraceLink(reqId, "project_item_cluster", sourceIds, "linked_to", new Dictionary<string, string>
                    {
                        ["operationId"] = op.OperationId,
                        ["relation"] = relation,
                        ["rationale"] = op.Rationale ?? ""
                    }));
                }
            }
        }

        var openDecisions = requirements
            .Where(r => r.Status.Equals("open_decision", StringComparison.OrdinalIgnoreCase))
            .Select((r, index) => new CanonicalOpenDecision(
                DecisionId: $"OPEN-{index + 1:D3}",
                Text: r.Text,
                SourceRequirementId: r.RequirementId,
                SourceItemIds: r.SourceItemIds,
                Reason: "Status open_decision aus L4 ConsolidationPlan."))
            .ToList();

        return new CanonicalRequirementsBaseline(
            BaselineId: $"baseline-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: state.ProjectId,
            SchemaVersion: CanonicalRequirementsBaseline.CurrentSchemaVersion,
            CreatedUtc: DateTime.UtcNow,
            SourceProjectStatePath: effectivePlan.SourceProjectStatePath,
            Requirements: requirements
                .OrderBy(r => SortKey(r.RequirementId), StringComparer.Ordinal)
                .ThenBy(r => r.RequirementId, StringComparer.Ordinal)
                .ToList(),
            OpenDecisions: openDecisions,
            TraceLinks: traceLinks);
    }

    private static void AddFallbackKeeps(
        ConsolidationOperation op,
        IReadOnlyDictionary<string, ConsolidationOperation> seedKeeps,
        HashSet<string> existingIds,
        List<ConsolidationOperation> effective,
        List<ConsolidationOperation> fallback)
    {
        if (op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase))
            return;

        foreach (var sourceId in op.SourceItemIds)
        {
            if (!seedKeeps.TryGetValue(sourceId, out var keep)) continue;
            var fallbackId = $"AUTO-KEEP-{sourceId}";
            var unique = fallbackId;
            var n = 1;
            while (!existingIds.Add(unique))
                unique = $"{fallbackId}-{++n}";
            var fallbackKeep = keep with
            {
                OperationId = unique,
                Rationale = $"Auto fallback KEEP because operation {op.OperationId} was not accepted for apply.",
                Metadata = new Dictionary<string, object?>(keep.Metadata, StringComparer.Ordinal)
                {
                    ["fallbackForOperationId"] = op.OperationId
                }
            };
            effective.Add(fallbackKeep);
            fallback.Add(fallbackKeep);
        }
    }

    private static string JsonValue(object? value)
        => value switch
        {
            null => "",
            string s => s,
            JsonElement el => el.ValueKind == JsonValueKind.String ? el.GetString() ?? "" : el.GetRawText(),
            _ => JsonSerializer.Serialize(value, ProjectStateJson.Options)
        };

    private static IReadOnlyList<string> ExtractStringArray(IReadOnlyDictionary<string, object?> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var raw) || raw is null) return [];
        if (raw is string s) return [s];
        if (raw is IEnumerable<string> strings) return strings.ToArray();
        if (raw is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return [element.GetString() ?? ""];
            if (element.ValueKind == JsonValueKind.Array)
                return element.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString() ?? "")
                    .Where(sv => !string.IsNullOrWhiteSpace(sv))
                    .ToArray();
        }
        return [];
    }

    private static string SortKey(string itemId)
    {
        var prefix = new string(itemId.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(itemId.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return $"{prefix}{(int.TryParse(digits, out var n) ? n : 0):D6}:{itemId}";
    }
}
