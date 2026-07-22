using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class ClarificationPlanningApplyRunner
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
            Console.Error.WriteLine($"[clarification-agent-apply] Clarification-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var allowUnreviewed = args.Contains("--allow-unreviewed", StringComparer.OrdinalIgnoreCase);
        var planPath = Path.Combine(planDir, "clarification-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine("[clarification-agent-apply] clarification-plan.json fehlt.");
            return 2;
        }
        if (!File.Exists(decisionsPath) && !allowUnreviewed)
        {
            Console.Error.WriteLine("[clarification-agent-apply] human-decisions.json fehlt. Nutze --allow-unreviewed nur fuer technischen Durchstich.");
            return 2;
        }

        var sourcePlan = await LoadAsync<ClarificationPlanDocument>(planPath).ConfigureAwait(false);
        var decisions = File.Exists(decisionsPath)
            ? await LoadAsync<ClarificationPlanningHumanDecisionsFile>(decisionsPath).ConfigureAwait(false)
            : new ClarificationPlanningHumanDecisionsFile(ParentRunId(planDir), "technical allow-unreviewed", []);
        var inputPath = ResolvePath(repoRoot, sourcePlan.SourceClarificationPlanningInputPath);
        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"[clarification-agent-apply] ClarificationPlanningInput nicht gefunden: {sourcePlan.SourceClarificationPlanningInputPath}");
            return 2;
        }
        var input = await LoadAsync<ClarificationPlanningInput>(inputPath).ConfigureAwait(false);

        ClarificationPlanningApplyResult result;
        try
        {
            result = ClarificationPlanningApply.Apply(input, sourcePlan, decisions, allowUnreviewed);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clarification-agent-apply] Apply fehlgeschlagen: {ex.Message}");
            return 2;
        }

        var outDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-clarification-plan.json"), JsonSerializer.Serialize(result.AcceptedPlan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "clarification-plan-apply-report.json"), JsonSerializer.Serialize(result.Report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-clarification-plan-gate-report.json"), JsonSerializer.Serialize(result.Report.Gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[clarification-agent-apply] acceptedItems={result.Report.AcceptedItems}/{result.Report.SourceItems} gate={result.Report.Gate.Decision} errors={result.Report.Gate.Errors.Count} warnings={result.Report.Gate.Warnings.Count}");
        Console.WriteLine($"[clarification-agent-apply] decisions accept={result.Report.Accepted} edit={result.Report.Edited} reject={result.Report.Rejected} revise={result.Report.RevisionRequested} missing={result.Report.MissingDecisions.Count}");
        Console.WriteLine($"[clarification-agent-apply] -> {Path.GetRelativePath(repoRoot, outDir)}");

        if (result.Report.MissingDecisions.Count > 0)
        {
            Console.Error.WriteLine("[clarification-agent-apply] Nicht alle ClarificationPlanItems wurden reviewed. Nutze --allow-unreviewed nur bewusst fuer partial review.");
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
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "clarification-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "clarification-plan.json"))) return Path.Combine(full, "plan");

        var root = Path.Combine(repoRoot, "runs", "clarification-agent");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "clarification-plan.json"))) return plan;
            if (File.Exists(Path.Combine(dir, "clarification-plan.json"))) return dir;
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
        Console.Error.WriteLine("Usage: clarification-agent-apply <clarification-agent-dir|runId> [--allow-unreviewed]");
    }
}
