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

        // Geteilte Ausfuehrung (identisch zum MAF-HITL-Pfad, S4) inkl. Idempotenz-Marker.
        var accepted = IngestionApplyExec.AcceptedFromDecisions(decisions.Decisions);
        IngestionApplyReport report;
        try
        {
            report = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, repoRoot).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"[ingest-apply] {ex.Message}");
            return 2;
        }

        var appliedDir = Path.Combine(planDir, "applied");
        var d = report.Delta;
        Console.WriteLine($"[ingest-apply] accepted={accepted.Count}/{plan.Operations.Count} applied={report.Applied.Count} skipped={report.Skipped.Count}");
        Console.WriteLine($"[ingest-apply] delta: added={d.Added} refined={d.Refined} reaffirmed={d.Reaffirmed} superseded={d.Superseded} contradicted={d.Contradicted} alreadyDecided={d.AlreadyDecided}");
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
