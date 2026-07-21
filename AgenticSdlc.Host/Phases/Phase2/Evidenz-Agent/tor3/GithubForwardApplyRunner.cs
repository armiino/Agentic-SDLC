using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-forward-apply <github-forward-run> [--execute] [--repo owner/name] [--token-env NAME]
//
// T3.4 — der EINZIGE Punkt mit echtem GitHub-Write. Safe-by-default: OHNE --execute nur Vorschau (kein Write,
// kein Core-Change). Mit --execute werden NUR die vom Menschen akzeptierten Ops ausgefuehrt (human-decisions.json
// Pflicht) und das Mapping ueber T3.1 (implemented_by_issue) in den Core zurueckgeschrieben (schliesst die Dedup-
// Schleife). Rev-3 wird HIER, am irreversiblen Rand, nochmals erzwungen: CREATE ohne searchedQueries+searchEvidence
// wird abgelehnt (nicht geschrieben) — unabhaengig vom frueheren Gate.
public static class GithubForwardApplyRunner
{
    private const string UserAgent = "Agentic-SDLC";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var execute = args.Contains("--execute", StringComparer.OrdinalIgnoreCase);
        string? token = null, repoArg = null, tokenEnv = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--execute", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (string.Equals(a, "--token-env", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { tokenEnv = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward-apply] unbekanntes Argument: {a}"); return 2; }
            token ??= a;
        }
        if (token is null) { Usage(); return 2; }

        var planDir = GithubForwardReviewRunner.ResolvePlanDir(repoRoot, token);
        if (planDir is null) { Console.Error.WriteLine($"[github-forward-apply] Lauf '{token}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-forward-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-forward-apply] github-forward-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[github-forward-apply] human-decisions.json fehlt - erst github-forward-review fahren."); return 2; }

        var plan = await LoadAsync<GithubForwardPlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<GithubForwardDecisionsFile>(decisionsPath).ConfigureAwait(false);
        var accepted = decisions.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.OpId).ToHashSet(StringComparer.Ordinal);
        var repository = repoArg ?? plan.Repository;

        // Geteilte Ausfuehrung (identisch zum MAF-HITL-Pfad, S1). Fehlender Repo/Token bei echtem Write -> Exit 2.
        GithubForwardApplyReport report;
        try
        {
            report = await GithubForwardApply.ExecuteAsync(planDir, plan, accepted, execute, repoRoot, repository, tokenEnv).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"[github-forward-apply] {ex.Message}");
            return 2;
        }

        var appliedDir = Path.Combine(planDir, "applied");
        var sum = report.Summary;
        Console.WriteLine(execute
            ? $"[github-forward-apply] EXECUTED repo={repository} accepted={accepted.Count} created={sum.Created} updated={sum.Updated} commented={sum.Commented} linked={sum.Linked} rejected={sum.Rejected} failed={sum.Failed}"
            : $"[github-forward-apply] DRY-RUN (kein GitHub-Write, kein Core-Change) accepted={accepted.Count} wouldCreate={sum.Created} wouldUpdate={sum.Updated} wouldComment={sum.Commented} wouldLink={sum.Linked} rejected={sum.Rejected}");
        Console.WriteLine($"[github-forward-apply] flagged={sum.Flagged} held={sum.Held} noChange={sum.NoChange} skipped={sum.Skipped} alreadyApplied={sum.AlreadyApplied}");
        foreach (var o in report.Operations.Where(x => x.Status is "rejected" or "failed")) Console.WriteLine($"[github-forward-apply]   {o.Status.ToUpperInvariant()} {o.OpId} {o.PbiId}: {o.Message}");
        if (!execute) Console.WriteLine("[github-forward-apply] -> --execute --repo owner/name zum Ausfuehren (Token via Env).");
        Console.WriteLine($"[github-forward-apply] -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        return report.Success ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: github-forward-apply <github-forward-run|dir> [--execute] [--repo owner/name] [--token-env NAME]");
}
