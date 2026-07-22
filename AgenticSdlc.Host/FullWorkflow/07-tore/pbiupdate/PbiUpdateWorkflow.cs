using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Inc 1c-3 als MAF-Workflow MIT gate-getriebenem Repair-Loop (R7, Pattern wie GithubForward):
//   Derive -> Maker[PbiPlacementAgent] -> Gate --[repairable&&attempt<max]--> Repair (Loop) / sonst Finalize.
// DoD = Gate pass. Repair = Platzierungs-Retry mit GateFeedback (kein Write). Bounded (maxAttempts).

internal sealed record PbiUpdateWfContext(ProjectStateDocument Core, IReadOnlyList<AppliedOperation> Applied, string SourceIngestionRun, string OutDir, bool DryRun, int MaxAttempts);
internal sealed record PbiUpdateSeeded(PbiUpdateWfContext Ctx, IReadOnlyList<PbiStateChangeOperation> DeterministicOps, IReadOnlyList<PbiUpdateDerivation.UnplacedRequirement> Unplaced);
internal sealed record PbiUpdateDraft(PbiUpdateWfContext Ctx, IReadOnlyList<PbiStateChangeOperation> DeterministicOps, IReadOnlyList<PbiUpdateDerivation.UnplacedRequirement> Unplaced, IReadOnlyList<PbiStateChangeOperation> Placements, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record PbiUpdateVerdict(PbiUpdateWfContext Ctx, IReadOnlyList<PbiUpdateDerivation.UnplacedRequirement> Unplaced, PbiStateChangePlanDocument Plan, PbiUpdateGateReport Report, GateDecision Decision, int Attempt, int UnplacedCount, IReadOnlyList<GateAttempt> History);
internal sealed record PbiUpdateWfResult(PbiStateChangePlanDocument Plan, PbiUpdateGateReport Gate, GateDecision FinalDecision);

[SendsMessage(typeof(PbiUpdateSeeded))]
internal sealed class PbiUpdateDeriveExecutor(RunContext run) : Executor<PbiUpdateWfContext>("PbiUpdateDerive")
{
    public override async ValueTask HandleAsync(PbiUpdateWfContext ctx, IWorkflowContext context, CancellationToken ct = default)
    {
        var derived = PbiUpdateDerivation.Derive(ctx.Core, ctx.Applied);
        run.AppendEvent(new { type = "PBI_UPDATE_DERIVE", runId = run.RunId, deterministic = derived.DeterministicOps.Count, unplaced = derived.Unplaced.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new PbiUpdateSeeded(ctx, derived.DeterministicOps, derived.Unplaced)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(PbiUpdateDraft))]
internal sealed class PbiUpdateMakerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<PbiUpdateSeeded>("PbiUpdateMaker")
{
    private const string Task = """
                                Ordne jedes NEUE Requirement (get_unplaced_requirements) GENAU EINEM PBI zu:
                                - EXTEND_PBI (mit pbiId), wenn ein bestehendes PBI dieselbe fachliche Aufgabe abdeckt.
                                - NEW_PBI (mit featureId), wenn es ein eigenes PBI braucht.
                                Nutze list_features / get_feature_pbis / get_pbi. Belege jede Platzierung (rationale).
                                Speichere genau einmal mit save_placements.
                                """;

    public override async ValueTask HandleAsync(PbiUpdateSeeded seeded, IWorkflowContext context, CancellationToken ct = default)
    {
        var placements = await PbiPlacementRunner.RunAsync(seeded.Ctx, seeded.Unplaced, agentFactory, run, Task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "PBI_UPDATE_MAKER", runId = run.RunId, placements = placements.Count, dryRun = seeded.Ctx.DryRun, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new PbiUpdateDraft(seeded.Ctx, seeded.DeterministicOps, seeded.Unplaced, placements, Attempt: 1, Source: "maker", History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(PbiUpdateVerdict))]
internal sealed class PbiUpdateGateExecutor(RunContext run) : Executor<PbiUpdateDraft>("PbiUpdateGate")
{
    public override async ValueTask HandleAsync(PbiUpdateDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var ctx = draft.Ctx;
        var ops = draft.DeterministicOps.Concat(draft.Placements).ToList();
        var plan = new PbiStateChangePlanDocument(
            PbiStateChangePlanDocument.CurrentSchemaVersion, $"pbi-change-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}", DateTime.UtcNow, ctx.SourceIngestionRun, ops);
        var unplacedIds = draft.Unplaced.Select(u => u.RequirementId).ToHashSet(StringComparer.Ordinal);
        var gate = PbiUpdateGate.Check(ctx.Core, plan, unplacedIds);
        var decision = PbiUpdateGate.Decide(gate, draft.Attempt, ctx.MaxAttempts);

        var attempt = new GateAttempt(draft.Attempt, draft.Source, gate.Pass, decision.ToString(),
            gate.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.RequirementId is null ? "" : " " + e.RequirementId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "PBI_UPDATE_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = gate.Pass, decision = decision.ToString(), errors = gate.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new PbiUpdateVerdict(ctx, draft.Unplaced, plan, gate, decision, draft.Attempt, unplacedIds.Count, draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(PbiUpdateDraft))]
internal sealed class PbiUpdateRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<PbiUpdateVerdict>("PbiUpdateRepair")
{
    public override async ValueTask HandleAsync(PbiUpdateVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    Deine vorherige Platzierung hat das Gate NICHT bestanden. Fehler:
                    {errors}

                    Korrigiere: JEDES Requirement aus get_unplaced_requirements bekommt GENAU EINE Platzierung
                    (EXTEND_PBI mit gueltiger pbiId ODER NEW_PBI mit gueltiger featureId aus list_features).
                    save_placements GENAU EINMAL.
                    """;
        // Der Repair kennt nur die deterministischen Ops nicht mehr direkt — er erzeugt neue Placements; die det.
        // Ops sind im Plan (Verdict) via Origin ableitbar. Einfacher: die det. Ops aus dem Plan (kind != Placement).
        var deterministic = v.Plan.Operations.Where(o => !PbiUpdateKind.Placement.Contains(o.Kind)).ToList();
        var placements = await PbiPlacementRunner.RunAsync(v.Ctx, v.Unplaced, agentFactory, run, task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "PBI_UPDATE_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, placements = placements.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new PbiUpdateDraft(v.Ctx, deterministic, v.Unplaced, placements, Attempt: v.Attempt + 1, Source: "repair", History: v.History)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(PbiUpdateWfResult))]
internal sealed class PbiUpdateFinalizeExecutor(RunContext run) : Executor<PbiUpdateVerdict>("PbiUpdateFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(PbiUpdateVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var ctx = v.Ctx;
        Directory.CreateDirectory(ctx.OutDir);
        var ops = v.Plan.Operations;
        var placements = ops.Count(o => PbiUpdateKind.Placement.Contains(o.Kind));
        var deterministic = ops.Count - placements;

        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "pbi-change-plan.json"), JsonSerializer.Serialize(v.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "pbi-update-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "pbi-update-attempts.json"), JsonSerializer.Serialize(v.History, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "pbi-update-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId, sourceIngestionRun = ctx.SourceIngestionRun, deterministic, unplaced = v.UnplacedCount, placements,
            operations = ops.Count, gatePass = v.Report.Pass, gateErrors = v.Report.Errors.Count,
            attempts = v.Attempt, finalDecision = v.Decision.ToString(),
            byKind = ops.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            dryRun = ctx.DryRun, timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new { type = "PBI_UPDATE_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new PbiUpdateWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
    }
}

internal static class PbiPlacementRunner
{
    public static async Task<List<PbiStateChangeOperation>> RunAsync(
        PbiUpdateWfContext ctx, IReadOnlyList<PbiUpdateDerivation.UnplacedRequirement> unplaced,
        Func<IReadOnlyList<AITool>, AIAgent> factory, RunContext run, string task, CancellationToken ct)
    {
        if (unplaced.Count == 0 || ctx.DryRun) return [];
        var tools = new PbiPlacementTools(unplaced, ctx.Core, run);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        return (tools.SavedPlacements ?? []).ToList();
    }
}

internal static class PbiUpdateWorkflow
{
    public const string WorkflowName = "Incremental-PBI-Update";

    public static Workflow Build(PbiUpdateDeriveExecutor derive, PbiUpdateMakerExecutor maker, PbiUpdateGateExecutor gate, PbiUpdateRepairExecutor repair, PbiUpdateFinalizeExecutor finalize)
    {
        var b = new WorkflowBuilder(derive)
            .WithName(WorkflowName)
            .WithDescription("Ingestion-Delta -> Derive -> Maker -> Gate --[repairable]--> Repair (Loop) / sonst Finalize.");
        b.AddEdge(derive, maker);
        b.AddEdge(maker, gate);
        b.AddEdge<PbiUpdateVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<PbiUpdateVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.WithOutputFrom(finalize);
        return b.Build();
    }
}
