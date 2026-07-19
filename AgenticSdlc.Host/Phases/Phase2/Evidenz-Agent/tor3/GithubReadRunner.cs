using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-read <list|get|search|labels> <snapshot.json> [args]
//   list   <snapshot> [--state open|closed|all] [--limit N]
//   get    <snapshot> <issueNumber>
//   search <snapshot> <query...> [--state ...] [--limit N]
//   labels <snapshot>
//
// T3.2-Beleg: uebt die GitHub-Read-Queries deterministisch gegen einen Snapshot aus (kein LLM, kein Token) —
// dasselbe Verhalten, das der Forward-Maker (T3.3) ueber GithubReadTools bekommt. Nur lesend.
public static class GithubReadRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3) { Usage(); return 2; }
        var sub = args[1].ToLowerInvariant();
        var snapshotPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);

        IReadOnlyList<L4.GithubIssueSnapshot> issues;
        try
        {
            issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-read] {ex.Message}");
            return 2;
        }

        var (state, limit, rest) = ParseOptions(args, startIndex: 3);

        switch (sub)
        {
            case "list":
            {
                var rows = GithubIssueQueries.List(issues, state, limit ?? 100);
                Console.WriteLine($"[github-read] list state={state ?? "all"} -> {rows.Count} von {issues.Count}");
                Console.WriteLine(JsonSerializer.Serialize(rows.Select(i => new { i.IssueNumber, i.State, i.Title, i.Labels }), Json));
                return 0;
            }
            case "get":
            {
                if (rest.Count == 0 || !int.TryParse(rest[0], out var number)) { Console.Error.WriteLine("[github-read] get braucht <issueNumber>."); return 2; }
                var issue = GithubIssueQueries.Get(issues, number);
                if (issue is null) { Console.WriteLine($"[github-read] UNKNOWN_ISSUE: #{number}"); return 0; }
                Console.WriteLine(JsonSerializer.Serialize(issue, Json));
                return 0;
            }
            case "search":
            {
                var query = string.Join(' ', rest);
                if (string.IsNullOrWhiteSpace(query)) { Console.Error.WriteLine("[github-read] search braucht <query>."); return 2; }
                var hits = GithubIssueQueries.Search(issues, query, state, limit ?? 20);
                Console.WriteLine($"[github-read] search \"{query}\" state={state ?? "all"} -> {hits.Count} Treffer");
                Console.WriteLine(JsonSerializer.Serialize(hits, Json));
                return 0;
            }
            case "labels":
            {
                var labels = GithubIssueQueries.Labels(issues);
                Console.WriteLine($"[github-read] labels -> {labels.Count}");
                Console.WriteLine(JsonSerializer.Serialize(labels, Json));
                return 0;
            }
            default:
                Usage();
                return 2;
        }
    }

    private static (string? State, int? Limit, IReadOnlyList<string> Positional) ParseOptions(string[] args, int startIndex)
    {
        string? state = null;
        int? limit = null;
        var rest = new List<string>();
        for (var i = startIndex; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--state", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) state = args[++i];
            else if (string.Equals(args[i], "--limit", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var l)) { limit = l; i++; }
            else rest.Add(args[i]);
        }
        return (state, limit, rest);
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-read list   <snapshot.json> [--state open|closed|all] [--limit N]");
        Console.Error.WriteLine("       github-read get    <snapshot.json> <issueNumber>");
        Console.Error.WriteLine("       github-read search <snapshot.json> <query...> [--state ...] [--limit N]");
        Console.Error.WriteLine("       github-read labels <snapshot.json>");
    }
}
