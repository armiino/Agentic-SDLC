using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

/// <summary>
/// L4 ConsolidationPlan utilities.
/// Usage:
///   l4-consolidation seed <project-state.json> [--out <plan.json>]
///   l4-consolidation check <project-state.json> <consolidation-plan.json> [--out <report.json>]
///   l4-consolidation agent <project-state.json> [model] [--out <dir>] [--dry-run]
/// </summary>
public static class L4ConsolidationRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "L4RequirementsConsolidationAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        return args[1].ToLowerInvariant() switch
        {
            "seed" => await RunSeedAsync(args, repoRoot).ConfigureAwait(false),
            "check" => await RunCheckAsync(args, repoRoot).ConfigureAwait(false),
            "agent" => await RunAgentAsync(args, settings, repoRoot).ConfigureAwait(false),
            _ => UnknownMode(args[1])
        };
    }

    private static async Task<int> RunSeedAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        string? projectState = null;
        string? output = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                output = args[++i];
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            projectState ??= arg;
        }

        if (string.IsNullOrWhiteSpace(projectState))
        {
            Console.Error.WriteLine("[l4] project-state.json ist erforderlich.");
            Usage();
            return 2;
        }

        var statePath = ResolvePath(repoRoot, projectState);
        var state = await LoadProjectStateAsync(statePath).ConfigureAwait(false);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, statePath);
        var plan = L4ConsolidationPlanFactory.CreateIdentityPlan(state, sourceRelativePath);

        output ??= Path.Combine("runs", "l4", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"), "consolidation-plan.json");
        var outputPath = ResolvePath(repoRoot, output);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(plan, ProjectStateJson.Options)).ConfigureAwait(false);

        Console.WriteLine($"[l4] consolidation seed operations={plan.Operations.Count}");
        Console.WriteLine($"[l4] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return 0;
    }

    private static async Task<int> RunCheckAsync(string[] args, string repoRoot)
    {
        if (args.Length < 4)
        {
            Usage();
            return 2;
        }

        string? projectState = null;
        string? planPathArg = null;
        string? output = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                output = args[++i];
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (projectState is null) projectState = arg;
            else planPathArg ??= arg;
        }

        if (string.IsNullOrWhiteSpace(projectState) || string.IsNullOrWhiteSpace(planPathArg))
        {
            Console.Error.WriteLine("[l4] project-state.json und consolidation-plan.json sind erforderlich.");
            Usage();
            return 2;
        }

        var statePath = ResolvePath(repoRoot, projectState);
        var planPath = ResolvePath(repoRoot, planPathArg);
        var state = await LoadProjectStateAsync(statePath).ConfigureAwait(false);
        var plan = await LoadPlanAsync(planPath).ConfigureAwait(false);
        var report = L4ConsolidationGate.Check(state, plan);

        output ??= Path.Combine(Path.GetDirectoryName(planPath) ?? ".", "consolidation-gate-report.json");
        var outputPath = ResolvePath(repoRoot, output);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(report, ProjectStateJson.Options)).ConfigureAwait(false);

        Console.WriteLine($"[l4] gate decision={report.Decision} pass={report.Pass}");
        Console.WriteLine($"[l4] errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[l4] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return report.Pass ? 0 : 1;
    }

    private static async Task<int> RunAgentAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        string? projectState = null;
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
                Console.Error.WriteLine($"[l4] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (projectState is null) projectState = arg;
            else modelArg ??= arg;
        }

        if (string.IsNullOrWhiteSpace(projectState))
        {
            Console.Error.WriteLine("[l4] project-state.json ist erforderlich.");
            Usage();
            return 2;
        }

        var statePath = ResolvePath(repoRoot, projectState);
        var state = await LoadProjectStateAsync(statePath).ConfigureAwait(false);
        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("consolidation") : ResolvePath(repoRoot, outputDir);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, statePath);
        run.WriteConfig(new
        {
            workflow = L4ConsolidationWorkflow.WorkflowName,
            runId = run.RunId,
            projectState = sourceRelativePath,
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

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L4ConsolidationAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L4-ConsolidationAgent", SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
        {
            var agent = genClient.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };

        var workflow = L4ConsolidationWorkflow.Build(
            new L4ConsolidationAgentExecutor(agentFactory, run, sourceRelativePath),
            new L4ConsolidationGateExecutor(run),
            new L4ConsolidationFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[l4] --dry-run: Graph Build()-bar (ConsolidationAgent[Tools] -> Gate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[l4] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[l4] running consolidation workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, state, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var reportPath = Path.Combine(outDir, "consolidation-gate-report.json");
        if (File.Exists(reportPath))
        {
            var report = JsonSerializer.Deserialize<ConsolidationGateReport>(
                await File.ReadAllTextAsync(reportPath).ConfigureAwait(false),
                ProjectStateJson.Options);
            if (report is not null)
            {
                Console.WriteLine($"[l4] gate decision={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
            }
        }
        Console.WriteLine($"[l4] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return 0;
    }

    private static async Task<ProjectStateDocument> LoadProjectStateAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"ProjectState nicht gefunden: {path}", path);
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ProjectStateDocument>(json, ProjectStateJson.Options)
               ?? throw new InvalidOperationException($"ProjectState konnte nicht gelesen werden: {path}");
    }

    private static async Task<ConsolidationPlan> LoadPlanAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"ConsolidationPlan nicht gefunden: {path}", path);
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ConsolidationPlan>(json, ProjectStateJson.Options)
               ?? throw new InvalidOperationException($"ConsolidationPlan konnte nicht gelesen werden: {path}");
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[l4] unbekannter l4-consolidation mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-consolidation seed <project-state.json> [--out <plan.json>]");
        Console.Error.WriteLine("       l4-consolidation check <project-state.json> <consolidation-plan.json> [--out <report.json>]");
        Console.Error.WriteLine("       l4-consolidation agent <project-state.json> [model] [--out <dir>] [--dry-run]");
    }
}
