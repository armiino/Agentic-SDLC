using AgenticSdlc.Host.FullWorkflow.Core;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Deterministischer Apply der akzeptierten PBI-Operationen in den Core (Update-by-Identity, Merge/Praezedenz,
// Requirement-Swap). Core NUR ueber den Port + Audit-Snapshot. Schreibt ein github-sync-Delta (betroffene PBIs).
public static class PbiUpdateApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

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

        // Geteilte Ausfuehrung (identisch zum MAF-HITL-Pfad, S4). Core-Mutation ueber den Port + Audit-Snapshot.
        var accepted = PbiUpdateApplyExec.AcceptedFromDecisions(plan, decisions.Decisions);
        var acceptedAligns = PbiUpdateApplyExec.AcceptedAlignments(plan, decisions.AlignmentDecisions); // R-26-C
        var featureOverrides = PbiUpdateApplyExec.FeatureOverrides(plan, decisions.Decisions);          // B1
        var decisionRequests = PbiUpdateReviewAdapter.DecisionRequestsFrom(plan, decisions.Decisions);  // R-14 D2
        PbiUpdateApplyReport report;
        try
        {
            report = await PbiUpdateApplyExec.ExecuteAsync(planDir, plan, accepted, repoRoot, acceptedAligns, featureOverrides, decisionRequests).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"[pbi-update-apply] {ex.Message}");
            return 2;
        }

        var appliedDir = Path.Combine(planDir, "applied");
        Console.WriteLine($"[pbi-update-apply] accepted={accepted.Count}/{plan.Operations.Count} newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count} relations(+{report.RelationsAdded}/-{report.RelationsRemoved}) skipped={report.Skipped.Count}");
        Console.WriteLine($"[pbi-update-apply] finalStatus: {string.Join(", ", report.FinalStatus.Select(kv => $"{kv.Key}={kv.Value}"))}");
        Console.WriteLine($"[pbi-update-apply] github-sync-Delta -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        foreach (var s in report.Skipped) Console.WriteLine($"[pbi-update-apply]   skip {s}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
