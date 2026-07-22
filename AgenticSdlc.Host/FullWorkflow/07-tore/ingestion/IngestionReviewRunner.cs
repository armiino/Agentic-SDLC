using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// HumanReview der Ingestion-Operationen - Runner um die generische AgenticSdlc.HumanReview-UI,
// exakt wie ReClarifyClusterReviewRunner. Schreibt human-decisions.json in den Ingestion-Run.
public static class IngestionReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null)
        {
            Console.Error.WriteLine($"[ingest-review] Ingestion-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var planPath = Path.Combine(planDir, "plan.json");
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine("[ingest-review] plan.json fehlt.");
            return 2;
        }
        var plan = await LoadAsync<StateChangePlanDocument>(planPath).ConfigureAwait(false);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[ingest-review] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaFull = Path.IsPathRooted(plan.SourceMeetingDeltaPath) ? plan.SourceMeetingDeltaPath : Path.Combine(repoRoot, plan.SourceMeetingDeltaPath);
        if (!File.Exists(deltaFull))
        {
            Console.Error.WriteLine($"[ingest-review] MeetingDelta nicht gefunden: {deltaFull}");
            return 2;
        }
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaFull).ConfigureAwait(false)).Document;

        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var interactive = forceInteractive || !forceFile;

        var session = IngestionReviewAdapter.BuildSession(runId, plan, delta, core);
        if (session.Items.Count == 0)
        {
            Console.WriteLine("[ingest-review] keine Operationen zu reviewen.");
            return 0;
        }

        var existing = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        IngestionReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var item in session.Items)
            item.Resolved = IngestionReviewAdapter.Resolved(item);

        if (!interactive)
        {
            Console.WriteLine($"[ingest-review] mode=file {session.Items.Count} Operationen. Schreibe/pruefe {Path.GetRelativePath(repoRoot, decisionsPath)} oder starte mit --interactive.");
            return 0;
        }

        Console.WriteLine($"[ingest-review] mode=interactive runId={runId} {session.Items.Count} Operationen");
        if (existing is not null)
            Console.WriteLine($"[ingest-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: IngestionReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(IngestionReviewAdapter.ResolveContext(key, plan, delta, core)),
            apply: s => IngestionReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[ingest-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        return 0;
    }

    private static Task<T> LoadAsync<T>(string path) => JsonFiles.LoadAsync<T>(path); // R3b: geteilt

    private static async Task<IngestionHumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try { return JsonSerializer.Deserialize<IngestionHumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json); }
        catch (Exception ex) { Console.Error.WriteLine($"[ingest-review] WARNUNG: human-decisions.json nicht ladbar: {ex.Message}"); return null; }
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "plan.json"))) return Path.Combine(full, "plan");

        var root = Path.Combine(repoRoot, "runs", "ingestion");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "plan.json"))) return plan;
            if (File.Exists(Path.Combine(dir, "plan.json"))) return dir;
        }
        return null;
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: ingest-review <ingestion-run|dir> [--interactive|--file] [--no-browser]");
}
