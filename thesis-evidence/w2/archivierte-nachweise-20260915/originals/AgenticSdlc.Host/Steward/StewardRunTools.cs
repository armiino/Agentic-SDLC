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
    Func<string, int, string, Task<string>>? postComment = null,
    string? sessionName = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // Abnahme-4.0-Feil ②: der Session-Name ist ein HARNESS-Fakt (--session), kein Modell-Parameter —
    // das Modell erfand vorher „steward-chat" und der Herkunfts-Stempel log (ingestedFromSession).
    private readonly string _session = sessionName ?? "steward";

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
            + "[{text, disposition requirement|architecture|question|risk, rationale?, githubIssueNumber?, "
            + "aspect?, decisionRef?}]; question = offene Frage, risk = benanntes Risiko — beide werden via "
            + "QuestionLane zur offenen Entscheidung im DEC-Topf (Risiko: akzeptieren/mitigieren/klaeren am "
            + "decision-gate); aspect='architecture' = Architektur-Unklarheit (C4-Luecken-Einkipp — landet in "
            + "der §3-Projektion des C4); decisionRef='DEC-nnn' = dieses Diktat BEANTWORTET die DEC (das "
            + "entstehende Wahrheits-Item traegt den Anker, die C4-Luecke schliesst nachweisbar); "
            + "githubIssueNumber IMMER setzen, wenn der Anstoß aus einem Issue kam — z. B. reject am Gate + "
            + "Neu-Diktat — damit Forward-Link/Vermerk/Ernte-Gedächtnis intakt bleiben) WOERTLICH als Delta im "
            + "Meeting-Ketten-Vertrag (kein Wahrheits-Write — die Kette prägt erst nach den Gates). "
            + "Gibt deltaPath fuer run_pipeline_from_delta zurueck."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(RunCoreAnalysisAsync, "run_core_analysis",
            "1g-B: startet die LUECKEN-ANALYSE des Core-Analysten (4 Linsen: funktional/NFR/arch/risiko ueber "
            + "den GANZEN aktiven Core; Kosten ~ eine Tor-1-Resolver-Runde). NUR LESEN — Ergebnis sind Report + "
            + "Delta in runs/core-analysis/<id>/; die Wahrheits-Wirkung entscheidet der Autor DANACH separat "
            + "(run_pipeline_from_delta mit dem gelieferten deltaPath). Braucht Autor-Zustimmung.")),
        AIFunctionFactory.Create(CurateAnalysisDelta, "curate_analysis_delta",
            "1g AUSWAHL (kein Edit!): schreibt aus GEWAEHLTEN Funden eines Analyse-Laufs (indices 1-basiert aus "
            + "read_analysis_report) ein kuratiertes Teil-Delta (delta-auswahl.json; Statements WOERTLICH, Herkunft "
            + "CoreAnalyst intakt, Voll-Delta bleibt Beleg). Nicht Gewaehltes ist NICHT abgelehnt — bleibt offen und "
            + "kommt in der naechsten Analyse als WEITERHIN OFFEN wieder (R-35-Gedaechtnis entsteht NUR am Tor). "
            + "Text aendern/ergaenzen = Diktat, nie hier. Danach: run_pipeline_from_delta mit dem Auswahl-Pfad (⚿)."),
        AIFunctionFactory.Create(RenderRequirementsDocAsync, "render_requirements_doc",
            "1g-A: erstellt das ANFORDERUNGSDOKUMENT als Core-Projektion (docs/anforderungen.md — Funktionale je "
            + "Feature · NFRs je Qualitaetsmerkmal · Rahmenbedingungen · Offene Entscheidungen; Kopf: Version + "
            + "Stand + Core-Fingerabdruck). ERSTELLT keinen Inhalt, rendert nur den autorisierten Stand — "
            + "kein Wahrheits-Write, jederzeit regenerierbar."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DraftAuthoredDocAsync, "draft_authored_doc",
            "ENTWURF eines Autor-Artefakts (art: vision|personas|glossar|c4) durch den spezialisierten "
            + "Drafting-Agenten (eigene Prompt-Rezepte je Art; LLM-Kosten ~ ein Agent-Aufruf, Beleg in "
            + "runs/artifact-draft/). hinweise = verbindliche Autor-Vorgaben (auch fuer Iterationen: "
            + "'kuerzer', 'Persona X raus'). Ergebnis WOERTLICH dem Autor vorlegen — speichern erst nach "
            + "Freigabe via save_authored_doc. Braucht Autor-Zustimmung.")),
        AIFunctionFactory.Create(ReadAuthoredDoc, "read_authored_doc",
            "Liest den aktuellen Stand eines Autor-Artefakts (art: vision|personas|glossar|c4) — Grundlage "
            + "fuer den Update-Zyklus: Stand lesen, neuen Entwurf aus dem Core bauen, dem Autor den "
            + "UNTERSCHIED zeigen, erst nach Freigabe save_authored_doc. Existiert keins: sagt es ehrlich."),
        // Autor-Frage 21.08. („ist das sauber?"): die Freigabe-Pflicht ist HART — der Save ist ⚿-gewrappt
        // (MAF-ToolApproval), das Harness fragt den Autor vor JEDEM Schreiben; Prompt-Disziplin ist nur Komfort.
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SaveAuthoredDocAsync, "save_authored_doc",
            "Sichert die vom Autor FREIGEGEBENE Fassung eines Autor-Artefakts (art: vision|personas|glossar|c4; "
            + "Version/Stand-Kopf automatisch; jeder Save ersetzt den ganzen Text). Die Zustimmungs-Abfrage IST "
            + "die Freigabe — vorher den vollstaendigen Text im Chat gezeigt haben. entwurf = 'Autor-Diktat' oder "
            + "'Steward aus der Projektwahrheit (Core)'. Kein Wahrheits-Write; publiziert wird beim naechsten "
            + "Forward/Abgleich ueber das gated Doc-Publish. Braucht Autor-Zustimmung.")),
        AIFunctionFactory.Create(ProposePbiFieldsAsync, "propose_pbi_fields",
            "Slice S Teil 2: Autor-Diktat 'setze Prio/Schaetzung von PBI-x' -> deterministischer SET-Plan, "
            + "als WARTENDES Gate registriert (Pending-Registry) — es aendert sich NICHTS, bis der Autor am "
            + "pbi-update-Review entscheidet (open_review_ui mit der proposalId). Werte: Prio hoch|mittel|niedrig "
            + "(gespeichert high|medium|low), Schaetzung S|M|L. Kein LLM, keine Status-Wirkung."),
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
            + "pbi-gate | ingest-/arch-ingest-gate | arch-classify-/adr-gate | github-forward-gate). "
            + "'Fertig' in der UI kettet automatisch den resume (Folgestufen inkl. LLM; beim forward-gate "
            + "bleibt der GitHub-Write execute-Policy-gebunden). Braucht Autor-Zustimmung.")),
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
            // R-61-Nachwehe (20.08.): die EIGENE Rückfrage ist Ausgang, kein Eingang — der Anker wird
            // sofort auf die gepostete Kommentar-Id gestempelt, sonst erntet das System seine eigene
            // Frage in der nächsten Runde als „neuen" Kandidaten zurück (der #12-Boomerang). Team-
            // ANTWORTEN (höhere Ids) bleiben normale Ernte-Beute. Kein Anker-Ziel ⇒ laut im Ergebnis.
            var stamped = false;
            var idPart = url.Split("issuecomment-").LastOrDefault();
            if (long.TryParse(idPart, out var commentId) && commentId > 0)
            {
                var repoObj = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
                if (await repoObj.ExistsAsync().ConfigureAwait(false))
                {
                    var core = await repoObj.LoadAsync().ConfigureAwait(false);
                    (core, stamped) = FullWorkflow.Core.GithubCommentMeta.Stamp(core, issueNumber, commentId);
                    if (stamped) await repoObj.SaveAsync(core).ConfigureAwait(false);
                }
            }
            return JsonSerializer.Serialize(new { posted = true, issueNumber, url,
                ankerGestempelt = stamped,
                hint = stamped ? "Eigene Rueckfrage wird NICHT rueckgeerntet; Team-Antworten darunter schon."
                               : "KEIN Anker-Ziel (Issue ohne Mapping) — die Rueckfrage kann in der naechsten Ernte als Kandidat erscheinen." }, Json);
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

    // 1g-B: Steward-Haut über der geteilten Analyse-Naht (K13 — CLI `core-analysis run` nutzt DIESELBE).
    // Synchron wie open_gate_ui (das Tool-Ergebnis SAGT das Fertigsein — Selbstbeschreibungs-Prinzip);
    // die Konsolen-Weiche hält die Roh-Logs des Laufs aus dem Chat.
    private async Task<string> RunCoreAnalysisAsync()
    {
        if (ActiveRun() is { } busy)
            return JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy, hint = "get_run_status abwarten" }, Json);
        var log = new StewardRunConsole.RunLogWriter();
        try
        {
            FullWorkflow.Analyst.AnalystRunner.Ergebnis ergebnis;
            using (StewardRunConsole.Redirect(log))
            {
                ergebnis = await FullWorkflow.Analyst.AnalystRunner.RunCoreAnalysisAsync(settings, repoRoot).ConfigureAwait(false);
                log.SetTarget(Path.Combine(repoRoot, "runs", "core-analysis", ergebnis.RunId, "logs", "console.log"));
            }
            StewardRunConsole.WriteLifecycle($"[steward] ✔ Analyse {ergebnis.RunId} ist FERTIG.");
            // Klasse-Regel-Fix (Abnahme-Fund 20.08.): das ERGEBNIS reist im Tool-Resultat mit (Selbst-
            // beschreibung) — vorher nannte es nur Pfade und der Steward konnte seine eigenen Funde nicht vorlegen.
            return JsonSerializer.Serialize(new
            {
                analyseAbgeschlossen = true, ergebnis.RunId, ergebnis.InsDelta,
                funde = FullWorkflow.Analyst.AnalystRunner.LiesFunde(repoRoot, ergebnis.RunId),
                report = Path.GetRelativePath(repoRoot, ergebnis.ReportPath),
                deltaPath = ergebnis.DeltaPath is null ? null : Path.GetRelativePath(repoRoot, ergebnis.DeltaPath),
                hint = "Lege dem Autor die `funde` WOERTLICH vor (Statement + Herleitung + Kategorie je Fund; auch die "
                     + "aussortierten MIT Grund nennen — kein stiller Cap). Danach BIETE den Tor-Lauf an "
                     + "(run_pipeline_from_delta mit deltaPath, eigenes ⚿) — NIE ungefragt starten. "
                     + "AUSSORTIERTE kann der Autor per Diktat retten (save_author_statements, woertlich). "
                     + "deltaPath null = keine Funde; das ehrlich sagen.",
            }, Json);
        }
        catch (Exception ex)
        {
            var buffered = log.DrainBuffered();
            if (!string.IsNullOrWhiteSpace(buffered)) Console.WriteLine(buffered.TrimEnd());
            return JsonSerializer.Serialize(new { error = "ANALYSIS_FAILED", message = ex.Message }, Json);
        }
        finally { log.Dispose(); }
    }

    // 1g AUSWAHL (Autor-⚖ 20.08.): Selektion ist deterministisch (Code wählt per Index) — kein Modell-Edit.
    private string CurateAnalysisDelta(string runId, IReadOnlyList<int> indices)
    {
        var (path, count, fehler) = FullWorkflow.Analyst.AnalystRunner.CurateDelta(repoRoot, runId, indices);
        return fehler.Count > 0
            ? JsonSerializer.Serialize(new { error = "CURATE_INVALID", details = fehler }, Json)
            : JsonSerializer.Serialize(new { curated = true, count, deltaPath = Path.GetRelativePath(repoRoot, path),
                hint = "Auswahl gespeichert (Rest bleibt OFFEN, nicht abgelehnt). Naechster Schritt: run_pipeline_from_delta(deltaPath) — eigenes ⚿." }, Json);
    }

    // 1g-A: Steward-Haut über der geteilten Projektion-Naht (K13 — CLI `requirements-doc` nutzt DIESELBE).
    private async Task<string> RenderRequirementsDocAsync()
    {
        var (path, version, items) = await FullWorkflow.Core.RequirementsDocumentProjection.RunAsync(repoRoot).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { written = true, version, coreItems = items,
            path = Path.GetRelativePath(repoRoot, path),
            hint = "Dem Autor Version + Pfad nennen; Inhalt = autorisierter Core-Stand (Projektion)." }, Json);
    }

    private string SaveAuthorStatements(IReadOnlyList<FullWorkflow.Delta.AuthorStatement> statements)
    {
        // §3-7: Issue-Herkunft aus dem letzten Snapshot auflösen (Url + Hashes = Stand-beim-Diktat) —
        // die Delta-Schicht bleibt github-frei, die Naht liefert der Steward.
        var (delta, errors) = FullWorkflow.Delta.AuthorFrontDeltaBuilder.Build(statements, _session, ResolveGithubOrigin);
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "STATEMENTS_INVALID", details = errors }, Json);
        var dir = Path.Combine(repoRoot, "state", "steward", "author-front");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}-delta.json");
        File.WriteAllText(path, JsonSerializer.Serialize(delta, FullWorkflow.Delta.ProjectStateJson.Options));
        return JsonSerializer.Serialize(new { saved = true, items = delta!.Items.Count,
            deltaPath = Path.GetRelativePath(repoRoot, path),
            hint = "naechster Schritt (zustimmungspflichtig): run_pipeline_from_delta(deltaPath) — Tor 1 pausiert dann im Chat." }, Json);
    }

    // Autor-Artefakte (Drei-Klassen-Ordnung ⚖ 21.08., Endform am selben Tag): der ENTWURF kommt vom
    // spezialisierten Drafting-Agenten (AuthoredDocDrafting — Rezepte als eigene Prompt-Dateien, geteilte
    // Naht für den geparkten C4-Auto-Knoten); der Steward orchestriert nur. Der Chat ist das Review;
    // gespeichert wird ausschließlich die Autor-Freigabe (AuthoredDocument-Naht, feste Whitelist).
    private async Task<string> DraftAuthoredDocAsync(string art, string? hinweise = null)
    {
        if (FullWorkflow.Core.AuthoredDocument.Resolve(art) is null)
            return JsonSerializer.Serialize(new { error = "ART_UNBEKANNT", erlaubt = FullWorkflow.Core.AuthoredDocument.Arten.Select(a => a.Key) }, Json);
        var (draft, draftRunId) = await FullWorkflow.Core.AuthoredDocDrafting.DraftAsync(settings, repoRoot, art, hinweise).ConfigureAwait(false);
        return JsonSerializer.Serialize(new
        {
            art, draft, draftRun = $"runs/artifact-draft/{draftRunId}",
            hint = "Entwurf WOERTLICH vorlegen; Iteration = erneut draft_authored_doc mit hinweise; nach Autor-Freigabe save_authored_doc (⚿)."
        }, Json);
    }

    private string ReadAuthoredDoc(string art)
    {
        if (FullWorkflow.Core.AuthoredDocument.Resolve(art) is null)
            return JsonSerializer.Serialize(new { error = "ART_UNBEKANNT", erlaubt = FullWorkflow.Core.AuthoredDocument.Arten.Select(a => a.Key) }, Json);
        var content = FullWorkflow.Core.AuthoredDocument.Read(repoRoot, art);
        return content is not null
            ? JsonSerializer.Serialize(new { exists = true, content }, Json)
            : JsonSerializer.Serialize(new { exists = false, hint = "Noch nicht erstellt — Erst-Entwurf aus dem Core anbieten." }, Json);
    }

    private async Task<string> SaveAuthoredDocAsync(string art, string text, string? entwurf = null)
    {
        if (FullWorkflow.Core.AuthoredDocument.Resolve(art) is null)
            return JsonSerializer.Serialize(new { error = "ART_UNBEKANNT", erlaubt = FullWorkflow.Core.AuthoredDocument.Arten.Select(a => a.Key) }, Json);
        if (string.IsNullOrWhiteSpace(text))
            return JsonSerializer.Serialize(new { error = "TEXT_LEER", hint = "die vom Autor freigegebene Fassung" }, Json);
        var (path, version) = await FullWorkflow.Core.AuthoredDocument.SaveAsync(repoRoot, art, text, entwurf).ConfigureAwait(false);
        return JsonSerializer.Serialize(new
        {
            saved = true, art, version, path = Path.GetRelativePath(repoRoot, path),
            hint = "Publikation ins Team-Repo: naechster Forward/Abgleich (gated Doc-Publish)."
        }, Json);
    }

    // Slice S Teil 2: das Feld-Setz-Seil — registriert NUR den wartenden Vorschlag (Kern: PbiFieldsPlan,
    // geteilte Naht). Die Wahrheits-Mutation passiert erst im gated Apply des pbi-update-Reviews.
    private async Task<string> ProposePbiFieldsAsync(IReadOnlyList<FullWorkflow.PbiUpdate.PbiFieldWish> wishes)
    {
        if (wishes is not { Count: > 0 })
            return JsonSerializer.Serialize(new { error = "WISHES_EMPTY", hint = "je Wunsch pbiId + priority und/oder estimate" }, Json);
        var (proposalId, ops, errors) = await FullWorkflow.PbiUpdate.PbiFieldsPlan.RegisterAsync(repoRoot, wishes).ConfigureAwait(false);
        if (ops == 0)
            return JsonSerializer.Serialize(new { error = "NO_VALID_WISHES", errors }, Json);
        return JsonSerializer.Serialize(new
        {
            registered = true, proposalId, ops, errors,
            hint = $"Autor entscheidet am Gate: open_review_ui(\"{proposalId}\") — bis dahin aendert sich nichts."
        }, Json);
    }

    private string SaveSweepAnswers(IReadOnlyList<FullWorkflow.PbiUpdate.ClarifySweepAnswer> answers)
    {
        if (answers.Count == 0 || answers.Any(a => string.IsNullOrWhiteSpace(a.PbiId) || string.IsNullOrWhiteSpace(a.Antwort)))
            return JsonSerializer.Serialize(new { error = "ANSWERS_INVALID", hint = "jede Antwort braucht pbiId + antwort" }, Json);
        var stamped = answers.Select(a => a with { Quelle = "author via steward-chat", SessionName = a.SessionName ?? _session }).ToList();
        var dir = Path.Combine(repoRoot, "state", "steward", "sweep-answers");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(stamped, Json));
        return JsonSerializer.Serialize(new { saved = true, answers = stamped.Count, answersPath = Path.GetRelativePath(repoRoot, path) }, Json);
    }

    /// <summary>1b-Rest (18.08., Block-E-Fund „Fehlangebot"): EINE Fähigkeits-Quelle — angeboten wird nur, was geht.
    /// 1g-C (19.08., 9k(c)-Durchstich): classify + adr sind jetzt steward-bedienbar — kein Fremd-Terminal mehr.</summary>
    internal static readonly string[] SupportedUiGates =
        ["decision-gate", "pbi-gate", "ingest-gate", "arch-ingest-gate", "arch-classify-gate", "adr-gate", "github-forward-gate"];

    /// <summary>Für UI-only-Checkpoints ohne open_gate_ui: der korrekte Standalone-Befehl je Gate
    /// (nur noch die Bootstrap-/Adjudikations-UIs — 9k(c) hob classify/adr in die Fähigkeitsliste).</summary>
    internal static string? UiCommandFor(string? gate) => gate switch
    {
        "adjudication-gate" => "ledger-adjudicate-ui runs/fullworkflow/<runId>/01-ledger/queue.json",
        "cluster-review-gate" => "l4-re-clarify-review <dir>",
        "backlog-review-gate" => "l4-re-clarify-backlog-review <dir>",
        _ => null,
    };

    private async Task<string> OpenGateUiAsync(string runId)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        if (status?.State == FullWorkflow.Pipeline.PipelineRunState.NotPausedNotFinished)
            return JsonSerializer.Serialize(new { error = "RUN_ACTIVE", runId,
                hint = "Der Lauf arbeitet gerade — gleich erneut get_run_status, dann die UI öffnen." }, Json);
        // 1g-C-Endform (9k(c)): ALLE sechs UI-Runner ketten den Resume SELBST (Haus-Muster 3b-②/R-43 —
        // classify/adr nachgezogen, Zwei-Bahnen-Regel) — open_gate_ui ist wieder eine uniforme Haut.
        (string Bahn, Func<Task<int>> Body)? ui = status?.PausedGate switch
        {
            "decision-gate" => ("decision-gate-review", () => FullWorkflow.Decision.DecisionGateReviewRunner.RunForRunAsync(runId, settings, repoRoot)),
            "pbi-gate" => ("pbi-update-review", () => FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.RunForPipelineRunAsync(runId, settings, repoRoot)),
            "ingest-gate" => ("ingest-review", () => FullWorkflow.Core.IngestionReviewRunner.RunForPipelineRunAsync(runId, "07-ingest", settings, repoRoot)),
            "arch-ingest-gate" => ("ingest-review", () => FullWorkflow.Core.IngestionReviewRunner.RunForPipelineRunAsync(runId, "07-arch-ingest", settings, repoRoot)),
            "arch-classify-gate" => ("arch-classify-review", () => FullWorkflow.ArchClassify.ArchClassifyReviewRunner.RunAsync(["arch-classify-review", runId], settings, repoRoot)),
            "adr-gate" => ("adr-review", (Func<Task<int>>)(() => FullWorkflow.Adr.AdrReviewRunner.RunAsync(["adr-review", runId], settings, repoRoot))),
            // Phase-1i ② (23.08., R-64-Ausbau): das letzte Urteils-Gate bekommt die UI — Endbild „Chat ODER UI"
            // an ALLEN Urteils-Gates; der Runner schreibt human-decisions.json (Responder-Naht) + kettet resume.
            "github-forward-gate" => ("github-forward-review", () => FullWorkflow.Tore.Github.GithubForwardReviewRunner.RunForPipelineRunAsync(runId, settings, repoRoot)),
            _ => null,
        };
        if (ui is null)
            return JsonSerializer.Serialize(new { error = "GATE_UI_NOT_SUPPORTED", pausedGate = status?.PausedGate,
                supported = SupportedUiGates,
                hint = UiCommandFor(status?.PausedGate) is { } cmd
                    ? $"Dieser Checkpoint hat eine EIGENE Review-UI — Befehl: {cmd}"
                    : "Kein Gate pausiert bzw. kein UI-Weg — Lage via get_run_status." }, Json);

        // UI-Weg-Stille (18.08., letzte 1b-Lücke): der Runner (inkl. seines eingebetteten R-43-Resume mit
        // allen Folgestufen) läuft unter der Konsolen-Weiche — Roh-Log in die Lauf-Datei; NUR der deklarierte
        // [review-ui]-Nutzer-Kanal (URL, Browser-Warnung) erreicht den Chat. Danach dieselbe ⏸/✔-Endzeile
        // wie am Start-/Resume-Weg (EINE Quelle: EndLine).
        var exit = await RunUiUnderSwitchAsync(ConsoleLogPath(runId),
            $"[steward] UI fuer '{status!.PausedGate}' — Roh-Protokoll: runs/fullworkflow/{runId}/logs/console.log",
            ui.Value.Body).ConfigureAwait(false);
        Console.WriteLine(EndLine(repoRoot, runId, exit));
        return await UiRoundResultAsync(repoRoot, runId, ui.Value.Bahn, exit).ConfigureAwait(false);
    }

    /// <summary>
    /// Abnahme-4.0-Feil ③: das Werkzeug BLOCKIERT durch die ganze UI-Runde inkl. automatischer Fortsetzung —
    /// sein Ergebnis muss das SAGEN (Selbstbeschreibungs-Prinzip). Vorher kam ein generisches ok+hint zurück,
    /// der Agent glaubte „die UI ist jetzt offen" und bat den Autor um eine „fertig"-Meldung, die nie nötig war.
    /// </summary>
    internal static async Task<string> UiRoundResultAsync(string repoRoot, string runId, string bahn, int exit)
    {
        if (exit != 0)
            return JsonSerializer.Serialize(new { error = "AUX_RUN_FAILED", bahn, exitCode = exit }, Json);
        var after = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        return JsonSerializer.Serialize(new
        {
            uiRundeAbgeschlossen = true, bahn, runId,
            state = after?.State.ToString(), pausedGate = after?.PausedGate,
            hint = "Die UI-Runde ist FERTIG (Autor hat in der UI entschieden; die Fortsetzung lief automatisch mit). "
                 + "Lege SOFORT den neuen Stand vor (Kompass; naechstes Gate bzw. Bilanz) — warte NICHT auf eine Autor-Meldung.",
        }, Json);
    }

    private async Task<string> OpenReviewUiAsync(string proposalId)
    {
        // UI-Weg-Stille auch auf der Pending-Bahn (kein Pipeline-Lauf, eigenes Beleg-Log im Steward-Staat).
        var rel = Path.Combine("state", "steward", "logs", $"ui-{proposalId}.log");
        var exit = await RunUiUnderSwitchAsync(Path.Combine(repoRoot, rel),
            $"[steward] UI fuer Pending-Review '{proposalId}' — Roh-Protokoll: {rel}",
            () => _openReview(proposalId)).ConfigureAwait(false);
        // Feil ③ auch hier: das Tool blockiert durch UI + R-43-Apply — das Ergebnis sagt es.
        return JsonSerializer.Serialize(exit == 0
            ? new { uiRundeAbgeschlossen = true, proposalId,
                hint = "UI-Runde FERTIG (inkl. Apply) — get_core_overview zeigt den neuen Stand; SOFORT berichten, nicht auf eine Autor-Meldung warten." }
            : (object)new { error = "AUX_RUN_FAILED", bahn = "pbi-update-review", exitCode = exit }, Json);
    }

    private static async Task<int> RunUiUnderSwitchAsync(string logPath, string announce, Func<Task<int>> body)
    {
        var log = new StewardRunConsole.RunLogWriter();
        log.SetTarget(logPath);
        Console.WriteLine(announce);
        using (StewardRunConsole.Redirect(log))
        {
            try { return await body().ConfigureAwait(false); }
            finally { log.Dispose(); }
        }
    }

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

        // Politur 1b: die Lauf-Konsole gehoert in die Protokoll-Datei des Laufs, nicht ins Chat-Prompt —
        // Redirect-Scope NUR um den Lauf-Task (AsyncLocal); Chat/⚿/UIs bleiben unberuehrt. Zeilen vor der
        // runId-Meldung puffert der Writer und traegt sie beim SetTarget nach.
        var log = new StewardRunConsole.RunLogWriter();
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = LoudOnFault(string.Join(' ', args), async () =>
        {
            using var _ = StewardRunConsole.Redirect(log);
            try { return await _runner(args, id => { tcs.TrySetResult(id); log.SetTarget(ConsoleLogPath(id)); }).ConfigureAwait(false); }
            finally { log.Dispose(); }
        });
        var completed = await Task.WhenAny(tcs.Task, task).ConfigureAwait(false);
        if (completed == task && !tcs.Task.IsCompleted)   // Runner endete VOR der runId-Meldung = Startfehler (z. B. Datei fehlt)
        {
            var buffered = log.DrainBuffered();            // stdout-Vorlauf des Fehlstarts NICHT verschlucken
            if (!string.IsNullOrWhiteSpace(buffered)) Console.WriteLine(buffered.TrimEnd());
            return JsonSerializer.Serialize(new { error = "RUN_START_FAILED", exitCode = await task.ConfigureAwait(false), what }, Json);
        }

        var runId = await tcs.Task.ConfigureAwait(false);
        _started[runId] = task; _activeRunId = runId;
        NotifyEnd(runId, task);
        Console.WriteLine($"[steward] Lauf {runId} gestartet — Roh-Protokoll: runs/fullworkflow/{runId}/logs/console.log (Chat bleibt still; `steward --debug` zeigt Rohzeilen).");
        return JsonSerializer.Serialize(new { started = true, runId, consoleLog = $"runs/fullworkflow/{runId}/logs/console.log", hint = $"get_run_status(\"{runId}\") fuer Fortschritt/Pause" }, Json);
    }

    private Task<string> ResumeRunAsync(string runId, bool acceptAll = false) => ChainResumeAsync(runId, acceptAll);

    /// <summary>1c: dieselbe Resume-Naht fuer das ⚿-Tool UND die Submit-Kette der Gate-Tools (R-43-Chat-Bahn).</summary>
    internal Task<string> ChainResumeAsync(string runId, bool acceptAll = false)
    {
        if (ActiveRun() is { } busy && busy != runId)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "STEWARD_BUSY", activeRunId = busy }, Json));
        // R-51-Wache (17.08., Block E): arbeitet der Lauf NOCH in diesem Prozess, hält er den MAF-Checkpoint-
        // Store (prozess-exklusiv) — ein zweiter Resume liefe in den Store-Konflikt. Ehrliche Sofort-Antwort.
        if (_started.TryGetValue(runId, out var running) && !running.IsCompleted)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "RUN_STILL_ACTIVE", runId,
                hint = "Der Lauf arbeitet noch in diesem Prozess — get_run_status abwarten, dann erneut." }, Json));

        string[] args = acceptAll ? ["pipeline-full", "resume", runId, "--accept-all"] : ["pipeline-full", "resume", runId];
        var log = new StewardRunConsole.RunLogWriter();
        log.SetTarget(ConsoleLogPath(runId));              // Politur 1b: runId ist bekannt — direkt in die Datei
        var task = LoudOnFault($"resume {runId}", async () =>
        {
            using var _ = StewardRunConsole.Redirect(log);
            try { return await _runner(args, null).ConfigureAwait(false); }
            finally { log.Dispose(); }
        });
        _started[runId] = task; _activeRunId = runId;
        NotifyEnd(runId, task);
        return Task.FromResult(JsonSerializer.Serialize(new { resuming = true, runId, acceptAll,
            hint = $"get_run_status(\"{runId}\") fuer Fortschritt/naechstes Gate" }, Json));
    }

    private string ConsoleLogPath(string runId)
        => Path.Combine(repoRoot, "runs", "fullworkflow", runId, "logs", "console.log");

    // Politur 1b: GENAU EINE wohlgeformte Steward-Zeile am Task-Ende (statt Roh-Log-Gewitter) — Pause,
    // FERTIG oder Fehler; die Kompass-Erklaerung liefert der Agent auf die naechste Autor-Frage.
    // Kosmetik-Feil (Abnahme 4.0): via WriteLifecycle — klebt nicht mehr am wartenden du>-Prompt.
    private void NotifyEnd(string runId, Task<int> task)
        => _ = task.ContinueWith(t => StewardRunConsole.WriteLifecycle(EndLine(repoRoot, runId,
                t.IsCompletedSuccessfully ? t.Result : -1)), TaskScheduler.Default);

    internal static string EndLine(string repoRoot, string runId, int exit)
    {
        var pointer = Path.Combine(repoRoot, "runs", "fullworkflow", runId, "checkpoints", "pointer.json");
        if (File.Exists(pointer))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(pointer));
                var mode = doc.RootElement.TryGetProperty("mode", out var m) ? m.GetString() : null;
                if (!string.IsNullOrWhiteSpace(mode))
                    return $"[steward] ⏸ Lauf {runId} haelt am Checkpoint '{mode}' — sag mir, ob ich vorlegen soll.";
            }
            catch { /* best-effort — die generische Zeile unten bleibt wahr */ }
            return $"[steward] ⏸ Lauf {runId} pausiert an einem Checkpoint — frag mich, ich lege vor.";
        }
        // Feil 18.08.: keine „frag mich"-Aufforderung — kettet der Submit, liefert der Agent die Bilanz
        // ohnehin sofort im selben Zug; die Code-Zeile bleibt neutrales Signal.
        return exit == 0
            ? $"[steward] ✔ Lauf {runId} ist FERTIG."
            : $"[steward] ✖ Lauf {runId} endete mit Fehler (exit {exit}).";
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
