using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// Bootstrap-Zweig B3 (pipeline-full-bootstrap-plan §7): das cluster-review-gate als echtes RequestPort-Gate
// im Graph + deterministischer Apply-Knoten. Volle Wiederverwendung: die vier Cluster-Executors bleiben
// unverändert (06-backlog/reclarify), der Apply-Kern ist ReClarifyClusterApplyRunner.ExecuteAsync (eine
// Quelle für CLI UND Graph, Muster IngestionApplyExec). Fachliche Semantik = HITL-UI #15: ein Item pro
// ReviewAgent-OPERATION, Entscheidung apply/skip.

/// <summary>cluster-review-gate-Anfrage: die vom ReviewAgent vorgeschlagenen Operationen (ein Item je Op).</summary>
public sealed record ClusterReviewRequest(
    IReadOnlyList<ClusterOperation> Ops,
    bool GatePass,
    string ReviewVerdict);

/// <summary>Antwort des Gates: akzeptierte OpIds (apply) — Rest wird deterministisch geskippt.</summary>
public sealed record ClusterReviewResponse(IReadOnlyList<string> AcceptedOpIds, string ReviewedBy);

/// <summary>
/// replay: aufgezeichnete Cluster-Entscheide (human-decisions.json, UI-#15-Format) → OpId→apply?.
/// Erste Gate-Anbindung der echten Replay-Datei (Plan §5: replay matcht per OpId; unmatcht → reject, geloggt).
/// </summary>
public static class ClusterReviewReplay
{
    public static IReadOnlyDictionary<string, bool>? Load(string? replayPath)
    {
        if (string.IsNullOrWhiteSpace(replayPath) || !File.Exists(replayPath)) return null;
        var file = JsonSerializer.Deserialize<ClusterHumanDecisionsFile>(File.ReadAllText(replayPath), JsonFiles.Json);
        return file?.Decisions.ToDictionary(
            d => d.OpId,
            d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase),
            StringComparer.Ordinal);
    }
}

// BRIDGE (U1): Stufengrenze core-bootstrap -> cluster. Lädt die eben materialisierte Baseline FRISCH über
// den View-Port (exakt der CLI-Weg) und baut den Cluster-Input. Muster: IngestPbiBridgeExecutor.
[SendsMessage(typeof(ReClarifyClusterInput))]
internal sealed class ClusterBridgeExecutor(RunContext run, string repoRoot) : Executor<CoreBootstrapOutput>("PipelineClusterBridge")
{
    public override async ValueTask HandleAsync(CoreBootstrapOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var view = await new AgenticSdlc.Host.FullWorkflow.Delta.JsonProjectStateViewRepository(repoRoot)
            .GetCanonicalRequirementsViewAsync(AgenticSdlc.Host.FullWorkflow.Delta.ProjectScope.FromSourcePath(input.BaselinePath, "re-clarify", "current_baseline"))
            .ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_CLUSTER_BRIDGE", requirements = view.Baseline.Requirements.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ReClarifyClusterInput(view.Baseline, Path.GetRelativePath(repoRoot, input.BaselinePath))).ConfigureAwait(false);
    }
}

// GATE-REQUEST: hebt das Cluster-Ergebnis als RequestPort-Anfrage (der zentrale Gate-Responder antwortet).
[SendsMessage(typeof(ClusterReviewRequest))]
internal sealed class ClusterGateRequestExecutor(RunContext run) : Executor<ClusterResult>("PipelineClusterGateRequest")
{
    public override async ValueTask HandleAsync(ClusterResult result, IWorkflowContext context, CancellationToken ct = default)
    {
        run.AppendEvent(new
        {
            type = "CLUSTER_GATE_REQUEST",
            ops = result.Review.Operations.Count,
            gatePass = result.Report.Pass,
            reviewVerdict = result.Review.Verdict,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterReviewRequest(result.Review.Operations, result.Report.Pass, result.Review.Verdict)).ConfigureAwait(false);
    }
}

// APPLY (komponiert): akzeptierte Operationen deterministisch anwenden + Coverage-Re-Check — derselbe Kern
// wie der CLI-Runner (ReClarifyClusterApplyRunner.ExecuteAsync), Artefakte nach <clustersDir>/applied/.
[SendsMessage(typeof(ClusterApplyOutput))]
internal sealed class ClusterComposedApplyExecutor(RunContext run, string repoRoot, string clustersDir) : Executor<ClusterReviewResponse>("PipelineClusterApply")
{
    public override async ValueTask HandleAsync(ClusterReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var accepted = resp.AcceptedOpIds.ToHashSet(StringComparer.Ordinal);
        var (result, clustersBefore, appliedDir) = await ReClarifyClusterApplyRunner.ExecuteAsync(clustersDir, repoRoot, accepted).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "PIPELINE_CLUSTER_APPLIED",
            accepted = accepted.Count,
            applied = result.AppliedOpIds.Count,
            skipped = result.SkippedOps.Count,
            clustersBefore,
            clustersAfter = result.Updated.Clusters.Count,
            gatePass = result.Gate.Pass,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterApplyOutput(
            Path.Combine(appliedDir, "feature-clusters.json"),
            result.AppliedOpIds.Count, result.SkippedOps.Count,
            result.Gate.Pass, result.Updated.Clusters.Count)).ConfigureAwait(false);
    }
}
