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
                               2c. QUER-SICHT (nur lesen): list_core_architecture zeigt die Wahrheit des ANDEREN
                                   Aspekts. Widerspricht eine eingehende Aussage einer AKTIVEN Architektur-Wahrheit,
                                   nutze CONTRADICT mit deren entityId — NIE stilles NEW daneben. RESTATE/REFINE/
                                   SUPERSEDE bleiben strikt requirement-intern.
                               3. Je eingehendem Item GENAU EINE Operation.
                                  Fuer Requirements (itemType=requirement): RESTATE/REFINE/SUPERSEDE/CONTRADICT
                                  (mit targetEntityId) · NEW/NEW_RELATED (ohne targetEntityId) · ALREADY_DECIDED (DEC-*).
                                  Vor CONTRADICT: list_open_decisions pruefen (sonst ALREADY_DECIDED).
                               3b. Fuer offene Fragen (itemType=open_question) NUR: OPEN_QUESTION (neue Frage, ohne
                                   targetEntityId — wird zur offenen Entscheidung) ODER ALREADY_DECIDED (dieselbe Frage
                                   ist bereits als DEC-* erfasst; vorher list_open_decisions pruefen). Formuliere das
                                   statement als die Frage selbst; NIE eine Frage als Anforderung umdeuten.
                               3z. ANTWORT-ANKER (R-74, hat Vorrang vor allen anderen Regeln): traegt ein eingehendes
                                   Item `answersDecision=DEC-x`, dann IST es die ANTWORT auf diese offene Entscheidung —
                                   IMMER NEW oder NEW_RELATED (es wird Wahrheit, der Anker reist mit), NIEMALS
                                   ALREADY_DECIDED oder RESTATE (das wuerde die Antwort in die Frage falten und sie
                                   ginge verloren; die Entscheidung selbst schliesst der Autor separat am decision-gate).
                               3c. Fuer Meeting-Risiken (itemType=risk) gilt DASSELBE Vokabular: OPEN_QUESTION (das
                                   Risiko wird zur offenen Entscheidung — akzeptieren, mitigieren oder klaeren
                                   entscheidet der Autor am decision-gate) ODER ALREADY_DECIDED. statement = das
                                   Risiko woertlich; NIE ein Risiko als Anforderung umdeuten oder verwerfen.
                               4. check_state_change_plan (muss pass sein), dann save_state_change_plan (genau einmal).
                               Beleg-Pflicht: claimIds je Operation; im Zweifel NEW_RELATED/NEW statt raten.
                               """;
}

[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionResolveExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, ICandidateRetriever retriever, RunContext run, AspectIngestionProfile profile)
    : Executor<IngestionResolveInput>(profile.ExecutorIdPrefix + "ResolveAgent")
{
    public override async ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new IngestionTools(input.MeetingDelta, input.Core, retriever, run, profile);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, profile.ResolverTaskText)], cancellationToken: ct).ConfigureAwait(false);
        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new { type = profile.EventPrefix + "_RESOLVE_DONE", runId = run.RunId, saved = tools.Saved, operations = ops.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionDraft(input.MeetingDelta, input.Core, input.SourceMeetingDeltaPath, ops, tools.Saved, tools.CheckRounds, Attempt: 1, Source: "maker", input.MaxAttempts, History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(IngestionVerdict))]
internal sealed class IngestionGateExecutor(RunContext run, AspectIngestionProfile? profile = null)
    : Executor<IngestionDraft>((profile ?? AspectIngestionProfile.Requirement).ExecutorIdPrefix + "Gate")
{
    public override async ValueTask HandleAsync(IngestionDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "gate", DateTime.UtcNow, draft.SourcePath, draft.Operations);
        var report = IngestionGate.Check(draft.MeetingDelta, draft.Core, plan, profile);
        var decision = IngestionGate.Decide(report, draft.Attempt, draft.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.IncomingItemId is null ? "" : " " + e.IncomingItemId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = (profile ?? AspectIngestionProfile.Requirement).EventPrefix + "_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = report.Pass, decision = decision.ToString(), errors = report.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionVerdict(draft.MeetingDelta, draft.Core, draft.SourcePath, draft.Operations, report, decision, draft.Saved, draft.CheckRounds, draft.Attempt, draft.MaxAttempts, draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, ICandidateRetriever retriever, RunContext run, AspectIngestionProfile profile)
    : Executor<IngestionVerdict>(profile.ExecutorIdPrefix + "Repair")
{
    public override async ValueTask HandleAsync(IngestionVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    {profile.ResolverTaskText}

                    Dein vorheriger Plan hat das Gate NICHT bestanden. Fehler:
                    {errors}
                    Korrigiere gezielt und speichere erneut (save_state_change_plan genau einmal).
                    """;
        var tools = new IngestionTools(v.MeetingDelta, v.Core, retriever, run, profile);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new { type = profile.EventPrefix + "_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, operations = ops.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionDraft(v.MeetingDelta, v.Core, v.SourcePath, ops, tools.Saved, tools.CheckRounds, Attempt: v.Attempt + 1, Source: "repair", v.MaxAttempts, History: v.History)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(IngestionResult))]
internal sealed class IngestionFinalizeExecutor(RunContext run, string outDir, AspectIngestionProfile? profileOrNull = null)
    : Executor<IngestionVerdict>((profileOrNull ?? AspectIngestionProfile.Requirement).ExecutorIdPrefix + "Finalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly AspectIngestionProfile profile = profileOrNull ?? AspectIngestionProfile.Requirement;

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

        run.AppendEvent(new { type = profile.EventPrefix + "_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, operations = v.Operations.Count, timestampUtc = DateTime.UtcNow });
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
