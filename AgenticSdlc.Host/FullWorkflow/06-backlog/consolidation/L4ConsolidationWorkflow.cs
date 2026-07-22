using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed record L4ConsolidationDraft(ProjectStateDocument State, ConsolidationPlan Plan, bool Saved, int ToolCheckRounds);
internal sealed record L4ConsolidationVerdict(ProjectStateDocument State, ConsolidationPlan Plan, ConsolidationGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record L4ConsolidationResult(ConsolidationPlan Plan, ConsolidationGateReport Report);

[SendsMessage(typeof(L4ConsolidationDraft))]
internal sealed class L4ConsolidationAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run,
    string sourceProjectStatePath)
    : Executor<ProjectStateDocument>("L4-ConsolidationAgent")
{
    public override async ValueTask HandleAsync(ProjectStateDocument state, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new L4ConsolidationTools(state, sourceProjectStatePath, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Beginne jetzt mit der L4-Konsolidierung.

                   Du bekommst den ProjectState nicht als kompletten Prompt-Dump. Erkunde ihn mit deinen Tools.
                   Erzeuge einen ConsolidationPlan. Pruefe ihn mit check_consolidation_plan und speichere genau einmal
                   mit save_consolidation_plan.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var plan = tools.SavedPlan ?? new ConsolidationPlan(
            SchemaVersion: ConsolidationPlan.CurrentSchemaVersion,
            ProjectId: state.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            SourceProjectStatePath: sourceProjectStatePath,
            Operations: []);
        run.AppendEvent(new
        {
            type = "L4_CONSOLIDATION_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            operations = plan.Operations.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L4ConsolidationDraft(state, plan, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(L4ConsolidationVerdict))]
internal sealed class L4ConsolidationGateExecutor(RunContext run) : Executor<L4ConsolidationDraft>("L4-ConsolidationGate")
{
    public override async ValueTask HandleAsync(L4ConsolidationDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = L4ConsolidationGate.Check(draft.State, draft.Plan);
        run.AppendEvent(new
        {
            type = "L4_CONSOLIDATION_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L4ConsolidationVerdict(draft.State, draft.Plan, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(L4ConsolidationResult))]
internal sealed class L4ConsolidationFinalizeExecutor(RunContext run, string outDir) : Executor<L4ConsolidationVerdict>("L4-ConsolidationFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(L4ConsolidationVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "consolidation-plan.json"), JsonSerializer.Serialize(verdict.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "consolidation-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "consolidation-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            verdict.Report.Pass,
            verdict.Report.Decision,
            operations = verdict.Plan.Operations.Count,
            errors = verdict.Report.Errors.Count,
            warnings = verdict.Report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "L4_CONSOLIDATION_DONE",
            runId = run.RunId,
            pass = verdict.Report.Pass,
            decision = verdict.Report.Decision,
            operations = verdict.Plan.Operations.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new L4ConsolidationResult(verdict.Plan, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class L4ConsolidationWorkflow
{
    public const string WorkflowName = "L4-Consolidation";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        L4ConsolidationAgentExecutor agent,
        L4ConsolidationGateExecutor gate,
        L4ConsolidationFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("ProjectState -> ConsolidationPlan[Agent+Tools] -> Gate[det] -> Finalize. "
                           + "Der Agent schreibt nur einen Plan; Projektwahrheit entsteht erst nach Gate/HumanReview/Apply.");

        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
