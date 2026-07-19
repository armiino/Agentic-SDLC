using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

internal sealed record IngestionResolveInput(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourceMeetingDeltaPath);
internal sealed record IngestionDraft(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourcePath, IReadOnlyList<StateChangeOperation> Operations, bool Saved, int CheckRounds);
internal sealed record IngestionVerdict(ProjectStateDocument MeetingDelta, ProjectStateDocument Core, string SourcePath, IReadOnlyList<StateChangeOperation> Operations, IngestionGateReport Report, bool Saved, int CheckRounds);
internal sealed record IngestionResult(StateChangePlanDocument Plan, IngestionGateReport Report);

// MAKER: loest jedes eingehende Requirement gegen den Core auf (bekannt/verfeinert/verwandt-neu/neu/
// ersetzt/widerspruechlich) und speichert einen StateChangePlan. Retrieval Stufe 0 = "alle zeigen".
[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionResolveExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    ICandidateRetriever retriever,
    RunContext run)
    : Executor<IngestionResolveInput>("RequirementIngestionResolveAgent")
{
    public override async ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new IngestionTools(input.MeetingDelta, input.Core, retriever, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Loese die eingehenden Requirements dieses Meetings gegen den bestehenden Core (die Wahrheit) auf.

                   Vorgehen mit deinen Tools:
                   1. get_incoming_items  - was kommt neu herein.
                   2. list_core_requirements / search_core / get_core_entity - was existiert bereits (Kandidaten).
                   3. Entscheide je eingehendem Requirement GENAU EINE Operation:
                      - RESTATE      identisch zu einer bestehenden Entitaet -> targetEntityId.
                      - REFINE       konkretisiert DIESELBE Anforderung -> targetEntityId.
                      - NEW_RELATED  fachlich NEU, aber im selben Feature (z.B. "bearbeiten" neben "anzeigen") -> featureKey, KEIN targetEntityId.
                      - NEW          nichts Passendes -> KEIN targetEntityId.
                      - SUPERSEDE    ersetzt eine alte Anforderung -> targetEntityId.
                      - CONTRADICT   widerspricht bestehender Wahrheit -> targetEntityId (wird zur Open Decision, kein Ueberschreiben).
                      - ALREADY_DECIDED  der Widerspruch ist schon als Open Decision erfasst -> targetEntityId = DEC-* (No-Op).
                   4. Vor CONTRADICT: list_open_decisions pruefen. Ist der Widerspruch schon offen, nutze ALREADY_DECIDED (kein Duplikat).
                   5. Pruefe mit check_state_change_plan (muss pass sein), dann save_state_change_plan (genau einmal).

                   Regeln:
                   - Beleg-Pflicht: RESTATE/REFINE/SUPERSEDE/CONTRADICT MUESSEN targetEntityId nennen; NEW/NEW_RELATED duerfen KEINS.
                   - claimIds jeder Operation aus dem eingehenden Item uebernehmen (Nachvollziehbarkeit).
                   - Nur echte fachliche Identitaet als MATCH/REFINE - im Zweifel NEW_RELATED oder NEW, nicht raten.
                   - statement = die praezise Aussage, die in den Core soll (bei RESTATE die bestehende Formulierung ok).
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new
        {
            type = "REQ_INGEST_RESOLVE_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            operations = ops.Count,
            checkRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new IngestionDraft(input.MeetingDelta, input.Core, input.SourceMeetingDeltaPath, ops, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

// CHECKER: deterministisches Gate (Coverage/Ziele/Belege).
[SendsMessage(typeof(IngestionVerdict))]
internal sealed class IngestionGateExecutor(RunContext run) : Executor<IngestionDraft>("RequirementIngestionGate")
{
    public override async ValueTask HandleAsync(IngestionDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "gate", DateTime.UtcNow, draft.SourcePath, draft.Operations);
        var report = IngestionGate.Check(draft.MeetingDelta, draft.Core, plan);
        run.AppendEvent(new
        {
            type = "REQ_INGEST_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new IngestionVerdict(draft.MeetingDelta, draft.Core, draft.SourcePath, draft.Operations, report, draft.Saved, draft.CheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(IngestionResult))]
internal sealed class IngestionFinalizeExecutor(RunContext run, string outDir) : Executor<IngestionVerdict>("RequirementIngestionFinalize")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(IngestionVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        var plan = new StateChangePlanDocument(
            SchemaVersion: StateChangePlanDocument.CurrentSchemaVersion,
            PlanId: $"state-change-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            CreatedUtc: DateTime.UtcNow,
            SourceMeetingDeltaPath: v.SourcePath,
            Operations: v.Operations);

        var byKind = v.Operations.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        await File.WriteAllTextAsync(Path.Combine(outDir, "plan.json"), JsonSerializer.Serialize(plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "ingestion-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "ingestion-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            v.Saved,
            v.CheckRounds,
            gatePass = v.Report.Pass,
            gateErrors = v.Report.Errors.Count,
            gateWarnings = v.Report.Warnings.Count,
            operations = v.Operations.Count,
            byKind,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "REQ_INGEST_DONE",
            runId = run.RunId,
            gatePass = v.Report.Pass,
            operations = v.Operations.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new IngestionResult(plan, v.Report)).ConfigureAwait(false);
    }
}

internal static class RequirementIngestionWorkflow
{
    public const string WorkflowName = "Requirement-Ingestion";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        IngestionResolveExecutor resolve,
        IngestionGateExecutor gate,
        IngestionFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(resolve)
            .WithName(WorkflowName)
            .WithDescription("MeetingDelta -> ResolveAgent[Tools] (maker, StateChangePlan) -> Gate[det Coverage/Ziele] -> Finalize.");

        builder.AddEdge(resolve, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
