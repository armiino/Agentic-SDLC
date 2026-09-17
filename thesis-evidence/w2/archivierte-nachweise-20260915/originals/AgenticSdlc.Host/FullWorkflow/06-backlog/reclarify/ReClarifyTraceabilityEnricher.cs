using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Setzt PBI.traceability DETERMINISTISCH aus der echten Provenienz, statt dem (unpraezisen) Agentenfeld
// zu vertrauen. Kette: PBI.requirementIds (CAN-REQ) -> canonical baseline.sourceItemIds (ProjectState-Items
// REQ-*/L3-REQ-*) -> ProjectState-Item.sourceClaimIds (Ledger-Claims). Damit ist das PBI selbst-enthaltend
// bis zur L1/Claim-Ebene rueckverfolgbar (von Claims weiter -> Transkript ueber den Ledger).
public static class ReClarifyTraceabilityEnricher
{
    public static ProductBacklogDocument Enrich(
        ProductBacklogDocument doc,
        CanonicalRequirementsBaseline baseline,
        IReadOnlyList<ProjectStateItem> stateItems)
    {
        var canById = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var stateById = stateItems.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var items = doc.Items.Select(p => p with { Traceability = Build(p, canById, stateById) }).ToList();
        return doc with { Items = items };
    }

    private static PbiTraceability Build(
        ProductBacklogItem p,
        IReadOnlyDictionary<string, CanonicalRequirement> canById,
        IReadOnlyDictionary<string, ProjectStateItem> stateById)
    {
        var canonical = p.RequirementIds
            .Where(canById.ContainsKey)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var sourceItems = canonical
            .SelectMany(id => canById[id].SourceItemIds)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var l3 = sourceItems.Where(IsL3).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var l1 = sourceItems.Where(x => !IsL3(x)).OrderBy(x => x, StringComparer.Ordinal).ToList();

        var claims = sourceItems
            .Where(stateById.ContainsKey)
            .SelectMany(id => stateById[id].SourceClaimIds)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        return new PbiTraceability(
            CanonicalRequirementIds: canonical,
            L3: l3,
            L1Req: l1,
            Claims: claims);
    }

    private static bool IsL3(string itemId)
        => itemId.StartsWith("L3-", StringComparison.OrdinalIgnoreCase)
        || itemId.StartsWith("CAND", StringComparison.OrdinalIgnoreCase);
}
