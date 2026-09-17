using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

/// <summary>
/// JSON-first Durchstich fuer den fachlichen Projektzustand.
/// Usage:
///   project-state-build <artifact.json>... [--l3-run <runId|path>] [--out <project-state.json>] [--project-id <id>]
/// </summary>
public static class ProjectStateBuildRunner
{
    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var artifacts = new List<string>();
        string? l3Run = null;
        string? output = null;
        string? projectId = null;
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--l3-run", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                l3Run = args[++i];
                continue;
            }
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                output = args[++i];
                continue;
            }
            if (string.Equals(arg, "--project-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                projectId = args[++i];
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[project-state] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            artifacts.Add(arg);
        }

        if (artifacts.Count == 0)
        {
            Console.Error.WriteLine("[project-state] mindestens ein artifact.json ist erforderlich.");
            Usage();
            return 2;
        }

        output ??= Path.Combine("runs", "project-state", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"), "project-state.json");
        var outputPath = Path.IsPathRooted(output) ? output : Path.Combine(repoRoot, output);

        var doc = await ProjectStateBuilder.BuildAsync(repoRoot, artifacts, l3Run, projectId).ConfigureAwait(false);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(doc, ProjectStateJson.Options)).ConfigureAwait(false);

        var byStatus = doc.Items.GroupBy(i => i.Status).OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key}={g.Count()}");
        var byType = doc.Items.GroupBy(i => i.ItemType).OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key}={g.Count()}");
        Console.WriteLine($"[project-state] items={doc.Items.Count} relations={doc.Relations.Count} provenance={doc.Provenance.Count}");
        Console.WriteLine($"[project-state] status: {string.Join(", ", byStatus)}");
        Console.WriteLine($"[project-state] types: {string.Join(", ", byType)}");
        if (doc.Proposals.Count > 0)
        {
            var byProposalStatus = doc.Proposals.GroupBy(p => p.Status).OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => $"{g.Key}={g.Count()}");
            Console.WriteLine($"[project-state] proposals={doc.Proposals.Count}: {string.Join(", ", byProposalStatus)}");
        }
        Console.WriteLine($"[project-state] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return 0;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: project-state-build <artifact.json>... [--l3-run <runId|path>] [--out <project-state.json>] [--project-id <id>]");
    }
}
