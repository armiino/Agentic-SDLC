using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// S1 (Worklist 20.07): github-forward als EIN MAF-Lauf MIT MAF-nativem Human-Gate (RequestPort) statt dem
// CLI-Split -review/-apply. Wiederverwendet Seed/Maker/Gate/Repair (unveraendert). Graph:
//
//   Seed -> Maker -> Gate --repair--> Repair --(Loop)--> Gate
//                     ├── Decision==Pass ─────────────> Finalize -> [RequestPort Human] -> Apply -> (Ende)
//                     └── Decision==Human/MaxAttempts ─> Finalize -(yield "needs manual")-> (Ende, KEIN Apply)
//
// Invarianten (Decision 20.07 §3): Plan = Disk-Artefakt (Evidenz); Core wird im Apply FRISCH geladen, NICHT im
// Checkpoint; nur die Review-Entscheidung fliesst durch den Port. Der Checkpoint (FileSystemJsonCheckpointStore,
// im Runner) haelt nur Prozess-State + den offenen Request. Pause am Human-Gate ist prozessuebergreifend resumebar.

// Request an den Menschen: Plan-Zusammenfassung zur Freigabe (Anzeige). Response: akzeptierte Ops + execute-Flag.
public sealed record ForwardReviewRequest(string RunId, string SourcePbiUpdateRun, IReadOnlyList<ForwardReviewOpView> Ops);
public sealed record ForwardReviewOpView(string OpId, string Kind, string PbiId, int? TargetIssueNumber, string? Title, string? Anchor, string Rationale);
// R-60: OverwriteOpIds = die am Gate BEWUSST zum Überschreiben freigegebenen FLAG_DRIFT-Ops (leer im Normalfall;
// accept-all/Policy-Modi setzen sie NIE — nur der explizite Autor-Entscheid 'overwrite').
public sealed record ForwardReviewResponse(IReadOnlyList<string> AcceptedOpIds, bool Execute, string Reviewer,
    IReadOnlyList<string>? OverwriteOpIds = null);

// FINALIZE (HITL): schreibt Plan/Gate/Attempts/Summary (Evidenz, wie der klassische Finalize) und verzweigt dann:
//   Decision==Pass -> ForwardReviewRequest an den Port (Human-Gate).  Sonst -> terminaler "needs manual"-Output.
[SendsMessage(typeof(ForwardReviewRequest))]
[SendsMessage(typeof(GithubForwardGateEmpty))]
[YieldsOutput(typeof(GithubForwardWfResult))]
internal sealed class GithubForwardHitlFinalizeExecutor(RunContext run) : Executor<GithubForwardVerdict>("GithubForwardHitlFinalize")
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

        if (v.Decision == GateDecision.Pass)
        {
            var views = ops.Select((o, i) => new ForwardReviewOpView($"op-{i}", o.Kind, o.PbiId, o.TargetIssueNumber, o.Title, o.Anchor, o.Rationale)).ToList();
            var request = new ForwardReviewRequest(run.RunId, ctx.SourcePbiUpdateRun, views);
            // R-50: TYP-Routing — 0 Ops ⇒ Marker statt Request (s. Pbi-Strip; Prädikat auf Port-Kanten wird ignoriert).
            if (views.Count == 0)
            {
                await context.SendMessageAsync(new GithubForwardGateEmpty(request)).ConfigureAwait(false);
                return;
            }
            run.AppendEvent(new { type = "GITHUB_FWD_HUMAN_GATE", runId = run.RunId, operations = ops.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(request).ConfigureAwait(false);
        }
        else
        {
            // Gate nicht bestanden (Human/MaxAttempts): kein Apply — dokumentierter Abbruch, Mensch loest manuell.
            run.AppendEvent(new { type = "GITHUB_FWD_DONE", runId = run.RunId, gatePass = v.Report.Pass, finalDecision = v.Decision.ToString(), attempts = v.Attempt, operations = ops.Count, applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(new GithubForwardWfResult(v.Plan, v.Report, v.Decision)).ConfigureAwait(false);
        }
    }
}

// APPLY (HITL): konsumiert die menschliche Response. Laedt den Plan FRISCH von Platte (outDir) und Core FRISCH (im
// geteilten GithubForwardApply.ExecuteAsync) -> identisch zum CLI-Apply. Der einzige Punkt mit echtem Write.
[YieldsOutput(typeof(GithubForwardApplyReport))]
internal sealed class GithubForwardApplyExecutor(RunContext run, string repoRoot, string outDir, string? repository, string? tokenEnv)
    : Executor<ForwardReviewResponse>("GithubForwardApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public override async ValueTask HandleAsync(ForwardReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var planPath = Path.Combine(outDir, "github-forward-plan.json");
        var plan = JsonSerializer.Deserialize<GithubForwardPlanDocument>(await File.ReadAllTextAsync(planPath, ct).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"Plan nicht lesbar: {planPath}");
        var accepted = resp.AcceptedOpIds.ToHashSet(StringComparer.Ordinal);
        var repo = repository ?? plan.Repository;

        run.AppendEvent(new { type = "GITHUB_FWD_APPLY_START", runId = run.RunId, accepted = accepted.Count, execute = resp.Execute, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
        var report = await GithubForwardApply.ExecuteAsync(outDir, plan, accepted, resp.Execute, repoRoot, repo, tokenEnv, ct, resp.OverwriteOpIds).ConfigureAwait(false);
        run.AppendEvent(new { type = "GITHUB_FWD_DONE", runId = run.RunId, applied = true, executed = report.Executed, success = report.Success, accepted = report.Summary.Accepted, linked = report.Summary.Linked, created = report.Summary.Created, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(report, ct).ConfigureAwait(false);
    }
}

internal static class GithubForwardHitlWorkflow
{
    public const string WorkflowName = "GitHub-Forward-Reconciliation-HITL";

    public static Workflow Build(
        GithubForwardSeedExecutor seed, GithubForwardMakerExecutor maker, GithubForwardGateExecutor gate,
        GithubForwardRepairExecutor repair, GithubForwardHitlFinalizeExecutor finalize,
        RequestPort humanGate, GithubForwardApplyExecutor apply,
        GithubForwardEmptyGateResponder emptyGate)
    {
        var b = new WorkflowBuilder(seed)
            .WithName(WorkflowName)
            .WithDescription("Delta -> Seed -> Maker -> Gate -> [Repair] -> Finalize -> [RequestPort Human] -> Apply.");
        AddTo(b, seed, maker, gate, repair, finalize, humanGate, apply, emptyGate);
        return b.Build();
    }

    // U2 (Ein-Graph): DIESELBE Kanten-Verdrahtung fuer den Standalone-Graph UND den Ein-Graph — eine Quelle.
    public static void AddTo(WorkflowBuilder b,
        GithubForwardSeedExecutor seed, GithubForwardMakerExecutor maker, GithubForwardGateExecutor gate,
        GithubForwardRepairExecutor repair, GithubForwardHitlFinalizeExecutor finalize,
        RequestPort humanGate, GithubForwardApplyExecutor apply,
        // R-50: der Leer-Gate-Durchleiter — Ops==0 wird per conditional edge hierher geroutet (LAUT), nie zum Port.
        GithubForwardEmptyGateResponder emptyGate)
    {
        b.AddEdge(seed, maker);
        b.AddEdge(maker, gate);
        b.AddEdge<GithubForwardVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<GithubForwardVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        // R-50: TYP-Routing statt Port-Pause bei 0 Ops (s. Pbi-Strip / shared/EmptyGateAutoResponder).
        b.AddEdge(finalize, humanGate);   // ForwardReviewRequest (Ops > 0)
        b.AddEdge(finalize, emptyGate);   // GithubForwardGateEmpty (Ops == 0) -> LAUTER Skip
        b.AddEdge(emptyGate, apply);      // leere Antwort -> NORMALER Apply-Pfad (bewährt, accepted=0)
        b.AddEdge(humanGate, apply);      // ForwardReviewResponse
        b.WithOutputFrom(finalize);       // terminaler "needs manual"-Output
        b.WithOutputFrom(apply);          // terminaler Apply-Report
    }
}
