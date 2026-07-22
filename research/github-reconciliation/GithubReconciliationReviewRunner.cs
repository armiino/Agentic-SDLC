using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubReconciliationReviewRunner
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
            Console.Error.WriteLine($"[github-reconciliation-review] GitHub-Reconciliation-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var inputPath = Path.Combine(planDir, "github-reconciliation-input.json");
        var planPath = Path.Combine(planDir, "github-action-plan.json");
        var gatePath = Path.Combine(planDir, "github-action-plan-gate-report.json");
        if (!File.Exists(inputPath) || !File.Exists(planPath) || !File.Exists(gatePath))
        {
            Console.Error.WriteLine("[github-reconciliation-review] Input, github-action-plan.json oder Gate-Report fehlt.");
            return 2;
        }

        var input = await LoadAsync<GithubReconciliationInput>(inputPath).ConfigureAwait(false);
        var plan = await LoadAsync<GithubActionPlanDocument>(planPath).ConfigureAwait(false);
        var gate = await LoadAsync<GithubActionPlanGateReport>(gatePath).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        var acceptWriteable = args.Contains("--accept-writeable", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(planDir);

        var session = GithubReconciliationReviewAdapter.BuildSession(runId, input, plan, gate, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[github-reconciliation-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        GithubReconciliationReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = GithubReconciliationReviewAdapter.Resolved(item);

        if (acceptWriteable)
        {
            MarkNeedsReviewAsNoChange(session);
            await File.WriteAllTextAsync(
                decisionsPath,
                JsonSerializer.Serialize(GithubReconciliationReviewAdapter.Apply(runId, session, plan), Json)).ConfigureAwait(false);
            Console.WriteLine($"[github-reconciliation-review] accept-writeable scope={scope} {session.Items.Count} GitHubActionPlanItems -> human-decisions.json");
            Console.WriteLine("[github-reconciliation-review] NEEDS_REVIEW wurde als NO_CHANGE editiert, damit Klaerungsthemen nicht in Delivery-Write laufen.");
            return 0;
        }

        if (acceptAll)
        {
            await File.WriteAllTextAsync(
                decisionsPath,
                JsonSerializer.Serialize(GithubReconciliationReviewAdapter.Apply(runId, session, plan), Json)).ConfigureAwait(false);
            Console.WriteLine($"[github-reconciliation-review] accept-all scope={scope} {session.Items.Count} GitHubActionPlanItems -> human-decisions.json");
            return 0;
        }

        if (!interactive)
        {
            Console.WriteLine($"[github-reconciliation-review] mode=file scope={scope} {session.Items.Count} GitHubActionPlanItems.");
            Console.WriteLine($"[github-reconciliation-review] Schreibe/pruefe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        async Task Persist() => await File.WriteAllTextAsync(
            decisionsPath,
            JsonSerializer.Serialize(GithubReconciliationReviewAdapter.Apply(runId, session, plan), Json)).ConfigureAwait(false);

        var options = new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = GithubReconciliationReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(GithubReconciliationReviewAdapter.ResolveContext(key, input, plan, gate)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        };

        Console.WriteLine($"[github-reconciliation-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} GitHubActionPlanItems");
        if (existing is not null)
            Console.WriteLine($"[github-reconciliation-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var result = await LocalReviewServerHost.RunAsync(options).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        Console.WriteLine($"[github-reconciliation-review] {result.Outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine("[github-reconciliation-review] Danach: github-reconciliation-apply (naechster Schritt).");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<GithubActionHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<GithubActionHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-reconciliation-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
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
        return "needs-human";
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
        Console.Error.WriteLine("Usage: github-reconciliation-review <github-reconciliation-dir|runId> [--interactive|--file|--accept-all|--accept-writeable] [--no-browser] [--scope all|needs-human]");
    }

    private static void MarkNeedsReviewAsNoChange(ReviewSession session)
    {
        foreach (var item in session.Items.Where(i => string.Equals(i.Badge, "NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase)))
        {
            Set(item, GithubReconciliationReviewAdapter.FieldDecision, "edit");
            Set(item, GithubReconciliationReviewAdapter.FieldEditOperation, "NO_CHANGE");
            Set(item, GithubReconciliationReviewAdapter.FieldEditRequiresHumanReview, "false");
            Set(item, GithubReconciliationReviewAdapter.FieldReason, "auto: Kein Delivery-Write; dieser Klaerungs-/Entscheidungspunkt laeuft im Clarification-Zweig weiter.");
            item.Resolved = GithubReconciliationReviewAdapter.Resolved(item);
        }
    }

    private static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }
}
