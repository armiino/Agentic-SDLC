using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Reverse-Plans (apply/skip; bei PBI_DONE = Verifikation, E4). Runner um die generische UI.
public static class GithubReverseReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: github-reverse-review <github-reverse-run|dir> [--interactive|--file] [--no-browser]"); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[github-reverse-review] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-reverse-plan.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-reverse-review] github-reverse-plan.json fehlt."); return 2; }

        var plan = await LoadAsync<GithubReversePlanDocument>(planPath).ConfigureAwait(false);
        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var interactive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase) || !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

        var session = GithubReverseReviewAdapter.BuildSession(runId, plan);
        if (session.Items.Count == 0) { Console.WriteLine("[github-reverse-review] keine Vorschlaege."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<GithubReverseDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        GithubReverseReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = GithubReverseReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[github-reverse-review] mode=file {session.Items.Count} Vorschlaege -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: GithubReverseReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(GithubReverseReviewAdapter.ResolveContext(key, plan)),
            apply: s => GithubReverseReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[github-reverse-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");
        return 0;
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "github-reverse-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "github-reverse-plan.json"))) return Path.Combine(full, "plan");
        var root = Path.Combine(repoRoot, "runs", "github-reverse");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "github-reverse-plan.json"))) return plan;
        }
        return null;
    }

    private static Task<T> LoadAsync<T>(string path) => JsonFiles.LoadAsync<T>(path); // R3b: geteilt
}
