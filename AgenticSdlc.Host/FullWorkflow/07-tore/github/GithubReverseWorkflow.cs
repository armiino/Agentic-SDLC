using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.5 Reverse als MAF-Workflow — rein DETERMINISTISCH (kein Agent), aber der Konsistenz/Austauschbarkeit wegen
// als Executor-Graph statt Inline-Runner:
//   Seed[Zustandsdiff -> Vorschlaege] -> Gate[E4-Invariante] -> Finalize[schreibt Plan]
// Zeigt: auch eine deterministische Pipeline sind austauschbare Knoten (keine agentische Stufe noetig/vorhanden).

internal sealed record GithubReverseWfContext(ProjectStateDocument Core, IReadOnlyList<GithubIssueSnapshot> Issues, string SnapshotRel, string OutDir);
internal sealed record GithubReverseDraftMsg(GithubReverseWfContext Ctx, IReadOnlyList<GithubReverseOp> Ops);
internal sealed record GithubReverseVerdictMsg(GithubReverseWfContext Ctx, GithubReversePlanDocument Plan, GithubReverseGateReport Report);
internal sealed record GithubReverseWfResult(GithubReversePlanDocument Plan, GithubReverseGateReport Gate);

// SEED: deterministischer Zustandsdiff (Snapshot vs Core-Mappings) -> Reverse-Vorschlaege (PBI_DONE?/Sync/Drift).
[SendsMessage(typeof(GithubReverseDraftMsg))]
internal sealed class GithubReverseSeedExecutor(RunContext run) : Executor<GithubReverseWfContext>("GithubReverseSeed")
{
    public override async ValueTask HandleAsync(GithubReverseWfContext ctx, IWorkflowContext context, CancellationToken ct = default)
    {
        var ops = GithubReverseSeed.Seed(ctx.Core, ctx.Issues);
        run.AppendEvent(new { type = "GITHUB_REV_SEED", runId = run.RunId, ops = ops.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubReverseDraftMsg(ctx, ops)).ConfigureAwait(false);
    }
}

// GATE: deterministisch (E4: PBI_DONE verifikationspflichtig, echtes PBI + Mapping, kein Doppel).
[SendsMessage(typeof(GithubReverseVerdictMsg))]
internal sealed class GithubReverseGateExecutor(RunContext run) : Executor<GithubReverseDraftMsg>("GithubReverseGate")
{
    public override async ValueTask HandleAsync(GithubReverseDraftMsg draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = new GithubReversePlanDocument(
            GithubReversePlanDocument.CurrentSchemaVersion, $"github-reverse-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            DateTime.UtcNow, draft.Ctx.SnapshotRel, draft.Ops);
        var gate = GithubReverseGate.Check(draft.Ctx.Core, plan);
        run.AppendEvent(new { type = "GITHUB_REV_GATE", runId = run.RunId, pass = gate.Pass, errors = gate.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubReverseVerdictMsg(draft.Ctx, plan, gate)).ConfigureAwait(false);
    }
}

// FINALIZE: schreibt Plan + Gate-Report + Summary.
[YieldsOutput(typeof(GithubReverseWfResult))]
internal sealed class GithubReverseFinalizeExecutor(RunContext run) : Executor<GithubReverseVerdictMsg>("GithubReverseFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(GithubReverseVerdictMsg v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(v.Ctx.OutDir);
        var ops = v.Plan.Operations;
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "github-reverse-plan.json"), JsonSerializer.Serialize(v.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "github-reverse-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(v.Ctx.OutDir, "github-reverse-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            ops = ops.Count,
            gatePass = v.Report.Pass,
            gateErrors = v.Report.Errors.Count,
            byKind = ops.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "GITHUB_REV_DONE", runId = run.RunId, gatePass = v.Report.Pass, ops = ops.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new GithubReverseWfResult(v.Plan, v.Report)).ConfigureAwait(false);
    }
}

internal static class GithubReverseWorkflow
{
    public const string WorkflowName = "GitHub-Reverse-Feedback";

    public static Workflow Build(GithubReverseSeedExecutor seed, GithubReverseGateExecutor gate, GithubReverseFinalizeExecutor finalize)
    {
        var b = new WorkflowBuilder(seed)
            .WithName(WorkflowName)
            .WithDescription("Snapshot+Core -> Seed[det. Zustandsdiff] -> Gate[E4] -> Finalize.");
        b.AddEdge(seed, gate); b.AddEdge(gate, finalize); b.WithOutputFrom(finalize);
        return b.Build();
    }
}
