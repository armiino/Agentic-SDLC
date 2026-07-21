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

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// S4 (Worklist 20.07): decision (Tor 2) als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint (Muster wie
// pbi-update-hitl). Zwei Modi (wie das Original): deterministisch (--input, KEIN LLM) + agentisch (--answer, LLM).
// Alter Pfad (decision-resolve / -resolve-agent / -review / -apply) bleibt UNVERAENDERT parallel.
//   decision-resolve-hitl start --input <resolution-input.json> [--max-attempts n]
//   decision-resolve-hitl start --answer <text|file> [model] [--max-attempts n]
//   decision-resolve-hitl resume <decision-run> [--ui | --accept-all | --accept op-0,..] [--no-browser]
public static class DecisionHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "DecisionResolverAgent";
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
        Console.Error.WriteLine("  decision-resolve-hitl start --input <resolution-input.json> [--max-attempts n]   (deterministisch, kein LLM)");
        Console.Error.WriteLine("  decision-resolve-hitl start --answer <text|file> [model] [--max-attempts n]      (agentisch)");
        Console.Error.WriteLine("  decision-resolve-hitl resume <decision-run> [--ui | --accept-all | --accept op-0,..] [--no-browser]");
        return 2;
    }

    // ---------------- START (Prozess A): bis zum Human-Gate, Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var maxAttempts = 2;
        string? inputArg = null, answerArg = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--input", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { inputArg = args[++i]; continue; }
            if (string.Equals(a, "--answer", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { answerArg = args[++i]; continue; }
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[decision-resolve-hitl] unbekanntes Argument: {a}"); return 2; }
            modelArg ??= a;
        }
        var agentic = inputArg is null;
        if (agentic && answerArg is null) return Usage();

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[decision-resolve-hitl] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "decision");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var checkpointDir = run.OutputDir("checkpoints");
        var humanGate = RequestPort.Create<DecisionReviewRequest, DecisionReviewResponse>("decision-gate");

        Workflow workflow;
        object startInput;
        if (agentic)
        {
            var answerPath = Path.IsPathRooted(answerArg!) ? answerArg! : Path.Combine(repoRoot, answerArg!);
            var stakeholderAnswer = File.Exists(answerPath) ? await File.ReadAllTextAsync(answerPath).ConfigureAwait(false) : answerArg!;
            if (string.IsNullOrWhiteSpace(stakeholderAnswer)) { Console.Error.WriteLine("[decision-resolve-hitl] leere Stakeholder-Antwort."); return 2; }
            var openCount = core.Items.Count(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase) && string.Equals(i.Status, DecisionStatus.Open, StringComparison.OrdinalIgnoreCase));
            if (openCount == 0) { Console.WriteLine("[decision-resolve-hitl] keine offenen Decisions im Core."); return 0; }

            var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "DecisionResolverAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
            var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
            Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
                client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

            var ctx = new DecisionWfContext(core, outDir, maxAttempts, stakeholderAnswer);
            workflow = DecisionHitlWorkflow.BuildAgentic(
                new DecisionMakerExecutor(factory, run), new DecisionDeriveExecutor(run), new DecisionGateExecutor(run),
                new DecisionRepairExecutor(factory, run), new DecisionHitlFinalizeExecutor(run), humanGate, new DecisionApplyExecutor(run, repoRoot, outDir));
            startInput = new DecisionAnswerMsg(ctx);
        }
        else
        {
            var inputPath = Path.IsPathRooted(inputArg!) ? inputArg! : Path.Combine(repoRoot, inputArg!);
            if (!File.Exists(inputPath)) { Console.Error.WriteLine($"[decision-resolve-hitl] Input nicht gefunden: {inputPath}"); return 2; }
            var input = await LoadAsync<DecisionResolutionInput>(inputPath).ConfigureAwait(false);
            var ctx = new DecisionWfContext(core, outDir, MaxAttempts: 1, StakeholderAnswer: null);
            workflow = DecisionHitlWorkflow.BuildDeterministic(
                new DecisionDeriveExecutor(run), new DecisionGateExecutor(run), new DecisionHitlFinalizeExecutor(run), humanGate, new DecisionApplyExecutor(run, repoRoot, outDir));
            startInput = new DecisionResolveInputMsg(ctx, input, Attempt: 1, Source: "maker", History: []);
        }

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        Console.WriteLine($"[decision-resolve-hitl] start runId={run.RunId} mode={(agentic ? "agent" : "det")} maxAttempts={maxAttempts}");
        CheckpointInfo? pending = null;
        try
        {
            // WICHTIG: konkreter Typ, damit RunStreamingAsync<T> die Nachricht an den Start-Executor routet
            // (T=object wuerde nicht matchen -> kein Executor laeuft).
            await using var runHandle = agentic
                ? await InProcessExecution.RunStreamingAsync(workflow, (DecisionAnswerMsg)startInput, manager, run.RunId).ConfigureAwait(false)
                : await InProcessExecution.RunStreamingAsync(workflow, (DecisionResolveInputMsg)startInput, manager, run.RunId).ConfigureAwait(false);
            await foreach (var evt in runHandle.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is RequestInfoEvent) Console.WriteLine("[decision-resolve-hitl] Human-Gate erreicht (Plan wartet auf Freigabe).");
                if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
                {
                    if (info.Checkpoint is { } cp) pending = cp;
                    if (info.HasPendingRequests && pending is not null) break;
                }
                if (evt is WorkflowOutputEvent outEvt && outEvt.Data is DecisionWfResult manual)
                {
                    Console.WriteLine($"[decision-resolve-hitl] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[decision-resolve-hitl] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        if (pending is null) { Console.Error.WriteLine("[decision-resolve-hitl] kein Checkpoint mit offenem Human-Gate erzeugt."); return 4; }
        await File.WriteAllTextAsync(Path.Combine(checkpointDir, "pointer.json"), JsonSerializer.Serialize(
            new PointerFile(run.RunId, pending.SessionId, pending.CheckpointId, agentic ? "agent" : "det", DateTime.UtcNow), Json)).ConfigureAwait(false);
        Console.WriteLine($"[decision-resolve-hitl] PAUSIERT am Human-Gate. checkpointId={pending.CheckpointId}");
        Console.WriteLine($"[decision-resolve-hitl] Fortsetzen: decision-resolve-hitl resume {run.RunId} --ui   (oder --accept-all)");
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
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[decision-resolve-hitl] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var planDir = DecisionReviewRunner.ResolvePlanDir(repoRoot, runToken);
        if (planDir is null) { Console.Error.WriteLine($"[decision-resolve-hitl] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runDir = Path.GetDirectoryName(planDir)!;
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (!File.Exists(pointerPath)) { Console.Error.WriteLine($"[decision-resolve-hitl] pointer.json fehlt unter {checkpointDir} - kein pausierter HITL-Lauf."); return 2; }
        var pointer = await LoadAsync<PointerFile>(pointerPath).ConfigureAwait(false);

        var plan = await LoadAsync<DecisionResolutionPlanDocument>(Path.Combine(planDir, "decision-plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = ResolveAccepted(plan, acceptAll, acceptList, decisionsPath);
            if (accepted is null) { Console.Error.WriteLine("[decision-resolve-hitl] keine Entscheidung: --ui, --accept-all, --accept op-0,.. oder human-decisions.json noetig."); return 2; }
        }

        var run = new RunContext(runId, "decision");
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Maker darf beim Resume nicht laufen.");
        var humanGate = RequestPort.Create<DecisionReviewRequest, DecisionReviewResponse>("decision-gate");
        var finalize = new DecisionHitlFinalizeExecutor(run);
        var apply = new DecisionApplyExecutor(run, repoRoot, outDir);
        var workflow = string.Equals(pointer.Mode, "agent", StringComparison.Ordinal)
            ? DecisionHitlWorkflow.BuildAgentic(new DecisionMakerExecutor(noAgent, run), new DecisionDeriveExecutor(run), new DecisionGateExecutor(run), new DecisionRepairExecutor(noAgent, run), finalize, humanGate, apply)
            : DecisionHitlWorkflow.BuildDeterministic(new DecisionDeriveExecutor(run), new DecisionGateExecutor(run), finalize, humanGate, apply);

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        var checkpoint = new CheckpointInfo(pointer.SessionId, pointer.CheckpointId);

        Console.WriteLine($"[decision-resolve-hitl] resume runId={runId} mode={(uiMode ? "ui" : "cli")}/{pointer.Mode}");
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
                await runHandle.SendResponseAsync(req.Request.CreateResponse(new DecisionReviewResponse(acc, uiMode ? "human (review-ui)" : "author (cli)"))).ConfigureAwait(false);
            }
            else if (evt is WorkflowOutputEvent outEvt && outEvt.Data is DecisionResolutionApplyReport report)
            {
                Console.WriteLine($"[decision-resolve-hitl] APPLIED resolved={report.Resolved.Count} unblocked={report.UnblockedPbis.Count} swapped={report.SwappedPbis.Count} newReqs={report.NewRequirements.Count} skipped={report.Skipped.Count}");
                Console.WriteLine($"[decision-resolve-hitl] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }
        }
        Console.Error.WriteLine("[decision-resolve-hitl] Resume beendet ohne Apply-Report (Timeout/kein Request?).");
        return 4;
    }

    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, DecisionResolutionPlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var session = DecisionResolutionReviewAdapter.BuildSession(runId, plan, core);
        var existing = File.Exists(decisionsPath) ? await LoadAsync<DecisionResolutionDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        DecisionResolutionReviewAdapter.MergeExistingDecisions(session, existing);
        foreach (var it in session.Items) it.Resolved = DecisionResolutionReviewAdapter.Resolved(it);

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(DecisionResolutionReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);
        var result = await LocalReviewServerHost.RunAsync(new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = DecisionResolutionReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(DecisionResolutionReviewAdapter.ResolveContext(key, plan, core)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),
            OpenBrowser = settings.L3ReviewOpenBrowser && !noBrowser
        }).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);

        var decisions = DecisionResolutionReviewAdapter.Apply(runId, session);
        var accepted = decisions.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList();
        Console.WriteLine($"[decision-resolve-hitl] UI {result.Outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

    // Akzeptierte OpIds: --accept-all | --accept Liste | human-decisions.json (EXPLIZIT apply, kein Default).
    private static IReadOnlyList<string>? ResolveAccepted(DecisionResolutionPlanDocument plan, bool acceptAll, string? acceptList, string decisionsPath)
    {
        if (acceptAll) return Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToList();
        if (!string.IsNullOrWhiteSpace(acceptList))
            return acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (File.Exists(decisionsPath))
        {
            var decisions = JsonSerializer.Deserialize<DecisionResolutionDecisionsFile>(File.ReadAllText(decisionsPath), Json)!;
            return DecisionApplyExec.AcceptedFromDecisions(plan, decisions.Decisions).Select(i => $"op-{i}").ToList();
        }
        return null;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private sealed record PointerFile(string RunId, string SessionId, string CheckpointId, string Mode, DateTime SavedUtc);
}
