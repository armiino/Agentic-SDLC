using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// C1c (07.08.2026) — die STARTENDEN Tools des Stewards (Vorlauf-Move C1c): `run_pipeline_full` und
/// `resume_run`. Beide sind in <see cref="ApprovalRequiredAIFunction"/> gewrappt (K3: Kosten/Läufe nur mit
/// expliziter Autor-Zustimmung — der MAF-native Approval-Loop, I-1-bewiesen) und folgen K2: in-process
/// **start-async** — das Tool kehrt SOFORT mit der runId zurück (runId-Naht am Runner), der Lauf arbeitet
/// im Hintergrund, `get_run_status` (C1a) liest den Fortschritt aus den Artefakten. Rückgaben = A3-Sprache.
/// Nebenläufigkeit (K2-Entscheid): EIN aktiver Lauf je Steward-Sitzung — ein zweiter Start wird LAUT mit
/// STEWARD_BUSY abgelehnt (kein stilles Parallel-Feuern von LLM-Kosten).
/// </summary>
public sealed class StewardRunTools(string repoRoot, HostSettings settings,
    Func<string[], Action<string>?, Task<int>>? runner = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // Test-Naht: der echte Runner ist der Default; Tests injizieren einen Fake (keine LLM-Läufe im Test).
    private readonly Func<string[], Action<string>?, Task<int>> _runner =
        runner ?? ((args, onRunId) => PipelineFullRunner.RunAsync(args, settings, repoRoot, onRunId));

    private readonly ConcurrentDictionary<string, Task<int>> _started = new(StringComparer.Ordinal);
    private volatile string? _activeRunId;

    /// <summary>Für die Hülle/Tests: die im Hintergrund gestarteten Läufe (runId → Task).</summary>
    public IReadOnlyDictionary<string, Task<int>> Started => _started;

    public IReadOnlyList<AITool> Build() =>
    [
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunPipelineFullAsync, "run_pipeline_full",
            "STARTET einen pipeline-full-Lauf (kostenpflichtig, LLM!) — von einem Delta (deltaPath) aus. "
            + "Kehrt SOFORT mit der runId zurueck; Fortschritt danach ueber get_run_status. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(ResumeRunAsync, "resume_run",
            "SETZT einen pausierten Lauf fort (runId; acceptAll=true uebernimmt Vorschlaege 1:1 = Experiment-Modus). "
            + "Kehrt sofort zurueck; Fortschritt ueber get_run_status. Braucht Autor-Zustimmung.")),
    ];

    private async Task<string> RunPipelineFullAsync(string deltaPath)
    {
        if (ActiveRun() is { } busy)
            return JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy, hint = "get_run_status abwarten" }, Json);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = Task.Run(() => _runner(["pipeline-full", "run", "--from-delta", deltaPath], id => tcs.TrySetResult(id)));
        var completed = await Task.WhenAny(tcs.Task, task).ConfigureAwait(false);
        if (completed == task && !tcs.Task.IsCompleted)   // Runner endete VOR der runId-Meldung = Startfehler (z. B. Datei fehlt)
            return JsonSerializer.Serialize(new { error = "RUN_START_FAILED", exitCode = await task.ConfigureAwait(false), deltaPath }, Json);

        var runId = await tcs.Task.ConfigureAwait(false);
        _started[runId] = task; _activeRunId = runId;
        return JsonSerializer.Serialize(new { started = true, runId, hint = $"get_run_status(\"{runId}\") fuer Fortschritt/Pause" }, Json);
    }

    private Task<string> ResumeRunAsync(string runId, bool acceptAll = false)
    {
        if (ActiveRun() is { } busy && busy != runId)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy }, Json));

        string[] args = acceptAll ? ["pipeline-full", "resume", runId, "--accept-all"] : ["pipeline-full", "resume", runId];
        var task = Task.Run(() => _runner(args, null));
        _started[runId] = task; _activeRunId = runId;
        return Task.FromResult(JsonSerializer.Serialize(new { resuming = true, runId, acceptAll,
            hint = $"get_run_status(\"{runId}\") fuer Fortschritt/naechstes Gate" }, Json));
    }

    private string? ActiveRun()
    {
        var id = _activeRunId;
        if (id is null || !_started.TryGetValue(id, out var t) || t.IsCompleted) { _activeRunId = null; return null; }
        return id;
    }
}
