using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// Bootstrap-Zweig B4 (pipeline-full-bootstrap-plan §7): das backlog-review-gate (EDIT-fähig!) + det. Apply +
// core-seed-backlog als Graph-Knoten. Volle Wiederverwendung: Clarify-Executors bleiben unverändert, Apply-Kern =
// ReClarifyBacklogApplyRunner.ExecuteAsync, Seed-Kern = CoreSeedBacklogRunner.ExecuteAsync (eine Quelle CLI+Graph).
// Fachliche Semantik = HITL-UI #14: ein Item pro PBI, Entscheidung accept/edit/reject — Response transportiert
// die Entscheide 1:1 als BacklogHumanDecision (inkl. EditedPbiJson), damit replay Edits identisch einspielt.

/// <summary>backlog-review-gate-Anfrage: die geschnittenen PBIs (ein Item je PBI, UI #14).</summary>
public sealed record BacklogReviewRequest(IReadOnlyList<string> PbiIds, bool GatePass);

/// <summary>Antwort: Entscheide je PBI (fehlender Eintrag = accept — exakt die CLI-Apply-Semantik).</summary>
public sealed record BacklogReviewResponse(IReadOnlyList<BacklogHumanDecision> Decisions, string ReviewedBy);

/// <summary>
/// Policy-Auflösung fürs EDIT-fähige Backlog-Gate (Plan §5): accept-all = leere Entscheidungsliste (Apply
/// behandelt fehlende Entscheide als accept, keine Edits); replay = aufgezeichnete human-decisions.json 1:1
/// (inkl. EditedPbiJson — Edits werden bit-identisch zum CLI wieder eingespielt; PBIs ohne Eintrag = accept,
/// wie im CLI); interactive = null (Pause). Der generische GateResponder passt hier NICHT (accept/reject-only).
/// </summary>
public static class BacklogReviewResolver
{
    public static IReadOnlyList<BacklogHumanDecision>? Resolve(GatePolicy policy) => policy.Kind switch
    {
        GatePolicyKind.AcceptAll => [],
        GatePolicyKind.Replay => LoadDecisions(policy.ReplayPath) ?? [],
        _ => null,
    };

    public static IReadOnlyList<BacklogHumanDecision>? LoadDecisions(string? replayPath)
    {
        if (string.IsNullOrWhiteSpace(replayPath) || !File.Exists(replayPath)) return null;
        return JsonSerializer.Deserialize<BacklogHumanDecisionsFile>(File.ReadAllText(replayPath), JsonFiles.Json)?.Decisions;
    }
}

// BRIDGE (U1): Stufengrenze cluster-apply -> clarify. Lädt die applied Cluster + Baseline FRISCH
// (exakt die CLI-Präferenz applied > roh) und baut den Clarify-Input. Muster: IngestPbiBridgeExecutor.
[SendsMessage(typeof(ReClarifyBacklogInput))]
internal sealed class BacklogBridgeExecutor(RunContext run, string repoRoot) : Executor<ClusterApplyOutput>("PipelineBacklogBridge")
{
    private static readonly System.Text.Json.JsonSerializerOptions Json = JsonFiles.Json;

    public override async ValueTask HandleAsync(ClusterApplyOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var clusters = System.Text.Json.JsonSerializer.Deserialize<FeatureClusterSet>(
            await File.ReadAllTextAsync(input.AppliedClustersPath, ct).ConfigureAwait(false), Json)!;
        var view = await new AgenticSdlc.Host.FullWorkflow.Delta.JsonProjectStateViewRepository(repoRoot)
            .GetCanonicalRequirementsViewAsync(AgenticSdlc.Host.FullWorkflow.Delta.ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
            .ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_BACKLOG_BRIDGE", clusters = clusters.Clusters.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ReClarifyBacklogInput(clusters, view.Baseline, Path.GetRelativePath(repoRoot, input.AppliedClustersPath))).ConfigureAwait(false);
    }
}

// GATE-REQUEST: hebt das Backlog-Ergebnis als RequestPort-Anfrage (der zentrale Gate-Responder antwortet).
[SendsMessage(typeof(BacklogReviewRequest))]
internal sealed class BacklogGateRequestExecutor(RunContext run) : Executor<BacklogResult>("PipelineBacklogGateRequest")
{
    public override async ValueTask HandleAsync(BacklogResult result, IWorkflowContext context, CancellationToken ct = default)
    {
        run.AppendEvent(new
        {
            type = "BACKLOG_GATE_REQUEST",
            pbis = result.Backlog.Items.Count,
            gatePass = result.Report.Pass,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new BacklogReviewRequest(
            result.Backlog.Items.Select(p => p.PbiId).ToList(), result.Report.Pass)).ConfigureAwait(false);
    }
}

// APPLY (komponiert): Entscheide deterministisch anwenden (accept/edit/reject) + Traceability + DoR-Re-Check —
// derselbe Kern wie der CLI-Runner, Artefakte nach <backlogDir>/applied/.
[SendsMessage(typeof(BacklogApplyOutput))]
internal sealed class BacklogComposedApplyExecutor(RunContext run, string repoRoot, string backlogDir) : Executor<BacklogReviewResponse>("PipelineBacklogApply")
{
    public override async ValueTask HandleAsync(BacklogReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var exec = await ReClarifyBacklogApplyRunner.ExecuteAsync(backlogDir, repoRoot, resp.Decisions).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "PIPELINE_BACKLOG_APPLIED",
            decisions = resp.Decisions.Count,
            pbisBefore = exec.PbisBefore,
            pbisAfter = exec.PbisAfter,
            dropped = exec.Dropped.Count,
            gatePass = exec.Gate.Pass,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new BacklogApplyOutput(
            exec.AppliedBacklogPath, exec.PbisBefore, exec.PbisAfter, exec.Dropped.Count, exec.Gate.Pass)).ConfigureAwait(false);
    }
}

// SEED: Features + PBIs in den Core heben (idempotent, Port-only-Write) — derselbe Kern wie core-seed-backlog.
[SendsMessage(typeof(BacklogSeedOutput))]
internal sealed class CoreSeedBacklogExecutor(RunContext run, string repoRoot) : Executor<BacklogApplyOutput>("PipelineCoreSeedBacklog")
{
    public override async ValueTask HandleAsync(BacklogApplyOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var (report, itemsBefore, itemsAfter) = await CoreSeedBacklogRunner
            .ExecuteAsync(input.AppliedBacklogPath, repoRoot, run.RunId).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "STAGE_CORE_SEED_BACKLOG_DONE",
            featuresAdded = report.FeaturesAdded,
            pbisAdded = report.PbisAdded,
            relationsAdded = report.RelationsAdded,
            coreItems = itemsAfter,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new BacklogSeedOutput(
            report.FeaturesAdded, report.PbisAdded, report.RelationsAdded, itemsBefore, itemsAfter)).ConfigureAwait(false);
    }
}
