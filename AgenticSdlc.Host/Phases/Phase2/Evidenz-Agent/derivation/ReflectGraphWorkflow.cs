using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

// ── EDGE-NATIVE Reflect-Form ────────────────────────────────────────────────────────────────────────────────────
// Dieselbe Semantik wie der node-interne Loop (HandleReflectAsync), aber als MAF-nativer Zyklus mit konditionalen
// Kanten — Spiegel von CheckerRepairWorkflow (verifizierte rc1-API): AddEdge<T>(src,tgt,Func<T,bool>) + Loop-Back +
// Iterationsschranke IN der Message. Alle inhaltliche Logik (Task-Texte, Gate, Snapshot, Diff, Reports) kommt aus
// ReflectPipeline → driftfrei mit der node-internen Form. Der Loop ist hier im Graphen SICHTBAR und der Gate ein
// eigener, wiederverwendbarer Knoten (vgl. iteration-note A-agentic-17 + maf-vs-custom).

internal enum ReflectDecision { Revise, Accept, GiveUp }

/// <summary>Akkumulator, der über die Loop-Back-Kante mitreist (im node-internen Loop waren das lokale Variablen).</summary>
internal sealed class ReflectTrace
{
    public int RoundsRun;
    public bool? FirstDraftGatePass;
    public double? FirstDraftR1;
    public List<object> GateHistory { get; } = [];
}

/// <summary>Producer/Revise → Gate: der (evtl. nicht gespeicherte) Entwurf dieser Runde + Tool-Zustand + Iteration.</summary>
internal sealed record ReflectDraft(
    SourceArtifactSet Sources, bool Saved, ArtifactDocument Doc, DerivationTools Tools,
    int Iteration, ArtifactDocument? PrevDoc, ReflectTrace Trace);

/// <summary>Gate → Revise/Finalize: das Verdikt (Decision) + alle Gate-Ausgaben für Re-Feed bzw. Reports.</summary>
internal sealed record ReflectVerdict(
    SourceArtifactSet Sources, ArtifactDocument Doc, DerivationTools Tools, bool Saved,
    IReadOnlyList<string> Fails, IReadOnlyDictionary<string, List<string>> FlaggedByItem,
    IReadOnlyList<InferenceVerdict> Verdicts, IReadOnlyList<InvalidAnchor> Invalid, CoverageReport? Coverage,
    double? R1, ReflectDecision Decision, int Iteration, ReflectTrace Trace);

/// <summary>Geteilte Abhängigkeiten aller vier Reflect-Graph-Knoten (was der node-interne Executor als Felder hielt).</summary>
internal sealed record ReflectGraphDeps(
    Func<IReadOnlyList<AITool>, AIAgent> AgentFactory, InferenceChecker Checker, InferenceChecker PostHocChecker,
    DerivationSpec Spec, string Model, RunContext Run, string OutDir,
    IReadOnlyDictionary<string, LedgerClaim> LedgerClaims, int MaxRetries, string PostHocJudgeModel, bool IndependentPostHoc)
{
    public const string ModeLabel = "explore-account-verify-reflect-graph";
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public static readonly JsonSerializerOptions Read = new(JsonSerializerDefaults.Web);

    public ArtifactDocument EmptyDoc() => new(Spec.ItemIdPrefix, Spec.TargetArtifactType, 1,
        ArtifactDocument.StageDerivation, new ProducerMetadata(Run.RunId, Model, Spec.AgenticAccountVerifyPromptName), []);

    public DerivationTools NewTools(SourceArtifactSet sources) => new(sources, Spec, Run, OutDir, Model, LedgerClaims, Checker);

    /// <summary>Agent laufen lassen und das (evtl. fehlende) derived.json zurücklesen — die gemeinsame Runden-Mechanik
    /// von Producer und Revise.</summary>
    public async Task<(bool Saved, ArtifactDocument Doc, DerivationTools Tools)> RunRoundAsync(
        SourceArtifactSet sources, string taskText, int round, CancellationToken ct)
    {
        var tools = NewTools(sources);
        var agent = AgentFactory(tools.BuildAccountVerify());
        Run.AppendEvent(new { type = "REFLECT_ROUND_START", runId = Run.RunId, spec = Spec.Id, round, timestampUtc = DateTime.UtcNow });
        await agent.RunAsync([new ChatMessage(ChatRole.User, taskText)], cancellationToken: ct).ConfigureAwait(false);
        var derivedPath = Path.Combine(OutDir, "derived.json");
        if (!tools.Saved || !File.Exists(derivedPath)) return (false, EmptyDoc(), tools);
        var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath, ct).ConfigureAwait(false), Read) ?? EmptyDoc();
        return (true, doc, tools);
    }
}

/// <summary>Start-Knoten: Runde 0 (BuildAccountVerifyTask) → ReflectDraft an den Gate.</summary>
[SendsMessage(typeof(ReflectDraft))]
internal sealed class ReflectProducerExecutor(ReflectGraphDeps deps) : Executor<SourceArtifactSet>($"ReflectProducer-{deps.Spec.Id}")
{
    public override async ValueTask HandleAsync(SourceArtifactSet sources, IWorkflowContext context, CancellationToken ct = default)
    {
        var (saved, doc, tools) = await deps.RunRoundAsync(sources, ReflectPipeline.BuildAccountVerifyTask(), 0, ct).ConfigureAwait(false);
        await context.SendMessageAsync(new ReflectDraft(sources, saved, doc, tools, 0, null, new ReflectTrace())).ConfigureAwait(false);
    }
}

/// <summary>Der externe Gate-Knoten (wiederverwendbarer Critic): Snapshot + Item-Diff + Gate-Auswertung → Verdikt mit
/// Decision. Terminierung: Iteration ≥ MaxRetries ⇒ GiveUp (statt weiterer Revise-Runde).</summary>
[SendsMessage(typeof(ReflectVerdict))]
internal sealed class ReflectGateExecutor(ReflectGraphDeps deps) : Executor<ReflectDraft>($"ReflectGate-{deps.Spec.Id}")
{
    public override async ValueTask HandleAsync(ReflectDraft d, IWorkflowContext context, CancellationToken ct = default)
    {
        d.Trace.RoundsRun = d.Iteration + 1;

        if (!d.Saved)
        {
            var fails = new List<string> { "Kein Artefakt gespeichert (save_derived nicht aufgerufen)." };
            if (d.Iteration == 0) { d.Trace.FirstDraftGatePass = false; d.Trace.FirstDraftR1 = null; }
            d.Trace.GateHistory.Add(new { round = d.Iteration, gatePass = false, fails });
            deps.Run.AppendEvent(new { type = "REFLECT_GATE", runId = deps.Run.RunId, spec = deps.Spec.Id, round = d.Iteration, gatePass = false, failCount = fails.Count, r1 = (double?)null, selfClean = false, note = "no_save", timestampUtc = DateTime.UtcNow });
            var dNoSave = d.Iteration >= deps.MaxRetries ? ReflectDecision.GiveUp : ReflectDecision.Revise;
            await context.SendMessageAsync(new ReflectVerdict(d.Sources, d.Doc, d.Tools, false, fails,
                new Dictionary<string, List<string>>(), [], [], null, null, dNoSave, d.Iteration, d.Trace)).ConfigureAwait(false);
            return;
        }

        var derivedPath = Path.Combine(deps.OutDir, "derived.json");
        var snapRel = ReflectPipeline.SnapshotRound(deps.Run, deps.OutDir, d.Iteration, derivedPath);
        if (d.PrevDoc is not null)
            ReflectPipeline.EmitReviseDiff(deps.Run, deps.Spec.Id, deps.OutDir, ReflectGraphDeps.Json, d.Iteration - 1, d.Iteration, d.PrevDoc, d.Doc, snapRel);

        var sourceIds = d.Sources.ItemsById().Keys.ToHashSet(StringComparer.Ordinal);
        var gate = await ReflectPipeline.EvaluateGateAsync(d.Sources, sourceIds, d.Doc, d.Tools, deps.PostHocChecker, ct).ConfigureAwait(false);

        if (d.Iteration == 0) { d.Trace.FirstDraftGatePass = gate.GatePass; d.Trace.FirstDraftR1 = gate.R1; }
        d.Trace.GateHistory.Add(new { round = d.Iteration, gatePass = gate.GatePass, fails = gate.Fails });
        deps.Run.AppendEvent(new { type = "REFLECT_GATE", runId = deps.Run.RunId, spec = deps.Spec.Id, round = d.Iteration, gatePass = gate.GatePass, failCount = gate.Fails.Count, r1 = gate.R1, selfClean = gate.Coverage.SelfAccountingClean, timestampUtc = DateTime.UtcNow });

        var decision = gate.GatePass ? ReflectDecision.Accept : (d.Iteration >= deps.MaxRetries ? ReflectDecision.GiveUp : ReflectDecision.Revise);
        await context.SendMessageAsync(new ReflectVerdict(d.Sources, d.Doc, d.Tools, true, gate.Fails, gate.FlaggedByItem,
            gate.Verdicts, gate.Invalid, gate.Coverage, gate.R1, decision, d.Iteration, d.Trace)).ConfigureAwait(false);
    }
}

/// <summary>Revise-Knoten (nur bei Decision==Revise): speist y_t + fb_t chirurgisch zurück (bzw. NoDraft bei no-save),
/// läuft die nächste Runde und schickt den neuen Entwurf über die Loop-Back-Kante zurück in den Gate.</summary>
[SendsMessage(typeof(ReflectDraft))]
internal sealed class ReflectReviseExecutor(ReflectGraphDeps deps) : Executor<ReflectVerdict>($"ReflectRevise-{deps.Spec.Id}")
{
    public override async ValueTask HandleAsync(ReflectVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var taskText = v.Saved
            ? ReflectPipeline.BuildReviseTask(v.Doc, v.FlaggedByItem, v.Coverage!)
            : ReflectPipeline.BuildNoDraftTask(v.Fails);
        var nextIter = v.Iteration + 1;
        var (saved, doc, tools) = await deps.RunRoundAsync(v.Sources, taskText, nextIter, ct).ConfigureAwait(false);
        var prevDoc = v.Saved ? v.Doc : null;   // Basis für den nächsten Item-Diff
        await context.SendMessageAsync(new ReflectDraft(v.Sources, saved, doc, tools, nextIter, prevDoc, v.Trace)).ConfigureAwait(false);
    }
}

/// <summary>Terminal-Knoten (Decision==Accept|GiveUp): reflect-Block + Reports schreiben, DerivationResult ausgeben.</summary>
[YieldsOutput(typeof(DerivationResult))]
internal sealed class ReflectFinalizeExecutor(ReflectGraphDeps deps) : Executor<ReflectVerdict>($"ReflectFinalize-{deps.Spec.Id}")
{
    public override async ValueTask HandleAsync(ReflectVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var accept = v.Decision == ReflectDecision.Accept;
        var finalBad = v.Verdicts.Count(x => x.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated);
        var finalR1 = v.Doc.Items.Count == 0 ? 0.0 : Math.Round((double)finalBad / v.Doc.Items.Count, 4);
        var overCorrected = v.Trace.FirstDraftR1 is double fd && finalR1 > fd;
        var reflectBlock = new
        {
            rounds = v.Trace.RoundsRun, maxRetries = deps.MaxRetries,
            firstDraftGatePass = v.Trace.FirstDraftGatePass, finalGatePass = accept, needsRepair = !accept,
            firstDraftR1 = v.Trace.FirstDraftR1, finalR1, overCorrected, gateHistory = v.Trace.GateHistory
        };
        var decision = accept ? "reflect_pass" : "reflect_needsRepair";
        await ReflectPipeline.WriteReportsAsync(v.Doc, v.Verdicts, v.Invalid, v.Sources, v.Tools, decision, deps.Run, deps.Spec, deps.OutDir,
            ReflectGraphDeps.ModeLabel, wantsVerify: true, wantsCoverage: true, accountVerify: true, deps.IndependentPostHoc, deps.PostHocJudgeModel,
            ReflectGraphDeps.Json, reflectBlock, ct).ConfigureAwait(false);
        deps.Run.AppendEvent(new { type = "REFLECT_DONE", runId = deps.Run.RunId, spec = deps.Spec.Id, rounds = v.Trace.RoundsRun, finalGatePass = accept, needsRepair = !accept, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DerivationResult(v.Doc, v.Verdicts, v.Invalid, decision)).ConfigureAwait(false);
    }
}

/// <summary>Baut den edge-nativen Reflect-Zyklus. Graph (Message-Passing, EIN Zyklus — MAF-nativ, Superstep-Modell):
/// <code>
///   (SourceArtifactSet)─► Producer ─Draft─► Gate ─Verdict─┬─(Revise)─► Revise ─Draft─┐
///                                                          │                          │ (Loop-Back)
///                                                          └─(Accept|GiveUp)─► Finalize ▼
///                                                                (DerivationResult)   Gate
/// </code>
/// Konditionale Kanten am Gate filtern auf <see cref="ReflectVerdict.Decision"/>; die Rückkante Revise→Gate ist der
/// Zyklus; die Iterationsschranke reist in <see cref="ReflectDraft.Iteration"/> und wird im Gate terminal (GiveUp).</summary>
internal static class ReflectGraphWorkflow
{
    public const string WorkflowName = "ReflectGraph";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(ReflectGraphDeps deps)
    {
        var producer = new ReflectProducerExecutor(deps);
        var gate = new ReflectGateExecutor(deps);
        var revise = new ReflectReviseExecutor(deps);
        var finalize = new ReflectFinalizeExecutor(deps);

        var builder = new WorkflowBuilder(producer)
            .WithName(WorkflowName)
            .WithDescription("Producer → Gate →[Revise-Loop]/[Finalize]. Edge-native Reflect (konditionale Kanten + Loop-Back, "
                           + "bounded via Iteration in der Message); inhaltlich identisch zur node-internen Form (ReflectPipeline).");

        builder.AddEdge(producer, gate);
        builder.AddEdge<ReflectVerdict>(gate, revise, m => m is not null && m.Decision == ReflectDecision.Revise);
        builder.AddEdge<ReflectVerdict>(gate, finalize, m => m is not null && m.Decision != ReflectDecision.Revise);
        builder.AddEdge(revise, gate);   // Loop-Back (echter Zyklus)
        builder.WithOutputFrom(finalize);

        return builder.Build();
    }
}
