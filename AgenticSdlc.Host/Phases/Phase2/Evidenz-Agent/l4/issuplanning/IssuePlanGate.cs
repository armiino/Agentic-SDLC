namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanGate
{
    private static readonly HashSet<string> AllowedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "CREATE", "LINK", "NO_CHANGE", "NEEDS_REVIEW"
    };

    public static IssuePlanGateReport Check(IssuePlanningInput input, IssuePlanDocument plan)
    {
        var errors = new List<IssuePlanGateIssue>();
        var warnings = new List<IssuePlanGateIssue>();
        var inputRequirementIds = input.Items.Select(i => i.RequirementId).ToHashSet(StringComparer.Ordinal);
        var planIds = new HashSet<string>(StringComparer.Ordinal);
        var coveredIds = new HashSet<string>(StringComparer.Ordinal);

        if (plan.SchemaVersion != IssuePlanDocument.CurrentSchemaVersion)
        {
            errors.Add(Issue("schema_version_mismatch", "error", $"SchemaVersion {plan.SchemaVersion} ist nicht erlaubt.", null, []));
        }

        if (!string.Equals(plan.ProjectId, input.ProjectId, StringComparison.Ordinal))
        {
            errors.Add(Issue("project_mismatch", "error", "IssuePlan projectId passt nicht zum IssuePlanningInput.", null, []));
        }

        if (!string.Equals(plan.BaselineId, input.BaselineId, StringComparison.Ordinal))
        {
            errors.Add(Issue("baseline_mismatch", "error", "IssuePlan baselineId passt nicht zum IssuePlanningInput.", null, []));
        }

        foreach (var item in plan.Items)
        {
            if (string.IsNullOrWhiteSpace(item.IssuePlanId))
            {
                errors.Add(Issue("missing_issue_plan_id", "error", "IssuePlanItem hat keine issuePlanId.", null, item.SourceRequirementIds));
            }
            else if (!planIds.Add(item.IssuePlanId))
            {
                errors.Add(Issue("duplicate_issue_plan_id", "error", $"IssuePlanId kommt mehrfach vor: {item.IssuePlanId}", item.IssuePlanId, item.SourceRequirementIds));
            }

            if (!AllowedOperations.Contains(item.Operation))
            {
                errors.Add(Issue("invalid_operation", "error", $"Operation ist nicht erlaubt: {item.Operation}", item.IssuePlanId, item.SourceRequirementIds));
            }

            if (string.IsNullOrWhiteSpace(item.Title))
                errors.Add(Issue("missing_title", "error", "IssuePlanItem hat keinen Titel.", item.IssuePlanId, item.SourceRequirementIds));
            if (string.IsNullOrWhiteSpace(item.Description))
                errors.Add(Issue("missing_description", "error", "IssuePlanItem hat keine Beschreibung.", item.IssuePlanId, item.SourceRequirementIds));

            if (item.SourceRequirementIds.Count == 0)
            {
                errors.Add(Issue("missing_sources", "error", "IssuePlanItem hat keine sourceRequirementIds.", item.IssuePlanId, []));
            }

            foreach (var sourceId in item.SourceRequirementIds)
            {
                if (!inputRequirementIds.Contains(sourceId))
                {
                    errors.Add(Issue("unknown_or_not_ready_source", "error", $"SourceRequirementId ist nicht im IssuePlanningInput freigegeben: {sourceId}", item.IssuePlanId, [sourceId]));
                }
                else
                {
                    coveredIds.Add(sourceId);
                }
            }

            if (item.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase) && item.AcceptanceCriteria.Count == 0)
            {
                warnings.Add(Issue("missing_acceptance_criteria", "warning", "CREATE-Plan hat keine Acceptance Criteria.", item.IssuePlanId, item.SourceRequirementIds));
            }

            if (item.SourceRequirementIds.Count > 4)
            {
                warnings.Add(Issue("large_issue_plan", "warning", "IssuePlanItem umfasst mehr als vier Requirements; pruefen, ob Split sinnvoll ist.", item.IssuePlanId, item.SourceRequirementIds));
            }
        }

        var uncovered = inputRequirementIds.Except(coveredIds, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        foreach (var id in uncovered)
        {
            errors.Add(Issue("unplanned_ready_requirement", "error", $"Ready Requirement ist in keinem IssuePlanItem enthalten: {id}", null, [id]));
        }

        var checks = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["readyInputItems"] = input.Items.Count,
            ["issuePlanItems"] = plan.Items.Count,
            ["coveredReadyItems"] = coveredIds.Count,
            ["uncoveredReadyItems"] = uncovered.Count,
            ["operations"] = plan.Items.GroupBy(i => i.Operation, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key.ToUpperInvariant(), g => g.Count(), StringComparer.Ordinal)
        };

        return new IssuePlanGateReport(
            Pass: errors.Count == 0,
            Decision: errors.Count == 0 ? "pass" : "fail",
            Errors: errors,
            Warnings: warnings,
            Checks: checks);
    }

    private static IssuePlanGateIssue Issue(string code, string severity, string message, string? issuePlanId, IReadOnlyList<string> sourceRequirementIds)
        => new(code, severity, message, issuePlanId, sourceRequirementIds);
}
