using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// HumanReview des Forward-Plans (apply/skip). Runner um die generische HumanReview-UI, wie pbi-update-review.
public static class GithubForwardReviewRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

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

        var session = GithubForwardReviewAdapter.BuildSession(runId, plan);
        if (session.Items.Count == 0) { Console.WriteLine("[github-forward-review] keine Operationen."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<GithubForwardDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        GithubForwardReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = GithubForwardReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[github-forward-review] mode=file {session.Items.Count} Operationen -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(GithubForwardReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = GithubForwardReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(GithubForwardReviewAdapter.ResolveContext(key, plan)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        Console.WriteLine($"[github-forward-review] {result.Outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");
        return 0;
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

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
