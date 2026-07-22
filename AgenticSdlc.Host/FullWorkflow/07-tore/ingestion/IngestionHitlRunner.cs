using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// S4 (Worklist 20.07): ingestion (Tor 1) als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint (Muster wie
// pbi-update-hitl/decision-resolve-hitl). Alter Pfad (ingest-requirements / -review / -apply) bleibt parallel.
//   ingest-requirements-hitl start  <meeting-delta project-state.json> [model] [--max-attempts n]
//   ingest-requirements-hitl resume <ingestion-run> [--ui | --accept-all | --accept ID,ID] [--no-browser]
public static class IngestionHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "RequirementIngestionAgent";
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
        Console.Error.WriteLine("  ingest-requirements-hitl start  <meeting-delta project-state.json> [model] [--max-attempts n]");
        Console.Error.WriteLine("  ingest-requirements-hitl resume <ingestion-run> [--ui | --accept-all | --accept ID,ID] [--no-browser]");
        return 2;
    }

    // ---------------- START (Prozess A): bis zum Human-Gate, Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var maxAttempts = 2;
        string? deltaToken = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[ingest-requirements-hitl] unbekanntes Argument: {a}"); return 2; }
            if (deltaToken is null) deltaToken = a; else modelArg ??= a;
        }
        if (deltaToken is null) return Usage();

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[ingest-requirements-hitl] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaPath = Path.IsPathRooted(deltaToken) ? deltaToken : Path.Combine(repoRoot, deltaToken);
        if (!File.Exists(deltaPath)) { Console.Error.WriteLine($"[ingest-requirements-hitl] MeetingDelta nicht gefunden: {deltaPath}"); return 2; }
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;
        var deltaRel = Path.GetRelativePath(repoRoot, deltaPath);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "ingestion");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var checkpointDir = run.OutputDir("checkpoints");

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "RequirementIngestionAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        ICandidateRetriever retriever = new ShowAllRequirementRetriever();

        var humanGate = RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingestion-gate");
        var workflow = IngestionHitlWorkflow.Build(
            new IngestionHitlResolveExecutor(factory, retriever, run), new IngestionGateExecutor(run), new IngestionRepairExecutor(factory, retriever, run),
            new IngestionHitlFinalizeExecutor(run, outDir), humanGate, new IngestionApplyExecutor(run, repoRoot, outDir));

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        var incoming = delta.Items.Count(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"[ingest-requirements-hitl] start runId={run.RunId} model={genSettings.ModelId} incoming-req={incoming} maxAttempts={maxAttempts}");
        CheckpointInfo? pending = null;
        try
        {
            await using var runHandle = await InProcessExecution.RunStreamingAsync(workflow, new IngestionResolveInput(delta, core, deltaRel, maxAttempts), manager, run.RunId).ConfigureAwait(false);
            await foreach (var evt in runHandle.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is RequestInfoEvent) Console.WriteLine("[ingest-requirements-hitl] Human-Gate erreicht (Plan wartet auf Freigabe).");
                if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
                {
                    if (info.Checkpoint is { } cp) pending = cp;
                    if (info.HasPendingRequests && pending is not null) break;
                }
                if (evt is WorkflowOutputEvent outEvt && outEvt.Data is IngestionResult manual)
                {
                    Console.WriteLine($"[ingest-requirements-hitl] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ingest-requirements-hitl] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        if (pending is null) { Console.Error.WriteLine("[ingest-requirements-hitl] kein Checkpoint mit offenem Human-Gate erzeugt."); return 4; }
        await File.WriteAllTextAsync(Path.Combine(checkpointDir, "pointer.json"), JsonSerializer.Serialize(
            new PointerFile(run.RunId, pending.SessionId, pending.CheckpointId, DateTime.UtcNow), Json)).ConfigureAwait(false);
        Console.WriteLine($"[ingest-requirements-hitl] PAUSIERT am Human-Gate. checkpointId={pending.CheckpointId}");
        Console.WriteLine($"[ingest-requirements-hitl] Fortsetzen: ingest-requirements-hitl resume {run.RunId} --ui   (oder --accept-all)");
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
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[ingest-requirements-hitl] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var planDir = IngestionReviewRunner.ResolvePlanDir(repoRoot, runToken);
        if (planDir is null) { Console.Error.WriteLine($"[ingest-requirements-hitl] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runDir = Path.GetDirectoryName(planDir)!;
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (!File.Exists(pointerPath)) { Console.Error.WriteLine($"[ingest-requirements-hitl] pointer.json fehlt unter {checkpointDir} - kein pausierter HITL-Lauf."); return 2; }
        var pointer = await LoadAsync<PointerFile>(pointerPath).ConfigureAwait(false);

        var plan = await LoadAsync<StateChangePlanDocument>(Path.Combine(planDir, "plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = ResolveAccepted(plan, acceptAll, acceptList, decisionsPath);
            if (accepted is null) { Console.Error.WriteLine("[ingest-requirements-hitl] keine Entscheidung: --ui, --accept-all, --accept ID,.. oder human-decisions.json noetig."); return 2; }
        }

        var run = new RunContext(runId, "ingestion");
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Resolver darf beim Resume nicht laufen.");
        ICandidateRetriever retriever = new ShowAllRequirementRetriever();
        var humanGate = RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingestion-gate");
        var workflow = IngestionHitlWorkflow.Build(
            new IngestionHitlResolveExecutor(noAgent, retriever, run), new IngestionGateExecutor(run), new IngestionRepairExecutor(noAgent, retriever, run),
            new IngestionHitlFinalizeExecutor(run, outDir), humanGate, new IngestionApplyExecutor(run, repoRoot, outDir));

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        var checkpoint = new CheckpointInfo(pointer.SessionId, pointer.CheckpointId);

        Console.WriteLine($"[ingest-requirements-hitl] resume runId={runId} mode={(uiMode ? "ui" : "cli")}");
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
                await runHandle.SendResponseAsync(req.Request.CreateResponse(new IngestionReviewResponse(acc, uiMode ? "human (review-ui)" : "author (cli)"))).ConfigureAwait(false);
            }
            else if (evt is WorkflowOutputEvent outEvt && outEvt.Data is IngestionApplyReport report)
            {
                var d = report.Delta;
                Console.WriteLine($"[ingest-requirements-hitl] APPLIED applied={report.Applied.Count} skipped={report.Skipped.Count} (added={d.Added} refined={d.Refined} superseded={d.Superseded} contradicted={d.Contradicted})");
                Console.WriteLine($"[ingest-requirements-hitl] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }
        }
        Console.Error.WriteLine("[ingest-requirements-hitl] Resume beendet ohne Apply-Report (Timeout/kein Request?).");
        return 4;
    }

    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, StateChangePlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var deltaFull = Path.IsPathRooted(plan.SourceMeetingDeltaPath) ? plan.SourceMeetingDeltaPath : Path.Combine(repoRoot, plan.SourceMeetingDeltaPath);
        var meetingDelta = (await JsonProjectStateRepository.LoadAsync(deltaFull).ConfigureAwait(false)).Document;

        var session = IngestionReviewAdapter.BuildSession(runId, plan, meetingDelta, core);
        var existing = File.Exists(decisionsPath) ? await LoadAsync<IngestionHumanDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        IngestionReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = IngestionReviewAdapter.Resolved(it);

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(IngestionReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = IngestionReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(IngestionReviewAdapter.ResolveContext(key, plan, meetingDelta, core)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);

        var decisions = IngestionReviewAdapter.Apply(runId, session);
        var accepted = IngestionApplyExec.AcceptedFromDecisions(decisions.Decisions).ToList();
        Console.WriteLine($"[ingest-requirements-hitl] UI {result.Outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

    // Akzeptierte IncomingItemIds: --accept-all | --accept Liste | human-decisions.json.
    private static IReadOnlyList<string>? ResolveAccepted(StateChangePlanDocument plan, bool acceptAll, string? acceptList, string decisionsPath)
    {
        if (acceptAll) return plan.Operations.Select(o => o.IncomingItemId).ToList();
        if (!string.IsNullOrWhiteSpace(acceptList))
            return acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (File.Exists(decisionsPath))
        {
            var decisions = JsonSerializer.Deserialize<IngestionHumanDecisionsFile>(File.ReadAllText(decisionsPath), Json)!;
            return IngestionApplyExec.AcceptedFromDecisions(decisions.Decisions).ToList();
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
