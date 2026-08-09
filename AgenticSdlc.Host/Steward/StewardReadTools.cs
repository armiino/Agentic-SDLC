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
        AIFunctionFactory.Create(ReadRunReportAsync, "read_run_report",
            "3c: liest den LAUF-REPORT (R-40-Vertrag) eines fullworkflow-Laufs — was wurde GELIEFERT "
            + "(applied je Item inkl. neuer entityId, skipped, delta-Zaehler). Nutze ihn nach einem Lauf, "
            + "um dem Autor in EINEM Satz zu berichten, was entstanden ist."),
        AIFunctionFactory.Create(SearchRejectionsAsync, "search_rejections",
            "3c-Vorpruefung: sucht im ABLEHNUNGS-GEDAECHTNIS (R-35, Tor-1-Ablehnungen mit Begruendung) nach "
            + "einem Suchbegriff — VOR dem Einspeisen neuer Themen pruefen: wurde so etwas schon einmal "
            + "bewusst abgelehnt? Treffer dem Autor MIT Begruendung nennen (kein Auto-Skip — er entscheidet)."),
        AIFunctionFactory.Create(CollectArchKatalogAsync, "collect_arch_katalog",
            "ARCH-SWEEP Schritt 1 (9k(b)): die deterministisch messbaren Architektur-Luecken — arbeitOhneRahmen "
            + "(aktive PBIs ohne constrained_by), designOhneAdr, unklassifiziert. Lies je Luecke vor und sammle "
            + "die Architektur-Aussagen des Autors ein (Antwort-Weg = Autor-Front, disposition architecture)."),
        AIFunctionFactory.Create(CollectClarifyKatalogAsync, "collect_clarify_katalog",
            "C4-Zielschleife Schritt 1: die offenen Klaerungen (needs_clarify-PBIs) mit LUECKEN-DIAGNOSE "
            + "(STATEMENT_FEHLT/AK_LEER/HISTORIE) + REQ-Kontext — read-only, LLM-frei. Lies dem Autor je PBI "
            + "die Luecke vor und sammle seine Antwort ein."),
    ];

    private async Task<string> ReadRunReportAsync(string runId)
    {
        // K12-③: Rückblick/Beleg = Chronik-Read (R-40-Vertrag; Geliefertes steht zusätzlich als Wahrheit im
        // Core — sourceRunId-Filter an list_core_items ist die Wahrheits-seitige Antwort).
        var path = Path.Combine(repoRoot, "runs", "fullworkflow", runId, "07-ingest", "applied", "run-report.json");
        if (!File.Exists(path))
            return JsonSerializer.Serialize(new { error = "REPORT_NOT_FOUND", runId, hint = "Lauf noch nicht bis zum Ingest-Apply gekommen? get_run_status pruefen." }, Json);
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
        return JsonSerializer.Serialize(new { runId, report = doc.RootElement.Clone() }, Json);
    }

    private async Task<string> SearchRejectionsAsync(string query)
    {
        var repo = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            return JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json);
        var core = await repo.LoadAsync().ConfigureAwait(false);
        var hits = FullWorkflow.Core.IngestionRejections.Of(core)
            .Where(p => FullWorkflow.QueryText.Contains(p.Metadata.GetValueOrDefault("statement"), query)
                     || FullWorkflow.QueryText.Contains(p.Metadata.GetValueOrDefault("reason"), query))
            .Take(10)
            .Select(p => new
            {
                statement = p.Metadata.GetValueOrDefault("statement"),
                reason = p.Metadata.GetValueOrDefault("reason"),
                date = p.Metadata.GetValueOrDefault("date"),
                sourceRunId = p.SourceRunId,
            }).ToList();
        return JsonSerializer.Serialize(new { query, treffer = hits.Count, hits }, Json);
    }

    private async Task<string> CollectArchKatalogAsync()
    {
        var repo = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            return JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json);
        return JsonSerializer.Serialize(
            FullWorkflow.Core.ArchGapCollector.Collect(await repo.LoadAsync().ConfigureAwait(false)), Json);
    }

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
