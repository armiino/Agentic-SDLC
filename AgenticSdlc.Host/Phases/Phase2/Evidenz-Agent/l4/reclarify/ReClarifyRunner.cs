using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// CLI-Dispatch fuer den l4-re-clarify-Knoten (RE Backlog Structuring).
// Schritt 2 (neu): `cluster` = agentische Feature-Cluster-Bildung, Maker + deterministisches
// Coverage-Gate + ReviewAgent (MAF-nativ, wie IssuePlanning/L4Completion). Waechst spaeter um
// Clarify/Cut -> PBIs + HumanReview + Apply.
public static class ReClarifyRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string MakerName = "L4ReClarifyClusterAgent";
    private const string ReviewName = "L4ReClarifyClusterReviewAgent";
    private const string ClarifyName = "L4ReClarifyBacklogAgent";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        return args[1].ToLowerInvariant() switch
        {
            "cluster" => await RunClusterAsync(args, settings, repoRoot).ConfigureAwait(false),
            "re-review" => await RunReReviewAsync(args, settings, repoRoot).ConfigureAwait(false),
            "clarify" => await RunClarifyAsync(args, settings, repoRoot).ConfigureAwait(false),
            _ => UnknownMode(args[1])
        };
    }

    // CLARIFY/CUT: aus den (akzeptierten) Feature-Clustern PBIs schneiden (Akzeptanzkriterien + offene Entscheidungen).
    private static async Task<int> RunClarifyAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        string? token = null;
        string? outputDir = null;
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { outputDir = args[++i]; continue; }
            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (arg.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[l4-re-clarify] unbekanntes Argument: {arg}"); Usage(); return 2; }
            if (token is null) token = arg;
            else modelArg ??= arg;
        }
        if (string.IsNullOrWhiteSpace(token)) { Usage(); return 2; }

        var clustersDir = ResolveClustersDir(repoRoot, token);
        if (clustersDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Cluster-Lauf '{token}' nicht gefunden.");
            return 2;
        }
        // bevorzugt applied/feature-clusters.json (nach HumanReview/Apply), sonst feature-clusters.json
        var appliedClusters = Path.Combine(clustersDir, "applied", "feature-clusters.json");
        var clustersPath = File.Exists(appliedClusters) ? appliedClusters : Path.Combine(clustersDir, "feature-clusters.json");
        if (!File.Exists(clustersPath))
        {
            Console.Error.WriteLine("[l4-re-clarify] feature-clusters.json fehlt.");
            return 2;
        }
        var clusters = await LoadAsync<FeatureClusterSet>(clustersPath).ConfigureAwait(false);

        CanonicalRequirementsBaseline baseline;
        try
        {
            var view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
            baseline = view.Baseline;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Baseline nicht gefunden ({clusters.SourceBaselinePath}): {ex.Message}");
            return 2;
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4-re-clarify");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("backlog") : ResolvePath(repoRoot, outputDir);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, clustersPath);
        run.WriteConfig(new
        {
            workflow = ReClarifyBacklogWorkflow.WorkflowName,
            runId = run.RunId,
            clusters = sourceRelativePath,
            provider = settings.LlmProvider,
            model = genSettings.ModelId,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            dryRun,
            timestampUtc = DateTime.UtcNow
        });

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var prompt = PromptProvider.Load(repoRoot, Phase, ClarifyName, "L4ReClarifyBacklogAgent1", vars);
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, ClarifyName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: ClarifyName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var workflow = ReClarifyBacklogWorkflow.Build(
            new ClarifyAgentExecutor(factory, run),
            new BacklogGateExecutor(run),
            new BacklogFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[l4-re-clarify] --dry-run: Graph Build()-bar (ClarifyAgent[Tools] -> BacklogGate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[l4-re-clarify] clusters={clusters.Clusters.Count} baseline={baseline.Requirements.Count} -> {sourceRelativePath}");
            Console.WriteLine($"[l4-re-clarify] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[l4-re-clarify] running backlog workflow runId={run.RunId} model={genSettings.ModelId} clusters={clusters.Clusters.Count}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new ReClarifyBacklogInput(clusters, baseline, sourceRelativePath), run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var summaryPath = Path.Combine(outDir, "backlog-summary.json");
        var backlogPath = Path.Combine(outDir, "product-backlog.json");
        if (!File.Exists(summaryPath) || !File.Exists(backlogPath))
        {
            Console.Error.WriteLine("[l4-re-clarify] Ausfuehrung unvollstaendig: product-backlog.json oder backlog-summary.json fehlt.");
            return 4;
        }
        var summary = JsonSerializer.Deserialize<ClarifyRunSummary>(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false), Json);
        if (summary is null)
        {
            Console.Error.WriteLine("[l4-re-clarify] Summary konnte nicht gelesen werden.");
            return 4;
        }

        Console.WriteLine($"[l4-re-clarify] pbis={summary.Pbis} gate={(summary.GatePass ? "pass" : "fail")} errors={summary.GateErrors} warnings={summary.GateWarnings}");
        Console.WriteLine($"[l4-re-clarify] -> {Path.GetRelativePath(repoRoot, outDir)}");
        if (!summary.Saved || summary.Pbis == 0)
        {
            Console.Error.WriteLine("[l4-re-clarify] Clarify-Agent hat keine PBIs gespeichert.");
            return 4;
        }
        return summary.GatePass ? 0 : 1;
    }

    private sealed record ClarifyRunSummary(
        bool Saved,
        int ToolCheckRounds,
        bool GatePass,
        int GateErrors,
        int GateWarnings,
        int Pbis);

    // Fuehrt NUR den ReviewAgent gegen ein bestehendes feature-clusters.json erneut aus (kein Re-Clustering).
    // Nuetzlich zum Iterieren des Reviews / Erzeugen von Operationen ohne teuren Maker-Lauf. Ueberschreibt
    // cluster-review.json im Ziel-Run, damit l4-re-clarify-review/-apply die neuen Operationen sehen.
    private static async Task<int> RunReReviewAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        string? token = null;
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4-re-clarify] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (token is null) token = arg;
            else modelArg ??= arg;
        }
        if (string.IsNullOrWhiteSpace(token)) { Usage(); return 2; }

        var clustersDir = ResolveClustersDir(repoRoot, token);
        if (clustersDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Cluster-Lauf '{token}' nicht gefunden.");
            return 2;
        }
        var clustersPath = Path.Combine(clustersDir, "feature-clusters.json");
        var gatePath = Path.Combine(clustersDir, "cluster-gate-report.json");
        if (!File.Exists(clustersPath) || !File.Exists(gatePath))
        {
            Console.Error.WriteLine("[l4-re-clarify] feature-clusters.json oder cluster-gate-report.json fehlt.");
            return 2;
        }

        var clusters = await LoadAsync<FeatureClusterSet>(clustersPath).ConfigureAwait(false);
        var gate = await LoadAsync<ReClarifyGateReport>(gatePath).ConfigureAwait(false);

        CanonicalRequirementsBaseline baseline;
        try
        {
            var view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
            baseline = view.Baseline;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Baseline nicht gefunden ({clusters.SourceBaselinePath}): {ex.Message}");
            return 2;
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4-re-clarify");
        run.EnsureFolders();
        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var reviewPrompt = PromptProvider.Load(repoRoot, Phase, ReviewName, "L4ReClarifyClusterReviewAgent1", vars);
        var reviewClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, ReviewName, SourceName);
        var tools = new ReClarifyClusterReviewTools(baseline, run);
        var agent = reviewClient.AsAIAgent(instructions: reviewPrompt, name: ReviewName, tools: [.. tools.Build()])
            .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var clustersJson = JsonSerializer.Serialize(clusters.Clusters, Json);
        var task = $$"""
                   Pruefe die vorgeschlagenen Feature-Cluster als unabhaengiger Requirements Engineer.

                   Vorgeschlagene Cluster:
                   {{clustersJson}}

                   Deterministisches Coverage-Gate: pass={{gate.Pass}}, errors={{gate.Errors.Count}}.

                   Zu JEDEM fachlichen Fehler liefere eine KONKRETE, anwendbare Operation (siehe deine Instruktionen:
                   add_crosscutting/remove_crosscutting/move_core/new_cluster/merge_clusters). Speichere genau einmal
                   mit save_review: verdict=approve + leere operations, wenn die Cluster tragen; sonst verdict=revise
                   mit findings UND operations.
                   """;

        Console.WriteLine($"[l4-re-clarify] re-review runId={run.RunId} model={genSettings.ModelId} clusters={clusters.Clusters.Count} -> {Path.GetRelativePath(repoRoot, clustersDir)}");
        try
        {
            await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Ausfuehrung fehlgeschlagen: {ex.Message}");
            return 4;
        }

        var review = tools.SavedReview ?? new ClusterReviewReport(
            SchemaVersion: ClusterReviewReport.CurrentSchemaVersion,
            ReviewId: $"cluster-review-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            Verdict: "revise",
            Summary: "ReviewAgent hat kein Review gespeichert.",
            Findings: []);
        await File.WriteAllTextAsync(Path.Combine(clustersDir, "cluster-review.json"), JsonSerializer.Serialize(review, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-re-clarify] re-review verdict={review.Verdict} findings={review.Findings.Count} operations={review.Operations.Count}");
        Console.WriteLine($"[l4-re-clarify] -> {Path.GetRelativePath(repoRoot, Path.Combine(clustersDir, "cluster-review.json"))}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveClustersDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "feature-clusters.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "clusters", "feature-clusters.json"))) return Path.Combine(full, "clusters");

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var clusters = Path.Combine(dir, "clusters");
            if (File.Exists(Path.Combine(clusters, "feature-clusters.json"))) return clusters;
            if (File.Exists(Path.Combine(dir, "feature-clusters.json"))) return dir;
        }
        return null;
    }

    private static async Task<int> RunClusterAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        string? inputArg = null;
        string? outputDir = null;
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputDir = args[++i];
                continue;
            }
            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4-re-clarify] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else modelArg ??= arg;
        }

        CanonicalRequirementsView view;
        try
        {
            view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(inputArg ?? "", "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] CanonicalRequirementsView nicht gefunden: {ex.Message}");
            return 2;
        }

        var relations = await RelationLookup.BuildAsync(view.Baseline, repoRoot).ConfigureAwait(false);
        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4-re-clarify");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("clusters") : ResolvePath(repoRoot, outputDir);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, view.BaselinePath);
        run.WriteConfig(new
        {
            workflow = ReClarifyClusterWorkflow.WorkflowName,
            runId = run.RunId,
            baseline = sourceRelativePath,
            relations = relations.HasData,
            provider = settings.LlmProvider,
            model = genSettings.ModelId,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            dryRun,
            timestampUtc = DateTime.UtcNow
        });

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var vars = new Dictionary<string, string> { ["runId"] = run.RunId };
        var makerPrompt = PromptProvider.Load(repoRoot, Phase, MakerName, "L4ReClarifyClusterAgent1", vars);
        var reviewPrompt = PromptProvider.Load(repoRoot, Phase, ReviewName, "L4ReClarifyClusterReviewAgent1", vars);

        var makerClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, MakerName, SourceName);
        var reviewClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, ReviewName, SourceName);

        Func<IReadOnlyList<AITool>, AIAgent> makerFactory = tools =>
        {
            var agent = makerClient.AsAIAgent(instructions: makerPrompt, name: MakerName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };
        Func<IReadOnlyList<AITool>, AIAgent> reviewFactory = tools =>
        {
            var agent = reviewClient.AsAIAgent(instructions: reviewPrompt, name: ReviewName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };

        var workflow = ReClarifyClusterWorkflow.Build(
            new ClusterAgentExecutor(makerFactory, relations, run),
            new ClusterGateExecutor(run),
            new ClusterReviewExecutor(reviewFactory, run),
            new ClusterFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[l4-re-clarify] --dry-run: Graph Build()-bar (ClusterAgent[Tools] -> Gate[det] -> ReviewAgent[Tools] -> Finalize). Kein LLM.");
            Console.WriteLine($"[l4-re-clarify] requirements={view.Baseline.Requirements.Count} relations={relations.HasData}");
            Console.WriteLine($"[l4-re-clarify] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[l4-re-clarify] running cluster workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new ReClarifyClusterInput(view.Baseline, sourceRelativePath), run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var summaryPath = Path.Combine(outDir, "cluster-summary.json");
        var gatePath = Path.Combine(outDir, "cluster-gate-report.json");
        var clustersPath = Path.Combine(outDir, "feature-clusters.json");
        if (!File.Exists(summaryPath) || !File.Exists(gatePath) || !File.Exists(clustersPath))
        {
            Console.Error.WriteLine("[l4-re-clarify] Ausfuehrung unvollstaendig: feature-clusters.json, cluster-gate-report.json oder cluster-summary.json fehlt.");
            Console.WriteLine($"[l4-re-clarify] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        var summary = JsonSerializer.Deserialize<ClusterRunSummary>(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false), Json);
        if (summary is null)
        {
            Console.Error.WriteLine("[l4-re-clarify] Summary konnte nicht gelesen werden.");
            return 4;
        }

        Console.WriteLine($"[l4-re-clarify] clusters={summary.Clusters} gate={(summary.GatePass ? "pass" : "fail")} gateErrors={summary.GateErrors} review={summary.ReviewVerdict} findings={summary.ReviewFindings}");
        Console.WriteLine($"[l4-re-clarify] -> {Path.GetRelativePath(repoRoot, outDir)}");
        if (!summary.Saved || summary.Clusters == 0)
        {
            Console.Error.WriteLine("[l4-re-clarify] Cluster-Agent hat keine gueltigen Cluster gespeichert.");
            return 4;
        }
        return summary.GatePass ? 0 : 1;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[l4-re-clarify] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-re-clarify cluster <l4-applied-dir|runId> [model] [--out <dir>] [--dry-run]");
        Console.Error.WriteLine("       l4-re-clarify re-review <l4-re-clarify-run|dir> [model]   (nur ReviewAgent gegen bestehende Cluster)");
        Console.Error.WriteLine("       l4-re-clarify clarify <l4-re-clarify-run|dir> [model] [--out <dir>] [--dry-run]   (Cluster -> PBIs)");
    }

    private sealed record ClusterRunSummary(
        bool Saved,
        int ToolCheckRounds,
        bool GatePass,
        int GateErrors,
        string ReviewVerdict,
        int ReviewFindings,
        int Clusters);
}
