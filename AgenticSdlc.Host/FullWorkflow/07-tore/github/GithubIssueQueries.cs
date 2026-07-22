using AgenticSdlc.Host.FullWorkflow.Backlog;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.2 — reine, deterministische Read-Queries ueber einen GithubIssueSnapshot[] (kein LLM, kein Netz, kein Token).
// Quelle ist ein reproduzierbarer Issue-Snapshot (github-snapshot issues bzw. eine gecachte Datei). Sowohl die
// Agent-Tools (GithubReadTools) als auch die CLI (github-read) nutzen genau diese Logik -> ein Verhalten, testbar.
public static class GithubIssueQueries
{
    // Nach Zustand filtern: open|closed|all (null/leer/"all" => alle). GitHub kennt genau open/closed.
    public static IReadOnlyList<GithubIssueSnapshot> List(IReadOnlyList<GithubIssueSnapshot> issues, string? state, int limit)
        => issues
            .Where(i => MatchesState(i, state))
            .OrderBy(i => i.IssueNumber)
            .Take(Math.Clamp(limit, 1, 500))
            .ToList();

    public static GithubIssueSnapshot? Get(IReadOnlyList<GithubIssueSnapshot> issues, int number)
        => issues.FirstOrDefault(i => i.IssueNumber == number);

    // Anti-Duplikat-Suche (Rev 2): Term-Overlap ueber Titel/Body/Labels. Nur Treffer mit score>0, nach Relevanz.
    // Der Forward-Maker (T3.3) nutzt genau das, um vor einem CREATE einen bestehenden Kandidaten zu finden.
    public static IReadOnlyList<IssueSearchHit> Search(IReadOnlyList<GithubIssueSnapshot> issues, string query, string? state, int limit)
    {
        var terms = Tokenize(query);
        return issues
            .Where(i => MatchesState(i, state))
            .Select(i =>
            {
                var overlapping = terms.Where(t => Contains(i, t)).ToList();
                return new IssueSearchHit(i.IssueNumber, i.State, i.Title, i.Url, i.Labels, overlapping.Count, overlapping);
            })
            .Where(h => terms.Count == 0 || h.Score > 0)
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.IssueNumber)
            .Take(Math.Clamp(limit, 1, 100))
            .ToList();
    }

    public static IReadOnlyList<LabelCount> Labels(IReadOnlyList<GithubIssueSnapshot> issues)
        => issues
            .SelectMany(i => i.Labels)
            .GroupBy(l => l, StringComparer.OrdinalIgnoreCase)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(l => l.Count)
            .ThenBy(l => l.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool MatchesState(GithubIssueSnapshot i, string? state)
        => string.IsNullOrWhiteSpace(state)
           || string.Equals(state, "all", StringComparison.OrdinalIgnoreCase)
           || string.Equals(i.State, state, StringComparison.OrdinalIgnoreCase);

    private static bool Contains(GithubIssueSnapshot i, string term)
        => i.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
           || (i.Body?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
           || i.Labels.Any(l => l.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> Tokenize(string? query)
        => (query ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

public sealed record IssueSearchHit(
    int IssueNumber,
    string State,
    string Title,
    string? Url,
    IReadOnlyList<string> Labels,
    int Score,
    IReadOnlyList<string> OverlappingTerms);

public sealed record LabelCount(string Label, int Count);
