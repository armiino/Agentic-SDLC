using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4CompletionReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var completionDir = ResolveCompletionDir(repoRoot, args[1]);
        if (completionDir is null)
        {
            Console.Error.WriteLine($"[l4-completion-review] Completion-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var adequacyPath = Path.Combine(completionDir, "l4-adequacy-report.json");
        var proposalsPath = Path.Combine(completionDir, "l4-completion-proposals.json");
        if (!File.Exists(adequacyPath) || !File.Exists(proposalsPath))
        {
            Console.Error.WriteLine("[l4-completion-review] l4-adequacy-report.json oder l4-completion-proposals.json fehlt.");
            return 2;
        }

        var adequacy = await LoadAsync<L4AdequacyReport>(adequacyPath).ConfigureAwait(false);
        var proposals = await LoadAsync<L4CompletionProposalDocument>(proposalsPath).ConfigureAwait(false);
        var baselinePath = ResolveBaselinePath(repoRoot, proposals.SourceRequirementsDocumentPath);
        if (baselinePath is null)
        {
            Console.Error.WriteLine("[l4-completion-review] canonical-requirements-baseline.json konnte nicht zum Completion-Run gefunden werden.");
            return 2;
        }
        var baseline = await LoadAsync<CanonicalRequirementsBaseline>(baselinePath).ConfigureAwait(false);

        var decisionsPath = Path.Combine(completionDir, "human-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(completionDir);

        var session = L4CompletionReviewAdapter.BuildSession(runId, adequacy, proposals, baseline, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[l4-completion-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        L4CompletionReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = L4CompletionReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[l4-completion-review] mode=file scope={scope} {session.Items.Count} Proposals.");
            Console.WriteLine($"[l4-completion-review] Schreibe/prüfe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        Console.WriteLine($"[l4-completion-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} Proposals");
        if (existing is not null)
            Console.WriteLine($"[l4-completion-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: L4CompletionReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(L4CompletionReviewAdapter.ResolveContext(key, adequacy, proposals, baseline)),
            apply: s => L4CompletionReviewAdapter.Apply(runId, s, proposals),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[l4-completion-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine("[l4-completion-review] Danach: l4-completion-apply (naechster Schritt).");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<L4CompletionHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<L4CompletionHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-completion-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
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
            if (i + 1 >= args.Length) return "needs-human";
            var value = args[i + 1].Trim().ToLowerInvariant();
            return value is "all" ? "all" : "needs-human";
        }
        return "needs-human";
    }

    private static string? ResolveCompletionDir(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "l4-completion-proposals.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "completion", "l4-completion-proposals.json"))) return Path.Combine(full, "completion");

        var root = Path.Combine(repoRoot, "runs", "l4-completion");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var completion = Path.Combine(dir, "completion");
            if (File.Exists(Path.Combine(completion, "l4-completion-proposals.json"))) return completion;
            if (File.Exists(Path.Combine(dir, "l4-completion-proposals.json"))) return dir;
        }
        return null;
    }

    private static string? ResolveBaselinePath(string repoRoot, string sourceRequirementsDocumentPath)
    {
        var docPath = ResolvePath(repoRoot, sourceRequirementsDocumentPath);
        var dir = Path.GetDirectoryName(docPath);
        if (dir is null) return null;
        var baseline = Path.Combine(dir, "canonical-requirements-baseline.json");
        return File.Exists(baseline) ? baseline : null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string completionDir)
    {
        var name = Path.GetFileName(completionDir);
        return string.Equals(name, "completion", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(completionDir) ?? completionDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-completion-review <l4-completion-dir|runId> [--interactive|--file] [--no-browser] [--scope needs-human|all]");
    }
}
