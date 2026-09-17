using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// B1/R-16 (07.08.2026) — der Snapshot-Repo-Wächter: „latest snapshot" wird NUR akzeptiert, wenn sein
/// Repo-Stempel zum konfigurierten Ziel-Repo passt. Hintergrund (Run `20260723_142742`): ein Alt-Snapshot
/// des FALSCHEN Repos hätte 22 Falsch-Mappings als Core-Relationen geschrieben — nur der Dry-Run fing es.
/// Der Stempel liegt seit jeher in der Schwester-Datei `github-issues-snapshot-summary.json` (repository).
/// Regeln: Stempel fehlt → LAUT ablehnen („neu ziehen") · Stempel ≠ Ziel → LAUT ablehnen (beide Namen) ·
/// KEIN Ziel-Repo konfiguriert → nichts zu prüfen (Betrieb ohne Repo ist ohnehin leer/dry, R-16-Vektor
/// ist der MISMATCH). Geteilte Naht für ALLE Snapshot-Konsumenten (Forward CLI+HITL, Reverse, Pipeline).
/// </summary>
public static class GithubSnapshotGuard
{
    public sealed class SnapshotRepoMismatchException(string message) : InvalidOperationException(message);

    /// <summary>Wirft LAUT bei fremdem/ungestempeltem Snapshot; no-op ohne erwartetes Repo oder ohne Pfad.</summary>
    public static void Verify(string? snapshotPath, string? expectedRepo)
    {
        if (string.IsNullOrWhiteSpace(snapshotPath) || string.IsNullOrWhiteSpace(expectedRepo)) return;

        var summaryPath = Path.Combine(Path.GetDirectoryName(snapshotPath)!, "github-issues-snapshot-summary.json");
        string? stamped = null;
        if (File.Exists(summaryPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(summaryPath));
                if (doc.RootElement.TryGetProperty("repository", out var r)) stamped = r.GetString();
            }
            catch (JsonException) { /* unlesbar = wie ungestempelt behandeln */ }
        }

        if (string.IsNullOrWhiteSpace(stamped))
            throw new SnapshotRepoMismatchException(
                $"SNAPSHOT_UNSTAMPED (R-16): '{snapshotPath}' trägt keinen Repo-Stempel (summary fehlt/leer) — "
                + $"Snapshot NEU ziehen: github-snapshot issues --repo {expectedRepo}");

        if (!string.Equals(stamped, expectedRepo, StringComparison.OrdinalIgnoreCase))
            throw new SnapshotRepoMismatchException(
                $"SNAPSHOT_REPO_MISMATCH (R-16): Snapshot ist von '{stamped}', Ziel ist '{expectedRepo}' — "
                + $"fremder Snapshot wird ABGELEHNT (stille Falsch-Mappings!). Neu ziehen: github-snapshot issues --repo {expectedRepo}");
    }
}
