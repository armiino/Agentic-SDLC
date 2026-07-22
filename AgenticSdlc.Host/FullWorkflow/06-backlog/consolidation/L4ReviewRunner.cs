using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4ReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-review <l4-consolidation-dir|runId> [--interactive|--file] [--no-browser] [--scope needs-human|changes|all]");
            return 2;
        }

        var consolidationDir = ResolveConsolidationDir(repoRoot, args[1]);
        if (consolidationDir is null)
        {
            Console.Error.WriteLine($"[l4-review] L4-Consolidation-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var planPath = Path.Combine(consolidationDir, "consolidation-plan.json");
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine("[l4-review] consolidation-plan.json fehlt.");
            return 2;
        }

        var plan = JsonSerializer.Deserialize<ConsolidationPlan>(await File.ReadAllTextAsync(planPath).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"ConsolidationPlan konnte nicht gelesen werden: {planPath}");
        var statePath = ResolvePath(repoRoot, plan.SourceProjectStatePath);
        var state = JsonSerializer.Deserialize<ProjectStateDocument>(await File.ReadAllTextAsync(statePath).ConfigureAwait(false), Json)
                    ?? throw new InvalidOperationException($"ProjectState konnte nicht gelesen werden: {statePath}");

        var decisionsPath = Path.Combine(consolidationDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(consolidationDir);

        var session = L4ReviewAdapter.BuildSession(runId, plan, state, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[l4-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        L4ReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = L4ReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[l4-review] mode=file scope={scope} {session.Items.Count} Operationen.");
            Console.WriteLine($"[l4-review] Schreibe/prüfe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        Console.WriteLine($"[l4-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} Operationen");
        if (existing is not null)
            Console.WriteLine($"[l4-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: L4ReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(L4ReviewAdapter.ResolveContext(key, state, plan)),
            apply: s => L4ReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[l4-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine($"[l4-review] Danach: l4-apply {Path.GetFileName(Path.GetDirectoryName(consolidationDir) ?? consolidationDir)}");
        return 0;
    }

    private static async Task<L4HumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<L4HumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
            return null;
        }
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase)) return "all";
            if (string.Equals(args[i], "--changes", StringComparison.OrdinalIgnoreCase)) return "changes";
            if (string.Equals(args[i], "--needs-human", StringComparison.OrdinalIgnoreCase)) return "needs-human";
            if (!string.Equals(args[i], "--scope", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) return "needs-human";
            var value = args[i + 1].Trim().ToLowerInvariant();
            return value is "all" or "changes" ? value : "needs-human";
        }
        return "needs-human";
    }

    private static string? ResolveConsolidationDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "consolidation-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "consolidation", "consolidation-plan.json"))) return Path.Combine(full, "consolidation");

        var l4Root = Path.Combine(repoRoot, "runs", "l4");
        if (!Directory.Exists(l4Root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(l4Root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var cons = Path.Combine(dir, "consolidation");
            if (File.Exists(Path.Combine(cons, "consolidation-plan.json"))) return cons;
            if (File.Exists(Path.Combine(dir, "consolidation-plan.json"))) return dir;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string consolidationDir)
    {
        var name = Path.GetFileName(consolidationDir);
        return string.Equals(name, "consolidation", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(consolidationDir) ?? consolidationDir)
            : name;
    }
}
