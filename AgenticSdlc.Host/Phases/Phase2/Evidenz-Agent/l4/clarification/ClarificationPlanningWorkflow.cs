using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed record ClarificationPlanningDraft(ClarificationPlanningInput Input, ClarificationPlanDocument Plan, bool Saved, int ToolCheckRounds);
internal sealed record ClarificationPlanningVerdict(ClarificationPlanningInput Input, ClarificationPlanDocument Plan, ClarificationPlanGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record ClarificationPlanningResult(ClarificationPlanDocument Plan, ClarificationPlanGateReport Report);

[SendsMessage(typeof(ClarificationPlanningDraft))]
internal sealed class ClarificationPlanningAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run,
    string sourceClarificationPlanningInputPath)
    : Executor<ClarificationPlanningInput>("ClarificationPlanningAgent")
{
    public override async ValueTask HandleAsync(ClarificationPlanningInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new ClarificationPlanningTools(input, sourceClarificationPlanningInputPath, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Beginne jetzt mit plan-only Clarification Planning.

                   Du bekommst ausschliesslich offene Requirements, die nach HumanReview als Klaerungs- oder
                   Breakdown-Arbeit weitergefuehrt werden sollen. Erkunde sie mit deinen Tools.

                   Erzeuge einen ClarificationPlan. Plane keine echten GitHub-Schreiboperationen. Pruefe deinen
                   Plan mit check_clarification_plan und speichere genau einmal mit save_clarification_plan.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var plan = tools.SavedPlan ?? new ClarificationPlanDocument(
            SchemaVersion: ClarificationPlanDocument.CurrentSchemaVersion,
            PlanId: $"clarification-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            Mode: "plan",
            CreatedUtc: DateTime.UtcNow,
            SourceClarificationPlanningInputPath: sourceClarificationPlanningInputPath,
            Items: []);
        run.AppendEvent(new
        {
            type = "CLARIFICATION_PLANNING_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            items = plan.Items.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClarificationPlanningDraft(input, plan, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(ClarificationPlanningVerdict))]
internal sealed class ClarificationPlanGateExecutor(RunContext run) : Executor<ClarificationPlanningDraft>("ClarificationPlanGate")
{
    public override async ValueTask HandleAsync(ClarificationPlanningDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ClarificationPlanGate.Check(draft.Input, draft.Plan);
        run.AppendEvent(new
        {
            type = "CLARIFICATION_PLAN_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClarificationPlanningVerdict(draft.Input, draft.Plan, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(ClarificationPlanningResult))]
internal sealed class ClarificationPlanningFinalizeExecutor(RunContext run, string outDir) : Executor<ClarificationPlanningVerdict>("ClarificationPlanningFinalize")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(ClarificationPlanningVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "clarification-plan.json"), JsonSerializer.Serialize(verdict.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "clarification-plan-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "clarification-plan-summary.json"), JsonSerializer.Serialize(new ClarificationPlanRunSummary(
            Saved: verdict.Saved,
            ToolCheckRounds: verdict.ToolCheckRounds,
            Pass: verdict.Report.Pass,
            Decision: verdict.Report.Decision,
            Items: verdict.Plan.Items.Count,
            Errors: verdict.Report.Errors.Count,
            Warnings: verdict.Report.Warnings.Count), Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "CLARIFICATION_PLANNING_DONE",
            runId = run.RunId,
            pass = verdict.Report.Pass,
            decision = verdict.Report.Decision,
            items = verdict.Plan.Items.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new ClarificationPlanningResult(verdict.Plan, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class ClarificationPlanningWorkflow
{
    public const string WorkflowName = "ClarificationPlanning";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        ClarificationPlanningAgentExecutor agent,
        ClarificationPlanGateExecutor gate,
        ClarificationPlanningFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("ClarificationPlanningInput -> ClarificationPlanningAgent[Tools] -> ClarificationPlanGate[det] -> Finalize. "
                           + "Der Agent erzeugt nur Klaerungs-/Breakdown-Planung; GitHub-Write erfolgt spaeter separat nach HumanReview.");

        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
