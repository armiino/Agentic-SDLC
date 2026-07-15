using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class L4ConsolidationPlanFactory
{
    public static ConsolidationPlan CreateIdentityPlan(ProjectStateDocument state, string sourceProjectStatePath)
    {
        var baseline = L4BaselineBuilder.Build(state, sourceProjectStatePath);
        var operations = baseline.Requirements
            .Select((req, index) => new ConsolidationOperation(
                OperationId: $"COP-{index + 1:D3}",
                Operation: "KEEP",
                SourceItemIds: req.SourceItemIds,
                Targets:
                [
                    new ConsolidationTargetRequirement(
                        RequirementId: req.RequirementId,
                        Title: req.Title,
                        Text: req.Text,
                        Status: req.Status,
                        Metadata: req.Metadata.ToDictionary(kv => kv.Key, kv => (object?)kv.Value, StringComparer.Ordinal))
                ],
                Rationale: "Identity seed from active ProjectState requirement.",
                RequiresHumanReview: false,
                Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["seed"] = "identity",
                    ["originSummary"] = req.OriginSummary
                }))
            .ToList();

        return new ConsolidationPlan(
            SchemaVersion: ConsolidationPlan.CurrentSchemaVersion,
            ProjectId: state.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            SourceProjectStatePath: sourceProjectStatePath,
            Operations: operations);
    }
}
