using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.ArchClassify;

// R-11 A2-2 (05.08.) — der Klassifikations-Strip in R-33-GateLoop-Form, quell-agnostisch (T2.21) und mit
// EINEM geteilten Strip für BEIDE Bahnen (T2.22): zwei dünne Bridges (Betrieb nach ArchApply · Bootstrap nach
// CoreBootstrap) speisen denselben Maker→Gate→[Repair]→Finalize→[arch-classify-gate]→Apply; der jeweilige
// Bahnen-Passagier (req-Report | CoreBootstrapOutput) überlebt den Port-Roundtrip per Datei-Vertrag
// (07-arch-classify/passenger-*.json + lane.json) und wird vom Apply TYP-GENAU re-emittiert (Kanten-Routing).

internal static class ArchClassifyLanes
{
    public const string Operational = "operational";
    public const string Bootstrap = "bootstrap";
}

internal static class ArchClassifyTask
{
    public const string Text = """
                               Klassifiziere die unklassifizierten Architektur-Items des Core nach ihren Konsum-ROLLEN
                               und benenne bei constraint die BETROFFENEN Ziel-PBIs (gebuendeltes Wirkungs-Gate).
                               1. list_unclassified_architecture - lies alle Items (itemId, text, Belege).
                               2. list_active_pbis - lies die aktiven PBIs (pbiId, title): das sind die einzigen erlaubten Ziele.
                               3. Je Item GENAU EIN Vorschlag mit 1..N Rollen (Rollen KOMBINIEREN sich):
                                  - constraint: schraenkt andere Arbeit ein -> erscheint in den Issues der betroffenen PBIs.
                                    PFLICHT: targetPbiIds = die PBIs, die der Rahmen WIRKLICH einschraenkt (nur fachlich betroffene,
                                    keine Sammel-Listen; existiert kein betroffenes PBI, Rolle ueberdenken).
                                  - work: es muss etwas GETAN werden -> wird ein eigenes technisches PBI.
                                    Optional: targetPbiIds = "gehoert fachlich zu" (reine Orientierung, KEINE Wirkung).
                                  - design: eine begruendbare Entscheidung -> wird ein ADR-Dokument (keine targetPbiIds noetig).
                                  Beispiel: eine Technologie-Festlegung ist oft design UND constraint zugleich.
                               4. rationale: kurz begruenden, WARUM diese Rollen und diese Ziele (deutsch).
                               5. check_classification (muss pass sein), dann save_classification (genau einmal).
                               """;
}

/// <summary>MAKER-Tools der Klassifikation (Muster IngestionTools): erkunden + deterministisch prüfen + speichern.</summary>
internal sealed class ArchClassifyTools(IReadOnlyList<ProjectStateItem> unclassified, IReadOnlyList<ArchPbiOption> activePbis, RunContext run)
{
    private IReadOnlyList<ArchClassifyProposal>? _saved;
    private int _checkRounds;

    public bool Saved => _saved is not null;
    public IReadOnlyList<ArchClassifyProposal>? SavedProposals => _saved;
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListUnclassified, "list_unclassified_architecture",
            "Listet die zu klassifizierenden Architektur-Items (itemId, text, sourceClaimIds)."),
        AIFunctionFactory.Create(ListActivePbis, "list_active_pbis",
            "Listet die aktiven PBIs des Core (pbiId, title) — die einzigen erlaubten targetPbiIds."),
        AIFunctionFactory.Create(Check, "check_classification",
            "Prueft die Rollen-Vorschlaege deterministisch (Coverage/Rollen/Begruendung). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(Save, "save_classification",
            "Speichert die finalen Rollen-Vorschlaege (genau einer je Item). Genau einmal am Ende aufrufen."),
    ];

    private object ListUnclassified()
    {
        run.AppendEvent(new { type = "ARCH_CLASSIFY_TOOL_LIST", runId = run.RunId, items = unclassified.Count, timestampUtc = DateTime.UtcNow });
        return unclassified.Select(i => new { itemId = i.ItemId, text = i.Text, sourceClaimIds = i.SourceClaimIds }).ToList();
    }

    private object ListActivePbis()
    {
        run.AppendEvent(new { type = "ARCH_CLASSIFY_TOOL_PBIS", runId = run.RunId, pbis = activePbis.Count, timestampUtc = DateTime.UtcNow });
        return activePbis.Select(p => new { pbiId = p.Id, title = p.Title }).ToList();
    }

    private object Check(IReadOnlyList<ArchClassifyProposal> proposals)
    {
        _checkRounds++;
        var r = ArchClassifyGate.Check(unclassified, proposals, activePbis.Select(p => p.Id).ToHashSet(StringComparer.Ordinal));
        run.AppendEvent(new { type = "ARCH_CLASSIFY_TOOL_CHECK", runId = run.RunId, pass = r.Pass, errors = r.Errors.Count, timestampUtc = DateTime.UtcNow });
        return new { pass = r.Pass, errors = r.Errors.Select(e => new { e.Code, e.Message, e.ItemId }).ToList() };
    }

    private string Save(IReadOnlyList<ArchClassifyProposal> proposals)
    {
        _saved = proposals;
        run.AppendEvent(new { type = "ARCH_CLASSIFY_TOOL_SAVE", runId = run.RunId, proposals = proposals.Count, timestampUtc = DateTime.UtcNow });
        return "gespeichert";
    }
}

// ---- Strip-Messages (Attempt/Source/History reisen in den Messages — stateless, R-33-Muster) ----
internal sealed record ArchClassifyWork(IReadOnlyList<ProjectStateItem> Unclassified, IReadOnlyList<ArchPbiOption> ActivePbis, string Lane, int MaxAttempts);
internal sealed record ArchClassifyDraft(IReadOnlyList<ProjectStateItem> Unclassified, IReadOnlyList<ArchPbiOption> ActivePbis,
    IReadOnlyList<ArchClassifyProposal> Proposals,
    bool Saved, int CheckRounds, int Attempt, string Source, string Lane, int MaxAttempts, IReadOnlyList<GateAttempt> History);
internal sealed record ArchClassifyVerdict(IReadOnlyList<ProjectStateItem> Unclassified, IReadOnlyList<ArchPbiOption> ActivePbis,
    IReadOnlyList<ArchClassifyProposal> Proposals,
    ArchClassifyGateReport Report, GateDecision Decision, int Attempt, string Lane, int MaxAttempts, IReadOnlyList<GateAttempt> History);

public sealed record ArchClassifyItemView(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("proposedRoles")] IReadOnlyList<string> ProposedRoles,
    [property: JsonPropertyName("rationale")] string Rationale,
    // ① (06.08.): die vorgeschlagenen Ziel-PBIs MIT Titel — der Mensch sieht in der Card, WEN der Rahmen
    // trifft, statt eine Alle-Liste zu wälzen (Autor-Feedback). Optional: Alt-Anfragen bleiben lesbar.
    [property: JsonPropertyName("proposedTargets")] IReadOnlyList<ArchPbiOption>? ProposedTargets = null);
public sealed record ArchClassifyReviewRequest(string RunId, IReadOnlyList<ArchClassifyItemView> Items,
    [property: JsonPropertyName("activePbis")] IReadOnlyList<ArchPbiOption>? ActivePbis = null);
/// <summary>FINALE Rollen je Item (Mensch korrigiert die Vorschläge; weggelassene Items bleiben unklassifiziert
/// = vertagt — der idempotente Scan legt sie beim nächsten Lauf erneut vor).</summary>
public sealed record ArchClassifyReviewResponse(IReadOnlyList<ArchClassifyProposal> Accepted, string Reviewer);

// ---- Bridges (Leer-Skip per Typ-Routing; Passagier + Lane als Datei-Vertrag) ----

[SendsMessage(typeof(IngestionApplyReport))]
[SendsMessage(typeof(ArchClassifyWork))]
internal sealed class OperationalClassifyBridgeExecutor(RunContext run, string repoRoot, string outDir, int maxAttempts,
    bool archCatchup = false)
    : Executor<IngestionApplyReport>("ArchClassifyBridgeOperational")
{
    public override async ValueTask HandleAsync(IngestionApplyReport passenger, IWorkflowContext context, CancellationToken ct = default)
    {
        // Betriebs-Scope (Fix 06.08., Smoke-Fund): klassifiziert wird NUR, wenn DIESER Lauf arch-Items einbrachte
        // (Delta via Datei-Vertrag) — sonst zündete jeder req-/Leer-Lauf (inkl. LLM-freiem Smoke!) den Maker für
        // den kompletten unklassifizierten Kern-Bestand. Der Rückstau-Abbau gehört dem Bootstrap-Spiegel bzw.
        // dem NÄCHSTEN arch-aktiven Lauf (der nimmt ihn quell-agnostisch mit — T2.21 bleibt erfüllt).
        var deltaPath = Path.Combine(run.OutputDir("04-delta"), "project-state.json");
        var archInRun = File.Exists(deltaPath)
            && (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document.Items.Any(AspectIngestionProfile.Architecture.Matches);
        if (!archInRun)
        {
            run.AppendEvent(new { type = "ARCH_CLASSIFY_SKIPPED", runId = run.RunId, lane = ArchClassifyLanes.Operational, reason = "no-arch-in-run", timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }

        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        // 1d Catch-up-Schalter (19.08.): Normal-Lauf klassifiziert NUR die eigenen arch-Items; der
        // Bestands-Rückstau bleibt LAUT liegen, bis der Autor ihn bewusst bestellt (--arch-catchup).
        var (unclassified, deferred) = RunScopedBacklog.Scope(ArchClassifyGate.Unclassified(core), run.RunId, archCatchup);
        if (deferred > 0)
        {
            Console.WriteLine(RunScopedBacklog.DeferredLine("arch-classify", deferred));
            run.AppendEvent(new { type = "ARCH_CLASSIFY_BACKLOG_DEFERRED", runId = run.RunId, deferred, timestampUtc = DateTime.UtcNow });
        }
        if (unclassified.Count == 0)
        {
            run.AppendEvent(new { type = "ARCH_CLASSIFY_SKIPPED", runId = run.RunId, lane = ArchClassifyLanes.Operational, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-operational.json"), JsonSerializer.Serialize(passenger, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), JsonSerializer.Serialize(new { lane = ArchClassifyLanes.Operational }, JsonFiles.Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ARCH_CLASSIFY_START", runId = run.RunId, lane = ArchClassifyLanes.Operational, items = unclassified.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ArchClassifyWork(unclassified, ArchClassifyGate.ActivePbis(core), ArchClassifyLanes.Operational, maxAttempts)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(CoreBootstrapOutput))]
[SendsMessage(typeof(ArchClassifyWork))]
internal sealed class BootstrapClassifyBridgeExecutor(RunContext run, string repoRoot, string outDir, int maxAttempts)
    : Executor<CoreBootstrapOutput>("ArchClassifyBridgeBootstrap")
{
    public override async ValueTask HandleAsync(CoreBootstrapOutput passenger, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        var unclassified = ArchClassifyGate.Unclassified(core);
        if (unclassified.Count == 0)
        {
            run.AppendEvent(new { type = "ARCH_CLASSIFY_SKIPPED", runId = run.RunId, lane = ArchClassifyLanes.Bootstrap, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-bootstrap.json"), JsonSerializer.Serialize(passenger, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), JsonSerializer.Serialize(new { lane = ArchClassifyLanes.Bootstrap }, JsonFiles.Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ARCH_CLASSIFY_START", runId = run.RunId, lane = ArchClassifyLanes.Bootstrap, items = unclassified.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ArchClassifyWork(unclassified, ArchClassifyGate.ActivePbis(core), ArchClassifyLanes.Bootstrap, maxAttempts)).ConfigureAwait(false);
    }
}

// ---- Der Loop (Maker → Gate → [Repair] → Finalize) ----

[SendsMessage(typeof(ArchClassifyDraft))]
internal sealed class ArchClassifyMakerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<ArchClassifyWork>("ArchClassifyMaker")
{
    public override async ValueTask HandleAsync(ArchClassifyWork work, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new ArchClassifyTools(work.Unclassified, work.ActivePbis, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, ArchClassifyTask.Text)], cancellationToken: ct).ConfigureAwait(false);
        var proposals = tools.SavedProposals ?? [];
        run.AppendEvent(new { type = "ARCH_CLASSIFY_MAKER_DONE", runId = run.RunId, saved = tools.Saved, proposals = proposals.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ArchClassifyDraft(work.Unclassified, work.ActivePbis, proposals, tools.Saved, tools.CheckRounds,
            Attempt: 1, Source: "maker", work.Lane, work.MaxAttempts, History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(ArchClassifyVerdict))]
internal sealed class ArchClassifyGateExecutor(RunContext run) : Executor<ArchClassifyDraft>("ArchClassifyGate")
{
    public override async ValueTask HandleAsync(ArchClassifyDraft draft, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = ArchClassifyGate.Check(draft.Unclassified, draft.Proposals,
            draft.ActivePbis.Select(p => p.Id).ToHashSet(StringComparer.Ordinal));
        var decision = ArchClassifyGate.Decide(report, draft.Attempt, draft.MaxAttempts);
        var attempt = new GateAttempt(draft.Attempt, draft.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}: {e.Message}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "ARCH_CLASSIFY_GATE", runId = run.RunId, attempt = draft.Attempt, source = draft.Source, pass = report.Pass, decision = decision.ToString(), errors = report.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ArchClassifyVerdict(draft.Unclassified, draft.ActivePbis, draft.Proposals, report, decision,
            draft.Attempt, draft.Lane, draft.MaxAttempts, draft.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(ArchClassifyDraft))]
internal sealed class ArchClassifyRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<ArchClassifyVerdict>("ArchClassifyRepair")
{
    public override async ValueTask HandleAsync(ArchClassifyVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        // R-33-Muster: die Gate-Fehler gehen WOERTLICH als Feedback in den Retry.
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    {ArchClassifyTask.Text}

                    Dein vorheriger Vorschlag hat das Gate NICHT bestanden. Fehler:
                    {errors}
                    Korrigiere gezielt und speichere erneut (save_classification genau einmal).
                    """;
        var tools = new ArchClassifyTools(v.Unclassified, v.ActivePbis, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        var proposals = tools.SavedProposals ?? [];
        run.AppendEvent(new { type = "ARCH_CLASSIFY_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, proposals = proposals.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ArchClassifyDraft(v.Unclassified, v.ActivePbis, proposals, tools.Saved, tools.CheckRounds,
            Attempt: v.Attempt + 1, Source: "repair", v.Lane, v.MaxAttempts, v.History)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(ArchClassifyReviewRequest))]
[YieldsOutput(typeof(string))]
internal sealed class ArchClassifyFinalizeExecutor(RunContext run, string outDir) : Executor<ArchClassifyVerdict>("ArchClassifyFinalize")
{
    public override async ValueTask HandleAsync(ArchClassifyVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "plan.json"),
            JsonSerializer.Serialize(new { lane = v.Lane, proposals = v.Proposals }, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "classify-attempts.json"),
            JsonSerializer.Serialize(v.History, JsonFiles.Json), ct).ConfigureAwait(false);

        if (v.Decision == GateDecision.Pass)
        {
            var textById = v.Unclassified.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
            var titleById = v.ActivePbis.ToDictionary(p => p.Id, p => p.Title, StringComparer.Ordinal);
            var views = v.Proposals.Select(p => new ArchClassifyItemView(p.ItemId,
                textById.TryGetValue(p.ItemId, out var t) ? t : "", p.Roles, p.Rationale,
                (p.TargetPbiIds ?? []).Select(id => new ArchPbiOption(id, titleById.TryGetValue(id, out var tt) ? tt : "")).ToList())).ToList();
            var request = new ArchClassifyReviewRequest(run.RunId, views, v.ActivePbis);
            await File.WriteAllTextAsync(Path.Combine(outDir, "arch-classify-request.json"),
                JsonSerializer.Serialize(request, JsonFiles.Json), ct).ConfigureAwait(false);   // Datei-Vertrag der Review-CLI (A2-3)
            run.AppendEvent(new { type = "ARCH_CLASSIFY_HUMAN_GATE", runId = run.RunId, items = views.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(request).ConfigureAwait(false);
        }
        else
        {
            run.AppendEvent(new { type = "ARCH_CLASSIFY_DONE", runId = run.RunId, decision = v.Decision.ToString(), applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync($"arch-Klassifikation: {v.Decision} nach {v.Attempt} Versuch(en) — {v.Report.Errors.Count} Fehler (07-arch-classify/plan.json). Lauf: {run.RunId}").ConfigureAwait(false);
        }
    }
}

/// <summary>APPLY: schreibt die AUTORISIERTEN Rollen als typisiertes Payload (Kangal am Save) und re-emittiert
/// den Bahnen-Passagier TYP-GENAU (Kanten-Routing zum DecisionScan bzw. zur ClusterBridge).</summary>
[SendsMessage(typeof(IngestionApplyReport))]
[SendsMessage(typeof(CoreBootstrapOutput))]
internal sealed class ArchClassifyApplyExecutor(RunContext run, string repoRoot, string outDir) : Executor<ArchClassifyReviewResponse>("ArchClassifyApply")
{
    public override async ValueTask HandleAsync(ArchClassifyReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var repo = new JsonCoreRepository(repoRoot);
        var core = await repo.LoadAsync(ct).ConfigureAwait(false);
        var updated = ArchClassifyGate.Apply(core, resp.Accepted);
        await repo.SaveAsync(updated, ct).ConfigureAwait(false);   // CoreKangal wacht am Port
        run.AppendEvent(new { type = "ARCH_CLASSIFY_APPLIED", runId = run.RunId, classified = resp.Accepted.Count, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });

        using var laneDoc = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "lane.json"), ct).ConfigureAwait(false));
        var lane = laneDoc.RootElement.GetProperty("lane").GetString();
        if (lane == ArchClassifyLanes.Bootstrap)
        {
            var p = JsonSerializer.Deserialize<CoreBootstrapOutput>(await File.ReadAllTextAsync(Path.Combine(outDir, "passenger-bootstrap.json"), ct).ConfigureAwait(false), JsonFiles.Json)
                    ?? throw new InvalidOperationException("bootstrap-Passagier nicht lesbar.");
            await context.SendMessageAsync(p).ConfigureAwait(false);
        }
        else
        {
            var p = JsonSerializer.Deserialize<IngestionApplyReport>(await File.ReadAllTextAsync(Path.Combine(outDir, "passenger-operational.json"), ct).ConfigureAwait(false), JsonFiles.Json)
                    ?? throw new InvalidOperationException("operational-Passagier nicht lesbar.");
            await context.SendMessageAsync(p).ConfigureAwait(false);
        }
    }
}

/// <summary>Die EINE Kanten-Quelle des Strips (beide Bahnen; Entry-/Exit-Kanten setzt der Aufrufer).</summary>
internal static class ArchClassifyWorkflow
{
    public static void AddTo(WorkflowBuilder b,
        ArchClassifyMakerExecutor maker, ArchClassifyGateExecutor gate, ArchClassifyRepairExecutor repair,
        ArchClassifyFinalizeExecutor finalize, RequestPort port, ArchClassifyApplyExecutor apply)
    {
        b.AddEdge(maker, gate);
        b.AddEdge<ArchClassifyVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<ArchClassifyVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, port);
        b.AddEdge(port, apply);
        b.WithOutputFrom(finalize);   // terminal bei MaxAttempts/HumanReview
    }
}
