using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-reverse-apply <github-reverse-run|dir>
// Deterministischer Apply der VERIFIZIERTEN Reverse-Ops in den Core (E4: done nur ueber freigegebenes PBI_DONE).
// Core NUR ueber den Port + Audit-Snapshot. human-decisions.json ist Pflicht.
public static class GithubReverseApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: github-reverse-apply <github-reverse-run|dir>"); return 2; }

        var planDir = GithubReverseReviewRunner.ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[github-reverse-apply] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-reverse-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-reverse-apply] github-reverse-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[github-reverse-apply] human-decisions.json fehlt - erst github-reverse-review."); return 2; }

        var plan = await LoadAsync<GithubReversePlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<GithubReverseDecisionsFile>(decisionsPath).ConfigureAwait(false);
        // E4/extern-nah: nur explizit freigegebene Ops (apply). Fehlende/skip-Entscheidung -> nicht anwenden.
        var accepted = decisions.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.OpId).ToHashSet(StringComparer.Ordinal);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-reverse-apply] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var appliedDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);

        var (updated, report) = GithubReverseApply.Apply(core, plan, accepted);
        await coreRepo.SaveAsync(updated).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-reverse-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-reverse-apply] accepted={report.Accepted} markedDone={report.MarkedDone.Count} mappingsClosed={report.MappingsClosed.Count} flagged={report.Flagged.Count} skipped={report.Skipped.Count}");
        foreach (var d in report.MarkedDone) Console.WriteLine($"[github-reverse-apply]   DONE {d}");
        foreach (var c in report.MappingsClosed) Console.WriteLine($"[github-reverse-apply]   MAPPING_CLOSED {c}");
        Console.WriteLine($"[github-reverse-apply] Core -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))} | {Path.GetRelativePath(repoRoot, appliedDir)}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
