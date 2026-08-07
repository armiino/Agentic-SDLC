namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// C3 (07.08.2026) — die EINE Quelle für „wo liegt der neueste Issue-Snapshot?" (Zwei-Bahnen-Regel:
/// vorher private Kopie im GithubReverseRunner, jetzt geteilt mit den Query-Tools). Snapshots sind
/// Momentaufnahmen unter runs/github-snapshot/&lt;runId&gt;/issues/ — der jüngste zählt.
/// </summary>
public static class GithubSnapshotLocator
{
    public const string FileName = "github-issues-snapshot.json";
    public const string SummaryFileName = "github-issues-snapshot-summary.json";

    /// <summary>Pfad des neuesten Snapshots oder null, wenn noch keiner gezogen wurde.</summary>
    public static string? FindLatest(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, FileName, SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
