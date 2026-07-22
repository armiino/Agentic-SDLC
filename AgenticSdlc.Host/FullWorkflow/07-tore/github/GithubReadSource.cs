using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Backlog;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.2 — laedt den reproduzierbaren Issue-Snapshot fuer die Read-Tools. Akzeptiert das Format von
// `github-snapshot issues` (reines GithubIssueSnapshot[]-Array) ODER einen Wrapper { "issues": [...] }.
// Der LIVE-Pull ist bewusst NICHT hier: er existiert bereits als `github-snapshot issues [--repo|--file]`
// (gh-CLI oder REST) und schreibt genau diese Datei -> Trennung Pull (einmalig) vs. Agent-Read (reproduzierbar).
public static class GithubReadSource
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<IReadOnlyList<GithubIssueSnapshot>> LoadAsync(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"GITHUB_SNAPSHOT_NOT_FOUND: {path} - erst 'github-snapshot issues ...' fahren.", path);

        var text = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(text);

        var arrayElement = doc.RootElement.ValueKind switch
        {
            JsonValueKind.Array => doc.RootElement,
            JsonValueKind.Object when doc.RootElement.TryGetProperty("issues", out var issues) && issues.ValueKind == JsonValueKind.Array => issues,
            _ => throw new InvalidOperationException($"GITHUB_SNAPSHOT_UNREADABLE: {path} - erwartet Array oder {{\"issues\":[...]}}.")
        };

        return JsonSerializer.Deserialize<List<GithubIssueSnapshot>>(arrayElement.GetRawText(), Json) ?? [];
    }
}
