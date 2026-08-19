using AgenticSdlc.Host.Run;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>Der A3-VERTRAG: die EINE typisierte Antwort auf „wie steht ein pipeline-full-Lauf?" —
/// abgeleitet AUSSCHLIESSLICH aus den bestehenden Run-Artefakten (pointer.json · events.jsonl ·
/// metrics.json). Konsumenten: das CLI `pipeline-full status` (Renderer) und die Steward-Tools
/// (`get_run_status`, C1) — EINE Quelle, kein Drift.</summary>
public sealed record PipelineRunStatus(
    string RunId,
    PipelineRunState State,
    string? PausedGate,
    string? CheckpointId,
    DateTime? PausedSinceUtc,
    string? LastPipelineEvent,
    IReadOnlyList<string> NextRequiredAction,
    PipelineRunArtifacts Artifacts);

// A3-Vertrag: als STRING serialisieren (LLM-/Menschen-lesbar, camelCase).
[JsonConverter(typeof(JsonStringEnumConverter<PipelineRunState>))]
public enum PipelineRunState
{
    /// <summary>pointer.json existiert — der Lauf wartet an einem Human-Gate.</summary>
    Paused,
    /// <summary>metrics.json existiert und kein Pointer — der Lauf ist durchgelaufen.</summary>
    Finished,
    /// <summary>weder Pointer noch metrics — läuft gerade ODER wurde abgebrochen (die Artefakte
    /// können das nicht unterscheiden; die Events sagen, wie weit er kam).</summary>
    NotPausedNotFinished,
    /// <summary>R-49: das letzte Pipeline-Event ist PIPELINE_ABORTED — der Lauf hat sich selbst LAUT
    /// beendet (z. B. Pull-Fehler VOR dem Graphen). Kein Raten mehr zwischen „aktiv" und „tot".</summary>
    Aborted
}

/// <summary>Die Artefakt-Pfade des Laufs (relativ zum Repo) — der Steward verlinkt/liest sie, statt raten.</summary>
public sealed record PipelineRunArtifacts(string RunDir, string? PointerPath, string? EventsPath, string? MetricsPath);

/// <summary>
/// A3 (07.08.2026) — der geteilte Status-Kern (Steward-Vorlauf A3, k-entscheidungen K2): NICHT additiv —
/// `pipeline-full status` und die Pause-Meldungen des Runners rendern DIESEN Kern; die Steward-Tools
/// bekommen dieselbe Wahrheit typisiert. `NextRequiredAction` = die ReviewHints (eine Gate→Anleitung-Quelle).
/// </summary>
public static class PipelineRunStatusReader
{
    public static async Task<PipelineRunStatus?> ReadAsync(string repoRoot, string runIdOrDir)
    {
        var dir = Path.IsPathRooted(runIdOrDir) ? runIdOrDir : Path.Combine(repoRoot, "runs", "fullworkflow", runIdOrDir);
        if (!Directory.Exists(dir)) return null;
        var runId = Path.GetFileName(dir.TrimEnd('/', '\\'));

        var pointerPath = Path.Combine(dir, "checkpoints", "pointer.json");
        var eventsPath = Path.Combine(dir, "logs", "events.jsonl");
        var metricsPath = Path.Combine(dir, "logs", "metrics.json");
        var artifacts = new PipelineRunArtifacts(
            Path.GetRelativePath(repoRoot, dir),
            File.Exists(pointerPath) ? Path.GetRelativePath(repoRoot, pointerPath) : null,
            File.Exists(eventsPath) ? Path.GetRelativePath(repoRoot, eventsPath) : null,
            File.Exists(metricsPath) ? Path.GetRelativePath(repoRoot, metricsPath) : null);

        var lastEvent = File.Exists(eventsPath)
            ? File.ReadLines(eventsPath).LastOrDefault(l => l.Contains("\"PIPELINE_", StringComparison.Ordinal))
            : null;

        if (File.Exists(pointerPath))
        {
            // Politur 1b′ (Stale-Pointer — 3 Live-Vorfälle Session 2, Spitze: durchlaufenes Gate erneut
            // vorgelegt): liegt in events.jsonl NACH der letzten Pause ein RESUME ohne NEUERE Pause, ist
            // der Zeiger VERALTET — der Lauf arbeitet gerade (oder starb laut, R-51b). Die alte Pause wird
            // dann NIE als Lage verkauft; Datei-Reihenfolge = Append-Wahrheit, kein Timestamp-Raten.
            if (File.Exists(eventsPath) && PointerIsStale(eventsPath))
                return new PipelineRunStatus(runId, PipelineRunState.NotPausedNotFinished,
                    PausedGate: null, CheckpointId: null, PausedSinceUtc: null, lastEvent,
                    ["  Lauf ARBEITET GERADE (Resume nach der letzten Pause — Zeiger veraltet). Gleich erneut prüfen; events.jsonl ist die Wahrheit."],
                    artifacts);

            var p = await HitlShell.LoadAsync<HitlPointer>(pointerPath).ConfigureAwait(false);
            var gate = p.Mode ?? "?";
            var actions = new List<string>();
            if (RouteLine(gate) is { } route) actions.Add(route);   // 1b-Rest: „Checkpoint n von m" (Ortsgefühl)
            actions.AddRange(NextRequiredAction(gate, runId));
            actions.Add($"  Weiter: pipeline-full resume {runId}   (oder … --accept-all)");
            return new PipelineRunStatus(runId, PipelineRunState.Paused, gate, p.CheckpointId, p.SavedUtc, lastEvent, actions, artifacts);
        }

        // R-49: ein PIPELINE_ABORTED als letztes Event = der Lauf hat sich LAUT selbst beendet (Frühausstieg,
        // z. B. Pull-404 vor dem Graphen). Vorher war das von „läuft noch" nicht unterscheidbar — der Steward
        // vertröstete den Autor über einen toten Task.
        if (lastEvent is not null && lastEvent.Contains("\"PIPELINE_ABORTED\"", StringComparison.Ordinal))
        {
            string reason = "(Grund siehe events.jsonl)";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(lastEvent);
                if (doc.RootElement.TryGetProperty("reason", out var r) && r.GetString() is { } rs) reason = rs;
            }
            catch { /* Event unlesbar → generischer Hinweis reicht */ }
            return new PipelineRunStatus(runId, PipelineRunState.Aborted,
                PausedGate: null, CheckpointId: null, PausedSinceUtc: null,
                LastPipelineEvent: lastEvent,
                NextRequiredAction: [$"  ABGEBROCHEN: {reason}", "  Ursache beheben, dann den Lauf NEU starten (kein resume — es gibt keinen Checkpoint)."],
                artifacts);
        }

        var finished = File.Exists(metricsPath);
        return new PipelineRunStatus(runId,
            finished ? PipelineRunState.Finished : PipelineRunState.NotPausedNotFinished,
            PausedGate: null, CheckpointId: null, PausedSinceUtc: null,
            LastPipelineEvent: lastEvent,
            NextRequiredAction: finished ? [] : ["  (kein Pointer, keine metrics — Lauf aktiv oder abgebrochen; events.jsonl prüfen)"],
            artifacts);
    }

    /// <summary>1b-Rest (Fortschritts-Vertrag, UX-TODO „Ortsgefühl"): die kanonische Strecken-Ordnung je
    /// Zweig — Checkpoints können per R-50 leer übersprungen werden, daher ehrlich „von max. m".</summary>
    internal static readonly string[] OperationalRoute =
        ["adjudication-gate", "ingest-gate", "arch-ingest-gate", "arch-classify-gate", "adr-gate",
         "decision-gate", "pbi-gate", "github-forward-gate"];
    internal static readonly string[] BootstrapRoute =
        ["adjudication-gate", "cluster-review-gate", "backlog-review-gate", "arch-classify-gate",
         "adr-gate", "github-forward-gate"];

    internal static string? RouteLine(string gate)
    {
        var bootstrap = gate is "cluster-review-gate" or "backlog-review-gate";
        var route = bootstrap ? BootstrapRoute : OperationalRoute;
        var i = Array.IndexOf(route, gate);
        if (i < 0) return null;
        var danach = route.Skip(i + 1).ToList();
        return $"  » Checkpoint {i + 1} von max. {route.Length} ({(bootstrap ? "Bootstrap" : "Betrieb")}): '{gate}'"
             + (danach.Count == 0 ? " · letzter Halt" : $" · danach: {string.Join(" → ", danach)}");
    }

    /// <summary>1b′: Zeiger veraltet? = das LETZTE Pause/Resume-Event in Datei-Reihenfolge ist ein RESUME.</summary>
    internal static bool PointerIsStale(string eventsPath)
    {
        var lastIsResume = false;
        foreach (var l in File.ReadLines(eventsPath))
        {
            if (l.Contains("\"PIPELINE_PAUSED\"", StringComparison.Ordinal)
                || l.Contains("\"PIPELINE_STILL_PAUSED\"", StringComparison.Ordinal)) lastIsResume = false;
            else if (l.Contains("\"PIPELINE_RESUME\"", StringComparison.Ordinal)) lastIsResume = true;
        }
        return lastIsResume;
    }

    /// <summary>Alle pausierten Läufe (die „wo hakt es?"-Sicht des Stewards und des status-Kommandos).</summary>
    public static async Task<IReadOnlyList<PipelineRunStatus>> ReadPausedAsync(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "runs", "fullworkflow");
        if (!Directory.Exists(root)) return [];
        var result = new List<PipelineRunStatus>();
        foreach (var dir in Directory.GetDirectories(root).OrderBy(d => d, StringComparer.Ordinal))
        {
            var s = await ReadAsync(repoRoot, dir).ConfigureAwait(false);
            if (s is { State: PipelineRunState.Paused }) result.Add(s);
        }
        return result;
    }

    /// <summary>Die EINE Gate→„was jetzt?"-Quelle (aus dem Runner hierher gehoben — Pause-Meldung,
    /// status-CLI und Steward sprechen damit identisch).</summary>
    public static IReadOnlyList<string> NextRequiredAction(string gate, string runId) => gate switch
    {
        "adjudication-gate" =>
            [$"  Review-UI: ledger-adjudicate-ui runs/fullworkflow/{runId}/01-ledger/queue.json  (Aktionen setzen, Autosave)",
             "  oder beim Resume: --accept-all (System-Vorschlag-Heuristik, EXPERIMENT)"],
        "cluster-review-gate" =>
            [$"  Review-UI: l4-re-clarify-review runs/fullworkflow/{runId}/06-backlog/clusters  → human-decisions.json"],
        "backlog-review-gate" =>
            [$"  Review-UI: l4-re-clarify-backlog-review runs/fullworkflow/{runId}/06-backlog/backlog  → human-decisions.json"],
        "decision-gate" =>
            [$"  Review-UI: decision-gate-review runs/fullworkflow/{runId}/07-decision  → decision-gate-decisions.json (Ausgang je Widerspruch, vertagen erlaubt)",
             "  Flags/accept-all koennen hier NICHT aufloesen (Wahrheits-Konflikt) — sie vertagen alles (laut)."],
        "adr-gate" =>
            [$"  Review-UI: adr-review {runId}  (Entwuerfe editieren/freigeben → adr-decisions.json, vertagen erlaubt)",
             "  oder beim Resume: --accept-all (Agent-Entwuerfe 1:1 uebernehmen)"],
        "arch-classify-gate" =>
            [$"  Review-UI: arch-classify-review {runId}  (drei Rollen je Item bestätigen/korrigieren → classify-decisions.json)",
             "  oder beim Resume: --accept-all (Agent-Vorschlag 1:1 uebernehmen)"],
        "arch-ingest-gate" =>
            [$"  arch-Plan: runs/fullworkflow/{runId}/07-arch-ingest/plan.json (Operationen je Architektur-Aussage)",
             "  Entscheid beim Resume: --accept-all | --accept incomingId1,incomingId2"],
        "github-forward-gate" =>
            [$"  Review-UI: github-forward-review runs/fullworkflow/{runId}/07-github  (→ human-decisions.json)",
             $"  oder Steward decide_gate (→ github-forward-decisions.json) — beide liest der resume",
             "  oder beim Resume: --accept-all | --accept opId1,opId2"],
        _ => ["  Entscheid beim Resume: --accept-all | --accept id1,id2"],
    };
}
