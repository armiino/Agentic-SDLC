using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4BaselineBuilder
{
    private static readonly string[] OpenDecisionMarkers =
    [
        "zu entscheiden", "ist zu entscheiden", "zu klären", "soll geklärt", "offen", "festzulegen",
        "verbindlich festzulegen", "rechtsgrundlage", "mvp"
    ];

    public static CanonicalRequirementsBaseline Build(ProjectStateDocument state, string sourceProjectStatePath)
    {
        var requirementItems = state.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .Where(i => IsActiveStatus(i.Status))
            .OrderBy(i => SortKey(i.ItemId), StringComparer.Ordinal)
            .ThenBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();

        var requirements = new List<CanonicalRequirement>(requirementItems.Count);
        var traceLinks = new List<CanonicalTraceLink>();
        var openDecisions = new List<CanonicalOpenDecision>();

        var seq = 0;
        foreach (var item in requirementItems)
        {
            var requirementId = $"CAN-REQ-{++seq:D3}";
            var metadata = new Dictionary<string, string>(item.Metadata, StringComparer.Ordinal)
            {
                ["projectStateItemId"] = item.ItemId,
                ["projectStateStatus"] = item.Status,
                ["projectStateOrigin"] = item.Origin
            };
            var requirement = new CanonicalRequirement(
                RequirementId: requirementId,
                Title: MakeTitle(item.Text),
                Text: item.Text,
                Status: IsOpenDecisionLike(item) ? "open_decision" : "active",
                SourceItemIds: [item.ItemId],
                OriginSummary: item.Origin,
                Version: 1,
                Metadata: metadata);
            requirements.Add(requirement);

            traceLinks.Add(new CanonicalTraceLink(requirementId, "project_item", item.ItemId, "canonicalizes", new Dictionary<string, string>
            {
                ["origin"] = item.Origin,
                ["status"] = item.Status
            }));
            foreach (var claimId in item.SourceClaimIds)
                traceLinks.Add(new CanonicalTraceLink(requirementId, "ledger_claim", claimId, "evidenced_by", new Dictionary<string, string>()));
            foreach (var sourceItemId in item.SourceArtifactItemIds)
                traceLinks.Add(new CanonicalTraceLink(requirementId, "project_item", sourceItemId, "derived_from", new Dictionary<string, string>()));
            if (!string.IsNullOrWhiteSpace(item.SourceDecisionId))
                traceLinks.Add(new CanonicalTraceLink(requirementId, "human_decision", item.SourceDecisionId!, "accepted_by", new Dictionary<string, string>()));
            if (!string.IsNullOrWhiteSpace(item.SourceCandidateId))
                traceLinks.Add(new CanonicalTraceLink(requirementId, "l3_candidate", item.SourceCandidateId!, "promoted_from", new Dictionary<string, string>()));

            if (requirement.Status == "open_decision")
            {
                openDecisions.Add(new CanonicalOpenDecision(
                    DecisionId: $"OPEN-{openDecisions.Count + 1:D3}",
                    Text: item.Text,
                    SourceRequirementId: requirementId,
                    SourceItemIds: [item.ItemId],
                    Reason: "Heuristisch als offene Entscheidung markiert; L4-Agent/Human Review kann dies spaeter konsolidieren."));
            }
        }

        return new CanonicalRequirementsBaseline(
            BaselineId: $"baseline-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: state.ProjectId,
            SchemaVersion: CanonicalRequirementsBaseline.CurrentSchemaVersion,
            CreatedUtc: DateTime.UtcNow,
            SourceProjectStatePath: sourceProjectStatePath,
            Requirements: requirements,
            OpenDecisions: openDecisions,
            TraceLinks: traceLinks);
    }

    private static bool IsActiveStatus(string status)
        => status.Equals("baseline", StringComparison.OrdinalIgnoreCase)
           || status.Equals("accepted", StringComparison.OrdinalIgnoreCase);

    private static bool IsOpenDecisionLike(ProjectStateItem item)
    {
        var text = item.Text.ToLowerInvariant();
        return OpenDecisionMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));
    }

    private static string MakeTitle(string text)
    {
        var normalized = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (normalized.Length <= 88) return normalized;
        var cut = normalized.LastIndexOf(' ', Math.Min(88, normalized.Length - 1));
        return normalized[..(cut > 40 ? cut : 88)].TrimEnd('.', ';', ',') + "...";
    }

    private static string SortKey(string itemId)
    {
        var prefix = new string(itemId.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(itemId.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return $"{prefix}{(int.TryParse(digits, out var n) ? n : 0):D6}:{itemId}";
    }
}
