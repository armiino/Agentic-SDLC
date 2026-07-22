using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed class IssuePlanningTools(IssuePlanningInput input, string sourceIssuePlanningInputPath, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, IssuePlanningInputItem> _itemsById = input.Items.ToDictionary(i => i.RequirementId, StringComparer.Ordinal);
    private readonly List<IssuePlanDocument> _savedPlans = [];
    private int _checkRounds;

    public bool Saved => _savedPlans.Count > 0;
    public IssuePlanDocument? SavedPlan => _savedPlans.LastOrDefault();
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListIssuePlanningItems, "list_issue_planning_items",
            "Listet Requirements, die durch Readiness fuer plan-only Issue Planning freigegeben sind."),
        AIFunctionFactory.Create(SearchIssuePlanningItems, "search_issue_planning_items",
            "Sucht in freigegebenen Requirements nach Stichworten."),
        AIFunctionFactory.Create(GetIssuePlanningItem, "get_issue_planning_item",
            "Liest ein freigegebenes Requirement inklusive Text, Quellen und Provenienz."),
        AIFunctionFactory.Create(GetSeedIssuePlan, "get_seed_issue_plan",
            "Liefert einen deterministischen Seed-Plan mit einem CREATE-Issue pro Requirement."),
        AIFunctionFactory.Create(CheckIssuePlan, "check_issue_plan",
            "Prueft IssuePlanItems deterministisch. Nutze dies vor dem Speichern."),
        AIFunctionFactory.Create(SaveIssuePlan, "save_issue_plan",
            "Speichert deinen finalen IssuePlan. Rufe dies genau einmal am Ende auf.")
    ];

    private string ListIssuePlanningItems(int limit = 80)
    {
        var rows = input.Items
            .OrderBy(i => i.RequirementId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(i => new
            {
                i.RequirementId,
                i.Title,
                sourceItemIds = i.SourceItemIds,
                labelsHint = BuildLabelHints(i),
                text = Truncate(i.Text, 500)
            })
            .ToArray();
        run.AppendEvent(new { type = "ISSUE_PLANNING_TOOL_LIST", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchIssuePlanningItems(string query, int limit = 30)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return "[]";
        var rows = input.Items
            .Select(i => new
            {
                item = i,
                score = terms.Count(t =>
                    i.RequirementId.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.Title.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.Text.Contains(t, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.item.RequirementId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => new
            {
                x.item.RequirementId,
                x.item.Title,
                sourceItemIds = x.item.SourceItemIds,
                text = Truncate(x.item.Text, 700)
            })
            .ToArray();
        run.AppendEvent(new { type = "ISSUE_PLANNING_TOOL_SEARCH", runId = run.RunId, query, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetIssuePlanningItem(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_itemsById.TryGetValue(id, out var item)) return $"UNKNOWN_OR_NOT_READY_REQUIREMENT: {id}";
        run.AppendEvent(new { type = "ISSUE_PLANNING_TOOL_GET", runId = run.RunId, requirementId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(item, Json);
    }

    private string GetSeedIssuePlan()
    {
        var plan = IssuePlanFactory.CreateOneIssuePerRequirement(input, sourceIssuePlanningInputPath);
        run.AppendEvent(new { type = "ISSUE_PLANNING_TOOL_GET_SEED", runId = run.RunId, items = plan.Items.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(plan, Json);
    }

    private string CheckIssuePlan(IssuePlanItem[] items)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var plan = BuildPlan(items ?? []);
        var report = IssuePlanGate.Check(input, plan);
        run.AppendEvent(new
        {
            type = "ISSUE_PLANNING_TOOL_CHECK",
            runId = run.RunId,
            round,
            items = plan.Items.Count,
            pass = report.Pass,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SaveIssuePlan(IssuePlanItem[] items)
    {
        if (Saved) return "ALREADY_SAVED: save_issue_plan darf nur einmal aufgerufen werden.";
        var plan = BuildPlan(items ?? []);
        _savedPlans.Add(plan);
        run.AppendEvent(new { type = "ISSUE_PLANNING_TOOL_SAVE", runId = run.RunId, items = plan.Items.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, items = plan.Items.Count }, Json);
    }

    private IssuePlanDocument BuildPlan(IReadOnlyList<IssuePlanItem> items)
        => new(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourceIssuePlanningInputPath,
            Items: items.ToList());

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }

    private static IReadOnlyList<string> BuildLabelHints(IssuePlanningInputItem item)
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
}
