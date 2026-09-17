using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Globalization;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// S4 (Worklist 20.07): decision (Tor 2) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort), Muster wie
// github-forward/pbi-update. Zwei Graphen (wie das Original), beide mit Human-Gate + Apply:
//   deterministisch: Derive -> Gate -> Finalize -> [RequestPort] -> Apply
//   agentisch:       Maker -> Derive -> Gate --[repairable]--> Repair -> Derive (Loop) / Finalize -> [RequestPort] -> Apply
// Invarianten: Plan = Disk-Artefakt; Core im Apply FRISCH geladen (nicht im Checkpoint); nur Entscheidung durch den Port.
// decision-apply MUTIERT den Core -> plan-level Idempotenz-Marker in DecisionApplyExec (S3-analog).

public sealed record DecisionReviewRequest(string RunId, IReadOnlyList<DecisionReviewOpView> Ops);
public sealed record DecisionReviewOpView(string OpId, string Outcome, string DecisionId, string TargetRequirementId, IReadOnlyList<string> AffectedPbis, string Rationale);
public sealed record DecisionReviewResponse(IReadOnlyList<string> AcceptedOpIds, string Reviewer);

// FINALIZE (HITL): schreibt Plan/Gate/Attempts/Summary (wie DecisionFinalize) und verzweigt:
//   Decision==Pass -> DecisionReviewRequest an den Port. Sonst -> terminaler "needs manual"-Output.
[SendsMessage(typeof(DecisionReviewRequest))]
[YieldsOutput(typeof(DecisionWfResult))]
internal sealed class DecisionHitlFinalizeExecutor(RunContext run) : Executor<DecisionVerdictMsg>("DecisionHitlFinalize")
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

        if (v.Decision == GateDecision.Pass)
        {
            var views = v.Plan.Operations.Select((o, i) => new DecisionReviewOpView($"op-{i}", o.Outcome, o.DecisionId, o.TargetRequirementId ?? "", o.AffectedPbis, o.Rationale)).ToList();
            run.AppendEvent(new { type = "DECISION_HUMAN_GATE", runId = run.RunId, operations = v.Plan.Operations.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(new DecisionReviewRequest(run.RunId, views)).ConfigureAwait(false);
        }
        else
        {
            run.AppendEvent(new { type = "DECISION_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(new DecisionWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
        }
    }
}

// APPLY (HITL): konsumiert die menschliche Response. Laedt Plan+Core FRISCH (in DecisionApplyExec) -> identisch zum CLI.
[YieldsOutput(typeof(DecisionResolutionApplyReport))]
internal sealed class DecisionApplyExecutor(RunContext run, string repoRoot, string outDir) : Executor<DecisionReviewResponse>("DecisionApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(DecisionReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var planPath = Path.Combine(outDir, "decision-plan.json");
        var plan = JsonSerializer.Deserialize<DecisionResolutionPlanDocument>(await File.ReadAllTextAsync(planPath, ct).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"Plan nicht lesbar: {planPath}");
        var accepted = resp.AcceptedOpIds
            .Select(id => id.StartsWith("op-", StringComparison.Ordinal) && int.TryParse(id["op-".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : -1)
            .Where(n => n >= 0).ToHashSet();

        run.AppendEvent(new { type = "DECISION_APPLY_START", runId = run.RunId, accepted = accepted.Count, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
        var report = await DecisionApplyExec.ExecuteAsync(outDir, plan, accepted, repoRoot, run.RunId, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "DECISION_DONE", runId = run.RunId, applied = true, resolved = report.Resolved.Count, unblocked = report.UnblockedPbis.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(report, ct).ConfigureAwait(false);
    }
}

internal static class DecisionHitlWorkflow
{
    // Deterministisch: Derive -> Gate -> Finalize -> [Port] -> Apply.
    public static Workflow BuildDeterministic(
        DecisionDeriveExecutor derive, DecisionGateExecutor gate, DecisionHitlFinalizeExecutor finalize,
        RequestPort humanGate, DecisionApplyExecutor apply)
    {
        var b = new WorkflowBuilder(derive).WithName("Decision-Ingestion-HITL")
            .WithDescription("Auflösungen -> Derive -> Gate -> Finalize -> [RequestPort Human] -> Apply.");
        b.AddEdge(derive, gate);
        b.AddEdge(gate, finalize);
        b.AddEdge(finalize, humanGate);
        b.AddEdge(humanGate, apply);
        b.WithOutputFrom(finalize);
        b.WithOutputFrom(apply);
        return b.Build();
    }

    // Agentisch: Maker -> Derive -> Gate --[repairable]--> Repair -> Derive (Loop) / Finalize -> [Port] -> Apply.
    public static Workflow BuildAgentic(
        DecisionMakerExecutor maker, DecisionDeriveExecutor derive, DecisionGateExecutor gate, DecisionRepairExecutor repair,
        DecisionHitlFinalizeExecutor finalize, RequestPort humanGate, DecisionApplyExecutor apply)
    {
        var b = new WorkflowBuilder(maker).WithName("Decision-Ingestion-HITL-Agent")
            .WithDescription("Freie Antwort -> Maker -> Derive -> Gate --[repairable]--> Repair (Loop) / Finalize -> [RequestPort] -> Apply.");
        b.AddEdge(maker, derive);
        b.AddEdge(derive, gate);
        b.AddEdge<DecisionVerdictMsg>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<DecisionVerdictMsg>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, derive);
        b.AddEdge(finalize, humanGate);
        b.AddEdge(humanGate, apply);
        b.WithOutputFrom(finalize);
        b.WithOutputFrom(apply);
        return b.Build();
    }
}
