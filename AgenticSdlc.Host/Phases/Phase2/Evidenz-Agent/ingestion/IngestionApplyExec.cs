using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// GETEILTE Apply-Ausfuehrung fuer ingestion (Tor 1, S4): der deterministische Upsert-by-Identity der akzeptierten
// Operationen in den Core (neue stabile IDs, REFINE-Versionen, CONTRADICT->Open Decision). Genutzt von ZWEI
// Aufrufern (Paritaet): CLI-IngestionApplyRunner + MAF-HITL-IngestionApplyExecutor. Core NUR ueber den Port +
// Audit-Snapshot. Plan-level Idempotenz-Marker (wie pbi-update/decision, S3-analog): ein zweiter Apply desselben
// Plans ist ein No-Op — kein NEW-Duplikat (frische REQ-<n>), keine Versions-Drift.
public sealed record IngestionAppliedMarker(
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("appliedUtc")] DateTime AppliedUtc);

public static class IngestionApplyExec
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    // accepted = Menge der IncomingItemIds mit decision=apply. MeetingDelta wird aus plan.SourceMeetingDeltaPath geladen.
    public static async Task<IngestionApplyReport> ExecuteAsync(
        string planDir, StateChangePlanDocument plan, ISet<string> accepted, string repoRoot, CancellationToken ct = default)
    {
        var appliedDir = Path.Combine(planDir, "applied");
        var markerPath = Path.Combine(appliedDir, "applied.marker");
        var reportPath = Path.Combine(appliedDir, "delta.json");

        // Idempotenz: Plan schon angewendet -> bestehenden Report zurueck, KEIN zweiter Core-Write.
        if (File.Exists(markerPath) && File.Exists(reportPath))
        {
            var prev = JsonSerializer.Deserialize<IngestionAppliedMarker>(await File.ReadAllTextAsync(markerPath, ct).ConfigureAwait(false), Json);
            if (prev is not null && string.Equals(prev.PlanId, plan.PlanId, StringComparison.Ordinal))
            {
                Console.WriteLine($"[ingest-apply] bereits angewendet (idempotent): Plan {plan.PlanId} — kein erneuter Core-Write.");
                return JsonSerializer.Deserialize<IngestionApplyReport>(await File.ReadAllTextAsync(reportPath, ct).ConfigureAwait(false), Json)!;
            }
        }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) throw new InvalidOperationException("Core fehlt.");
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaFull = Path.IsPathRooted(plan.SourceMeetingDeltaPath) ? plan.SourceMeetingDeltaPath : Path.Combine(repoRoot, plan.SourceMeetingDeltaPath);
        if (!File.Exists(deltaFull)) throw new InvalidOperationException($"MeetingDelta nicht gefunden: {deltaFull}");
        var meetingDelta = (await JsonProjectStateRepository.LoadAsync(deltaFull).ConfigureAwait(false)).Document;

        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json), ct).ConfigureAwait(false);

        var (updatedCore, report, affectedIds) = IngestionApply.Apply(core, meetingDelta, plan, accepted);
        var affectedView = CoreViews.AffectedItems(updatedCore, affectedIds);
        await coreRepo.SaveAsync(updatedCore).ConfigureAwait(false);

        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "affected-view.json"), JsonSerializer.Serialize(affectedView, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(markerPath, JsonSerializer.Serialize(new IngestionAppliedMarker(plan.PlanId, DateTime.UtcNow), Json), ct).ConfigureAwait(false);
        return report;
    }

    // accepted = IncomingItemIds mit decision=apply (explizit, wie der bisherige ingest-apply).
    public static HashSet<string> AcceptedFromDecisions(IReadOnlyList<IngestionHumanDecision> decisions)
        => decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.IncomingItemId).ToHashSet(StringComparer.Ordinal);
}
