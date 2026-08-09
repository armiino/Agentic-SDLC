using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// C1a (07.08.2026) — die PIPELINE-Lese-Tools des Stewards (Steward-Vorlauf C1-Move, Kollegen-Schnitt):
/// die NEUE SCHICHT über der abgehakt-stabilen Workflow-Pipeline (Haken-Prinzip). Kein Tool hier schreibt
/// irgendetwas — sie lesen den typisierten A3-Status-Kern (<see cref="PipelineRunStatusReader"/>, dieselbe
/// Quelle wie `pipeline-full status` — EINE Wahrheit, kein Drift). Die CORE-Lese-Tools wohnen seit C3 im
/// geteilten Werkzeugkasten <c>CoreQueryTools</c> (⚖ K5/K7), GitHub-Lesen in <c>GithubSnapshotQueryTools</c>;
/// startende Tools (run/resume) in <c>StewardRunTools</c> als <c>ApprovalRequiredAIFunction</c>.
/// </summary>
public sealed class StewardReadTools(string repoRoot)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetRunStatusAsync, "get_run_status",
            "Liest den typisierten Zustand EINES pipeline-full-Laufs (A3-Vertrag): state (paused|finished|notPausedNotFinished), pausedGate, checkpointId, nextRequiredAction (was der Autor als Naechstes tun kann), Artefakt-Pfade."),
        AIFunctionFactory.Create(ListPausedRunsAsync, "list_paused_runs",
            "Listet ALLE pausierten pipeline-full-Laeufe (wo wartet ein Human-Gate?) mit Gate, Checkpoint und naechster Aktion."),
        AIFunctionFactory.Create(CollectClarifyKatalogAsync, "collect_clarify_katalog",
            "C4-Zielschleife Schritt 1: die offenen Klaerungen (needs_clarify-PBIs) mit LUECKEN-DIAGNOSE "
            + "(STATEMENT_FEHLT/AK_LEER/HISTORIE) + REQ-Kontext — read-only, LLM-frei. Lies dem Autor je PBI "
            + "die Luecke vor und sammle seine Antwort ein."),
    ];

    private async Task<string> CollectClarifyKatalogAsync()
    {
        var repo = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            return JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json);
        return JsonSerializer.Serialize(
            FullWorkflow.PbiUpdate.ClarifySweepCollector.Collect(await repo.LoadAsync().ConfigureAwait(false)), Json);
    }

    private async Task<string> GetRunStatusAsync(string runId)
    {
        var status = await PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        return status is null
            ? JsonSerializer.Serialize(new { error = "RUN_NOT_FOUND", runId }, Json)
            : JsonSerializer.Serialize(status, Json);
    }

    private async Task<string> ListPausedRunsAsync()
    {
        var paused = await PipelineRunStatusReader.ReadPausedAsync(repoRoot).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { pausedCount = paused.Count, runs = paused }, Json);
    }
}
