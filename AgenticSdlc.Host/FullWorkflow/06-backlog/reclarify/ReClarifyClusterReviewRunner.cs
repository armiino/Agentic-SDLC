using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// HumanReview der Feature-Cluster - reiner Runner um die generische AgenticSdlc.HumanReview-UI,
// exakt wie IssuePlanningReviewRunner. Schreibt human-decisions.json in den Cluster-Run.
public static class ReClarifyClusterReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var clustersDir = ResolveClustersDir(repoRoot, args[1]);
        if (clustersDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-review] Cluster-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var clustersPath = Path.Combine(clustersDir, "feature-clusters.json");
        var gatePath = Path.Combine(clustersDir, "cluster-gate-report.json");
        var reviewPath = Path.Combine(clustersDir, "cluster-review.json");
        if (!File.Exists(clustersPath) || !File.Exists(gatePath) || !File.Exists(reviewPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-review] feature-clusters.json, cluster-gate-report.json oder cluster-review.json fehlt.");
            return 2;
        }

        var clusters = await LoadAsync<FeatureClusterSet>(clustersPath).ConfigureAwait(false);
        var gate = await LoadAsync<ReClarifyGateReport>(gatePath).ConfigureAwait(false);
        var review = await LoadAsync<ClusterReviewReport>(reviewPath).ConfigureAwait(false);

        CanonicalRequirementsBaseline baseline;
        try
        {
            var view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
            baseline = view.Baseline;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify-review] Baseline nicht gefunden ({clusters.SourceBaselinePath}): {ex.Message}");
            return 2;
        }

        var decisionsPath = Path.Combine(clustersDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(clustersDir);

        var session = ReClarifyClusterReviewAdapter.BuildSession(runId, baseline, clusters, gate, review, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[l4-re-clarify-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        ReClarifyClusterReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = ReClarifyClusterReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[l4-re-clarify-review] mode=file scope={scope} {session.Items.Count} vorgeschlagene Korrekturen.");
            Console.WriteLine($"[l4-re-clarify-review] Schreibe/pruefe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        Console.WriteLine($"[l4-re-clarify-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} vorgeschlagene Korrekturen");
        if (existing is not null)
            Console.WriteLine($"[l4-re-clarify-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: ReClarifyClusterReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(ReClarifyClusterReviewAdapter.ResolveContext(key, baseline, clusters, gate, review)),
            apply: s => ReClarifyClusterReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[l4-re-clarify-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<ClusterHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<ClusterHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
            return null;
        }
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase)) return "all";
            if (string.Equals(args[i], "--flagged", StringComparison.OrdinalIgnoreCase)) return "flagged";
            if (!string.Equals(args[i], "--scope", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) return "all";
            var value = args[i + 1].Trim().ToLowerInvariant();
            return value is "flagged" ? "flagged" : "all";
        }
        return "all";
    }

    private static string? ResolveClustersDir(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "feature-clusters.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "clusters", "feature-clusters.json"))) return Path.Combine(full, "clusters");

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var clusters = Path.Combine(dir, "clusters");
            if (File.Exists(Path.Combine(clusters, "feature-clusters.json"))) return clusters;
            if (File.Exists(Path.Combine(dir, "feature-clusters.json"))) return dir;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string clustersDir)
    {
        var name = Path.GetFileName(clustersDir);
        return string.Equals(name, "clusters", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(clustersDir) ?? clustersDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-re-clarify-review <l4-re-clarify-dir|runId> [--interactive|--file] [--no-browser] [--scope all|flagged]");
    }
}
