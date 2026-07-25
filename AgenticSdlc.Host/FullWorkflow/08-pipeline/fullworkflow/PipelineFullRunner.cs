using System.Text.Json;
using AgenticSdlc.Host.Configuration;
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
/// Bauplan-Schritt 2 (Run-Faden + minimale Shell): <c>start</c> liest <c>run-config.fullworkflow</c>, legt den
/// Faden-Ordner + Unterordner + config-Snapshot an und schreibt den Stufen-Plan als Events (SKELETON, KEIN LLM,
/// KEIN externer Write). Der eigentliche Graph-Bau + Gate-Responder folgen in Schritt 3-4; <c>resume</c> ist
/// bis dahin ein bewusster No-op. Der Stufen-Plan (<see cref="Plan"/>) ist die EINE Quelle für Ordnernamen,
/// Events und die Konsolen-Flow-Leiste.
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
            "resume" => ResumeAsync(args, repoRoot),
            _ => Task.FromResult(Usage()),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  pipeline-full start [<transcript.txt>]   (Faden + Skeleton-Plan, kein LLM)");
        Console.Error.WriteLine("  pipeline-full run   [<transcript.txt>]   (echter Graph-Lauf; Slice v0 = 01-ledger)");
        Console.Error.WriteLine("  pipeline-full resume <runId>             (Resume folgt mit den Gates)");
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

        // Stufen-Modell: fullworkflow.models["01-ledger"] > jury.judgeModel > default.
        var ledgerModel = fw.Models.TryGetValue("01-ledger", out var m) && !string.IsNullOrWhiteSpace(m) ? m
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings.JuryJudgeModel!
            : settings.ModelId;
        var judgeSettings = settings with { ModelId = ledgerModel };

        run.WriteConfig(new
        {
            command = Cmd,
            mode = "run",
            slice = "ledger-v0",
            runId = run.RunId,
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            execute = fw.Execute,
            policyProfile = fw.PolicyProfile.Kind.ToString(),
            ledgerModel,
            timestampUtc = DateTime.UtcNow
        });

        var adjudicationPolicy = fw.GateFor("adjudication-gate");
        var baselineModel = fw.Models.TryGetValue("02-baselines", out var bm) && !string.IsNullOrWhiteSpace(bm) ? bm : null;

        var ledger = new LedgerWrapperExecutor(settings, judgeSettings, run);
        var adjudication = new AdjudicationStageExecutor(run, adjudicationPolicy);
        var baselines = new BaselineStageExecutor(settings, baselineModel, run, repoRoot);
        var delta = new ProjectStateBuildExecutor(run, repoRoot);
        var workflow = PipelineFullWorkflow.Assemble(ledger, adjudication, baselines, delta);

        run.AppendEvent(new { type = "PIPELINE_START", runId = run.RunId, mode = "run", slice = "v3b-ledger..pbi", adjudicationPolicy = adjudicationPolicy.Kind.ToString(), timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] run runId={run.RunId} slice=v3b (ledger->adjudikation->baselines->delta ->[ingest]->[pbi]) ledgerModel={ledgerModel} baselineModel={baselineModel ?? ledgerModel} adjudication={adjudicationPolicy.Kind} execute={fw.Execute}");

        // W1e' Schritt 4 (Slice v3b): STREAMING + zentraler Gate-Responder (R-25) + Durability, in ZWEI Phasen.
        // Phase 1 = Vorderhälfte (kein Core-Write). Phase 2 = Hinterhälfte (Ingest/Pbi) MIT RequestPort-Gates,
        // die der Responder unbeaufsichtigt beantwortet — MUTIERT den Core (nach Freigabe).
        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(run.OutputDir("checkpoints")));
        var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(20));

        var exit = await RunWorkflowStreamingAsync(workflow, new TranscriptInput(transcriptPath, transcriptText), run.RunId, run, fw, manager, cts.Token).ConfigureAwait(false);
        if (exit == 0)
            exit = await RunBackHalfAsync(run, settings, judgeSettings, fw, repoRoot, manager, cts.Token).ConfigureAwait(false);
        if (exit == 0)
            exit = await RunForwardAsync(run, settings, judgeSettings, fw, repoRoot, manager, cts.Token).ConfigureAwait(false);

        run.AppendEvent(new { type = "PIPELINE_RUN_DONE", runId = run.RunId, exit, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{run.RunId}/  exit={exit}");
        return exit;
    }

    // Gemeinsamer Streaming-Treiber (Phase 1 + 2): RunStreamingAsync + zentraler Gate-Responder + Output-Ernte.
    private static async Task<int> RunWorkflowStreamingAsync<TInput>(
        Workflow workflow, TInput input, string streamRunId, RunContext run, FullWorkflowSettings fw,
        CheckpointManager manager, CancellationToken ct) where TInput : notnull
    {
        var exit = 0;
        await using var handle = await InProcessExecution.RunStreamingAsync(workflow, input, manager, streamRunId).ConfigureAwait(false);
        await foreach (var evt in handle.WatchStreamAsync(ct).ConfigureAwait(false))
        {
            switch (evt)
            {
                case RequestInfoEvent req:
                {
                    // Zentraler Gate-Responder (R-25): Dispatch per PortId, Antwort aus der Policy via GateResponder.
                    var portId = req.Request.PortInfo.PortId;
                    var gatePolicy = fw.GateFor(portId);
                    var answered = false;
                    if (portId == "ingest-gate" && req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null)
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
                    if (!answered)
                    {
                        Console.Error.WriteLine($"[{Cmd}] Gate '{portId}' interactive/unbekannt — unbeaufsichtigt nicht beantwortbar (Pause-Resume folgt).");
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
        return exit;
    }

    // Hinterhälfte (Phase 2): Delta -> Ingest -> Pbi über die bestehende PipelineComposed-Komposition (Reuse).
    // MUTIERT den Core (nach Gate-Freigabe) — Testläufe deshalb mit Core-Backup/Restore fahren.
    private static async Task<int> RunBackHalfAsync(
        RunContext run, HostSettings settings, HostSettings genSettings, FullWorkflowSettings fw, string repoRoot,
        CheckpointManager manager, CancellationToken ct)
    {
        var deltaPath = Path.Combine(run.OutputDir("04-delta"), "project-state.json");
        if (!File.Exists(deltaPath)) { Console.Error.WriteLine($"[{Cmd}] Delta fehlt für Hinterhälfte."); return 3; }
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine($"[{Cmd}] Core fehlt — Hinterhälfte übersprungen."); return 0; }

        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath, ct).ConfigureAwait(false)).Document;
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);
        const int maxAttempts = 2;
        var ingestOutDir = run.OutputDir("07-ingest");
        var pbiOutDir = run.OutputDir("07-pbi-update");

        var ingestFactory = PipelineComposedRunner.AgentFactory(repoRoot, settings, genSettings, run, "RequirementIngestionAgent", "RequirementIngestionAgent1");
        var pbiFactory = PipelineComposedRunner.AgentFactory(repoRoot, settings, genSettings, run, "PbiPlacementAgent", "PbiPlacementAgent1");
        var backWorkflow = PipelineComposedRunner.BuildWorkflow(run, repoRoot, ingestOutDir, pbiOutDir, maxAttempts, ingestFactory, pbiFactory);

        run.AppendEvent(new { type = "STAGE_BACKHALF_START", coreItems = core.Items.Count, deltaItems = delta.Items.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] Hinterhälfte: Ingest -> [ingest-gate] -> Apply -> Bridge -> PbiUpdate -> [pbi-gate] -> Apply (Core: {core.Items.Count} items)");

        var input = new IngestionResolveInput(delta, core, Path.GetRelativePath(repoRoot, deltaPath), maxAttempts);
        return await RunWorkflowStreamingAsync(backWorkflow, input, run.RunId + "-back", run, fw, manager, ct).ConfigureAwait(false);
    }

    // Forward (Phase 3): GitHub-Sync als DRY-RUN (Default). Reuse der komponierbaren GithubForwardHitl-Komposition.
    // Snapshot bewusst LEER (unmapped PBIs -> nur CREATE) → kein GitHub-Token für den Dry-Run nötig; realer Write
    // NUR bei fw.Execute=true (Execute-Flag im Gate-Response). Gates sind heilig.
    private static async Task<int> RunForwardAsync(
        RunContext run, HostSettings settings, HostSettings genSettings, FullWorkflowSettings fw, string repoRoot,
        CheckpointManager manager, CancellationToken ct)
    {
        var deltaPath = Path.Combine(run.OutputDir("07-pbi-update"), "applied", "github-sync-delta.json");
        if (!File.Exists(deltaPath))
        {
            Console.WriteLine($"[{Cmd}] Kein github-sync-delta -> Forward übersprungen (nichts zu synchronisieren).");
            return 0;
        }
        var delta = await HitlShell.LoadAsync<GithubSyncDeltaDocument>(deltaPath).ConfigureAwait(false);
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var mappingByPbi = CoreGithubMapping.ByPbi(core);
        var outDir = run.OutputDir("07-github");
        const int maxAttempts = 2;

        // Snapshot-Stufe (Design-Note Knoten 10, R-16): frischer READ-ONLY GET des realen Repos VOR Forward.
        // Ohne repo/token -> leer (Forward flaggt dann korrekt Drift). Kein Write, nur GET.
        IReadOnlyList<GithubIssueSnapshot> issues = [];
        string? snapshotRel = null;
        var token = string.IsNullOrWhiteSpace(fw.TokenEnv) ? null : Environment.GetEnvironmentVariable(fw.TokenEnv!);
        if (!string.IsNullOrWhiteSpace(fw.Repo) && !string.IsNullOrWhiteSpace(token))
        {
            try
            {
                issues = await GithubIssueSnapshotRunner.LoadWithGitHubApiAsync(fw.Repo!, 500, token).ConfigureAwait(false);
                var snapshotPath = Path.Combine(outDir, "github-snapshot.json");
                await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(issues, HitlShell.Json), ct).ConfigureAwait(false);
                snapshotRel = Path.GetRelativePath(repoRoot, snapshotPath);
                run.AppendEvent(new { type = "STAGE_SNAPSHOT_DONE", repository = fw.Repo, issues = issues.Count, timestampUtc = DateTime.UtcNow });
                Console.WriteLine($"[{Cmd}] Snapshot (R-16): {issues.Count} Issues von {fw.Repo} geholt (read-only, kein Write).");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{Cmd}] Snapshot-GET fehlgeschlagen: {ex.Message} -> leerer Snapshot (Forward flaggt evtl. Drift).");
            }
        }
        else
        {
            Console.WriteLine($"[{Cmd}] Kein fullworkflow.repo/tokenEnv (oder Token leer) -> leerer Snapshot; Forward-Gate braucht echten Snapshot (R-16).");
        }

        // Forward ist der anspruchsvollste Agent (jeden Delta-PBI adressieren) → eigenes Modell:
        // fullworkflow.models["07-github"] > default agentModel (nicht das mini der Front-Stufen).
        var fwdModel = fw.Models.TryGetValue("07-github", out var fm) && !string.IsNullOrWhiteSpace(fm) ? fm! : settings.ModelId;
        var fwdSettings = settings with { ModelId = fwdModel };
        var prompt = PromptProvider.Load(repoRoot, "phase2_evidence", "GithubForwardAgent", "GithubForwardAgent1",
            new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(fwdSettings), settings, run, "GithubForwardAgent", "AgenticSdlc.Host");
        Console.WriteLine($"[{Cmd}] Forward-Modell: {fwdModel}");
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: "GithubForwardAgent", tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var humanGate = RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate");
        var workflow = GithubForwardHitlWorkflow.Build(
            new GithubForwardSeedExecutor(run), new GithubForwardMakerExecutor(factory, run), new GithubForwardGateExecutor(run),
            new GithubForwardRepairExecutor(factory, run), new GithubForwardHitlFinalizeExecutor(run),
            humanGate, new GithubForwardApplyExecutor(run, repoRoot, outDir, null, null));

        var dryRun = !fw.Execute;
        // R-26: im unbeaufsichtigten accept-all-Lauf ist KEIN Mensch da, der ein Issue fuer ein neues, unklares PBI
        // autorisiert → solche PBIs parken (HOLD_CLARIFY) statt Auto-CREATE. interactive/replay tragen die
        // Menschen-Autorisierung → Flag aus, Verhalten identisch zum CLI (M-1 Vergleichbarkeit).
        var holdUnclear = fw.GateFor("github-forward-gate").Kind == GatePolicyKind.AcceptAll;
        var ctx = new GithubForwardWfContext(core, delta.Entries, mappingByPbi, issues, fw.Repo, "pipeline-full", outDir, snapshotRel, dryRun, maxAttempts, holdUnclear);
        run.AppendEvent(new { type = "STAGE_FORWARD_START", deltaPbis = delta.Entries.Count, dryRun, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] Forward (DRY-RUN={dryRun}): {delta.Entries.Count} PBI-Ops -> GitHub-Plan (realer Write nur bei execute=true)");

        return await RunWorkflowStreamingAsync(workflow, ctx, run.RunId + "-fwd", run, fw, manager, ct).ConfigureAwait(false);
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

    private static async Task<int> ResumeAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var runToken = args[2];
        var runDir = Path.IsPathRooted(runToken) ? runToken : Path.Combine(repoRoot, "runs", "fullworkflow", runToken);
        if (!Directory.Exists(runDir))
        {
            Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {runDir}");
            return 2;
        }
        Console.WriteLine($"[{Cmd}] resume runId={Path.GetFileName(runDir)} — Graph/Checkpoints noch nicht gebaut (Bauplan-Schritt 4). Skeleton-Resume ist ein No-op.");
        await Task.CompletedTask.ConfigureAwait(false);
        return 0;
    }
}
