using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4ConsolidationGate
{
    private static readonly HashSet<string> AllowedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "KEEP", "ADD", "MERGE", "SPLIT", "REVISE", "DEPRECATE", "LINK", "MARK_OPEN_DECISION"
    };

    private static readonly HashSet<string> TargetProducingOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "KEEP", "ADD", "MERGE", "SPLIT", "REVISE", "MARK_OPEN_DECISION"
    };

    private static readonly HashSet<string> HumanReviewOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADD", "MERGE", "SPLIT", "REVISE", "DEPRECATE", "MARK_OPEN_DECISION"
    };

    private static readonly HashSet<string> AllowedTargetStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active", "open_decision", "deprecated", "proposed"
    };

    public static ConsolidationGateReport Check(ProjectStateDocument state, ConsolidationPlan plan)
    {
        var errors = new List<ConsolidationGateIssue>();
        var warnings = new List<ConsolidationGateIssue>();
        var activeRequirements = state.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .Where(i => IsActiveStatus(i.Status))
            .ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var allItems = state.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        if (plan.SchemaVersion != ConsolidationPlan.CurrentSchemaVersion)
            errors.Add(Issue("SCHEMA_VERSION", "error", $"Unsupported ConsolidationPlan schemaVersion {plan.SchemaVersion}.", null, []));
        if (!string.Equals(plan.ProjectId, state.ProjectId, StringComparison.Ordinal))
            errors.Add(Issue("PROJECT_MISMATCH", "error", $"Plan projectId '{plan.ProjectId}' does not match ProjectState '{state.ProjectId}'.", null, []));
        if (plan.Operations.Count == 0)
            errors.Add(Issue("EMPTY_PLAN", "error", "ConsolidationPlan must contain at least one operation.", null, []));

        var operationIds = new HashSet<string>(StringComparer.Ordinal);
        var materializedSourceUsage = new Dictionary<string, string>(StringComparer.Ordinal);
        var targetIds = new HashSet<string>(StringComparer.Ordinal);
        var coveredActiveSources = new HashSet<string>(StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (string.IsNullOrWhiteSpace(op.OperationId))
                errors.Add(Issue("MISSING_OPERATION_ID", "error", "Operation must have operationId.", null, op.SourceItemIds));
            else if (!operationIds.Add(op.OperationId))
                errors.Add(Issue("DUPLICATE_OPERATION_ID", "error", $"Duplicate operationId '{op.OperationId}'.", op.OperationId, op.SourceItemIds));

            var operation = op.Operation?.Trim() ?? string.Empty;
            if (!AllowedOperations.Contains(operation))
            {
                errors.Add(Issue("UNKNOWN_OPERATION", "error", $"Unknown operation '{op.Operation}'.", op.OperationId, op.SourceItemIds));
                continue;
            }

            foreach (var sourceItemId in op.SourceItemIds.Distinct(StringComparer.Ordinal))
            {
                if (!allItems.ContainsKey(sourceItemId))
                {
                    errors.Add(Issue("UNKNOWN_SOURCE_ITEM", "error", $"Source item '{sourceItemId}' does not exist.", op.OperationId, [sourceItemId]));
                    continue;
                }

                if (activeRequirements.ContainsKey(sourceItemId))
                    coveredActiveSources.Add(sourceItemId);
            }

            CheckOperationShape(op, operation, errors, warnings);

            if (HumanReviewOperations.Contains(operation) && !op.RequiresHumanReview)
            {
                errors.Add(Issue(
                    "HUMAN_REVIEW_REQUIRED",
                    "error",
                    $"Operation '{operation}' changes project semantics and must require human review.",
                    op.OperationId,
                    op.SourceItemIds));
            }

            if (TargetProducingOperations.Contains(operation))
            {
                foreach (var sourceItemId in op.SourceItemIds.Where(activeRequirements.ContainsKey))
                {
                    if (materializedSourceUsage.TryGetValue(sourceItemId, out var previous))
                    {
                        errors.Add(Issue(
                            "SOURCE_MATERIALIZED_TWICE",
                            "error",
                            $"Source item '{sourceItemId}' is materialized by both '{previous}' and '{op.OperationId}'.",
                            op.OperationId,
                            [sourceItemId]));
                    }
                    else
                    {
                        materializedSourceUsage[sourceItemId] = op.OperationId;
                    }
                }
            }

            foreach (var target in op.Targets)
            {
                if (string.IsNullOrWhiteSpace(target.RequirementId))
                {
                    errors.Add(Issue("MISSING_TARGET_ID", "error", "Target requirement must have requirementId.", op.OperationId, op.SourceItemIds));
                    continue;
                }
                if (!targetIds.Add(target.RequirementId))
                    errors.Add(Issue("DUPLICATE_TARGET_ID", "error", $"Duplicate target requirementId '{target.RequirementId}'.", op.OperationId, op.SourceItemIds));
                if (string.IsNullOrWhiteSpace(target.Text))
                    errors.Add(Issue("EMPTY_TARGET_TEXT", "error", $"Target '{target.RequirementId}' has empty text.", op.OperationId, op.SourceItemIds));
                if (string.IsNullOrWhiteSpace(target.Title))
                    warnings.Add(Issue("EMPTY_TARGET_TITLE", "warning", $"Target '{target.RequirementId}' has empty title.", op.OperationId, op.SourceItemIds));
                if (!AllowedTargetStatuses.Contains(target.Status))
                    errors.Add(Issue("INVALID_TARGET_STATUS", "error", $"Target '{target.RequirementId}' has invalid status '{target.Status}'.", op.OperationId, op.SourceItemIds));
                if (operation.Equals("MARK_OPEN_DECISION", StringComparison.OrdinalIgnoreCase)
                    && !target.Status.Equals("open_decision", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(Issue("OPEN_DECISION_STATUS_REQUIRED", "error", "MARK_OPEN_DECISION target must have status open_decision.", op.OperationId, op.SourceItemIds));
                }
            }
        }

        foreach (var missing in activeRequirements.Keys.Except(coveredActiveSources, StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            errors.Add(Issue("UNCOVERED_ACTIVE_REQUIREMENT", "error", $"Active requirement '{missing}' is not covered by any consolidation operation.", null, [missing]));
        }

        var checks = new Dictionary<string, object>
        {
            ["projectItems"] = state.Items.Count,
            ["activeRequirements"] = activeRequirements.Count,
            ["operations"] = plan.Operations.Count,
            ["targetRequirements"] = targetIds.Count,
            ["coveredActiveRequirements"] = coveredActiveSources.Count,
            ["errors"] = errors.Count,
            ["warnings"] = warnings.Count
        };
        var pass = errors.Count == 0;
        return new ConsolidationGateReport(
            Pass: pass,
            Decision: pass ? "pass" : "revise",
            Errors: errors,
            Warnings: warnings,
            Checks: checks);
    }

    private static void CheckOperationShape(
        ConsolidationOperation op,
        string operation,
        List<ConsolidationGateIssue> errors,
        List<ConsolidationGateIssue> warnings)
    {
        var sourceCount = op.SourceItemIds.Distinct(StringComparer.Ordinal).Count();
        var targetCount = op.Targets.Count;
        switch (operation.ToUpperInvariant())
        {
            case "KEEP":
                Require(sourceCount == 1, "KEEP_SOURCE_COUNT", "KEEP requires exactly one source item.", op, errors);
                Require(targetCount == 1, "KEEP_TARGET_COUNT", "KEEP requires exactly one target requirement.", op, errors);
                break;
            case "ADD":
                Require(targetCount == 1, "ADD_TARGET_COUNT", "ADD requires exactly one target requirement.", op, errors);
                if (sourceCount == 0)
                    warnings.Add(Issue("ADD_WITHOUT_SOURCE", "warning", "ADD has no source item; this must remain explicit human-authorized open-world work.", op.OperationId, op.SourceItemIds));
                break;
            case "MERGE":
                Require(sourceCount >= 2, "MERGE_SOURCE_COUNT", "MERGE requires at least two source items.", op, errors);
                Require(targetCount == 1, "MERGE_TARGET_COUNT", "MERGE requires exactly one target requirement.", op, errors);
                break;
            case "SPLIT":
                Require(sourceCount == 1, "SPLIT_SOURCE_COUNT", "SPLIT requires exactly one source item.", op, errors);
                Require(targetCount >= 2, "SPLIT_TARGET_COUNT", "SPLIT requires at least two target requirements.", op, errors);
                break;
            case "REVISE":
                Require(sourceCount >= 1, "REVISE_SOURCE_COUNT", "REVISE requires at least one source item.", op, errors);
                Require(targetCount == 1, "REVISE_TARGET_COUNT", "REVISE requires exactly one target requirement.", op, errors);
                Require(!string.IsNullOrWhiteSpace(op.Rationale), "RATIONALE_REQUIRED", "REVISE requires rationale.", op, errors);
                break;
            case "DEPRECATE":
                Require(sourceCount >= 1, "DEPRECATE_SOURCE_COUNT", "DEPRECATE requires at least one source item.", op, errors);
                Require(targetCount == 0, "DEPRECATE_TARGET_COUNT", "DEPRECATE must not create target requirements.", op, errors);
                Require(!string.IsNullOrWhiteSpace(op.Rationale), "RATIONALE_REQUIRED", "DEPRECATE requires rationale.", op, errors);
                break;
            case "LINK":
                Require(sourceCount >= 2, "LINK_SOURCE_COUNT", "LINK requires at least two source items.", op, errors);
                Require(targetCount == 0, "LINK_TARGET_COUNT", "LINK must not create target requirements.", op, errors);
                break;
            case "MARK_OPEN_DECISION":
                Require(sourceCount >= 1, "OPEN_DECISION_SOURCE_COUNT", "MARK_OPEN_DECISION requires at least one source item.", op, errors);
                Require(targetCount == 1, "OPEN_DECISION_TARGET_COUNT", "MARK_OPEN_DECISION requires exactly one target.", op, errors);
                break;
        }
    }

    private static void Require(bool condition, string code, string message, ConsolidationOperation op, List<ConsolidationGateIssue> errors)
    {
        if (!condition)
            errors.Add(Issue(code, "error", message, op.OperationId, op.SourceItemIds));
    }

    private static bool IsActiveStatus(string status)
        => status.Equals("baseline", StringComparison.OrdinalIgnoreCase)
           || status.Equals("accepted", StringComparison.OrdinalIgnoreCase);

    private static ConsolidationGateIssue Issue(string code, string severity, string message, string? operationId, IReadOnlyList<string> sourceItemIds)
        => new(code, severity, message, operationId, sourceItemIds);
}
