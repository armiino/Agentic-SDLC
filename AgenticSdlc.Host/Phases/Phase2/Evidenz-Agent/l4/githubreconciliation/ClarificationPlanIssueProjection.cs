namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class ClarificationPlanIssueProjection
{
    public static IssuePlanDocument ToIssuePlan(ClarificationPlanDocument plan, string sourceAcceptedClarificationPlanPath)
    {
        var items = plan.Items
            .OrderBy(i => i.ClarificationPlanId, StringComparer.Ordinal)
            .Select(ToIssuePlanItem)
            .ToList();

        return new IssuePlanDocument(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"clarification-as-issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: plan.ProjectId,
            BaselineId: plan.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourceAcceptedClarificationPlanPath,
            Items: items);
    }

    private static IssuePlanItem ToIssuePlanItem(ClarificationPlanItem item)
    {
        var operation = item.Operation.ToUpperInvariant() switch
        {
            "CREATE_CLARIFICATION_ISSUE" => "CREATE",
            "CREATE_BREAKDOWN_ISSUE" => "CREATE",
            "MERGE_WITH_EXISTING_CLARIFICATION" => "CREATE",
            "DEFER" => "NO_CHANGE",
            "NO_ACTION" => "NO_CHANGE",
            _ => "NEEDS_REVIEW"
        };

        var labels = item.Labels
            .Concat(["clarification-plan"])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new IssuePlanItem(
            IssuePlanId: item.ClarificationPlanId,
            Operation: operation,
            Title: item.Title,
            Description: BuildBody(item),
            SourceRequirementIds: item.SourceRequirementIds,
            AcceptanceCriteria: item.AcceptanceCriteria,
            Labels: labels,
            Dependencies: [],
            Rationale: item.Question,
            RequiresHumanReview: true,
            Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["inputKind"] = "accepted_clarification_plan",
                ["sourceClarificationPlanOperation"] = item.Operation,
                ["sourceClarificationIds"] = item.SourceClarificationIds,
                ["priority"] = item.Priority
            });
    }

    private static string BuildBody(ClarificationPlanItem item)
        => $"""
           Klaerungs-/Breakdown-Arbeit aus accepted ClarificationPlan.

           Frage:
           {item.Question}

           Beschreibung:
           {item.Description}

           Source clarification IDs: {string.Join(", ", item.SourceClarificationIds)}
           Source requirement IDs: {string.Join(", ", item.SourceRequirementIds)}
           Priority: {item.Priority}
           """;
}
