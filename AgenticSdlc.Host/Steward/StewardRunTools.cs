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
    Func<string, Task<int>>? pullSnapshot = null,
    Func<bool, Task<int>>? runInbound = null,
    Func<Task<int>>? runReverse = null,
    Func<string, Task<int>>? runSweep = null,
    Func<string, Task<int>>? openReview = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // Test-Naht: der echte Runner ist der Default; Tests injizieren einen Fake (keine LLM-Läufe im Test).
    private readonly Func<string[], Action<string>?, Task<int>> _runner =
        runner ?? ((args, onRunId) => PipelineFullRunner.RunAsync(args, settings, repoRoot, onRunId));

    // K13-2 (09.08.): TYPISIERTE Nähte statt CLI-String-Args (K13: CLI ist Eingang, nie Integrationsschicht).
    // Je Bahn ein Delegate auf den gehobenen Kern — Test-injizierbar, kompilierfest.
    private readonly Func<string, Task<int>> _pullSnapshot =
        pullSnapshot ?? (repo => FullWorkflow.Tore.Github.GithubIssueSnapshotRunner.PullIssuesAsync(repo, repoRoot));
    private readonly Func<bool, Task<int>> _runInbound =
        runInbound ?? (draft => FullWorkflow.Tore.Github.Inbound.GithubInboundRunner.RunHarvestAsync(settings, repoRoot, null, draft));
    private readonly Func<Task<int>> _runReverse =
        runReverse ?? (() => FullWorkflow.Tore.Github.GithubReverseRunner.RunReverseAsync(repoRoot));
    private readonly Func<string, Task<int>> _runSweep =
        runSweep ?? (answersPath => FullWorkflow.PbiUpdate.ClarifySweepAnswersRunner.RunFromAnswersAsync(answersPath, settings, repoRoot));
    private readonly Func<string, Task<int>> _openReview =
        openReview ?? (proposalId => FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.RunPendingAsync(proposalId, settings, repoRoot));

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
        AIFunctionFactory.Create(SaveAuthorStatements, "save_author_statements",
            "3c AUTOR-FRONT Schritt 1: schreibt die vom Autor DIKTIERTEN Wahrheits-Kandidaten (statements: "
            + "[{text, disposition requirement|architecture, rationale?}]) WOERTLICH als Delta im Meeting-"
            + "Ketten-Vertrag (kein Wahrheits-Write — die Kette prägt erst nach den Gates). Gibt deltaPath "
            + "fuer run_pipeline_full zurueck."),
        AIFunctionFactory.Create(SaveSweepAnswers, "save_sweep_answers",
            "C4-Zielschleife Schritt 2: schreibt die vom Autor DIKTIERTEN Klaerungs-Antworten als "
            + "sweep-answers.json (kein Wahrheits-Write; Quelle = author via steward-chat). "
            + "answers: [{pbiId, antwort}]. Gibt den Pfad fuer run_clarify_sweep zurueck."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunClarifySweepAsync, "run_clarify_sweep",
            "C4-Zielschleife Schritt 3 (LLM-Kosten): faehrt den Klaerungs-Sweep mit der Antworten-Datei — "
            + "Angleichungs-Vorschlaege entstehen, danach entscheidet der Autor am pbi-update-review-Gate. "
            + "Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(OpenGateUiAsync, "open_gate_ui",
            "3b: oeffnet auf Autor-Ja die REVIEW-UI zum PAUSIERTEN Graph-Gate eines Laufs (decision-gate | "
            + "pbi-gate | ingest-/arch-ingest-gate). 'Fertig' in der UI kettet automatisch den resume "
            + "(Folgestufen inkl. LLM). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(OpenReviewUiAsync, "open_review_ui",
            "Oeffnet auf Autor-Ja das REVIEW zu einem wartenden Registry-Eintrag (proposalId aus "
            + "get_core_overview.openDecisions.pendingReviews) — der Autor entscheidet am Gate, nie du. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunGithubReverseAsync, "run_github_reverse",
            "Fuehrt die ZUSTANDS-Reverse-Bahn aus (deterministisch: Issue geschlossen/wieder offen → Vorschlaege; "
            + "danach github-reverse-review als Gate). Braucht Autor-Zustimmung.")),
    ];

    private async Task<string> PullGithubSnapshotAsync(string repo)
        => AuxResult(await _pullSnapshot(repo).ConfigureAwait(false), "github-snapshot");

    private async Task<string> RunGithubInboundAsync(bool draft = false)
        => AuxResult(await _runInbound(draft).ConfigureAwait(false), "github-inbound");

    private async Task<string> RunGithubReverseAsync()
        => AuxResult(await _runReverse().ConfigureAwait(false), "github-reverse");

    private static string AuxResult(int exitCode, string bahn)
        => JsonSerializer.Serialize(exitCode == 0
            ? new { ok = true, bahn, hint = "Details stehen im juengsten runs/-Ordner der Bahn." }
            : (object)new { error = "AUX_RUN_FAILED", bahn, exitCode }, Json);

    private Task<string> RunPipelineFromGithubAsync() => StartPipelineAsync(["pipeline-full", "run", "--from-github"], "github-ernte");

    private string SaveAuthorStatements(IReadOnlyList<FullWorkflow.Delta.AuthorStatement> statements, string? sessionName = null)
    {
        var session = sessionName ?? "steward";
        var (delta, errors) = FullWorkflow.Delta.AuthorFrontDeltaBuilder.Build(statements, session);
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "STATEMENTS_INVALID", details = errors }, Json);
        var dir = Path.Combine(repoRoot, "state", "steward", "author-front");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}-delta.json");
        File.WriteAllText(path, JsonSerializer.Serialize(delta, FullWorkflow.Delta.ProjectStateJson.Options));
        return JsonSerializer.Serialize(new { saved = true, items = delta!.Items.Count,
            deltaPath = Path.GetRelativePath(repoRoot, path),
            hint = "naechster Schritt (zustimmungspflichtig): run_pipeline_full(deltaPath) — Tor 1 pausiert dann im Chat." }, Json);
    }

    private string SaveSweepAnswers(IReadOnlyList<FullWorkflow.PbiUpdate.ClarifySweepAnswer> answers, string? sessionName = null)
    {
        if (answers.Count == 0 || answers.Any(a => string.IsNullOrWhiteSpace(a.PbiId) || string.IsNullOrWhiteSpace(a.Antwort)))
            return JsonSerializer.Serialize(new { error = "ANSWERS_INVALID", hint = "jede Antwort braucht pbiId + antwort" }, Json);
        var stamped = answers.Select(a => a with { Quelle = "author via steward-chat", SessionName = a.SessionName ?? sessionName ?? "steward" }).ToList();
        var dir = Path.Combine(repoRoot, "state", "steward", "sweep-answers");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(stamped, Json));
        return JsonSerializer.Serialize(new { saved = true, answers = stamped.Count, answersPath = Path.GetRelativePath(repoRoot, path) }, Json);
    }

    private async Task<string> OpenGateUiAsync(string runId)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        return status?.PausedGate switch
        {
            "decision-gate" => AuxResult(await FullWorkflow.Decision.DecisionGateReviewRunner.RunForRunAsync(runId, settings, repoRoot).ConfigureAwait(false), "decision-gate-review"),
            "pbi-gate" => AuxResult(await FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.RunForPipelineRunAsync(runId, settings, repoRoot).ConfigureAwait(false), "pbi-update-review"),
            "ingest-gate" => AuxResult(await FullWorkflow.Core.IngestionReviewRunner.RunForPipelineRunAsync(runId, "07-ingest", settings, repoRoot).ConfigureAwait(false), "ingest-review"),
            "arch-ingest-gate" => AuxResult(await FullWorkflow.Core.IngestionReviewRunner.RunForPipelineRunAsync(runId, "07-arch-ingest", settings, repoRoot).ConfigureAwait(false), "ingest-review"),
            var g => JsonSerializer.Serialize(new { error = "GATE_UI_NOT_SUPPORTED", pausedGate = g,
                hint = "UI-Start: decision-gate|pbi-gate (ingest = 3b-2); Chat-Weg: get_paused_gate." }, Json),
        };
    }

    private async Task<string> OpenReviewUiAsync(string proposalId)
        => AuxResult(await _openReview(proposalId).ConfigureAwait(false), "pbi-update-review");

    private async Task<string> RunClarifySweepAsync(string answersPath)
        => AuxResult(await _runSweep(answersPath).ConfigureAwait(false), "clarify-sweep");

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
