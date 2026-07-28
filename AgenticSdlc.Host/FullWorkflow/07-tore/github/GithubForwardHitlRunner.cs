using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// S1 (Worklist 20.07): github-forward als EIN MAF-Lauf mit MAF-nativem Human-Gate + Checkpoint.
//   github-forward-hitl start  <pbi-update-run> [model] [--issues <snap>] [--repo o/n] [--dry-run] [--max-attempts n] [--token-env NAME]
//   github-forward-hitl resume <github-forward-run> [--accept-all | --accept op-0,op-2] [--execute] [--repo o/n] [--token-env NAME]
// start laeuft bis zum Human-Gate (Decision==Pass) und pausiert (Checkpoint auf Platte); bei Gate-Fail terminiert er
// ohne Apply. resume (NEUER Prozess) restauriert, uebergibt die menschliche Entscheidung und fuehrt den Apply aus.
// Alter Pfad (github-forward / -review / -apply) bleibt UNVERAENDERT parallel.
public static class GithubForwardHitlRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "GithubForwardAgent";
    private const string Cmd = "github-forward-hitl";
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
        Console.Error.WriteLine("  github-forward-hitl start  <pbi-update-run> [model] [--issues <snap>] [--repo o/n] [--dry-run] [--max-attempts n] [--token-env NAME]");
        Console.Error.WriteLine("  github-forward-hitl resume <github-forward-run> [--ui | --accept-all | --accept op-0,op-2] [--execute] [--no-browser] [--repo o/n] [--token-env NAME]");
        return 2;
    }

    // ---------------- START (Prozess A): bis zum Human-Gate, Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        var maxAttempts = 2;
        string? token = null, modelArg = null, issuesArg = null, repoArg = null, tokenEnv = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { issuesArg = args[++i]; continue; }
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (string.Equals(a, "--token-env", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { tokenEnv = args[++i]; continue; }
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward-hitl] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) return Usage();

        // Laden wie github-forward: Delta -> Core-Mappings -> Issue-Snapshot.
        var planSrcDir = PbiUpdateReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = planSrcDir is null ? null : Path.Combine(planSrcDir, "applied", "github-sync-delta.json");
        if (deltaPath is null || !File.Exists(deltaPath)) { Console.Error.WriteLine($"[github-forward-hitl] github-sync-delta.json fuer '{token}' nicht gefunden - erst pbi-update-apply fahren."); return 2; }
        var delta = await LoadAsync<GithubSyncDeltaDocument>(deltaPath).ConfigureAwait(false);
        var sourcePbiUpdateRun = Path.GetFileName(Path.GetDirectoryName(planSrcDir!) ?? planSrcDir!);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-forward-hitl] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        var snapshotPath = ResolveSnapshotPath(repoRoot, issuesArg);
        IReadOnlyList<GithubIssueSnapshot> issues = [];
        if (snapshotPath is not null) issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);
        else Console.WriteLine("[github-forward-hitl] WARN: kein Issue-Snapshot - unmapped PBIs koennen nur CREATE.");

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "github-forward");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var checkpointDir = run.OutputDir("checkpoints");
        var snapshotRel = snapshotPath is null ? null : Path.GetRelativePath(repoRoot, snapshotPath);

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "GithubForwardAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var humanGate = RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate");
        var workflow = GithubForwardHitlWorkflow.Build(
            new GithubForwardSeedExecutor(run), new GithubForwardMakerExecutor(factory, run), new GithubForwardGateExecutor(run),
            new GithubForwardRepairExecutor(factory, run), new GithubForwardHitlFinalizeExecutor(run),
            humanGate, new GithubForwardApplyExecutor(run, repoRoot, outDir, repoArg, tokenEnv));

        var ctx = new GithubForwardWfContext(core, delta.Entries, mappingByPbi, issues, repoArg, sourcePbiUpdateRun, outDir, snapshotRel, dryRun, maxAttempts);
        Console.WriteLine($"[{Cmd}] start runId={run.RunId} deltaPbis={delta.Entries.Count} model={genSettings.ModelId} dryRun={dryRun} maxAttempts={maxAttempts}");
        return await HitlShell.StartAsync(Cmd, workflow, ctx, run, checkpointDir,
            onManualOutput: data =>
            {
                if (data is not GithubForwardWfResult manual) return null;
                Console.WriteLine($"[{Cmd}] Gate NICHT bestanden ({manual.FinalDecision}) - kein Apply, kein Checkpoint noetig. Plan: {Path.GetRelativePath(repoRoot, outDir)}");
                return 1;
            },
            pausedLines:
            [
                $"[{Cmd}] Plan: {Path.GetRelativePath(repoRoot, outDir)}/github-forward-plan.json",
                $"[{Cmd}] Fortsetzen: {Cmd} resume {run.RunId} --accept-all [--execute]"
            ],
            pointerRepository: repoArg, pointerTokenEnv: tokenEnv).ConfigureAwait(false);
    }

    // ---------------- RESUME (Prozess B): Checkpoint restaurieren, Entscheidung uebergeben, Apply. ----------------
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var execute = args.Contains("--execute", StringComparer.OrdinalIgnoreCase);
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        var uiMode = args.Contains("--ui", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        string? runToken = null, repoArg = null, tokenEnv = null, acceptList = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--execute", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--accept-all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "--ui", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--no-browser", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (string.Equals(a, "--token-env", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { tokenEnv = args[++i]; continue; }
            if (string.Equals(a, "--accept", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { acceptList = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward-hitl] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var planDir = GithubForwardReviewRunner.ResolvePlanDir(repoRoot, runToken);
        if (planDir is null) { Console.Error.WriteLine($"[github-forward-hitl] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runDir = Path.GetDirectoryName(planDir)!;               // .../runs/github-forward/<runId>
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointer = await HitlShell.LoadPointerAsync(Cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        // Akzeptierte OpIds: im UI-Modus interaktiv am Human-Gate (unten), sonst vorab aus --accept-all | --accept | Datei.
        var plan = await HitlShell.LoadAsync<GithubForwardPlanDocument>(Path.Combine(planDir, "github-forward-plan.json")).ConfigureAwait(false);
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        IReadOnlyList<string>? accepted = null;
        if (!uiMode)
        {
            accepted = await HumanDecisions.ResolveAcceptedAsync(acceptAll, acceptList, decisionsPath,
                allIds: () => Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToList(),
                acceptedFromFile: async p =>
                    (await HitlShell.LoadAsync<GithubForwardDecisionsFile>(p).ConfigureAwait(false)).Decisions
                        .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList()).ConfigureAwait(false);
            if (accepted is null) { Console.Error.WriteLine("[github-forward-hitl] keine Entscheidung: --ui, --accept-all, --accept op-0,.. oder human-decisions.json noetig."); return 2; }
        }

        // Denselben Lauf rekonstruieren (gleicher runId -> gleicher Ordner/Logs). Maker/Repair laufen beim Resume NICHT.
        var run = new RunContext(runId, "github-forward");
        var outDir = run.OutputDir("plan");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Maker darf beim Resume nicht laufen.");
        var humanGate = RequestPort.Create<ForwardReviewRequest, ForwardReviewResponse>("github-forward-gate");
        var workflow = GithubForwardHitlWorkflow.Build(
            new GithubForwardSeedExecutor(run), new GithubForwardMakerExecutor(noAgent, run), new GithubForwardGateExecutor(run),
            new GithubForwardRepairExecutor(noAgent, run), new GithubForwardHitlFinalizeExecutor(run),
            humanGate, new GithubForwardApplyExecutor(run, repoRoot, outDir, repoArg ?? pointer.Repository, tokenEnv ?? pointer.TokenEnv));

        Console.WriteLine($"[{Cmd}] resume runId={runId} mode={(uiMode ? "ui" : "cli")} execute={execute}");
        return await HitlShell.ResumeAsync(Cmd, workflow, runId, checkpointDir, pointer, uiMode,
            makeResponse: async () =>
            {
                // S2: der offene Request wird prozessuebergreifend re-emittiert. Entscheidung holen (UI oder CLI),
                // als human-decisions.json festhalten (Evidenz/Fallback) und zurueckgeben.
                var acc = uiMode
                    ? await CollectViaUiAsync(runId, plan, decisionsPath, settings, noBrowser, repoRoot).ConfigureAwait(false)
                    : accepted!;
                return new ForwardReviewResponse(acc, execute, uiMode ? "human (review-ui)" : "author (cli)");
            },
            onOutput: data =>
            {
                if (data is not GithubForwardApplyReport report) return null;
                var s = report.Summary;
                Console.WriteLine(report.Executed
                    ? $"[{Cmd}] EXECUTED accepted={s.Accepted} created={s.Created} updated={s.Updated} linked={s.Linked} alreadyApplied={s.AlreadyApplied} rejected={s.Rejected} failed={s.Failed}"
                    : $"[{Cmd}] DRY-RUN accepted={s.Accepted} wouldCreate={s.Created} wouldLink={s.Linked} rejected={s.Rejected}");
                Console.WriteLine($"[{Cmd}] -> {Path.GetRelativePath(repoRoot, Path.Combine(planDir, "applied"))}");
                return report.Success ? 0 : 1;
            }).ConfigureAwait(false);
    }

    // S2: Entscheidung interaktiv ueber die generische HumanReview-UI holen (derselbe Server/Adapter wie
    // github-forward-review). Nach Fertig -> human-decisions.json (Evidenz/Fallback) + akzeptierte OpIds.
    private static async Task<IReadOnlyList<string>> CollectViaUiAsync(
        string runId, GithubForwardPlanDocument plan, string decisionsPath, HostSettings settings, bool noBrowser, string repoRoot)
    {
        // E0.2a: Snapshot des Laufs (Summary im planDir verlinkt ihn) fuer Vorher/Nachher bei UPDATE — best-effort.
        var planDir = Path.GetDirectoryName(decisionsPath)!;
        var issuesByNumber = await GithubForwardReviewRunner.LoadIssuesAsync(repoRoot, planDir).ConfigureAwait(false);

        var session = GithubForwardReviewAdapter.BuildSession(runId, plan, issuesByNumber);
        var existing = File.Exists(decisionsPath) ? await HitlShell.LoadAsync<GithubForwardDecisionsFile>(decisionsPath).ConfigureAwait(false) : null;
        GithubForwardReviewAdapter.MergeExistingDecisions(session, existing);

        var (decisions, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: GithubForwardReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(GithubForwardReviewAdapter.ResolveContext(key, plan, issuesByNumber)),
            apply: s => GithubForwardReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);

        var accepted = decisions.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId).ToList();
        Console.WriteLine($"[{Cmd}] UI {outcome}: {accepted.Count}/{plan.Operations.Count} akzeptiert -> human-decisions.json");
        return accepted;
    }

    private static string? ResolveSnapshotPath(string repoRoot, string? issuesArg)
    {
        if (!string.IsNullOrWhiteSpace(issuesArg))
            return Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg);
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, "github-issues-snapshot.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private static Task<T> LoadAsync<T>(string path) => HitlShell.LoadAsync<T>(path); // R2: geteilt (StartAsync/ResolveAccepted nutzen es)
}
