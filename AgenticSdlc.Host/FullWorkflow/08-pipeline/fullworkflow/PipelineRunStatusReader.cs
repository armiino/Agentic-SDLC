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
    NotPausedNotFinished
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
            var p = await HitlShell.LoadAsync<HitlPointer>(pointerPath).ConfigureAwait(false);
            var gate = p.Mode ?? "?";
            var actions = NextRequiredAction(gate, runId)
                .Append($"  Weiter: pipeline-full resume {runId}   (oder … --accept-all)").ToList();
            return new PipelineRunStatus(runId, PipelineRunState.Paused, gate, p.CheckpointId, p.SavedUtc, lastEvent, actions, artifacts);
        }

        var finished = File.Exists(metricsPath);
        return new PipelineRunStatus(runId,
            finished ? PipelineRunState.Finished : PipelineRunState.NotPausedNotFinished,
            PausedGate: null, CheckpointId: null, PausedSinceUtc: null,
            LastPipelineEvent: lastEvent,
            NextRequiredAction: finished ? [] : ["  (kein Pointer, keine metrics — Lauf aktiv oder abgebrochen; events.jsonl prüfen)"],
            artifacts);
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
            [$"  Entscheide: runs/fullworkflow/{runId}/07-github/github-forward-decisions.json (opId/decision apply|skip)",
             "  oder beim Resume: --accept-all | --accept opId1,opId2"],
        _ => ["  Entscheid beim Resume: --accept-all | --accept id1,id2"],
    };
}
