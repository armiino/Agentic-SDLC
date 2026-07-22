using System.Text;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class OperationalizationAuditBuilder
{
    public static OperationalizationAuditDocument Build(OperationalizationAuditInput input)
    {
        var canonicalById = input.Canonical?.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal)
                            ?? new Dictionary<string, CanonicalRequirement>(StringComparer.Ordinal);
        var readinessById = input.Readiness?.Items.ToDictionary(r => r.RequirementId, StringComparer.Ordinal)
                            ?? new Dictionary<string, RequirementReadinessItem>(StringComparer.Ordinal);
        var issueInputById = input.IssuePlanningInput.Items.ToDictionary(i => i.RequirementId, StringComparer.Ordinal);
        var issuePlansByReq = IndexIssuePlans(input.AcceptedIssuePlan);
        var clarificationPlansByReq = IndexClarificationPlans(input.AcceptedClarificationPlan);
        var actionsByReq = IndexGithubActions(input.AcceptedGithubActionPlan);
        var dryRunsByAction = input.DryRun.Operations.ToDictionary(o => o.ActionId, StringComparer.Ordinal);
        var findings = new List<OperationalizationAuditFinding>();
        var requirementIds = canonicalById.Keys
            .Concat(readinessById.Keys)
            .Concat(issueInputById.Keys)
            .Concat(issuePlansByReq.Keys)
            .Concat(clarificationPlansByReq.Keys)
            .Concat(actionsByReq.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var audits = new List<OperationalizationRequirementAudit>();
        foreach (var requirementId in requirementIds)
        {
            canonicalById.TryGetValue(requirementId, out var canonical);
            readinessById.TryGetValue(requirementId, out var readiness);
            issueInputById.TryGetValue(requirementId, out var issueInput);
            issuePlansByReq.TryGetValue(requirementId, out var issuePlans);
            clarificationPlansByReq.TryGetValue(requirementId, out var clarificationPlans);
            actionsByReq.TryGetValue(requirementId, out var actions);
            issuePlans ??= [];
            clarificationPlans ??= [];
            actions ??= [];
            var dryRuns = actions
                .Select(a => dryRunsByAction.TryGetValue(a.ActionId, out var dryRun) ? dryRun : null)
                .Where(d => d is not null)
                .Select(d => d!)
                .ToArray();

            var operationalizationStatus = DetermineStatus(readiness, issueInput, issuePlans, clarificationPlans, actions);
            AddRequirementFindings(requirementId, readiness, issueInput, issuePlans, clarificationPlans, actions, dryRuns, findings);
            var findingCodes = findings
                .Where(f => string.Equals(f.RequirementId, requirementId, StringComparison.Ordinal))
                .Select(f => f.Code)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            audits.Add(new OperationalizationRequirementAudit(
                RequirementId: requirementId,
                Title: canonical?.Title ?? readiness?.Title ?? issueInput?.Title ?? requirementId,
                Status: canonical?.Status ?? readiness?.Status,
                Readiness: readiness?.Readiness,
                InIssuePlanningInput: issueInput is not null,
                IssuePlanIds: issuePlans.Select(i => i.IssuePlanId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                ClarificationPlanIds: clarificationPlans.Select(i => i.ClarificationPlanId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                GithubActionIds: actions.Select(a => a.ActionId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                GithubOperations: actions.Select(a => a.Operation.ToUpperInvariant()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                DryRunOperations: dryRuns.Select(d => d.Operation.ToUpperInvariant()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                TargetIssueNumbers: actions.Select(a => a.TargetIssueNumber).Where(n => n is not null).Select(n => n!.Value).Distinct().Order().ToArray(),
                OperationalizationStatus: operationalizationStatus,
                FindingCodes: findingCodes));
        }

        AddPlanFindings(input, findings);
        var operationByRequirement = audits
            .SelectMany(a => a.GithubOperations.DefaultIfEmpty("NOT_IN_ISSUE_PLANNING").Select(op => new { op, a.RequirementId }))
            .GroupBy(x => x.op, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key.ToUpperInvariant(), g => g.Select(x => x.RequirementId).Distinct(StringComparer.Ordinal).Count(), StringComparer.Ordinal);
        var findingsBySeverity = findings
            .GroupBy(f => f.Severity, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key.ToLowerInvariant(), g => g.Count(), StringComparer.Ordinal);
        var blockedRequirements = audits.Count(a => a.GithubOperations.Contains("NEEDS_REVIEW", StringComparer.OrdinalIgnoreCase));
        var noChangeRequirements = audits.Count(a => a.GithubOperations.Count == 1 && a.GithubOperations.Contains("NO_CHANGE", StringComparer.OrdinalIgnoreCase));
        var requirementsWithDeliveryCoverage = audits.Count(a => a.IssuePlanIds.Count > 0 && a.GithubOperations.Any(op => !op.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase)));
        var requirementsWithClarificationCoverage = audits.Count(a => a.ClarificationPlanIds.Count > 0);
        var requirementsWithoutOperationalCoverage = audits.Count(a => a.OperationalizationStatus.Equals("missing_operational_coverage", StringComparison.OrdinalIgnoreCase));
        var summary = new OperationalizationAuditSummary(
            CanonicalRequirements: canonicalById.Count,
            ReadinessItems: readinessById.Count,
            IssuePlanningInputItems: input.IssuePlanningInput.Items.Count,
            IssuePlanItems: input.AcceptedIssuePlan.Items.Count,
            GithubActions: input.AcceptedGithubActionPlan.Actions.Count,
            ClarificationPlanItems: input.AcceptedClarificationPlan?.Items.Count ?? 0,
            DryRunOperations: input.DryRun.Operations.Count,
            CoveredIssuePlanningInputItems: issueInputById.Keys.Count(id => issuePlansByReq.ContainsKey(id)),
            UncoveredIssuePlanningInputItems: issueInputById.Keys.Count(id => !issuePlansByReq.ContainsKey(id)),
            RequirementsWithDeliveryCoverage: requirementsWithDeliveryCoverage,
            RequirementsWithClarificationCoverage: requirementsWithClarificationCoverage,
            RequirementsWithoutOperationalCoverage: requirementsWithoutOperationalCoverage,
            RequirementsByGithubOperation: operationByRequirement,
            BlockedRequirements: blockedRequirements,
            NoChangeRequirements: noChangeRequirements,
            FindingsBySeverity: findingsBySeverity,
            ReadyForGithubWrite: input.DryRun.ReadyForExecute && findings.All(f => !f.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)));

        return new OperationalizationAuditDocument(
            SchemaVersion: OperationalizationAuditDocument.CurrentSchemaVersion,
            CreatedUtc: DateTime.UtcNow,
            BaselineId: input.AcceptedGithubActionPlan.BaselineId,
            ProjectId: input.AcceptedGithubActionPlan.ProjectId,
            SourcePaths: input.SourcePaths,
            Summary: summary,
            Requirements: audits,
            Findings: findings.OrderByDescending(f => SeverityRank(f.Severity)).ThenBy(f => f.RequirementId, StringComparer.Ordinal).ThenBy(f => f.Code, StringComparer.Ordinal).ToArray());
    }

    public static string RenderMarkdown(OperationalizationAuditDocument audit)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Operationalization Audit");
        sb.AppendLine();
        sb.AppendLine($"Baseline: `{audit.BaselineId}`");
        sb.AppendLine($"Project: `{audit.ProjectId}`");
        sb.AppendLine($"Ready for GitHub write: `{audit.Summary.ReadyForGithubWrite}`");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- Canonical requirements: `{audit.Summary.CanonicalRequirements}`");
        sb.AppendLine($"- Readiness items: `{audit.Summary.ReadinessItems}`");
        sb.AppendLine($"- IssuePlanning input items: `{audit.Summary.IssuePlanningInputItems}`");
        sb.AppendLine($"- IssuePlan items: `{audit.Summary.IssuePlanItems}`");
        sb.AppendLine($"- GitHub actions: `{audit.Summary.GithubActions}`");
        sb.AppendLine($"- ClarificationPlan items: `{audit.Summary.ClarificationPlanItems}`");
        sb.AppendLine($"- Dry-run operations: `{audit.Summary.DryRunOperations}`");
        sb.AppendLine($"- Covered IssuePlanning input items: `{audit.Summary.CoveredIssuePlanningInputItems}`");
        sb.AppendLine($"- Uncovered IssuePlanning input items: `{audit.Summary.UncoveredIssuePlanningInputItems}`");
        sb.AppendLine($"- Requirements with delivery coverage: `{audit.Summary.RequirementsWithDeliveryCoverage}`");
        sb.AppendLine($"- Requirements with clarification coverage: `{audit.Summary.RequirementsWithClarificationCoverage}`");
        sb.AppendLine($"- Requirements without operational coverage: `{audit.Summary.RequirementsWithoutOperationalCoverage}`");
        sb.AppendLine($"- Blocked requirements: `{audit.Summary.BlockedRequirements}`");
        sb.AppendLine($"- NO_CHANGE-only requirements: `{audit.Summary.NoChangeRequirements}`");
        sb.AppendLine();
        sb.AppendLine("## Operations");
        sb.AppendLine();
        foreach (var kv in audit.Summary.RequirementsByGithubOperation.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            sb.AppendLine($"- `{kv.Key}`: `{kv.Value}` requirements");
        sb.AppendLine();
        sb.AppendLine("## Findings");
        sb.AppendLine();
        if (audit.Findings.Count == 0)
        {
            sb.AppendLine("No findings.");
            sb.AppendLine();
        }
        else
        {
            foreach (var finding in audit.Findings)
                sb.AppendLine($"- `{finding.Severity}` `{finding.Code}` req=`{finding.RequirementId ?? "-"}` issuePlan=`{finding.IssuePlanId ?? "-"}` action=`{finding.GithubActionId ?? "-"}`: {finding.Message}");
            sb.AppendLine();
        }
        sb.AppendLine("## Requirements");
        sb.AppendLine();
        foreach (var req in audit.Requirements)
        {
            sb.AppendLine($"- `{req.RequirementId}` {req.Title}");
            sb.AppendLine($"  - readiness: `{req.Readiness ?? "-"}`, inIssuePlanningInput: `{req.InIssuePlanningInput}`");
            sb.AppendLine($"  - operationalizationStatus: `{req.OperationalizationStatus}`");
            sb.AppendLine($"  - issuePlanIds: `{string.Join(", ", req.IssuePlanIds)}`");
            sb.AppendLine($"  - clarificationPlanIds: `{string.Join(", ", req.ClarificationPlanIds)}`");
            sb.AppendLine($"  - githubActions: `{string.Join(", ", req.GithubActionIds)}`");
            sb.AppendLine($"  - operations: `{string.Join(", ", req.GithubOperations)}`");
            if (req.TargetIssueNumbers.Count > 0)
                sb.AppendLine($"  - targetIssues: `{string.Join(", ", req.TargetIssueNumbers.Select(n => "#" + n))}`");
            if (req.FindingCodes.Count > 0)
                sb.AppendLine($"  - findings: `{string.Join(", ", req.FindingCodes)}`");
        }
        return sb.ToString();
    }

    private static Dictionary<string, IReadOnlyList<IssuePlanItem>> IndexIssuePlans(IssuePlanDocument plan)
        => plan.Items
            .SelectMany(item => item.SourceRequirementIds.Select(reqId => new { reqId, item }))
            .GroupBy(x => x.reqId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<IssuePlanItem>)g.Select(x => x.item).ToArray(), StringComparer.Ordinal);

    private static Dictionary<string, IReadOnlyList<ClarificationPlanItem>> IndexClarificationPlans(ClarificationPlanDocument? plan)
        => plan?.Items
            .SelectMany(item => item.SourceRequirementIds.Select(reqId => new { reqId, item }))
            .GroupBy(x => x.reqId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ClarificationPlanItem>)g.Select(x => x.item).ToArray(), StringComparer.Ordinal)
           ?? new Dictionary<string, IReadOnlyList<ClarificationPlanItem>>(StringComparer.Ordinal);

    private static Dictionary<string, IReadOnlyList<GithubActionPlanItem>> IndexGithubActions(GithubActionPlanDocument plan)
        => plan.Actions
            .SelectMany(action => action.SourceRequirementIds.Select(reqId => new { reqId, action }))
            .GroupBy(x => x.reqId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<GithubActionPlanItem>)g.Select(x => x.action).ToArray(), StringComparer.Ordinal);

    private static void AddRequirementFindings(
        string requirementId,
        RequirementReadinessItem? readiness,
        IssuePlanningInputItem? issueInput,
        IReadOnlyList<IssuePlanItem> issuePlans,
        IReadOnlyList<ClarificationPlanItem> clarificationPlans,
        IReadOnlyList<GithubActionPlanItem> actions,
        IReadOnlyList<GithubWriteDryRunOperation> dryRuns,
        List<OperationalizationAuditFinding> findings)
    {
        if (issueInput is not null && issuePlans.Count == 0)
            findings.Add(Finding("uncovered_issue_planning_input", "error", "Requirement ist im IssuePlanningInput, aber in keinem IssuePlanItem enthalten.", requirementId, null, null));
        if (issuePlans.Count > 0 && actions.Count == 0)
            findings.Add(Finding("missing_github_action", "error", "Requirement ist in IssuePlanItems enthalten, aber in keiner GitHubAction.", requirementId, issuePlans[0].IssuePlanId, null));
        if (actions.Count > 0 && actions.All(a => a.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase)))
            findings.Add(Finding("no_change_only", "info", "Requirement ist bewusst ohne GitHub-Write operationalisiert.", requirementId, actions[0].IssuePlanId, actions[0].ActionId));
        if (issueInput is null && issuePlans.Count == 0 && clarificationPlans.Count == 0 && IsDeferred(readiness))
            findings.Add(Finding("deferred_without_issue", "info", "Requirement ist als deferred/optional markiert und bewusst nicht operationalisiert.", requirementId, null, null));
        else if (issueInput is null && issuePlans.Count == 0 && clarificationPlans.Count == 0)
            findings.Add(Finding("missing_operational_coverage", "warning", "Requirement hat weder Delivery- noch Klaerungsabdeckung im aktuellen Operationalisierungsstand.", requirementId, null, null));
        if (issuePlans.Count > 0 && clarificationPlans.Count > 0)
            findings.Add(Finding("delivery_and_clarification", "info", "Requirement ist sowohl in Delivery- als auch Klaerungsarbeit enthalten; Links/Abhaengigkeiten pruefen.", requirementId, issuePlans[0].IssuePlanId, actions.FirstOrDefault()?.ActionId));
        foreach (var dryRun in dryRuns.Where(d => d.Blocked))
            findings.Add(Finding("dry_run_blocked", "warning", dryRun.BlockReason ?? "Dry-run Operation blockiert.", requirementId, dryRun.IssuePlanId, dryRun.ActionId));
    }

    private static string DetermineStatus(
        RequirementReadinessItem? readiness,
        IssuePlanningInputItem? issueInput,
        IReadOnlyList<IssuePlanItem> issuePlans,
        IReadOnlyList<ClarificationPlanItem> clarificationPlans,
        IReadOnlyList<GithubActionPlanItem> actions)
    {
        if (actions.Any(a => a.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase)
                             || a.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase)
                             || a.Operation.Equals("REOPEN", StringComparison.OrdinalIgnoreCase)
                             || a.Operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase)))
            return clarificationPlans.Count > 0 ? "delivery_with_clarification" : "delivery";
        if (issuePlans.Count > 0 && actions.Count == 0)
            return "planned_without_github_action";
        if (clarificationPlans.Count > 0)
            return "clarification";
        if (actions.Count > 0 && actions.All(a => a.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase)))
            return "no_change";
        if (issueInput is null && issuePlans.Count == 0 && clarificationPlans.Count == 0 && IsDeferred(readiness))
            return "deferred";
        if (issueInput is not null)
            return "issue_planning_input_uncovered";
        return "missing_operational_coverage";
    }

    private static bool IsDeferred(RequirementReadinessItem? readiness)
        => readiness?.Readiness.Equals("deferred_or_optional", StringComparison.OrdinalIgnoreCase) == true
           || readiness?.Status.Equals("deferred", StringComparison.OrdinalIgnoreCase) == true
           || readiness?.Status.Equals("optional", StringComparison.OrdinalIgnoreCase) == true;

    private static void AddPlanFindings(OperationalizationAuditInput input, List<OperationalizationAuditFinding> findings)
    {
        var issuePlanGateLargeWarnings = (input.AcceptedIssuePlanGate?.Warnings ?? [])
            .Where(w => w.Code.Equals("large_issue_plan", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(w.IssuePlanId))
            .Select(w => w.IssuePlanId!)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var item in input.AcceptedIssuePlan.Items.Where(i => i.SourceRequirementIds.Count > 4 && !issuePlanGateLargeWarnings.Contains(i.IssuePlanId)))
            findings.Add(Finding("large_issue_plan", "warning", "IssuePlanItem umfasst mehr als vier Requirements; Split pruefen.", null, item.IssuePlanId, null));
        foreach (var warning in input.AcceptedIssuePlanGate?.Warnings ?? [])
            findings.Add(Finding($"issue_plan_gate_{warning.Code}", warning.Severity, warning.Message, null, warning.IssuePlanId, null));
        foreach (var warning in input.AcceptedGithubActionGate?.Warnings ?? [])
            findings.Add(Finding($"github_action_gate_{warning.Code}", warning.Severity, warning.Message, null, warning.IssuePlanId, warning.ActionId));
    }

    private static OperationalizationAuditFinding Finding(string code, string severity, string message, string? requirementId, string? issuePlanId, string? githubActionId)
        => new(code, severity, message, requirementId, issuePlanId, githubActionId);

    private static int SeverityRank(string severity)
        => severity.ToLowerInvariant() switch
        {
            "error" => 3,
            "warning" => 2,
            "info" => 1,
            _ => 0
        };
}

public sealed record OperationalizationAuditInput(
    CanonicalRequirementsBaseline? Canonical,
    RequirementsReadinessReport? Readiness,
    IssuePlanningInput IssuePlanningInput,
    IssuePlanDocument AcceptedIssuePlan,
    IssuePlanGateReport? AcceptedIssuePlanGate,
    GithubActionPlanDocument AcceptedGithubActionPlan,
    GithubActionPlanGateReport? AcceptedGithubActionGate,
    GithubWriteDryRunDocument DryRun,
    ClarificationPlanDocument? AcceptedClarificationPlan,
    OperationalizationAuditSourcePaths SourcePaths);
