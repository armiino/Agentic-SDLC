using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed record ReClarifyClusterInput(CanonicalRequirementsBaseline Baseline, string SourceBaselinePath, int MaxAttempts);

// R-33 S2: Attempt/Source/History reisen in den Messages (MAF-superstep, Graph bleibt stateless) —
// dieselbe Form wie S1/PbiUpdate. Der Verdict traegt das typisierte GateDecision fuer die Kanten.
internal sealed record ClusterDraft(ReClarifyClusterInput Input, IReadOnlyList<FeatureCluster> Clusters, bool Saved, int ToolCheckRounds, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record ClusterVerdict(ReClarifyClusterInput Input, IReadOnlyList<FeatureCluster> Clusters, ReClarifyGateReport Report, GateDecision Decision, bool Saved, int ToolCheckRounds, int Attempt, IReadOnlyList<GateAttempt> History);
internal sealed record ClusterReviewed(ReClarifyClusterInput Input, IReadOnlyList<FeatureCluster> Clusters, ReClarifyGateReport Report, ClusterReviewReport Review, GateDecision Decision, bool Saved, int ToolCheckRounds, int Attempt, IReadOnlyList<GateAttempt> History);
internal sealed record ClusterResult(FeatureClusterSet Clusters, ReClarifyGateReport Report, ClusterReviewReport Review);

// Geteilter Agent-Aufruf von Maker UND Repair (eine Quelle): RelationLookup zur Laufzeit (U1), Tools bauen,
// Agent laufen lassen, gespeicherte Cluster einsammeln. Kein Zustand ausserhalb der Tools-Instanz.
internal static class ClusterAgentRunner
{
    public static async Task<(IReadOnlyList<FeatureCluster> Clusters, bool Saved, int CheckRounds, bool HasRelations)> RunAsync(
        ReClarifyClusterInput input, Func<IReadOnlyList<AITool>, AIAgent> factory, string repoRoot, RunContext run, string task, CancellationToken ct)
    {
        var relations = await RelationLookup.BuildAsync(input.Baseline, repoRoot, ct).ConfigureAwait(false);
        var tools = new ReClarifyClusterTools(input.Baseline, relations, run);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        return (tools.SavedClusters ?? [], tools.Saved, tools.CheckRounds, relations.HasData);
    }
}

// MAKER: gruppiert Requirements semantisch zu Feature-Clustern (Bedeutung, nicht Wortabgleich).
[SendsMessage(typeof(ClusterDraft))]
internal sealed class ClusterAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    string repoRoot,
    RunContext run)
    : Executor<ReClarifyClusterInput>("L4ReClarifyClusterAgent")
{
    private const string Task = """
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

    public override async ValueTask HandleAsync(ReClarifyClusterInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var (clusters, saved, rounds, hasRelations) = await ClusterAgentRunner.RunAsync(input, agentFactory, repoRoot, run, Task, ct).ConfigureAwait(false);
        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_AGENT_DONE",
            runId = run.RunId,
            saved,
            clusters = clusters.Count,
            toolCheckRounds = rounds,
            relations = hasRelations,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterDraft(input, clusters, saved, rounds, Attempt: 1, Source: "maker", History: [])).ConfigureAwait(false);
    }
}

// CHECKER 1: deterministisches Coverage-Gate (Recall-Garantie) — entscheidet typisiert (R-33 S2).
[SendsMessage(typeof(ClusterVerdict))]
internal sealed class ClusterGateExecutor(RunContext run) : Executor<ClusterDraft>("L4ReClarifyClusterGate")
{
    public override async ValueTask HandleAsync(ClusterDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ReClarifyClusterGate.Check(draft.Input.Baseline, draft.Clusters);
        var decision = ReClarifyClusterGate.Decide(report, draft.Attempt, draft.Input.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.SubjectId is null ? "" : " " + e.SubjectId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_GATE",
            runId = run.RunId,
            attempt = draft.Attempt,
            source = draft.Source,
            pass = report.Pass,
            decision = decision.ToString(),
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ClusterVerdict(draft.Input, draft.Clusters, report, decision, draft.Saved, draft.ToolCheckRounds, draft.Attempt,
            draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

// R-33 S2: REPAIR — die Gate-Fehler gehen WOERTLICH als Feedback an den Cluster-Agenten (Attempt+1),
// dann zurueck zum Gate (Loop-back). Kein Write, bounded durch MaxAttempts (GateLoop.Decide).
[SendsMessage(typeof(ClusterDraft))]
internal sealed class ClusterRepairExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    string repoRoot,
    RunContext run)
    : Executor<ClusterVerdict>("L4ReClarifyClusterRepair")
{
    public override async ValueTask HandleAsync(ClusterVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var task = BuildRepairTask(v.Report);
        var (clusters, saved, rounds, _) = await ClusterAgentRunner.RunAsync(v.Input, agentFactory, repoRoot, run, task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "RE_CLARIFY_CLUSTER_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, clusters = clusters.Count, saved, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ClusterDraft(v.Input, clusters, saved, rounds, Attempt: v.Attempt + 1, Source: "repair", History: v.History)).ConfigureAwait(false);
    }

    // intern testbar: die Gate-Fehler muessen den Agenten WOERTLICH erreichen (GateFeedback-Vertrag).
    internal static string BuildRepairTask(ReClarifyGateReport report)
    {
        var errors = string.Join("\n", report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        return $"""
                Deine vorherige Cluster-Gruppierung hat das deterministische Gate NICHT bestanden. Fehler:
                {errors}

                Korrigiere die Gruppierung vollstaendig neu:
                - Jedes aktive Requirement ist in GENAU EINEM Cluster core (keines fehlt, keines doppelt).
                - Nur existierende requirementIds verwenden; kein Cluster ohne coreRequirementIds.
                - crossCutting nur fuer Regeln, die Kern eines ANDEREN Clusters sind.

                Pruefe mit check_clusters (Coverage muss pass sein) und speichere genau einmal mit save_clusters.
                """;
    }
}

// CHECKER 2: zweiter Agent kritisiert die Cluster fachlich (Feedback-Signal). R-33 S2: laeuft NUR bei
// bestandenem Gate — auf strukturell kaputtem Input ist fachliche Kritik wertlos (kein Agent-Call);
// der Skip ist im Report ehrlich markiert (Muster: Pass-Kante von R-26-C/PbiAlign, hier wegen des
// Typ-Wechsels Verdict->Reviewed als Executor-Skip statt Kante).
[SendsMessage(typeof(ClusterReviewed))]
internal sealed class ClusterReviewExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<ClusterVerdict>("L4ReClarifyClusterReviewAgent")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(ClusterVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        if (verdict.Decision != GateDecision.Pass)
        {
            var skipped = new ClusterReviewReport(
                SchemaVersion: ClusterReviewReport.CurrentSchemaVersion,
                ReviewId: $"cluster-review-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Verdict: "skipped",
                Summary: $"Deterministisches Gate nicht bestanden ({verdict.Decision}) - fachliches Review uebersprungen, kein Agent-Call.",
                Findings: []);
            run.AppendEvent(new { type = "RE_CLARIFY_CLUSTER_REVIEW_SKIPPED", runId = run.RunId, decision = verdict.Decision.ToString(), gateErrors = verdict.Report.Errors.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(Reviewed(verdict, skipped)).ConfigureAwait(false);
            return;
        }

        var tools = new ReClarifyClusterReviewTools(verdict.Input.Baseline, run);
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
        await context.SendMessageAsync(Reviewed(verdict, review)).ConfigureAwait(false);
    }

    private static ClusterReviewed Reviewed(ClusterVerdict v, ClusterReviewReport review)
        => new(v.Input, v.Clusters, v.Report, review, v.Decision, v.Saved, v.ToolCheckRounds, v.Attempt, v.History);
}

[YieldsOutput(typeof(ClusterResult))]
[SendsMessage(typeof(ClusterResult))] // pipeline-full (B3): Ergebnis fließt zusätzlich als Message weiter (Gate-Komposition); im CLI-Graph ohne Kante wirkungslos
internal sealed class ClusterFinalizeExecutor(RunContext run, string outDir) : Executor<ClusterReviewed>("L4ReClarifyClusterFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(ClusterReviewed reviewed, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        var set = new FeatureClusterSet(
            SchemaVersion: FeatureClusterSet.CurrentSchemaVersion,
            ClusterSetId: $"cluster-set-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: reviewed.Input.Baseline.ProjectId,
            BaselineId: reviewed.Input.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceBaselinePath: reviewed.Input.SourceBaselinePath,
            Clusters: reviewed.Clusters);

        await File.WriteAllTextAsync(Path.Combine(outDir, "feature-clusters.json"), JsonSerializer.Serialize(set, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-gate-report.json"), JsonSerializer.Serialize(reviewed.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-review.json"), JsonSerializer.Serialize(reviewed.Review, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "cluster-attempts.json"), JsonSerializer.Serialize(reviewed.History, Json), ct).ConfigureAwait(false);
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
            attempts = reviewed.Attempt,
            finalDecision = reviewed.Decision.ToString(),
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "RE_CLARIFY_CLUSTER_DONE",
            runId = run.RunId,
            gatePass = reviewed.Report.Pass,
            finalDecision = reviewed.Decision.ToString(),
            attempts = reviewed.Attempt,
            reviewVerdict = reviewed.Review.Verdict,
            clusters = reviewed.Clusters.Count,
            timestampUtc = DateTime.UtcNow
        });
        var result = new ClusterResult(set, reviewed.Report, reviewed.Review);
        await context.YieldOutputAsync(result).ConfigureAwait(false);
        await context.SendMessageAsync(result).ConfigureAwait(false);
    }
}

internal static class ReClarifyClusterWorkflow
{
    public const string WorkflowName = "L4-ReClarify-Cluster";

    // R-33 S2 (§4b): DIE eine Kanten-Quelle fuer CLI-Graph UND pipeline-full-Ein-Graph — keine Kopie.
    // Review bleibt im Fluss (Typ-Wechsel Verdict->Reviewed), ueberspringt aber selbst bei Nicht-Pass.
    public static void AddTo(WorkflowBuilder b,
        ClusterAgentExecutor agent, ClusterGateExecutor gate, ClusterRepairExecutor repair,
        ClusterReviewExecutor review, ClusterFinalizeExecutor finalize)
    {
        b.AddEdge(agent, gate);
        b.AddEdge<ClusterVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<ClusterVerdict>(gate, review, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(review, finalize);
    }

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        ClusterAgentExecutor agent,
        ClusterGateExecutor gate,
        ClusterRepairExecutor repair,
        ClusterReviewExecutor review,
        ClusterFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("CanonicalRequirements -> ClusterAgent[Tools] (maker) -> Gate[det Coverage] --[Repair]--> Repair (Loop) "
                           + "-> ReviewAgent[Tools] (checker, nur bei Pass) -> Finalize. Feature-Cluster als Analyse-Granularitaet.");
        AddTo(builder, agent, gate, repair, review, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
