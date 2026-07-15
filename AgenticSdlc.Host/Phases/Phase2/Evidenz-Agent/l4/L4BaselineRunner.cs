using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

/// <summary>
/// L4-v1: deterministische Projektion des ProjectState in eine kanonische Requirements-Baseline.
/// Usage:
///   l4-baseline <project-state.json> [--out <dir>]
/// </summary>
public static class L4BaselineRunner
{
    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        string? projectState = null;
        string? outputDir = null;
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputDir = args[++i];
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }

            projectState ??= arg;
        }

        if (string.IsNullOrWhiteSpace(projectState))
        {
            Console.Error.WriteLine("[l4] project-state.json ist erforderlich.");
            Usage();
            return 2;
        }

        var projectStatePath = ResolvePath(repoRoot, projectState);
        if (!File.Exists(projectStatePath))
        {
            Console.Error.WriteLine($"[l4] project-state.json nicht gefunden: {projectState}");
            return 2;
        }

        outputDir ??= Path.Combine("runs", "l4", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        var outputPath = ResolvePath(repoRoot, outputDir);

        var json = await File.ReadAllTextAsync(projectStatePath).ConfigureAwait(false);
        var state = JsonSerializer.Deserialize<ProjectStateDocument>(json, ProjectStateJson.Options)
                    ?? throw new InvalidOperationException($"ProjectState konnte nicht gelesen werden: {projectStatePath}");

        var sourceRelativePath = Path.GetRelativePath(repoRoot, projectStatePath);
        var baseline = L4BaselineBuilder.Build(state, sourceRelativePath);

        Directory.CreateDirectory(outputPath);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "canonical-requirements-baseline.json"),
            JsonSerializer.Serialize(baseline, ProjectStateJson.Options)).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "requirements-baseline.md"),
            L4BaselineRenderer.RenderMarkdown(baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "traceability-matrix.md"),
            L4BaselineRenderer.RenderTraceabilityMarkdown(baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "open-decisions.json"),
            JsonSerializer.Serialize(baseline.OpenDecisions, ProjectStateJson.Options)).ConfigureAwait(false);

        var byStatus = baseline.Requirements
            .GroupBy(r => r.Status)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key}={g.Count()}");

        Console.WriteLine($"[l4] baseline={baseline.BaselineId}");
        Console.WriteLine($"[l4] requirements={baseline.Requirements.Count} ({string.Join(", ", byStatus)})");
        Console.WriteLine($"[l4] openDecisions={baseline.OpenDecisions.Count} traceLinks={baseline.TraceLinks.Count}");
        Console.WriteLine($"[l4] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return 0;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-baseline <project-state.json> [--out <dir>]");
    }
}
