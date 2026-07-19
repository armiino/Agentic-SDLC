using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.Host.Run;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-reverse [--issues <snapshot.json>]
// Reverse-Maker (deterministisch): vergleicht GitHub-Snapshot mit dem Core (Mappings) und schlaegt gepruefte
// StateChanges vor (PBI_DONE?, Mapping-Sync, Drift) -> Gate -> Review (github-reverse-review) -> Apply.
public static class GithubReverseRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        string? issuesArg = null;
        for (var i = 1; i < args.Length; i++)
            if (string.Equals(args[i], "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) issuesArg = args[++i];

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-reverse] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var snapshotPath = ResolveSnapshotPath(repoRoot, issuesArg);
        if (snapshotPath is null) { Console.Error.WriteLine("[github-reverse] kein Issue-Snapshot (--issues <file> oder erst github-snapshot issues)."); return 2; }
        IReadOnlyList<GithubIssueSnapshot> issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);

        var ops = GithubReverseSeed.Seed(core, issues);
        var run = new RunContext(RunId.New(), "github-reverse");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        var plan = new GithubReversePlanDocument(
            GithubReversePlanDocument.CurrentSchemaVersion, $"github-reverse-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            DateTime.UtcNow, Path.GetRelativePath(repoRoot, snapshotPath), ops);
        var gate = GithubReverseGate.Check(core, plan);

        await File.WriteAllTextAsync(Path.Combine(outDir, "github-reverse-plan.json"), JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-reverse-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-reverse] ops={ops.Count} gate={(gate.Pass ? "pass" : "fail")} errors={gate.Errors.Count}");
        Console.WriteLine($"[github-reverse] byKind: {string.Join(", ", ops.GroupBy(o => o.Kind, StringComparer.Ordinal).Select(g => $"{g.Key}={g.Count()}"))}");
        foreach (var e in gate.Errors) Console.WriteLine($"[github-reverse]   ERROR {e.Code} {e.PbiId}: {e.Message}");
        Console.WriteLine($"[github-reverse] -> {Path.GetRelativePath(repoRoot, outDir)} (danach: github-reverse-review)");
        return gate.Pass ? 0 : 1;
    }

    private static string? ResolveSnapshotPath(string repoRoot, string? issuesArg)
    {
        if (!string.IsNullOrWhiteSpace(issuesArg))
            return Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg);
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, "github-issues-snapshot.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
