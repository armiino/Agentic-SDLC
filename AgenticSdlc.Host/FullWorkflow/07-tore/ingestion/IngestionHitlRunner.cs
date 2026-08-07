using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// S4 (Worklist 20.07): ingestion (Tor 1) als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint (Muster wie
// pbi-update-hitl/decision-resolve-hitl). Alter Pfad (ingest-requirements / -review / -apply) bleibt parallel.
//   ingest-requirements-hitl start  <meeting-delta project-state.json> [model] [--max-attempts n]
//   ingest-requirements-hitl resume <ingestion-run> [--ui | --accept-all | --accept ID,ID] [--no-browser]
public static class IngestionHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private static readonly JsonSerializerOptions Json = HitlShell.Json; // R2: geteilte Optionen

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
        => RunAsync(args, settings, repoRoot, AspectIngestionProfile.Requirement, "ingest-requirements-hitl", "ingestion");

    // R-11 A1d: die geteilte CLI-Bahn — dieselbe Mechanik je Aspekt-Profil (arch: `ingest-architecture-hitl`).
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot,
        AspectIngestionProfile profile, string cmd, string runPhase)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => await StartAsync(args, settings, repoRoot, profile, cmd, runPhase).ConfigureAwait(false),
            "resume" => await ResumeAsync(args, settings, repoRoot, profile, cmd, runPhase).ConfigureAwait(false),
            _ => Usage(cmd),
        };
    }

    private static int Usage(string cmd)
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine($"  {cmd} start  <meeting-delta project-state.json> [model] [--max-attempts n]");
        Console.Error.WriteLine($"  {cmd} resume <run> [--ui | --accept-all | --accept ID,ID] [--no-browser]");
        return 2;
    }

    // ---------------- START (Prozess A): bis zum Human-Gate, Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot,
        AspectIngestionProfile profile, string cmd, string runPhase)
    {
        if (args.Length < 3) return Usage(cmd);
        var maxAttempts = 2;
        string? deltaToken = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[{cmd}] unbekanntes Argument: {a}"); return 2; }
            if (deltaToken is null) deltaToken = a; else modelArg ??= a;
        }
        if (deltaToken is null) return Usage(cmd);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine($"[{cmd}] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaPath = Path.IsPathRooted(deltaToken) ? deltaToken : Path.Combine(repoRoot, deltaToken);
        if (!File.Exists(deltaPath)) { Console.Error.WriteLine($"[{cmd}] MeetingDelta nicht gefunden: {deltaPath}"); return 2; }
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;
        var deltaRel = Path.GetRelativePath(repoRoot, deltaPath);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), runPhase);
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var checkpointDir = run.OutputDir("checkpoints");

        var prompt = PromptProvider.Load(repoRoot, Phase, profile.AgentName, profile.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId })
                     + Configuration.ReasoningSchema.ToolAgentPromptAppendix(settings.ReasoningCapture);   // W1a-Erweiterung (geteilter Schalter)
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, profile.AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: profile.AgentName, tools: [.. tools]).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        ICandidateRetriever retriever = new ShowAllRequirementRetriever(profile.Aspect);

        var humanGate = RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingestion-gate");
        var workflow = IngestionHitlWorkflow.Build(
            new IngestionHitlResolveExecutor(factory, retriever, run, profile), new IngestionGateExecutor(run, profile), new IngestionRepairExecutor(factory, retriever, run, profile),
            new IngestionHitlFinalizeExecutor(run, outDir, profile), humanGate, new IngestionApplyExecutor(run, repoRoot, outDir, profile), profile);

        var incoming = delta.Items.Count(profile.Matches);
        Console.WriteLine($"[{cmd}] start runId={run.RunId} model={genSettings.ModelId} incoming-req={incoming} maxAttempts={maxAttempts}");
        return await HitlShell.StartAsync(cmd, workflow, new IngestionResolveInput(delta, core, deltaRel, maxAttempts), run, checkpointDir,
            onManualOutput: data =>
            {
                if (data is not IngestionResult manual) return null;
                Console.WriteLine($"[{cmd}] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                return 1;
            },
            pausedLines: [$"[{cmd}] Fortsetzen: {cmd} resume {run.RunId} --ui   (oder --accept-all)"]).ConfigureAwait(false);
    }

    // ---------------- RESUME (Prozess B): Checkpoint restaurieren, Entscheidung uebergeben, Apply. ----------------
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot,
        AspectIngestionProfile profile, string cmd, string runPhase)
    {
        if (args.Length < 3) return Usage(cmd);
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
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[{cmd}] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage(cmd);

        var phaseCandidate = Path.Combine(repoRoot, "runs", runPhase, runToken);
        var planDir = Directory.Exists(phaseCandidate) ? Path.Combine(phaseCandidate, "plan") : IngestionReviewRunner.ResolvePlanDir(repoRoot, runToken);
        if (planDir is null) { Console.Error.WriteLine($"[{cmd}] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runDir = Path.GetDirectoryName(planDir)!;
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointer = await HitlShell.LoadPointerAsync(cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        var plan = await HitlShell.LoadAsync<StateChangePlanDocument>(Path.Combine(planDir, "plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = await HumanDecisions.ResolveAcceptedAsync(acceptAll, acceptList, decisionsPath,
                allIds: () => plan.Operations.Select(o => o.IncomingItemId).ToList(),
                acceptedFromFile: async p => IngestionApplyExec.AcceptedFromDecisions(
                    (await HitlShell.LoadAsync<IngestionHumanDecisionsFile>(p).ConfigureAwait(false)).Decisions).ToList()).ConfigureAwait(false);
            if (accepted is null) { Console.Error.WriteLine($"[{cmd}] keine Entscheidung: --ui, --accept-all, --accept ID,.. oder human-decisions.json noetig."); return 2; }
        }

        var run = new RunContext(runId, runPhase);
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Resolver darf beim Resume nicht laufen.");
        ICandidateRetriever retriever = new ShowAllRequirementRetriever(profile.Aspect);
        var humanGate = RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingestion-gate");
        var workflow = IngestionHitlWorkflow.Build(
            new IngestionHitlResolveExecutor(noAgent, retriever, run, profile), new IngestionGateExecutor(run, profile), new IngestionRepairExecutor(noAgent, retriever, run, profile),
            new IngestionHitlFinalizeExecutor(run, outDir, profile), humanGate, new IngestionApplyExecutor(run, repoRoot, outDir, profile), profile);

        Console.WriteLine($"[{cmd}] resume runId={runId} mode={(uiMode ? "ui" : "cli")}");
        return await HitlShell.ResumeAsync(cmd, workflow, runId, checkpointDir, pointer, uiMode,
            makeResponse: async () =>
            {
                var acc = uiMode
                    ? await CollectViaUiAsync(runId, plan, decisionsPath, repoRoot, settings, noBrowser, cmd).ConfigureAwait(false)
                    : accepted!;
                return new IngestionReviewResponse(acc, uiMode ? "human (review-ui)" : "author (cli)");
            },
            onOutput: data =>
            {
                if (data is not IngestionApplyReport report) return null;
                var d = report.Delta;
                Console.WriteLine($"[{cmd}] APPLIED applied={report.Applied.Count} skipped={report.Skipped.Count} (added={d.Added} refined={d.Refined} superseded={d.Superseded} contradicted={d.Contradicted})");
                Console.WriteLine($"[{cmd}] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, StateChangePlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser, string cmd)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var deltaFull = Path.IsPathRooted(plan.SourceMeetingDeltaPath) ? plan.SourceMeetingDeltaPath : Path.Combine(repoRoot, plan.SourceMeetingDeltaPath);
        var meetingDelta = (await JsonProjectStateRepository.LoadAsync(deltaFull).ConfigureAwait(false)).Document;

        var session = IngestionReviewAdapter.BuildSession(runId, plan, meetingDelta, core);
        var existing = File.Exists(decisionsPath) ? await HitlShell.LoadAsync<IngestionHumanDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        IngestionReviewAdapter.MergeExistingDecisions(session, existing);

        var (decisions, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: IngestionReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(IngestionReviewAdapter.ResolveContext(key, plan, meetingDelta, core)),
            apply: s => IngestionReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);

        var accepted = IngestionApplyExec.AcceptedFromDecisions(decisions.Decisions).ToList();
        Console.WriteLine($"[{cmd}] UI {outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

}
