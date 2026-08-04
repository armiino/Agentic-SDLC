using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

internal sealed record IngestionResolveInput(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourceMeetingDeltaPath, int MaxAttempts);
internal sealed record IngestionDraft(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourcePath, IReadOnlyList<StateChangeOperation> Operations, bool Saved, int CheckRounds, int Attempt, string Source, int MaxAttempts, IReadOnlyList<GateAttempt> History);
internal sealed record IngestionVerdict(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourcePath, IReadOnlyList<StateChangeOperation> Operations, IngestionGateReport Report, GateDecision Decision, bool Saved, int CheckRounds, int Attempt, int MaxAttempts, IReadOnlyList<GateAttempt> History);
internal sealed record IngestionResult(StateChangePlanDocument Plan, IngestionGateReport Report, GateDecision FinalDecision);

// R7: der Requirement-Resolver als MAF-Workflow MIT gate-getriebenem Repair-Loop:
//   Resolve -> Gate --[repairable&&attempt<max]--> Repair -> Gate (Loop) / sonst Finalize.
// Repair = Resolve-Retry mit GateFeedback (kein Write; Apply bleibt eigener Lauf). DoD = Gate pass, bounded.

internal static class IngestionResolveTask
{
    public const string Text = """
                               Loese die eingehenden Requirements dieses Meetings gegen den bestehenden Core auf.
                               1. get_incoming_items · 2. list_core_requirements / search_core / get_core_entity.
                               2b. search_rejections: Wurde etwas inhaltlich Gleiches frueher ABGELEHNT? Wenn ja,
                                   relatedRejectionId (REJ-*) auf der Operation setzen — die Operation trotzdem
                                   normal vorschlagen, die Entscheidung trifft der Mensch am Gate.
                               3. Je eingehendem Requirement GENAU EINE Operation: RESTATE/REFINE/SUPERSEDE/CONTRADICT
                                  (mit targetEntityId) · NEW/NEW_RELATED (ohne targetEntityId) · ALREADY_DECIDED (DEC-*).
                                  Vor CONTRADICT: list_open_decisions pruefen (sonst ALREADY_DECIDED).
                               4. check_state_change_plan (muss pass sein), dann save_state_change_plan (genau einmal).
                               Beleg-Pflicht: claimIds je Operation; im Zweifel NEW_RELATED/NEW statt raten.
                               """;
}

[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionResolveExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, ICandidateRetriever retriever, RunContext run)
    : Executor<IngestionResolveInput>("RequirementIngestionResolveAgent")
{
    public override async ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new IngestionTools(input.MeetingDelta, input.Core, retriever, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, IngestionResolveTask.Text)], cancellationToken: ct).ConfigureAwait(false);
        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new { type = "REQ_INGEST_RESOLVE_DONE", runId = run.RunId, saved = tools.Saved, operations = ops.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionDraft(input.MeetingDelta, input.Core, input.SourceMeetingDeltaPath, ops, tools.Saved, tools.CheckRounds, Attempt: 1, Source: "maker", input.MaxAttempts, History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(IngestionVerdict))]
internal sealed class IngestionGateExecutor(RunContext run) : Executor<IngestionDraft>("RequirementIngestionGate")
{
    public override async ValueTask HandleAsync(IngestionDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "gate", DateTime.UtcNow, draft.SourcePath, draft.Operations);
        var report = IngestionGate.Check(draft.MeetingDelta, draft.Core, plan);
        var decision = IngestionGate.Decide(report, draft.Attempt, draft.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.IncomingItemId is null ? "" : " " + e.IncomingItemId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "REQ_INGEST_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = report.Pass, decision = decision.ToString(), errors = report.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionVerdict(draft.MeetingDelta, draft.Core, draft.SourcePath, draft.Operations, report, decision, draft.Saved, draft.CheckRounds, draft.Attempt, draft.MaxAttempts, draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, ICandidateRetriever retriever, RunContext run)
    : Executor<IngestionVerdict>("RequirementIngestionRepair")
{
    public override async ValueTask HandleAsync(IngestionVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    {IngestionResolveTask.Text}

                    Dein vorheriger Plan hat das Gate NICHT bestanden. Fehler:
                    {errors}
                    Korrigiere gezielt und speichere erneut (save_state_change_plan genau einmal).
                    """;
        var tools = new IngestionTools(v.MeetingDelta, v.Core, retriever, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new { type = "REQ_INGEST_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, operations = ops.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionDraft(v.MeetingDelta, v.Core, v.SourcePath, ops, tools.Saved, tools.CheckRounds, Attempt: v.Attempt + 1, Source: "repair", v.MaxAttempts, History: v.History)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(IngestionResult))]
internal sealed class IngestionFinalizeExecutor(RunContext run, string outDir) : Executor<IngestionVerdict>("RequirementIngestionFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(IngestionVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, $"state-change-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}", DateTime.UtcNow, v.SourcePath, v.Operations);
        var byKind = v.Operations.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        await File.WriteAllTextAsync(Path.Combine(outDir, "plan.json"), JsonSerializer.Serialize(plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "ingestion-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "ingestion-attempts.json"), JsonSerializer.Serialize(v.History, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "ingestion-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId, v.Saved, v.CheckRounds, gatePass = v.Report.Pass, gateErrors = v.Report.Errors.Count,
            gateWarnings = v.Report.Warnings.Count, operations = v.Operations.Count, attempts = v.Attempt, finalDecision = v.Decision.ToString(),
            byKind, timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new { type = "REQ_INGEST_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, operations = v.Operations.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new IngestionResult(plan, v.Report, v.Decision)).ConfigureAwait(false);
    }
}

internal static class RequirementIngestionWorkflow
{
    public const string WorkflowName = "Requirement-Ingestion";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        IngestionResolveExecutor resolve, IngestionGateExecutor gate, IngestionRepairExecutor repair, IngestionFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(resolve)
            .WithName(WorkflowName)
            .WithDescription("MeetingDelta -> Resolve -> Gate --[repairable]--> Repair (Loop) / sonst Finalize.");
        builder.AddEdge(resolve, gate);
        builder.AddEdge<IngestionVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        builder.AddEdge<IngestionVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        builder.AddEdge(repair, gate);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
