using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// S6 (Worklist 20.07) — A: die migrierten Stufen-Workflows zu EINEM durchgehenden MAF-Graphen komponiert. Beleg an
// der natuerlichsten Kette ingest -> pbi-update (ingest-Apply erzeugt das Delta, das pbi-update-Derive konsumiert):
//
//   IngestResolve -> IngestGate -> [IngestRepair] -> IngestFinalize -> [Port1 Human] -> IngestApply
//        -> Bridge (Core FRISCH laden) -> PbiDerive -> PbiMaker -> PbiGate -> [PbiRepair] -> PbiFinalize -> [Port2 Human] -> PbiApply
//
// ZWEI Human-Gates in EINEM Lauf (prozessuebergreifend resumebar: start -> resume(Gate1) -> resume(Gate2)). Weil die
// Invarianten aus Decision 20.07 §4 in S1-S4 eingehalten sind (geteilte idempotente Applies, Core extern, typisierte
// Ports), ist A eine KOMPOSITION bestehender Knoten, kein Neubau. Die Stufen-HITL-Runner bleiben eigenstaendig.

// APPLY-Ingest (komponiert): wie IngestionApplyExecutor, aber SENDET den Report an die Bridge (statt terminal zu yielden).
[SendsMessage(typeof(IngestionApplyReport))]
internal sealed class IngestComposedApplyExecutor(RunContext run, string repoRoot, string ingestOutDir) : Executor<IngestionReviewResponse>("PipelineIngestApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(IngestionReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = JsonSerializer.Deserialize<StateChangePlanDocument>(await File.ReadAllTextAsync(Path.Combine(ingestOutDir, "plan.json"), ct).ConfigureAwait(false), Json)!;
        var accepted = resp.AcceptedIncomingIds.ToHashSet(StringComparer.Ordinal);
        var report = await IngestionApplyExec.ExecuteAsync(ingestOutDir, plan, accepted, repoRoot, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_INGEST_APPLIED", runId = run.RunId, applied = report.Applied.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(report).ConfigureAwait(false);
    }
}

// BRIDGE: Stufengrenze. Laedt den (durch IngestApply mutierten) Core FRISCH und baut den pbi-update-Eingang aus dem
// Ingest-Applied-Delta. Kein Core im Checkpoint — der Core ist System-of-Record (Decision 20.07 §3.1).
[SendsMessage(typeof(PbiUpdateWfContext))]
internal sealed class IngestPbiBridgeExecutor(RunContext run, string repoRoot, string pbiOutDir, int maxAttempts) : Executor<IngestionApplyReport>("PipelineBridge")
{
    public override async ValueTask HandleAsync(IngestionApplyReport ingestReport, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_BRIDGE", runId = run.RunId, appliedIn = ingestReport.Applied.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new PbiUpdateWfContext(core, ingestReport.Applied, run.RunId, pbiOutDir, DryRun: false, maxAttempts)).ConfigureAwait(false);
    }
}

internal static class PipelineComposedWorkflow
{
    public static Workflow Build(
        // Ingest-Knoten
        IngestionHitlResolveExecutor ingestResolve, IngestionGateExecutor ingestGate, IngestionRepairExecutor ingestRepair,
        IngestionHitlFinalizeExecutor ingestFinalize, RequestPort ingestPort, IngestComposedApplyExecutor ingestApply,
        // Bruecke
        IngestPbiBridgeExecutor bridge,
        // Pbi-Knoten
        PbiUpdateDeriveExecutor pbiDerive, PbiUpdateMakerExecutor pbiMaker, PbiUpdateGateExecutor pbiGate, PbiUpdateRepairExecutor pbiRepair,
        PbiUpdateHitlFinalizeExecutor pbiFinalize, RequestPort pbiPort, PbiUpdateApplyExecutor pbiApply)
    {
        var b = new WorkflowBuilder(ingestResolve)
            .WithName("Pipeline-Ingest-PbiUpdate-HITL")
            .WithDescription("Ingest -> [Human] -> Apply -> Bridge -> PbiUpdate -> [Human] -> Apply. Zwei Gates, ein Graph.");

        // Stufe 1: Ingest
        b.AddEdge(ingestResolve, ingestGate);
        b.AddEdge<IngestionVerdict>(ingestGate, ingestRepair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<IngestionVerdict>(ingestGate, ingestFinalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(ingestRepair, ingestGate);
        b.AddEdge(ingestFinalize, ingestPort);
        b.AddEdge(ingestPort, ingestApply);

        // Bruecke Stufe 1 -> Stufe 2
        b.AddEdge(ingestApply, bridge);
        b.AddEdge(bridge, pbiDerive);

        // Stufe 2: PbiUpdate
        b.AddEdge(pbiDerive, pbiMaker);
        b.AddEdge(pbiMaker, pbiGate);
        b.AddEdge<PbiUpdateVerdict>(pbiGate, pbiRepair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<PbiUpdateVerdict>(pbiGate, pbiFinalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(pbiRepair, pbiGate);
        b.AddEdge(pbiFinalize, pbiPort);
        b.AddEdge(pbiPort, pbiApply);

        b.WithOutputFrom(ingestFinalize);  // terminal, falls Ingest-Gate scheitert (Pipeline-Abbruch)
        b.WithOutputFrom(pbiFinalize);     // terminal, falls Pbi-Gate scheitert
        b.WithOutputFrom(pbiApply);        // terminal bei Erfolg
        return b.Build();
    }
}
