namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanFactory
{
    public static IssuePlanDocument CreateOneIssuePerRequirement(IssuePlanningInput input, string sourceIssuePlanningInputPath)
    {
        var items = input.Items
            .OrderBy(i => i.RequirementId, StringComparer.Ordinal)
            .Select((item, index) => new IssuePlanItem(
                IssuePlanId: $"IPLAN-{index + 1:D3}",
                Operation: "CREATE",
                Title: MakeIssueTitle(item.Title),
                Description: BuildDescription(item),
                SourceRequirementIds: [item.RequirementId],
                AcceptanceCriteria: BuildAcceptanceCriteria(item),
                Labels: BuildLabels(item),
                Dependencies: [],
                Rationale: "Deterministischer Seed: ein Issue pro ready Requirement.",
                RequiresHumanReview: true,
                Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["seed"] = "one_issue_per_requirement",
                    ["l4OperationId"] = item.Provenance?.L4OperationId ?? ""
                }))
            .ToList();

        return new IssuePlanDocument(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourceIssuePlanningInputPath,
            Items: items);
    }

    private static string MakeIssueTitle(string title)
    {
        var normalized = Normalize(title);
        if (normalized.Length <= 90) return normalized;
        var cut = normalized.LastIndexOf(' ', Math.Min(90, normalized.Length - 1));
        return normalized[..(cut > 40 ? cut : 90)].TrimEnd('.', ',', ';') + "...";
    }

    private static string BuildDescription(IssuePlanningInputItem item)
    {
        var provenance = item.Provenance;
        var sourceItems = item.SourceItemIds.Count == 0 ? "keine" : string.Join(", ", item.SourceItemIds);
        var ledgerClaims = provenance is null || provenance.LedgerClaimIds.Count == 0 ? "keine" : string.Join(", ", provenance.LedgerClaimIds);
        return $"""
               Requirement: {item.RequirementId}

               {item.Text}

               Source items: {sourceItems}
               Ledger claims: {ledgerClaims}
               """;
    }

    private static IReadOnlyList<string> BuildAcceptanceCriteria(IssuePlanningInputItem item)
        =>
        [
            $"Die Umsetzung erfuellt {item.RequirementId}: {Normalize(item.Title)}.",
            "Die Umsetzung ist anhand der verknuepften Requirement-Quelle rueckpruefbar."
        ];

    private static IReadOnlyList<string> BuildLabels(IssuePlanningInputItem item)
    {
        var text = $"{item.Title} {item.Text}".ToLowerInvariant();
        var labels = new List<string> { "requirements" };
        if (ContainsAny(text, ["login", "screen", "appbar", "seite", "profil", "button"])) labels.Add("frontend");
        if (ContainsAny(text, ["account", "admin", "rechte", "zugriff", "rollen"])) labels.Add("access-control");
        if (ContainsAny(text, ["android", "ios", "flutter", "dart"])) labels.Add("platform");
        return labels.Distinct(StringComparer.Ordinal).ToList();
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> terms)
        => terms.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string value)
        => string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
