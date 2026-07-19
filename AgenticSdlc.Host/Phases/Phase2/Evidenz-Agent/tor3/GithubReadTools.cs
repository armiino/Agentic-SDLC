using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.2 — GitHub-READ als Agent-Tools (Variante B: schmale Client-/Snapshot-Tools statt MCP-Server; E-B).
// READ-ONLY: hier gibt es KEINEN Write-Pfad (create/update/close bleiben im gated Apply, T3.4). Die Tools lesen
// aus einem reproduzierbaren GithubIssueSnapshot[] und delegieren die Logik an GithubIssueQueries (ein Verhalten,
// deterministisch testbar). Jeder Aufruf wird als Run-Event belegt (Nachweis: was hat der Maker tatsaechlich gelesen).
//
// get_comments ist bewusst NICHT dabei: das Snapshot-Modell fuehrt keine Kommentare. Kommentare brauchen den
// Live-Client oder den GitHub-MCP-Server (Variante A) -> in den Notes als bewusst zurueckgestellt dokumentiert.
internal sealed class GithubReadTools(IReadOnlyList<GithubIssueSnapshot> issues, string? repository, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListIssues, "list_issues",
            "Listet bekannte GitHub-Issues aus dem Snapshot (state=open|closed|all). read-only, schreibt nichts."),
        AIFunctionFactory.Create(GetIssue, "get_issue",
            "Liest ein einzelnes GitHub-Issue (Titel, Body, State, Labels) per Nummer aus dem Snapshot."),
        AIFunctionFactory.Create(SearchIssues, "search_issues",
            "Sucht Issues per Stichwort-Overlap (Titel/Body/Labels). Vor einem CREATE nutzen, um Duplikate zu finden."),
        AIFunctionFactory.Create(GetLabels, "get_labels",
            "Listet alle im Snapshot vorkommenden Labels mit Haeufigkeit.")
    ];

    public string RepositoryLabel => repository ?? "<snapshot>";

    private string ListIssues(string state = "all", int limit = 100)
    {
        var rows = GithubIssueQueries.List(issues, state, limit);
        run.AppendEvent(new { type = "GITHUB_READ_TOOL_LIST", runId = run.RunId, repository, state, returned = rows.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows.Select(Compact), Json);
    }

    private string GetIssue(int issueNumber)
    {
        var issue = GithubIssueQueries.Get(issues, issueNumber);
        run.AppendEvent(new { type = "GITHUB_READ_TOOL_GET", runId = run.RunId, repository, issueNumber, found = issue is not null, timestampUtc = DateTime.UtcNow });
        return issue is null ? $"UNKNOWN_ISSUE: #{issueNumber}" : JsonSerializer.Serialize(issue, Json);
    }

    private string SearchIssues(string query, string state = "all", int limit = 20)
    {
        var hits = GithubIssueQueries.Search(issues, query, state, limit);
        run.AppendEvent(new { type = "GITHUB_READ_TOOL_SEARCH", runId = run.RunId, repository, query, state, returned = hits.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(hits, Json);
    }

    private string GetLabels()
    {
        var labels = GithubIssueQueries.Labels(issues);
        run.AppendEvent(new { type = "GITHUB_READ_TOOL_LABELS", runId = run.RunId, repository, returned = labels.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(labels, Json);
    }

    // Kompakte Listen-Sicht (ohne Body) — der Maker holt Details gezielt per get_issue.
    private static object Compact(GithubIssueSnapshot i)
        => new { i.IssueNumber, i.State, i.Title, i.Url, i.Labels };
}
