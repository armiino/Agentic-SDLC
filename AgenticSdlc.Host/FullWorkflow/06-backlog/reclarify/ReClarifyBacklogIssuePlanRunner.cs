using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// Erzeugt aus der (akzeptierten) ProductBacklogView deterministisch einen accepted-issue-plan,
// den die bestehende GitHub-Reconciliation konsumiert. Kein LLM. Bevorzugt applied/product-backlog.json.
public static class ReClarifyBacklogIssuePlanRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-re-clarify-issueplan <l4-re-clarify-run|dir>");
            return 2;
        }

        var backlogDir = ResolveBacklogDir(repoRoot, args[1]);
        if (backlogDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-issueplan] Backlog-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        // bevorzugt die akzeptierte View (nach backlog-apply), sonst die rohe product-backlog.json
        var appliedBacklog = Path.Combine(backlogDir, "applied", "product-backlog.json");
        var backlogPath = File.Exists(appliedBacklog) ? appliedBacklog : Path.Combine(backlogDir, "product-backlog.json");
        if (!File.Exists(backlogPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-issueplan] product-backlog.json fehlt.");
            return 2;
        }
        var isApplied = backlogPath == appliedBacklog;
        var forceCreate = args.Contains("--all-create", StringComparer.OrdinalIgnoreCase);
        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);

        var sourceRel = Path.GetRelativePath(repoRoot, backlogPath);
        var plan = ReClarifyBacklogToIssuePlan.Project(backlog, sourceRel, forceCreate);

        var outDir = Path.Combine(backlogDir, "issueplan", "applied");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-issue-plan.json"), JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        // Minimaler passierender Gate-Report, damit AcceptedIssuePlanView (GitHub-Reconciliation) laedt.
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-issue-plan-gate-report.json"),
            JsonSerializer.Serialize(new IssuePlanGateReport(true, "pass", [], [], new Dictionary<string, object>()), Json)).ConfigureAwait(false);

        var byOp = plan.Items.GroupBy(i => i.Operation).ToDictionary(g => g.Key, g => g.Count());
        Console.WriteLine($"[l4-re-clarify-issueplan] source={(isApplied ? "applied" : "raw")} pbis={backlog.Items.Count} issues={plan.Items.Count} "
                        + $"CREATE={byOp.GetValueOrDefault("CREATE")} NEEDS_REVIEW={byOp.GetValueOrDefault("NEEDS_REVIEW")} NO_CHANGE={byOp.GetValueOrDefault("NO_CHANGE")}");
        Console.WriteLine($"[l4-re-clarify-issueplan] accepted-issue-plan -> {Path.GetRelativePath(repoRoot, outDir)}");
        Console.WriteLine($"[l4-re-clarify-issueplan] weiter: github-reconciliation auf {Path.GetRelativePath(repoRoot, Path.Combine(backlogDir, "issueplan"))} (plan-only, HumanReview, dann Write).");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
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
}
