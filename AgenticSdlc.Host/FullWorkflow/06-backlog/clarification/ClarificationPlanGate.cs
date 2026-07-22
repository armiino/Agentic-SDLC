namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class ClarificationPlanGate
{
    private static readonly HashSet<string> AllowedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "CREATE_CLARIFICATION_ISSUE",
        "CREATE_BREAKDOWN_ISSUE",
        "MERGE_WITH_EXISTING_CLARIFICATION",
        "DEFER",
        "NO_ACTION"
    };

    public static ClarificationPlanGateReport Check(ClarificationPlanningInput input, ClarificationPlanDocument plan)
    {
        var errors = new List<ClarificationPlanGateIssue>();
        var warnings = new List<ClarificationPlanGateIssue>();
        var inputClarificationIds = input.Items.Select(i => i.ClarificationId).ToHashSet(StringComparer.Ordinal);
        var inputRequirementIds = input.Items.Select(i => i.SourceRequirementId).ToHashSet(StringComparer.Ordinal);
        var planIds = new HashSet<string>(StringComparer.Ordinal);
        var coveredClarificationIds = new HashSet<string>(StringComparer.Ordinal);
        var coveredRequirementIds = new HashSet<string>(StringComparer.Ordinal);

        if (plan.SchemaVersion != ClarificationPlanDocument.CurrentSchemaVersion)
            errors.Add(Issue("schema_version_mismatch", "error", $"SchemaVersion {plan.SchemaVersion} ist nicht erlaubt.", null, [], []));

        if (!string.Equals(plan.ProjectId, input.ProjectId, StringComparison.Ordinal))
            errors.Add(Issue("project_mismatch", "error", "ClarificationPlan projectId passt nicht zum ClarificationPlanningInput.", null, [], []));

        if (!string.Equals(plan.BaselineId, input.BaselineId, StringComparison.Ordinal))
            errors.Add(Issue("baseline_mismatch", "error", "ClarificationPlan baselineId passt nicht zum ClarificationPlanningInput.", null, [], []));

        if (!string.Equals(plan.Mode, "plan", StringComparison.OrdinalIgnoreCase))
            errors.Add(Issue("invalid_mode", "error", "ClarificationPlan muss mode='plan' verwenden. Resolve ist ein separater TBD-Modus.", null, [], []));

        foreach (var item in plan.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ClarificationPlanId))
            {
                errors.Add(Issue("missing_clarification_plan_id", "error", "ClarificationPlanItem hat keine clarificationPlanId.", null, item.SourceClarificationIds, item.SourceRequirementIds));
            }
            else if (!planIds.Add(item.ClarificationPlanId))
            {
                errors.Add(Issue("duplicate_clarification_plan_id", "error", $"ClarificationPlanId kommt mehrfach vor: {item.ClarificationPlanId}", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));
            }

            if (!AllowedOperations.Contains(item.Operation))
                errors.Add(Issue("invalid_operation", "error", $"Operation ist nicht erlaubt: {item.Operation}", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));

            if (string.IsNullOrWhiteSpace(item.Title))
                errors.Add(Issue("missing_title", "error", "ClarificationPlanItem hat keinen Titel.", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));
            if (string.IsNullOrWhiteSpace(item.Question))
                errors.Add(Issue("missing_question", "error", "ClarificationPlanItem hat keine Klaerungsfrage.", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));
            if (string.IsNullOrWhiteSpace(item.Description))
                errors.Add(Issue("missing_description", "error", "ClarificationPlanItem hat keine Beschreibung.", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));

            if (item.SourceClarificationIds.Count == 0)
                errors.Add(Issue("missing_clarification_sources", "error", "ClarificationPlanItem hat keine sourceClarificationIds.", item.ClarificationPlanId, [], item.SourceRequirementIds));
            if (item.SourceRequirementIds.Count == 0)
                errors.Add(Issue("missing_requirement_sources", "error", "ClarificationPlanItem hat keine sourceRequirementIds.", item.ClarificationPlanId, item.SourceClarificationIds, []));

            foreach (var clarificationId in item.SourceClarificationIds)
            {
                if (!inputClarificationIds.Contains(clarificationId))
                    errors.Add(Issue("unknown_clarification_source", "error", $"SourceClarificationId ist nicht im ClarificationPlanningInput: {clarificationId}", item.ClarificationPlanId, [clarificationId], item.SourceRequirementIds));
                else
                    coveredClarificationIds.Add(clarificationId);
            }

            foreach (var requirementId in item.SourceRequirementIds)
            {
                if (!inputRequirementIds.Contains(requirementId))
                    errors.Add(Issue("unknown_requirement_source", "error", $"SourceRequirementId ist nicht im ClarificationPlanningInput: {requirementId}", item.ClarificationPlanId, item.SourceClarificationIds, [requirementId]));
                else
                    coveredRequirementIds.Add(requirementId);
            }

            if (IsCreateOperation(item.Operation) && item.AcceptanceCriteria.Count == 0)
                warnings.Add(Issue("missing_acceptance_criteria", "warning", "CREATE-Operation hat keine Acceptance Criteria.", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));

            if (item.SourceClarificationIds.Count > 3)
                warnings.Add(Issue("large_clarification_plan", "warning", "ClarificationPlanItem buendelt mehr als drei offene Punkte; pruefen, ob Split sinnvoll ist.", item.ClarificationPlanId, item.SourceClarificationIds, item.SourceRequirementIds));
        }

        var uncovered = inputClarificationIds.Except(coveredClarificationIds, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        foreach (var id in uncovered)
            errors.Add(Issue("unplanned_clarification_item", "error", $"ClarificationPlanningInputItem ist in keinem PlanItem enthalten: {id}", null, [id], []));

        var checks = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["inputItems"] = input.Items.Count,
            ["planItems"] = plan.Items.Count,
            ["coveredClarificationItems"] = coveredClarificationIds.Count,
            ["coveredRequirementItems"] = coveredRequirementIds.Count,
            ["uncoveredClarificationItems"] = uncovered.Count,
            ["operations"] = plan.Items.GroupBy(i => i.Operation, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key.ToUpperInvariant(), g => g.Count(), StringComparer.Ordinal)
        };

        return new ClarificationPlanGateReport(
            Pass: errors.Count == 0,
            Decision: errors.Count == 0 ? "pass" : "fail",
            Errors: errors,
            Warnings: warnings,
            Checks: checks);
    }

    private static bool IsCreateOperation(string operation)
        => operation.Equals("CREATE_CLARIFICATION_ISSUE", StringComparison.OrdinalIgnoreCase)
           || operation.Equals("CREATE_BREAKDOWN_ISSUE", StringComparison.OrdinalIgnoreCase);

    private static ClarificationPlanGateIssue Issue(
        string code,
        string severity,
        string message,
        string? clarificationPlanId,
        IReadOnlyList<string> sourceClarificationIds,
        IReadOnlyList<string> sourceRequirementIds)
        => new(code, severity, message, clarificationPlanId, sourceClarificationIds, sourceRequirementIds);
}
