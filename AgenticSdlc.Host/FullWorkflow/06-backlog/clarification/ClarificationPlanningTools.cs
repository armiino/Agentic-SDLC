using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed class ClarificationPlanningTools(
    ClarificationPlanningInput input,
    string sourceClarificationPlanningInputPath,
    RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, ClarificationPlanningInputItem> _itemsById = input.Items.ToDictionary(i => i.ClarificationId, StringComparer.Ordinal);
    private readonly List<ClarificationPlanDocument> _savedPlans = [];
    private int _checkRounds;

    public bool Saved => _savedPlans.Count > 0;
    public ClarificationPlanDocument? SavedPlan => _savedPlans.LastOrDefault();
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListClarificationItems, "list_clarification_items",
            "Listet offene Klaerungs- und Breakdown-Items aus dem ClarificationPlanningInput."),
        AIFunctionFactory.Create(SearchClarificationItems, "search_clarification_items",
            "Sucht in offenen Klaerungsitems nach Stichworten."),
        AIFunctionFactory.Create(GetClarificationItem, "get_clarification_item",
            "Liest ein offenes Klaerungsitem inklusive Quell-Requirement und Rationale."),
        AIFunctionFactory.Create(GetSeedClarificationPlan, "get_seed_clarification_plan",
            "Liefert einen deterministischen Seed-Plan mit einem Klaerungs-/Breakdown-Issue pro Item."),
        AIFunctionFactory.Create(CheckClarificationPlan, "check_clarification_plan",
            "Prueft ClarificationPlanItems deterministisch. Nutze dies vor dem Speichern."),
        AIFunctionFactory.Create(SaveClarificationPlan, "save_clarification_plan",
            "Speichert deinen finalen ClarificationPlan. Rufe dies genau einmal am Ende auf.")
    ];

    private string ListClarificationItems(int limit = 80)
    {
        var rows = input.Items
            .OrderBy(i => i.ClarificationId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(i => new
            {
                i.ClarificationId,
                i.SourceRequirementId,
                i.ClarificationType,
                i.Priority,
                i.Readiness,
                i.Title,
                i.Question,
                i.RequirementTitle,
                rationale = Truncate(i.Rationale ?? "", 500)
            })
            .ToArray();
        run.AppendEvent(new { type = "CLARIFICATION_TOOL_LIST", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchClarificationItems(string query, int limit = 30)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return "[]";
        var rows = input.Items
            .Select(i => new
            {
                item = i,
                score = terms.Count(t =>
                    i.ClarificationId.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.SourceRequirementId.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.Title.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.Question.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.RequirementTitle.Contains(t, StringComparison.OrdinalIgnoreCase)
                    || i.ClarificationType.Contains(t, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.item.ClarificationId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => new
            {
                x.item.ClarificationId,
                x.item.SourceRequirementId,
                x.item.ClarificationType,
                x.item.Priority,
                x.item.Title,
                x.item.Question,
                x.item.RequirementTitle
            })
            .ToArray();
        run.AppendEvent(new { type = "CLARIFICATION_TOOL_SEARCH", runId = run.RunId, query, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetClarificationItem(string clarificationId)
    {
        var id = (clarificationId ?? string.Empty).Trim();
        if (!_itemsById.TryGetValue(id, out var item)) return $"UNKNOWN_CLARIFICATION_ITEM: {id}";
        run.AppendEvent(new { type = "CLARIFICATION_TOOL_GET", runId = run.RunId, clarificationId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(item, Json);
    }

    private string GetSeedClarificationPlan()
    {
        var plan = ClarificationPlanFactory.CreateOneClarificationIssuePerItem(input, sourceClarificationPlanningInputPath);
        run.AppendEvent(new { type = "CLARIFICATION_TOOL_GET_SEED", runId = run.RunId, items = plan.Items.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(plan, Json);
    }

    private string CheckClarificationPlan(ClarificationPlanItem[] items)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var plan = BuildPlan(items ?? []);
        var report = ClarificationPlanGate.Check(input, plan);
        run.AppendEvent(new
        {
            type = "CLARIFICATION_TOOL_CHECK",
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

    private string SaveClarificationPlan(ClarificationPlanItem[] items)
    {
        if (Saved) return "ALREADY_SAVED: save_clarification_plan darf nur einmal aufgerufen werden.";
        var plan = BuildPlan(items ?? []);
        _savedPlans.Add(plan);
        run.AppendEvent(new { type = "CLARIFICATION_TOOL_SAVE", runId = run.RunId, items = plan.Items.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, items = plan.Items.Count }, Json);
    }

    private ClarificationPlanDocument BuildPlan(IReadOnlyList<ClarificationPlanItem> items)
        => new(
            SchemaVersion: ClarificationPlanDocument.CurrentSchemaVersion,
            PlanId: $"clarification-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            Mode: "plan",
            CreatedUtc: DateTime.UtcNow,
            SourceClarificationPlanningInputPath: sourceClarificationPlanningInputPath,
            Items: items.ToList());

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}
