using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// Deterministischer Apply der akzeptierten PBI-Operationen in den Core (Update-by-Identity, Merge/Praezedenz,
// Requirement-Swap). Core NUR ueber den Port + Audit-Snapshot. Schreibt ein github-sync-Delta (betroffene PBIs).
public static class PbiUpdateApplyRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: pbi-update-apply <pbi-update-run|dir>"); return 2; }

        var planDir = PbiUpdateReviewRunner.ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null) { Console.Error.WriteLine($"[pbi-update-apply] Lauf '{args[1]}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "pbi-change-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[pbi-update-apply] pbi-change-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[pbi-update-apply] human-decisions.json fehlt - erst pbi-update-review."); return 2; }

        var plan = await LoadAsync<PbiStateChangePlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<PbiUpdateDecisionsFile>(decisionsPath).ConfigureAwait(false);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pbi-update-apply] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        // akzeptiert = op-<i> mit decision=apply (fehlende Entscheidung -> default apply).
        var byOp = decisions.Decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var accepted = new HashSet<int>();
        for (var i = 0; i < plan.Operations.Count; i++)
            if (!byOp.TryGetValue($"op-{i}", out var d) || string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                accepted.Add(i);

        var appliedDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);

        var (updated, report) = PbiUpdateApply.Apply(core, plan, accepted, plan.SourceIngestionRun);
        await coreRepo.SaveAsync(updated).ConfigureAwait(false);

        // github-sync-Delta: nur die betroffenen (neuen/aktualisierten) PBIs.
        var touched = report.NewPbis.Concat(report.UpdatedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(updated).Entries.Where(e => touched.Contains(e.PbiId)).ToList();
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-sync-delta.json"), JsonSerializer.Serialize(new { newPbis = report.NewPbis, updatedPbis = report.UpdatedPbis, entries = syncDelta }, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "pbi-update-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[pbi-update-apply] accepted={accepted.Count}/{plan.Operations.Count} newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count} relations(+{report.RelationsAdded}/-{report.RelationsRemoved}) skipped={report.Skipped.Count}");
        Console.WriteLine($"[pbi-update-apply] finalStatus: {string.Join(", ", report.FinalStatus.Select(kv => $"{kv.Key}={kv.Value}"))}");
        Console.WriteLine($"[pbi-update-apply] github-sync-Delta: {syncDelta.Count} PBIs -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        foreach (var s in report.Skipped) Console.WriteLine($"[pbi-update-apply]   skip {s}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
