using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed class L4ConsolidationTools(ProjectStateDocument state, string sourceProjectStatePath, RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly Dictionary<string, ProjectStateItem> _itemsById = state.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
    private readonly List<ConsolidationPlan> _savedPlans = [];
    private int _checkRounds;

    public bool Saved => _savedPlans.Count > 0;
    public ConsolidationPlan? SavedPlan => _savedPlans.LastOrDefault();
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListProjectItems, "list_project_items",
            "Listet ProjectState-Items mit Filter nach itemType/status. Nutze dies zur ersten Orientierung."),
        AIFunctionFactory.Create(SearchProjectItems, "search_project_items",
            "Sucht ProjectState-Items per Stichwort. Liefert IDs, Typ, Status, Ursprung und Textauszug."),
        AIFunctionFactory.Create(GetProjectItem, "get_project_item",
            "Liest ein ProjectState-Item inklusive Relationen und Provenienz."),
        AIFunctionFactory.Create(GetRelatedItems, "get_related_items",
            "Findet ProjectState-Items mit direkter Relation zum angegebenen Item."),
        AIFunctionFactory.Create(GetSeedPlan, "get_seed_plan",
            "Liefert den deterministischen Identity-Seed-Plan als sicheren Ausgangspunkt."),
        AIFunctionFactory.Create(CheckConsolidationPlan, "check_consolidation_plan",
            "Prueft einen ConsolidationPlan-Operationsentwurf deterministisch. Nutze dies vor dem Speichern."),
        AIFunctionFactory.Create(SaveConsolidationPlan, "save_consolidation_plan",
            "Speichert deinen finalen ConsolidationPlan. Rufe dies genau einmal am Ende auf.")
    ];

    private string ListProjectItems(string itemType = "requirement", string status = "", int limit = 80)
    {
        var lim = Math.Clamp(limit, 1, 200);
        var rows = state.Items
            .Where(i => string.IsNullOrWhiteSpace(itemType) || string.Equals(i.ItemType, itemType, StringComparison.OrdinalIgnoreCase))
            .Where(i => string.IsNullOrWhiteSpace(status) || string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Take(lim)
            .Select(i => new
            {
                i.ItemId,
                i.ItemType,
                i.Status,
                i.Origin,
                i.SourceCandidateId,
                i.SourceDecisionId,
                sourceClaimIds = i.SourceClaimIds,
                text = Truncate(i.Text, 500)
            })
            .ToArray();
        run.AppendEvent(new { type = "L4_TOOL_LIST_PROJECT_ITEMS", runId = run.RunId, itemType, status, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchProjectItems(string query, string itemType = "", int limit = 30)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return "[]";
        var rows = state.Items
            .Where(i => string.IsNullOrWhiteSpace(itemType) || string.Equals(i.ItemType, itemType, StringComparison.OrdinalIgnoreCase))
            .Select(i => new { item = i, score = terms.Count(t => i.Text.Contains(t, StringComparison.OrdinalIgnoreCase) || i.ItemId.Contains(t, StringComparison.OrdinalIgnoreCase)) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.item.ItemId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => new
            {
                x.item.ItemId,
                x.item.ItemType,
                x.item.Status,
                x.item.Origin,
                text = Truncate(x.item.Text, 600)
            })
            .ToArray();
        run.AppendEvent(new { type = "L4_TOOL_SEARCH_PROJECT_ITEMS", runId = run.RunId, query, itemType, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetProjectItem(string itemId)
    {
        var id = (itemId ?? string.Empty).Trim();
        if (!_itemsById.TryGetValue(id, out var item)) return $"UNKNOWN_ITEM: {id}";
        var relations = state.Relations
            .Where(r => string.Equals(r.FromId, id, StringComparison.Ordinal) || string.Equals(r.ToId, id, StringComparison.Ordinal))
            .ToArray();
        var provenance = state.Provenance.FirstOrDefault(p => string.Equals(p.ItemId, id, StringComparison.Ordinal));
        run.AppendEvent(new { type = "L4_TOOL_GET_PROJECT_ITEM", runId = run.RunId, itemId = id, relations = relations.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { item, relations, provenance }, Json);
    }

    private string GetRelatedItems(string itemId, int limit = 30)
    {
        var id = (itemId ?? string.Empty).Trim();
        if (!_itemsById.ContainsKey(id)) return $"UNKNOWN_ITEM: {id}";
        var relatedIds = state.Relations
            .Where(r => string.Equals(r.FromId, id, StringComparison.Ordinal) || string.Equals(r.ToId, id, StringComparison.Ordinal))
            .Select(r => string.Equals(r.FromId, id, StringComparison.Ordinal) ? r.ToId : r.FromId)
            .Distinct(StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .ToArray();
        var rows = relatedIds
            .Where(_itemsById.ContainsKey)
            .Select(rid =>
            {
                var item = _itemsById[rid];
                return new
                {
                    item.ItemId,
                    item.ItemType,
                    item.Status,
                    item.Origin,
                    text = Truncate(item.Text, 500)
                };
            })
            .ToArray();
        run.AppendEvent(new { type = "L4_TOOL_GET_RELATED_ITEMS", runId = run.RunId, itemId = id, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetSeedPlan()
    {
        var plan = L4ConsolidationPlanFactory.CreateIdentityPlan(state, sourceProjectStatePath);
        run.AppendEvent(new { type = "L4_TOOL_GET_SEED_PLAN", runId = run.RunId, operations = plan.Operations.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(plan, Json);
    }

    private string CheckConsolidationPlan(ConsolidationOperation[] operations)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var plan = BuildPlan(operations ?? []);
        var report = L4ConsolidationGate.Check(state, plan);
        run.AppendEvent(new
        {
            type = "L4_TOOL_CHECK_CONSOLIDATION_PLAN",
            runId = run.RunId,
            round,
            operations = plan.Operations.Count,
            pass = report.Pass,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SaveConsolidationPlan(ConsolidationOperation[] operations)
    {
        if (Saved) return "ALREADY_SAVED: save_consolidation_plan darf nur einmal aufgerufen werden.";
        var plan = BuildPlan(operations ?? []);
        _savedPlans.Add(plan);
        run.AppendEvent(new { type = "L4_TOOL_SAVE_CONSOLIDATION_PLAN", runId = run.RunId, operations = plan.Operations.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, operations = plan.Operations.Count }, Json);
    }

    private ConsolidationPlan BuildPlan(IReadOnlyList<ConsolidationOperation> operations)
        => new(
            SchemaVersion: ConsolidationPlan.CurrentSchemaVersion,
            ProjectId: state.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            SourceProjectStatePath: sourceProjectStatePath,
            Operations: operations.ToList());

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}
