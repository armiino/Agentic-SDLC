using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// S4/S3-analog: Idempotenz-Marker. Haelt die planId, die zuletzt erfolgreich angewendet wurde. Ein zweiter Apply
// desselben Plans (Doppel-Resume / Re-Run) ist dann ein No-Op — kein zweiter Core-Write (kein NEW_PBI-Duplikat,
// keine Version/History-Drift). Plan-Ebene, weil pbi-update-apply pro Run einen unveraenderlichen Plan hat.
public sealed record PbiUpdateAppliedMarker(
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("appliedUtc")] DateTime AppliedUtc);

// GETEILTE Apply-Ausfuehrung fuer pbi-update (S4, Worklist 20.07): der deterministische Punkt, der die akzeptierten
// PBI-Operationen in den Core schreibt und das github-sync-Delta erzeugt. Genutzt von ZWEI Aufrufern (Paritaet):
//   (1) PbiUpdateApplyRunner    — CLI (Entscheidungen aus human-decisions.json)
//   (2) PbiUpdateApplyExecutor  — MAF-HITL (Entscheidungen aus der RequestPort-Response)
// Core NUR ueber den Repository-Port + Audit-Snapshot (core-before.json).
public static class PbiUpdateApplyExec
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<PbiUpdateApplyReport> ExecuteAsync(
        string planDir, PbiStateChangePlanDocument plan, ISet<int> accepted, string repoRoot, CancellationToken ct = default)
    {
        var appliedDir = Path.Combine(planDir, "applied");
        var markerPath = Path.Combine(appliedDir, "applied.marker");
        var reportPath = Path.Combine(appliedDir, "pbi-update-apply-report.json");

        // S3-analoge Idempotenz: dieser Plan wurde bereits angewendet -> bestehenden Report zurueckgeben, KEIN
        // zweiter Core-Write (verhindert NEW_PBI-Duplikate + Version/History-Drift bei Doppel-Resume/Re-Run).
        if (File.Exists(markerPath) && File.Exists(reportPath))
        {
            var prev = JsonSerializer.Deserialize<PbiUpdateAppliedMarker>(await File.ReadAllTextAsync(markerPath, ct).ConfigureAwait(false), Json);
            if (prev is not null && string.Equals(prev.PlanId, plan.PlanId, StringComparison.Ordinal))
            {
                Console.WriteLine($"[pbi-update-apply] bereits angewendet (idempotent): Plan {plan.PlanId} — kein erneuter Core-Write.");
                return JsonSerializer.Deserialize<PbiUpdateApplyReport>(await File.ReadAllTextAsync(reportPath, ct).ConfigureAwait(false), Json)!;
            }
        }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) throw new InvalidOperationException("Core fehlt.");
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json), ct).ConfigureAwait(false);

        var (updated, report) = PbiUpdateApply.Apply(core, plan, accepted, plan.SourceIngestionRun);
        await coreRepo.SaveAsync(updated).ConfigureAwait(false);

        // github-sync-Delta: nur die betroffenen (neuen/aktualisierten) PBIs.
        var touched = report.NewPbis.Concat(report.UpdatedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(updated).Entries.Where(e => touched.Contains(e.PbiId)).ToList();
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-sync-delta.json"),
            JsonSerializer.Serialize(new { newPbis = report.NewPbis, updatedPbis = report.UpdatedPbis, entries = syncDelta }, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "pbi-update-apply-report.json"), JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        // Idempotenz-Marker ZULETZT (nach erfolgreichem Core-Write) — so bleibt ein mitten abgebrochener Lauf
        // wiederholbar; erst ein vollstaendiger Apply markiert den Plan als erledigt.
        await File.WriteAllTextAsync(markerPath, JsonSerializer.Serialize(new PbiUpdateAppliedMarker(plan.PlanId, DateTime.UtcNow), Json), ct).ConfigureAwait(false);
        return report;
    }

    // akzeptiert = op-<i> mit decision=apply (fehlende Entscheidung -> default apply). Geteilt zwischen CLI + HITL.
    public static HashSet<int> AcceptedFromDecisions(PbiStateChangePlanDocument plan, IReadOnlyList<PbiUpdateDecision> decisions)
    {
        var byOp = decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var accepted = new HashSet<int>();
        for (var i = 0; i < plan.Operations.Count; i++)
            if (!byOp.TryGetValue($"op-{i}", out var d) || string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                accepted.Add(i);
        return accepted;
    }
}
