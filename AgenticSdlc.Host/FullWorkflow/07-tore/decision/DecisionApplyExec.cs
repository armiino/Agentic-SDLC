using AgenticSdlc.Host.FullWorkflow.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// GETEILTE Apply-Ausfuehrung fuer decision (Tor 2, S4): setzt die freigegebenen Auflösungen in den Core (DEC=resolved,
// Requirement-Swap, PBIs entblockt) und schreibt ein github-sync-Delta. Genutzt von ZWEI Aufrufern (Paritaet):
//   (1) DecisionApplyRunner    — CLI (Entscheidungen aus human-decisions.json)
//   (2) DecisionApplyExecutor  — MAF-HITL (Entscheidungen aus der RequestPort-Response)
// Core NUR ueber den Port + Audit-Snapshot. Plan-level Idempotenz-Marker (wie pbi-update, S3-analog): ein zweiter
// Apply desselben Plans ist ein No-Op (kein REQ-Duplikat bei ADOPT_NEW, keine Version-Drift).
public sealed record DecisionAppliedMarker(
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("appliedUtc")] DateTime AppliedUtc);

public static class DecisionApplyExec
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<DecisionResolutionApplyReport> ExecuteAsync(
        string planDir, DecisionResolutionPlanDocument plan, ISet<int> accepted, string repoRoot, string runId, CancellationToken ct = default)
    {
        var appliedDir = Path.Combine(planDir, "applied");
        var markerPath = Path.Combine(appliedDir, "applied.marker");
        var reportPath = Path.Combine(appliedDir, "decision-apply-report.json");

        // Idempotenz: Plan schon angewendet -> bestehenden Report zurueck, KEIN zweiter Core-Write.
        if (File.Exists(markerPath) && File.Exists(reportPath))
        {
            var prev = JsonSerializer.Deserialize<DecisionAppliedMarker>(await File.ReadAllTextAsync(markerPath, ct).ConfigureAwait(false), Json);
            if (prev is not null && string.Equals(prev.PlanId, plan.PlanId, StringComparison.Ordinal))
            {
                Console.WriteLine($"[decision-apply] bereits angewendet (idempotent): Plan {plan.PlanId} — kein erneuter Core-Write.");
                return JsonSerializer.Deserialize<DecisionResolutionApplyReport>(await File.ReadAllTextAsync(reportPath, ct).ConfigureAwait(false), Json)!;
            }
        }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) throw new InvalidOperationException("Core fehlt.");
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json), ct).ConfigureAwait(false);

        var (updated, report) = DecisionResolutionApply.Apply(core, plan, accepted, runId);
        await coreRepo.SaveAsync(updated).ConfigureAwait(false);

        var touched = report.UnblockedPbis.Concat(report.SwappedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(updated).Entries.Where(e => touched.Contains(e.PbiId)).ToList();
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-sync-delta.json"),
            JsonSerializer.Serialize(new { newPbis = Array.Empty<string>(), updatedPbis = touched.OrderBy(x => x, StringComparer.Ordinal).ToArray(), entries = syncDelta }, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(markerPath, JsonSerializer.Serialize(new DecisionAppliedMarker(plan.PlanId, DateTime.UtcNow), Json), ct).ConfigureAwait(false);
        return report;
    }

    // akzeptiert = op-<i> mit decision=apply (EXPLIZIT, wie der bisherige decision-apply — kein Default-apply).
    public static HashSet<int> AcceptedFromDecisions(DecisionResolutionPlanDocument plan, IReadOnlyList<DecisionReviewDecision> decisions)
    {
        var byOp = decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var accepted = new HashSet<int>();
        for (var i = 0; i < plan.Operations.Count; i++)
            if (byOp.TryGetValue($"op-{i}", out var d) && string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                accepted.Add(i);
        return accepted;
    }
}
