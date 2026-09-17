using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal sealed record ReClarifyBacklogInput(FeatureClusterSet Clusters, CanonicalRequirementsBaseline Baseline, string SourceClustersPath, int MaxAttempts);

// R-33 S1: Attempt/Source/History reisen in den Messages (MAF-superstep, Graph bleibt stateless) —
// dieselbe Form wie PbiUpdateDraft/Verdict. Der Verdict traegt das typisierte GateDecision fuer die Kanten.
internal sealed record BacklogDraft(ReClarifyBacklogInput Input, ProductBacklogDocument Backlog, bool Saved, int ToolCheckRounds, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record BacklogVerdict(ReClarifyBacklogInput Input, ProductBacklogDocument Backlog, ReClarifyGateReport Report, GateDecision Decision, bool Saved, int ToolCheckRounds, int Attempt, IReadOnlyList<GateAttempt> History);
internal sealed record BacklogResult(ProductBacklogDocument Backlog, ReClarifyGateReport Report);

// Geteilter Agent-Aufruf von Maker UND Repair (eine Quelle): Tools bauen, Agent laufen lassen, Dokument
// aus den gespeicherten Items formen. Kein Zustand ausserhalb der Tools-Instanz.
internal static class ClarifyAgentRunner
{
    public static async Task<(ProductBacklogDocument Doc, bool Saved, int CheckRounds)> RunAsync(
        ReClarifyBacklogInput input, Func<IReadOnlyList<AITool>, AIAgent> factory, RunContext run, string task, CancellationToken ct)
    {
        var tools = new ReClarifyBacklogTools(input.Clusters, input.Baseline, run);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var doc = new ProductBacklogDocument(
            ProductBacklogDocument.CurrentSchemaVersion, $"product-backlog-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            input.Baseline.ProjectId, input.Baseline.BaselineId, DateTime.UtcNow, input.SourceClustersPath,
            tools.SavedItems ?? []);
        return (doc, tools.Saved, tools.CheckRounds);
    }
}

// CLARIFY/CUT: pro Feature-Cluster interrogieren + in PBIs schneiden (Akzeptanzkriterien + offene Entscheidungen).
[SendsMessage(typeof(BacklogDraft))]
internal sealed class ClarifyAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<ReClarifyBacklogInput>("L4ReClarifyBacklogAgent")
{
    private const string Task = """
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

    public override async ValueTask HandleAsync(ReClarifyBacklogInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var (doc, saved, rounds) = await ClarifyAgentRunner.RunAsync(input, agentFactory, run, Task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_AGENT_DONE", runId = run.RunId, saved, pbis = doc.Items.Count, toolCheckRounds = rounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new BacklogDraft(input, doc, saved, rounds, Attempt: 1, Source: "maker", History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(BacklogVerdict))]
internal sealed class BacklogGateExecutor(RunContext run) : Executor<BacklogDraft>("L4ReClarifyBacklogGate")
{
    public override async ValueTask HandleAsync(BacklogDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ReClarifyBacklogGate.Check(draft.Input.Clusters, draft.Input.Baseline, draft.Backlog);
        var decision = ReClarifyBacklogGate.Decide(report, draft.Attempt, draft.Input.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.SubjectId is null ? "" : " " + e.SubjectId)}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = report.Pass, decision = decision.ToString(), errors = report.Errors.Count, warnings = report.Warnings.Count, saved = draft.Saved, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new BacklogVerdict(draft.Input, draft.Backlog, report, decision, draft.Saved, draft.ToolCheckRounds, draft.Attempt,
            draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

// R-33 S1: REPAIR — die Gate-Fehler gehen WOERTLICH als Feedback an den Clarify-Agenten (Attempt+1),
// dann zurueck zum Gate (Loop-back). Kein Write, bounded durch MaxAttempts (GateLoop.Decide).
[SendsMessage(typeof(BacklogDraft))]
internal sealed class BacklogRepairExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<BacklogVerdict>("L4ReClarifyBacklogRepair")
{
    public override async ValueTask HandleAsync(BacklogVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var task = BuildRepairTask(v.Report);
        var (doc, saved, rounds) = await ClarifyAgentRunner.RunAsync(v.Input, agentFactory, run, task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, pbis = doc.Items.Count, saved, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new BacklogDraft(v.Input, doc, saved, rounds, Attempt: v.Attempt + 1, Source: "repair", History: v.History)).ConfigureAwait(false);
    }

    // intern testbar: die Gate-Fehler muessen den Agenten WOERTLICH erreichen (GateFeedback-Vertrag).
    internal static string BuildRepairTask(ReClarifyGateReport report)
    {
        var errors = string.Join("\n", report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        return $"""
                Dein vorheriger Backlog-Schnitt hat das deterministische Gate NICHT bestanden. Fehler:
                {errors}

                Korrigiere den Schnitt vollstaendig neu:
                - Jedes core-Requirement jedes Clusters muss in mindestens einem PBI vorkommen (keine Coverage-Luecke).
                - Jedes PBI hat entweder testbare acceptanceCriteria ODER explizite openDecisions (keine stille Luecke).
                - Nur existierende requirementIds verwenden; pbiId eindeutig; title gesetzt.

                Pruefe mit check_pbis (Gate muss pass sein) und speichere genau einmal mit save_pbis.
                """;
    }
}

[YieldsOutput(typeof(BacklogResult))]
[SendsMessage(typeof(BacklogResult))] // pipeline-full (B4): Ergebnis fließt zusätzlich als Message weiter (Gate-Komposition); im CLI-Graph ohne Kante wirkungslos
internal sealed class BacklogFinalizeExecutor(RunContext run, string outDir) : Executor<BacklogVerdict>("L4ReClarifyBacklogFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(BacklogVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "product-backlog.json"), JsonSerializer.Serialize(verdict.Backlog, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "backlog-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "backlog-attempts.json"), JsonSerializer.Serialize(verdict.History, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "backlog-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            gatePass = verdict.Report.Pass,
            gateErrors = verdict.Report.Errors.Count,
            gateWarnings = verdict.Report.Warnings.Count,
            pbis = verdict.Backlog.Items.Count,
            attempts = verdict.Attempt,
            finalDecision = verdict.Decision.ToString(),
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_DONE", runId = run.RunId, gatePass = verdict.Report.Pass, finalDecision = verdict.Decision.ToString(), attempts = verdict.Attempt, pbis = verdict.Backlog.Items.Count, timestampUtc = DateTime.UtcNow });
        var result = new BacklogResult(verdict.Backlog, verdict.Report);
        await context.YieldOutputAsync(result).ConfigureAwait(false);
        await context.SendMessageAsync(result).ConfigureAwait(false);
    }
}

internal static class ReClarifyBacklogWorkflow
{
    public const string WorkflowName = "L4-ReClarify-Backlog";

    // R-33 S1 (§4b): DIE eine Kanten-Quelle fuer CLI-Graph UND pipeline-full-Ein-Graph — keine Kopie.
    // Form = kanonischer Loop (PbiUpdateWorkflow): Gate entscheidet typisiert, Repair schleift zum Gate zurueck.
    public static void AddTo(WorkflowBuilder b,
        ClarifyAgentExecutor agent, BacklogGateExecutor gate, BacklogRepairExecutor repair, BacklogFinalizeExecutor finalize)
    {
        b.AddEdge(agent, gate);
        b.AddEdge<BacklogVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<BacklogVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
    }

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        ClarifyAgentExecutor agent,
        BacklogGateExecutor gate,
        BacklogRepairExecutor repair,
        BacklogFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(agent)
            .WithName(WorkflowName)
            .WithDescription("FeatureClusters -> ClarifyAgent[Tools] (interrogate + cut) -> BacklogGate[det DoR] --[Repair]--> Repair (Loop) / sonst Finalize. "
                           + "Erzeugt Product Backlog Items mit Akzeptanzkriterien + expliziten offenen Entscheidungen.");
        AddTo(builder, agent, gate, repair, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
