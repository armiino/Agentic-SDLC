using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// S4 (Worklist 20.07): ingestion (Tor 1) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort), Muster wie
// pbi-update/decision. Wiederverwendet Gate/Repair unveraendert; Resolve als HITL-Variante mit Skip-bei-leerem-Delta
// (0 eingehende Requirements -> kein LLM -> leerer Plan; ermoeglicht deterministische Verifikation).
//   Resolve -> Gate --[repairable]--> Repair -> Gate (Loop) / Finalize -> [RequestPort Human] -> Apply
// Invarianten: Plan = Disk-Artefakt; Core im Apply FRISCH geladen; nur Entscheidung durch den Port. ingestion-apply
// MUTIERT den Core + vergibt IDs -> plan-level Idempotenz-Marker in IngestionApplyExec (S3-analog).

public sealed record IngestionReviewRequest(string RunId, IReadOnlyList<IngestionReviewOpView> Ops);
public sealed record IngestionReviewOpView(string IncomingItemId, string Kind, string? TargetEntityId, string Statement, string Rationale);
public sealed record IngestionReviewResponse(IReadOnlyList<string> AcceptedIncomingIds, string Reviewer);

// RESOLVE (HITL): wie IngestionResolveExecutor, ABER Skip des Agenten bei 0 eingehenden Requirements (leeres Meeting
// -> leerer Plan, kein LLM). Sonst identischer Resolver-Lauf.
[SendsMessage(typeof(IngestionDraft))]
internal sealed class IngestionHitlResolveExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, ICandidateRetriever retriever, RunContext run, AspectIngestionProfile profile)
    : Executor<IngestionResolveInput>(profile.ExecutorIdPrefix + "ResolveAgent")
{
    public override async ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var incoming = input.MeetingDelta.Items.Count(profile.Matches);
        if (incoming == 0)
        {
            run.AppendEvent(new { type = profile.EventPrefix + "_RESOLVE_DONE", runId = run.RunId, saved = true, operations = 0, checkRounds = 0, skipped = "no-incoming", timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(new IngestionDraft(input.MeetingDelta, input.Core, input.SourceMeetingDeltaPath, [], Saved: true, CheckRounds: 0, Attempt: 1, Source: "maker", input.MaxAttempts, History: [])).ConfigureAwait(false);
            return;
        }

        var tools = new IngestionTools(input.MeetingDelta, input.Core, retriever, run, profile);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, profile.ResolverTaskText)], cancellationToken: ct).ConfigureAwait(false);
        var ops = tools.SavedOperations ?? [];
        run.AppendEvent(new { type = profile.EventPrefix + "_RESOLVE_DONE", runId = run.RunId, saved = tools.Saved, operations = ops.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionDraft(input.MeetingDelta, input.Core, input.SourceMeetingDeltaPath, ops, tools.Saved, tools.CheckRounds, Attempt: 1, Source: "maker", input.MaxAttempts, History: [])).ConfigureAwait(false);
    }
}

// FINALIZE (HITL): schreibt Plan/Gate/Attempts/Summary (wie IngestionFinalize) und verzweigt:
//   Decision==Pass -> IngestionReviewRequest an den Port. Sonst -> terminaler "needs manual"-Output.
[SendsMessage(typeof(IngestionReviewRequest))]
[YieldsOutput(typeof(IngestionResult))]
internal sealed class IngestionHitlFinalizeExecutor(RunContext run, string outDir, AspectIngestionProfile? profile = null)
    : Executor<IngestionVerdict>((profile ?? AspectIngestionProfile.Requirement).ExecutorIdPrefix + "HitlFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly AspectIngestionProfile _p = profile ?? AspectIngestionProfile.Requirement;

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

        if (v.Decision == GateDecision.Pass)
        {
            var views = v.Operations.Select(o => new IngestionReviewOpView(o.IncomingItemId, o.Kind, o.TargetEntityId, o.Statement, o.Rationale)).ToList();
            run.AppendEvent(new { type = _p.EventPrefix + "_HUMAN_GATE", runId = run.RunId, operations = v.Operations.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(new IngestionReviewRequest(run.RunId, views)).ConfigureAwait(false);
        }
        else
        {
            run.AppendEvent(new { type = _p.EventPrefix + "_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(new IngestionResult(plan, v.Report, v.Decision)).ConfigureAwait(false);
        }
    }
}

// APPLY (HITL): konsumiert die menschliche Response. Laedt Plan+Core+MeetingDelta FRISCH (in IngestionApplyExec).
[YieldsOutput(typeof(IngestionApplyReport))]
internal sealed class IngestionApplyExecutor(RunContext run, string repoRoot, string outDir, AspectIngestionProfile? profile = null)
    : Executor<IngestionReviewResponse>((profile ?? AspectIngestionProfile.Requirement).ExecutorIdPrefix + "Apply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly AspectIngestionProfile _p = profile ?? AspectIngestionProfile.Requirement;

    public override async ValueTask HandleAsync(IngestionReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var planPath = Path.Combine(outDir, "plan.json");
        var plan = JsonSerializer.Deserialize<StateChangePlanDocument>(await File.ReadAllTextAsync(planPath, ct).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"Plan nicht lesbar: {planPath}");
        var accepted = resp.AcceptedIncomingIds.ToHashSet(StringComparer.Ordinal);

        run.AppendEvent(new { type = _p.EventPrefix + "_APPLY_START", runId = run.RunId, accepted = accepted.Count, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
        var report = await IngestionApplyExec.ExecuteAsync(outDir, plan, accepted, repoRoot, run.RunId, ct, profile).ConfigureAwait(false);
        run.AppendEvent(new { type = _p.EventPrefix + "_DONE", runId = run.RunId, applied = true, appliedOps = report.Applied.Count, skipped = report.Skipped.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(report, ct).ConfigureAwait(false);
    }
}

internal static class IngestionHitlWorkflow
{
    public static Workflow Build(
        IngestionHitlResolveExecutor resolve, IngestionGateExecutor gate, IngestionRepairExecutor repair,
        IngestionHitlFinalizeExecutor finalize, RequestPort humanGate, IngestionApplyExecutor apply,
        AspectIngestionProfile? profile = null)
    {
        // Name aus dem Profil (req ergibt WÖRTLICH den bisherigen Namen "Requirement-Ingestion-HITL").
        var b = new WorkflowBuilder(resolve)
            .WithName($"{(profile ?? AspectIngestionProfile.Requirement).ItemLabel}-Ingestion-HITL")
            .WithDescription("MeetingDelta -> Resolve -> Gate -> [Repair] -> Finalize -> [RequestPort Human] -> Apply.");
        b.AddEdge(resolve, gate);
        b.AddEdge<IngestionVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<IngestionVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, humanGate);   // IngestionReviewRequest (nur bei Decision==Pass gesendet)
        b.AddEdge(humanGate, apply);      // IngestionReviewResponse
        b.WithOutputFrom(finalize);       // terminaler "needs manual"-Output
        b.WithOutputFrom(apply);          // terminaler Apply-Report
        return b.Build();
    }
}
