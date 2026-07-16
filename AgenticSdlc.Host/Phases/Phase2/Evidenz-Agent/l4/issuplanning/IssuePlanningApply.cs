using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanningApply
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static IssuePlanningApplyResult Apply(
        IssuePlanningInput input,
        IssuePlanDocument sourcePlan,
        IssuePlanningHumanDecisionsFile decisions,
        bool allowUnreviewed)
    {
        var sourceById = sourcePlan.Items.ToDictionary(i => i.IssuePlanId, StringComparer.Ordinal);
        var decisionsById = decisions.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.IssuePlanId))
            .GroupBy(d => d.IssuePlanId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var acceptedItems = new List<IssuePlanItem>();
        var accepted = 0;
        var edited = 0;
        var rejected = 0;
        var revisionRequested = 0;

        foreach (var item in sourcePlan.Items)
        {
            if (!decisionsById.TryGetValue(item.IssuePlanId, out var decision))
            {
                if (allowUnreviewed)
                {
                    acceptedItems.Add(item);
                    accepted++;
                }
                continue;
            }

            switch ((decision.Decision ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "accept":
                    acceptedItems.Add(item);
                    accepted++;
                    break;
                case "edit":
                    var editedItem = ParseEditedItem(decision, item);
                    acceptedItems.Add(editedItem);
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

        var missing = sourcePlan.Items
            .Select(i => i.IssuePlanId)
            .Where(id => !decisionsById.ContainsKey(id))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (allowUnreviewed) missing = [];

        var acceptedPlan = sourcePlan with
        {
            PlanId = $"{sourcePlan.PlanId}-accepted",
            CreatedUtc = DateTime.UtcNow,
            Items = acceptedItems
        };
        var gate = IssuePlanGate.Check(input, acceptedPlan);
        var report = new IssuePlanningApplyReport(
            RunId: decisions.RunId,
            SourcePlanId: sourcePlan.PlanId,
            AcceptedPlanId: acceptedPlan.PlanId,
            InputItems: input.Items.Count,
            SourceItems: sourcePlan.Items.Count,
            AcceptedItems: acceptedItems.Count,
            Accepted: accepted,
            Edited: edited,
            Rejected: rejected,
            RevisionRequested: revisionRequested,
            MissingDecisions: missing,
            Gate: gate,
            TimestampUtc: DateTime.UtcNow);
        return new IssuePlanningApplyResult(acceptedPlan, report);
    }

    private static IssuePlanItem ParseEditedItem(IssuePlanningHumanDecision decision, IssuePlanItem original)
    {
        if (string.IsNullOrWhiteSpace(decision.EditedIssuePlanItemJson))
            throw new InvalidOperationException($"Edit-Decision ohne editedIssuePlanItemJson: {decision.IssuePlanId}");

        var edited = JsonSerializer.Deserialize<IssuePlanItem>(decision.EditedIssuePlanItemJson, Json)
                     ?? throw new InvalidOperationException($"Edited IssuePlanItem konnte nicht gelesen werden: {decision.IssuePlanId}");
        return edited with
        {
            IssuePlanId = original.IssuePlanId,
            SourceRequirementIds = original.SourceRequirementIds,
            Metadata = edited.Metadata.Count == 0 ? original.Metadata : edited.Metadata
        };
    }
}

public sealed record IssuePlanningApplyResult(IssuePlanDocument AcceptedPlan, IssuePlanningApplyReport Report);
