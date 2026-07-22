using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// HumanReview der PBI-Operationen (apply/skip). Runner um die generische HumanReview-UI, wie ingest-review.
public static class PbiUpdateReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: pbi-update-review <pbi-update-run|dir> [--interactive|--file] [--no-browser]"); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[pbi-update-review] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "pbi-change-plan.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[pbi-update-review] pbi-change-plan.json fehlt."); return 2; }

        var plan = await LoadAsync<PbiStateChangePlanDocument>(planPath).ConfigureAwait(false);
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pbi-update-review] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var interactive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase) || !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

        var session = PbiUpdateReviewAdapter.BuildSession(runId, plan, core);
        if (session.Items.Count == 0) { Console.WriteLine("[pbi-update-review] keine Operationen."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<PbiUpdateDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        PbiUpdateReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = PbiUpdateReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[pbi-update-review] mode=file {session.Items.Count} Operationen -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: PbiUpdateReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(PbiUpdateReviewAdapter.ResolveContext(key, plan, core)),
            apply: s => PbiUpdateReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[pbi-update-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");
        return 0;
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "pbi-change-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "pbi-change-plan.json"))) return Path.Combine(full, "plan");
        var root = Path.Combine(repoRoot, "runs", "pbi-update");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "pbi-change-plan.json"))) return plan;
        }
        return null;
    }

    private static Task<T> LoadAsync<T>(string path) => JsonFiles.LoadAsync<T>(path); // R3b: geteilt
}
