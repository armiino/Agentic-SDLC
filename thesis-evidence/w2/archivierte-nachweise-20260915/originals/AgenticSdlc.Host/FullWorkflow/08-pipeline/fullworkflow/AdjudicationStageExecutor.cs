using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// H2 (27.07.) — das Adjudikations-Gate als ECHTER RequestPort: die letzte offene Masterplan-Schuld
// („Adjudikation ist fachlich HITL, aber noch nicht RequestPort — Hebung ist W1e'-Kern"). Vorher löste EIN
// Executor die Policy intern (interactive unmöglich, terminaler Abbruch); jetzt: Queue-Bau → [adjudication-gate]
// → Apply — damit greifen H1-Pause/Resume und der zentrale Responder auch hier. Fach-Kern UNVERÄNDERT:
// LedgerAdjudicationAdapter.Build + AdjudicationResolver + AdjudicationCompletenessGate.Project.

/// <summary>adjudication-gate-Anfrage: die Queue-Items (eine Aktion je Item; UI: ledger-adjudicate-ui &lt;QueuePath&gt;).</summary>
public sealed record AdjudicationReviewRequest(
    IReadOnlyList<AdjudicationItem> Items, string LedgerRunId, string QueuePath, string ValidatedPath);

/// <summary>Antwort: Aktion je ItemId (fehlend/invalide ⇒ reject — Governance-Fallback des Resolvers).</summary>
public sealed record AdjudicationReviewResponse(IReadOnlyDictionary<string, string> Actions, string ReviewedBy);

// Geteilte IO der Adjudikations-Knoten + des Responders (Ledger-Artefakte, Queue-Aktionen).
internal static class AdjudicationIo
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    internal sealed record LedgerArtifacts(
        ValidatedLedger Validated, string? MissSignalJson, IReadOnlyDictionary<string, AtomicUnit> Units, string LedgerRunId);

    // Ledger-Run-Dir aus ValidatedPath ableiten: <ledgerRun>/step-03-facet-validation/output.json
    internal static async Task<LedgerArtifacts> LoadAsync(string validatedPath, CancellationToken ct)
    {
        var stepDir = Path.GetDirectoryName(validatedPath)!;
        var ledgerRunDir = Directory.GetParent(stepDir)!.FullName;
        var ledgerRunId = Path.GetFileName(ledgerRunDir);

        var validated = JsonSerializer.Deserialize<ValidatedLedger>(
            await File.ReadAllTextAsync(validatedPath, ct).ConfigureAwait(false), Json) ?? new ValidatedLedger([]);

        var missPath = Path.Combine(ledgerRunDir, "step-01d-unused-unit-ledger-compare", "output.json");
        var missSignalJson = File.Exists(missPath) ? await File.ReadAllTextAsync(missPath, ct).ConfigureAwait(false) : null;

        var unitsPath = Path.Combine(ledgerRunDir, "step-00-atomic-units", "output.json");
        IReadOnlyDictionary<string, AtomicUnit> units = File.Exists(unitsPath)
            ? (JsonSerializer.Deserialize<AtomicUnitFixture>(await File.ReadAllTextAsync(unitsPath, ct).ConfigureAwait(false), Json)?.Units ?? [])
                .ToDictionary(u => u.Id, u => u, StringComparer.Ordinal)
            : new Dictionary<string, AtomicUnit>();

        return new LedgerArtifacts(validated, missSignalJson, units, ledgerRunId);
    }

    // Queue mit gesetzten Aktionen laden (replay-Datei ODER die vom Menschen/UI gefüllte Thread-Queue)
    // → ItemId→Action (nur gesetzte, valide Aktionen; leer/fehlend → null).
    internal static IReadOnlyDictionary<string, string>? LoadQueueActions(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        var queue = JsonSerializer.Deserialize<AdjudicationQueue>(File.ReadAllText(path), Json);
        var actions = queue?.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Action) && AdjudicationActions.IsValid(i.Action))
            .ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
        return actions is { Count: > 0 } ? actions : null;
    }

    internal sealed record AdjudicationContext(string ValidatedPath);
    internal static string ContextPath(RunContext run) => Path.Combine(run.OutputDir("01-ledger"), "adjudication-context.json");
}

// REQUEST: Queue deterministisch bauen, als queue.json materialisieren (UI-fähig!) und ans Gate heben.
[SendsMessage(typeof(AdjudicationReviewRequest))]
[SendsMessage(typeof(AdjudicationGateEmpty))]
internal sealed class AdjudicationGateRequestExecutor(RunContext parentRun) : Executor<LedgerStageOutput>("PipelineAdjudicationRequest")
{
    public override async ValueTask HandleAsync(LedgerStageOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var art = await AdjudicationIo.LoadAsync(input.ValidatedPath, ct).ConfigureAwait(false);
        var queue = LedgerAdjudicationAdapter.Build(art.Validated, art.LedgerRunId, art.MissSignalJson, art.LedgerRunId);

        var outDir = parentRun.OutputDir("01-ledger");
        var queuePath = Path.Combine(outDir, "queue.json");
        await File.WriteAllTextAsync(queuePath, JsonSerializer.Serialize(queue, AdjudicationIo.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(AdjudicationIo.ContextPath(parentRun),
            JsonSerializer.Serialize(new AdjudicationIo.AdjudicationContext(input.ValidatedPath), AdjudicationIo.Json), ct).ConfigureAwait(false);

        var request = new AdjudicationReviewRequest(queue.Items, art.LedgerRunId, queuePath, input.ValidatedPath);
        // R-50: TYP-Routing — leere Queue (perfekter Ledger) ⇒ Marker statt Request (leeres Gate ruft nie).
        if (queue.Items.Count == 0)
        {
            await context.SendMessageAsync(new AdjudicationGateEmpty(request)).ConfigureAwait(false);
            return;
        }
        parentRun.AppendEvent(new { type = "ADJUDICATION_GATE_REQUEST", items = queue.Items.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(request).ConfigureAwait(false);
    }
}

// APPLY: Aktionen deterministisch anwenden (Resolver mit Governance-Fallback) → consumable projizieren.
[SendsMessage(typeof(ConsumableLedgerOutput))]
internal sealed class AdjudicationApplyExecutor(RunContext parentRun) : Executor<AdjudicationReviewResponse>("PipelineAdjudicationApply")
{
    public override async ValueTask HandleAsync(AdjudicationReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var ctx = JsonSerializer.Deserialize<AdjudicationIo.AdjudicationContext>(
            await File.ReadAllTextAsync(AdjudicationIo.ContextPath(parentRun), ct).ConfigureAwait(false), AdjudicationIo.Json)!;
        var art = await AdjudicationIo.LoadAsync(ctx.ValidatedPath, ct).ConfigureAwait(false);

        // Antwort-Aktionen = „aufgezeichnete Entscheide" → Replay-Semantik (fehlend/invalide ⇒ reject, geloggt).
        var (_, consumable, resolution) = AdjudicationResolver.Resolve(
            art.Validated, art.LedgerRunId, art.MissSignalJson, art.LedgerRunId, art.Units,
            new GatePolicy(GatePolicyKind.Replay), resp.Actions);

        var outDir = parentRun.OutputDir("01-ledger");
        var consumablePath = Path.Combine(outDir, "consumable.json");
        await File.WriteAllTextAsync(consumablePath, JsonSerializer.Serialize(consumable, AdjudicationIo.Json), ct).ConfigureAwait(false);

        parentRun.AppendEvent(new
        {
            type = "STAGE_ADJUDICATION_DONE",
            reviewedBy = resp.ReviewedBy,
            claims = consumable!.Claims.Count,
            accepted = resolution.AcceptedCount,
            rejected = resolution.RejectedCount,
            unmatched = resolution.UnmatchedItemIds.Count,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new ConsumableLedgerOutput(consumablePath, consumable.Claims.Count)).ConfigureAwait(false);
    }
}

// REFINE (A10, Schluss der Adjudikationsstufe): per accept_gap/promote_to_claim NEU gemintete Claims
// tragen bewusst generische Platzhalter-Facetten (facetStatus=pending) — dieser Knoten hebt sie über die
// GETEILTE Naht AdjudicationRefine (identisch zur CLI-Bahn ledger-adjudicate-refine) auf Pipeline-Niveau,
// BEVOR die Baseline-Ableitung den Bestand liest. Ohne pending-Claims: reiner Durchreich-Knoten ohne
// LLM-Aufbau (die Assigner-Factory wird gar nicht erst gerufen); Fehler der Zuweisung sind LAUT (fail-fast).
[SendsMessage(typeof(ConsumableLedgerOutput))]
internal sealed class AdjudicationRefineExecutor(RunContext parentRun, Func<RefineAssign> assignFactory, string? transcript)
    : Executor<ConsumableLedgerOutput>("PipelineAdjudicationRefine")
{
    public override async ValueTask HandleAsync(ConsumableLedgerOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var consumable = JsonSerializer.Deserialize<ConsumableLedger>(
            await File.ReadAllTextAsync(input.ConsumablePath, ct).ConfigureAwait(false), AdjudicationIo.Json)
            ?? throw new InvalidOperationException($"ADJUDICATION_REFINE_UNREADABLE: {input.ConsumablePath}");

        if (AdjudicationRefine.CountPending(consumable) == 0)
        {
            parentRun.AppendEvent(new { type = "ADJUDICATION_REFINE_SKIPPED", pending = 0, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(input).ConfigureAwait(false);
            return;
        }

        var result = await AdjudicationRefine.RefineAsync(consumable, assignFactory(), transcript, ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(input.ConsumablePath,
            JsonSerializer.Serialize(result.Consumable, AdjudicationIo.Json), ct).ConfigureAwait(false);

        parentRun.AppendEvent(new
        {
            type = "STAGE_ADJUDICATION_REFINE_DONE",
            pendingBefore = result.PendingBefore,
            refined = result.RefinedCount,
            stillPending = result.StillPending,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(
            new ConsumableLedgerOutput(input.ConsumablePath, result.Consumable.Claims.Count)).ConfigureAwait(false);
    }
}
