using AgenticSdlc.Host.Configuration;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class OpenRequirementsReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var auditPath = ResolveAuditPath(repoRoot, args[1]);
        if (auditPath is null)
        {
            Console.Error.WriteLine($"[open-requirements-review] operationalization-audit nicht gefunden: {args[1]}");
            return 2;
        }

        var audit = await LoadAsync<OperationalizationAuditDocument>(auditPath).ConfigureAwait(false);
        var readiness = await TryLoadReadinessAsync(repoRoot, audit).ConfigureAwait(false);
        var decisionsPath = Path.Combine(Path.GetDirectoryName(auditPath) ?? repoRoot, "open-requirement-decisions.json");
        var scope = ParseScope(args);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;
        var runId = ParentRunId(auditPath);

        var session = OpenRequirementsReviewAdapter.BuildSession(runId, audit, readiness, scope);
        if (session.Items.Count == 0)
        {
            Console.WriteLine($"[open-requirements-review] keine Review-Items fuer scope={scope}.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        OpenRequirementsReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = OpenRequirementsReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[open-requirements-review] mode=file scope={scope} {session.Items.Count} offene Requirements.");
            Console.WriteLine($"[open-requirements-review] Schreibe/pruefe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        async Task Persist() => await File.WriteAllTextAsync(
            decisionsPath,
            JsonSerializer.Serialize(OpenRequirementsReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);

        var options = new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = OpenRequirementsReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(OpenRequirementsReviewAdapter.ResolveContext(key, audit, readiness)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        };

        Console.WriteLine($"[open-requirements-review] mode=interactive scope={scope} runId={runId} {session.Items.Count} offene Requirements");
        if (existing is not null)
            Console.WriteLine($"[open-requirements-review] Re-Launch: vorhandene Decisions geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var result = await LocalReviewServerHost.RunAsync(options).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);
        Console.WriteLine($"[open-requirements-review] {result.Outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> open-requirement-decisions.json");
        Console.WriteLine("[open-requirements-review] Danach: open-requirements-apply.");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static async Task<RequirementsReadinessReport?> TryLoadReadinessAsync(string repoRoot, OperationalizationAuditDocument audit)
    {
        var path = audit.SourcePaths.ReadinessReportPath;
        if (string.IsNullOrWhiteSpace(path)) return null;
        var full = ResolvePath(repoRoot, path);
        return File.Exists(full) ? await LoadAsync<RequirementsReadinessReport>(full).ConfigureAwait(false) : null;
    }

    private static async Task<OpenRequirementHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<OpenRequirementHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[open-requirements-review] WARNUNG: vorhandene Decisions konnten nicht geladen werden: {ex.Message}");
            return null;
        }
    }

    internal static string? ResolveAuditPath(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && Path.GetFileName(full).Equals("operationalization-audit.json", StringComparison.OrdinalIgnoreCase)) return full;
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "operationalization-audit.json");
            if (File.Exists(direct)) return direct;
            var nested = Path.Combine(full, "operationalization-audit", "operationalization-audit.json");
            if (File.Exists(nested)) return nested;
            var appliedNested = Path.Combine(full, "plan", "applied", "operationalization-audit", "operationalization-audit.json");
            if (File.Exists(appliedNested)) return appliedNested;
        }

        var root = Path.Combine(repoRoot, "runs", "github-reconciliation");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var candidate = Path.Combine(dir, "plan", "applied", "operationalization-audit", "operationalization-audit.json");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--scope", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                return args[i + 1].Trim().ToLowerInvariant();
        }
        return "open";
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string auditPath)
    {
        var dir = Path.GetDirectoryName(auditPath) ?? auditPath;
        var applied = Directory.GetParent(dir)?.FullName ?? dir;
        var plan = Directory.GetParent(applied)?.FullName ?? applied;
        var run = Directory.GetParent(plan)?.FullName ?? plan;
        return Path.GetFileName(run);
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: open-requirements-review <operationalization-audit.json|github-reconciliation-runId> [--interactive|--file] [--no-browser] [--scope open|all|needs-breakdown|deferred]");
    }
}
