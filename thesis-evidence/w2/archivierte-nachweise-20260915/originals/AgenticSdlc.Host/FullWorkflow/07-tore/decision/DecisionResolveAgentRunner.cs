using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// CLI: decision-resolve-agent <stakeholder-answer.txt|inline-text> [model]
// T2.2 — agentischer Aufsatz auf T2.1 als MAF-Workflow: Maker -> Derive -> Gate -> Finalize. Der Maker deutet die
// freie Antwort; Derive/Gate/Finalize sind DIESELBEN Executor-Knoten wie im deterministischen decision-resolve.
public static class DecisionResolveAgentRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "DecisionResolverAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: decision-resolve-agent <stakeholder-answer.txt|text> [model]"); return 2; }
        var answerArg = args[1];
        var maxAttempts = 2;
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) modelArg ??= args[i];
        }

        var answerPath = Path.IsPathRooted(answerArg) ? answerArg : Path.Combine(repoRoot, answerArg);
        var stakeholderAnswer = File.Exists(answerPath) ? await File.ReadAllTextAsync(answerPath).ConfigureAwait(false) : answerArg;
        if (string.IsNullOrWhiteSpace(stakeholderAnswer)) { Console.Error.WriteLine("[decision-resolve-agent] leere Stakeholder-Antwort."); return 2; }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[decision-resolve-agent] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var openCount = core.Items.Count(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase)
            && i.ReadStatus().IsOpenDecision);
        if (openCount == 0) { Console.WriteLine("[decision-resolve-agent] keine offenen Decisions im Core."); return 0; }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "decision");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
            Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "DecisionResolverAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var workflow = DecisionResolveWorkflow.BuildAgentic(
            new DecisionMakerExecutor(factory, run),
            new DecisionDeriveExecutor(run), new DecisionGateExecutor(run),
            new DecisionRepairExecutor(factory, run), new DecisionFinalizeExecutor(run));
        var ctx = new DecisionWfContext(core, outDir, maxAttempts, stakeholderAnswer);

        Console.WriteLine($"[decision-resolve-agent] running runId={run.RunId} model={genSettings.ModelId} openDecisions={openCount} maxAttempts={maxAttempts} (Maker->Derive->Gate->[Repair]/Finalize)");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new DecisionAnswerMsg(ctx), run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) { Console.Error.WriteLine($"[decision-resolve-agent] Resolver-Agent fehlgeschlagen: {ex.Message}"); return 4; }

        return await DecisionResolveRunner.ReportAsync(repoRoot, outDir, "decision-resolve-agent").ConfigureAwait(false);
    }
}
