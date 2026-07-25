using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' Schritt 4 — Graph-Knoten der Adjudikations-Stufe (der EINE neue Gate). Nimmt den
/// <see cref="LedgerStageOutput"/>, löst die Autor-Entscheide über den <see cref="AdjudicationResolver"/> nach
/// der Gate-Policy auf und hebt den <see cref="ConsumableLedgerOutput"/> in den Graphen.
/// </summary>
/// <remarks>
/// Automatik-Pfad (accept-all/replay) resolved OHNE Pause — für unbeaufsichtigte Messläufe. Der
/// <b>interactive</b>-Pfad (RequestPort-Pause + Resume) wird als eigener Slice ergänzt; hier liefert er
/// vorerst einen terminalen Hinweis. Die Roh-Bausteine (Queue-Bau, Projektion) sind bestehend/erprobt; NEU
/// ist nur die Policy-Auflösung (im Resolver, separat unit-getestet). Consumable → Faden-Ordner
/// <c>01-ledger/consumable.json</c> (Design-Note §2: Adjudikation gehört zum 01-ledger-Ordner).
/// </remarks>
[SendsMessage(typeof(ConsumableLedgerOutput))]
internal sealed class AdjudicationStageExecutor(RunContext parentRun, GatePolicy policy)
    : Executor<LedgerStageOutput>("PipelineAdjudication")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(LedgerStageOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        // Ledger-Run-Dir aus ValidatedPath ableiten: <ledgerRun>/step-03-facet-validation/output.json
        var stepDir = Path.GetDirectoryName(input.ValidatedPath)!;
        var ledgerRunDir = Directory.GetParent(stepDir)!.FullName;
        var ledgerRunId = Path.GetFileName(ledgerRunDir);

        var validated = JsonSerializer.Deserialize<ValidatedLedger>(
            await File.ReadAllTextAsync(input.ValidatedPath, ct).ConfigureAwait(false), Json) ?? new ValidatedLedger([]);

        var missPath = Path.Combine(ledgerRunDir, "step-01d-unused-unit-ledger-compare", "output.json");
        var missSignalJson = File.Exists(missPath) ? await File.ReadAllTextAsync(missPath, ct).ConfigureAwait(false) : null;

        var unitsPath = Path.Combine(ledgerRunDir, "step-00-atomic-units", "output.json");
        IReadOnlyDictionary<string, AtomicUnit> units = File.Exists(unitsPath)
            ? (JsonSerializer.Deserialize<AtomicUnitFixture>(await File.ReadAllTextAsync(unitsPath, ct).ConfigureAwait(false), Json)?.Units ?? [])
                .ToDictionary(u => u.Id, u => u, StringComparer.Ordinal)
            : new Dictionary<string, AtomicUnit>();

        var recorded = LoadRecordedActions(policy);
        var (outcome, consumable, resolution) = AdjudicationResolver.Resolve(
            validated, ledgerRunId, missSignalJson, ledgerRunId, units, policy, recorded);

        if (outcome == AdjudicationOutcome.Pause)
        {
            parentRun.AppendEvent(new { type = "STAGE_ADJUDICATION_PAUSE", policy = policy.Kind.ToString(), timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(
                "Adjudikations-Gate: interactive-Pause noch nicht im Graph verdrahtet (RequestPort-Slice folgt).")
                .ConfigureAwait(false);
            return;
        }

        var outDir = parentRun.OutputDir("01-ledger");
        var consumablePath = Path.Combine(outDir, "consumable.json");
        await File.WriteAllTextAsync(consumablePath, JsonSerializer.Serialize(consumable, Json), ct).ConfigureAwait(false);

        parentRun.AppendEvent(new
        {
            type = "STAGE_ADJUDICATION_DONE",
            policy = policy.Kind.ToString(),
            claims = consumable!.Claims.Count,
            accepted = resolution.AcceptedCount,
            rejected = resolution.RejectedCount,
            unmatched = resolution.UnmatchedItemIds.Count,
            timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(new ConsumableLedgerOutput(consumablePath, consumable.Claims.Count)).ConfigureAwait(false);
    }

    // replay: aufgezeichnete Queue (queue.json mit gesetzten Actions) laden → ItemId→Action. Sonst null.
    private static IReadOnlyDictionary<string, string>? LoadRecordedActions(GatePolicy policy)
    {
        if (policy.Kind != GatePolicyKind.Replay || string.IsNullOrWhiteSpace(policy.ReplayPath) || !File.Exists(policy.ReplayPath))
            return null;
        var queue = JsonSerializer.Deserialize<AdjudicationQueue>(File.ReadAllText(policy.ReplayPath), Json);
        return queue?.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Action))
            .ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
    }
}
