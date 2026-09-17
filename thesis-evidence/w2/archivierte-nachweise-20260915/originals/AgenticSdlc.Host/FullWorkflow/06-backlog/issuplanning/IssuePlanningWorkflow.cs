using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed record IssuePlanningDraft(IssuePlanningInput Input, IssuePlanDocument Plan, bool Saved, int ToolCheckRounds);
internal sealed record IssuePlanningVerdict(IssuePlanningInput Input, IssuePlanDocument Plan, IssuePlanGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record IssuePlanningResult(IssuePlanDocument Plan, IssuePlanGateReport Report);

[SendsMessage(typeof(IssuePlanningDraft))]
internal sealed class IssuePlanningAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run,
    string sourceIssuePlanningInputPath)
    : Executor<IssuePlanningInput>("L4IssuePlanningAgent")
{
    public override async ValueTask HandleAsync(IssuePlanningInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new IssuePlanningTools(input, sourceIssuePlanningInputPath, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Beginne jetzt mit plan-only Issue Planning.

                   Du bekommst ausschliesslich Requirements, die durch RequirementsReadiness fuer Issue Planning
                   freigegeben wurden. Erkunde sie mit deinen Tools.

                   Erzeuge einen IssuePlan. Plane keine GitHub-Schreiboperation. Pruefe deinen Plan mit
                   check_issue_plan und speichere genau einmal mit save_issue_plan.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var plan = tools.SavedPlan ?? new IssuePlanDocument(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourceIssuePlanningInputPath,
            Items: []);
        run.AppendEvent(new
        {
            type = "ISSUE_PLANNING_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            items = plan.Items.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new IssuePlanningDraft(input, plan, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(IssuePlanningVerdict))]
internal sealed class IssuePlanningGateExecutor(RunContext run) : Executor<IssuePlanningDraft>("IssuePlanGate")
{
    public override async ValueTask HandleAsync(IssuePlanningDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = IssuePlanGate.Check(draft.Input, draft.Plan);
        run.AppendEvent(new
        {
            type = "ISSUE_PLAN_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new IssuePlanningVerdict(draft.Input, draft.Plan, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(IssuePlanningResult))]
internal sealed class IssuePlanningFinalizeExecutor(RunContext run, string outDir) : Executor<IssuePlanningVerdict>("IssuePlanningFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(IssuePlanningVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "issue-plan.json"), JsonSerializer.Serialize(verdict.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "issue-plan-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "issue-plan-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            verdict.Report.Pass,
            verdict.Report.Decision,
            items = verdict.Plan.Items.Count,
            errors = verdict.Report.Errors.Count,
            warnings = verdict.Report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "ISSUE_PLANNING_DONE",
            runId = run.RunId,
            pass = verdict.Report.Pass,
            decision = verdict.Report.Decision,
            items = verdict.Plan.Items.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new IssuePlanningResult(verdict.Plan, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class IssuePlanningWorkflow
{
    public const string WorkflowName = "L4-IssuPlanning";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        IssuePlanningAgentExecutor agent,
        IssuePlanningGateExecutor gate,
        IssuePlanningFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("IssuePlanningInput -> L4IssuePlanningAgent[Tools] -> IssuePlanGate[det] -> Finalize. "
                           + "Der Agent erzeugt nur IssuePlans; GitHub-Write erfolgt spaeter separat nach HumanReview.");

        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
