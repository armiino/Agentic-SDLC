using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed record ReClarifyClusterInput(CanonicalRequirementsBaseline Baseline, string SourceBaselinePath);

internal sealed record ClusterDraft(CanonicalRequirementsBaseline Baseline, string SourceBaselinePath, IReadOnlyList<FeatureCluster> Clusters, bool Saved, int ToolCheckRounds);
internal sealed record ClusterVerdict(CanonicalRequirementsBaseline Baseline, string SourceBaselinePath, IReadOnlyList<FeatureCluster> Clusters, ReClarifyGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record ClusterReviewed(CanonicalRequirementsBaseline Baseline, string SourceBaselinePath, IReadOnlyList<FeatureCluster> Clusters, ReClarifyGateReport Report, ClusterReviewReport Review, bool Saved, int ToolCheckRounds);
internal sealed record ClusterResult(FeatureClusterSet Clusters, ReClarifyGateReport Report, ClusterReviewReport Review);

// MAKER: gruppiert Requirements semantisch zu Feature-Clustern (Bedeutung, nicht Wortabgleich).
[SendsMessage(typeof(ClusterDraft))]
internal sealed class ClusterAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RelationLookup relations,
    RunContext run)
    : Executor<ReClarifyClusterInput>("L4ReClarifyClusterAgent")
{
    public override async ValueTask HandleAsync(ReClarifyClusterInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new ReClarifyClusterTools(input.Baseline, relations, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Gruppiere die kanonischen Requirements zu Feature-Clustern.

                   Ein Feature-Cluster buendelt die Requirements EINES fachlichen Features. Nutze deine Tools:
                   erkunde Requirements, folge AUTHORED Relationen (get_related_requirements), pruefe deine
                   Gruppierung mit check_clusters.

                   Regeln:
                   - coreRequirementIds: die Requirements, die DIESES Feature ausmachen. Jedes aktive Requirement
                     ist in GENAU EINEM Cluster core.
                   - crossCuttingRequirementIds: Regeln (z.B. Eingabe/Validierung, Rollen/Rechte, Datenschutz/
                     Lifecycle), die das Feature MIT-betreffen, aber Kern eines ANDEREN Clusters sind. Sie duerfen
                     in mehreren Clustern als crossCutting auftauchen - NICHT in core mergen.
                   - Gruppiere nach fachlicher Bedeutung, nicht nach zufaelligen Wortuebereinstimmungen.

                   Pruefe mit check_clusters (Coverage muss pass sein) und speichere genau einmal mit save_clusters.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var clusters = tools.SavedClusters ?? [];
        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            clusters = clusters.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterDraft(input.Baseline, input.SourceBaselinePath, clusters, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

// CHECKER 1: deterministisches Coverage-Gate (Recall-Garantie).
[SendsMessage(typeof(ClusterVerdict))]
internal sealed class ClusterGateExecutor(RunContext run) : Executor<ClusterDraft>("L4ReClarifyClusterGate")
{
    public override async ValueTask HandleAsync(ClusterDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ReClarifyClusterGate.Check(draft.Baseline, draft.Clusters);
        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterVerdict(draft.Baseline, draft.SourceBaselinePath, draft.Clusters, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

// CHECKER 2: zweiter Agent kritisiert die Cluster fachlich (Feedback-Signal).
[SendsMessage(typeof(ClusterReviewed))]
internal sealed class ClusterReviewExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<ClusterVerdict>("L4ReClarifyClusterReviewAgent")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(ClusterVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new ReClarifyClusterReviewTools(verdict.Baseline, run);
        var agent = agentFactory(tools.Build());
        var clustersJson = JsonSerializer.Serialize(verdict.Clusters, Json);
        var task = $$"""
                   Pruefe die vorgeschlagenen Feature-Cluster als unabhaengiger Requirements Engineer.

                   Vorgeschlagene Cluster:
                   {{clustersJson}}

                   Deterministisches Coverage-Gate: pass={{verdict.Report.Pass}}, errors={{verdict.Report.Errors.Count}}.

                   Beurteile fachlich (nicht die Coverage - die prueft das Gate):
                   - Cluster faelschlich VERSCHMOLZEN? -> kind=wrong_merge
                   - Feature faelschlich GETEILT? -> kind=wrong_split
                   - Querschnitt-Requirement faelschlich als core statt crossCutting (oder umgekehrt)? -> kind=miscategorized_crosscutting
                   - Granularitaet unpassend? -> kind=granularity

                   WICHTIG: Zu JEDEM fachlichen Fehler liefere eine KONKRETE, anwendbare Operation, damit der
                   Mensch sie mit einem Klick uebernehmen kann. Verfuegbare operations.kind:
                   - add_crosscutting    (requirementId, toClusterId)     Querschnitt-Regel einem Feature-Cluster zufuegen
                   - remove_crosscutting (requirementId, fromClusterId)   falschen crossCutting-Verweis entfernen
                   - move_core           (requirementId, fromClusterId, toClusterId)  Kern-Requirement in den richtigen Cluster
                   - new_cluster         (newClusterId, label, identityKey, coreRequirementIds[])  Feature ausgliedern (Split)
                   - merge_clusters      (fromClusterId, toClusterId)     zwei Cluster zusammenfuehren
                   Jede Operation braucht eine rationale. Gib nur Operationen, die du fachlich belegen kannst.

                   Nutze get_requirement, wo du Text brauchst. Speichere genau einmal mit save_review:
                   verdict=approve + leere operations, wenn die Cluster tragen; sonst verdict=revise mit findings UND operations.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var review = tools.SavedReview ?? new ClusterReviewReport(
            SchemaVersion: ClusterReviewReport.CurrentSchemaVersion,
            ReviewId: $"cluster-review-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            Verdict: "revise",
            Summary: "ReviewAgent hat kein Review gespeichert.",
            Findings: []);
        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_REVIEW_DONE",
            runId = run.RunId,
            verdict = review.Verdict,
            findings = review.Findings.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterReviewed(verdict.Baseline, verdict.SourceBaselinePath, verdict.Clusters, verdict.Report, review, verdict.Saved, verdict.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(ClusterResult))]
internal sealed class ClusterFinalizeExecutor(RunContext run, string outDir) : Executor<ClusterReviewed>("L4ReClarifyClusterFinalize")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(ClusterReviewed reviewed, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        var set = new FeatureClusterSet(
            SchemaVersion: FeatureClusterSet.CurrentSchemaVersion,
            ClusterSetId: $"cluster-set-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: reviewed.Baseline.ProjectId,
            BaselineId: reviewed.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceBaselinePath: reviewed.SourceBaselinePath,
            Clusters: reviewed.Clusters);

        await File.WriteAllTextAsync(Path.Combine(outDir, "feature-clusters.json"), JsonSerializer.Serialize(set, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-gate-report.json"), JsonSerializer.Serialize(reviewed.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-review.json"), JsonSerializer.Serialize(reviewed.Review, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            reviewed.Saved,
            reviewed.ToolCheckRounds,
            gatePass = reviewed.Report.Pass,
            gateErrors = reviewed.Report.Errors.Count,
            reviewVerdict = reviewed.Review.Verdict,
            reviewFindings = reviewed.Review.Findings.Count,
            clusters = reviewed.Clusters.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_DONE",
            runId = run.RunId,
            gatePass = reviewed.Report.Pass,
            reviewVerdict = reviewed.Review.Verdict,
            clusters = reviewed.Clusters.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new ClusterResult(set, reviewed.Report, reviewed.Review)).ConfigureAwait(false);
    }
}

internal static class ReClarifyClusterWorkflow
{
    public const string WorkflowName = "L4-ReClarify-Cluster";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        ClusterAgentExecutor agent,
        ClusterGateExecutor gate,
        ClusterReviewExecutor review,
        ClusterFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("CanonicalRequirements -> ClusterAgent[Tools] (maker) -> Gate[det Coverage] "
                           + "-> ReviewAgent[Tools] (checker) -> Finalize. Feature-Cluster als Analyse-Granularitaet.");

        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, review);
        builder.AddEdge(review, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
