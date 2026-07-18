namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubActionPlanGate
{
    private static readonly HashSet<string> AllowedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "CREATE", "UPDATE", "LINK", "REOPEN", "NO_CHANGE", "NEEDS_REVIEW"
    };

    public static GithubActionPlanGateReport Check(GithubReconciliationInput input, GithubActionPlanDocument plan)
    {
        var errors = new List<GithubActionPlanGateIssue>();
        var warnings = new List<GithubActionPlanGateIssue>();
        var issuePlanItems = input.AcceptedIssuePlan.Items.ToDictionary(i => i.IssuePlanId, StringComparer.Ordinal);
        var actionIds = new HashSet<string>(StringComparer.Ordinal);
        var coveredIssuePlanIds = new HashSet<string>(StringComparer.Ordinal);

        if (plan.SchemaVersion != GithubActionPlanDocument.CurrentSchemaVersion)
            errors.Add(Issue("schema_version_mismatch", "error", $"SchemaVersion {plan.SchemaVersion} ist nicht erlaubt.", null, null));
        if (!string.Equals(plan.ProjectId, input.ProjectId, StringComparison.Ordinal))
            errors.Add(Issue("project_mismatch", "error", "GithubActionPlan projectId passt nicht zum Input.", null, null));
        if (!string.Equals(plan.BaselineId, input.BaselineId, StringComparison.Ordinal))
            errors.Add(Issue("baseline_mismatch", "error", "GithubActionPlan baselineId passt nicht zum Input.", null, null));

        foreach (var action in plan.Actions)
        {
            if (string.IsNullOrWhiteSpace(action.ActionId))
                errors.Add(Issue("missing_action_id", "error", "Action hat keine actionId.", null, action.IssuePlanId));
            else if (!actionIds.Add(action.ActionId))
                errors.Add(Issue("duplicate_action_id", "error", $"ActionId kommt mehrfach vor: {action.ActionId}", action.ActionId, action.IssuePlanId));

            if (!AllowedOperations.Contains(action.Operation))
                errors.Add(Issue("invalid_operation", "error", $"Operation ist nicht erlaubt: {action.Operation}", action.ActionId, action.IssuePlanId));

            if (!issuePlanItems.TryGetValue(action.IssuePlanId, out var source))
            {
                errors.Add(Issue("unknown_issue_plan_id", "error", $"IssuePlanId ist nicht im accepted IssuePlan: {action.IssuePlanId}", action.ActionId, action.IssuePlanId));
                continue;
            }
            coveredIssuePlanIds.Add(action.IssuePlanId);

            if (action.SourceRequirementIds.Count == 0)
                errors.Add(Issue("missing_source_requirements", "error", "Action hat keine sourceRequirementIds.", action.ActionId, action.IssuePlanId));
            foreach (var sourceRequirementId in action.SourceRequirementIds)
            {
                if (!source.SourceRequirementIds.Contains(sourceRequirementId, StringComparer.Ordinal))
                    errors.Add(Issue("unsupported_source_requirement", "error", $"SourceRequirementId gehoert nicht zum IssuePlanItem: {sourceRequirementId}", action.ActionId, action.IssuePlanId));
            }

            if (string.IsNullOrWhiteSpace(action.Reason))
                errors.Add(Issue("missing_reason", "error", "Action hat keine Begruendung.", action.ActionId, action.IssuePlanId));
            if (action.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                if (action.TargetIssueNumber is not null)
                    errors.Add(Issue("create_has_target_issue", "error", "CREATE darf kein targetIssueNumber haben.", action.ActionId, action.IssuePlanId));
                if (string.IsNullOrWhiteSpace(action.Title))
                    errors.Add(Issue("missing_title", "error", "CREATE hat keinen Titel.", action.ActionId, action.IssuePlanId));
                if (string.IsNullOrWhiteSpace(action.Body))
                    errors.Add(Issue("missing_body", "error", "CREATE hat keinen Body.", action.ActionId, action.IssuePlanId));
                if (action.AcceptanceCriteria.Count == 0)
                    warnings.Add(Issue("missing_acceptance_criteria", "warning", "CREATE hat keine Acceptance Criteria.", action.ActionId, action.IssuePlanId));
            }
            if (IsTargetedOperation(action.Operation))
            {
                if (action.TargetIssueNumber is null)
                    errors.Add(Issue("target_issue_required", "error", $"{action.Operation} braucht targetIssueNumber.", action.ActionId, action.IssuePlanId));
            }
            if (action.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase) && action.TargetIssueNumber is not null)
                warnings.Add(Issue("no_change_with_target", "warning", "NO_CHANGE mit targetIssueNumber sollte begruendet sein.", action.ActionId, action.IssuePlanId));

            foreach (var finding in GithubIssueQualityRules.Check(action))
            {
                var issue = Issue(finding.Code, finding.Severity, finding.Message, finding.ActionId, finding.IssuePlanId);
                if (finding.Severity.Equals("error", StringComparison.OrdinalIgnoreCase))
                    errors.Add(issue);
                else
                    warnings.Add(issue);
            }

            if (action.TargetIssueNumber is null
                && (action.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase)
                    || action.Operation.Equals("NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase)))
            {
                var candidate = GithubIssueCandidateMatcher.Best(source, input.ExistingIssues);
                if (candidate is not null)
                    warnings.Add(Issue(
                        "possible_existing_issue_match",
                        "warning",
                        $"Moegliches vorhandenes GitHub-Issue #{candidate.Issue.IssueNumber} ({candidate.Issue.State}, score {candidate.Score}): {candidate.Issue.Title}",
                        action.ActionId,
                        action.IssuePlanId));
            }
        }

        foreach (var id in issuePlanItems.Keys.Except(coveredIssuePlanIds, StringComparer.Ordinal).Order(StringComparer.Ordinal))
            errors.Add(Issue("unplanned_issue_plan_item", "error", $"Accepted IssuePlanItem ist in keiner GitHubAction enthalten: {id}", null, id));

        var checks = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["acceptedIssuePlanItems"] = input.AcceptedIssuePlan.Items.Count,
            ["actions"] = plan.Actions.Count,
            ["coveredIssuePlanItems"] = coveredIssuePlanIds.Count,
            ["uncoveredIssuePlanItems"] = issuePlanItems.Count - coveredIssuePlanIds.Count,
            ["existingIssues"] = input.ExistingIssues.Count,
            ["existingMappings"] = input.ExistingMappings.Count,
            ["candidateMatches"] = input.AcceptedIssuePlan.Items.Count(i => GithubIssueCandidateMatcher.Best(i, input.ExistingIssues) is not null),
            ["operations"] = plan.Actions.GroupBy(a => a.Operation, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToUpperInvariant(), g => g.Count(), StringComparer.Ordinal)
        };

        return new GithubActionPlanGateReport(
            Pass: errors.Count == 0,
            Decision: errors.Count == 0 ? "pass" : "fail",
            Errors: errors,
            Warnings: warnings,
            Checks: checks);
    }

    private static GithubActionPlanGateIssue Issue(string code, string severity, string message, string? actionId, string? issuePlanId)
        => new(code, severity, message, actionId, issuePlanId);

    private static bool IsTargetedOperation(string operation)
        => operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase)
           || operation.Equals("LINK", StringComparison.OrdinalIgnoreCase)
           || operation.Equals("REOPEN", StringComparison.OrdinalIgnoreCase);
}
