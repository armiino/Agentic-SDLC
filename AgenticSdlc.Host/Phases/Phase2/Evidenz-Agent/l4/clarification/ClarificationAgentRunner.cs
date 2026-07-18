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

public static class ClarificationAgentRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "ClarificationPlanningAgent";
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
            "plan" => await RunPlanAsync(args, settings, repoRoot).ConfigureAwait(false),
            "resolve" => await RunResolveAsync(args, repoRoot).ConfigureAwait(false),
            _ => UnknownMode(args[1])
        };
    }

    private static async Task<int> RunPlanAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        return args[2].ToLowerInvariant() switch
        {
            "seed" => await RunPlanSeedAsync(args, repoRoot).ConfigureAwait(false),
            "check" => await RunPlanCheckAsync(args, repoRoot).ConfigureAwait(false),
            "agent" => await RunPlanAgentAsync(args, settings, repoRoot).ConfigureAwait(false),
            _ => UnknownPlanMode(args[2])
        };
    }

    private static async Task<int> RunPlanSeedAsync(string[] args, string repoRoot)
    {
        if (args.Length < 4)
        {
            Usage();
            return 2;
        }

        string? inputArg = null;
        string? output = null;
        for (var i = 3; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                output = args[++i];
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[clarification-agent] unbekanntes Argument: {arg}");
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

        ClarificationPlanningView view;
        try
        {
            view = await LoadClarificationPlanningViewAsync(repoRoot, inputArg).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clarification-agent] ClarificationPlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var sourceRelativePath = Path.GetRelativePath(repoRoot, view.ClarificationPlanningInputPath);
        var plan = ClarificationPlanFactory.CreateOneClarificationIssuePerItem(view.Input, sourceRelativePath);
        var gate = ClarificationPlanGate.Check(view.Input, plan);
        var outputPath = ResolvePath(repoRoot, output ?? Path.Combine(view.SourceDirectory, "clarification-plan-seed.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(outputPath) ?? ".", "clarification-plan-seed-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[clarification-agent] plan seed items={plan.Items.Count} gate={gate.Decision} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[clarification-agent] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunPlanCheckAsync(string[] args, string repoRoot)
    {
        if (args.Length < 5)
        {
            Usage();
            return 2;
        }

        string? inputArg = null;
        string? planArg = null;
        string? output = null;
        for (var i = 3; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                output = args[++i];
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[clarification-agent] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else planArg ??= arg;
        }

        if (string.IsNullOrWhiteSpace(inputArg) || string.IsNullOrWhiteSpace(planArg))
        {
            Usage();
            return 2;
        }

        ClarificationPlanningView view;
        try
        {
            view = await LoadClarificationPlanningViewAsync(repoRoot, inputArg).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clarification-agent] ClarificationPlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var planPath = ResolvePath(repoRoot, planArg);
        var plan = await LoadPlanAsync(planPath).ConfigureAwait(false);
        var gate = ClarificationPlanGate.Check(view.Input, plan);
        var outputPath = ResolvePath(repoRoot, output ?? Path.Combine(Path.GetDirectoryName(planPath) ?? ".", "clarification-plan-gate-report.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);

        Console.WriteLine($"[clarification-agent] plan gate={gate.Decision} pass={gate.Pass} errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
        Console.WriteLine($"[clarification-agent] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<int> RunPlanAgentAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        string? inputArg = null;
        string? outputDir = null;
        string? modelArg = null;
        for (var i = 3; i < args.Length; i++)
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
                Console.Error.WriteLine($"[clarification-agent] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else modelArg ??= arg;
        }

        ClarificationPlanningView view;
        try
        {
            view = await LoadClarificationPlanningViewAsync(repoRoot, inputArg ?? "").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clarification-agent] ClarificationPlanningView nicht gefunden: {ex.Message}");
            return 2;
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "clarification-agent");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("plan") : ResolvePath(repoRoot, outputDir);
        var sourceRelativePath = Path.GetRelativePath(repoRoot, view.ClarificationPlanningInputPath);
        run.WriteConfig(new
        {
            workflow = ClarificationPlanningWorkflow.WorkflowName,
            mode = "plan",
            runId = run.RunId,
            clarificationPlanningInput = sourceRelativePath,
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

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "ClarificationPlanningAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
        {
            var agent = genClient.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools]);
            return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        };

        var workflow = ClarificationPlanningWorkflow.Build(
            new ClarificationPlanningAgentExecutor(agentFactory, run, sourceRelativePath),
            new ClarificationPlanGateExecutor(run),
            new ClarificationPlanningFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[clarification-agent] --dry-run: Graph Build()-bar (ClarificationPlanningAgent[Tools] -> Gate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[clarification-agent] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[clarification-agent] running plan workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, view.Input, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clarification-agent] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var reportPath = Path.Combine(outDir, "clarification-plan-gate-report.json");
        var summaryPath = Path.Combine(outDir, "clarification-plan-summary.json");
        var planPath = Path.Combine(outDir, "clarification-plan.json");
        if (!File.Exists(reportPath) || !File.Exists(summaryPath) || !File.Exists(planPath))
        {
            Console.Error.WriteLine("[clarification-agent] Ausfuehrung unvollstaendig: clarification-plan.json, summary oder gate-report fehlt.");
            Console.WriteLine($"[clarification-agent] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        var report = JsonSerializer.Deserialize<ClarificationPlanGateReport>(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false), Json);
        var summary = JsonSerializer.Deserialize<ClarificationPlanRunSummary>(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false), Json);
        var plan = await LoadPlanAsync(planPath).ConfigureAwait(false);
        if (report is null || summary is null)
        {
            Console.Error.WriteLine("[clarification-agent] Ausfuehrung unvollstaendig: Summary oder Gate Report konnte nicht gelesen werden.");
            Console.WriteLine($"[clarification-agent] -> {Path.GetRelativePath(repoRoot, outDir)}");
            return 4;
        }

        Console.WriteLine($"[clarification-agent] gate={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[clarification-agent] saved={summary.Saved} toolCheckRounds={summary.ToolCheckRounds} items={plan.Items.Count}");
        Console.WriteLine($"[clarification-agent] -> {Path.GetRelativePath(repoRoot, outDir)}");
        if (!summary.Saved || plan.Items.Count == 0)
        {
            Console.Error.WriteLine("[clarification-agent] Agent hat keinen gueltigen ClarificationPlan gespeichert.");
            return 4;
        }
        return report.Pass ? 0 : 1;
    }

    private static async Task<int> RunResolveAsync(string[] args, string repoRoot)
    {
        string? outputDir = null;
        var describe = false;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputDir = args[++i];
                continue;
            }
            if (string.Equals(arg, "--describe", StringComparison.OrdinalIgnoreCase))
            {
                describe = true;
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[clarification-agent] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
        }

        var contract = BuildResolveContract();
        if (outputDir is not null)
        {
            var dir = ResolvePath(repoRoot, outputDir);
            Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(Path.Combine(dir, "clarification-resolve-tbd.json"), JsonSerializer.Serialize(contract, Json)).ConfigureAwait(false);
            await File.WriteAllTextAsync(Path.Combine(dir, "clarification-resolve-tbd.md"), BuildResolveContractMarkdown(), System.Text.Encoding.UTF8).ConfigureAwait(false);
            Console.WriteLine($"[clarification-agent] resolve TBD contract -> {Path.GetRelativePath(repoRoot, dir)}");
        }

        Console.WriteLine("[clarification-agent] mode=resolve ist bewusst TBD.");
        Console.WriteLine("[clarification-agent] Zweck: offene Klaerungsitems agentisch loesen/vorschlagen statt nur als Klaerungsissues zu planen.");
        Console.WriteLine("[clarification-agent] Contract kann mit --describe --out <dir> materialisiert werden.");
        return describe || outputDir is not null ? 0 : 2;
    }

    private static async Task<ClarificationPlanningView> LoadClarificationPlanningViewAsync(string repoRoot, string token)
    {
        var viewRepository = new JsonProjectStateViewRepository(repoRoot);
        return await viewRepository.GetClarificationPlanningViewAsync(
            ProjectScope.FromSourcePath(token, "clarification-planning", "open_requirements")).ConfigureAwait(false);
    }

    private static async Task<ClarificationPlanDocument> LoadPlanAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ClarificationPlanDocument>(json, Json)
               ?? throw new InvalidOperationException($"ClarificationPlan konnte nicht gelesen werden: {path}");
    }

    private static object BuildResolveContract()
        => new
        {
            mode = "resolve",
            status = "tbd",
            purpose = "Agentischer Vorschlag zur fachlichen Loesung offener Klaerungsitems nach HumanReview.",
            intendedWorkflow = "ClarificationResolutionInput -> ClarificationResolutionAgent[ProjectStateTools, optional GithubReadTools] -> ResolutionGate[det/semantic] -> HumanReview -> ApplyProposal",
            nonGoals = new[]
            {
                "keine direkten GitHub-Writes",
                "keine direkten ProjectState-Mutationen ohne Gate und HumanReview",
                "kein Ersetzen des plan-Modus; resolve ist ein optionaler Modus fuer Loesungsvorschlaege"
            },
            futureInputs = new[]
            {
                "accepted-open-requirement-decisions",
                "clarification-plan oder bestehende Clarification-Issues",
                "ProjectState Repository View",
                "optional GitHub-Kommentare, Issue-Status und historische Entscheidungen"
            },
            futureOutputs = new[]
            {
                "ClarificationResolutionProposal mit DECIDE, REVISE_REQUIREMENT, SPLIT, DEFER, ASK_HUMAN",
                "GateReport",
                "HumanReview Session",
                "deterministisches Apply in ProjectState"
            }
        };

    private static string BuildResolveContractMarkdown()
        => """
           # Clarification Agent Resolve Mode (TBD)

           `clarification-agent resolve` soll spaeter offene Klaerungsitems nicht nur als Issues planen,
           sondern fachliche Loesungsvorschlaege erzeugen.

           Geplanter Workflow:
           `ClarificationResolutionInput -> ClarificationResolutionAgent[Tools] -> ResolutionGate -> HumanReview -> Apply`.

           Der Agent darf Loesungen vorschlagen, aber keine Projektwahrheit schreiben. Projektwahrheit entsteht
           erst nach Gate, HumanReview und deterministischem Apply.

           Typische Operationen:
           - `DECIDE`: konkrete fachliche Entscheidung vorschlagen.
           - `REVISE_REQUIREMENT`: bestehendes Requirement mit Entscheidung praezisieren.
           - `SPLIT`: offenes Requirement in mehrere operationalisierbare Items zerlegen.
           - `DEFER`: bewusst offen halten.
           - `ASK_HUMAN`: Entscheidung bleibt menschlich.

           Dieser Modus ist absichtlich noch nicht produktiv implementiert. Der aktuelle produktive Modus ist
           `clarification-agent plan`.
           """;

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[clarification-agent] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static int UnknownPlanMode(string mode)
    {
        Console.Error.WriteLine($"[clarification-agent] unbekannter plan mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: clarification-agent plan seed <clarification-planning-input.json|githubReconciliationRunId> [--out <clarification-plan.json>]");
        Console.Error.WriteLine("       clarification-agent plan check <clarification-planning-input.json|githubReconciliationRunId> <clarification-plan.json> [--out <report.json>]");
        Console.Error.WriteLine("       clarification-agent plan agent <clarification-planning-input.json|githubReconciliationRunId> [model] [--out <dir>] [--dry-run]");
        Console.Error.WriteLine("       clarification-agent resolve --describe [--out <dir>]");
    }
}
