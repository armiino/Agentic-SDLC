using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubReconciliationApplyRunner
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
            Console.Error.WriteLine($"[github-reconciliation-apply] GitHub-Reconciliation-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var allowUnreviewed = args.Contains("--allow-unreviewed", StringComparer.OrdinalIgnoreCase);
        var inputPath = Path.Combine(planDir, "github-reconciliation-input.json");
        var planPath = Path.Combine(planDir, "github-action-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(inputPath) || !File.Exists(planPath) || (!allowUnreviewed && !File.Exists(decisionsPath)))
        {
            Console.Error.WriteLine("[github-reconciliation-apply] Input, github-action-plan.json oder human-decisions.json fehlt.");
            return 2;
        }

        var input = await LoadAsync<GithubReconciliationInput>(inputPath).ConfigureAwait(false);
        var sourcePlan = await LoadAsync<GithubActionPlanDocument>(planPath).ConfigureAwait(false);
        var decisions = File.Exists(decisionsPath)
            ? await LoadAsync<GithubActionHumanDecisionsFile>(decisionsPath).ConfigureAwait(false)
            : new GithubActionHumanDecisionsFile(ParentRunId(planDir), "system (--allow-unreviewed)", []);

        GithubReconciliationApplyResult result;
        try
        {
            result = GithubReconciliationApply.Apply(input, sourcePlan, decisions, allowUnreviewed);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-reconciliation-apply] Apply fehlgeschlagen: {ex.Message}");
            return 2;
        }

        var outDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-github-action-plan.json"), JsonSerializer.Serialize(result.AcceptedPlan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-action-plan-apply-report.json"), JsonSerializer.Serialize(result.Report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-github-action-plan-gate-report.json"), JsonSerializer.Serialize(result.Report.Gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-reconciliation-apply] acceptedActions={result.Report.AcceptedActions}/{result.Report.SourceActions} gate={result.Report.Gate.Decision} errors={result.Report.Gate.Errors.Count} warnings={result.Report.Gate.Warnings.Count}");
        Console.WriteLine($"[github-reconciliation-apply] decisions accept={result.Report.Accepted} edit={result.Report.Edited} reject={result.Report.Rejected} revise={result.Report.RevisionRequested} missing={result.Report.MissingDecisions.Count}");
        Console.WriteLine($"[github-reconciliation-apply] -> {Path.GetRelativePath(repoRoot, outDir)}");

        if (result.Report.MissingDecisions.Count > 0)
        {
            Console.Error.WriteLine("[github-reconciliation-apply] Nicht alle GitHubActionPlanItems wurden reviewed. Nutze --allow-unreviewed nur bewusst fuer partial review.");
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
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "github-action-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "github-action-plan.json"))) return Path.Combine(full, "plan");

        var root = Path.Combine(repoRoot, "runs", "github-reconciliation");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "github-action-plan.json"))) return plan;
            if (File.Exists(Path.Combine(dir, "github-action-plan.json"))) return dir;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string planDir)
    {
        var name = Path.GetFileName(planDir);
        return string.Equals(name, "plan", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-reconciliation-apply <github-reconciliation-dir|runId> [--allow-unreviewed]");
    }
}
