using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Adr;

// R-11 A5 Schicht 2 (06.08.) — der ADR-Autor-Strip in R-33-GateLoop-Form, EXAKT im Classify-Muster (A2-2):
// zwei dünne Bridges (Betrieb nach dem Classify-Apply, arch-aktiv-gescoped · Bootstrap nach dessen Spiegel)
// speisen denselben Maker→Gate→[Repair]→Finalize→[adr-gate]→Apply; der Bahnen-Passagier überlebt den
// Port-Roundtrip per Datei-Vertrag (07-adr/passenger-*.json + lane.json) und wird TYP-GENAU re-emittiert.

internal static class AdrLanes
{
    public const string Operational = "operational";
    public const string Bootstrap = "bootstrap";
}

internal static class AdrTask
{
    public const string Text = """
                               Formuliere fuer jedes pending design-Item des Core einen ADR-Entwurf (MADR-Form).
                               1. list_pending_adrs - lies die Items (itemId, text, rationale, Belege).
                               2. list_core_truth - die Wahrheits-Items (requirement|architecture) fuer relatedIds:
                                  nenne verwandte Wahrheiten (z. B. die Anforderungs-Sicht derselben Festlegung).
                               3. Je Item GENAU EIN Entwurf: {itemId, title, context, decision, consequences,
                                  relatedIds?, alternatives?}.
                                  - title: praegnanter Entscheidungs-Titel (kein Satz).
                                  - context: WARUM stand die Entscheidung an (aus Text/Begruendung/Belegen).
                                  - decision: WAS wurde entschieden (praezise, aktiv formuliert).
                                  - consequences: was folgt daraus (positiv wie einschraenkend).
                                  Du dokumentierst die getroffene Entscheidung - du triffst KEINE neue.
                               4. check_adr_drafts (muss pass sein), dann save_adr_drafts (genau einmal).
                               """;
}

/// <summary>MAKER-Tools des ADR-Autors (Muster ArchClassifyTools): erkunden + deterministisch prüfen + speichern.</summary>
internal sealed class AdrTools(IReadOnlyList<ProjectStateItem> pending, ProjectStateDocument core, RunContext run)
{
    private IReadOnlyList<AdrDraft>? _saved;
    private int _checkRounds;

    public bool Saved => _saved is not null;
    public IReadOnlyList<AdrDraft>? SavedDrafts => _saved;
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListPending, "list_pending_adrs",
            "Listet die design-Items, die ein ADR brauchen (itemId, text, rationale, sourceClaimIds)."),
        AIFunctionFactory.Create(ListTruth, "list_core_truth",
            "Listet die Wahrheits-Items des Core (requirement|architecture: entityId, itemType, text) - die einzigen erlaubten relatedIds."),
        AIFunctionFactory.Create(Check, "check_adr_drafts",
            "Prueft die Entwuerfe deterministisch (Coverage/Pflichtfelder/relatedIds). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(Save, "save_adr_drafts",
            "Speichert die finalen ADR-Entwuerfe (genau einer je Item). Genau einmal am Ende aufrufen."),
    ];

    private object ListPending()
    {
        run.AppendEvent(new { type = "ADR_TOOL_PENDING", runId = run.RunId, items = pending.Count, timestampUtc = DateTime.UtcNow });
        return pending.Select(i => new { itemId = i.ItemId, text = i.Text, rationale = i.Architecture?.Rationale, sourceClaimIds = i.SourceClaimIds }).ToList();
    }

    private object ListTruth()
    {
        var rows = core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new { entityId = i.ItemId, itemType = i.ItemType.ToLowerInvariant(), text = i.Text })
            .ToList();
        run.AppendEvent(new { type = "ADR_TOOL_TRUTH", runId = run.RunId, returned = rows.Count, timestampUtc = DateTime.UtcNow });
        return rows;
    }

    private object Check(IReadOnlyList<AdrDraft> drafts)
    {
        _checkRounds++;
        var r = AdrGate.Check(pending, drafts, core);
        run.AppendEvent(new { type = "ADR_TOOL_CHECK", runId = run.RunId, pass = r.Pass, errors = r.Errors.Count, timestampUtc = DateTime.UtcNow });
        return new { pass = r.Pass, errors = r.Errors.Select(e => new { e.Code, e.Message, e.ItemId }).ToList() };
    }

    private string Save(IReadOnlyList<AdrDraft> drafts)
    {
        _saved = drafts;
        run.AppendEvent(new { type = "ADR_TOOL_SAVE", runId = run.RunId, drafts = drafts.Count, timestampUtc = DateTime.UtcNow });
        return "gespeichert";
    }
}

// ---- Strip-Messages (stateless, R-33: alles reist in den Messages; Core wie im Ingestion-Strip) ----
internal sealed record AdrWork(IReadOnlyList<ProjectStateItem> Pending, ProjectStateDocument Core, string Lane, int MaxAttempts);
internal sealed record AdrDraftBatch(IReadOnlyList<ProjectStateItem> Pending, ProjectStateDocument Core,
    IReadOnlyList<AdrDraft> Drafts, bool Saved, int CheckRounds, int Attempt, string Source, string Lane, int MaxAttempts, IReadOnlyList<GateAttempt> History);
internal sealed record AdrVerdict(IReadOnlyList<ProjectStateItem> Pending, ProjectStateDocument Core,
    IReadOnlyList<AdrDraft> Drafts, AdrGateReport Report, GateDecision Decision, int Attempt, string Lane, int MaxAttempts, IReadOnlyList<GateAttempt> History);

/// <summary>U5-Anfrage: Entwürfe + Wahrheits-Katalog (ReferenceList) + gerenderte Datei-Vorschau je Entwurf.</summary>
public sealed record AdrItemView(
    [property: JsonPropertyName("draft")] AdrDraft Draft,
    [property: JsonPropertyName("itemText")] string ItemText,
    [property: JsonPropertyName("preview")] string Preview);
public sealed record AdrReviewRequest(string RunId, IReadOnlyList<AdrItemView> Items,
    [property: JsonPropertyName("truth")] IReadOnlyList<ArchClassify.ArchPbiOption>? Truth = null);
/// <summary>FINALE Entwürfe (Mensch editiert/nimmt an; weggelassene Items = vertagt, Scan legt erneut vor).</summary>
public sealed record AdrReviewResponse(IReadOnlyList<AdrDraft> Accepted, string Reviewer);

// ---- Bridges (Leer-Skip per Typ-Routing; Passagier + Lane als Datei-Vertrag; Betrieb arch-aktiv-gescoped) ----

[SendsMessage(typeof(IngestionApplyReport))]
[SendsMessage(typeof(AdrWork))]
internal sealed class AdrOperationalBridgeExecutor(RunContext run, string repoRoot, string outDir, int maxAttempts)
    : Executor<IngestionApplyReport>("AdrBridgeOperational")
{
    public override async ValueTask HandleAsync(IngestionApplyReport passenger, IWorkflowContext context, CancellationToken ct = default)
    {
        // Smoke-/Leer-Lauf-Schutz (Lehre A2/A4): nur arch-aktive Läufe projizieren; Rückstau-Abbau gehört
        // dem Bootstrap-Spiegel bzw. dem nächsten arch-aktiven Lauf (T2.21 bleibt erfüllt).
        var deltaPath = Path.Combine(run.OutputDir("04-delta"), "project-state.json");
        var archInRun = File.Exists(deltaPath)
            && (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document.Items.Any(AspectIngestionProfile.Architecture.Matches);
        if (!archInRun)
        {
            run.AppendEvent(new { type = "ADR_SKIPPED", runId = run.RunId, lane = AdrLanes.Operational, reason = "no-arch-in-run", timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        var pending = AdrProjection.PendingAdrItems(core);
        if (pending.Count == 0 && AdrProjection.StatusFollowUps(core).Count == 0)
        {
            run.AppendEvent(new { type = "ADR_SKIPPED", runId = run.RunId, lane = AdrLanes.Operational, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-operational.json"), JsonSerializer.Serialize(passenger, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), JsonSerializer.Serialize(new { lane = AdrLanes.Operational }, JsonFiles.Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ADR_START", runId = run.RunId, lane = AdrLanes.Operational, pending = pending.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AdrWork(pending, core, AdrLanes.Operational, maxAttempts)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(CoreBootstrapOutput))]
[SendsMessage(typeof(AdrWork))]
internal sealed class AdrBootstrapBridgeExecutor(RunContext run, string repoRoot, string outDir, int maxAttempts)
    : Executor<CoreBootstrapOutput>("AdrBridgeBootstrap")
{
    public override async ValueTask HandleAsync(CoreBootstrapOutput passenger, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        var pending = AdrProjection.PendingAdrItems(core);
        if (pending.Count == 0)
        {
            run.AppendEvent(new { type = "ADR_SKIPPED", runId = run.RunId, lane = AdrLanes.Bootstrap, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(passenger).ConfigureAwait(false);
            return;
        }
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-bootstrap.json"), JsonSerializer.Serialize(passenger, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), JsonSerializer.Serialize(new { lane = AdrLanes.Bootstrap }, JsonFiles.Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ADR_START", runId = run.RunId, lane = AdrLanes.Bootstrap, pending = pending.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AdrWork(pending, core, AdrLanes.Bootstrap, maxAttempts)).ConfigureAwait(false);
    }
}

// ---- Der Loop (Maker → Gate → [Repair] → Finalize) ----

[SendsMessage(typeof(AdrDraftBatch))]
internal sealed class AdrMakerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<AdrWork>("AdrMaker")
{
    public override async ValueTask HandleAsync(AdrWork work, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new AdrTools(work.Pending, work.Core, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, AdrTask.Text)], cancellationToken: ct).ConfigureAwait(false);
        var drafts = tools.SavedDrafts ?? [];
        run.AppendEvent(new { type = "ADR_MAKER_DONE", runId = run.RunId, saved = tools.Saved, drafts = drafts.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AdrDraftBatch(work.Pending, work.Core, drafts, tools.Saved, tools.CheckRounds,
            Attempt: 1, Source: "maker", work.Lane, work.MaxAttempts, History: [])).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(AdrVerdict))]
internal sealed class AdrGateExecutor(RunContext run) : Executor<AdrDraftBatch>("AdrGate")
{
    public override async ValueTask HandleAsync(AdrDraftBatch b, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = AdrGate.Check(b.Pending, b.Drafts, b.Core);
        var decision = AdrGate.Decide(report, b.Attempt, b.MaxAttempts);
        var attempt = new GateAttempt(b.Attempt, b.Source, report.Pass, decision.ToString(),
            report.Errors.Select(e => $"{e.Code}: {e.Message}").ToList(), DateTime.UtcNow);
        run.AppendEvent(new { type = "ADR_GATE", runId = run.RunId, attempt = b.Attempt, source = b.Source, pass = report.Pass, decision = decision.ToString(), errors = report.Errors.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AdrVerdict(b.Pending, b.Core, b.Drafts, report, decision,
            b.Attempt, b.Lane, b.MaxAttempts, b.History.Append(attempt).ToList())).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(AdrDraftBatch))]
internal sealed class AdrRepairExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<AdrVerdict>("AdrRepair")
{
    public override async ValueTask HandleAsync(AdrVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        var errors = string.Join("\n", v.Report.Errors.Select(e => $"- {e.Code}: {e.Message}"));
        var task = $"""
                    {AdrTask.Text}

                    Dein vorheriger Entwurf hat das Gate NICHT bestanden. Fehler:
                    {errors}
                    Korrigiere gezielt und speichere erneut (save_adr_drafts genau einmal).
                    """;
        var tools = new AdrTools(v.Pending, v.Core, run);
        var agent = agentFactory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);
        var drafts = tools.SavedDrafts ?? [];
        run.AppendEvent(new { type = "ADR_REPAIR", runId = run.RunId, fromAttempt = v.Attempt, drafts = drafts.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AdrDraftBatch(v.Pending, v.Core, drafts, tools.Saved, tools.CheckRounds,
            Attempt: v.Attempt + 1, Source: "repair", v.Lane, v.MaxAttempts, v.History)).ConfigureAwait(false);
    }
}

[SendsMessage(typeof(AdrReviewRequest))]
[YieldsOutput(typeof(string))]
internal sealed class AdrFinalizeExecutor(RunContext run, string outDir) : Executor<AdrVerdict>("AdrFinalize")
{
    public override async ValueTask HandleAsync(AdrVerdict v, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "plan.json"),
            JsonSerializer.Serialize(new { lane = v.Lane, drafts = v.Drafts }, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "adr-attempts.json"),
            JsonSerializer.Serialize(v.History, JsonFiles.Json), ct).ConfigureAwait(false);

        if (v.Decision == GateDecision.Pass)
        {
            var textById = v.Pending.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
            // Vorschau = die KORREKTE Datei (Autor 06.08.): prospektive Nummer (Vergabe-Reihenfolge des Apply)
            // + Vermerk-Kopf; die endgueltige Datei entsteht erst mit der Freigabe.
            var nextNo = AdrProjection.NextAdrNumber(v.Core);
            var views = v.Drafts.Select((d, i) => new AdrItemView(d,
                textById.GetValueOrDefault(d.ItemId, ""),
                "> Vorschau auf Basis des System-Vorschlags — Datei und Nummer entstehen erst mit deiner Freigabe.\n\n"
                + AdrProjection.Render(d, AdrProjection.AdrIdFor(nextNo + i), AdrStatus.Accepted, v.Core))).ToList();
            var truth = v.Core.Items
                .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.ItemId, StringComparer.Ordinal)
                .Select(i => new ArchClassify.ArchPbiOption(i.ItemId, i.Text)).ToList();
            var request = new AdrReviewRequest(run.RunId, views, truth);
            await File.WriteAllTextAsync(Path.Combine(outDir, "adr-request.json"),
                JsonSerializer.Serialize(request, JsonFiles.Json), ct).ConfigureAwait(false);   // Datei-Vertrag der Review-CLI
            run.AppendEvent(new { type = "ADR_HUMAN_GATE", runId = run.RunId, items = views.Count, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(request).ConfigureAwait(false);
        }
        else
        {
            run.AppendEvent(new { type = "ADR_DONE", runId = run.RunId, decision = v.Decision.ToString(), applied = false, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync($"ADR-Projektion: {v.Decision} nach {v.Attempt} Versuch(en) — {v.Report.Errors.Count} Fehler (07-adr/plan.json). Lauf: {run.RunId}").ConfigureAwait(false);
        }
    }
}

/// <summary>APPLY: schreibt die AUTORISIERTEN ADRs (Dateien + Payload via Kangal-Save + Index/Übersicht)
/// und re-emittiert den Bahnen-Passagier TYP-GENAU (Kanten-Routing).</summary>
[SendsMessage(typeof(IngestionApplyReport))]
[SendsMessage(typeof(CoreBootstrapOutput))]
internal sealed class AdrApplyExecutor(RunContext run, string repoRoot, string outDir, string adrDir) : Executor<AdrReviewResponse>("AdrApply")
{
    public override async ValueTask HandleAsync(AdrReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var repo = new JsonCoreRepository(repoRoot);
        var core = await repo.LoadAsync(ct).ConfigureAwait(false);
        var (updated, files, adrIds) = AdrGate.Apply(core, resp.Accepted, adrDir);
        foreach (var (rel, content) in files)
        {
            var abs = Path.Combine(repoRoot, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
            await File.WriteAllTextAsync(abs, content, ct).ConfigureAwait(false);
        }
        await repo.SaveAsync(updated, ct).ConfigureAwait(false);   // CoreKangal wacht am Port
        run.AppendEvent(new { type = "ADR_APPLIED", runId = run.RunId, adrs = adrIds.Count, files = files.Count, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });

        using var laneDoc = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "lane.json"), ct).ConfigureAwait(false));
        var lane = laneDoc.RootElement.GetProperty("lane").GetString();
        if (lane == AdrLanes.Bootstrap)
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
internal static class AdrWorkflow
{
    public static void AddTo(WorkflowBuilder b,
        AdrMakerExecutor maker, AdrGateExecutor gate, AdrRepairExecutor repair,
        AdrFinalizeExecutor finalize, RequestPort port, AdrApplyExecutor apply)
    {
        b.AddEdge(maker, gate);
        b.AddEdge<AdrVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<AdrVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, port);
        b.AddEdge(port, apply);
        b.WithOutputFrom(finalize);
    }
}
