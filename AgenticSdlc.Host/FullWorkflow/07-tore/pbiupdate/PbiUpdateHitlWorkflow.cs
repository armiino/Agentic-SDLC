using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Globalization;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// S4 (Worklist 20.07): pbi-update als EIN MAF-Lauf MIT MAF-nativem Human-Gate (RequestPort) statt CLI -review/-apply.
// Muster identisch zu GithubForwardHitlWorkflow (S1). Wiederverwendet Derive/Maker/Gate/Repair unveraendert.
//
//   Derive -> Maker -> Gate --repair--> Repair --(Loop)--> Gate
//                        ├── Pass ─────────────> Finalize -> [RequestPort Human] -> Apply -> (Ende)
//                        └── Human/MaxAttempts ─> Finalize -(yield "needs manual")-> (Ende, KEIN Apply)
//
// Invarianten: Plan = Disk-Artefakt; Core wird im Apply FRISCH geladen (nicht im Checkpoint); nur die Entscheidung
// fliesst durch den Port. Achtung: pbi-update-apply MUTIERT den Core (anders als github-forward LINK) — Idempotenz
// bei Doppel-Resume ist ein eigener, noch offener Punkt (Update-by-Identity; s. Worklist S4-Residual).

public sealed record PbiUpdateReviewRequest(string RunId, string SourceIngestionRun, IReadOnlyList<PbiUpdateReviewOpView> Ops);
public sealed record PbiUpdateReviewOpView(string OpId, string Kind, string? PbiId, string? RequirementId, string Rationale);
// R-26-C: die Response traegt zusaetzlich die AKZEPTIERTEN Angleichungen (schon mit den Human-Edits) — sie
// werden im selben Review autorisiert. Optional/abwaertskompatibel: alte 2-Positional-Aufrufer bleiben gueltig.
public sealed record PbiUpdateReviewResponse(IReadOnlyList<string> AcceptedOpIds, string Reviewer,
    IReadOnlyList<PbiAlignment>? AcceptedAlignments = null);

// FINALIZE (HITL): schreibt Plan/Gate/Attempts/Summary (Evidenz, wie PbiUpdateFinalize) und verzweigt:
//   Decision==Pass -> PbiUpdateReviewRequest an den Port. Sonst -> terminaler "needs manual"-Output.
[SendsMessage(typeof(PbiUpdateReviewRequest))]
[YieldsOutput(typeof(PbiUpdateWfResult))]
internal sealed class PbiUpdateHitlFinalizeExecutor(RunContext run) : Executor<PbiUpdateVerdict>("PbiUpdateHitlFinalize")
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

        if (v.Decision == GateDecision.Pass)
        {
            var views = ops.Select((o, i) => new PbiUpdateReviewOpView($"op-{i}", o.Kind, o.PbiId, o.RequirementId, o.Rationale)).ToList();
            run.AppendEvent(new { type = "PBI_UPDATE_HUMAN_GATE", runId = run.RunId, operations = ops.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(new PbiUpdateReviewRequest(run.RunId, ctx.SourceIngestionRun, views)).ConfigureAwait(false);
        }
        else
        {
            run.AppendEvent(new { type = "PBI_UPDATE_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(new PbiUpdateWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
        }
    }
}

// APPLY (HITL): konsumiert die menschliche Response. Laedt den Plan FRISCH von Platte, Core FRISCH (in ExecuteAsync)
// -> identisch zum CLI-Apply. Mutiert den Core deterministisch + schreibt github-sync-Delta.
[YieldsOutput(typeof(PbiUpdateApplyReport))]
[SendsMessage(typeof(PbiUpdateApplyReport))] // U2 (Ein-Graph): Report fließt zusätzlich als Message weiter (Forward-Bridge); ohne Kante wirkungslos
internal sealed class PbiUpdateApplyExecutor(RunContext run, string repoRoot, string outDir) : Executor<PbiUpdateReviewResponse>("PbiUpdateApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(PbiUpdateReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var planPath = Path.Combine(outDir, "pbi-change-plan.json");
        var plan = JsonSerializer.Deserialize<PbiStateChangePlanDocument>(await File.ReadAllTextAsync(planPath, ct).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"Plan nicht lesbar: {planPath}");
        var accepted = resp.AcceptedOpIds
            .Select(id => id.StartsWith("op-", StringComparison.Ordinal) && int.TryParse(id["op-".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : -1)
            .Where(n => n >= 0).ToHashSet();

        run.AppendEvent(new { type = "PBI_UPDATE_APPLY_START", runId = run.RunId, accepted = accepted.Count, alignments = resp.AcceptedAlignments?.Count ?? 0, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
        // R-26-C: die im Review autorisierten Angleichungen werden mitgeschrieben (needs_clarify -> active).
        var report = await PbiUpdateApplyExec.ExecuteAsync(outDir, plan, accepted, repoRoot, acceptedAlignments: resp.AcceptedAlignments, ct: ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "PBI_UPDATE_DONE", runId = run.RunId, applied = true, newPbis = report.NewPbis.Count, updatedPbis = report.UpdatedPbis.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(report, ct).ConfigureAwait(false);
        await context.SendMessageAsync(report).ConfigureAwait(false);
    }
}

internal static class PbiUpdateHitlWorkflow
{
    public const string WorkflowName = "Incremental-PBI-Update-HITL";

    public static Workflow Build(
        PbiUpdateDeriveExecutor derive, PbiUpdateMakerExecutor maker, PbiUpdateGateExecutor gate,
        PbiUpdateRepairExecutor repair, PbiAlignExecutor align, PbiUpdateHitlFinalizeExecutor finalize,
        RequestPort humanGate, PbiUpdateApplyExecutor apply)
    {
        var b = new WorkflowBuilder(derive)
            .WithName(WorkflowName)
            .WithDescription("Ingestion-Delta -> Derive -> Maker -> Gate -> [Repair] -> Align (R-26-C) -> Finalize -> [RequestPort Human] -> Apply.");
        b.AddEdge(derive, maker);
        b.AddEdge(maker, gate);
        b.AddEdge<PbiUpdateVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        // R-26-C: NUR bei bestandenem Gate (Pass) reichert der Angleichungs-Agent den Plan an — VOR dem
        // Human-Gate, damit der Mensch die Angleichungen im selben Review autorisiert. HumanReview/
        // MaxAttemptsReached gehen direkt zu Finalize (terminaler manual-Output, kein LLM-Call).
        b.AddEdge<PbiUpdateVerdict>(gate, align, m => m is not null && m.Decision == GateDecision.Pass);
        b.AddEdge(align, finalize);
        b.AddEdge<PbiUpdateVerdict>(gate, finalize, m => m is not null && m.Decision is not GateDecision.Pass and not GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, humanGate);   // PbiUpdateReviewRequest (nur bei Decision==Pass gesendet)
        b.AddEdge(humanGate, apply);      // PbiUpdateReviewResponse
        b.WithOutputFrom(finalize);       // terminaler "needs manual"-Output
        b.WithOutputFrom(apply);          // terminaler Apply-Report
        return b.Build();
    }
}
