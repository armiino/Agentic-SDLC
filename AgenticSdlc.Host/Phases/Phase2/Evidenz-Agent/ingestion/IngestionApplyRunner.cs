using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Deterministischer Apply: fuehrt die vom Menschen akzeptierten Ingestion-Operationen in den Core aus
// (Upsert-by-Identity, kein LLM). Schreibt den Core NUR ueber den Repository-Port + Audit-Snapshot + Delta
// + affected-view. Analog l4-re-clarify-backlog-apply.
public static class IngestionApplyRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }

        var planDir = IngestionReviewRunner.ResolvePlanDir(repoRoot, args[1]);
        if (planDir is null)
        {
            Console.Error.WriteLine($"[ingest-apply] Ingestion-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }

        var planPath = Path.Combine(planDir, "plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine("[ingest-apply] plan.json fehlt.");
            return 2;
        }
        if (!File.Exists(decisionsPath))
        {
            Console.Error.WriteLine("[ingest-apply] human-decisions.json fehlt - erst ingest-review fahren.");
            return 2;
        }

        var plan = await LoadAsync<StateChangePlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<IngestionHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[ingest-apply] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaFull = Path.IsPathRooted(plan.SourceMeetingDeltaPath) ? plan.SourceMeetingDeltaPath : Path.Combine(repoRoot, plan.SourceMeetingDeltaPath);
        if (!File.Exists(deltaFull))
        {
            Console.Error.WriteLine($"[ingest-apply] MeetingDelta nicht gefunden: {deltaFull}");
            return 2;
        }
        var meetingDelta = (await JsonProjectStateRepository.LoadAsync(deltaFull).ConfigureAwait(false)).Document;

        var accepted = decisions.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.IncomingItemId)
            .ToHashSet(StringComparer.Ordinal);

        // Audit-Snapshot des Core VOR dem Apply (Reproduzierbarkeit).
        var appliedDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);

        var (updatedCore, report, affectedIds) = IngestionApply.Apply(core, meetingDelta, plan, accepted);
        // Inc 1c-2: affected-view = transitiver Blast-Radius ueber den Core-Graphen (nicht mehr nur direkt).
        var affectedView = CoreViews.AffectedItems(updatedCore, affectedIds);

        // Core NUR ueber den Port schreiben (DB-tauschbar).
        await coreRepo.SaveAsync(updatedCore).ConfigureAwait(false);

        await File.WriteAllTextAsync(Path.Combine(appliedDir, "delta.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "affected-view.json"), JsonSerializer.Serialize(affectedView, Json)).ConfigureAwait(false);

        var d = report.Delta;
        Console.WriteLine($"[ingest-apply] accepted={accepted.Count}/{plan.Operations.Count} applied={report.Applied.Count} skipped={report.Skipped.Count}");
        Console.WriteLine($"[ingest-apply] delta: added={d.Added} refined={d.Refined} reaffirmed={d.Reaffirmed} superseded={d.Superseded} contradicted={d.Contradicted} alreadyDecided={d.AlreadyDecided}");
        Console.WriteLine($"[ingest-apply] Core -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))} (items {core.Items.Count}->{updatedCore.Items.Count})");
        Console.WriteLine($"[ingest-apply] -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        foreach (var s in report.Skipped) Console.WriteLine($"[ingest-apply]   skip {s}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: ingest-apply <ingestion-run|dir>");
}
