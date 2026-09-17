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

    /// <summary>C2d: Datei des eigenen Kommentar-Snapshots (comments-Modus, c2d-plan §3-1).</summary>
    public const string CommentsFileName = "issue-comments.json";

    /// <summary>Pfad des neuesten Snapshots oder null, wenn noch keiner gezogen wurde.</summary>
    public static string? FindLatest(string repoRoot) => FindLatestFile(repoRoot, FileName);

    /// <summary>Pfad des neuesten Kommentar-Snapshots oder null (C2d — eigene Frische, eigenes Artefakt).</summary>
    public static string? FindLatestComments(string repoRoot) => FindLatestFile(repoRoot, CommentsFileName);

    private static string? FindLatestFile(string repoRoot, string fileName)
    {
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, fileName, SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
