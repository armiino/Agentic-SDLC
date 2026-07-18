using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// HumanReview der Product Backlog Items - Runner um die generische HumanReview-UI (wie IssuePlanning).
public static class ReClarifyBacklogReviewRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }

        var backlogDir = ResolveBacklogDir(repoRoot, args[1]);
        if (backlogDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-review] Backlog-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }
        var backlogPath = Path.Combine(backlogDir, "product-backlog.json");
        var gatePath = Path.Combine(backlogDir, "backlog-gate-report.json");
        if (!File.Exists(backlogPath) || !File.Exists(gatePath))
        {
            Console.Error.WriteLine("[l4-re-clarify-backlog-review] product-backlog.json oder backlog-gate-report.json fehlt.");
            return 2;
        }

        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);
        var gate = await LoadAsync<ReClarifyGateReport>(gatePath).ConfigureAwait(false);
        var baseline = await LoadBaselineBestEffortAsync(repoRoot, backlog.SourcePath).ConfigureAwait(false);

        var decisionsPath = Path.Combine(backlogDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(backlogDir);

        var session = ReClarifyBacklogReviewAdapter.BuildSession(runId, baseline, backlog, gate, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[l4-re-clarify-backlog-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingAsync(decisionsPath).ConfigureAwait(false);
        ReClarifyBacklogReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items) item.Resolved = ReClarifyBacklogReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[l4-re-clarify-backlog-review] mode=file scope={scope} {session.Items.Count} PBIs.");
            Console.WriteLine($"[l4-re-clarify-backlog-review] {Path.GetRelativePath(repoRoot, decisionsPath)} oder --interactive.");
            return 0;
        }

        async Task Persist() => await File.WriteAllTextAsync(
            decisionsPath, JsonSerializer.Serialize(ReClarifyBacklogReviewAdapter.Apply(runId, session, backlog), Json)).ConfigureAwait(false);

        var options = new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = ReClarifyBacklogReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(ReClarifyBacklogReviewAdapter.ResolveContext(key, baseline, backlog, gate)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        };

        Console.WriteLine($"[l4-re-clarify-backlog-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} PBIs");
        if (existing is not null)
            Console.WriteLine($"[l4-re-clarify-backlog-review] Re-Launch: {session.ResolvedCount()}/{session.Items.Count} resolved.");
        var result = await LocalReviewServerHost.RunAsync(options).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        Console.WriteLine($"[l4-re-clarify-backlog-review] {result.Outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine("[l4-re-clarify-backlog-review] Danach: l4-re-clarify-backlog-apply.");
        return 0;
    }

    private static async Task<CanonicalRequirementsBaseline> LoadBaselineBestEffortAsync(string repoRoot, string clustersPath)
    {
        try
        {
            var full = Path.IsPathRooted(clustersPath) ? clustersPath : Path.Combine(repoRoot, clustersPath);
            if (!File.Exists(full)) throw new FileNotFoundException(full);
            var clusters = await LoadAsync<FeatureClusterSet>(full).ConfigureAwait(false);
            var view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
            return view.Baseline;
        }
        catch
        {
            return new CanonicalRequirementsBaseline("", "", CanonicalRequirementsBaseline.CurrentSchemaVersion, DateTime.UtcNow, "", [], [], []);
        }
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<BacklogHumanDecisionsFile?> LoadExistingAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try { return JsonSerializer.Deserialize<BacklogHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json); }
        catch (Exception ex) { Console.Error.WriteLine($"[l4-re-clarify-backlog-review] WARNUNG: human-decisions.json nicht ladbar: {ex.Message}"); return null; }
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase)) return "all";
            if (string.Equals(args[i], "--blocked", StringComparison.OrdinalIgnoreCase)) return "blocked";
            if (!string.Equals(args[i], "--scope", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) return "all";
            return args[i + 1].Trim().ToLowerInvariant() is "blocked" ? "blocked" : "all";
        }
        return "all";
    }

    private static string? ResolveBacklogDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "product-backlog.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "backlog", "product-backlog.json"))) return Path.Combine(full, "backlog");

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var backlog = Path.Combine(dir, "backlog");
            if (File.Exists(Path.Combine(backlog, "product-backlog.json"))) return backlog;
            if (File.Exists(Path.Combine(dir, "product-backlog.json"))) return dir;
        }
        return null;
    }

    private static string ParentRunId(string backlogDir)
    {
        var name = Path.GetFileName(backlogDir);
        return string.Equals(name, "backlog", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(backlogDir) ?? backlogDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-re-clarify-backlog-review <l4-re-clarify-run|dir> [--interactive|--file] [--no-browser] [--scope all|blocked]");
    }
}
