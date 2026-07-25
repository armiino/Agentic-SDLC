using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.3 als MAF-Workflow MIT gate-getriebenem Repair-Loop (R1-R4, Pattern des CheckerRepairWorkflow):
//
//   Seed -> Maker -> Gate --pass/human/max--> Finalize
//                     └----repairable&&attempt<max----> Repair --(Loop-Back)--> Gate
//
// DoD = Gate pass, sonst dokumentierter Abbruch (Finalize schreibt Plan + attemptHistory + finalDecision). Der
// Repair ist ein Maker-Retry mit GateFeedback (kein Write). Bounded (maxAttempts) -> garantierte Terminierung.

internal sealed record GithubForwardWfContext(
    ProjectStateDocument Core,
    IReadOnlyList<GithubSyncEntry> Entries,
    IReadOnlyDictionary<string, GithubMappingRecord> MappingByPbi,
    IReadOnlyList<GithubIssueSnapshot> Issues,
    string? Repository,
    string SourcePbiUpdateRun,
    string OutDir,
    string? SnapshotRel,
    bool DryRun,
    int MaxAttempts,
    bool HoldUnclearNewPbis = false);

internal sealed record GithubForwardSeeded(GithubForwardWfContext Ctx, IReadOnlyList<GithubForwardOp> DeterministicOps, IReadOnlyList<GithubSyncEntry> Unmapped);
internal sealed record GithubForwardDraft(
    GithubForwardWfContext Ctx, IReadOnlyList<GithubForwardOp> DeterministicOps, IReadOnlyList<GithubSyncEntry> Unmapped,
    IReadOnlyList<GithubForwardOp> AgentOps, int Attempt, string Source, IReadOnlyList<GateAttempt> History);
internal sealed record GithubForwardVerdict(
    GithubForwardWfContext Ctx, IReadOnlyList<GithubForwardOp> DeterministicOps, IReadOnlyList<GithubSyncEntry> Unmapped,
    IReadOnlyList<GithubForwardOp> AgentOps, GithubForwardPlanDocument Plan, GithubForwardGateReport Report,
    GateDecision Decision, int Attempt, IReadOnlyList<GateAttempt> History);
internal sealed record GithubForwardWfResult(GithubForwardPlanDocument Plan, GithubForwardGateReport Gate, GateDecision FinalDecision);

// SEED: deterministischer Vorfilter.
[SendsMessage(typeof(GithubForwardSeeded))]
internal sealed class GithubForwardSeedExecutor(RunContext run) : Executor<GithubForwardWfContext>("GithubForwardSeed")
{
    public override async ValueTask HandleAsync(GithubForwardWfContext ctx, IWorkflowContext context, CancellationToken ct = default)
    {
        var seed = GithubForwardSeed.Seed(ctx.Entries, ctx.MappingByPbi, ctx.Issues, ctx.HoldUnclearNewPbis);
        run.AppendEvent(new { type = "GITHUB_FWD_SEED", runId = run.RunId, deterministic = seed.DeterministicOps.Count, unmapped = seed.UnmappedPbis.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubForwardSeeded(ctx, seed.DeterministicOps, seed.UnmappedPbis)).ConfigureAwait(false);
    }
}

// MAKER: je unmapped PBI Suche -> LINK/CREATE (agentisch). Erster Draft (attempt=1).
[SendsMessage(typeof(GithubForwardDraft))]
internal sealed class GithubForwardMakerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<GithubForwardSeeded>("GithubForwardMaker")
{
    private const string Task = """
                                Fuer JEDES PBI aus get_unmapped_pbis GENAU EIN Op:
                                1. get_pbi(pbiId) lesen (Titel + abgedeckte Requirement-Texte).
                                2. search_issues MIT diesen Begriffen ausfuehren (Anti-Duplikat, Pflicht vor CREATE).
                                3. Plausibler Kandidat? -> LINK (targetIssueNumber + anchor). Sonst -> CREATE_ISSUE mit
                                   title, body, searchedQueries und searchEvidence. OHNE diese Felder lehnt das Gate ab.
                                check_forward_plan zur Selbstpruefung, dann save_forward_plan GENAU EINMAL.
                                """;

    public override async ValueTask HandleAsync(GithubForwardSeeded seeded, IWorkflowContext context, CancellationToken ct = default)
    {
        var agentOps = await GithubForwardAgentRunner.RunAsync(seeded.Ctx, seeded.Unmapped, agentFactory, run, Task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "GITHUB_FWD_MAKER", runId = run.RunId, agentOps = agentOps.Count, dryRun = seeded.Ctx.DryRun, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubForwardDraft(seeded.Ctx, seeded.DeterministicOps, seeded.Unmapped, agentOps, Attempt: 1, Source: "maker", History: [])).ConfigureAwait(false);
    }
}

// GATE: deterministisch (Rev-3, Coverage) + Entscheidung (pass/repair/human/max) + Attempt-Historie.
[SendsMessage(typeof(GithubForwardVerdict))]
internal sealed class GithubForwardGateExecutor(RunContext run) : Executor<GithubForwardDraft>("GithubForwardGate")
{
    public override async ValueTask HandleAsync(GithubForwardDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var ctx = draft.Ctx;
        var ops = draft.DeterministicOps.Concat(draft.AgentOps).ToList();
        var plan = new GithubForwardPlanDocument(
            GithubForwardPlanDocument.CurrentSchemaVersion, $"github-forward-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            DateTime.UtcNow, ctx.SourcePbiUpdateRun, ctx.Repository, ops);
        var gate = GithubForwardGate.Check(plan, ctx.Entries, ctx.Issues);
        var decision = GithubForwardGate.Decide(gate, draft.Attempt, ctx.MaxAttempts);

        var attempt = new GateAttempt(draft.Attempt, draft.Source, gate.Pass, decision.ToString(),
            gate.Errors.Select(e => $"{e.Code}({e.Repairability}){(e.PbiId is null ? "" : " " + e.PbiId)}").ToList(), DateTime.UtcNow);
        var history = draft.History.Append(attempt).ToList();

        run.AppendEvent(new { type = "GITHUB_FWD_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = gate.Pass, decision = decision.ToString(), errors = gate.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubForwardVerdict(ctx, draft.DeterministicOps, draft.Unmapped, draft.AgentOps, plan, gate, decision, draft.Attempt, history)).ConfigureAwait(false);
    }
}

// REPAIR: Maker-Retry mit GateFeedback. Nur ueber die konditionale Kante (Decision==Repair) erreicht. Kein Write.
[SendsMessage(typeof(GithubForwardDraft))]
internal sealed class GithubForwardRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<GithubForwardVerdict>("GithubForwardRepair")
{
    public override async ValueTask HandleAsync(GithubForwardVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    Dein vorheriger Forward-Plan hat das Gate NICHT bestanden. Fehler:
                    {errors}

                    Korrigiere gezielt: fuer jedes betroffene PBI erneut search_issues ausfuehren; CREATE_ISSUE NUR mit
                    searchedQueries + searchEvidence; jedes PBI aus get_unmapped_pbis bekommt GENAU EIN Op (LINK oder
                    CREATE). check_forward_plan zur Selbstpruefung, dann save_forward_plan GENAU EINMAL.
                    """;
        var agentOps = await GithubForwardAgentRunner.RunAsync(v.Ctx, v.Unmapped, agentFactory, run, task, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "GITHUB_FWD_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, agentOps = agentOps.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GithubForwardDraft(v.Ctx, v.DeterministicOps, v.Unmapped, agentOps, Attempt: v.Attempt + 1, Source: "repair", History: v.History)).ConfigureAwait(false);
    }
}

// FINALIZE: schreibt Plan + Gate-Report + Summary (inkl. attemptHistory + finalDecision).
[YieldsOutput(typeof(GithubForwardWfResult))]
internal sealed class GithubForwardFinalizeExecutor(RunContext run) : Executor<GithubForwardVerdict>("GithubForwardFinalize")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(GithubForwardVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var ctx = v.Ctx;
        Directory.CreateDirectory(ctx.OutDir);
        var ops = v.Plan.Operations;
        var deterministic = ops.Count(o => string.Equals(o.Origin, "deterministic", StringComparison.Ordinal));
        var agentOps = ops.Count(o => string.Equals(o.Origin, "agent", StringComparison.Ordinal));

        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "github-forward-plan.json"), JsonSerializer.Serialize(v.Plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "github-forward-gate-report.json"), JsonSerializer.Serialize(v.Report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "github-forward-attempts.json"), JsonSerializer.Serialize(v.History, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(ctx.OutDir, "github-forward-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            sourcePbiUpdateRun = ctx.SourcePbiUpdateRun,
            deltaPbis = ctx.Entries.Count,
            deterministic,
            unmapped = v.Unmapped.Count,
            agentOps,
            operations = ops.Count,
            snapshot = ctx.SnapshotRel,
            gatePass = v.Report.Pass,
            gateErrors = v.Report.Errors.Count,
            attempts = v.Attempt,
            finalDecision = v.Decision.ToString(),
            byKind = ops.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            dryRun = ctx.DryRun,
            timestampUtc = DateTime.UtcNow
        }, Json), ct).ConfigureAwait(false);

        run.AppendEvent(new { type = "GITHUB_FWD_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, operations = ops.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new GithubForwardWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
    }
}

// Geteilte Maker-Logik fuer Maker- UND Repair-Knoten (dieselben Tools, unterschiedlicher Task). Dry-run/kein-unmapped
// -> keine Agent-Ops (der Loop laeuft dann bis MaxAttempts -> HumanReview, deterministisch belegbar).
internal static class GithubForwardAgentRunner
{
    public static async Task<List<GithubForwardOp>> RunAsync(
        GithubForwardWfContext ctx, IReadOnlyList<GithubSyncEntry> unmapped,
        Func<IReadOnlyList<AITool>, AIAgent> factory, RunContext run, string task, CancellationToken ct)
    {
        if (unmapped.Count == 0 || ctx.DryRun) return [];
        var readTools = new GithubReadTools(ctx.Issues, ctx.Repository, run);
        var fwdTools = new GithubForwardTools(unmapped, ctx.Core, ctx.Issues, run);
        var agent = factory([.. readTools.Build(), .. fwdTools.Build()]);
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        return fwdTools.SavedOps.ToList();
    }
}

internal static class GithubForwardWorkflow
{
    public const string WorkflowName = "GitHub-Forward-Reconciliation";

    public static Workflow Build(
        GithubForwardSeedExecutor seed, GithubForwardMakerExecutor maker, GithubForwardGateExecutor gate,
        GithubForwardRepairExecutor repair, GithubForwardFinalizeExecutor finalize)
    {
        var b = new WorkflowBuilder(seed)
            .WithName(WorkflowName)
            .WithDescription("Delta -> Seed -> Maker -> Gate --[repairable&&attempt<max]--> Repair (Loop) / sonst Finalize.");
        b.AddEdge(seed, maker);
        b.AddEdge(maker, gate);
        // Konditionale Verzweigung am Gate (auf der Decision) + Loop-Back (Pattern: CheckerRepairWorkflow).
        b.AddEdge<GithubForwardVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<GithubForwardVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.WithOutputFrom(finalize);
        return b.Build();
    }
}
