using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class IssuePlanningApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null)
        {
            Console.Error.WriteLine($"[l4-issuplanning-apply] IssuePlanning-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var allowUnreviewed = args.Contains("--allow-unreviewed", StringComparer.OrdinalIgnoreCase);
        var planPath = Path.Combine(planDir, "issue-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath) || !File.Exists(decisionsPath))
        {
            Console.Error.WriteLine("[l4-issuplanning-apply] issue-plan.json oder human-decisions.json fehlt.");
            return 2;
        }

        var sourcePlan = await LoadAsync<IssuePlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<IssuePlanningHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);
        var inputPath = ResolvePath(repoRoot, sourcePlan.SourceIssuePlanningInputPath);
        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"[l4-issuplanning-apply] IssuePlanningInput nicht gefunden: {sourcePlan.SourceIssuePlanningInputPath}");
            return 2;
        }
        var input = await LoadAsync<IssuePlanningInput>(inputPath).ConfigureAwait(false);

        IssuePlanningApplyResult result;
        try
        {
            result = IssuePlanningApply.Apply(input, sourcePlan, decisions, allowUnreviewed);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning-apply] Apply fehlgeschlagen: {ex.Message}");
            return 2;
        }

        var outDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-issue-plan.json"), JsonSerializer.Serialize(result.AcceptedPlan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "issue-plan-apply-report.json"), JsonSerializer.Serialize(result.Report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-issue-plan-gate-report.json"), JsonSerializer.Serialize(result.Report.Gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-issuplanning-apply] acceptedItems={result.Report.AcceptedItems}/{result.Report.SourceItems} gate={result.Report.Gate.Decision} errors={result.Report.Gate.Errors.Count} warnings={result.Report.Gate.Warnings.Count}");
        Console.WriteLine($"[l4-issuplanning-apply] decisions accept={result.Report.Accepted} edit={result.Report.Edited} reject={result.Report.Rejected} revise={result.Report.RevisionRequested} missing={result.Report.MissingDecisions.Count}");
        Console.WriteLine($"[l4-issuplanning-apply] -> {Path.GetRelativePath(repoRoot, outDir)}");

        if (result.Report.MissingDecisions.Count > 0)
        {
            Console.Error.WriteLine("[l4-issuplanning-apply] Nicht alle IssuePlanItems wurden reviewed. Nutze --allow-unreviewed nur bewusst fuer partial review.");
            return 1;
        }
        return result.Report.Gate.Pass ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "issue-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "issue-plan.json"))) return Path.Combine(full, "plan");

        var root = Path.Combine(repoRoot, "runs", "l4-issuplanning");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "issue-plan.json"))) return plan;
            if (File.Exists(Path.Combine(dir, "issue-plan.json"))) return dir;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-issuplanning-apply <l4-issuplanning-dir|runId> [--allow-unreviewed]");
    }
}
