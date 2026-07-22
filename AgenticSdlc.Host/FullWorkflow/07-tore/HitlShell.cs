using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent;

// R2 (2026-07-22): geteilte HITL-Shell der *-hitl-Runner (vorher 4x nahezu identisch dupliziert).
// Kapselt NUR die Mechanik: Checkpoint-Store, Start-Schleife (bis Human-Gate + pointer.json), Resume-Schleife
// (Restore + Antwort senden + Report). Fachlogik bleibt pro Stufe: Workflow-Bau, Request/Response-Typen,
// Accept-Semantik, UI-Collect, Report-Ausgaben.

// Ein pointer.json fuer alle Stufen; optionale Felder (Mode: decision, Repository/TokenEnv: github-forward)
// werden bei null NICHT geschrieben -> Dateiformat je Stufe unveraendert, alte pointer.json bleiben lesbar.
public sealed record HitlPointer(
    string RunId,
    string SessionId,
    string CheckpointId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Mode,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Repository,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? TokenEnv,
    DateTime SavedUtc);

public static class HitlShell
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    // pointer.json laden; Fehlermeldung + null, wenn kein pausierter HITL-Lauf existiert.
    public static async Task<HitlPointer?> LoadPointerAsync(string cmd, string checkpointDir)
    {
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (!File.Exists(pointerPath))
        {
            Console.Error.WriteLine($"[{cmd}] pointer.json fehlt unter {checkpointDir} - kein pausierter HITL-Lauf.");
            return null;
        }
        return await LoadAsync<HitlPointer>(pointerPath).ConfigureAwait(false);
    }

    // START (Prozess A): Workflow bis zum Human-Gate fahren, Checkpoint sichern, pausieren.
    // onManualOutput: WorkflowOutputEvent.Data -> Exit-Code (Stufe druckt ihre Gate-Fail-Meldung selbst) oder null.
    // pausedLines: stufen-spezifische Zeilen NACH der PAUSIERT-Meldung (Plan-Pfad, Fortsetzen-Hinweis).
    public static async Task<int> StartAsync<TInput>(
        string cmd, Workflow workflow, TInput input, RunContext run, string checkpointDir,
        Func<object?, int?> onManualOutput, IReadOnlyList<string> pausedLines,
        string? pointerMode = null, string? pointerRepository = null, string? pointerTokenEnv = null)
        where TInput : notnull
    {
        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        CheckpointInfo? pending = null;
        try
        {
            await using var runHandle = await InProcessExecution.RunStreamingAsync(workflow, input, manager, run.RunId).ConfigureAwait(false);
            await foreach (var evt in runHandle.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is RequestInfoEvent) Console.WriteLine($"[{cmd}] Human-Gate erreicht (Plan wartet auf Freigabe).");
                if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
                {
                    if (info.Checkpoint is { } cp) pending = cp;
                    if (info.HasPendingRequests && pending is not null) break;
                }
                if (evt is WorkflowOutputEvent outEvt && onManualOutput(outEvt.Data) is { } exit)
                    return exit;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{cmd}] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        if (pending is null)
        {
            Console.Error.WriteLine($"[{cmd}] kein Checkpoint mit offenem Human-Gate erzeugt.");
            return 4;
        }
        await File.WriteAllTextAsync(Path.Combine(checkpointDir, "pointer.json"), JsonSerializer.Serialize(
            new HitlPointer(run.RunId, pending.SessionId, pending.CheckpointId, pointerMode, pointerRepository, pointerTokenEnv, DateTime.UtcNow), Json)).ConfigureAwait(false);
        Console.WriteLine($"[{cmd}] PAUSIERT am Human-Gate. checkpointId={pending.CheckpointId}");
        foreach (var line in pausedLines) Console.WriteLine(line);
        return 0;
    }

    // RESUME (Prozess B): Checkpoint restaurieren, am Human-Gate die Antwort der Stufe senden, auf den Report warten.
    // makeResponse: Response-Payload der Stufe (CLI-Entscheidung oder UI-Collect). onOutput: Report -> Exit-Code oder null.
    public static async Task<int> ResumeAsync<TResponse>(
        string cmd, Workflow workflow, string runId, string checkpointDir, HitlPointer pointer,
        bool uiMode, Func<Task<TResponse>> makeResponse, Func<object?, int?> onOutput)
        where TResponse : notnull
    {
        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        var checkpoint = new CheckpointInfo(pointer.SessionId, pointer.CheckpointId);

        await using var runHandle = await InProcessExecution.OpenStreamingAsync(workflow, manager, runId).ConfigureAwait(false);
        await runHandle.RestoreCheckpointAsync(checkpoint).ConfigureAwait(false);

        // UI-Modus: der Mensch kann lange brauchen -> kein enger Timeout. CLI-Modus: 2 min (automatisiert).
        using var cts = new CancellationTokenSource(uiMode ? Timeout.InfiniteTimeSpan : TimeSpan.FromMinutes(2));
        await foreach (var evt in runHandle.WatchStreamAsync(cts.Token).ConfigureAwait(false))
        {
            if (evt is RequestInfoEvent req)
            {
                var payload = await makeResponse().ConfigureAwait(false);
                await runHandle.SendResponseAsync(req.Request.CreateResponse(payload)).ConfigureAwait(false);
            }
            else if (evt is WorkflowOutputEvent outEvt && onOutput(outEvt.Data) is { } exit)
                return exit;
        }
        Console.Error.WriteLine($"[{cmd}] Resume beendet ohne Apply-Report (Timeout/kein Request?).");
        return 4;
    }
}
