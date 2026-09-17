using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>Signatur der Facetten-Zuweisung für pending-Claims (produktiv: <see cref="FacetAssigner.AssignAllAsync"/>).</summary>
public delegate Task<IReadOnlyList<SemanticLedgerEntry>> RefineAssign(
    IReadOnlyList<SemanticLedgerEntry> pendingClaims, string? transcript, CancellationToken ct);

/// <summary>Ergebnis eines Refine-Durchgangs: aktualisierter Consumable + Zählwerte für Event/Console.</summary>
public sealed record AdjudicationRefineResult(
    ConsumableLedger Consumable, int PendingBefore, int RefinedCount, int StillPending);

/// <summary>
/// A10-Kern — die EINE geteilte Naht der Facettenvervollständigung (Zwei-Bahnen-Bauregel):
/// CLI-Bahn <c>ledger-adjudicate-refine</c> UND Ein-Graph-Knoten <c>PipelineAdjudicationRefine</c>
/// heben per Adjudikation NEU gemintete Claims (facetStatus=pending) auf Pipeline-Niveau.
/// Der pending-Filter ist DETERMINISTISCH: ohne pending-Claims wird die Zuweisung nicht
/// aufgerufen (keine LLM-Kosten), bereits facettierte Claims bleiben referenzidentisch erhalten.
/// </summary>
public static class AdjudicationRefine
{
    public static int CountPending(ConsumableLedger consumable) => consumable.Claims.Count(IsPending);

    public static async Task<AdjudicationRefineResult> RefineAsync(
        ConsumableLedger consumable, RefineAssign assign, string? transcript, CancellationToken ct)
    {
        var claims = consumable.Claims.ToList();
        var pending = claims.Select((c, i) => (Claim: c, Index: i)).Where(x => IsPending(x.Claim)).ToList();
        if (pending.Count == 0)
            return new AdjudicationRefineResult(consumable, PendingBefore: 0, RefinedCount: 0, StillPending: 0);

        var refined = await assign([.. pending.Select(x => x.Claim)], transcript, ct).ConfigureAwait(false);
        var refinedById = refined.ToDictionary(r => r.Id, r => r, StringComparer.Ordinal);
        foreach (var (claim, index) in pending)
            if (refinedById.TryGetValue(claim.Id, out var r))
                claims[index] = r;

        // ⚠ ConsumableLedger.PendingCount bleibt UNVERÄNDERT: das Feld zählt beim Erzeuger
        // (AdjudicationCompletenessGate.Project) die VERTAGTEN Adjudikationspositionen (defer) —
        // NICHT ausstehende Facetten. Facetten-Stände werden ausschließlich über
        // PendingBefore/StillPending dieses Ergebnisses ausgewiesen (Kollegen-Fund 09.09.).
        var stillPending = claims.Count(IsPending);
        var updated = consumable with
        {
            Claims = claims,
            Note = consumable.Note + " | A10-refine: neue Claims facettiert (facetStatus gelöscht).",
        };
        return new AdjudicationRefineResult(updated, pending.Count, refined.Count, stillPending);
    }

    private static bool IsPending(SemanticLedgerEntry c) =>
        string.Equals(c.FacetStatus, "pending", StringComparison.OrdinalIgnoreCase);
}
