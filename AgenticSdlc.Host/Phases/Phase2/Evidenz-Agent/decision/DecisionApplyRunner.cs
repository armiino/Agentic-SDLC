using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// CLI: decision-apply <decision-run|dir>
// T2.1 Apply (deterministisch): führt die freigegebenen Auflösungen in den Core aus (DEC resolved, Widerspruch
// aufgelöst, PBIs entblockt/geswappt), Core NUR über den Port + Audit-Snapshot. Schreibt ein github-sync-Delta der
// entblockten/geänderten PBIs → der nächste github-forward bringt die freigewordene Arbeit nach GitHub (Kreis zu).
public static class DecisionApplyRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: decision-apply <decision-run|dir>"); return 2; }

        var planDir = DecisionReviewRunner.ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[decision-apply] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "decision-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[decision-apply] decision-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[decision-apply] human-decisions.json fehlt - erst decision-review."); return 2; }

        var plan = await LoadAsync<DecisionResolutionPlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<DecisionResolutionDecisionsFile>(decisionsPath).ConfigureAwait(false);

        var byOp = decisions.Decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var accepted = new HashSet<int>();
        for (var i = 0; i < plan.Operations.Count; i++)
            if (byOp.TryGetValue($"op-{i}", out var d) && string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                accepted.Add(i);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[decision-apply] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var appliedDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);

        var runId = Path.GetFileName(Path.GetDirectoryName(planDir) ?? planDir);
        var (updated, report) = DecisionResolutionApply.Apply(core, plan, accepted, runId);
        await coreRepo.SaveAsync(updated).ConfigureAwait(false);

        // github-sync-Delta der entblockten/geänderten PBIs (→ Tor 3).
        var touched = report.UnblockedPbis.Concat(report.SwappedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(updated).Entries.Where(e => touched.Contains(e.PbiId)).ToList();
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-sync-delta.json"),
            JsonSerializer.Serialize(new { newPbis = Array.Empty<string>(), updatedPbis = touched.OrderBy(x => x, StringComparer.Ordinal).ToArray(), entries = syncDelta }, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "decision-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[decision-apply] accepted={accepted.Count}/{plan.Operations.Count} resolved={report.Resolved.Count} superseded={report.SupersededRequirements.Count} refined={report.RefinedRequirements.Count} newReqs={report.NewRequirements.Count} unblocked={report.UnblockedPbis.Count} swapped={report.SwappedPbis.Count}");
        Console.WriteLine($"[decision-apply] github-sync-Delta: {syncDelta.Count} PBIs -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        foreach (var s in report.Skipped) Console.WriteLine($"[decision-apply]   skip {s}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
