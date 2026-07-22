using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed class GithubReconciliationTools(GithubReconciliationInput input, RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly Dictionary<string, IssuePlanItem> _itemsById = input.AcceptedIssuePlan.Items.ToDictionary(i => i.IssuePlanId, StringComparer.Ordinal);
    private readonly List<GithubActionPlanDocument> _savedPlans = [];
    private int _checkRounds;

    public bool Saved => _savedPlans.Count > 0;
    public GithubActionPlanDocument? SavedPlan => _savedPlans.LastOrDefault();
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListAcceptedIssuePlanItems, "list_accepted_issue_plan_items",
            "Listet akzeptierte IssuePlanItems, die fuer GitHub Reconciliation betrachtet werden muessen."),
        AIFunctionFactory.Create(GetIssuePlanItem, "get_issue_plan_item",
            "Liest ein akzeptiertes IssuePlanItem inklusive Quellen, Kriterien und Labels."),
        AIFunctionFactory.Create(ListExistingMappings, "list_existing_mappings",
            "Listet bekannte lokale Mappings zwischen IssuePlanItems/Requirements und GitHub Issues."),
        AIFunctionFactory.Create(SearchExistingIssues, "search_existing_issues",
            "Sucht in vorhandenen read-only GitHub-Issue-Snapshots. Schreibt nichts nach GitHub."),
        AIFunctionFactory.Create(FindCandidateIssueMatches, "find_candidate_issue_matches",
            "Findet deterministische Kandidaten zwischen einem AcceptedIssuePlanItem und vorhandenen GitHub Issues."),
        AIFunctionFactory.Create(GetSeedGithubActionPlan, "get_seed_github_action_plan",
            "Liefert einen deterministischen Startplan aus AcceptedIssuePlan + vorhandenen Mappings."),
        AIFunctionFactory.Create(CheckGithubActionPlan, "check_github_action_plan",
            "Prueft GitHubActionPlanItems deterministisch. Nutze dies vor dem Speichern."),
        AIFunctionFactory.Create(SaveGithubActionPlan, "save_github_action_plan",
            "Speichert deinen finalen GitHubActionPlan. Rufe dies genau einmal am Ende auf.")
    ];

    private string ListAcceptedIssuePlanItems(int limit = 80)
    {
        var rows = input.AcceptedIssuePlan.Items
            .OrderBy(i => i.IssuePlanId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(i => new
            {
                i.IssuePlanId,
                i.Operation,
                i.Title,
                sourceRequirementIds = i.SourceRequirementIds,
                labels = i.Labels,
                description = Truncate(i.Description, 500)
            })
            .ToArray();
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_LIST_PLAN_ITEMS", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetIssuePlanItem(string issuePlanId)
    {
        var id = (issuePlanId ?? string.Empty).Trim();
        if (!_itemsById.TryGetValue(id, out var item)) return $"UNKNOWN_ISSUE_PLAN_ITEM: {id}";
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_GET_PLAN_ITEM", runId = run.RunId, issuePlanId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(item, Json);
    }

    private string ListExistingMappings()
    {
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_LIST_MAPPINGS", runId = run.RunId, returned = input.ExistingMappings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(input.ExistingMappings, Json);
    }

    private string SearchExistingIssues(string query, int limit = 20)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rows = input.ExistingIssues
            .Select(i => new
            {
                issue = i,
                score = terms.Length == 0
                    ? 1
                    : terms.Count(t => i.Title.Contains(t, StringComparison.OrdinalIgnoreCase)
                                       || (i.Body?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false)
                                       || i.Labels.Any(l => l.Contains(t, StringComparison.OrdinalIgnoreCase)))
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.issue.IssueNumber)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => x.issue)
            .ToArray();
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_SEARCH_ISSUES", runId = run.RunId, query, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string FindCandidateIssueMatches(string issuePlanId, int limit = 5)
    {
        var id = (issuePlanId ?? string.Empty).Trim();
        if (!_itemsById.TryGetValue(id, out var item)) return $"UNKNOWN_ISSUE_PLAN_ITEM: {id}";
        var rows = GithubIssueCandidateMatcher.Find(item, input.ExistingIssues, limit)
            .Select(match => new
            {
                issueNumber = match.Issue.IssueNumber,
                match.Issue.State,
                match.Issue.Title,
                match.Issue.Url,
                score = match.Score,
                overlappingTerms = match.OverlappingTerms,
                sourceRequirementIds = match.SourceRequirementIds
            })
            .ToArray();
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_FIND_CANDIDATES", runId = run.RunId, issuePlanId = id, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetSeedGithubActionPlan()
    {
        var plan = GithubActionPlanFactory.CreateSeed(input);
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_GET_SEED", runId = run.RunId, actions = plan.Actions.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(plan, Json);
    }

    private string CheckGithubActionPlan(GithubActionPlanItem[] actions)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var plan = BuildPlan(actions ?? []);
        var report = GithubActionPlanGate.Check(input, plan);
        run.AppendEvent(new
        {
            type = "GITHUB_RECON_TOOL_CHECK",
            runId = run.RunId,
            round,
            actions = plan.Actions.Count,
            pass = report.Pass,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SaveGithubActionPlan(GithubActionPlanItem[] actions)
    {
        if (Saved) return "ALREADY_SAVED: save_github_action_plan darf nur einmal aufgerufen werden.";
        var plan = BuildPlan(actions ?? []);
        _savedPlans.Add(plan);
        run.AppendEvent(new { type = "GITHUB_RECON_TOOL_SAVE", runId = run.RunId, actions = plan.Actions.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, actions = plan.Actions.Count }, Json);
    }

    private GithubActionPlanDocument BuildPlan(IReadOnlyList<GithubActionPlanItem> actions)
        => new(
            SchemaVersion: GithubActionPlanDocument.CurrentSchemaVersion,
            PlanId: $"github-action-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAcceptedIssuePlanPath: input.SourceAcceptedIssuePlanPath,
            Repository: input.Repository,
            Actions: actions.ToList());

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}
