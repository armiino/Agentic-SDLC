using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class GithubReconciliationApply
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static GithubReconciliationApplyResult Apply(
        GithubReconciliationInput input,
        GithubActionPlanDocument sourcePlan,
        GithubActionHumanDecisionsFile decisions,
        bool allowUnreviewed)
    {
        var decisionsById = decisions.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.ActionId))
            .GroupBy(d => d.ActionId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var acceptedActions = new List<GithubActionPlanItem>();
        var accepted = 0;
        var edited = 0;
        var rejected = 0;
        var revisionRequested = 0;

        foreach (var action in sourcePlan.Actions)
        {
            if (!decisionsById.TryGetValue(action.ActionId, out var decision))
            {
                if (allowUnreviewed)
                {
                    acceptedActions.Add(action);
                    accepted++;
                }
                continue;
            }

            switch ((decision.Decision ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "accept":
                    acceptedActions.Add(action);
                    accepted++;
                    break;
                case "edit":
                    acceptedActions.Add(ParseEditedAction(decision, action));
                    edited++;
                    break;
                case "reject":
                    rejected++;
                    break;
                case "revise":
                    revisionRequested++;
                    break;
            }
        }

        var missing = sourcePlan.Actions
            .Select(a => a.ActionId)
            .Where(id => !decisionsById.ContainsKey(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (allowUnreviewed) missing = [];

        var acceptedPlan = sourcePlan with
        {
            PlanId = $"{sourcePlan.PlanId}-accepted",
            CreatedUtc = DateTime.UtcNow,
            Actions = acceptedActions
        };
        var gate = GithubActionPlanGate.Check(input, acceptedPlan);
        var report = new GithubReconciliationApplyReport(
            RunId: decisions.RunId,
            SourcePlanId: sourcePlan.PlanId,
            AcceptedPlanId: acceptedPlan.PlanId,
            AcceptedIssuePlanItems: input.AcceptedIssuePlan.Items.Count,
            SourceActions: sourcePlan.Actions.Count,
            AcceptedActions: acceptedActions.Count,
            Accepted: accepted,
            Edited: edited,
            Rejected: rejected,
            RevisionRequested: revisionRequested,
            MissingDecisions: missing,
            Gate: gate,
            TimestampUtc: DateTime.UtcNow);
        return new GithubReconciliationApplyResult(acceptedPlan, report);
    }

    private static GithubActionPlanItem ParseEditedAction(GithubActionHumanDecision decision, GithubActionPlanItem original)
    {
        if (string.IsNullOrWhiteSpace(decision.EditedActionJson))
            throw new InvalidOperationException($"Edit-Decision ohne editedActionJson: {decision.ActionId}");

        var edited = JsonSerializer.Deserialize<GithubActionPlanItem>(decision.EditedActionJson, Json)
                     ?? throw new InvalidOperationException($"Edited GithubActionPlanItem konnte nicht gelesen werden: {decision.ActionId}");
        return edited with
        {
            ActionId = original.ActionId,
            IssuePlanId = original.IssuePlanId,
            SourceRequirementIds = original.SourceRequirementIds,
            Metadata = edited.Metadata.Count == 0 ? original.Metadata : edited.Metadata
        };
    }
}

public sealed record GithubReconciliationApplyResult(GithubActionPlanDocument AcceptedPlan, GithubReconciliationApplyReport Report);
