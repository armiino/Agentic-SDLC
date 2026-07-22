using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// HumanReview der Decision-Auflösungen (apply/skip). Runner um die generische HumanReview-UI, wie ingest-review.
public static class DecisionReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: decision-review <decision-run|dir> [--interactive|--file] [--no-browser]"); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[decision-review] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "decision-plan.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[decision-review] decision-plan.json fehlt."); return 2; }

        var plan = await LoadAsync<DecisionResolutionPlanDocument>(planPath).ConfigureAwait(false);
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[decision-review] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var interactive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase) || !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

        var session = DecisionResolutionReviewAdapter.BuildSession(runId, plan, core);
        if (session.Items.Count == 0) { Console.WriteLine("[decision-review] keine Auflösungen."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<DecisionResolutionDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        DecisionResolutionReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = DecisionResolutionReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[decision-review] mode=file {session.Items.Count} Auflösungen -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(DecisionResolutionReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = DecisionResolutionReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(DecisionResolutionReviewAdapter.ResolveContext(key, plan, core)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        Console.WriteLine($"[decision-review] {result.Outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");
        return 0;
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "decision-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "decision-plan.json"))) return Path.Combine(full, "plan");
        var root = Path.Combine(repoRoot, "runs", "decision");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "decision-plan.json"))) return plan;
        }
        return null;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
