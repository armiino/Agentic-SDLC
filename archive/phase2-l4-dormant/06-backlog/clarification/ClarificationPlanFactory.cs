namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class ClarificationPlanFactory
{
    public static ClarificationPlanDocument CreateOneClarificationIssuePerItem(
        ClarificationPlanningInput input,
        string sourceClarificationPlanningInputPath)
    {
        var items = input.Items
            .OrderBy(i => i.ClarificationId, StringComparer.Ordinal)
            .Select((item, index) => new ClarificationPlanItem(
                ClarificationPlanId: $"CPLAN-{index + 1:D3}",
                Operation: item.ClarificationType.Equals("breakdown", StringComparison.OrdinalIgnoreCase)
                    ? "CREATE_BREAKDOWN_ISSUE"
                    : "CREATE_CLARIFICATION_ISSUE",
                Title: MakeTitle(item),
                Question: Normalize(item.Question),
                Description: BuildDescription(item),
                SourceClarificationIds: [item.ClarificationId],
                SourceRequirementIds: [item.SourceRequirementId],
                AcceptanceCriteria: BuildAcceptanceCriteria(item),
                Labels: BuildLabels(item),
                Priority: Normalize(item.Priority).Length == 0 ? "medium" : Normalize(item.Priority),
                RequiresHumanReview: true,
                Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["seed"] = "one_clarification_issue_per_item",
                    ["clarificationType"] = item.ClarificationType,
                    ["readiness"] = item.Readiness,
                    ["requirementTitle"] = item.RequirementTitle
                }))
            .ToList();

        return new ClarificationPlanDocument(
            SchemaVersion: ClarificationPlanDocument.CurrentSchemaVersion,
            PlanId: $"clarification-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            Mode: "plan",
            CreatedUtc: DateTime.UtcNow,
            SourceClarificationPlanningInputPath: sourceClarificationPlanningInputPath,
            Items: items);
    }

    private static string MakeTitle(ClarificationPlanningInputItem item)
    {
        var prefix = item.ClarificationType.Equals("breakdown", StringComparison.OrdinalIgnoreCase)
            ? "Breakdown klaeren: "
            : "Klaerung: ";
        var title = Normalize(item.RequirementTitle);
        var value = prefix + (title.Length == 0 ? item.SourceRequirementId : title);
        if (value.Length <= 110) return value;
        var cut = value.LastIndexOf(' ', Math.Min(110, value.Length - 1));
        return value[..(cut > 50 ? cut : 110)].TrimEnd('.', ',', ';') + "...";
    }

    private static string BuildDescription(ClarificationPlanningInputItem item)
    {
        var readiness = string.IsNullOrWhiteSpace(item.Readiness) ? "unbekannt" : item.Readiness;
        var rationale = string.IsNullOrWhiteSpace(item.Rationale) ? "keine" : item.Rationale;
        return $"""
               Offenes Requirement: {item.SourceRequirementId}
               Clarification Item: {item.ClarificationId}
               Typ: {item.ClarificationType}
               Readiness: {readiness}

               Frage:
               {Normalize(item.Question)}

               Kontext:
               {Normalize(item.RequirementTitle)}

               Rationale:
               {Normalize(rationale)}
               """;
    }

    private static IReadOnlyList<string> BuildAcceptanceCriteria(ClarificationPlanningInputItem item)
    {
        if (item.ClarificationType.Equals("breakdown", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                $"Fuer {item.SourceRequirementId} liegt eine konkrete Zerlegung in umsetzbare Requirements oder Arbeitspakete vor.",
                "Jeder neue oder aktualisierte Punkt ist rueckverfolgbar mit dem offenen Requirement verknuepft.",
                "Offene Entscheidungen aus der Zerlegung sind separat markiert."
            ];
        }

        return
        [
            $"Die offene Frage zu {item.SourceRequirementId} ist entschieden oder als bewusst offen dokumentiert.",
            "Die Entscheidung enthaelt Auswirkung, Begruendung und naechste operative Konsequenz.",
            "Der Project State kann danach aktualisiert oder die weitere Klaerung begruendet offen gehalten werden."
        ];
    }

    private static IReadOnlyList<string> BuildLabels(ClarificationPlanningInputItem item)
    {
        var labels = new List<string> { "requirements", "clarification" };
        var type = Normalize(item.ClarificationType).ToLowerInvariant();
        if (type.Length > 0) labels.Add(type);
        var readiness = Normalize(item.Readiness ?? "").ToLowerInvariant();
        if (readiness.Length > 0) labels.Add(readiness.Replace('_', '-'));
        return labels.Distinct(StringComparer.Ordinal).ToList();
    }

    private static string Normalize(string value)
        => string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
