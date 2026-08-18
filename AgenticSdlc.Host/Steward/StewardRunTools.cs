using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// C1c (07.08.2026) — die STARTENDEN Tools des Stewards (Vorlauf-Move C1c): `run_pipeline_from_delta` und
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
    Func<string, Task<int>>? openReview = null,
    Func<string, Task<int>>? pullComments = null,
    Func<string, int, string, Task<string>>? postComment = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // Test-Naht: der echte Runner ist der Default; Tests injizieren einen Fake (keine LLM-Läufe im Test).
    private readonly Func<string[], Action<string>?, Task<int>> _runner =
        runner ?? ((args, onRunId) => PipelineFullRunner.RunAsync(args, settings, repoRoot, onRunId));

    // K13-2 (09.08.): TYPISIERTE Nähte statt CLI-String-Args (K13: CLI ist Eingang, nie Integrationsschicht).
    // Je Bahn ein Delegate auf den gehobenen Kern — Test-injizierbar, kompilierfest.
    // Klasse-Regel (⚖ 13.08., system-inventar §3): die vier blinden Werkbank-Seile (run_clarify_sweep,
    // run_github_inbound, run_comment_distill, run_github_reverse) sind ENTFERNT — ihre Ergebnisse waren im
    // Chat nicht lesbar (nur runs/-Ordner); die Bahnen bleiben als CLI-Werkbank. Betriebswege: Ernte =
    // run_pipeline_from_github (pullt+destilliert selbst, stoppt leer) · Klärung = run_clarify_via_graph.
    private readonly Func<string, Task<int>> _pullSnapshot =
        pullSnapshot ?? (repo => FullWorkflow.Tore.Github.GithubIssueSnapshotRunner.PullIssuesAsync(repo, repoRoot));
    private readonly Func<string, Task<int>> _pullComments =
        pullComments ?? (repo => FullWorkflow.Tore.Github.GithubIssueSnapshotRunner.PullCommentsAsync(repo, repoRoot));
    private readonly Func<string, int, string, Task<string>> _postComment =
        postComment ?? (async (repo, issueNumber, text) =>
        {
            var token = Mcp.GithubMcp.ResolveToken();
            if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("Kein GitHub-Token (GITHUB_AGENTIC_REFACTOR_TOKEN/GITHUB_TEST_TOKEN).");
            using var http = new HttpClient();
            var result = await new FullWorkflow.Tore.Github.GithubRestIssueClient(http, token!, "Agentic-SDLC")
                .CreateCommentAsync(repo, issueNumber, text, CancellationToken.None).ConfigureAwait(false);
            return result.IssueUrl ?? $"gh#{issueNumber}";
        });
    private readonly Func<string, Task<int>> _openReview =
        openReview ?? (proposalId => FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.RunPendingAsync(proposalId, settings, repoRoot));

    private readonly ConcurrentDictionary<string, Task<int>> _started = new(StringComparer.Ordinal);
    private volatile string? _activeRunId;

    /// <summary>Für die Hülle/Tests: die im Hintergrund gestarteten Läufe (runId → Task).</summary>
    public IReadOnlyDictionary<string, Task<int>> Started => _started;

    public IReadOnlyList<AITool> Build() =>
    [
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunPipelineFromDeltaAsync, "run_pipeline_from_delta",
            "STARTET die Kette AUS EINEM BESTEHENDEN DELTA (deltaPath — z. B. Autor-Diktat oder Kommentar-Destillat); "
            + "kostenpflichtig, LLM. NICHT von vorne (kein Ledger/Transkript — dafuer gibt es keinen Steward-Start; "
            + "GitHub-Front = run_pipeline_from_github). Kehrt SOFORT mit der runId zurueck; Fortschritt ueber get_run_status. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunPipelineFromGithubAsync, "run_pipeline_from_github",
            "DIE GitHub-Ernte (auch als CHECK 'was ist neu auf GitHub?'): startet einen pipeline-full-Lauf mit der "
            + "Ernte als Front — pullt SELBST frisch (Issues + Kommentare), erkennt deterministisch Neues/Geaendertes, "
            + "STOPPT SAUBER wenn nichts Tor-faehiges da ist (dann 0 LLM-Kosten); bei Funden: Drafts → Delta → Tore "
            + "(LLM). Kehrt sofort mit runId zurueck; Ernte-Ergebnis danach: read_run_report (harvest). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunReprojectAsync, "run_reproject",
            "ONLY-STEWARD RECOVERY: aufrufen, wenn GitHub hinter dem Core haengt — ENTWEDER ein Forward scheiterte "
            + "(success=false / 403 / 404) ODER der Autor bittet AUSDRUECKLICH um den Core->GitHub-Abgleich ('gleich ab', "
            + "'run_reproject', 'GitHub haengt hinterher'). Bei ausdruecklicher Bitte NICHT erst pull_github_snapshot vorschalten "
            + "und NICHT nach einem pausierten Lauf suchen: run_reproject STARTET SELBST einen frischen durablen Lauf und zieht "
            + "den Snapshot selbst. Es gleicht die gemappten PBIs des Cores gegen den frischen Snapshot ab (Delta AUS DEM CORE, "
            + "kein Run-Ordner; deterministisch, LLM-frei) und pausiert am forward-gate — DER PLAN DORT IST der Drift-Check "
            + "(nichts wird ohne Freigabe geschrieben). Nur nicht als unaufgeforderter Routine-Schritt. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(ResumeRunAsync, "resume_run",
            "SETZT einen pausierten Lauf fort (runId; acceptAll=true uebernimmt Vorschlaege 1:1 = Experiment-Modus). "
            + "Kehrt sofort zurueck; Fortschritt ueber get_run_status. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(PullGithubSnapshotAsync, "pull_github_snapshot",
            "Zieht einen FRISCHEN GitHub-Issue-Snapshot (externer API-Read; repo = owner/name, z. B. "
            + "armiino/Agentic-GitHub-refactor). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(PullIssueCommentsAsync, "pull_issue_comments",
            "C2d: zieht einen FRISCHEN Kommentar-Snapshot (externer API-Read; Filter = letzter Issue-Snapshot; "
            + "Kommentare sind Diskussionsraum/Evidenz, nie Wahrheit). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(PostIssueCommentAsync, "post_issue_comment",
            "C2d-③ RUECKFRAGE: postet einen Diskussions-Kommentar an ein Issue (z. B. eine Rueckfrage fuer die "
            + "naechste Team-Runde). Kommentare sind Diskussionsraum, NIE Wahrheit — trotzdem Aussenwirkung: "
            + "den WOERTLICHEN Text VORHER dem Autor vorlesen, erst nach seinem OK aufrufen. Braucht Autor-Zustimmung.")),
        AIFunctionFactory.Create(SaveAuthorStatements, "save_author_statements",
            "3c AUTOR-FRONT Schritt 1: schreibt die vom Autor DIKTIERTEN Wahrheits-Kandidaten (statements: "
            + "[{text, disposition requirement|architecture|question, rationale?, githubIssueNumber?}]; "
            + "question = offene Frage, wird via 9g-Schiene zur offenen Entscheidung im DEC-Topf; "
            + "githubIssueNumber IMMER setzen, wenn der Anstoß aus einem Issue kam — z. B. reject am Gate + "
            + "Neu-Diktat — damit Forward-Link/Vermerk/Ernte-Gedächtnis intakt bleiben) WOERTLICH als Delta im "
            + "Meeting-Ketten-Vertrag (kein Wahrheits-Write — die Kette prägt erst nach den Gates). "
            + "Gibt deltaPath fuer run_pipeline_from_delta zurueck."),
        AIFunctionFactory.Create(SaveSweepAnswers, "save_sweep_answers",
            "C4-Zielschleife Schritt 2: schreibt die vom Autor DIKTIERTEN Klaerungs-Antworten als "
            + "sweep-answers.json (kein Wahrheits-Write; Quelle = author via steward-chat). "
            + "answers: [{pbiId, antwort}]. Gibt den Pfad zurueck — BETRIEBSPFAD danach: run_clarify_via_graph."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunClarifyViaGraphAsync, "run_clarify_via_graph",
            "C4-BETRIEBSPFAD (LLM-Kosten): STARTET aus der Antworten-Datei einen DURABLEN pipeline-full-Lauf, der "
            + "Angleichung → pbi-Gate → Apply → Forward DURCHFAEHRT (Steward = Orchestrator EINES Workflows). Kehrt "
            + "SOFORT mit runId zurueck; pausiert an den Human-Gates (get_run_status / resume_run). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(OpenGateUiAsync, "open_gate_ui",
            "3b: oeffnet auf Autor-Ja die REVIEW-UI zum PAUSIERTEN Graph-Gate eines Laufs (decision-gate | "
            + "pbi-gate | ingest-/arch-ingest-gate). 'Fertig' in der UI kettet automatisch den resume "
            + "(Folgestufen inkl. LLM). Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(OpenReviewUiAsync, "open_review_ui",
            "Oeffnet auf Autor-Ja das REVIEW zu einem wartenden Registry-Eintrag (proposalId aus "
            + "get_core_overview.openDecisions.pendingReviews) — der Autor entscheidet am Gate, nie du. Braucht Autor-Zustimmung.")),
    ];

    private async Task<string> PullGithubSnapshotAsync(string repo)
        => AuxResult(await _pullSnapshot(repo).ConfigureAwait(false), "github-snapshot");

    private async Task<string> PullIssueCommentsAsync(string repo)
        => AuxResult(await _pullComments(repo).ConfigureAwait(false), "github-snapshot comments");

    // §3-7: Herkunfts-Auflösung fürs Autor-Diktat — Url + Titel/Body-Hashes aus dem letzten Issue-Snapshot
    // (Stand-beim-Diktat als Drift-Anker). Issue unbekannt/kein Snapshot ⇒ null (nur die Nummer reist).
    private IReadOnlyDictionary<string, string>? ResolveGithubOrigin(int issueNumber)
    {
        var path = FullWorkflow.Tore.Github.GithubSnapshotLocator.FindLatest(repoRoot);
        if (path is null) return null;
        try
        {
            var issues = JsonSerializer.Deserialize<List<FullWorkflow.Tore.Github.GithubIssueSnapshot>>(File.ReadAllText(path), Json) ?? [];
            var issue = issues.LastOrDefault(i => i.IssueNumber == issueNumber);
            if (issue is null) return null;
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [FullWorkflow.Core.GithubOriginMeta.IssueUrl] = issue.Url ?? "",
                [FullWorkflow.Core.GithubOriginMeta.HarvestedTitleHash] = FullWorkflow.Tore.Github.GithubProjectionHash.Compute(issue.Title),
                [FullWorkflow.Core.GithubOriginMeta.HarvestedBodyHash] = FullWorkflow.Tore.Github.GithubProjectionHash.Compute(issue.Body),
            };
        }
        catch { return null; }   // kaputter Snapshot kostet nur die Anreicherung, nie das Diktat
    }

    private async Task<string> PostIssueCommentAsync(string repo, int issueNumber, string text)
    {
        try
        {
            var url = await _postComment(repo, issueNumber, text).ConfigureAwait(false);
            return JsonSerializer.Serialize(new { posted = true, issueNumber, url }, Json);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = "COMMENT_POST_FAILED", message = ex.Message }, Json);
        }
    }

    private static string AuxResult(int exitCode, string bahn)
        => JsonSerializer.Serialize(exitCode == 0
            ? new { ok = true, bahn, hint = "Details stehen im juengsten runs/-Ordner der Bahn." }
            : (object)new { error = "AUX_RUN_FAILED", bahn, exitCode }, Json);

    private Task<string> RunPipelineFromGithubAsync() => StartPipelineAsync(["pipeline-full", "run", "--from-github"], "github-ernte");

    // ONLY-STEWARD Recovery (steward/reprojektion-only-steward.md): re-projiziert die Wahrheit nach GitHub, wenn ein
    // Forward am externen Rand scheiterte. Delta AUS DEM CORE (truth-first), durch den durablen Graphen, pausiert am
    // forward-gate. Gleiche Start-async-Naht wie die anderen Läufe (runId sofort, Pause/Resume).
    private Task<string> RunReprojectAsync() => StartPipelineAsync(["pipeline-full", "run", "--reproject"], "reproject");

    private string SaveAuthorStatements(IReadOnlyList<FullWorkflow.Delta.AuthorStatement> statements, string? sessionName = null)
    {
        var session = sessionName ?? "steward";
        // §3-7: Issue-Herkunft aus dem letzten Snapshot auflösen (Url + Hashes = Stand-beim-Diktat) —
        // die Delta-Schicht bleibt github-frei, die Naht liefert der Steward.
        var (delta, errors) = FullWorkflow.Delta.AuthorFrontDeltaBuilder.Build(statements, session, ResolveGithubOrigin);
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "STATEMENTS_INVALID", details = errors }, Json);
        var dir = Path.Combine(repoRoot, "state", "steward", "author-front");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}-delta.json");
        File.WriteAllText(path, JsonSerializer.Serialize(delta, FullWorkflow.Delta.ProjectStateJson.Options));
        return JsonSerializer.Serialize(new { saved = true, items = delta!.Items.Count,
            deltaPath = Path.GetRelativePath(repoRoot, path),
            hint = "naechster Schritt (zustimmungspflichtig): run_pipeline_from_delta(deltaPath) — Tor 1 pausiert dann im Chat." }, Json);
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

    private Task<string> RunPipelineFromDeltaAsync(string deltaPath)
        => StartPipelineAsync(["pipeline-full", "run", "--from-delta", deltaPath], deltaPath);

    // A′ Schritt 3a — der BETRIEBSPFAD der Klärung: startet einen DURABLEN pipeline-full-Lauf aus der Antworten-Datei
    // (clarify → pbi-Gate → Apply → Forward). Gleiche Start-async-Naht wie die anderen Läufe (runId sofort, Pause/Resume).
    private Task<string> RunClarifyViaGraphAsync(string answersPath)
        => StartPipelineAsync(["pipeline-full", "run", "--from-clarify", answersPath], answersPath);

    private async Task<string> StartPipelineAsync(string[] args, string what)
    {
        if (ActiveRun() is { } busy)
            return JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy, hint = "get_run_status abwarten" }, Json);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = LoudOnFault(string.Join(' ', args), () => _runner(args, id => tcs.TrySetResult(id)));
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
        // R-51-Wache (17.08., Block E): arbeitet der Lauf NOCH in diesem Prozess, hält er den MAF-Checkpoint-
        // Store (prozess-exklusiv) — ein zweiter Resume liefe in den Store-Konflikt. Ehrliche Sofort-Antwort.
        if (_started.TryGetValue(runId, out var running) && !running.IsCompleted)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "RUN_STILL_ACTIVE", runId,
                hint = "Der Lauf arbeitet noch in diesem Prozess — get_run_status abwarten, dann erneut." }, Json));

        string[] args = acceptAll ? ["pipeline-full", "resume", runId, "--accept-all"] : ["pipeline-full", "resume", runId];
        var task = LoudOnFault($"resume {runId}", () => _runner(args, null));
        _started[runId] = task; _activeRunId = runId;
        return Task.FromResult(JsonSerializer.Serialize(new { resuming = true, runId, acceptAll,
            hint = $"get_run_status(\"{runId}\") fuer Fortschritt/naechstes Gate" }, Json));
    }

    // R-51b (17.08., Block E): Lauf-Tasks laufen NIE unbeobachtet — ein sterbender Runner (z. B. Checkpoint-
    // Store-Konflikt beim Resume) wurde vorher spurlos verschluckt („resuming true", dann Stille; ActiveRun
    // räumte still auf). Jetzt: Fault → laute Konsolen-Meldung + Exit −1 (Task bleibt auswertbar, nie faulted).
    private static Task<int> LoudOnFault(string what, Func<Task<int>> body) => Task.Run(async () =>
    {
        try { return await body().ConfigureAwait(false); }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[steward] LAUF-TASK GESTORBEN ({what}): {ex.GetBaseException().Message}");
            return -1;
        }
    });

    private string? ActiveRun()
    {
        var id = _activeRunId;
        if (id is null || !_started.TryGetValue(id, out var t) || t.IsCompleted) { _activeRunId = null; return null; }
        return id;
    }
}
