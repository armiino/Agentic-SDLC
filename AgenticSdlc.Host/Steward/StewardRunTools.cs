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
    Func<string[], Action<string>?, Task<int>>? runner = null,
    Func<string[], Task<int>>? auxRunner = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // Test-Naht: der echte Runner ist der Default; Tests injizieren einen Fake (keine LLM-Läufe im Test).
    private readonly Func<string[], Action<string>?, Task<int>> _runner =
        runner ?? ((args, onRunId) => PipelineFullRunner.RunAsync(args, settings, repoRoot, onRunId));

    // C2c: zweite Naht für die kurzen GitHub-Bahnen (synchron, exitCode zurück; Test-injizierbar).
    private readonly Func<string[], Task<int>> _aux = auxRunner ?? (args => args[0] switch
    {
        "github-snapshot" => FullWorkflow.Tore.Github.GithubIssueSnapshotRunner.RunAsync(args, repoRoot),
        "github-inbound" => FullWorkflow.Tore.Github.Inbound.GithubInboundRunner.RunAsync(args, settings, repoRoot),
        "github-reverse" => FullWorkflow.Tore.Github.GithubReverseRunner.RunAsync(args, repoRoot),
        _ => throw new InvalidOperationException($"Unbekannte Aux-Bahn '{args[0]}'."),
    });

    private readonly ConcurrentDictionary<string, Task<int>> _started = new(StringComparer.Ordinal);
    private volatile string? _activeRunId;

    /// <summary>Für die Hülle/Tests: die im Hintergrund gestarteten Läufe (runId → Task).</summary>
    public IReadOnlyDictionary<string, Task<int>> Started => _started;

    public IReadOnlyList<AITool> Build() =>
    [
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunPipelineFullAsync, "run_pipeline_full",
            "STARTET einen pipeline-full-Lauf (kostenpflichtig, LLM!) — von einem Delta (deltaPath) aus. "
            + "Kehrt SOFORT mit der runId zurueck; Fortschritt danach ueber get_run_status. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunPipelineFromGithubAsync, "run_pipeline_from_github",
            "STARTET einen pipeline-full-Lauf mit der GITHUB-ERNTE als Front (kostenpflichtig, LLM!): Snapshot → "
            + "Detect → InboundAgent-Drafts → Delta → Tore. Kehrt sofort mit runId zurueck. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(ResumeRunAsync, "resume_run",
            "SETZT einen pausierten Lauf fort (runId; acceptAll=true uebernimmt Vorschlaege 1:1 = Experiment-Modus). "
            + "Kehrt sofort zurueck; Fortschritt ueber get_run_status. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(PullGithubSnapshotAsync, "pull_github_snapshot",
            "Zieht einen FRISCHEN GitHub-Issue-Snapshot (externer API-Read; repo = owner/name, z. B. "
            + "armiino/Agentic-GitHub-refactor). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunGithubInboundAsync, "run_github_inbound",
            "Fuehrt die GitHub-ERNTE-ERKENNUNG aus (deterministisch, LLM-frei; draft=true ergaenzt den "
            + "InboundAgent-Draft + Delta = LLM-Kosten). Ergebnis: harvest-report unter runs/github-inbound/. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunGithubReverseAsync, "run_github_reverse",
            "Fuehrt die ZUSTANDS-Reverse-Bahn aus (deterministisch: Issue geschlossen/wieder offen → Vorschlaege; "
            + "danach github-reverse-review als Gate). Braucht Autor-Zustimmung.")),
    ];

    private async Task<string> PullGithubSnapshotAsync(string repo)
        => AuxResult(await _aux(["github-snapshot", "issues", "--repo", repo]).ConfigureAwait(false), "github-snapshot");

    private async Task<string> RunGithubInboundAsync(bool draft = false)
        => AuxResult(await _aux(draft ? ["github-inbound", "--draft"] : ["github-inbound"]).ConfigureAwait(false), "github-inbound");

    private async Task<string> RunGithubReverseAsync()
        => AuxResult(await _aux(["github-reverse"]).ConfigureAwait(false), "github-reverse");

    private static string AuxResult(int exitCode, string bahn)
        => JsonSerializer.Serialize(exitCode == 0
            ? new { ok = true, bahn, hint = "Details stehen im juengsten runs/-Ordner der Bahn." }
            : (object)new { error = "AUX_RUN_FAILED", bahn, exitCode }, Json);

    private Task<string> RunPipelineFromGithubAsync() => StartPipelineAsync(["pipeline-full", "run", "--from-github"], "github-ernte");

    private Task<string> RunPipelineFullAsync(string deltaPath)
        => StartPipelineAsync(["pipeline-full", "run", "--from-delta", deltaPath], deltaPath);

    private async Task<string> StartPipelineAsync(string[] args, string what)
    {
        if (ActiveRun() is { } busy)
            return JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy, hint = "get_run_status abwarten" }, Json);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = Task.Run(() => _runner(args, id => tcs.TrySetResult(id)));
        var completed = await Task.WhenAny(tcs.Task, task).ConfigureAwait(false);
        if (completed == task && !tcs.Task.IsCompleted)   // Runner endete VOR der runId-Meldung = Startfehler (z. B. Datei fehlt)
            return JsonSerializer.Serialize(new { error = "RUN_START_FAILED", exitCode = await task.ConfigureAwait(false), what }, Json);

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
