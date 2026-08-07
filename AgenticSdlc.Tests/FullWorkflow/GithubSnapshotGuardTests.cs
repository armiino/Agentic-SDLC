using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B1/R-16 (07.08.): der Snapshot-Repo-Wächter — fremde/ungestempelte „latest"-Snapshots werden LAUT
// abgelehnt (Giftfall 20260723_142742: Alt-Repo-Snapshot hätte 22 Falsch-Mappings geschrieben).
public sealed class GithubSnapshotGuardTests
{
    private static string Snapshot(string? repository)
    {
        var dir = Directory.CreateTempSubdirectory("snap-").FullName;
        var path = Path.Combine(dir, "github-issues-snapshot.json");
        File.WriteAllText(path, "[]");
        if (repository is not null)
            File.WriteAllText(Path.Combine(dir, "github-issues-snapshot-summary.json"),
                $"{{\"schemaVersion\":1,\"repository\":\"{repository}\"}}");
        return path;
    }

    [Fact]
    public void Passender_Stempel_passiert_fremder_und_fehlender_werden_LAUT_abgelehnt()
    {
        GithubSnapshotGuard.Verify(Snapshot("armiino/Agentic-GitHub-refactor"), "armiino/Agentic-GitHub-refactor");   // ok
        GithubSnapshotGuard.Verify(Snapshot("egal/egal"), null);                                                       // kein Ziel = nichts zu prüfen
        GithubSnapshotGuard.Verify(null, "armiino/x");                                                                 // kein Snapshot = nichts zu prüfen

        var fremd = Assert.Throws<GithubSnapshotGuard.SnapshotRepoMismatchException>(
            () => GithubSnapshotGuard.Verify(Snapshot("armiino/Agentic-GitHub"), "armiino/Agentic-GitHub-refactor"));
        Assert.Contains("SNAPSHOT_REPO_MISMATCH", fremd.Message);

        var ohne = Assert.Throws<GithubSnapshotGuard.SnapshotRepoMismatchException>(
            () => GithubSnapshotGuard.Verify(Snapshot(null), "armiino/Agentic-GitHub-refactor"));
        Assert.Contains("SNAPSHOT_UNSTAMPED", ohne.Message);
    }
}
