using AgenticSdlc.Host.FullWorkflow;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// C3 (⚖ K7, 07.08.2026) — GitHub-LESE-Fragen beantwortet der fragende Agent SELBST aus dem neuesten
/// Issue-SNAPSHOT (kein LLM, kein Live-API-Call, kein zweiter Agent im Datenpfad). R-16-Bewusstsein ist
/// eingebaut: jede Antwort trägt Repo-Stempel + Snapshot-Zeitpunkt, und ein alter/ungestempelter Snapshot
/// wird LAUT benannt statt still konsumiert.
/// </summary>
public sealed class GithubSnapshotQueryTools(string repoRoot)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    private const int ResultCap = 20;
    private static readonly TimeSpan FreshnessLimit = TimeSpan.FromHours(24);

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(SearchIssuesAsync, "search_github_issues",
            "Sucht im NEUESTEN GitHub-Issue-Snapshot (Momentaufnahme, kein Live-Stand!). query = genau EIN woertlicher Suchbegriff (Substring in Titel/Body, case-insensitiv, Umlaut-tolerant: schriftgroesse trifft Schriftgroesse UND Schriftgröße). KEINE Operatoren (OR/AND/Quotes) — fuer Alternativen das Tool MEHRFACH aufrufen. state: open|closed. Antwort traegt IMMER repository + snapshotUtc — beides dem Autor nennen; staleNote weitergeben."),
    ];

    private async Task<string> SearchIssuesAsync(string? query = null, string? state = null)
    {
        var snapshotPath = GithubSnapshotLocator.FindLatest(repoRoot);
        if (snapshotPath is null)
            return JsonSerializer.Serialize(new
            {
                error = "SNAPSHOT_NOT_FOUND",
                hint = "Es wurde noch kein Issue-Snapshot gezogen: github-snapshot issues --repo <owner/name>",
            }, Json);

        var issues = JsonSerializer.Deserialize<List<GithubIssueSnapshot>>(
            await File.ReadAllTextAsync(snapshotPath).ConfigureAwait(false), Json) ?? [];
        var summary = await ReadSummaryAsync(snapshotPath).ConfigureAwait(false);

        var hits = issues
            .Where(i => state is null || string.Equals(i.State, state, StringComparison.OrdinalIgnoreCase))
            .Where(i => QueryText.Contains(i.Title, query) || (query is not null && QueryText.Contains(i.Body, query)))
            .OrderByDescending(i => i.UpdatedUtc)
            .ToList();

        TimeSpan? age = summary.TimestampUtc is { } ts ? DateTime.UtcNow - ts : null;
        return JsonSerializer.Serialize(new
        {
            repository = summary.Repository,                       // null = ungestempelt (R-16: laut benennen!)
            snapshotUtc = summary.TimestampUtc,
            staleNote = summary.Repository is null
                ? "Snapshot ist UNGESTEMPELT (Alt-Format) — vor Schreib-Aktionen frisch ziehen: github-snapshot issues --repo <owner/name>"
                : age is { } a && a > FreshnessLimit
                    ? $"Snapshot ist {(int)a.TotalHours}h alt — fuer aktuelle Aussagen/Schreib-Aktionen frisch ziehen: github-snapshot issues --repo {summary.Repository}"
                    : null,
            total = hits.Count,
            returned = Math.Min(hits.Count, ResultCap),
            dropped = Math.Max(0, hits.Count - ResultCap),          // kein stilles Kappen — query eingrenzen
            issues = hits.Take(ResultCap).Select(i => new
            {
                issueNumber = i.IssueNumber,
                title = i.Title,
                state = i.State,
                labels = i.Labels,
                updatedUtc = i.UpdatedUtc,
                url = i.Url,
            }),
        }, Json);
    }

    private static async Task<(string? Repository, DateTime? TimestampUtc)> ReadSummaryAsync(string snapshotPath)
    {
        var summaryPath = Path.Combine(Path.GetDirectoryName(snapshotPath)!, GithubSnapshotLocator.SummaryFileName);
        if (!File.Exists(summaryPath)) return (null, null);
        try
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
            var repository = doc.RootElement.TryGetProperty("repository", out var r) ? r.GetString() : null;
            DateTime? ts = doc.RootElement.TryGetProperty("timestampUtc", out var t) && t.TryGetDateTime(out var dt) ? dt : null;
            return (repository, ts);
        }
        catch (JsonException) { return (null, null); }   // unlesbar = wie ungestempelt (R-16-Linie des Guards)
    }
}
