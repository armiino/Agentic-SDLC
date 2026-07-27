using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
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
/// Bootstrap | Betrieb → Snapshot → Forward, 7 Gates) in <see cref="PipelineFullWorkflow.Assemble"/> und
/// startet EINEN Stream — keine Phasen-Sequenzierung mehr im Runner. <c>run --dry-run</c> = LLM-freie
/// Build()-Validierung des Gesamtgraphen. <c>start</c> = Skeleton (Faden-Ordner + Plan-Events, kein LLM);
/// <c>resume</c> ist noch No-op (H1). Gate-Antworten kommen aus dem zentralen Responder
/// (<see cref="RunWorkflowStreamingAsync"/>, Policies interactive|accept-all|replay).
/// </remarks>
public static class PipelineFullRunner
{
    private const string Cmd = "pipeline-full";

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
        stages.Add(new("07-pbi-update", "07-pbi-update", "pbi-gate"));
        stages.Add(new("snapshot",      "07-github",     null));        // Pflicht VOR Forward (R-16)
        stages.Add(new("07-github",     "07-github",     "github-forward-gate"));
        return stages;
    }

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => StartAsync(args, repoRoot),
            "run" => RunGraphAsync(args, settings, repoRoot),
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
        Console.Error.WriteLine("  pipeline-full run --dry-run              (Assemble/Build()-Validierung, kein LLM)");
        Console.Error.WriteLine("  pipeline-full resume <runId> [--accept-all | --accept id1,id2] [--open-ui]  (pausiertes Gate beantworten, weiterfahren)");
        Console.Error.WriteLine("  pipeline-full status [<runId>]           (WO steht was: pausierte Läufe + Gate + Weiter-Kommando)");
        return 2;
    }

    // W1e' Schritt 4 — echter Graph-Lauf (Slice v0: 01-ledger). Beweist, dass pipeline-full einen ECHTEN
    // MAF-Graphen ausführt (nicht mehr Skeleton). Weitere Stufen werden iterativ an Assemble angehängt.
    private static async Task<int> RunGraphAsync(string[] args, HostSettings settings, string repoRoot)
    {
        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);

        var transcriptArg = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : fw.Transcript;
        if (string.IsNullOrWhiteSpace(transcriptArg))
        {
            Console.Error.WriteLine($"[{Cmd}] Transkript fehlt (Arg oder run-config.fullworkflow.transcript).");
            return 2;
        }
        var transcriptPath = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, transcriptArg);
        if (!File.Exists(transcriptPath))
        {
            Console.Error.WriteLine($"[{Cmd}] Transkript nicht gefunden: {transcriptPath}");
            return 2;
        }
        var transcriptText = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);

        var run = new RunContext(RunId.New(), "fullworkflow");
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

        // H1: der Graph-Bau ist eine eigene Funktion — run UND resume bauen den IDENTISCHEN Graph
        // (Voraussetzung für RestoreCheckpointAsync).
        var (workflow, ledgerModel, baselineModel, adjudicationPolicy) = BuildGraph(run, settings, fw, repoRoot);

        run.WriteConfig(new
        {
            command = Cmd,
            mode = "run",
            pipelineMode = fw.Mode.ToString(),
            slice = "ledger-v0",
            runId = run.RunId,
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            execute = fw.Execute,
            policyProfile = fw.PolicyProfile.Kind.ToString(),
            ledgerModel,
            timestampUtc = DateTime.UtcNow
        });

        // U1/U2: LLM-freie Validierung des GANZEN Assemble (Build() lief soeben — Kanten/Typen geprüft).
        if (args.Contains("--dry-run"))
        {
            Console.WriteLine($"[{Cmd}] --dry-run: Ein-Graph Build()-bar (Front -> Branch -> Bootstrap | Betrieb -> Snapshot -> Forward; 7 Gates). Kein LLM.");
            return 0;
        }

        run.AppendEvent(new { type = "PIPELINE_START", runId = run.RunId, mode = "run", slice = "u2-ein-graph", adjudicationPolicy = adjudicationPolicy.Kind.ToString(), timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] run runId={run.RunId} slice=u2 (EIN Graph: front->branch->bootstrap|betrieb->forward) ledgerModel={ledgerModel} baselineModel={baselineModel ?? ledgerModel} adjudication={adjudicationPolicy.Kind} execute={fw.Execute}");

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(run.OutputDir("checkpoints")));
        var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(fw.TimeoutMinutes));

        var exit = await RunWorkflowStreamingAsync<TranscriptInput>(
            workflow, new TranscriptInput(transcriptPath, transcriptText), run.RunId, run, fw, manager, cts.Token,
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
    private static (Workflow Workflow, string LedgerModel, string? BaselineModel, GatePolicy AdjudicationPolicy) BuildGraph(
        RunContext run, HostSettings settings, FullWorkflowSettings fw, string repoRoot)
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

        var ingestFactory = PipelineComposedRunner.AgentFactory(repoRoot, settings, judgeSettings, run, "RequirementIngestionAgent", "RequirementIngestionAgent1");
        var pbiFactory = PipelineComposedRunner.AgentFactory(repoRoot, settings, judgeSettings, run, "PbiPlacementAgent", "PbiPlacementAgent1");
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

        var workflow = PipelineFullWorkflow.Assemble(
            new FrontNodes(
                new LedgerWrapperExecutor(settings, judgeSettings, run),
                new AdjudicationGateRequestExecutor(run),
                RequestPort.Create<AdjudicationReviewRequest, AdjudicationReviewResponse>("adjudication-gate"),
                new AdjudicationApplyExecutor(run),
                new BaselineStageExecutor(settings, baselineModel, run, repoRoot),
                new ProjectStateBuildExecutor(run, repoRoot),
                new BranchDetectorExecutor(run, repoRoot, fw.Mode)),
            new BootstrapNodes(
                new CoreBootstrapStageExecutor(run, repoRoot),
                new ClusterBridgeExecutor(run, repoRoot),
                new ClusterAgentExecutor(makerFactory, repoRoot, run),
                new ClusterGateExecutor(run),
                new ClusterReviewExecutor(reviewFactory, run),
                new ClusterFinalizeExecutor(run, clustersDir),
                new ClusterGateRequestExecutor(run),
                RequestPort.Create<ClusterReviewRequest, ClusterReviewResponse>("cluster-review-gate"),
                new ClusterComposedApplyExecutor(run, repoRoot, clustersDir),
                new BacklogBridgeExecutor(run, repoRoot),
                new ClarifyAgentExecutor(clarifyFactory, run),
                new BacklogGateExecutor(run),
                new BacklogFinalizeExecutor(run, backlogDir),
                new BacklogGateRequestExecutor(run),
                RequestPort.Create<BacklogReviewRequest, BacklogReviewResponse>("backlog-review-gate"),
                new BacklogComposedApplyExecutor(run, repoRoot, backlogDir),
                new CoreSeedBacklogExecutor(run, repoRoot)),
            new OperationalNodes(
                new IngestBridgeExecutor(run, repoRoot, maxAttempts),
                new IngestionHitlResolveExecutor(ingestFactory, retriever, run), new IngestionGateExecutor(run),
                new IngestionRepairExecutor(ingestFactory, retriever, run),
                new IngestionHitlFinalizeExecutor(run, ingestOutDir),
                RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingest-gate"),
                new IngestComposedApplyExecutor(run, repoRoot, ingestOutDir),
                new IngestPbiBridgeExecutor(run, repoRoot, pbiOutDir, maxAttempts),
                new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(pbiFactory, run), new PbiUpdateGateExecutor(run),
                new PbiUpdateRepairExecutor(pbiFactory, run), new PbiUpdateHitlFinalizeExecutor(run),
                RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-gate"),
                new PbiUpdateApplyExecutor(run, repoRoot, pbiOutDir)),
            new ForwardNodes(
                new BootstrapForwardBridgeExecutor(run, repoRoot),
                new OperationalForwardBridgeExecutor(run, pbiOutDir),
                new SnapshotExecutor(run, repoRoot, fw, githubOutDir, maxAttempts),
                new GithubForwardSeedExecutor(run), new GithubForwardMakerExecutor(fwdFactory, run),
                new GithubForwardGateExecutor(run), new GithubForwardRepairExecutor(fwdFactory, run),
                new GithubForwardHitlFinalizeExecutor(run),
                RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate"),
                new GithubForwardApplyExecutor(run, repoRoot, githubOutDir, null, null)));

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
                    else if (portId == "ingest-gate" && req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, ir.Ops.Select(o => new GateItem(o.IncomingItemId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new IngestionReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, timestampUtc = DateTime.UtcNow });
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
    private static IReadOnlyList<string> ReviewHints(string gate, RunContext run) => gate switch
    {
        "adjudication-gate" =>
            [$"  Review-UI: ledger-adjudicate-ui runs/fullworkflow/{run.RunId}/01-ledger/queue.json  (Aktionen setzen, Autosave)",
             "  oder beim Resume: --accept-all (System-Vorschlag-Heuristik, EXPERIMENT)"],
        "cluster-review-gate" =>
            [$"  Review-UI: l4-re-clarify-review runs/fullworkflow/{run.RunId}/06-backlog/clusters  → human-decisions.json"],
        "backlog-review-gate" =>
            [$"  Review-UI: l4-re-clarify-backlog-review runs/fullworkflow/{run.RunId}/06-backlog/backlog  → human-decisions.json"],
        "github-forward-gate" =>
            [$"  Entscheide: runs/fullworkflow/{run.RunId}/07-github/github-forward-decisions.json (opId/decision apply|skip)",
             "  oder beim Resume: --accept-all | --accept opId1,opId2"],
        _ => ["  Entscheid beim Resume: --accept-all | --accept id1,id2"],
    };

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
            case "ingest-gate" when req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null:
            {
                var accepted = Flags(ir.Ops.Select(o => o.IncomingItemId));
                if (accepted is null) return false;
                return await AnswerAsync(new IngestionReviewResponse(accepted, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "pbi-gate" when req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr) && pr is not null:
            {
                var accepted = Flags(pr.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                return await AnswerAsync(new PbiUpdateReviewResponse(accepted, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
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

        var paused = 0;
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir)) { Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {dir}"); return 2; }
            var runId = Path.GetFileName(dir.TrimEnd('/', '\\'));
            var pointerPath = Path.Combine(dir, "checkpoints", "pointer.json");
            if (File.Exists(pointerPath))
            {
                var p = await HitlShell.LoadAsync<HitlPointer>(pointerPath).ConfigureAwait(false);
                paused++;
                Console.WriteLine($"[{Cmd}] PAUSIERT  {runId}  gate={p.Mode ?? "?"}  seit={p.SavedUtc:u}  checkpointId={p.CheckpointId}");
                foreach (var line in ReviewHints(p.Mode ?? "?", new RunContext(runId, "fullworkflow"))) Console.WriteLine(line);
                Console.WriteLine($"  Weiter: pipeline-full resume {runId}   (oder … --accept-all)");
            }
            else if (runToken is not null)
            {
                // Einzelabfrage ohne Pointer: letzten Lauf-Stand aus den Events zeigen.
                var eventsPath = Path.Combine(dir, "logs", "events.jsonl");
                var last = File.Exists(eventsPath)
                    ? File.ReadLines(eventsPath).LastOrDefault(l => l.Contains("\"PIPELINE_", StringComparison.Ordinal))
                    : null;
                Console.WriteLine($"[{Cmd}] {runId}: kein pointer.json — NICHT pausiert. Letztes Pipeline-Event:");
                Console.WriteLine($"  {last ?? "(keine events.jsonl)"}");
            }
        }
        if (runToken is null)
            Console.WriteLine(paused == 0
                ? $"[{Cmd}] Keine pausierten Läufe unter runs/fullworkflow/."
                : $"[{Cmd}] {paused} pausierte(r) Lauf/Läufe.");
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

        var (workflow, _, _, _) = BuildGraph(run, settings, fw, repoRoot);
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
}
