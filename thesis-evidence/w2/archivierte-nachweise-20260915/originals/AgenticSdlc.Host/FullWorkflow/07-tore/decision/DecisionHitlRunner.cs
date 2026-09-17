using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

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
    private const string Cmd = "decision-resolve-hitl";
    private static readonly JsonSerializerOptions Json = HitlShell.Json; // R2: geteilte Optionen

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
            var openCount = core.Items.Count(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase) && i.ReadStatus().IsOpenDecision);
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

        Console.WriteLine($"[{Cmd}] start runId={run.RunId} mode={(agentic ? "agent" : "det")} maxAttempts={maxAttempts}");
        Func<object?, int?> onManualOutput = data =>
        {
            if (data is not DecisionWfResult manual) return null;
            Console.WriteLine($"[{Cmd}] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
            return 1;
        };
        IReadOnlyList<string> pausedLines = [$"[{Cmd}] Fortsetzen: {Cmd} resume {run.RunId} --ui   (oder --accept-all)"];
        // WICHTIG: konkreter Typ, damit RunStreamingAsync<T> die Nachricht an den Start-Executor routet
        // (T=object wuerde nicht matchen -> kein Executor laeuft).
        return agentic
            ? await HitlShell.StartAsync(Cmd, workflow, (DecisionAnswerMsg)startInput, run, checkpointDir, onManualOutput, pausedLines, pointerMode: "agent").ConfigureAwait(false)
            : await HitlShell.StartAsync(Cmd, workflow, (DecisionResolveInputMsg)startInput, run, checkpointDir, onManualOutput, pausedLines, pointerMode: "det").ConfigureAwait(false);
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
        var pointer = await HitlShell.LoadPointerAsync(Cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        var plan = await HitlShell.LoadAsync<DecisionResolutionPlanDocument>(Path.Combine(planDir, "decision-plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = await HumanDecisions.ResolveAcceptedAsync(acceptAll, acceptList, decisionsPath,
                allIds: () => Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToList(),
                acceptedFromFile: async p => DecisionApplyExec.AcceptedFromDecisions(plan,
                    (await HitlShell.LoadAsync<DecisionResolutionDecisionsFile>(p).ConfigureAwait(false)).Decisions).Select(i => $"op-{i}").ToList()).ConfigureAwait(false);
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

        Console.WriteLine($"[{Cmd}] resume runId={runId} mode={(uiMode ? "ui" : "cli")}/{pointer.Mode}");
        return await HitlShell.ResumeAsync(Cmd, workflow, runId, checkpointDir, pointer, uiMode,
            makeResponse: async () =>
            {
                var acc = uiMode
                    ? await CollectViaUiAsync(runId, plan, decisionsPath, repoRoot, settings, noBrowser).ConfigureAwait(false)
                    : accepted!;
                return new DecisionReviewResponse(acc, uiMode ? "human (review-ui)" : "author (cli)");
            },
            onOutput: data =>
            {
                if (data is not DecisionResolutionApplyReport report) return null;
                Console.WriteLine($"[{Cmd}] APPLIED resolved={report.Resolved.Count} unblocked={report.UnblockedPbis.Count} swapped={report.SwappedPbis.Count} newReqs={report.NewRequirements.Count} skipped={report.Skipped.Count}");
                Console.WriteLine($"[{Cmd}] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, DecisionResolutionPlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var session = DecisionResolutionReviewAdapter.BuildSession(runId, plan, core);
        var existing = File.Exists(decisionsPath) ? await HitlShell.LoadAsync<DecisionResolutionDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        DecisionResolutionReviewAdapter.MergeExistingDecisions(session, existing);

        var (decisions, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: DecisionResolutionReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(DecisionResolutionReviewAdapter.ResolveContext(key, plan, core)),
            apply: s => DecisionResolutionReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);

        var accepted = decisions.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList();
        Console.WriteLine($"[{Cmd}] UI {outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

    private static Task<T> LoadAsync<T>(string path) => HitlShell.LoadAsync<T>(path); // R2: geteilt (StartAsync nutzt es fuer --input)
}
