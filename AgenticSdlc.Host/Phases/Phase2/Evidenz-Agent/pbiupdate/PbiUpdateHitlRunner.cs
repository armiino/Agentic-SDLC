using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// S4 (Worklist 20.07): pbi-update als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint (Muster wie
// github-forward-hitl, S1/S2). Alter Pfad (pbi-update / -review / -apply) bleibt UNVERAENDERT parallel.
//   pbi-update-hitl start  <ingestion-run> [model] [--dry-run] [--max-attempts n]
//   pbi-update-hitl resume <pbi-update-run> [--ui | --accept-all | --accept op-0,op-2] [--no-browser]
public static class PbiUpdateHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "PbiPlacementAgent";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => await StartAsync(args, settings, repoRoot).ConfigureAwait(false),
            "resume" => await ResumeAsync(args, settings, repoRoot).ConfigureAwait(false),
            _ => Usage(),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  pbi-update-hitl start  <ingestion-run> [model] [--dry-run] [--max-attempts n]");
        Console.Error.WriteLine("  pbi-update-hitl resume <pbi-update-run> [--ui | --accept-all | --accept op-0,op-2] [--no-browser]");
        return 2;
    }

    // ---------------- START (Prozess A): bis zum Human-Gate, Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        var maxAttempts = 2;
        string? token = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pbi-update-hitl] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) return Usage();

        var ingestPlanDir = IngestionReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = ingestPlanDir is null ? null : Path.Combine(ingestPlanDir, "applied", "delta.json");
        if (deltaPath is null || !File.Exists(deltaPath)) { Console.Error.WriteLine($"[pbi-update-hitl] Ingestion-Delta (applied/delta.json) fuer '{token}' nicht gefunden - erst ingest-apply fahren."); return 2; }
        var delta = await LoadAsync<IngestionApplyReport>(deltaPath).ConfigureAwait(false);
        var sourceIngestionRun = Path.GetFileName(Path.GetDirectoryName(ingestPlanDir!) ?? ingestPlanDir!);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pbi-update-hitl] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "pbi-update");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var checkpointDir = run.OutputDir("checkpoints");

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "PbiPlacementAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var humanGate = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-update-gate");
        var workflow = PbiUpdateHitlWorkflow.Build(
            new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(factory, run), new PbiUpdateGateExecutor(run),
            new PbiUpdateRepairExecutor(factory, run), new PbiUpdateHitlFinalizeExecutor(run),
            humanGate, new PbiUpdateApplyExecutor(run, repoRoot, outDir));

        var ctx = new PbiUpdateWfContext(core, delta.Applied, sourceIngestionRun, outDir, dryRun, maxAttempts);
        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);

        Console.WriteLine($"[pbi-update-hitl] start runId={run.RunId} model={genSettings.ModelId} dryRun={dryRun} maxAttempts={maxAttempts}");
        CheckpointInfo? pending = null;
        try
        {
            await using var runHandle = await InProcessExecution.RunStreamingAsync(workflow, ctx, manager, run.RunId).ConfigureAwait(false);
            await foreach (var evt in runHandle.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is RequestInfoEvent) Console.WriteLine("[pbi-update-hitl] Human-Gate erreicht (Plan wartet auf Freigabe).");
                if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
                {
                    if (info.Checkpoint is { } cp) pending = cp;
                    if (info.HasPendingRequests && pending is not null) break;
                }
                if (evt is WorkflowOutputEvent outEvt && outEvt.Data is PbiUpdateWfResult manual)
                {
                    Console.WriteLine($"[pbi-update-hitl] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[pbi-update-hitl] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        if (pending is null) { Console.Error.WriteLine("[pbi-update-hitl] kein Checkpoint mit offenem Human-Gate erzeugt."); return 4; }
        await File.WriteAllTextAsync(Path.Combine(checkpointDir, "pointer.json"), JsonSerializer.Serialize(
            new PointerFile(run.RunId, pending.SessionId, pending.CheckpointId, DateTime.UtcNow), Json)).ConfigureAwait(false);

        Console.WriteLine($"[pbi-update-hitl] PAUSIERT am Human-Gate. checkpointId={pending.CheckpointId}");
        Console.WriteLine($"[pbi-update-hitl] Plan: {Path.GetRelativePath(repoRoot, outDir)}/pbi-change-plan.json");
        Console.WriteLine($"[pbi-update-hitl] Fortsetzen: pbi-update-hitl resume {run.RunId} --ui   (oder --accept-all)");
        return 0;
    }

    // ---------------- RESUME (Prozess B): Checkpoint restaurieren, Entscheidung uebergeben, Apply. ----------------
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        var uiMode = args.Contains("--ui", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        string? runToken = null, acceptList = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--accept-all", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--ui", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "--no-browser", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--accept", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { acceptList = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pbi-update-hitl] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var planDir = PbiUpdateReviewRunner.ResolvePlanDir(repoRoot, runToken);
        if (planDir is null) { Console.Error.WriteLine($"[pbi-update-hitl] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runDir = Path.GetDirectoryName(planDir)!;
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (!File.Exists(pointerPath)) { Console.Error.WriteLine($"[pbi-update-hitl] pointer.json fehlt unter {checkpointDir} - kein pausierter HITL-Lauf."); return 2; }
        var pointer = await LoadAsync<PointerFile>(pointerPath).ConfigureAwait(false);

        var plan = await LoadAsync<PbiStateChangePlanDocument>(Path.Combine(planDir, "pbi-change-plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = ResolveAccepted(planDir, plan, acceptAll, acceptList, decisionsPath);
            if (accepted is null) { Console.Error.WriteLine("[pbi-update-hitl] keine Entscheidung: --ui, --accept-all, --accept op-0,.. oder human-decisions.json noetig."); return 2; }
        }

        var run = new RunContext(runId, "pbi-update");
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Maker darf beim Resume nicht laufen.");
        var humanGate = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-update-gate");
        var workflow = PbiUpdateHitlWorkflow.Build(
            new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(noAgent, run), new PbiUpdateGateExecutor(run),
            new PbiUpdateRepairExecutor(noAgent, run), new PbiUpdateHitlFinalizeExecutor(run),
            humanGate, new PbiUpdateApplyExecutor(run, repoRoot, outDir));

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        var checkpoint = new CheckpointInfo(pointer.SessionId, pointer.CheckpointId);

        Console.WriteLine($"[pbi-update-hitl] resume runId={runId} mode={(uiMode ? "ui" : "cli")}");
        await using var runHandle = await InProcessExecution.OpenStreamingAsync(workflow, manager, runId).ConfigureAwait(false);
        await runHandle.RestoreCheckpointAsync(checkpoint).ConfigureAwait(false);

        using var cts = new CancellationTokenSource(uiMode ? Timeout.InfiniteTimeSpan : TimeSpan.FromMinutes(2));
        await foreach (var evt in runHandle.WatchStreamAsync(cts.Token).ConfigureAwait(false))
        {
            if (evt is RequestInfoEvent req)
            {
                var acc = uiMode
                    ? await CollectViaUiAsync(runId, plan, decisionsPath, repoRoot, settings, noBrowser).ConfigureAwait(false)
                    : accepted!;
                await runHandle.SendResponseAsync(req.Request.CreateResponse(new PbiUpdateReviewResponse(acc, uiMode ? "human (review-ui)" : "author (cli)"))).ConfigureAwait(false);
            }
            else if (evt is WorkflowOutputEvent outEvt && outEvt.Data is PbiUpdateApplyReport report)
            {
                Console.WriteLine($"[pbi-update-hitl] APPLIED newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count} relations(+{report.RelationsAdded}/-{report.RelationsRemoved}) skipped={report.Skipped.Count}");
                Console.WriteLine($"[pbi-update-hitl] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }
        }
        Console.Error.WriteLine("[pbi-update-hitl] Resume beendet ohne Apply-Report (Timeout/kein Request?).");
        return 4;
    }

    // S2-Muster: Entscheidung interaktiv ueber die generische HumanReview-UI (derselbe Adapter wie pbi-update-review).
    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, PbiStateChangePlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var session = PbiUpdateReviewAdapter.BuildSession(runId, plan, core);
        var existing = File.Exists(decisionsPath) ? await LoadAsync<PbiUpdateDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        PbiUpdateReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = PbiUpdateReviewAdapter.Resolved(it);

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(PbiUpdateReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = PbiUpdateReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(PbiUpdateReviewAdapter.ResolveContext(key, plan, core)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);

        var decisions = PbiUpdateReviewAdapter.Apply(runId, session);
        var accepted = decisions.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList();
        Console.WriteLine($"[pbi-update-hitl] UI {result.Outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

    // Akzeptierte OpIds: --accept-all | --accept Liste | human-decisions.json (fehlende Entscheidung -> default apply).
    private static IReadOnlyList<string>? ResolveAccepted(string planDir, PbiStateChangePlanDocument plan, bool acceptAll, string? acceptList, string decisionsPath)
    {
        if (acceptAll) return Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToList();
        if (!string.IsNullOrWhiteSpace(acceptList))
            return acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (File.Exists(decisionsPath))
        {
            var decisions = JsonSerializer.Deserialize<PbiUpdateDecisionsFile>(File.ReadAllText(decisionsPath), Json)!;
            return PbiUpdateApplyExec.AcceptedFromDecisions(plan, decisions.Decisions).Select(i => $"op-{i}").ToList();
        }
        return null;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private sealed record PointerFile(string RunId, string SessionId, string CheckpointId, DateTime SavedUtc);
}
