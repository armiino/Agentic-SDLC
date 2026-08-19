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
            "3c: liest die APPLY-/ERGEBNIS-REPORTS eines fullworkflow-Laufs — was wurde GELIEFERT, je vorhandener "
            + "Stufe (stages: ingest | pbiUpdate | forward | harvest). Funktioniert fuer JEDEN Lauf-Typ (auch clarify "
            + "ohne Ingest; harvest = Ernte-Funde eines --from-github-Laufs, auch wenn er leer stoppte). "
            + "Nutze ihn nach einem Lauf, um dem Autor in EINEM Satz zu berichten, was entstanden ist."),
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

    // K12-③: Rückblick/Beleg = Chronik-Read. GENERAL (nicht nur Ingest — Fund 11.08.): ein Lauf hat je nach
    // Einstieg unterschiedliche Apply-Stufen (ein clarify-Graph-Lauf z. B. KEINEN Ingest, aber pbi-update-/forward-
    // Apply-Reports). Wir sammeln die VORHANDENEN Apply-Reports aller Stufen — so berichtet der Steward das
    // Gelieferte für JEDEN Lauf-Typ, nicht nur für Ingest-Läufe. Geliefertes steht zusätzlich als Wahrheit im Core.
    private static readonly (string Stage, string Rel)[] ReportCandidates =
    [
        ("ingest",    Path.Combine("07-ingest", "applied", "run-report.json")),          // R-40-Vertrag (inkl. decision-Augment)
        ("pbiUpdate", Path.Combine("07-pbi-update", "applied", "pbi-update-apply-report.json")),
        ("forward",   Path.Combine("07-github", "applied", "github-forward-apply-report.json")),
        // ⚖ 13.08. (ersetzt „Slice 1"): das Ernte-Ergebnis eines --from-github-Laufs LESBAR machen — die Engine
        // schreibt den Report ohnehin; damit beantwortet der Steward „was fand die Ernte?" (auch: nichts → 0 LLM).
        ("harvest",   Path.Combine("00-github-inbound", "harvest-report.json")),
        // Fix A zu R-48 (13.08.): der Forward-PLAN-Ausgang (gatePass/finalDecision) — OHNE ihn war ein Forward,
        // der am eigenen Checker scheiterte (MaxAttemptsReached, kein Apply), im Bericht UNSICHTBAR und der
        // Steward meldete „fertig ✓" (Fund Block F, Lauf 20260813_131946).
        ("forwardPlan", Path.Combine("07-github", "github-forward-summary.json")),
    ];

    private async Task<string> ReadRunReportAsync(string runId)
    {
        var runDir = Path.Combine(repoRoot, "runs", "fullworkflow", runId);
        var stages = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var (stage, rel) in ReportCandidates)
        {
            var p = Path.Combine(runDir, rel);
            if (!File.Exists(p)) continue;
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(p).ConfigureAwait(false));
            stages[stage] = doc.RootElement.Clone();
        }
        if (stages.Count == 0)
            return JsonSerializer.Serialize(new { error = "REPORT_NOT_FOUND", runId, hint = "Lauf noch nicht bis zu einem Apply gekommen? get_run_status pruefen." }, Json);
        return JsonSerializer.Serialize(new { runId, stages }, Json);
    }

    private async Task<string> SearchRejectionsAsync(string query)
    {
        var repo = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            return JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json);
        var core = await repo.LoadAsync().ConfigureAwait(false);
        // R-58: die EINE Stichwort-Suche (Token-ODER + Umlaut-Faltung + Ranking) statt Phrasen-Match —
        // vorher fand „Besuchs-Erinnerung Angehoerige bestaetigt" die passenden REJ-Eintraege NICHT.
        var hits = FullWorkflow.Core.IngestionRejections.Search(core, query)
            .Select(x => new
            {
                rejectionId = x.Proposal.ProposalId,
                statement = x.Proposal.Metadata.GetValueOrDefault("statement"),
                reason = x.Proposal.Metadata.GetValueOrDefault("reason"),
                date = x.Proposal.Metadata.GetValueOrDefault("date"),
                sourceRunId = x.Proposal.SourceRunId,
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
