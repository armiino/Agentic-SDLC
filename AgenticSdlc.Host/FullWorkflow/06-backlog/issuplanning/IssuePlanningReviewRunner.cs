using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanningReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null)
        {
            Console.Error.WriteLine($"[l4-issuplanning-review] IssuePlanning-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var planPath = Path.Combine(planDir, "issue-plan.json");
        var gatePath = Path.Combine(planDir, "issue-plan-gate-report.json");
        if (!File.Exists(planPath) || !File.Exists(gatePath))
        {
            Console.Error.WriteLine("[l4-issuplanning-review] issue-plan.json oder issue-plan-gate-report.json fehlt.");
            return 2;
        }

        var plan = await LoadAsync<IssuePlanDocument>(planPath).ConfigureAwait(false);
        var gate = await LoadAsync<IssuePlanGateReport>(gatePath).ConfigureAwait(false);
        var inputPath = ResolvePath(repoRoot, plan.SourceIssuePlanningInputPath);
        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"[l4-issuplanning-review] IssuePlanningInput nicht gefunden: {plan.SourceIssuePlanningInputPath}");
            return 2;
        }
        var input = await LoadAsync<IssuePlanningInput>(inputPath).ConfigureAwait(false);

        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(planDir);

        var session = IssuePlanningReviewAdapter.BuildSession(runId, input, plan, gate, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[l4-issuplanning-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        IssuePlanningReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = IssuePlanningReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[l4-issuplanning-review] mode=file scope={scope} {session.Items.Count} IssuePlanItems.");
            Console.WriteLine($"[l4-issuplanning-review] Schreibe/prüfe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        Console.WriteLine($"[l4-issuplanning-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} IssuePlanItems");
        if (existing is not null)
            Console.WriteLine($"[l4-issuplanning-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: IssuePlanningReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(IssuePlanningReviewAdapter.ResolveContext(key, input, plan, gate)),
            apply: s => IssuePlanningReviewAdapter.Apply(runId, s, plan),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[l4-issuplanning-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine("[l4-issuplanning-review] Danach: l4-issuplanning-apply (naechster Schritt).");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<IssuePlanningHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<IssuePlanningHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
            return null;
        }
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase)) return "all";
            if (string.Equals(args[i], "--needs-human", StringComparison.OrdinalIgnoreCase)) return "needs-human";
            if (!string.Equals(args[i], "--scope", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) return "all";
            var value = args[i + 1].Trim().ToLowerInvariant();
            return value is "needs-human" ? "needs-human" : "all";
        }
        return "all";
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

    private static string ParentRunId(string planDir)
    {
        var name = Path.GetFileName(planDir);
        return string.Equals(name, "plan", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-issuplanning-review <l4-issuplanning-dir|runId> [--interactive|--file] [--no-browser] [--scope all|needs-human]");
    }
}
