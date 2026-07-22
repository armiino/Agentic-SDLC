using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed record ReClarifyBacklogInput(FeatureClusterSet Clusters, CanonicalRequirementsBaseline Baseline, string SourceClustersPath);

internal sealed record BacklogDraft(ReClarifyBacklogInput Input, ProductBacklogDocument Backlog, bool Saved, int ToolCheckRounds);
internal sealed record BacklogVerdict(ReClarifyBacklogInput Input, ProductBacklogDocument Backlog, ReClarifyGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record BacklogResult(ProductBacklogDocument Backlog, ReClarifyGateReport Report);

// CLARIFY/CUT: pro Feature-Cluster interrogieren + in PBIs schneiden (Akzeptanzkriterien + offene Entscheidungen).
[SendsMessage(typeof(BacklogDraft))]
internal sealed class ClarifyAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<ReClarifyBacklogInput>("L4ReClarifyBacklogAgent")
{
    public override async ValueTask HandleAsync(ReClarifyBacklogInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new ReClarifyBacklogTools(input.Clusters, input.Baseline, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Erzeuge aus den Feature-Clustern einen Product Backlog.

                   Gehe Cluster fuer Cluster vor: hole mit get_feature_context den vollen Kontext (core + crossCutting
                   mit Text). Verhoere jedes Feature entlang: Zweck/Scope, Verhalten/Daten, Regeln/Berechtigungen,
                   Fehler/Lifecycle, Abnahme. Schneide das Feature dann in 1..n wertorientierte PBIs (vertikale Slices).

                   Je PBI: statement (Als <Rolle> will ich ..., damit ...), scope (inScope/outOfScope), testbare
                   acceptanceCriteria, openDecisions (evidence x resolution, blocksScope, proposedResolution bei
                   engineering_default), mvp, priorityRank, dependencies, readiness, traceability (requirementIds).

                   Regeln: KEINE stille Luecke - jedes PBI hat entweder Akzeptanzkriterien ODER explizite openDecisions.
                   Erfinde keine Wahrheit - unbelegte Annahmen als openDecision (evidence=not_stated) markieren, nicht
                   als Fakt. Jedes core-Requirement eines Clusters muss in mindestens einem PBI vorkommen.

                   Pruefe mit check_pbis (Gate muss pass sein) und speichere genau einmal mit save_pbis.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var doc = tools.SavedItems is { } items
            ? new ProductBacklogDocument(ProductBacklogDocument.CurrentSchemaVersion, $"product-backlog-{DateTime.UtcNow:yyyyMMdd_HHmmss}", input.Baseline.ProjectId, input.Baseline.BaselineId, DateTime.UtcNow, input.SourceClustersPath, items)
            : new ProductBacklogDocument(ProductBacklogDocument.CurrentSchemaVersion, $"product-backlog-{DateTime.UtcNow:yyyyMMdd_HHmmss}", input.Baseline.ProjectId, input.Baseline.BaselineId, DateTime.UtcNow, input.SourceClustersPath, []);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_AGENT_DONE", runId = run.RunId, saved = tools.Saved, pbis = doc.Items.Count, toolCheckRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new BacklogDraft(input, doc, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(BacklogVerdict))]
internal sealed class BacklogGateExecutor(RunContext run) : Executor<BacklogDraft>("L4ReClarifyBacklogGate")
{
    public override async ValueTask HandleAsync(BacklogDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ReClarifyBacklogGate.Check(draft.Input.Clusters, draft.Input.Baseline, draft.Backlog);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_GATE", runId = run.RunId, pass = report.Pass, decision = report.Decision, errors = report.Errors.Count, warnings = report.Warnings.Count, saved = draft.Saved, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new BacklogVerdict(draft.Input, draft.Backlog, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(BacklogResult))]
internal sealed class BacklogFinalizeExecutor(RunContext run, string outDir) : Executor<BacklogVerdict>("L4ReClarifyBacklogFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(BacklogVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "product-backlog.json"), JsonSerializer.Serialize(verdict.Backlog, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "backlog-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "backlog-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            gatePass = verdict.Report.Pass,
            gateErrors = verdict.Report.Errors.Count,
            gateWarnings = verdict.Report.Warnings.Count,
            pbis = verdict.Backlog.Items.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_DONE", runId = run.RunId, gatePass = verdict.Report.Pass, pbis = verdict.Backlog.Items.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new BacklogResult(verdict.Backlog, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class ReClarifyBacklogWorkflow
{
    public const string WorkflowName = "L4-ReClarify-Backlog";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        ClarifyAgentExecutor agent,
        BacklogGateExecutor gate,
        BacklogFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("FeatureClusters -> ClarifyAgent[Tools] (interrogate + cut) -> BacklogGate[det DoR] -> Finalize. "
                           + "Erzeugt Product Backlog Items mit Akzeptanzkriterien + expliziten offenen Entscheidungen.");
        builder.AddEdge(agent, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
