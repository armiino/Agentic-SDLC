using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed record L4AdequacyDraft(L4CompletionInput Input, L4AdequacyReport Report, bool Saved);
internal sealed record L4CompletionDraft(L4CompletionInput Input, L4AdequacyReport Adequacy, L4CompletionProposalDocument Proposals, bool Saved, int ToolCheckRounds);
internal sealed record L4CompletionVerdict(L4CompletionInput Input, L4AdequacyReport Adequacy, L4CompletionProposalDocument Proposals, L4CompletionGateReport Report, bool Saved, int ToolCheckRounds);
internal sealed record L4CompletionResult(L4AdequacyReport Adequacy, L4CompletionProposalDocument Proposals, L4CompletionGateReport Report);

[SendsMessage(typeof(L4AdequacyDraft))]
internal sealed class L4AdequacyFeedbackAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<L4CompletionInput>("L4AdequacyFeedbackAgent")
{
    public override async ValueTask HandleAsync(L4CompletionInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new L4AdequacyTools(input, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Bewerte den aktuellen L4 Requirements-Stand als Requirements Engineer.

                   Erkunde das Requirements-Dokument, Requirements, Readiness und Provenance mit deinen Tools.
                   Beurteile kapitelweise, ob der Stand fuer einen Projektstart substanziell genug ist.
                   Speichere am Ende genau einmal ein Adequacy-Feedback mit save_adequacy_report.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var report = tools.SavedReport ?? new L4AdequacyReport(
            SchemaVersion: L4AdequacyReport.CurrentSchemaVersion,
            ReportId: $"l4-adequacy-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.Baseline.ProjectId,
            BaselineId: input.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceRequirementsDocumentPath: input.SourceRequirementsDocumentPath,
            Findings: [],
            OverallAssessment: "Agent did not save an adequacy report.");
        run.AppendEvent(new
        {
            type = "L4_ADEQUACY_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            findings = report.Findings.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L4AdequacyDraft(input, report, tools.Saved)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(L4CompletionDraft))]
internal sealed class L4CompletionAgentExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    RunContext run)
    : Executor<L4AdequacyDraft>("L4CompletionAgent")
{
    public override async ValueTask HandleAsync(L4AdequacyDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new L4CompletionTools(draft.Input, draft.Report, run);
        var agent = agentFactory(tools.Build());
        var task = """
                   Erzeuge L4-Completion-Vorschlaege auf Basis des Adequacy-Feedbacks.

                   Du darfst Open-World-Luecken sichtbar machen, aber nicht als Wahrheit ausgeben.
                   Nutze ADD_DISK_POINT fuer Dinge, die nie gesagt wurden, aber fuer Projektstart plausibel/noetig
                   als Diskussionspunkt sind. DISK muss requiresHumanDecision=true und evidenceState=not_stated
                   oder weakly_inferred tragen.

                   Pruefe deine Items mit check_completion_proposals und speichere genau einmal mit
                   save_completion_proposals.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        var proposals = tools.SavedDocument ?? new L4CompletionProposalDocument(
            SchemaVersion: L4CompletionProposalDocument.CurrentSchemaVersion,
            ProposalId: $"l4-completion-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: draft.Input.Baseline.ProjectId,
            BaselineId: draft.Input.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAdequacyReportId: draft.Report.ReportId,
            SourceRequirementsDocumentPath: draft.Input.SourceRequirementsDocumentPath,
            Items: []);
        run.AppendEvent(new
        {
            type = "L4_COMPLETION_AGENT_DONE",
            runId = run.RunId,
            saved = tools.Saved,
            items = proposals.Items.Count,
            toolCheckRounds = tools.CheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L4CompletionDraft(draft.Input, draft.Report, proposals, tools.Saved, tools.CheckRounds)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(L4CompletionVerdict))]
internal sealed class L4CompletionGateExecutor(RunContext run) : Executor<L4CompletionDraft>("L4CompletionGate")
{
    public override async ValueTask HandleAsync(L4CompletionDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = L4CompletionGate.Check(draft.Input, draft.Adequacy, draft.Proposals);
        run.AppendEvent(new
        {
            type = "L4_COMPLETION_GATE",
            runId = run.RunId,
            pass = report.Pass,
            decision = report.Decision,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            saved = draft.Saved,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L4CompletionVerdict(draft.Input, draft.Adequacy, draft.Proposals, report, draft.Saved, draft.ToolCheckRounds)).ConfigureAwait(false);
    }
}

[YieldsOutput(typeof(L4CompletionResult))]
internal sealed class L4CompletionFinalizeExecutor(RunContext run, string outDir) : Executor<L4CompletionVerdict>("L4CompletionFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(L4CompletionVerdict verdict, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "l4-adequacy-report.json"), JsonSerializer.Serialize(verdict.Adequacy, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "l4-completion-proposals.json"), JsonSerializer.Serialize(verdict.Proposals, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "l4-completion-gate-report.json"), JsonSerializer.Serialize(verdict.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "l4-completion-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            verdict.Saved,
            verdict.ToolCheckRounds,
            verdict.Report.Pass,
            verdict.Report.Decision,
            findings = verdict.Adequacy.Findings.Count,
            proposalItems = verdict.Proposals.Items.Count,
            errors = verdict.Report.Errors.Count,
            warnings = verdict.Report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "L4_COMPLETION_DONE",
            runId = run.RunId,
            pass = verdict.Report.Pass,
            findings = verdict.Adequacy.Findings.Count,
            proposals = verdict.Proposals.Items.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(new L4CompletionResult(verdict.Adequacy, verdict.Proposals, verdict.Report)).ConfigureAwait(false);
    }
}

internal static class L4CompletionWorkflow
{
    public const string WorkflowName = "L4-Completion";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        L4AdequacyFeedbackAgentExecutor adequacy,
        L4CompletionAgentExecutor completion,
        L4CompletionGateExecutor gate,
        L4CompletionFinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(adequacy)
            .WithName(WorkflowName)
            .WithDescription("RequirementsDocument -> L4AdequacyFeedbackAgent[Tools] -> L4CompletionAgent[Tools] -> Gate[det] -> Finalize. "
                           + "Open-World-Ergaenzungen werden nur als DISK/OpenDecision-Proposals markiert.");

        builder.AddEdge(adequacy, completion);
        builder.AddEdge(completion, gate);
        builder.AddEdge(gate, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
