using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Forward-Plans (apply/skip). Runner um die generische HumanReview-UI, wie pbi-update-review.
public static class GithubForwardReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: github-forward-review <github-forward-run|dir> [--interactive|--file] [--no-browser]"); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[github-forward-review] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-forward-plan.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-forward-review] github-forward-plan.json fehlt."); return 2; }

        var plan = await LoadAsync<GithubForwardPlanDocument>(planPath).ConfigureAwait(false);
        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var interactive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase) || !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

        // E0.2a: Snapshot des Laufs (Summary verlinkt ihn) fuer Vorher/Nachher bei UPDATE — best-effort:
        // ohne Snapshot laeuft das Review weiter, das Item warnt dann sichtbar.
        var issuesByNumber = await LoadIssuesAsync(repoRoot, planDir).ConfigureAwait(false);
        if (issuesByNumber is null)
            Console.WriteLine("[github-forward-review] HINWEIS: Issue-Snapshot nicht ladbar — UPDATE-Items ohne Vorher-Ansicht.");

        var session = GithubForwardReviewAdapter.BuildSession(runId, plan, issuesByNumber);
        if (session.Items.Count == 0) { Console.WriteLine("[github-forward-review] keine Operationen."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<GithubForwardDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        GithubForwardReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = GithubForwardReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[github-forward-review] mode=file {session.Items.Count} Operationen -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: GithubForwardReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(GithubForwardReviewAdapter.ResolveContext(key, plan, issuesByNumber)),
            apply: s => GithubForwardReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[github-forward-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");

        // R-43-Endform (09.08.): „Fertig" kettet den Apply — hier als VORSCHAU (safe-by-default): der echte
        // GitHub-Write bleibt bewusst ein eigener Akt (--execute = irreversible Außen-Grenze, R-16-Frische
        // zählt am Write-Zeitpunkt). --no-apply = Inspektions-Opt-out.
        if (args.Contains("--no-apply", StringComparer.OrdinalIgnoreCase)) return 0;
        if (outcome != AgenticSdlc.HumanReview.ReviewOutcome.Finished)
        { Console.WriteLine($"[github-forward-review] nicht abgeschlossen ({outcome}) — kein Auto-Apply."); return 0; }
        Console.WriteLine("[github-forward-review] R-43: Apply-VORSCHAU läuft automatisch an …");
        var rc = await GithubForwardApplyRunner.ApplyFromPlanDirAsync(ResolvePlanDir(repoRoot, args[1])!, repoRoot).ConfigureAwait(false);   // K13-1: typisierte Naht (Vorschau)
        Console.WriteLine($"[github-forward-review] echter GitHub-Write bewusst separat: github-forward-apply {args[1]} --execute");
        return rc;
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "github-forward-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "github-forward-plan.json"))) return Path.Combine(full, "plan");
        var root = Path.Combine(repoRoot, "runs", "github-forward");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "github-forward-plan.json"))) return plan;
        }
        return null;
    }

    internal static async Task<IReadOnlyDictionary<int, GithubIssueSnapshot>?> LoadIssuesAsync(string repoRoot, string planDir)
    {
        try
        {
            var summaryPath = Path.Combine(planDir, "github-forward-summary.json");
            if (!File.Exists(summaryPath)) return null;
            using var doc = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
            if (!doc.RootElement.TryGetProperty("snapshot", out var snap) || snap.ValueKind != System.Text.Json.JsonValueKind.String)
                return null;
            var snapshotPath = Path.Combine(repoRoot, snap.GetString()!);
            if (!File.Exists(snapshotPath)) return null;
            var issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);
            return issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        }
        catch
        {
            return null;
        }
    }

    private static Task<T> LoadAsync<T>(string path) => JsonFiles.LoadAsync<T>(path); // R3b: geteilt
}
