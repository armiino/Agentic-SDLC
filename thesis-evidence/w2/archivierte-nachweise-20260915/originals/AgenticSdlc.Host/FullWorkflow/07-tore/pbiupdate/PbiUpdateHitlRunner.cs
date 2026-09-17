using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// S4 (Worklist 20.07): pbi-update als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint (Muster wie
// github-forward-hitl, S1/S2). Alter Pfad (pbi-update / -review / -apply) bleibt UNVERAENDERT parallel.
//   pbi-update-hitl start  <ingestion-run> [model] [--dry-run] [--max-attempts n]
//   pbi-update-hitl resume <pbi-update-run> [--ui | --accept-all | --accept op-0,op-2] [--no-browser]
public static class PbiUpdateHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "PbiPlacementAgent";
    private const string AlignAgentName = "PbiAlignmentAgent"; // R-26-C
    private const string Cmd = "pbi-update-hitl";
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
        var alignPrompt = PromptProvider.Load(repoRoot, Phase, AlignAgentName, "PbiAlignmentAgent1", new Dictionary<string, string> { ["runId"] = run.RunId }); // R-26-C
        Func<IReadOnlyList<AITool>, AIAgent> alignFactory = tools =>
            client.AsAIAgent(instructions: alignPrompt, name: AlignAgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var humanGate = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-update-gate");
        var workflow = PbiUpdateHitlWorkflow.Build(
            new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(factory, run), new PbiUpdateGateExecutor(run),
            new PbiUpdateRepairExecutor(factory, run), new PbiAlignExecutor(alignFactory, run), new PbiUpdateHitlFinalizeExecutor(run),
            humanGate, new PbiUpdateApplyExecutor(run, repoRoot, outDir),
            new PbiUpdateEmptyGateResponder(run));

        var ctx = new PbiUpdateWfContext(core, delta.Applied, sourceIngestionRun, outDir, dryRun, maxAttempts);
        Console.WriteLine($"[{Cmd}] start runId={run.RunId} model={genSettings.ModelId} dryRun={dryRun} maxAttempts={maxAttempts}");
        return await HitlShell.StartAsync(Cmd, workflow, ctx, run, checkpointDir,
            onManualOutput: data =>
            {
                // R-50: leeres Gate wird LAUT übersprungen — der Lauf endet dann schon im Start mit dem Apply-Report.
                if (data is PbiUpdateApplyReport report)
                {
                    Console.WriteLine($"[{Cmd}] APPLIED (leeres Gate, R-50-Skip) newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count}");
                    return 0;
                }
                if (data is not PbiUpdateWfResult manual) return null;
                Console.WriteLine($"[{Cmd}] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                return 1;
            },
            pausedLines:
            [
                $"[{Cmd}] Plan: {Path.GetRelativePath(repoRoot, outDir)}/pbi-change-plan.json",
                $"[{Cmd}] Fortsetzen: {Cmd} resume {run.RunId} --ui   (oder --accept-all)"
            ]).ConfigureAwait(false);
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
        var pointer = await HitlShell.LoadPointerAsync(Cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        var plan = await HitlShell.LoadAsync<PbiStateChangePlanDocument>(Path.Combine(planDir, "pbi-change-plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = await HumanDecisions.ResolveAcceptedAsync(acceptAll, acceptList, decisionsPath,
                allIds: () => Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToList(),
                acceptedFromFile: async p => PbiUpdateApplyExec.AcceptedFromDecisions(plan,
                    (await HitlShell.LoadAsync<PbiUpdateDecisionsFile>(p).ConfigureAwait(false)).Decisions).Select(i => $"op-{i}").ToList()).ConfigureAwait(false);
            if (accepted is null) { Console.Error.WriteLine("[pbi-update-hitl] keine Entscheidung: --ui, --accept-all, --accept op-0,.. oder human-decisions.json noetig."); return 2; }
        }

        var run = new RunContext(runId, "pbi-update");
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Maker darf beim Resume nicht laufen.");
        var humanGate = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-update-gate");
        var workflow = PbiUpdateHitlWorkflow.Build(
            new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(noAgent, run), new PbiUpdateGateExecutor(run),
            new PbiUpdateRepairExecutor(noAgent, run), new PbiAlignExecutor(noAgent, run), new PbiUpdateHitlFinalizeExecutor(run),
            humanGate, new PbiUpdateApplyExecutor(run, repoRoot, outDir),
            new PbiUpdateEmptyGateResponder(run));

        Console.WriteLine($"[{Cmd}] resume runId={runId} mode={(uiMode ? "ui" : "cli")}");
        return await HitlShell.ResumeAsync(Cmd, workflow, runId, checkpointDir, pointer, uiMode,
            makeResponse: async () =>
            {
                // R-26-C: im UI-Modus traegt die Response auch die autorisierten Angleichungen; im CLI-Modus
                // (--accept-all/--accept) werden bewusst KEINE Angleichungen geschrieben (Inhalts-Mutation
                // braucht eine explizite menschliche Freigabe im Review).
                if (uiMode)
                {
                    var (ops, aligns, overrides, requests) = await CollectViaUiAsync(runId, plan, decisionsPath, repoRoot, settings, noBrowser).ConfigureAwait(false);
                    return new PbiUpdateReviewResponse(ops, "human (review-ui)", aligns, overrides, requests.Count > 0 ? requests : null);
                }
                return new PbiUpdateReviewResponse(accepted!, "author (cli)");
            },
            onOutput: data =>
            {
                if (data is not PbiUpdateApplyReport report) return null;
                Console.WriteLine($"[{Cmd}] APPLIED newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count} relations(+{report.RelationsAdded}/-{report.RelationsRemoved}) skipped={report.Skipped.Count}");
                Console.WriteLine($"[{Cmd}] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return 0;
            }).ConfigureAwait(false);
    }

    // S2-Muster: Entscheidung interaktiv ueber die generische HumanReview-UI (derselbe Adapter wie pbi-update-review).
    // R-26-C: liefert zusaetzlich die AKZEPTIERTEN Angleichungen (accept/edit) fuer die Response.
    private static async Task<(IReadOnlyList<string> Ops, IReadOnlyList<PbiAlignment> Aligns, IReadOnlyList<PbiFeatureOverride> Overrides, IReadOnlyList<Decision.PbiDecisionRequest> Requests)> CollectViaUiAsync(
        string runId, PbiStateChangePlanDocument plan, string decisionsPath, string repoRoot, HostSettings settings, bool noBrowser)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var session = PbiUpdateReviewAdapter.BuildSession(runId, plan, core);
        var existing = File.Exists(decisionsPath) ? await HitlShell.LoadAsync<PbiUpdateDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        PbiUpdateReviewAdapter.MergeExistingDecisions(session, existing);

        var (decisions, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: PbiUpdateReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(PbiUpdateReviewAdapter.ResolveContext(key, plan, core)),
            apply: s => PbiUpdateReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser,
            // B1/B2: die rechte Feature-Landkarte — bestehendes Feature -> seine PBIs, vorgeschlagenes neues Feature
            // -> die geplanten Anforderungen (aus dem Plan).
            resolveReference: reference => Task.FromResult(PbiUpdateReviewAdapter.ResolveReference(reference, core, plan))).ConfigureAwait(false);

        var accepted = decisions.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList();
        var aligns = PbiUpdateApplyExec.AcceptedAlignments(plan, decisions.AlignmentDecisions);
        var overrides = PbiUpdateApplyExec.FeatureOverrides(plan, decisions.Decisions);   // B1: geänderte Feature-Zuordnungen
        var requests = PbiUpdateReviewAdapter.DecisionRequestsFrom(plan, decisions.Decisions);   // R-14 D2: „→ Entscheidung"-Anträge
        Console.WriteLine($"[{Cmd}] UI {outcome}: {accepted.Count}/{plan.Operations.Count} Ops + {aligns.Count} Angleichung(en) + {overrides.Count} Feature-Korrektur(en) + {requests.Count} Entscheidungs-Antrag/-Anträge -> human-decisions.json");
        return (accepted, aligns, overrides, requests);
    }

    private static Task<T> LoadAsync<T>(string path) => HitlShell.LoadAsync<T>(path); // R2: geteilt (StartAsync nutzt es noch fuers Delta)
}
