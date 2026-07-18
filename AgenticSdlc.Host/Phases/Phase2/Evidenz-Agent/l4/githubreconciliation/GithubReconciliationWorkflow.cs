using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed record GithubReconciliationDraft(GithubReconciliationInput Input, GithubActionPlanDocument Plan, bool Saved, int ToolCheckRounds);
internal sealed record GithubReconciliationVerdict(GithubReconciliationInput Input, GithubActionPlanDocument Plan, GithubActionPlanGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record GithubReconciliationResult(GithubActionPlanDocument Plan, GithubActionPlanGateReport Report);

[SendsMessage(typeof(GithubReconciliationDraft))]
internal sealed class GithubReconciliationAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<GithubReconciliationInput>("GithubReconciliationAgent")
{
    public override async ValueTask HandleAsync(GithubReconciliationInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new GithubReconciliationTools(input, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Beginne jetzt mit plan-only GitHub Reconciliation.

                   Du bekommst einen akzeptierten IssuePlan sowie optional vorhandene GitHub-Issue-Snapshots
                   und lokale Mappings. Erzeuge daraus einen GitHubActionPlan.

                   Schreibe nichts nach GitHub. Pruefe deinen Plan mit check_github_action_plan und speichere
                   genau einmal mit save_github_action_plan.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var plan = tools.SavedPlan ?? new GithubActionPlanDocument(
            SchemaVersion: GithubActionPlanDocument.CurrentSchemaVersion,
            PlanId: $"github-action-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAcceptedIssuePlanPath: input.SourceAcceptedIssuePlanPath,
            Repository: input.Repository,
            Actions: []);
        run.AppendEvent(new
        {
            type = "GITHUB_RECON_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            actions = plan.Actions.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new GithubReconciliationDraft(input, plan, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(GithubReconciliationVerdict))]
internal sealed class GithubReconciliationGateExecutor(RunContext run) : Executor<GithubReconciliationDraft>("GithubActionPlanGate")
{
    public override async ValueTask HandleAsync(GithubReconciliationDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = GithubActionPlanGate.Check(draft.Input, draft.Plan);
        run.AppendEvent(new
        {
            type = "GITHUB_ACTION_PLAN_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new GithubReconciliationVerdict(draft.Input, draft.Plan, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(GithubReconciliationResult))]
internal sealed class GithubReconciliationFinalizeExecutor(RunContext run, string outDir) : Executor<GithubReconciliationVerdict>("GithubReconciliationFinalize")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(GithubReconciliationVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-reconciliation-input.json"), JsonSerializer.Serialize(verdict.Input, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-action-plan.json"), JsonSerializer.Serialize(verdict.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-action-plan-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-action-plan-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            verdict.Report.Pass,
            verdict.Report.Decision,
            actions = verdict.Plan.Actions.Count,
            errors = verdict.Report.Errors.Count,
            warnings = verdict.Report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "GITHUB_RECON_DONE",
            runId = run.RunId,
            pass = verdict.Report.Pass,
            decision = verdict.Report.Decision,
            actions = verdict.Plan.Actions.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new GithubReconciliationResult(verdict.Plan, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class GithubReconciliationWorkflow
{
    public const string WorkflowName = "Github-Reconciliation";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        GithubReconciliationAgentExecutor agent,
        GithubReconciliationGateExecutor gate,
        GithubReconciliationFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("GithubReconciliationInput -> GithubReconciliationAgent[Tools] -> GithubActionPlanGate[det] -> Finalize. "
                           + "Der Agent erzeugt nur GitHubActionPlans; GitHub-Write erfolgt spaeter separat nach HumanReview.");

        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
