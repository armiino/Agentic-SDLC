using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// CLI: github-reverse [--issues <snapshot.json>]
// Reverse-Maker als MAF-Workflow (Seed -> Gate -> Finalize, deterministische Executor-Knoten, siehe
// GithubReverseWorkflow). Vergleicht GitHub-Snapshot mit dem Core und schlaegt gepruefte StateChanges vor.
public static class GithubReverseRunner
{
    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        string? issuesArg = null;
        for (var i = 1; i < args.Length; i++)
            if (string.Equals(args[i], "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) issuesArg = args[++i];
        return await RunReverseAsync(repoRoot, issuesArg).ConfigureAwait(false);
    }

    /// <summary>K13-2: typisierte Naht (Steward + künftige Konsumenten) — Zustands-Reverse ohne CLI-Args.</summary>
    public static async Task<int> RunReverseAsync(string repoRoot, string? issuesArg = null)
    {
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-reverse] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var snapshotPath = ResolveSnapshotPath(repoRoot, issuesArg);
        if (snapshotPath is null) { Console.Error.WriteLine("[github-reverse] kein Issue-Snapshot (--issues <file> oder erst github-snapshot issues)."); return 2; }
        IReadOnlyList<GithubIssueSnapshot> issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);

        var run = new RunContext(RunId.New(), "github-reverse");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        var workflow = GithubReverseWorkflow.Build(
            new GithubReverseSeedExecutor(run), new GithubReverseGateExecutor(run), new GithubReverseFinalizeExecutor(run));
        var ctx = new GithubReverseWfContext(core, issues, Path.GetRelativePath(repoRoot, snapshotPath), outDir);

        try
        {
            await InProcessExecution.Default.RunAsync(workflow, ctx, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) { Console.Error.WriteLine($"[github-reverse] Ausfuehrung fehlgeschlagen: {ex.Message}"); return 4; }

        var summaryPath = Path.Combine(outDir, "github-reverse-summary.json");
        if (!File.Exists(summaryPath)) { Console.Error.WriteLine("[github-reverse] Ausfuehrung unvollstaendig: summary fehlt."); return 4; }
        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
        var s = summary.RootElement;
        var gatePass = s.GetProperty("gatePass").GetBoolean();
        var byKind = string.Join(", ", s.GetProperty("byKind").EnumerateObject().Select(p => $"{p.Name}={p.Value.GetInt32()}"));
        Console.WriteLine($"[github-reverse] ops={s.GetProperty("ops").GetInt32()} gate={(gatePass ? "pass" : "fail")} errors={s.GetProperty("gateErrors").GetInt32()}");
        if (byKind.Length > 0) Console.WriteLine($"[github-reverse] byKind: {byKind}");
        Console.WriteLine($"[github-reverse] -> {Path.GetRelativePath(repoRoot, outDir)} (danach: github-reverse-review)");
        return gatePass ? 0 : 1;
    }

    private static string? ResolveSnapshotPath(string repoRoot, string? issuesArg)
    {
        if (!string.IsNullOrWhiteSpace(issuesArg))
            return Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg);
        return GithubSnapshotLocator.FindLatest(repoRoot);
    }
}
