using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Core;

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
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    // accepted = Menge der IncomingItemIds mit decision=apply. MeetingDelta wird aus plan.SourceMeetingDeltaPath geladen.
    // ingestRunId = der AUSLOESER-Lauf (R-31/I7) — landet als sourceRunId an jeder Inhalts-Mutation.
    // A1a: profile = Aspekt-Naht (Default Requirement — Alt-Aufrufer unverändert; Graph reicht explizit).
    public static async Task<IngestionApplyReport> ExecuteAsync(
        string planDir, StateChangePlanDocument plan, ISet<string> accepted, string repoRoot, string ingestRunId, CancellationToken ct = default, AspectIngestionProfile? profile = null)
    {
        profile ??= AspectIngestionProfile.Requirement;
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

        var (updatedCore, report, affectedIds) = IngestionApply.Apply(core, meetingDelta, plan, accepted, ingestRunId, profile);
        var affectedView = CoreViews.AffectedItems(updatedCore, affectedIds);

        // R-35: menschliche Ablehnungen (mit P2a-Begruendung) als ingest_rejection-Proposals mitheben —
        // Wiedervorlage-Wissen, im SELBEN Save (ein Snapshot, ein Kangal-Pass). Quelle seit 1d/R-57: die EINE
        // TryLoadAny-Naht (Chat- ODER UI-Vertrag — vorher fehlten Chat-Ablehnungen im Projekt-Gedächtnis).
        // Fehlt jede Datei (Experiment-Modi accept-all/replay), wird bewusst nichts aufgezeichnet — fail-open, laut.
        if (Ingestion.IngestGateDecisions.TryLoadAnyFile(planDir) is { } decisionsFile)
        {
            var planRel = Path.GetRelativePath(repoRoot, Path.Combine(planDir, "plan.json"));
            var (withRejections, recorded) = IngestionRejections.Record(updatedCore, plan, decisionsFile.Decisions, planRel);
            updatedCore = withRejections;
            if (recorded.Count > 0)
                Console.WriteLine($"[ingest-apply] R-35: {recorded.Count} Ablehnung(en) als Wiedervorlage-Wissen im Core: {string.Join(", ", recorded)}");
        }
        else
        {
            Console.WriteLine("[ingest-apply] R-35: keine Entscheid-Datei (Chat- oder UI-Vertrag) im Plan-Ordner — Ablehnungen werden nicht aufgezeichnet (Experiment-Modus?).");
        }

        // C2d §3-2 (Kommentar-Gedächtnis): Incoming-Items aus dem Kommentar-Destillat tragen den Anker
        // (githubCommentAnchor). JEDES vom Plan abgedeckte Incoming gilt nach dem Gate als VERARBEITET —
        // Annahme wie Ablehnung stempeln (§11-Prinzip), im SELBEN Save. Kein Anker-Ziel im Core (weder
        // Mapping noch adoptierendes Item) ⇒ LAUT benannt, dokumentierte Grenze (GithubCommentMeta).
        var planIncomingIds = plan.Operations.Select(o => o.IncomingItemId).ToHashSet(StringComparer.Ordinal);
        foreach (var group in meetingDelta.Items
                     .Where(i => planIncomingIds.Contains(i.ItemId)
                                 && i.Metadata.ContainsKey(GithubCommentMeta.AnchorKey)
                                 && GithubOriginMeta.IssueNumberOf(i) is not null)
                     .GroupBy(i => GithubOriginMeta.IssueNumberOf(i)!.Value))
        {
            var anchor = group
                .Select(i => long.TryParse(i.Metadata[GithubCommentMeta.AnchorKey], out var a) ? a : 0L)
                .Max();
            if (anchor <= 0) continue;
            var (stampedCore, stamped) = GithubCommentMeta.Stamp(updatedCore, group.Key, anchor);
            if (stamped) updatedCore = stampedCore;
            else Console.WriteLine($"[ingest-apply] C2d: Kommentar-Anker fuer gh#{group.Key} hat KEIN Core-Ziel "
                + "(weder Mapping noch adoptiertes Item) — Kommentare erscheinen beim naechsten Pull erneut, dann mit R-35-Note.");
        }

        await coreRepo.SaveAsync(updatedCore).ConfigureAwait(false);

        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "affected-view.json"), JsonSerializer.Serialize(affectedView, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(markerPath, JsonSerializer.Serialize(new IngestionAppliedMarker(plan.PlanId, DateTime.UtcNow), Json), ct).ConfigureAwait(false);
        return report;
    }

    // CLI-Konvention: planDir = runs/ingestion/<runId>/plan ODER der Run-Ordner selbst (ResolvePlanDir).
    public static string RunIdFromPlanDir(string planDir)
    {
        var name = Path.GetFileName(Path.TrimEndingDirectorySeparator(planDir));
        return string.Equals(name, "plan", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(planDir)) ?? planDir)
            : name;
    }

    // accepted = IncomingItemIds mit decision=apply (explizit, wie der bisherige ingest-apply).
    public static HashSet<string> AcceptedFromDecisions(IReadOnlyList<IngestionHumanDecision> decisions)
        => decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.IncomingItemId).ToHashSet(StringComparer.Ordinal);
}
