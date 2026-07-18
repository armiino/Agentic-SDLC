using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class ClarificationPlanningApply
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static ClarificationPlanningApplyResult Apply(
        ClarificationPlanningInput input,
        ClarificationPlanDocument sourcePlan,
        ClarificationPlanningHumanDecisionsFile decisions,
        bool allowUnreviewed)
    {
        var decisionsById = decisions.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.ClarificationPlanId))
            .GroupBy(d => d.ClarificationPlanId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var acceptedItems = new List<ClarificationPlanItem>();
        var accepted = 0;
        var edited = 0;
        var rejected = 0;
        var revisionRequested = 0;

        foreach (var item in sourcePlan.Items)
        {
            if (!decisionsById.TryGetValue(item.ClarificationPlanId, out var decision))
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
                    acceptedItems.Add(ParseEditedItem(decision, item));
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
            .Select(i => i.ClarificationPlanId)
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
        var gate = ClarificationPlanGate.Check(input, acceptedPlan);
        var report = new ClarificationPlanningApplyReport(
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
        return new ClarificationPlanningApplyResult(acceptedPlan, report);
    }

    private static ClarificationPlanItem ParseEditedItem(ClarificationPlanningHumanDecision decision, ClarificationPlanItem original)
    {
        if (string.IsNullOrWhiteSpace(decision.EditedClarificationPlanItemJson))
            throw new InvalidOperationException($"Edit-Decision ohne editedClarificationPlanItemJson: {decision.ClarificationPlanId}");

        var edited = JsonSerializer.Deserialize<ClarificationPlanItem>(decision.EditedClarificationPlanItemJson, Json)
                     ?? throw new InvalidOperationException($"Edited ClarificationPlanItem konnte nicht gelesen werden: {decision.ClarificationPlanId}");
        return edited with
        {
            ClarificationPlanId = original.ClarificationPlanId,
            SourceClarificationIds = original.SourceClarificationIds,
            SourceRequirementIds = original.SourceRequirementIds,
            Metadata = edited.Metadata.Count == 0 ? original.Metadata : edited.Metadata
        };
    }
}

public sealed record ClarificationPlanningApplyResult(ClarificationPlanDocument AcceptedPlan, ClarificationPlanningApplyReport Report);

public sealed record ClarificationPlanningApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("acceptedPlanId")] string AcceptedPlanId,
    [property: JsonPropertyName("inputItems")] int InputItems,
    [property: JsonPropertyName("sourceItems")] int SourceItems,
    [property: JsonPropertyName("acceptedItems")] int AcceptedItems,
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("edited")] int Edited,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("revisionRequested")] int RevisionRequested,
    [property: JsonPropertyName("missingDecisions")] IReadOnlyList<string> MissingDecisions,
    [property: JsonPropertyName("gate")] ClarificationPlanGateReport Gate,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc);
