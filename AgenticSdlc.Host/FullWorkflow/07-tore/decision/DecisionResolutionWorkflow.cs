using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// Tor 2 als MAF-Workflow, zwei Graphen mit geteilten Knoten + gate-getriebenem Repair-Loop (R7):
//   decision-resolve (det., maxAttempts=1):  Derive -> Gate -> Finalize
//   decision-resolve-agent (agent):          Maker -> Derive -> Gate --[repairable]--> Repair -> Derive (Loop) / Finalize
// Repair = Resolver-Retry mit GateFeedback (neuer Input -> Derive). DoD = Gate pass, bounded.

internal sealed record DecisionWfContext(ProjectStateDocument Core, string OutDir, int MaxAttempts, string? StakeholderAnswer);
internal sealed record DecisionAnswerMsg(DecisionWfContext Ctx);
internal sealed record DecisionResolveInputMsg(DecisionWfContext Ctx, DecisionResolutionInput Input, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record DecisionDraftMsg(DecisionWfContext Ctx, IReadOnlyList<DecisionResolutionOp> Ops, int ResolutionCount, int ProblemCount, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record DecisionVerdictMsg(DecisionWfContext Ctx, DecisionResolutionPlanDocument Plan, DecisionResolutionGateReport Report, GateDecision Decision, int ResolutionCount, int ProblemCount, int Attempt, IReadOnlyList<GateAttempt> History);
internal sealed record DecisionWfResult(DecisionResolutionPlanDocument Plan, DecisionResolutionGateReport Gate, GateDecision FinalDecision);

// MAKER (nur agentisch): freie Antwort -> DecisionResolutionInput.
[SendsMessage(typeof(DecisionResolveInputMsg))]
internal sealed class DecisionMakerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<DecisionAnswerMsg>("DecisionResolverMaker")
{
    public override async ValueTask HandleAsync(DecisionAnswerMsg msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var task = $"""
                    Stakeholder-Antwort (frei):
                    {msg.Ctx.StakeholderAnswer}

                    Nutze get_open_decisions und get_decision_context. Deute die Antwort und speichere je adressierter
                    Decision GENAU EINE Auflösung mit save_resolutions (outcome + newStatement bei ADOPT_NEW/REFINE).
                    Decisions, die die Antwort NICHT adressiert, laesst du weg.
                    """;
        var input = await DecisionResolverInvoker.RunAsync(msg.Ctx, agentFactory, run, task, ct).ConfigureAwait(false);
        Directory.CreateDirectory(msg.Ctx.OutDir);
        await File.WriteAllTextAsync(Path.Combine(msg.Ctx.OutDir, "stakeholder-answer.txt"), msg.Ctx.StakeholderAnswer ?? "", ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "DECISION_MAKER", runId = run.RunId, resolutions = input.Resolutions.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new DecisionResolveInputMsg(msg.Ctx, input, Attempt: 1, Source: "maker", History: [])).ConfigureAwait(false);
    }
}

// DERIVE: deterministisch (geteilt von beiden Graphen; im Loop von Maker UND Repair gespeist).
[SendsMessage(typeof(DecisionDraftMsg))]
internal sealed class DecisionDeriveExecutor(RunContext run) : Executor<DecisionResolveInputMsg>("DecisionDerive")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(DecisionResolveInputMsg msg, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(msg.Ctx.OutDir);
        await File.WriteAllTextAsync(Path.Combine(msg.Ctx.OutDir, "resolver-input.json"), JsonSerializer.Serialize(msg.Input, Json), ct).ConfigureAwait(false);
        var (ops, problems) = DecisionResolutionDerivation.Derive(msg.Ctx.Core, msg.Input);
        run.AppendEvent(new { type = "DECISION_DERIVE", runId = run.RunId, ops = ops.Count, problems = problems.Count, attempt = msg.Attempt, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new DecisionDraftMsg(msg.Ctx, ops, msg.Input.Resolutions.Count, problems.Count, msg.Attempt, msg.Source, msg.History)).ConfigureAwait(false);
    }
}

// GATE: deterministisch + Entscheidung + Attempt-Historie.
[SendsMessage(typeof(DecisionVerdictMsg))]
internal sealed class DecisionGateExecutor(RunContext run) : Executor<DecisionDraftMsg>("DecisionGate")
{
    public override async ValueTask HandleAsync(DecisionDraftMsg draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = new DecisionResolutionPlanDocument(
            DecisionResolutionPlanDocument.CurrentSchemaVersion, $"decision-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}", DateTime.UtcNow, draft.Ops);
        var gate = DecisionResolutionGate.Check(draft.Ctx.Core, plan);
        var decision = DecisionResolutionGate.Decide(gate, draft.Attempt, draft.Ctx.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, gate.Pass, decision.ToString(),
            gate.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.DecisionId is null ? "" : " " + e.DecisionId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "DECISION_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = gate.Pass, decision = decision.ToString(), errors = gate.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new DecisionVerdictMsg(draft.Ctx, plan, gate, decision, draft.ResolutionCount, draft.ProblemCount, draft.Attempt, draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

// REPAIR (nur agentisch): Resolver-Retry mit GateFeedback -> neuer Input -> Derive (Loop-Back).
[SendsMessage(typeof(DecisionResolveInputMsg))]
internal sealed class DecisionRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<DecisionVerdictMsg>("DecisionRepair")
{
    public override async ValueTask HandleAsync(DecisionVerdictMsg v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    Stakeholder-Antwort (frei):
                    {v.Ctx.StakeholderAnswer}

                    Deine vorherige Auflösung hat das Gate NICHT bestanden. Fehler:
                    {errors}

                    Korrigiere: je adressierter Decision GENAU EINE Auflösung; bei ADOPT_NEW/REFINE eine klare
                    newStatement; gueltiges outcome (KEEP_ORIGINAL|ADOPT_NEW|REFINE). save_resolutions GENAU EINMAL.
                    """;
        var input = await DecisionResolverInvoker.RunAsync(v.Ctx, agentFactory, run, task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "DECISION_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, resolutions = input.Resolutions.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new DecisionResolveInputMsg(v.Ctx, input, Attempt: v.Attempt + 1, Source: "repair", History: v.History)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(DecisionWfResult))]
internal sealed class DecisionFinalizeExecutor(RunContext run) : Executor<DecisionVerdictMsg>("DecisionFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(DecisionVerdictMsg v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(v.Ctx.OutDir);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "decision-plan.json"), JsonSerializer.Serialize(v.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "decision-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "decision-attempts.json"), JsonSerializer.Serialize(v.History, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "decision-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId, resolutions = v.ResolutionCount, ops = v.Plan.Operations.Count, problems = v.ProblemCount,
            gatePass = v.Report.Pass, gateErrors = v.Report.Errors.Count, attempts = v.Attempt, finalDecision = v.Decision.ToString(),
            byOutcome = v.Plan.Operations.GroupBy(o => o.Outcome, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "DECISION_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DecisionWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
    }
}

internal static class DecisionResolverInvoker
{
    public static async Task<DecisionResolutionInput> RunAsync(DecisionWfContext ctx, Func<IReadOnlyList<AITool>, AIAgent> factory, RunContext run, string task, CancellationToken ct)
    {
        var tools = new DecisionResolverTools(ctx.Core, run);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        return tools.SavedInput;
    }
}

internal static class DecisionResolveWorkflow
{
    public const string WorkflowName = "Decision-Ingestion";

    // Deterministisch (T2.1): kein Maker, kein Repair (maxAttempts=1 -> Decide gibt nie Repair).
    public static Workflow BuildDeterministic(DecisionDeriveExecutor derive, DecisionGateExecutor gate, DecisionFinalizeExecutor finalize)
    {
        var b = new WorkflowBuilder(derive).WithName(WorkflowName).WithDescription("Auflösungen -> Derive -> Gate -> Finalize.");
        b.AddEdge(derive, gate); b.AddEdge(gate, finalize); b.WithOutputFrom(finalize);
        return b.Build();
    }

    // Agentisch (T2.2): Maker -> Derive -> Gate --[repairable]--> Repair -> Derive (Loop) / sonst Finalize.
    public static Workflow BuildAgentic(DecisionMakerExecutor maker, DecisionDeriveExecutor derive, DecisionGateExecutor gate, DecisionRepairExecutor repair, DecisionFinalizeExecutor finalize)
    {
        var b = new WorkflowBuilder(maker).WithName(WorkflowName + "-Agent").WithDescription("Freie Antwort -> Maker -> Derive -> Gate --[repairable]--> Repair (Loop) / Finalize.");
        b.AddEdge(maker, derive);
        b.AddEdge(derive, gate);
        b.AddEdge<DecisionVerdictMsg>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<DecisionVerdictMsg>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, derive);
        b.WithOutputFrom(finalize);
        return b.Build();
    }
}
