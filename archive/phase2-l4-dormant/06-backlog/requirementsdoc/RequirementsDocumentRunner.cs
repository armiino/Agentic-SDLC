using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class RequirementsDocumentRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-requirements-doc <l4-applied-dir|runId|canonical-requirements-baseline.json> [--out <requirements-document.md>]");
            return 2;
        }

        string? outArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outArg = args[++i];
                continue;
            }
            if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4-requirements-doc] unbekanntes Argument: {args[i]}");
                return 2;
            }
        }

        var resolved = ResolveInput(repoRoot, args[1]);
        if (resolved is null)
        {
            Console.Error.WriteLine($"[l4-requirements-doc] L4-Applied-Artefakt '{args[1]}' nicht gefunden.");
            return 2;
        }

        var baseline = await LoadAsync<CanonicalRequirementsBaseline>(resolved.BaselinePath).ConfigureAwait(false);
        var provenance = File.Exists(resolved.ProvenancePath)
            ? await LoadAsync<L4ProvenanceMap>(resolved.ProvenancePath).ConfigureAwait(false)
            : null;
        var readiness = File.Exists(resolved.ReadinessPath)
            ? await LoadAsync<RequirementsReadinessReport>(resolved.ReadinessPath).ConfigureAwait(false)
            : null;

        var outputPath = ResolvePath(repoRoot, outArg ?? Path.Combine(resolved.OutputDir, "requirements-document.md"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, RequirementsDocumentRenderer.Render(baseline, provenance, readiness)).ConfigureAwait(false);

        Console.WriteLine($"[l4-requirements-doc] requirements={baseline.Requirements.Count} openDecisions={baseline.OpenDecisions.Count}");
        if (readiness is not null)
            Console.WriteLine($"[l4-requirements-doc] ready={readiness.Summary.ReadyForIssuePlanning} needsDecision={readiness.Summary.NeedsDecision} needsBreakdown={readiness.Summary.NeedsBreakdown}");
        Console.WriteLine($"[l4-requirements-doc] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static ResolvedRequirementsDocumentInput? ResolveInput(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "canonical-requirements-baseline.json", StringComparison.OrdinalIgnoreCase))
        {
            var dir = Path.GetDirectoryName(full) ?? repoRoot;
            return ResolveDirectory(dir);
        }

        if (Directory.Exists(full))
        {
            var direct = ResolveDirectory(full);
            if (direct is not null) return direct;
            var applied = Path.Combine(full, "consolidation", "applied");
            if (Directory.Exists(applied)) return ResolveDirectory(applied);
        }

        var l4Root = Path.Combine(repoRoot, "runs", "l4");
        if (!Directory.Exists(l4Root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(l4Root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var applied = Path.Combine(runDir, "consolidation", "applied");
            if (Directory.Exists(applied))
            {
                var resolved = ResolveDirectory(applied);
                if (resolved is not null) return resolved;
            }
        }
        return null;
    }

    private static ResolvedRequirementsDocumentInput? ResolveDirectory(string dir)
    {
        var baselinePath = Path.Combine(dir, "canonical-requirements-baseline.json");
        if (!File.Exists(baselinePath)) return null;
        return new ResolvedRequirementsDocumentInput(
            BaselinePath: baselinePath,
            ProvenancePath: Path.Combine(dir, "provenance-map.json"),
            ReadinessPath: Path.Combine(dir, "requirements-readiness.json"),
            OutputDir: dir);
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private sealed record ResolvedRequirementsDocumentInput(
        string BaselinePath,
        string ProvenancePath,
        string ReadinessPath,
        string OutputDir);
}
