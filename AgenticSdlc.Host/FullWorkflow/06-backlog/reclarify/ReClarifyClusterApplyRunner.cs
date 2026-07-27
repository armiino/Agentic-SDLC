using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Materialisiert die menschlich akzeptierten Cluster-Operationen deterministisch in einen erweiterten
// Cluster-Stand (+ erneuter Coverage-Check). Analog l4-issuplanning-apply. Kein LLM.
public static class ReClarifyClusterApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-re-clarify-apply <l4-re-clarify-dir|runId>");
            return 2;
        }

        var clustersDir = ResolveClustersDir(repoRoot, args[1]);
        if (clustersDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-apply] Cluster-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var clustersPath = Path.Combine(clustersDir, "feature-clusters.json");
        var reviewPath = Path.Combine(clustersDir, "cluster-review.json");
        var decisionsPath = Path.Combine(clustersDir, "human-decisions.json");
        if (!File.Exists(clustersPath) || !File.Exists(reviewPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-apply] feature-clusters.json oder cluster-review.json fehlt.");
            return 2;
        }
        if (!File.Exists(decisionsPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-apply] human-decisions.json fehlt - erst l4-re-clarify-review fahren.");
            return 2;
        }

        var decisions = await LoadAsync<ClusterHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);
        var accepted = decisions.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.OpId)
            .ToHashSet(StringComparer.Ordinal);

        ClusterApplyResult result;
        int clustersBefore;
        try
        {
            (result, clustersBefore, _) = await ExecuteAsync(clustersDir, repoRoot, accepted).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify-apply] {ex.Message}");
            return 2;
        }

        Console.WriteLine($"[l4-re-clarify-apply] accepted={accepted.Count} applied={result.AppliedOpIds.Count} skipped={result.SkippedOps.Count} "
                        + $"clusters {clustersBefore}->{result.Updated.Clusters.Count} gate={(result.Gate.Pass ? "pass" : "fail")} errors={result.Gate.Errors.Count}");
        foreach (var s in result.SkippedOps) Console.WriteLine($"[l4-re-clarify-apply]   skip {s}");
        Console.WriteLine($"[l4-re-clarify-apply] -> {Path.GetRelativePath(repoRoot, Path.Combine(clustersDir, "applied"))}");
        return result.Gate.Pass ? 0 : 1;
    }

    // pipeline-full (B3): der EINE Apply-Kern fuer CLI-Runner UND Graph-Knoten (Muster: IngestionApplyExec).
    // Laedt clusters+review aus clustersDir, wendet die akzeptierten Operationen deterministisch an
    // (ReClarifyClusterApply.Apply) und materialisiert applied/. Wirft bei fehlenden Artefakten/Baseline.
    internal static async Task<(ClusterApplyResult Result, int ClustersBefore, string AppliedDir)> ExecuteAsync(
        string clustersDir, string repoRoot, ISet<string> acceptedOpIds)
    {
        var clustersPath = Path.Combine(clustersDir, "feature-clusters.json");
        var clusters = await LoadAsync<FeatureClusterSet>(clustersPath).ConfigureAwait(false);
        var review = await LoadAsync<ClusterReviewReport>(Path.Combine(clustersDir, "cluster-review.json")).ConfigureAwait(false);

        var view = await new JsonProjectStateViewRepository(repoRoot)
            .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
            .ConfigureAwait(false);

        var result = ReClarifyClusterApply.Apply(view.Baseline, clusters, review.Operations, acceptedOpIds);

        var appliedDir = Path.Combine(clustersDir, "applied");
        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "feature-clusters.json"), JsonSerializer.Serialize(result.Updated, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "cluster-gate-report.json"), JsonSerializer.Serialize(result.Gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "cluster-apply-report.json"), JsonSerializer.Serialize(new
        {
            sourceClusters = Path.GetRelativePath(repoRoot, clustersPath),
            acceptedOps = acceptedOpIds.Count,
            appliedOps = result.AppliedOpIds,
            skippedOps = result.SkippedOps,
            removedEmptyClusters = result.RemovedClusters,
            clustersBefore = clusters.Clusters.Count,
            clustersAfter = result.Updated.Clusters.Count,
            gatePass = result.Gate.Pass,
            gateErrors = result.Gate.Errors.Count,
            timestampUtc = DateTime.UtcNow
        }, Json)).ConfigureAwait(false);

        return (result, clusters.Clusters.Count, appliedDir);
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveClustersDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
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
}
