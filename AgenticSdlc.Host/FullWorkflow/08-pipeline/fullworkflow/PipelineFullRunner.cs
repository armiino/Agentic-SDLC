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
public static class PipelineFullRunner
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
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--from-delta", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) deltaArg = args[i + 1];
            else if (string.Equals(args[i], "--from-clarify", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) clarifyArg = args[i + 1];
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
            if (issuesArg is null)
            {
                var repo = repoArg ?? fw.Repo;
                if (string.IsNullOrWhiteSpace(repo))
                {
                    Console.Error.WriteLine($"[{Cmd}] --from-github braucht ein Repo für den Auto-Pull "
                        + "(--repo owner/name oder run-config.fullworkflow.repo) — ODER --issues <snapshot.json> als Replay-Weg.");
                    return 2;
                }
                var snapDir = Path.Combine(run.OutputDir("00-github-inbound"), "snapshot");
                if (await Tore.Github.GithubIssueSnapshotRunner.PullIssuesAsync(repo, repoRoot, outDir: snapDir).ConfigureAwait(false) != 0)
                    return 2;
                var pulledIssues = Path.Combine(snapDir, Tore.Github.GithubSnapshotLocator.FileName);
                if (await Tore.Github.GithubIssueSnapshotRunner.PullCommentsAsync(repo, repoRoot, outDir: snapDir, issuesPath: pulledIssues).ConfigureAwait(false) != 0)
                    return 2;
                issuesArg = pulledIssues;
                commentsArg = Path.Combine(snapDir, Tore.Github.GithubSnapshotLocator.CommentsFileName);
            }

            var (hExit, harvest) = await Tore.Github.Inbound.GithubInboundHarvest.RunAsync(
                settings, repoRoot, run, issuesArg, draft: true, run.OutputDir("00-github-inbound"), createOtel: false,
                commentsArg: commentsArg)
                .ConfigureAwait(false);
            if (hExit != 0) return hExit;
            if (harvest!.Delta is not { Items.Count: > 0 })
            {
                Console.WriteLine($"[{Cmd}] GitHub-Ernte ohne Tor-faehige Funde — nichts zu fahren (Report: 00-github-inbound/).");
                return 0;
            }
            entryDelta = harvest.Delta;
        }

        // H1: der Graph-Bau ist eine eigene Funktion — run UND resume bauen den IDENTISCHEN Graph
        // (Voraussetzung für RestoreCheckpointAsync).
        var (workflow, ledgerModel, baselineModel, adjudicationPolicy) = BuildGraph(
            run, settings, fw, repoRoot, transcriptText, transcriptPath is null ? "" : Path.GetFileName(transcriptPath));

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
        RunContext run, HostSettings settings, FullWorkflowSettings fw, string repoRoot, string transcriptText, string transcriptSourceName)
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
            new AgenticSdlc.Host.FullWorkflow.ArchClassify.OperationalClassifyBridgeExecutor(run, repoRoot, classifyOutDir, maxAttempts),
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
            new AgenticSdlc.Host.FullWorkflow.Adr.AdrOperationalBridgeExecutor(run, repoRoot, adrOutDir, maxAttempts),
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
                new PbiUpdateApplyExecutor(run, repoRoot, pbiOutDir)),
            classifyNodes,
            adrNodes,
            new ForwardNodes(
                new BootstrapForwardBridgeExecutor(run, repoRoot),
                new OperationalForwardBridgeExecutor(run, pbiOutDir),
                new SnapshotExecutor(run, repoRoot, fw, githubOutDir, maxAttempts),
                new GithubForwardSeedExecutor(run), new GithubForwardMakerExecutor(fwdFactory, run),
                new GithubForwardGateExecutor(run), new GithubForwardRepairExecutor(fwdFactory, run),
                new GithubForwardHitlFinalizeExecutor(run),
                RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate"),
                // R-47: repository + tokenEnv aus fw durchreichen — sonst fällt ResolveToken(null) auf GITHUB_TEST_TOKEN
                // zurück (falscher Token → 403/404 beim echten Write, obwohl der Snapshot mit fw.TokenEnv liest).
                new GithubForwardApplyExecutor(run, repoRoot, githubOutDir, fw.Repo, fw.TokenEnv)));

        return (workflow, ledgerModel, baselineModel, adjudicationPolicy);
    }

    // H1: Resume-Antwortquellen (CLI-Flags beim `resume` — Muster pipeline-hitl `--accept-all`/`--accept`).
    // R-29: Die Flags gelten für GENAU EIN Gate (das pausierte) — danach verbraucht, damit weitere
    // interactive-Gates wieder sauber pausieren (pipeline-hitl: „responded"-Semantik). Entscheid-DATEIEN
    // (human-decisions.json/queue.json) sind davon unberührt und wirken an jedem Gate.
    private sealed class ResumeAnswers(bool acceptAll, IReadOnlyList<string> acceptIds)
    {
        private bool _consumed;
        public List<string>? TakeFor(IEnumerable<string> allIds)
        {
            if (_consumed) return null;
            var result = acceptAll ? allIds.ToList() : acceptIds.Count > 0 ? acceptIds.ToList() : null;
            if (result is not null) _consumed = true;
            return result;
        }
        public bool TakeAcceptAll()
        {
            if (_consumed || !acceptAll) return false;
            _consumed = true;
            return true;
        }
    }

    // Gemeinsamer Streaming-Treiber: EIN Stream (run) bzw. Restore (resume, H1) + zentraler Gate-Responder +
    // Output-Ernte + Pause-Mechanik (HitlShell-Muster: Checkpoint mit offenem Gate sichern -> pointer.json -> Exit 6).
    private static async Task<int> RunWorkflowStreamingAsync<TInput>(
        Workflow workflow, TInput input, string streamRunId, RunContext run, FullWorkflowSettings fw,
        CheckpointManager manager, CancellationToken ct,
        CheckpointInfo? restoreFrom = null, ResumeAnswers? answers = null, bool openUi = false, Action<object>? onOutput = null) where TInput : notnull
    {
        var exit = 0;
        string? pausedGate = null;
        CheckpointInfo? pendingCp = null;
        var pauseReady = false;

        await using var handle = restoreFrom is null
            ? await InProcessExecution.RunStreamingAsync(workflow, input, manager, streamRunId).ConfigureAwait(false)
            : await InProcessExecution.OpenStreamingAsync(workflow, manager, streamRunId).ConfigureAwait(false);
        if (restoreFrom is not null)
            await handle.RestoreCheckpointAsync(restoreFrom).ConfigureAwait(false);

        await foreach (var evt in handle.WatchStreamAsync(ct).ConfigureAwait(false))
        {
            if (evt is WorkflowOutputEvent { Data: { } outputData }) onOutput?.Invoke(outputData);
            // H1: Checkpoints mitschneiden; sobald ein interactive-Gate offen ist UND der SuperStep mit dem
            // pending Request gesichert wurde, sauber aussteigen (exakt HitlShell.StartAsync-Mechanik).
            if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
            {
                if (info.Checkpoint is { } cp) pendingCp = cp;
                if (pausedGate is not null && info.HasPendingRequests && pendingCp is not null) pauseReady = true;
            }
            if (pauseReady) break;
            switch (evt)
            {
                case RequestInfoEvent req:
                {
                    // Zentraler Gate-Responder (R-25): Dispatch per PortId, Antwort aus der Policy via GateResponder.
                    var portId = req.Request.PortInfo.PortId;
                    var gatePolicy = fw.GateFor(portId);
                    var answered = false;
                    if (portId == "adjudication-gate" && req.Request.TryGetDataAs<AdjudicationReviewRequest>(out var ar) && ar is not null)
                    {
                        // H2: aktions-typisiertes Gate — eigener Resolver (accept-all = System-Vorschlag-Heuristik;
                        // replay = aufgezeichnete queue.json per ItemId, unmatcht ⇒ reject geloggt).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var recorded = gatePolicy.Kind == GatePolicyKind.Replay ? AdjudicationIo.LoadQueueActions(gatePolicy.ReplayPath) : null;
                            var res = AdjudicationResolver.ResolveActions(ar.Items, gatePolicy, recorded);
                            var actions = res.FilledItems.ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AdjudicationReviewResponse(actions, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = res.AcceptedCount, rejected = res.RejectedCount, unmatched = res.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "arch-classify-gate" && req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRequest>(out var cq) && cq is not null)
                    {
                        // accept-all/replay: Agent-Vorschlag 1:1 uebernehmen (U2-Korrektur ist der interactive Weg).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var accepted = cq.Items.Select(i => new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyProposal(i.ItemId, i.ProposedRoles, i.Rationale, i.ProposedTargets?.Select(t => t.Id).ToList())).ToList();
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewResponse(accepted, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = accepted.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "adr-gate" && req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRequest>(out var aq) && aq is not null)
                    {
                        // accept-all/replay: Agent-Entwuerfe 1:1 uebernehmen (U5-Edit ist der interactive Weg).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var acceptedDrafts = aq.Items.Select(i => i.Draft).ToList();
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewResponse(acceptedDrafts, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = acceptedDrafts.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId is "ingest-gate" or "arch-ingest-gate" && req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, ir.Ops.Select(o => new GateItem(o.IncomingItemId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new IngestionReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "decision-gate" && req.Request.TryGetDataAs<PipelineDecisionReviewRequest>(out var dq) && dq is not null)
                    {
                        // R-14 G1 Governance-Politik (Autor 04.08.): accept-all/replay koennen keine Wahrheits-
                        // Konflikte entscheiden (Outcome/neuer Text waeren ERFUNDEN) -> Experiment-Modi vertagen
                        // ALLE Entscheidungen (defer-all): DECs parken sichtbar, der Block haelt, Replay bleibt konstant.
                        if (gatePolicy.Kind != GatePolicyKind.Interactive)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(DecisionStage.DeferAll(dq, $"author (pipeline-full/{gatePolicy.Kind}/defer-all)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), deferred = dq.Decisions.Count, timestampUtc = DateTime.UtcNow });
                            Console.WriteLine($"[{Cmd}] decision-gate: {dq.Decisions.Count} offene Entscheidung(en) VERTAGT (Politik {gatePolicy.Kind}: Experiment-Modi entscheiden keine Wahrheits-Konflikte).");
                            answered = true;
                        }
                    }
                    else if (portId == "pbi-gate" && req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr) && pr is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, pr.Ops.Select(o => new GateItem(o.OpId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new PbiUpdateReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "cluster-review-gate" && req.Request.TryGetDataAs<ClusterReviewRequest>(out var crq) && crq is not null)
                    {
                        // B3: erstes Gate mit echter Replay-Datei-Anbindung (human-decisions.json per OpId; unmatcht => reject).
                        var r = GateResponder.Resolve(gatePolicy, crq.Ops.Select(o => new GateItem(o.OpId)).ToList(), ClusterReviewReplay.Load(gatePolicy.ReplayPath));
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new ClusterReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, unmatched = r.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "backlog-review-gate" && req.Request.TryGetDataAs<BacklogReviewRequest>(out var brq) && brq is not null)
                    {
                        // B4: EDIT-fähiges Gate — eigener Resolver (accept-all = keine Edits; replay = Entscheide
                        // inkl. EditedPbiJson 1:1; fehlender Entscheid = accept, exakt die CLI-Apply-Semantik).
                        var decisions = BacklogReviewResolver.Resolve(gatePolicy);
                        if (decisions is not null)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new BacklogReviewResponse(decisions, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), decisions = decisions.Count, pbis = brq.PbiIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "github-forward-gate" && req.Request.TryGetDataAs<ForwardReviewRequest>(out var frq) && frq is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, frq.Ops.Select(o => new GateItem(o.OpId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            // Execute-Flag = fw.Execute: Default false => DRY-RUN (kein realer GitHub-Write). Gates sind heilig.
                            await handle.SendResponseAsync(req.Request.CreateResponse(new ForwardReviewResponse(r.AcceptedItemIds, fw.Execute, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, execute = fw.Execute, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    // H1: interactive — erst die Entscheid-Quellen versuchen (human-decisions.json des Gates
                    // aus den bestehenden Review-UIs; beim resume zusätzlich --accept-all/--accept).
                    // inline (Host-Muster A): am Gate STEHEN BLEIBEN, UI optional öffnen, auf Entscheide warten.
                    // Sonst: durable PAUSE (Host-Muster B).
                    if (!answered && gatePolicy.Kind is GatePolicyKind.Interactive or GatePolicyKind.InteractiveInline)
                    {
                        answered = await TryAnswerInteractiveAsync(handle, req, portId, run, fw, answers).ConfigureAwait(false);
                        if (!answered && gatePolicy.Kind == GatePolicyKind.InteractiveInline)
                            answered = await InlineAnswerLoopAsync(handle, req, portId, run, fw, openUi).ConfigureAwait(false);
                        if (!answered)
                        {
                            pausedGate = portId;
                            Console.WriteLine($"[{Cmd}] Gate '{portId}' — pausiere (Checkpoint mit offenem Gate wird gesichert)…");
                        }
                    }
                    if (!answered && pausedGate is null)
                    {
                        Console.Error.WriteLine($"[{Cmd}] Gate '{portId}' unbekannt/nicht parsebar — nicht beantwortbar.");
                        exit = 5;
                    }
                    break;
                }
                case WorkflowOutputEvent { Data: GithubForwardApplyReport far }:
                    Console.WriteLine($"[{Cmd}] PIPELINE FERTIG (Forward): dryRun={far.DryRun} executed={far.Executed} success={far.Success}.");
                    break;
                case WorkflowOutputEvent { Data: PbiUpdateApplyReport rep }:
                    Console.WriteLine($"[{Cmd}] pbi-update applied newPbis={rep.NewPbis.Count} updatedPbis={rep.UpdatedPbis.Count}");
                    break;
                case WorkflowOutputEvent { Data: ProjectStateDocument psd }:
                    Console.WriteLine($"[{Cmd}] 04-delta OK: items={psd.Items.Count} relations={psd.Relations.Count}");
                    break;
                case WorkflowOutputEvent { Data: CoreBootstrapOutput boot }:
                    Console.WriteLine($"[{Cmd}] core-bootstrap OK: coreItems={boot.CoreItems} (requirements={boot.Requirements}) baseline={boot.BaselinePath}");
                    break;
                case WorkflowOutputEvent { Data: ClusterApplyOutput ca }:
                    Console.WriteLine($"[{Cmd}] cluster-apply OK: applied={ca.Applied} skipped={ca.Skipped} clusters={ca.Clusters} gate={(ca.GatePass ? "pass" : "FAIL")}");
                    break;
                case WorkflowOutputEvent { Data: BacklogSeedOutput bs }:
                    Console.WriteLine($"[{Cmd}] BOOTSTRAP FERTIG (Backlog im Core): features+={bs.FeaturesAdded} pbis+={bs.PbisAdded} relations+={bs.RelationsAdded} core {bs.CoreItemsBefore}->{bs.CoreItemsAfter}");
                    break;
                case WorkflowOutputEvent { Data: ForwardSkipped fs }:
                    Console.WriteLine($"[{Cmd}] {fs.Reason}");
                    break;
                case WorkflowOutputEvent { Data: string s }:
                    Console.Error.WriteLine($"[{Cmd}] {s}");
                    exit = 4;
                    break;
                case ExecutorFailedEvent failed:
                    Console.Error.WriteLine($"[{Cmd}] executor failed: {failed.ExecutorId} {failed.Data?.Message}");
                    exit = 3;
                    break;
                case WorkflowErrorEvent err:
                    Console.Error.WriteLine($"[{Cmd}] workflow error: {err.Exception?.Message}");
                    exit = 3;
                    break;
            }
        }

        // H1: PAUSE materialisieren — pointer.json (HitlShell-Format, Mode = Gate) + klare Weiter-Anleitung.
        if (pausedGate is not null)
        {
            if (pendingCp is null)
            {
                Console.Error.WriteLine($"[{Cmd}] PAUSE fehlgeschlagen: kein Checkpoint mit offenem Gate erzeugt.");
                return 4;
            }
            var checkpointDir = run.OutputDir("checkpoints");
            await HitlShell.WritePointerAsync(checkpointDir,
                new HitlPointer(run.RunId, pendingCp.SessionId, pendingCp.CheckpointId, pausedGate, null, null, DateTime.UtcNow)).ConfigureAwait(false);
            run.AppendEvent(new { type = "PIPELINE_PAUSED", gate = pausedGate, checkpointId = pendingCp.CheckpointId, timestampUtc = DateTime.UtcNow });
            Console.WriteLine($"[{Cmd}] PAUSIERT am Gate '{pausedGate}'. checkpointId={pendingCp.CheckpointId}");
            foreach (var line in ReviewHints(pausedGate, run)) Console.WriteLine(line);
            Console.WriteLine($"[{Cmd}] Fortsetzen: pipeline-full resume {run.RunId}   (oder: … resume {run.RunId} --accept-all)");
            return 6;
        }
        return exit;
    }

    // H1: Gate-spezifische Review-Anleitung für die Pause (bestehende UIs zeigen auf die Faden-Ordner).
    // A3 (07.08.): die Gate→Anleitung-Quelle wohnt jetzt im geteilten Status-Kern (PipelineRunStatusReader)
    // — Pause-Meldung, status-CLI und Steward-Tools sprechen damit IDENTISCH. Hier nur Delegation.
    private static IReadOnlyList<string> ReviewHints(string gate, RunContext run)
        => PipelineRunStatusReader.NextRequiredAction(gate, run.RunId);

    // H1: interactive-Antwortquellen je Gate — 1) human-decisions.json (aus den bestehenden Review-UIs, im
    // Faden-Ordner des Gates), 2) Resume-CLI-Flags. Liefert false => Pause. Semantik je Gate = exakt die
    // jeweilige Apply-/Replay-Semantik (cluster: apply-OpIds; backlog: Entscheide inkl. Edits; sonst accept-Ids).
    private static async Task<bool> TryAnswerInteractiveAsync(
        StreamingRun handle, RequestInfoEvent req, string portId, RunContext run, FullWorkflowSettings fw, ResumeAnswers? answers)
    {
        // R-29: Flags einmalig (TakeFor/TakeAcceptAll konsumieren) — Entscheid-Dateien bleiben mehrfach nutzbar.
        List<string>? Flags(IEnumerable<string> all) => answers?.TakeFor(all);

        async Task<bool> AnswerAsync<T>(T payload, object evtFields) where T : notnull
        {
            await handle.SendResponseAsync(req.Request.CreateResponse(payload)).ConfigureAwait(false);
            run.AppendEvent(evtFields);
            return true;
        }

        switch (portId)
        {
            case "adjudication-gate" when req.Request.TryGetDataAs<AdjudicationReviewRequest>(out var a) && a is not null:
            {
                // H2 interactive: die vom Menschen/UI gefüllte queue.json (ledger-adjudicate-ui <QueuePath>)
                // ist die Entscheid-Quelle; --accept-all = System-Vorschlag-Heuristik (EXPERIMENT).
                var recorded = AdjudicationIo.LoadQueueActions(a.QueuePath);
                AdjudicationActionResolution? res = null;
                if (recorded is not null)
                    res = AdjudicationResolver.ResolveActions(a.Items, new GatePolicy(GatePolicyKind.Replay, a.QueuePath), recorded);
                else if (answers?.TakeAcceptAll() == true)
                    res = AdjudicationResolver.ResolveActions(a.Items, new GatePolicy(GatePolicyKind.AcceptAll));
                if (res is null) return false;
                var actions = res.FilledItems.ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
                return await AnswerAsync(new AdjudicationReviewResponse(actions, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = recorded is not null ? "queue.json" : "cli-flags", accepted = res.AcceptedCount, rejected = res.RejectedCount, unmatched = res.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "cluster-review-gate" when req.Request.TryGetDataAs<ClusterReviewRequest>(out var c) && c is not null:
            {
                var map = ClusterReviewReplay.Load(Path.Combine(run.RunDir, "06-backlog", "clusters", "human-decisions.json"));
                var accepted = map is not null
                    ? c.Ops.Where(o => map.TryGetValue(o.OpId, out var apply) && apply).Select(o => o.OpId).ToList()
                    : Flags(c.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                return await AnswerAsync(new ClusterReviewResponse(accepted, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = map is not null ? "human-decisions" : "cli-flags", accepted = accepted.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "backlog-review-gate" when req.Request.TryGetDataAs<BacklogReviewRequest>(out var b) && b is not null:
            {
                var decisions = BacklogReviewResolver.LoadDecisions(Path.Combine(run.RunDir, "06-backlog", "backlog", "human-decisions.json"));
                if (decisions is null && answers?.TakeAcceptAll() == true) decisions = []; // leere Liste = alle accept (CLI-Semantik)
                if (decisions is null) return false;
                return await AnswerAsync(new BacklogReviewResponse(decisions, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", decisions = decisions.Count, pbis = b.PbiIds.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "arch-classify-gate" when req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRequest>(out var cq) && cq is not null:
            {
                // interactive: 1) classify-decisions.json (U2-Edit-UI, A2-3) · 2) --accept-all (Vorschlag 1:1) · sonst Pause.
                var decisionsPath = Path.Combine(run.RunDir, "07-arch-classify", "classify-decisions.json");
                if (File.Exists(decisionsPath))
                {
                    var file = JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyDecisionsFile>(
                        await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), HitlShell.Json);
                    if (file is not null)
                        return await AnswerAsync(AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewAdapter.ToResponse(file, "human (review-ui)"),
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = "classify-decisions", accepted = file.Decisions.Count, deferred = cq.Items.Count - file.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                }
                if (answers?.TakeAcceptAll() != true) return false;
                var acc = cq.Items.Select(i => new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyProposal(i.ItemId, i.ProposedRoles, i.Rationale, i.ProposedTargets?.Select(t => t.Id).ToList())).ToList();
                return await AnswerAsync(new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewResponse(acc, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = acc.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "adr-gate" when req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRequest>(out var aq) && aq is not null:
            {
                // interactive: 1) adr-decisions.json (U5-Abnahme-UI) · 2) --accept-all (Entwuerfe 1:1) · sonst Pause.
                var adrDecisionsPath = Path.Combine(run.RunDir, "07-adr", "adr-decisions.json");
                if (File.Exists(adrDecisionsPath))
                {
                    var file = JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Adr.AdrDecisionsFile>(
                        await File.ReadAllTextAsync(adrDecisionsPath).ConfigureAwait(false), HitlShell.Json);
                    if (file is not null)
                        return await AnswerAsync(AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewAdapter.ToResponse(file, "human (review-ui)"),
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = "adr-decisions", accepted = file.Decisions.Count, deferred = aq.Items.Count - file.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                }
                if (answers?.TakeAcceptAll() != true) return false;
                var accAdr = aq.Items.Select(i => i.Draft).ToList();
                return await AnswerAsync(new AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewResponse(accAdr, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accAdr.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "ingest-gate" or "arch-ingest-gate" when req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null:
            {
                // R-44 (09.08.): Datei-Vertrag zuerst (UI/Steward-Chat schreiben ingest-gate-decisions.json) —
                // vorher war Tor 1 beim resume NUR per accept-all beantwortbar (kein per-Item-Entscheid).
                var stageDir = string.Equals(portId, "arch-ingest-gate", StringComparison.Ordinal) ? "07-arch-ingest" : "07-ingest";
                var fileDecision = Ingestion.IngestGateDecisions.TryLoadAny(Path.Combine(run.RunDir, stageDir));   // 3b-2: Chat-Vertrag ODER UI-Datei
                var accepted = fileDecision?.Accepted ?? Flags(ir.Ops.Select(o => o.IncomingItemId));
                if (accepted is null) return false;
                return await AnswerAsync(new IngestionReviewResponse(accepted, fileDecision?.Reviewer ?? "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, source = fileDecision is null ? "flags" : "decisions-file", timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "decision-gate" when req.Request.TryGetDataAs<PipelineDecisionReviewRequest>(out var dq) && dq is not null:
            {
                // G1-b: die UI (decision-gate-review) schreibt decision-gate-decisions.json — die ist die Antwort
                // (DECs ohne Eintrag werden sicher VERTAGT). Ohne Datei: Flags koennen keine Outcomes erfinden ->
                // --accept-all/'a' = laut defer-all; sonst warten (Inline-Loop/UI).
                var decisionsPath = Path.Combine(run.RunDir, "07-decision", "decision-gate-decisions.json");
                if (File.Exists(decisionsPath))
                {
                    var file = JsonSerializer.Deserialize<Decision.PipelineDecisionDecisionsFile>(
                        await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), JsonFiles.Json);
                    if (file is not null)
                    {
                        var response = Decision.PipelineDecisionReviewAdapter.ToResponse(file, dq);
                        var resolvedCount = response.Resolutions.Count(r => string.Equals(r.Action, DecisionStage.ActionResolve, StringComparison.OrdinalIgnoreCase));
                        return await AnswerAsync(response,
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", resolved = resolvedCount, deferred = response.Resolutions.Count - resolvedCount, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                    }
                }
                if (answers is null) return false;   // warten: UI nutzen (decision-gate-review) und erneut pruefen
                Console.WriteLine($"[{Cmd}] decision-gate: {dq.Decisions.Count} offene Entscheidung(en) — Flags koennen nicht aufloesen, alle VERTAGT (UI: decision-gate-review).");
                return await AnswerAsync(DecisionStage.DeferAll(dq, "author (resume/defer-all)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", deferred = dq.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "pbi-gate" when req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr) && pr is not null:
            {
                // R-44b (09.08.): Datei-Vertrag zuerst — die pbi-update-Review-UI kann auf 07-pbi-update zeigen
                // (pbi-change-plan.json liegt dort) und schreibt human-decisions.json; vorher nur Flags.
                IReadOnlyList<string>? accepted = null; var reviewer = "author (interactive)";
                IReadOnlyList<PbiAlignment>? aligns = null;   // R-26-C-Fix: akzeptierte Angleichungen MITFÜHREN (sonst fällt needs_clarify im Graphen nie)
                var pbiStage = Path.Combine(run.RunDir, "07-pbi-update");
                var pbiDecisionsPath = Path.Combine(pbiStage, "human-decisions.json");
                if (File.Exists(pbiDecisionsPath) && File.Exists(Path.Combine(pbiStage, "pbi-change-plan.json")))
                {
                    var stagePlan = JsonSerializer.Deserialize<PbiUpdate.PbiStateChangePlanDocument>(
                        await File.ReadAllTextAsync(Path.Combine(pbiStage, "pbi-change-plan.json")).ConfigureAwait(false), JsonFiles.Json)!;
                    var df = JsonSerializer.Deserialize<PbiUpdate.PbiUpdateDecisionsFile>(
                        await File.ReadAllTextAsync(pbiDecisionsPath).ConfigureAwait(false), JsonFiles.Json)!;
                    (accepted, aligns, reviewer) = ResolvePbiGateDecisions(stagePlan, df);
                }
                accepted ??= Flags(pr.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                return await AnswerAsync(new PbiUpdateReviewResponse(accepted, reviewer, aligns),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, alignments = aligns?.Count ?? 0, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "github-forward-gate" when req.Request.TryGetDataAs<ForwardReviewRequest>(out var f) && f is not null:
            {
                var accepted = LoadForwardDecisions(Path.Combine(run.RunDir, "07-github", "github-forward-decisions.json"))
                               ?? Flags(f.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                // Execute-Flag bleibt Policy-gebunden (fw.Execute) — auch interactive kein Write ohne execute=true.
                return await AnswerAsync(new ForwardReviewResponse(accepted, fw.Execute, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, execute = fw.Execute, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
        }
        return false;
    }

    // interactive-inline (Host-Muster A): am Gate stehen bleiben, UI optional öffnen, auf Entscheide warten.
    // ENTER = Entscheid-Datei erneut prüfen; 'a'+ENTER = accept-all (einmalig); stdin-EOF (kein Terminal,
    // z. B. Hintergrund-Lauf) => false => sauberer Fallback auf die durable Pause.
    private static async Task<bool> InlineAnswerLoopAsync(
        StreamingRun handle, RequestInfoEvent req, string portId, RunContext run, FullWorkflowSettings fw, bool openUi)
    {
        Console.WriteLine($"[{Cmd}] Gate '{portId}' interactive-inline — der Lauf WARTET auf deine Entscheide:");
        foreach (var line in ReviewHints(portId, run)) Console.WriteLine(line);
        if (openUi) StartUi(portId, run);

        while (true)
        {
            Console.Write($"[{Cmd}] Entscheide gespeichert? ENTER = prüfen · 'a'+ENTER = accept-all · 'p'+ENTER = pausieren: ");
            var input = Console.ReadLine();
            if (input is null) // kein Terminal (EOF) -> durable Pause statt Endlosschleife
            {
                Console.WriteLine($"[{Cmd}] Kein Terminal (stdin EOF) — falle auf durable Pause zurück.");
                return false;
            }
            if (input.Trim().Equals("p", StringComparison.OrdinalIgnoreCase)) return false;

            var oneShot = input.Trim().Equals("a", StringComparison.OrdinalIgnoreCase)
                ? new ResumeAnswers(acceptAll: true, acceptIds: [])
                : null;
            if (await TryAnswerInteractiveAsync(handle, req, portId, run, fw, oneShot).ConfigureAwait(false))
                return true;
            Console.WriteLine($"[{Cmd}] Noch keine Entscheide gefunden (Datei fehlt/leer) — UI nutzen und erneut ENTER.");
        }
    }

    // UI-Prozess für das Gate starten (eigene Host-Instanz, gleiche Binary). Best effort — Fehler nur melden.
    private static void StartUi(string gate, RunContext run)
    {
        var uiArgs = gate switch
        {
            "adjudication-gate" => $"ledger-adjudicate-ui runs/fullworkflow/{run.RunId}/01-ledger/queue.json",
            "cluster-review-gate" => $"l4-re-clarify-review runs/fullworkflow/{run.RunId}/06-backlog/clusters",
            "backlog-review-gate" => $"l4-re-clarify-backlog-review runs/fullworkflow/{run.RunId}/06-backlog/backlog",
            "decision-gate" => $"decision-gate-review runs/fullworkflow/{run.RunId}/07-decision",
            _ => null,
        };
        if (uiArgs is null)
        {
            Console.WriteLine($"[{Cmd}] --open-ui: für '{gate}' gibt es (noch) keine direkte UI — Entscheid per Datei/Flags.");
            return;
        }
        try
        {
            var repoRoot = Path.GetFullPath(Path.Combine(run.RunDir, "..", "..", ".."));
            var psi = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!, uiArgs)
            {
                WorkingDirectory = repoRoot,
                UseShellExecute = false,
            };
            System.Diagnostics.Process.Start(psi);
            Console.WriteLine($"[{Cmd}] --open-ui: gestartet: {Path.GetFileName(Environment.ProcessPath!)} {uiArgs}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{Cmd}] --open-ui fehlgeschlagen ({ex.Message}) — UI manuell starten: {uiArgs}");
        }
    }

    // Entscheide der Forward-Review (bekanntes GithubForwardDecisionsFile-Format) → akzeptierte OpIds.
    private static List<string>? LoadForwardDecisions(string path)
    {
        if (!File.Exists(path)) return null;
        var file = JsonSerializer.Deserialize<GithubForwardDecisionsFile>(File.ReadAllText(path), HitlShell.Json);
        return file?.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.OpId).ToList();
    }
    // U2: Betriebs-Zweig (Ingest->Pbi) und Forward sind KEIN Runner-Code mehr — sie leben als Kanten in
    // PipelineFullWorkflow.Assemble (IngestBridge/ForwardBridges/SnapshotExecutor). Historie:
    // RunBackHalfAsync + RunForwardAsync (Phasen 2/3) — ersetzt am 27.07. (ein-graph-vereinheitlichung U2).

    // H1: `status` — die EINE Antwort auf „wo ist der letzte Stand?". Quelle der Wahrheit: pointer.json
    // (existiert NUR bei pausiertem Lauf; nach Abschluss gelöscht) + die PIPELINE_*-Events in logs/events.jsonl.
    private static async Task<int> StatusAsync(string[] args, string repoRoot)
    {
        var root = Path.Combine(repoRoot, "runs", "fullworkflow");
        var runToken = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        var dirs = runToken is not null
            ? new[] { Path.IsPathRooted(runToken) ? runToken : Path.Combine(root, runToken) }
            : Directory.Exists(root) ? Directory.GetDirectories(root).OrderBy(d => d, StringComparer.Ordinal).ToArray() : [];

        // A3 (07.08.): status ist nur noch RENDERER über dem geteilten Kern (PipelineRunStatusReader) —
        // dieselbe typisierte Wahrheit, die auch die Steward-Tools (get_run_status) bekommen.
        var paused = 0;
        foreach (var dir in dirs)
        {
            var st = await PipelineRunStatusReader.ReadAsync(repoRoot, dir).ConfigureAwait(false);
            if (st is null) { Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {dir}"); return 2; }
            if (st.State == PipelineRunState.Paused)
            {
                paused++;
                Console.WriteLine($"[{Cmd}] PAUSIERT  {st.RunId}  gate={st.PausedGate}  seit={st.PausedSinceUtc:u}  checkpointId={st.CheckpointId}");
                foreach (var line in st.NextRequiredAction) Console.WriteLine(line);
            }
            else if (runToken is not null)
            {
                Console.WriteLine($"[{Cmd}] {st.RunId}: kein pointer.json — {(st.State == PipelineRunState.Finished ? "FERTIG (metrics.json vorhanden)" : "NICHT pausiert")}. Letztes Pipeline-Event:");
                Console.WriteLine($"  {st.LastPipelineEvent ?? "(keine events.jsonl)"}");
            }
        }
        if (runToken is null)
            Console.WriteLine(paused == 0
                ? $"[{Cmd}] Keine pausierten Läufe unter runs/fullworkflow/."
                : $"[{Cmd}] {paused} pausierte(r) Lauf/Läufe.");

        // R-14 D4: der Parkplatz ist IMMER sichtbar („unsichtbar = rottet") + Kangal-Integritätszeile (read-only) —
        // status ist die eine Seite für „wie geht es der Wahrheit": fachlich Offenes getrennt von strukturell Lautem.
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            var core = await coreRepo.LoadAsync().ConfigureAwait(false);
            Console.WriteLine($"[{Cmd}] Core ({core.Items.Count} Items):");
            foreach (var line in CoreParkplatz.RenderLines(CoreParkplatz.Count(core), CoreKangal.Check(core)))
                Console.WriteLine(line);
        }
        return 0;
    }

    private static async Task<int> StartAsync(string[] args, string repoRoot)
    {
        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);

        // Transkript: CLI-Arg > run-config.fullworkflow.transcript. Im Skeleton optional (nur validiert, nicht gelesen).
        var transcriptArg = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : fw.Transcript;
        string? transcriptPath = null;
        if (!string.IsNullOrWhiteSpace(transcriptArg))
        {
            transcriptPath = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, transcriptArg);
            if (!File.Exists(transcriptPath))
            {
                Console.Error.WriteLine($"[{Cmd}] Transkript nicht gefunden: {transcriptPath}");
                return 2;
            }
        }

        var run = new RunContext(RunId.New(), "fullworkflow");
        run.EnsureFolders();
        var plan = Plan(fw);

        // Faden-Unterordner (distinct) + checkpoints — Design-Note §7.
        foreach (var folder in plan.Select(p => p.Folder).Distinct(StringComparer.Ordinal))
            run.OutputDir(folder);
        run.OutputDir("checkpoints");

        run.WriteConfig(new
        {
            command = Cmd,
            runId = run.RunId,
            transcript = transcriptPath is null ? null : Path.GetRelativePath(repoRoot, transcriptPath),
            execute = fw.Execute,
            policyProfile = fw.PolicyProfile.Kind.ToString(),
            l3Enabled = fw.L3Enabled,
            models = fw.Models,
            maxAttempts = fw.MaxAttempts,
            skeleton = true,
            timestampUtc = DateTime.UtcNow
        });

        run.AppendEvent(new { type = "PIPELINE_START", runId = run.RunId, mode = "skeleton", execute = fw.Execute, stages = plan.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] start runId={run.RunId} mode=SKELETON execute={fw.Execute} policyProfile={fw.PolicyProfile.Kind} l3={(fw.L3Enabled ? "on" : "off")}");
        Console.WriteLine($"[{Cmd}] Flow ({plan.Count} Stufen):");
        for (var i = 0; i < plan.Count; i++)
        {
            var st = plan[i];
            var isDryRunGithub = st.Id == "07-github" && !fw.Execute;
            var gateStr = st.Gate is null ? "" : $"  Gate={st.Gate} policy={fw.GateFor(st.Gate).Kind}";
            var dryStr = isDryRunGithub ? "  (DRY-RUN)" : "";
            run.AppendEvent(new
            {
                type = "STAGE_PLANNED",
                index = i + 1,
                stage = st.Id,
                folder = st.Folder,
                gate = st.Gate,
                policy = st.Gate is null ? null : fw.GateFor(st.Gate).Kind.ToString(),
                dryRun = isDryRunGithub,
                timestampUtc = DateTime.UtcNow
            });
            Console.WriteLine($"  [{i + 1}/{plan.Count}] {st.Id}{gateStr}{dryStr}");
        }
        run.AppendEvent(new { type = "PIPELINE_SKELETON_DONE", note = "Graph-Ausfuehrung folgt Bauplan-Schritt 3-4", timestampUtc = DateTime.UtcNow });

        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{run.RunId}/  (Ordner + config.json + events.jsonl; KEIN LLM, KEIN Write)");
        await Task.CompletedTask.ConfigureAwait(false);
        return 0;
    }

    // H1: Resume eines PAUSIERTEN Laufs — Checkpoint restaurieren (identischer Graph via BuildGraph), das
    // re-emittierte Gate beantworten (human-decisions.json des Gates ODER --accept-all/--accept id1,id2),
    // weiterfahren; ggf. am NÄCHSTEN Gate erneut pausieren. Muster: pipeline-hitl (bewiesen) — durch den
    // Ein-Graph gibt es genau EINEN Restore-Punkt, kein Phasen-Pointer.
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        string? runToken = null, acceptList = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--accept-all", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--open-ui", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--accept", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { acceptList = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[{Cmd}] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var runDir = Path.IsPathRooted(runToken) ? runToken : Path.Combine(repoRoot, "runs", "fullworkflow", runToken);
        if (!Directory.Exists(runDir)) { Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {runDir}"); return 2; }
        var runId = Path.GetFileName(runDir.TrimEnd('/', '\\'));
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointer = await HitlShell.LoadPointerAsync(Cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);
        var run = new RunContext(runId, "fullworkflow");

        // Faden-Exporter auch beim Resume (nach dem Gate können weitere LLM-Stufen laufen).
        using var fadenOtel = OtelRunExporters.TryCreate(
            settings.OtelEnabled, "AgenticSdlc.Host",
            Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Schritt 5 ② / H1: das Konstruktions-Transkript der Ledger-Kapsel aus der Lauf-config rekonstruieren
        // (identischer Graph!). Delta-Läufe haben keins (Kapsel bleibt unangesprochen). Der Intake-Guard
        // (LEDGER_TRANSCRIPT_MISMATCH) fängt eine zwischenzeitlich editierte Transkript-Datei laut ab.
        var (resumeTranscriptText, resumeTranscriptName) = await LoadRunTranscriptAsync(run, repoRoot).ConfigureAwait(false);
        var (workflow, _, _, _) = BuildGraph(run, settings, fw, repoRoot, resumeTranscriptText, resumeTranscriptName);
        var answers = new ResumeAnswers(acceptAll,
            string.IsNullOrWhiteSpace(acceptList) ? [] : acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(fw.TimeoutMinutes));

        run.AppendEvent(new { type = "PIPELINE_RESUME", runId, gate = pointer.Mode, checkpointId = pointer.CheckpointId, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] resume runId={runId} gate={pointer.Mode ?? "?"} checkpointId={pointer.CheckpointId}");

        var exit = await RunWorkflowStreamingAsync<TranscriptInput>(
            workflow, default!, runId, run, fw, manager, cts.Token,
            restoreFrom: new CheckpointInfo(pointer.SessionId, pointer.CheckpointId), answers: answers,
            openUi: args.Contains("--open-ui")).ConfigureAwait(false);

        if (exit == 6)
        {
            Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{runId}/  (erneut PAUSIERT — nächstes Gate)");
            return 0;
        }

        // Pointer-Lebenszyklus: nach echtem Lauf-Ende den Pause-Zeiger LÖSCHEN — `status` zeigt sonst einen
        // fertigen Lauf als pausiert, und ein versehentliches resume würde ein beantwortetes Gate restaurieren.
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (File.Exists(pointerPath))
        {
            File.Delete(pointerPath);
            run.AppendEvent(new { type = "PIPELINE_POINTER_CLEARED", runId, timestampUtc = DateTime.UtcNow });
        }

        run.AppendEvent(new { type = "PIPELINE_RUN_DONE", runId, exit, timestampUtc = DateTime.UtcNow });
        fadenOtel?.ForceFlush();
        var coreRepo = new JsonCoreRepository(repoRoot);
        var coreAfter = await coreRepo.ExistsAsync().ConfigureAwait(false)
            ? (await coreRepo.LoadAsync().ConfigureAwait(false)).Items.Count : 0;
        await MetricsFinalizer.WriteAsync(run, repoRoot, fw, settings, coreAfter, cts.Token).ConfigureAwait(false);
        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{runId}/  exit={exit}");
        return exit;
    }

    // Schritt 5 ② / H1: Transkript-Recovery fürs Resume — liest den `transcript`-Pfad aus der config.json des
    // Laufs (seit ③ geschrieben) und lädt den Text neu. Delta-Läufe (transcript=null) liefern leer.
    private static async Task<(string Text, string SourceName)> LoadRunTranscriptAsync(RunContext run, string repoRoot)
    {
        if (!File.Exists(run.ConfigPath)) return ("", "");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(run.ConfigPath).ConfigureAwait(false));
        if (!doc.RootElement.TryGetProperty("transcript", out var t) || t.ValueKind != JsonValueKind.String) return ("", "");
        var rel = t.GetString()!;
        var path = Path.IsPathRooted(rel) ? rel : Path.Combine(repoRoot, rel);
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[{Cmd}] WARNUNG: Transkript des Laufs nicht mehr auffindbar: {path} (Kapsel-Konstruktion mit leerem Transkript — Intake-Guard greift, falls die Front noch läuft).");
            return ("", Path.GetFileName(path));
        }
        return (await File.ReadAllTextAsync(path).ConfigureAwait(false), Path.GetFileName(path));
    }
}
