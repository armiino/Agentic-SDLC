using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Deterministischer Apply: fuehrt die vom Menschen akzeptierten Ingestion-Operationen in den Core aus
// (Upsert-by-Identity, kein LLM). Schreibt den Core NUR ueber den Repository-Port + Audit-Snapshot + Delta
// + affected-view. Analog l4-re-clarify-backlog-apply.
public static class IngestionApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

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
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine("[ingest-apply] plan.json fehlt.");
            return 2;
        }
        // 1d (18.08., N3-Fund): die Werkbank liest ueber DIESELBE Naht wie Responder+Rekorder —
        // Chat-entschiedene Laeufe (ingest-gate-decisions.json) sind gleichberechtigt.
        var decisions = Ingestion.IngestGateDecisions.TryLoadAnyFile(planDir);
        if (decisions is null)
        {
            Console.Error.WriteLine("[ingest-apply] keine Entscheid-Datei (weder ingest-gate-decisions.json [Chat] noch human-decisions.json [UI]) — erst entscheiden (ingest-review ODER Chat-Gate).");
            return 2;
        }

        var plan = await LoadAsync<StateChangePlanDocument>(planPath).ConfigureAwait(false);

        // Geteilte Ausfuehrung (identisch zum MAF-HITL-Pfad, S4) inkl. Idempotenz-Marker.
        var accepted = IngestionApplyExec.AcceptedFromDecisions(decisions.Decisions);
        IngestionApplyReport report;
        try
        {
            report = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, repoRoot,
                IngestionApplyExec.RunIdFromPlanDir(planDir)).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"[ingest-apply] {ex.Message}");
            return 2;
        }

        var appliedDir = Path.Combine(planDir, "applied");
        var d = report.Delta;
        Console.WriteLine($"[ingest-apply] accepted={accepted.Count}/{plan.Operations.Count} applied={report.Applied.Count} skipped={report.Skipped.Count}");
        Console.WriteLine($"[ingest-apply] delta: added={d.Added} refined={d.Refined} reaffirmed={d.Reaffirmed} superseded={d.Superseded} contradicted={d.Contradicted} alreadyDecided={d.AlreadyDecided} questions={d.Questions}");
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
