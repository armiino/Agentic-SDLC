using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' — <c>pipeline-full</c>: die GANZE Kette als EIN durabler MAF-Graph mit zentralem Gate-Responder.
/// </summary>
/// <remarks>
/// Stand U2 (ein-graph-vereinheitlichung): <c>run</c> assembliert die GANZE Kette (Front → Branch →
/// Bootstrap | Betrieb → Snapshot → Forward, 7 Human-Gates) in <see cref="PipelineFullWorkflow.Assemble"/> und
/// startet EINEN Stream — keine Phasen-Sequenzierung mehr im Runner. <c>run --dry-run</c> = LLM-freie
/// Build()-Validierung des Gesamtgraphen. <c>start</c> = Skeleton (Faden-Ordner + Plan-Events, kein LLM);
/// <c>resume</c> ist noch No-op (H1). Gate-Antworten kommen aus dem zentralen Responder
/// (<see cref="RunWorkflowStreamingAsync"/>, Policies interactive|accept-all|replay).
/// </remarks>
public static partial class PipelineFullRunner
{
    private const string Cmd = "pipeline-full";

    // A′ Schritt 3a — DÜNN + testbar: liest die Sweep-Antworten-Datei und baut den typisierten Eingangs-Vertrag.
    // KEINE Fachlogik hier (Plan/Alignment/Checker/Injektion liegen im ClarifyEntryExecutor).
    internal static ClarifySweepInput ReadClarifyInput(string path)
        => new(JsonSerializer.Deserialize<List<ClarifySweepAnswer>>(File.ReadAllText(path), JsonFiles.Json) ?? []);

    // R-26-C-Fix (11.08.-E2E-Fund): die pbi-gate-Antwort aus der Review-UI-Datei trägt Ops UND die akzeptierten
    // ANGLEICHUNGEN. Der frühere pipeline-full-Resume ließ die Angleichungen fallen (alignments=0 am Apply) →
    // needs_clarify fiel im Graphen NIE. Geteilte Umwandlung wie im Standalone-Apply → CLI/Graph-Parität.
    internal static (IReadOnlyList<string> AcceptedOpIds, IReadOnlyList<PbiAlignment>? AcceptedAlignments, string Reviewer)
        ResolvePbiGateDecisions(PbiStateChangePlanDocument plan, PbiUpdateDecisionsFile df)
        => (PbiUpdateApplyExec.AcceptedFromDecisions(plan, df.Decisions).Select(i => $"op-{i}").ToList(),
            PbiUpdateApplyExec.AcceptedAlignments(plan, df.AlignmentDecisions),
            df.Reviewer);

    private sealed record StagePlan(string Id, string Folder, string? Gate);

    private static IReadOnlyList<StagePlan> Plan(FullWorkflowSettings s)
    {
        var stages = new List<StagePlan>
        {
            new("01-ledger",     "01-ledger",     null),                // Quality-Gate det. (kein RequestPort)
            new("adjudication",  "01-ledger",     "adjudication-gate"), // NEUER RequestPort-Gate (Schritt 3)
            new("02-baselines",  "02-baselines",  null),                // Checker intern
        };
        if (s.L3Enabled) stages.Add(new("03-gap", "03-gap", null));     // v1: per Default aus
        stages.Add(new("04-delta",      "04-delta",      null));        // det. Executor (Spike 1)
        stages.Add(new("07-ingest",     "07-ingest",     "ingest-gate"));
        stages.Add(new("07-arch-ingest", "07-arch-ingest", "arch-ingest-gate")); // R-11 A1d: arch-Strip (Leer-Skip bei 0 arch-Items)
        stages.Add(new("07-arch-classify", "07-arch-classify", "arch-classify-gate")); // R-11 A2: Rollen (Leer-Skip wenn alles klassifiziert)
        stages.Add(new("07-adr", "07-adr", "adr-gate")); // R-11 A5: ADR-Projektion (Leer-Skip ohne pending design-Items)
        stages.Add(new("07-decision",   "07-decision",   "decision-gate")); // R-14 G1: Tor 2 (nur bei offenen DECs)
        stages.Add(new("07-pbi-update", "07-pbi-update", "pbi-gate"));
        stages.Add(new("snapshot",      "07-github",     null));        // Pflicht VOR Forward (R-16)
        stages.Add(new("07-github",     "07-github",     "github-forward-gate"));
        return stages;
    }

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
        => RunAsync(args, settings, repoRoot, onRunId: null);

    /// <summary>C1c (07.08.): dieselbe Bahn mit runId-Rückgabe-Naht — der Steward startet in-process
    /// im Hintergrund und braucht die runId SOFORT (K2: start-async + get_run_status). Die CLI nutzt
    /// den parameterlosen Weg; Verhalten dort unverändert.</summary>
    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot, Action<string>? onRunId)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => StartAsync(args, repoRoot),
            "run" => RunGraphAsync(args, settings, repoRoot, onRunId),
            "resume" => ResumeAsync(args, settings, repoRoot),
            "status" => StatusAsync(args, repoRoot),
            _ => Task.FromResult(Usage()),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  pipeline-full start [<transcript.txt>]   (Faden + Skeleton-Plan, kein LLM)");
        Console.Error.WriteLine("  pipeline-full run   [<transcript.txt>]   (EIN Graph: front->branch->bootstrap|betrieb->forward)");
        Console.Error.WriteLine("  pipeline-full run --from-delta <meeting-delta.json>  (Einstieg an der Hinterhälfte; Branch entscheidet Bootstrap|Betrieb)");
        Console.Error.WriteLine("  pipeline-full run --from-clarify <sweep-answers.json>  (A′: Klärungs-Antworten → PBI-Update → Forward, alles im durablen Graphen)");
        Console.Error.WriteLine("  pipeline-full run --reproject                         (ONLY-STEWARD Recovery: Wahrheit→GitHub neu abgleichen, wenn ein Forward scheiterte; Delta aus dem Core)");
        Console.Error.WriteLine("  pipeline-full run --from-github [--repo owner/name] [--issues <snapshot.json> [--comments <issue-comments.json>]]");
        Console.Error.WriteLine("  (jede run-Form: --arch-catchup = Bestands-Rueckstau an classify/adr BEWUSST mitnehmen; Default: nur Items dieses Laufs)");
        Console.Error.WriteLine("      (C2/C2d: GitHub-Front — AUTO-PULL Issues+Kommentare -> Detect+Destillat -> Delta -> Tore; --issues = Replay-Weg)");
        Console.Error.WriteLine("  pipeline-full run ... --policy <interactive|accept-all>  (Gate-Politik NUR für diesen Lauf; ersetzt auch explizite Gate-Einträge)");
        Console.Error.WriteLine("  pipeline-full run --dry-run              (Assemble/Build()-Validierung, kein LLM)");
        Console.Error.WriteLine("  pipeline-full resume <runId> [--accept-all | --accept id1,id2] [--open-ui]  (pausiertes Gate beantworten, weiterfahren)");
        Console.Error.WriteLine("  pipeline-full status [<runId>]           (WO steht was: pausierte Läufe + Gate + Weiter-Kommando)");
        return 2;
    }

    // W1e' Schritt 4 — echter Graph-Lauf (Slice v0: 01-ledger). Beweist, dass pipeline-full einen ECHTEN
    // MAF-Graphen ausführt (nicht mehr Skeleton). Weitere Stufen werden iterativ an Assemble angehängt.
    private static async Task<int> RunGraphAsync(string[] args, HostSettings settings, string repoRoot, Action<string>? onRunId = null)
    {
        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);

        // Schritt 5 ③: --policy <interactive|accept-all|replay:...> überschreibt das Profil DIESES Laufs
        // (Smoke/Experimente config-unabhängig; explizite Gate-Einträge der run-config gelten weiter vorrangig).
        for (var i = 2; i < args.Length; i++)
            if (string.Equals(args[i], "--policy", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                fw = fw with { PolicyProfile = GatePolicy.Parse(args[i + 1]), Gates = new Dictionary<string, GatePolicy>(StringComparer.OrdinalIgnoreCase) };

        // Schritt 5 ③: Eingangs-Vertrag — Transkript (volle Front) ODER --from-delta <meeting-delta.json>
        // (Hinterhälfte; der BranchDetector entscheidet Bootstrap|Betrieb wie immer). Ersetzt pipeline-hitl.
        string? deltaArg = null; string? issuesArg = null; string? commentsArg = null; string? repoArg = null; var fromGithub = false;
        string? clarifyArg = null;   // A′ Schritt 3a: --from-clarify <sweep-answers.json>
        var reproject = false;       // ONLY-STEWARD: --reproject (parameterlos, Recovery aus dem Core)
        var archCatchup = false;     // 1d (19.08.): Bestands-Rückstau (classify/adr) als BEWUSSTER Akt
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--from-delta", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) deltaArg = args[i + 1];
            else if (string.Equals(args[i], "--from-clarify", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) clarifyArg = args[i + 1];
            else if (string.Equals(args[i], "--arch-catchup", StringComparison.OrdinalIgnoreCase)) archCatchup = true;
            else if (string.Equals(args[i], "--reproject", StringComparison.OrdinalIgnoreCase)) reproject = true;
            else if (string.Equals(args[i], "--from-github", StringComparison.OrdinalIgnoreCase)) fromGithub = true;
            else if (string.Equals(args[i], "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) issuesArg = args[i + 1];
            else if (string.Equals(args[i], "--comments", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) commentsArg = args[i + 1];
            else if (string.Equals(args[i], "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) repoArg = args[i + 1];
        }

        ProjectStateDocument? entryDelta = null;
        PbiUpdate.ClarifySweepInput? entryClarify = null;   // A′ Schritt 3a
        string? transcriptPath = null;
        var transcriptText = string.Empty;
        if (deltaArg is not null)
        {
            var deltaPath = Path.IsPathRooted(deltaArg) ? deltaArg : Path.Combine(repoRoot, deltaArg);
            if (!File.Exists(deltaPath)) { Console.Error.WriteLine($"[{Cmd}] MeetingDelta nicht gefunden: {deltaPath}"); return 2; }
            entryDelta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;
        }
        else if (clarifyArg is not null)
        {
            // A′ Schritt 3a — DÜNN: nur Datei lesen/deserialisieren → Eingangs-Vertrag. Die Fachlogik
            // (Plan/Alignment/Checker/Injektion) liegt im ClarifyEntryExecutor, NICHT hier.
            var clarifyPath = Path.IsPathRooted(clarifyArg) ? clarifyArg : Path.Combine(repoRoot, clarifyArg);
            if (!File.Exists(clarifyPath)) { Console.Error.WriteLine($"[{Cmd}] Sweep-Antworten nicht gefunden: {clarifyPath}"); return 2; }
            entryClarify = ReadClarifyInput(clarifyPath);
        }
        else if (fromGithub)
        {
            // C2c (§13): die GitHub-Ernte als ZWEITE FRONT — geerntet wird NACH der Run-Erzeugung
            // (Agent-Logs/otel gehören zum Lauf); das Delta entsteht dann wie bei --from-delta.
        }
        else if (reproject)
        {
            // ONLY-STEWARD Recovery: kein Eingangs-Artefakt — der ReprojectEntryExecutor leitet das Sync-Delta
            // AUS DEM CORE ab (truth-first). Kein Transkript, kein Delta, keine Datei.
        }
        else
        {
            var transcriptArg = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : fw.Transcript;
            if (string.IsNullOrWhiteSpace(transcriptArg))
            {
                Console.Error.WriteLine($"[{Cmd}] Transkript fehlt (Arg oder run-config.fullworkflow.transcript) — oder --from-delta <datei> nutzen.");
                return 2;
            }
            transcriptPath = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, transcriptArg);
            if (!File.Exists(transcriptPath))
            {
                Console.Error.WriteLine($"[{Cmd}] Transkript nicht gefunden: {transcriptPath}");
                return 2;
            }
            transcriptText = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        }

        var run = new RunContext(RunId.New(), "fullworkflow");
        onRunId?.Invoke(run.RunId);   // C1c: runId sofort an den Starter melden (Steward), bevor der Lauf arbeitet
        run.EnsureFolders();
        run.OutputDir("01-ledger");
        run.OutputDir("checkpoints");

        // Telemetrie-Fix (25.07.): EIN Faden-Exporter für den GANZEN pipeline-full-Lauf. Die inline-Stufen
        // (Ingest/Pbi/Forward) haben keinen eigenen Sub-Run + Exporter → ihre Token-Spans (gen_ai.usage.*) fielen
        // bisher weg (kein TracerProvider hörte zu). Dieser Faden-Provider ist der dauerhafte "Boden", der auch
        // nach dem Dispose der Sub-Run-Provider (Ledger/Recipe) weiter zuhört. Fasst NUR den Exporter-Lebenszyklus
        // an — Middleware-Logs (response-text.md, tool-calls.jsonl …) sind RunContext-basiert und davon unabhängig.
        using var fadenOtel = OtelRunExporters.TryCreate(
            settings.OtelEnabled, "AgenticSdlc.Host",
            Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // C2c (§13): GitHub-Front — Ernte über die GETEILTE Naht (dieselbe wie die Zwischenbahn `github-inbound`);
        // der Faden-Exporter oben hört bereits zu (createOtel: false gegen Doppel-Spans). Ergebnis = entryDelta.
        if (fromGithub)
        {
            // C2d §3-1 AUTO-PULL: der Lauf zieht Issues + Kommentare SELBST frisch — das ⚿-Ja zum Lauf deckt
            // den deterministischen Read mit ab; der Snapshot wird Run-Artefakt (Beweis-Basis dieses Laufs).
            // `--issues [--comments]` bleibt der Replay-/Test-Weg (archivierter Stand statt Live-Pull).
            // R-49: Frühausstiege dieses Blocks passieren NACH der runId-Meldung an den Aufrufer (Steward!) —
            // ohne Event wirkte der tote Task im Chat wie „läuft noch". Jeder Abbruch wird als PIPELINE_ABORTED
            // mit Grund verewigt; der StatusReader macht daraus ein ehrliches ABGEBROCHEN.
            int Abort(string reason)
            {
                Console.Error.WriteLine($"[{Cmd}] ABGEBROCHEN: {reason}");
                run.AppendEvent(new { type = "PIPELINE_ABORTED", runId = run.RunId, reason, timestampUtc = DateTime.UtcNow });
                return 2;
            }

            if (issuesArg is null)
            {
                var repo = repoArg ?? fw.Repo;
                if (string.IsNullOrWhiteSpace(repo))
                    return Abort("--from-github braucht ein Repo für den Auto-Pull (--repo owner/name oder run-config.fullworkflow.repo) — ODER --issues <snapshot.json> als Replay-Weg.");
                var snapDir = Path.Combine(run.OutputDir("00-github-inbound"), "snapshot");
                if (await Tore.Github.GithubIssueSnapshotRunner.PullIssuesAsync(repo, repoRoot, outDir: snapDir, tokenEnv: fw.TokenEnv).ConfigureAwait(false) != 0)
                    return Abort($"Issue-Pull von {repo} fehlgeschlagen (Token/Netz/Repo prüfen — Details oben).");
                var pulledIssues = Path.Combine(snapDir, Tore.Github.GithubSnapshotLocator.FileName);
                if (await Tore.Github.GithubIssueSnapshotRunner.PullCommentsAsync(repo, repoRoot, outDir: snapDir, issuesPath: pulledIssues, tokenEnv: fw.TokenEnv).ConfigureAwait(false) != 0)
                    return Abort($"Kommentar-Pull von {repo} fehlgeschlagen (Details oben).");
                issuesArg = pulledIssues;
                commentsArg = Path.Combine(snapDir, Tore.Github.GithubSnapshotLocator.CommentsFileName);
            }

            var (hExit, harvest) = await Tore.Github.Inbound.GithubInboundHarvest.RunAsync(
                settings, repoRoot, run, issuesArg, draft: true, run.OutputDir("00-github-inbound"), createOtel: false,
                commentsArg: commentsArg)
                .ConfigureAwait(false);
            if (hExit != 0) return Abort($"GitHub-Ernte fehlgeschlagen (exit {hExit} — Details oben).");
            if (harvest!.Delta is not { Items.Count: > 0 })
            {
                Console.WriteLine($"[{Cmd}] GitHub-Ernte ohne Tor-faehige Funde — nichts zu fahren (Report: 00-github-inbound/).");
                // 1d (18.08., Session-2-Notiz): auch der Früh-Stopp ist ein LAUF-ENDE — dasselbe Abschluss-
                // Event wie der Normal-Weg (Status-Leser können sonst „fertig" nicht von „gestorben"
                // unterscheiden); `reason` macht den Leer-Stopp im Faden benennbar.
                run.AppendEvent(new { type = "PIPELINE_RUN_DONE", runId = run.RunId, exit = 0,
                    reason = "EMPTY_HARVEST", timestampUtc = DateTime.UtcNow });
                return 0;
            }
            entryDelta = harvest.Delta;
        }

        // H1: der Graph-Bau ist eine eigene Funktion — run UND resume bauen den IDENTISCHEN Graph
        // (Voraussetzung für RestoreCheckpointAsync).
        var (workflow, ledgerModel, baselineModel, adjudicationPolicy) = BuildGraph(
            run, settings, fw, repoRoot, transcriptText, transcriptPath is null ? "" : Path.GetFileName(transcriptPath), archCatchup);

        run.WriteConfig(new
        {
            command = Cmd,
            mode = "run",
            pipelineMode = fw.Mode.ToString(),
            slice = "ledger-v0",
            runId = run.RunId,
            entryPoint = fromGithub ? "github" : reproject ? "reproject" : entryClarify is not null ? "clarify" : entryDelta is null ? "transcript" : "delta",
            transcript = transcriptPath is null ? null : Path.GetRelativePath(repoRoot, transcriptPath),
            fromClarify = clarifyArg,
            fromDelta = deltaArg,
            execute = fw.Execute,
            policyProfile = fw.PolicyProfile.Kind.ToString(),
            ledgerModel,
            archCatchup,   // 1d: Resume rebaut den Graph mit DEMSELBEN Scope (Lauf-Fakt, kein CLI-Gedächtnis)
            timestampUtc = DateTime.UtcNow
        });

        // R-42 (08.08.): vergessene Experiment-Policies aus run-config sind eine teure Falle — ein Lauf
        // schrieb ungewollt in den Core. Deshalb LAUT am Start, wenn irgendein Gate nicht interactive ist.
        var experimentGates = fw.Gates
            .Where(g => g.Value.Kind != GatePolicyKind.Interactive)
            .Select(g => $"{g.Key}={g.Value.Kind}").OrderBy(x => x, StringComparer.Ordinal).ToList();
        if (fw.PolicyProfile.Kind != GatePolicyKind.Interactive || experimentGates.Count > 0)
            Console.WriteLine($"[{Cmd}] ⚠ EXPERIMENT-MODUS AKTIV — Gates antworten OHNE Menschen: "
                + (experimentGates.Count > 0 ? string.Join(", ", experimentGates) : $"profil={fw.PolicyProfile.Kind}")
                + " (Quelle: run-config.json/--policy; Wahrheits-Writes erfolgen automatisch!)");

        // U1/U2: LLM-freie Validierung des GANZEN Assemble (Build() lief soeben — Kanten/Typen geprüft).
        if (args.Contains("--dry-run"))
        {
            Console.WriteLine($"[{Cmd}] --dry-run: Ein-Graph Build()-bar (Front -> Branch -> Bootstrap | Betrieb -> Snapshot -> Forward; 7 Human-Gates inkl. decision-gate/Tor 2). Kein LLM.");
            return 0;
        }

        run.AppendEvent(new { type = "PIPELINE_START", runId = run.RunId, mode = "run", slice = "u2-ein-graph", adjudicationPolicy = adjudicationPolicy.Kind.ToString(), timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] run runId={run.RunId} slice=u2 (EIN Graph: front->branch->bootstrap|betrieb->forward) ledgerModel={ledgerModel} baselineModel={baselineModel ?? ledgerModel} adjudication={adjudicationPolicy.Kind} execute={fw.Execute}");

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(run.OutputDir("checkpoints")));
        var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(fw.TimeoutMinutes));

        var entry = reproject
            ? new PipelineFullEntry(null, null, null, new ReprojectRequest())        // ONLY-STEWARD: Recovery aus dem Core
            : entryClarify is not null
                ? new PipelineFullEntry(null, null, entryClarify)                    // A′ Schritt 3a: clarify-Batch
                : entryDelta is not null
                    ? new PipelineFullEntry(null, entryDelta)
                    : new PipelineFullEntry(new TranscriptInput(transcriptPath!, transcriptText), null);
        var exit = await RunWorkflowStreamingAsync<PipelineFullEntry>(
            workflow, entry, run.RunId, run, fw, manager, cts.Token,
            openUi: args.Contains("--open-ui")).ConfigureAwait(false);

        // H1: PAUSIERT (Exit 6) ist ein ERFOLGS-Ende dieses Prozesses — Zustand liegt im Checkpoint,
        // Weiterarbeit via `pipeline-full resume` (Metrics erst am echten Lauf-Ende).
        if (exit == 6)
        {
            Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{run.RunId}/  (PAUSIERT)");
            return 0;
        }

        run.AppendEvent(new { type = "PIPELINE_RUN_DONE", runId = run.RunId, exit, timestampUtc = DateTime.UtcNow });

        // W1e' Schritt 5 — metrics.json (reiner Sammler). Faden-otel VOR dem Lesen flushen (BatchProcessor);
        // coreItemsAfter = Live-Core JETZT (nach Ingest/Pbi-Apply, vor evtl. Restore). Metrics-Fehler kippen den Lauf nicht.
        fadenOtel?.ForceFlush();
        var coreRepoForMetrics = new JsonCoreRepository(repoRoot);
        var coreAfter = await coreRepoForMetrics.ExistsAsync().ConfigureAwait(false)
            ? (await coreRepoForMetrics.LoadAsync().ConfigureAwait(false)).Items.Count : 0;
        await MetricsFinalizer.WriteAsync(run, repoRoot, fw, settings, coreAfter, cts.Token).ConfigureAwait(false);

        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{run.RunId}/  exit={exit}");
        return exit;
    }

    // U2/H1: der EINE Graph-Bau — von run UND resume genutzt (identischer Graph ist Restore-Voraussetzung).
    // Schritt 5 ②: transcriptText ist Konstruktions-Input der Ledger-Kapsel (FacetValidation braucht das
    // Transkript im Konstruktor); beim Delta-Einstieg leer (die Kapsel wird nie angesprochen).
    private static (Workflow Workflow, string LedgerModel, string? BaselineModel, GatePolicy AdjudicationPolicy) BuildGraph(
        RunContext run, HostSettings settings, FullWorkflowSettings fw, string repoRoot, string transcriptText, string transcriptSourceName,
        bool archCatchup = false)
    {
        // Stufen-Modell: fullworkflow.models["01-ledger"] > jury.judgeModel > default.
        var ledgerModel = fw.Models.TryGetValue("01-ledger", out var m) && !string.IsNullOrWhiteSpace(m) ? m
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings.JuryJudgeModel!
            : settings.ModelId;
        var judgeSettings = settings with { ModelId = ledgerModel };

        var adjudicationPolicy = fw.GateFor("adjudication-gate");
        var baselineModel = fw.Models.TryGetValue("02-baselines", out var bm) && !string.IsNullOrWhiteSpace(bm) ? bm : null;
        // Stufen-Modell 06-backlog (Cluster/Clarify): fullworkflow.models["06-backlog"] > default agentModel.
        var backlogModel = fw.Models.TryGetValue("06-backlog", out var cm) && !string.IsNullOrWhiteSpace(cm) ? cm : settings.ModelId;
        var backlogSettings = settings with { ModelId = backlogModel };

        var (makerFactory, reviewFactory) = ReClarifyRunner.BuildClusterAgentFactories(repoRoot, settings, backlogSettings, run);
        var clarifyFactory = ReClarifyRunner.BuildClarifyAgentFactory(repoRoot, settings, backlogSettings, run);
        var clustersDir = Path.Combine(run.OutputDir("06-backlog"), "clusters");
        var backlogDir = Path.Combine(run.OutputDir("06-backlog"), "backlog");
        var ingestOutDir = run.OutputDir("07-ingest");
        var pbiOutDir = run.OutputDir("07-pbi-update");
        var githubOutDir = run.OutputDir("07-github");
        const int maxAttempts = 2;

        var ingestFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, AspectIngestionProfile.Requirement.AgentName, AspectIngestionProfile.Requirement.PromptName);
        // R-11 A1d: der arch-Strip — zweite Instanz des geteilten Kerns (Architecture-Profil, eigener Port).
        var archFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, AspectIngestionProfile.Architecture.AgentName, AspectIngestionProfile.Architecture.PromptName);
        var archOutDir = run.OutputDir("07-arch-ingest");
        ICandidateRetriever archRetriever = new ShowAllRequirementRetriever("architecture");
        var pbiFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, "PbiPlacementAgent", "PbiPlacementAgent1");
        var pbiAlignFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, "PbiAlignmentAgent", "PbiAlignmentAgent1"); // R-26-C
        ICandidateRetriever retriever = new ShowAllRequirementRetriever();

        // Forward ist der anspruchsvollste Agent (jeden Delta-PBI adressieren) → eigenes Modell:
        // fullworkflow.models["07-github"] > default agentModel.
        var fwdModel = fw.Models.TryGetValue("07-github", out var fm) && !string.IsNullOrWhiteSpace(fm) ? fm! : settings.ModelId;
        var fwdSettings = settings with { ModelId = fwdModel };
        var fwdPrompt = PromptProvider.Load(repoRoot, "phase2_evidence", "GithubForwardAgent", "GithubForwardAgent1",
            new Dictionary<string, string> { ["runId"] = run.RunId });
        var fwdClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(fwdSettings), settings, run, "GithubForwardAgent", "AgenticSdlc.Host");
        Func<IReadOnlyList<AITool>, AIAgent> fwdFactory = tools =>
            fwdClient.AsAIAgent(instructions: fwdPrompt, name: "GithubForwardAgent", tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        // Schritt 5 ② — die Ledger-Kapsel: Sub-Run H1-fest verankert (01-ledger/ledger-run.json), Workflow aus
        // der geteilten Montage-Naht, Bindung über den lauten Gate-frei-Wächter (Spike-Befund: Ports deadlocken
        // still in Kapseln). Intake/Summary sind die sichtbaren Stufen-Ränder im Ein-Graph.
        // Delta-Einstieg (leeres Transkript): die Kapsel wird nie angesprochen — KEIN Anker, KEIN Ordner
        // (sonst hinterließe jeder --from-delta-Lauf einen leeren runs/ledger-Beleg; Fund am Smoke 05.08.).
        var ledgerRun = transcriptText.Length > 0
            ? LedgerStageRun.GetOrCreate(run)
            : new RunContext(RunId.New(), "ledger");
        var ledgerCapsule = LedgerBuildUnitsRunner
            .CreateWorkflow(transcriptText, transcriptSourceName, settings, judgeSettings, ledgerRun)
            .BindGateFree("LedgerCapsule");

        // R-11 A2-2: Klassifikations-Strip (ein Strip, zwei Bahnen; 9. Gate arch-classify-gate).
        var classifyFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, "ArchClassifyAgent", "ArchClassifyAgent1");
        var classifyOutDir = run.OutputDir("07-arch-classify");
        var classifyNodes = new ArchClassifyNodes(
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.OperationalClassifyBridgeExecutor(run, repoRoot, classifyOutDir, maxAttempts, archCatchup),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.BootstrapClassifyBridgeExecutor(run, repoRoot, classifyOutDir, maxAttempts),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyMakerExecutor(classifyFactory, run),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyGateExecutor(run),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyRepairExecutor(classifyFactory, run),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyFinalizeExecutor(run, classifyOutDir),
            RequestPort.Create<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRequest, AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewResponse>("arch-classify-gate"),
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyApplyExecutor(run, repoRoot, classifyOutDir));

        // R-11 A5: ADR-Strip (10. Gate adr-gate; Betrieb arch-aktiv-gescoped, Bootstrap-Spiegel).
        var adrFactory = PipelineAgents.Factory(repoRoot, settings, judgeSettings, run, "AdrAuthorAgent", "AdrAuthorAgent1");
        var adrOutDir = run.OutputDir("07-adr");
        var adrNodes = new AdrNodes(
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrOperationalBridgeExecutor(run, repoRoot, adrOutDir, maxAttempts, archCatchup),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrBootstrapBridgeExecutor(run, repoRoot, adrOutDir, maxAttempts),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrMakerExecutor(adrFactory, run),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrGateExecutor(run),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrRepairExecutor(adrFactory, run),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrFinalizeExecutor(run, adrOutDir),
            RequestPort.Create<AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRequest, AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewResponse>("adr-gate"),
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrApplyExecutor(run, repoRoot, adrOutDir, "docs/adr"));

        var workflow = PipelineFullWorkflow.Assemble(
            new FrontNodes(
                new PipelineEntryExecutor(run),
                // A′ Schritt 2: der clarify-Graph-Eingang mit der GETEILTEN LLM-Alignment-Naht (PbiAnswerAlignment).
                new AgenticSdlc.Host.FullWorkflow.Pipeline.ClarifyEntryExecutor(run, repoRoot, pbiOutDir,
                    AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiAnswerAlignment.Llm(settings, repoRoot, run)),
                // ONLY-STEWARD: der Recovery-Eingang (Core→Delta, gemappt-only, truth-first).
                new AgenticSdlc.Host.FullWorkflow.Pipeline.ReprojectEntryExecutor(run, repoRoot),
                new LedgerIntakeExecutor(run, ledgerRun, transcriptText), ledgerCapsule, new LedgerSummaryExecutor(run, ledgerRun),
                new AdjudicationGateRequestExecutor(run),
                RequestPort.Create<AdjudicationReviewRequest, AdjudicationReviewResponse>("adjudication-gate"),
                new AdjudicationApplyExecutor(run),
                new AdjudicationEmptyGateResponder(run),
                new BaselineStageExecutor(settings, baselineModel, run, repoRoot),
                new ProjectStateBuildExecutor(run, repoRoot),
                new BranchDetectorExecutor(run, repoRoot, fw.Mode)),
            new BootstrapNodes(
                new CoreBootstrapStageExecutor(run, repoRoot),
                new ClusterBridgeExecutor(run, repoRoot, maxAttempts),
                new ClusterAgentExecutor(makerFactory, repoRoot, run),
                new ClusterGateExecutor(run),
                new ClusterRepairExecutor(makerFactory, repoRoot, run),
                new ClusterReviewExecutor(reviewFactory, run),
                new ClusterFinalizeExecutor(run, clustersDir),
                new ClusterGateRequestExecutor(run),
                RequestPort.Create<ClusterReviewRequest, ClusterReviewResponse>("cluster-review-gate"),
                new ClusterComposedApplyExecutor(run, repoRoot, clustersDir),
                new ClusterEmptyGateResponder(run),
                new BacklogBridgeExecutor(run, repoRoot, maxAttempts),
                new ClarifyAgentExecutor(clarifyFactory, run),
                new BacklogGateExecutor(run),
                new BacklogRepairExecutor(clarifyFactory, run),
                new BacklogFinalizeExecutor(run, backlogDir),
                new BacklogGateRequestExecutor(run),
                RequestPort.Create<BacklogReviewRequest, BacklogReviewResponse>("backlog-review-gate"),
                new BacklogComposedApplyExecutor(run, repoRoot, backlogDir),
                new CoreSeedBacklogExecutor(run, repoRoot)),
            new OperationalNodes(
                new IngestBridgeExecutor(run, repoRoot, maxAttempts),
                new AspectIngestionRouterExecutor(run, [AspectIngestionProfile.Requirement, AspectIngestionProfile.Architecture]),
                new IngestionHitlResolveExecutor(ingestFactory, retriever, run, AspectIngestionProfile.Requirement), new IngestionGateExecutor(run, AspectIngestionProfile.Requirement),
                new IngestionRepairExecutor(ingestFactory, retriever, run, AspectIngestionProfile.Requirement),
                new IngestionHitlFinalizeExecutor(run, ingestOutDir),
                RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingest-gate"),
                new IngestComposedApplyExecutor(run, repoRoot, ingestOutDir),
                new ArchIngestBridgeExecutor(run, repoRoot, maxAttempts),
                new IngestionHitlResolveExecutor(archFactory, archRetriever, run, AspectIngestionProfile.Architecture),
                new IngestionGateExecutor(run, AspectIngestionProfile.Architecture),
                new IngestionRepairExecutor(archFactory, archRetriever, run, AspectIngestionProfile.Architecture),
                new IngestionHitlFinalizeExecutor(run, archOutDir, AspectIngestionProfile.Architecture),
                RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("arch-ingest-gate"),
                new ArchComposedApplyExecutor(run, repoRoot, archOutDir, ingestOutDir),
                new DecisionScanExecutor(run, repoRoot, run.OutputDir("07-decision")),
                RequestPort.Create<PipelineDecisionReviewRequest, PipelineDecisionReviewResponse>("decision-gate"),
                new DecisionComposedApplyExecutor(run, repoRoot, ingestOutDir, run.OutputDir("07-decision")),
                new IngestPbiBridgeExecutor(run, repoRoot, pbiOutDir, maxAttempts),
                new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(pbiFactory, run), new PbiUpdateGateExecutor(run),
                new PbiUpdateRepairExecutor(pbiFactory, run), new PbiAlignExecutor(pbiAlignFactory, run), new PbiUpdateHitlFinalizeExecutor(run),
                RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-gate"),
                new PbiUpdateApplyExecutor(run, repoRoot, pbiOutDir),
                new PbiUpdateEmptyGateResponder(run),
                new IngestionEmptyGateResponder(run, AspectIngestionProfile.Requirement.GateName),
                new IngestionEmptyGateResponder(run, AspectIngestionProfile.Architecture.GateName)),
            classifyNodes,
            adrNodes,
            new ForwardNodes(
                new BootstrapForwardBridgeExecutor(run, repoRoot),
                new OperationalForwardBridgeExecutor(run, pbiOutDir, repoRoot, fw.Repo),   // R-66: Doc-Quelle im Skip
                new SnapshotExecutor(run, repoRoot, fw, githubOutDir, maxAttempts),
                new GithubForwardSeedExecutor(run, repoRoot), new GithubForwardMakerExecutor(fwdFactory, run),
                new GithubForwardGateExecutor(run), new GithubForwardRepairExecutor(fwdFactory, run),
                new GithubForwardHitlFinalizeExecutor(run),
                RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate"),
                // R-47: repository + tokenEnv aus fw durchreichen — sonst fällt ResolveToken(null) auf GITHUB_TEST_TOKEN
                // zurück (falscher Token → 403/404 beim echten Write, obwohl der Snapshot mit fw.TokenEnv liest).
                new GithubForwardApplyExecutor(run, repoRoot, githubOutDir, fw.Repo, fw.TokenEnv),
                new GithubForwardEmptyGateResponder(run)));

        return (workflow, ledgerModel, baselineModel, adjudicationPolicy);
    }
}
