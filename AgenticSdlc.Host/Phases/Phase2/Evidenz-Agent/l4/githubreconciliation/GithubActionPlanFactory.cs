namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubActionPlanFactory
{
    public static GithubActionPlanDocument CreateSeed(GithubReconciliationInput input)
    {
        var actions = input.AcceptedIssuePlan.Items
            .OrderBy(i => i.IssuePlanId, StringComparer.Ordinal)
            .Select((item, index) => CreateSeedAction(input, item, index + 1))
            .ToList();

        return new GithubActionPlanDocument(
            SchemaVersion: GithubActionPlanDocument.CurrentSchemaVersion,
            PlanId: $"github-action-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAcceptedIssuePlanPath: input.SourceAcceptedIssuePlanPath,
            Repository: input.Repository,
            Actions: actions);
    }

    private static GithubActionPlanItem CreateSeedAction(GithubReconciliationInput input, IssuePlanItem item, int index)
    {
        var mapping = input.ExistingMappings.FirstOrDefault(m => string.Equals(m.IssuePlanId, item.IssuePlanId, StringComparison.Ordinal));
        if (mapping is not null)
        {
            return CreateAction(
                index,
                "LINK",
                item,
                mapping.IssueNumber,
                item.Title,
                item.Description,
                item.AcceptanceCriteria,
                item.Labels,
                $"Existing mapping found for {item.IssuePlanId}; link to issue #{mapping.IssueNumber}.",
                requiresHumanReview: true,
                metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["mappingIssueUrl"] = mapping.IssueUrl,
                    ["createdFromPlanId"] = mapping.CreatedFromPlanId
                });
        }

        var candidate = GithubIssueCandidateMatcher.Best(item, input.ExistingIssues);
        if (candidate is not null && GithubIssueCandidateMatcher.IsStrong(candidate))
        {
            var operationFromCandidate = candidate.Issue.State.Equals("closed", StringComparison.OrdinalIgnoreCase)
                ? "REOPEN"
                : "LINK";
            return CreateAction(
                index,
                operationFromCandidate,
                item,
                candidate.Issue.IssueNumber,
                item.Title,
                item.Description,
                item.AcceptanceCriteria,
                item.Labels,
                $"Possible existing GitHub issue match #{candidate.Issue.IssueNumber} (score {candidate.Score}); link/reopen instead of creating duplicate unless human review rejects the match.",
                requiresHumanReview: true,
                metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["sourceIssuePlanOperation"] = item.Operation,
                    ["candidateIssueNumber"] = candidate.Issue.IssueNumber,
                    ["candidateIssueState"] = candidate.Issue.State,
                    ["candidateIssueTitle"] = candidate.Issue.Title,
                    ["candidateScore"] = candidate.Score,
                    ["candidateOverlappingTerms"] = candidate.OverlappingTerms,
                    ["candidateSourceRequirementIds"] = candidate.SourceRequirementIds
                });
        }

        var operation = item.Operation.ToUpperInvariant() switch
        {
            "CREATE" => "CREATE",
            "NO_CHANGE" => "NO_CHANGE",
            "NEEDS_REVIEW" => "NEEDS_REVIEW",
            "LINK" => "NEEDS_REVIEW",
            _ => "NEEDS_REVIEW"
        };
        var reason = operation switch
        {
            "CREATE" => "Accepted IssuePlanItem is implementation-ready and has no known mapping.",
            "NO_CHANGE" => "Accepted IssuePlanItem explicitly states that no GitHub action is needed.",
            _ => "Accepted IssuePlanItem requires review before GitHub action planning."
        };
        return CreateAction(
            index,
            operation,
            item,
            targetIssueNumber: null,
            title: item.Title,
            body: item.Description,
            acceptanceCriteria: item.AcceptanceCriteria,
            labels: item.Labels,
            reason: reason,
            requiresHumanReview: operation is "NEEDS_REVIEW",
            metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["sourceIssuePlanOperation"] = item.Operation
            });
    }

    private static GithubActionPlanItem CreateAction(
        int index,
        string operation,
        IssuePlanItem item,
        int? targetIssueNumber,
        string title,
        string body,
        IReadOnlyList<string> acceptanceCriteria,
        IReadOnlyList<string> labels,
        string reason,
        bool requiresHumanReview,
        IReadOnlyDictionary<string, object?> metadata)
    {
        var enrichedMetadata = new Dictionary<string, object?>(metadata, StringComparer.Ordinal)
        {
            ["dependencies"] = item.Dependencies,
            ["knownContext"] = item.KnownContext,
            ["implementationHints"] = item.ImplementationHints,
            ["openQuestions"] = item.OpenQuestions,
            ["relatedClarificationIds"] = item.RelatedClarificationIds,
            ["readiness"] = item.Readiness ?? ""
        };

        return new(
            ActionId: $"GHACT-{index:D3}",
            Operation: operation,
            IssuePlanId: item.IssuePlanId,
            SourceRequirementIds: item.SourceRequirementIds,
            TargetIssueNumber: targetIssueNumber,
            Title: title,
            Body: body,
            AcceptanceCriteria: acceptanceCriteria,
            Labels: labels,
            Reason: reason,
            RequiresHumanReview: requiresHumanReview,
            Metadata: enrichedMetadata);
    }
}
