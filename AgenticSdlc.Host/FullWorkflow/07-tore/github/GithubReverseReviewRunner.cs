using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Reverse-Plans (apply/skip; bei PBI_DONE = Verifikation, E4). Runner um die generische UI.
public static class GithubReverseReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: github-reverse-review <github-reverse-run|dir> [--interactive|--file] [--no-browser]"); return 2; }

        var planDir = ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[github-reverse-review] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-reverse-plan.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-reverse-review] github-reverse-plan.json fehlt."); return 2; }

        var plan = await LoadAsync<GithubReversePlanDocument>(planPath).ConfigureAwait(false);
        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        var interactive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase) || !args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

        // E0.5: Anzeige-Kontext laden (PBI-Titel aus dem Core, Issue-Titel/-Zustand aus dem Snapshot). Rein additiv —
        // scheitert das Laden, faellt das Gate auf die reine ID-Anzeige zurueck (context = null).
        var context = await LoadContextAsync(repoRoot, plan).ConfigureAwait(false);

        var session = GithubReverseReviewAdapter.BuildSession(runId, plan, context);
        if (session.Items.Count == 0) { Console.WriteLine("[github-reverse-review] keine Vorschlaege."); return 0; }

        var existing = File.Exists(decisionsPath) ? await LoadAsync<GithubReverseDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        GithubReverseReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = GithubReverseReviewAdapter.Resolved(it);

        if (!interactive)
        {
            Console.WriteLine($"[github-reverse-review] mode=file {session.Items.Count} Vorschlaege -> {Path.GetRelativePath(repoRoot, decisionsPath)} (oder --interactive).");
            return 0;
        }

        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: GithubReverseReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(GithubReverseReviewAdapter.ResolveContext(key, plan, context)),
            apply: s => GithubReverseReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[github-reverse-review] {outcome} - {session.ResolvedCount()}/{session.Items.Count} -> human-decisions.json");

        // R-43-Endform (09.08.): „Fertig" kettet den Apply automatisch (Core-Write, gate-autorisiert wie
        // pbi-update; Entscheidungs-Datei wurde zuerst geschrieben). --no-apply = Inspektions-Opt-out.
        if (args.Contains("--no-apply", StringComparer.OrdinalIgnoreCase)) return 0;
        if (outcome != AgenticSdlc.HumanReview.ReviewOutcome.Finished)
        { Console.WriteLine($"[github-reverse-review] nicht abgeschlossen ({outcome}) — kein Auto-Apply."); return 0; }
        Console.WriteLine("[github-reverse-review] R-43: Apply läuft automatisch an …");
        return await GithubReverseApplyRunner.RunAsync(["github-reverse-apply", args[1]], repoRoot).ConfigureAwait(false);
    }

    internal static string? ResolvePlanDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "github-reverse-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "plan", "github-reverse-plan.json"))) return Path.Combine(full, "plan");
        var root = Path.Combine(repoRoot, "runs", "github-reverse");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var plan = Path.Combine(dir, "plan");
            if (File.Exists(Path.Combine(plan, "github-reverse-plan.json"))) return plan;
        }
        return null;
    }

    private static Task<T> LoadAsync<T>(string path) => JsonFiles.LoadAsync<T>(path); // R3b: geteilt

    // E0.5 — PBI-Titel aus dem Core + Issue-Titel/-Zustand aus dem Plan-Snapshot. Best-effort: fehlt eine Quelle,
    // bleibt der jeweilige Teil leer (das Gate zeigt dann nur die IDs). Kein Einfluss auf Entscheidung/Apply.
    private static async Task<ReverseReviewContext?> LoadContextAsync(string repoRoot, GithubReversePlanDocument plan)
    {
        var pbiTitles = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var coreRepo = new JsonCoreRepository(repoRoot);
            if (await coreRepo.ExistsAsync().ConfigureAwait(false))
            {
                var core = await coreRepo.LoadAsync().ConfigureAwait(false);
                foreach (var it in core.Items.Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)))
                    pbiTitles[it.ItemId] = it.Text;
            }
        }
        catch { /* best-effort: ohne Core-Titel weiter */ }

        var issues = new Dictionary<int, GithubIssueSnapshot>();
        try
        {
            if (!string.IsNullOrWhiteSpace(plan.Snapshot))
            {
                var snapshotPath = Path.IsPathRooted(plan.Snapshot) ? plan.Snapshot : Path.Combine(repoRoot, plan.Snapshot);
                if (File.Exists(snapshotPath))
                    foreach (var iss in await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false))
                        issues[iss.IssueNumber] = iss;
            }
        }
        catch { /* best-effort: ohne Issue-Titel weiter */ }

        return pbiTitles.Count == 0 && issues.Count == 0 ? null : new ReverseReviewContext(pbiTitles, issues);
    }
}
