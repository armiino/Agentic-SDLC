using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

using AgenticSdlc.Host.FullWorkflow.Tore.Github;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class GithubReconciliationRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "GithubReconciliationAgent";
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

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

        var options = ParseOptions(args, startIndex: 2);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Usage();
            return 2;
        }

        var input = await BuildInputAsync(repoRoot, options.InputArg ?? "", options.Repository, options.IssuesPath, options.MappingsPath).ConfigureAwait(false);
        var plan = GithubActionPlanFactory.CreateSeed(input);
        var gate = GithubActionPlanGate.Check(input, plan);
        var outputDir = ResolvePath(repoRoot, options.Output ?? Path.Combine(Path.GetDirectoryName(ResolvePath(repoRoot, options.InputArg ?? "")) ?? repoRoot, "github-reconciliation"));
        Directory.CreateDirectory(outputDir);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "github-reconciliation-input.json"), JsonSerializer.Serialize(input, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "github-action-plan.json"), JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "github-action-plan-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "github-action-plan-summary.json"), JsonSerializer.Serialize(new
        {
            input.ProjectId,
            input.BaselineId,
            input.Repository,
            actions = plan.Actions.Count,
            gate.Pass,
            gate.Decision,
            errors = gate.Errors.Count,
            warnings = gate.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-reconciliation] seed actions={plan.Actions.Count} gate={gate.Decision} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[github-reconciliation] -> {Path.GetRelativePath(repoRoot, outputDir)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunCheckAsync(string[] args, string repoRoot)
    {
        if (args.Length < 4)
        {
            Usage();
            return 2;
        }

        var options = ParseOptions(args, startIndex: 2, requirePlanArg: true);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Usage();
            return 2;
        }

        var input = await BuildInputAsync(repoRoot, options.InputArg ?? "", options.Repository, options.IssuesPath, options.MappingsPath).ConfigureAwait(false);
        var planPath = ResolvePath(repoRoot, options.PlanArg ?? "");
        var plan = await LoadAsync<GithubActionPlanDocument>(planPath).ConfigureAwait(false);
        var gate = GithubActionPlanGate.Check(input, plan);
        var outputPath = ResolvePath(repoRoot, options.Output ?? Path.Combine(Path.GetDirectoryName(planPath) ?? repoRoot, "github-action-plan-gate-report.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-reconciliation] gate={gate.Decision} pass={gate.Pass} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[github-reconciliation] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunAgentAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        var options = ParseOptions(args, startIndex: 2, allowDryRun: true);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Usage();
            return 2;
        }

        var input = await BuildInputAsync(repoRoot, options.InputArg ?? "", options.Repository, options.IssuesPath, options.MappingsPath).ConfigureAwait(false);
        var genSettings = string.IsNullOrWhiteSpace(options.Model) ? settings : settings with { ModelId = options.Model };
        var run = new RunContext(RunId.New(), "github-reconciliation");
        run.EnsureFolders();
        var outDir = options.Output is null ? run.OutputDir("plan") : ResolvePath(repoRoot, options.Output);
        run.WriteConfig(new
        {
            workflow = GithubReconciliationWorkflow.WorkflowName,
            runId = run.RunId,
            acceptedIssuePlan = input.SourceAcceptedIssuePlanPath,
            repository = input.Repository,
            existingIssues = input.ExistingIssues.Count,
            existingMappings = input.ExistingMappings.Count,
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

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "GithubReconciliationAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
        {
            var agent = genClient.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };

        var workflow = GithubReconciliationWorkflow.Build(
            new GithubReconciliationAgentExecutor(agentFactory, run),
            new GithubReconciliationGateExecutor(run),
            new GithubReconciliationFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[github-reconciliation] --dry-run: Graph Build()-bar (GithubReconciliationAgent[Tools] -> Gate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[github-reconciliation] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[github-reconciliation] running workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, input, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-reconciliation] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var reportPath = Path.Combine(outDir, "github-action-plan-gate-report.json");
        var summaryPath = Path.Combine(outDir, "github-action-plan-summary.json");
        var planPath = Path.Combine(outDir, "github-action-plan.json");
        if (!File.Exists(reportPath) || !File.Exists(summaryPath) || !File.Exists(planPath))
        {
            Console.Error.WriteLine("[github-reconciliation] Ausfuehrung unvollstaendig: github-action-plan.json, summary oder gate-report fehlt.");
            Console.WriteLine($"[github-reconciliation] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        var report = await LoadAsync<GithubActionPlanGateReport>(reportPath).ConfigureAwait(false);
        var summary = await LoadAsync<GithubActionPlanRunSummary>(summaryPath).ConfigureAwait(false);
        var plan = await LoadAsync<GithubActionPlanDocument>(planPath).ConfigureAwait(false);
        Console.WriteLine($"[github-reconciliation] gate={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[github-reconciliation] saved={summary.Saved} toolCheckRounds={summary.ToolCheckRounds} actions={plan.Actions.Count}");
        Console.WriteLine($"[github-reconciliation] -> {Path.GetRelativePath(repoRoot, outDir)}");
        if (!summary.Saved || plan.Actions.Count == 0)
        {
            Console.Error.WriteLine("[github-reconciliation] Agent hat keinen gueltigen GitHubActionPlan gespeichert.");
            return 4;
        }
        return report.Pass ? 0 : 1;
    }

    private static async Task<GithubReconciliationInput> BuildInputAsync(
        string repoRoot,
        string acceptedIssuePlanToken,
        string? repository,
        string? issuesPath,
        string? mappingsPath)
    {
        var viewRepository = new JsonProjectStateViewRepository(repoRoot);
        var scope = ProjectScope.FromSourcePath(acceptedIssuePlanToken, "github-reconciliation", "accepted_issue_plan");
        IssuePlanDocument acceptedPlan;
        string acceptedPlanPath;
        try
        {
            var accepted = await viewRepository.GetAcceptedIssuePlanViewAsync(scope).ConfigureAwait(false);
            acceptedPlan = accepted.Plan;
            acceptedPlanPath = accepted.AcceptedIssuePlanPath;
        }
        catch (FileNotFoundException)
        {
            var acceptedClarification = await viewRepository.GetAcceptedClarificationPlanViewAsync(
                ProjectScope.FromSourcePath(acceptedIssuePlanToken, "github-reconciliation", "accepted_clarification_plan")).ConfigureAwait(false);
            acceptedPlanPath = acceptedClarification.AcceptedClarificationPlanPath;
            acceptedPlan = ClarificationPlanIssueProjection.ToIssuePlan(
                acceptedClarification.Plan,
                Path.GetRelativePath(repoRoot, acceptedPlanPath));
        }
        var issues = string.IsNullOrWhiteSpace(issuesPath)
            ? []
            : await LoadAsync<IReadOnlyList<GithubIssueSnapshot>>(ResolvePath(repoRoot, issuesPath)).ConfigureAwait(false);
        var mappings = string.IsNullOrWhiteSpace(mappingsPath)
            ? []
            : await LoadAsync<IReadOnlyList<GithubIssueMapping>>(ResolvePath(repoRoot, mappingsPath)).ConfigureAwait(false);

        return new GithubReconciliationInput(
            SchemaVersion: GithubReconciliationInput.CurrentSchemaVersion,
            ProjectId: acceptedPlan.ProjectId,
            BaselineId: acceptedPlan.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAcceptedIssuePlanPath: Path.GetRelativePath(repoRoot, acceptedPlanPath),
            Repository: repository,
            AcceptedIssuePlan: acceptedPlan,
            ExistingIssues: issues,
            ExistingMappings: mappings);
    }

    private static GithubReconciliationCliOptions ParseOptions(string[] args, int startIndex, bool requirePlanArg = false, bool allowDryRun = false)
    {
        string? inputArg = null;
        string? planArg = null;
        string? output = null;
        string? repository = null;
        string? issues = null;
        string? mappings = null;
        string? model = null;

        for (var i = startIndex; i < args.Length; i++)
        {
            var arg = args[i];
            if (allowDryRun && string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (TryReadOption(arg, "--out", args, ref i, out var outValue))
            {
                output = outValue;
                continue;
            }
            if (TryReadOption(arg, "--repo", args, ref i, out var repoValue))
            {
                repository = repoValue;
                continue;
            }
            if (TryReadOption(arg, "--issues", args, ref i, out var issuesValue))
            {
                issues = issuesValue;
                continue;
            }
            if (TryReadOption(arg, "--mappings", args, ref i, out var mappingsValue))
            {
                mappings = mappingsValue;
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
                return new GithubReconciliationCliOptions(null, null, null, null, null, null, null, $"[github-reconciliation] unbekanntes Argument: {arg}");

            if (inputArg is null) inputArg = arg;
            else if (requirePlanArg && planArg is null) planArg = arg;
            else if (allowDryRun && model is null) model = arg;
            else return new GithubReconciliationCliOptions(null, null, null, null, null, null, null, $"[github-reconciliation] unerwartetes Argument: {arg}");
        }

        if (string.IsNullOrWhiteSpace(inputArg))
            return new GithubReconciliationCliOptions(null, null, null, null, null, null, null, "[github-reconciliation] accepted IssuePlan fehlt.");
        if (requirePlanArg && string.IsNullOrWhiteSpace(planArg))
            return new GithubReconciliationCliOptions(null, null, null, null, null, null, null, "[github-reconciliation] github-action-plan.json fehlt.");
        return new GithubReconciliationCliOptions(inputArg, planArg, output, repository, issues, mappings, model, null);
    }

    private static bool TryReadOption(string arg, string name, string[] args, ref int index, out string? value)
    {
        value = null;
        if (!string.Equals(arg, name, StringComparison.OrdinalIgnoreCase)) return false;
        if (index + 1 >= args.Length) throw new ArgumentException($"Option {name} braucht einen Wert.");
        value = args[++index];
        return true;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[github-reconciliation] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-reconciliation seed <accepted-issue-plan-dir|runId> [--out <dir>] [--repo owner/name] [--issues <issues.json>] [--mappings <mappings.json>]");
        Console.Error.WriteLine("       github-reconciliation check <accepted-issue-plan-dir|runId> <github-action-plan.json> [--out <report.json>] [--repo owner/name] [--issues <issues.json>] [--mappings <mappings.json>]");
        Console.Error.WriteLine("       github-reconciliation agent <accepted-issue-plan-dir|runId> [model] [--out <dir>] [--repo owner/name] [--issues <issues.json>] [--mappings <mappings.json>] [--dry-run]");
    }

    private sealed record GithubReconciliationCliOptions(
        string? InputArg,
        string? PlanArg,
        string? Output,
        string? Repository,
        string? IssuesPath,
        string? MappingsPath,
        string? Model,
        string? Error);

    private sealed record GithubActionPlanRunSummary(
        bool Saved,
        int ToolCheckRounds,
        bool Pass,
        string Decision,
        int Actions,
        int Errors,
        int Warnings);
}
