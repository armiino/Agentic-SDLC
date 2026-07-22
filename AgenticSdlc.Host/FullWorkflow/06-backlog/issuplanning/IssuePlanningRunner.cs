using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanningRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "L4IssuePlanningAgent";
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

        string? inputArg = null;
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
                Console.Error.WriteLine($"[l4-issuplanning] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            inputArg ??= arg;
        }

        if (string.IsNullOrWhiteSpace(inputArg))
        {
            Usage();
            return 2;
        }

        IssuePlanningView view;
        try
        {
            view = await LoadIssuePlanningViewAsync(repoRoot, inputArg).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning] IssuePlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var input = view.Input;
        var plan = IssuePlanFactory.CreateOneIssuePerRequirement(input, Path.GetRelativePath(repoRoot, view.IssuePlanningInputPath));
        var gate = IssuePlanGate.Check(input, plan);
        var outputPath = ResolvePath(repoRoot, output ?? Path.Combine(view.SourceDirectory, "issue-plan-seed.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(outputPath) ?? ".", "issue-plan-seed-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-issuplanning] seed items={plan.Items.Count} gate={gate.Decision} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[l4-issuplanning] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunCheckAsync(string[] args, string repoRoot)
    {
        if (args.Length < 4)
        {
            Usage();
            return 2;
        }

        string? inputArg = null;
        string? planArg = null;
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
                Console.Error.WriteLine($"[l4-issuplanning] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else planArg ??= arg;
        }

        if (string.IsNullOrWhiteSpace(planArg))
        {
            Usage();
            return 2;
        }

        IssuePlanningView view;
        try
        {
            view = await LoadIssuePlanningViewAsync(repoRoot, inputArg ?? "").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning] IssuePlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var input = view.Input;
        var planPath = ResolvePath(repoRoot, planArg);
        var plan = await LoadPlanAsync(planPath).ConfigureAwait(false);
        var gate = IssuePlanGate.Check(input, plan);
        var outputPath = ResolvePath(repoRoot, output ?? Path.Combine(Path.GetDirectoryName(planPath) ?? ".", "issue-plan-gate-report.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-issuplanning] gate={gate.Decision} pass={gate.Pass} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[l4-issuplanning] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunAgentAsync(string[] args, HostSettings settings, string repoRoot)
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
                Console.Error.WriteLine($"[l4-issuplanning] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else modelArg ??= arg;
        }

        IssuePlanningView view;
        try
        {
            view = await LoadIssuePlanningViewAsync(repoRoot, inputArg ?? "").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning] IssuePlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var input = view.Input;
        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4-issuplanning");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("plan") : ResolvePath(repoRoot, outputDir);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, view.IssuePlanningInputPath);
        run.WriteConfig(new
        {
            workflow = IssuePlanningWorkflow.WorkflowName,
            runId = run.RunId,
            issuePlanningInput = sourceRelativePath,
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

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L4IssuePlanningAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L4IssuePlanningAgent", SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
        {
            var agent = genClient.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };

        var workflow = IssuePlanningWorkflow.Build(
            new IssuePlanningAgentExecutor(agentFactory, run, sourceRelativePath),
            new IssuePlanningGateExecutor(run),
            new IssuePlanningFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[l4-issuplanning] --dry-run: Graph Build()-bar (L4IssuePlanningAgent[Tools] -> Gate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[l4-issuplanning] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[l4-issuplanning] running workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, input, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-issuplanning] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var reportPath = Path.Combine(outDir, "issue-plan-gate-report.json");
        var summaryPath = Path.Combine(outDir, "issue-plan-summary.json");
        var planPath = Path.Combine(outDir, "issue-plan.json");
        if (!File.Exists(reportPath) || !File.Exists(summaryPath) || !File.Exists(planPath))
        {
            Console.Error.WriteLine("[l4-issuplanning] Ausfuehrung unvollstaendig: issue-plan.json, issue-plan-summary.json oder issue-plan-gate-report.json fehlt.");
            Console.WriteLine($"[l4-issuplanning] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        var report = JsonSerializer.Deserialize<IssuePlanGateReport>(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false), Json);
        var summary = JsonSerializer.Deserialize<IssuePlanRunSummary>(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false), Json);
        var plan = await LoadPlanAsync(planPath).ConfigureAwait(false);
        if (report is null || summary is null)
        {
            Console.Error.WriteLine("[l4-issuplanning] Ausfuehrung unvollstaendig: Summary oder Gate Report konnte nicht gelesen werden.");
            Console.WriteLine($"[l4-issuplanning] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        Console.WriteLine($"[l4-issuplanning] gate={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[l4-issuplanning] saved={summary.Saved} toolCheckRounds={summary.ToolCheckRounds} items={plan.Items.Count}");
        Console.WriteLine($"[l4-issuplanning] -> {Path.GetRelativePath(repoRoot, outDir)}");
        if (!summary.Saved || plan.Items.Count == 0)
        {
            Console.Error.WriteLine("[l4-issuplanning] Agent hat keinen gueltigen IssuePlan gespeichert.");
            return 4;
        }
        return report.Pass ? 0 : 1;
    }

    private static async Task<IssuePlanningView> LoadIssuePlanningViewAsync(string repoRoot, string token)
    {
        var viewRepository = new JsonProjectStateViewRepository(repoRoot);
        return await viewRepository.GetIssuePlanningViewAsync(
            ProjectScope.FromSourcePath(token, "issue-planning", "current_baseline")).ConfigureAwait(false);
    }

    private static async Task<IssuePlanDocument> LoadPlanAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<IssuePlanDocument>(json, Json)
               ?? throw new InvalidOperationException($"IssuePlan konnte nicht gelesen werden: {path}");
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[l4-issuplanning] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-issuplanning seed <issue-planning-input.json|l4RunId> [--out <issue-plan.json>]");
        Console.Error.WriteLine("       l4-issuplanning check <issue-planning-input.json|l4RunId> <issue-plan.json> [--out <report.json>]");
        Console.Error.WriteLine("       l4-issuplanning agent <issue-planning-input.json|l4RunId> [model] [--out <dir>] [--dry-run]");
    }

    private sealed record IssuePlanRunSummary(
        bool Saved,
        int ToolCheckRounds,
        bool Pass,
        string Decision,
        int Items,
        int Errors,
        int Warnings);
}
