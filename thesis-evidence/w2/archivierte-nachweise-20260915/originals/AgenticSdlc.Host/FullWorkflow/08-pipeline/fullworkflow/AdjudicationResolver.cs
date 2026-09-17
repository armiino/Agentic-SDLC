using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

public enum AdjudicationOutcome { Pause, Resolved }

/// <summary>Ergebnis der Aktions-Auflösung (die Queue-Items mit gesetzter Action) — vor der Projektion.</summary>
public sealed record AdjudicationActionResolution(
    AdjudicationOutcome Outcome,
    IReadOnlyList<AdjudicationItem> FilledItems,
    int AcceptedCount,
    int RejectedCount,
    IReadOnlyList<string> UnmatchedItemIds);

/// <summary>
/// W1e' Schritt 3 — der Adjudikations-Gate-Kern: Policy → gefüllte Queue → consumable Projektion.
/// </summary>
/// <remarks>
/// Adjudikation ist NICHT nur accept/reject (wie Ingest/Pbi), sondern hat pro Item eine spezifische
/// <see cref="AdjudicationActions"/>-Aktion (apply_repair, accept_gap, attach_evidence, reject …). Daher
/// ein eigener Resolver statt des generischen <see cref="GateResponder"/>:
/// <list type="bullet">
///   <item><b>interactive</b> → <see cref="AdjudicationOutcome.Pause"/> (RequestPort, Schritt 4).</item>
///   <item><b>replay</b> → aufgezeichnete Aktion je ItemId einspielen; <b>unmatcht ⇒ reject</b> (dokumentiert).
///     Der saubere N≥3-Pfad: keine Aktions-Inferenz, nur die realen Autor-Entscheide des E2E.</item>
///   <item><b>accept-all</b> → EXPERIMENT-Heuristik: dem System-Vorschlag folgen; nicht auto-entscheidbares
///     (needs_human, kein Vorschlag) ⇒ reject (sicher). KEIN Ersatz für echte Adjudikation.</item>
/// </list>
/// <see cref="ResolveActions"/> ist rein (nur Items) und voll unit-testbar; <see cref="Resolve"/> verkettet
/// Queue-Bau (<see cref="LedgerAdjudicationAdapter.Build"/>) und Projektion
/// (<see cref="AdjudicationCompletenessGate.Project"/>) — beides bestehende, erprobte Bausteine.
/// </remarks>
public static class AdjudicationResolver
{
    // Claim-erzeugende Aktionen = "akzeptiert"; merge/covered/reject/defer fügen keinen Claim hinzu.
    private static bool IsAccept(string? action) =>
        action is AdjudicationActions.AcceptGap or AdjudicationActions.PromoteToClaim
               or AdjudicationActions.AttachEvidence or AdjudicationActions.ApplyRepair;

    public static AdjudicationActionResolution ResolveActions(
        IReadOnlyList<AdjudicationItem> items,
        GatePolicy policy,
        IReadOnlyDictionary<string, string>? recordedActions = null)
    {
        if (policy.Kind == GatePolicyKind.Interactive)
            return new AdjudicationActionResolution(AdjudicationOutcome.Pause, [], 0, 0, []);

        var unmatched = new List<string>();
        var filled = items.Select(item =>
        {
            string action;
            if (policy.Kind == GatePolicyKind.AcceptAll)
            {
                action = AcceptAllAction(item);
            }
            else // Replay
            {
                if (recordedActions is not null
                    && recordedActions.TryGetValue(item.ItemId, out var a)
                    && AdjudicationActions.IsValid(a))
                {
                    action = a!;
                }
                else
                {
                    unmatched.Add(item.ItemId);
                    action = AdjudicationActions.Reject; // Governance-sicherer Fallback (kein stiller Durchrutscher).
                }
            }
            return item with { Action = action, ActionReason = item.ActionReason ?? $"pipeline-full/{policy.Kind}" };
        }).ToList();

        var accepted = filled.Count(i => IsAccept(i.Action));
        return new AdjudicationActionResolution(
            AdjudicationOutcome.Resolved, filled, accepted, filled.Count - accepted, unmatched);
    }

    // accept-all EXPERIMENT-Heuristik: System-Vorschlag → Aktion; sonst reject.
    private static string AcceptAllAction(AdjudicationItem item)
    {
        var s = item.SystemSuggestion;
        if (s is null)
            return string.Equals(item.ItemType, "review_required_claim", StringComparison.Ordinal)
                ? AdjudicationActions.AcceptGap   // review_required als-ist übernehmen
                : AdjudicationActions.Reject;

        return s.Kind switch
        {
            "facet_repair" => AdjudicationActions.ApplyRepair,
            "compare_classification" => s.Classification switch
            {
                "missing_claim" => AdjudicationActions.AcceptGap,
                "attach_as_evidence" => AdjudicationActions.AttachEvidence,
                "already_covered_indirectly" => AdjudicationActions.MarkCoveredBy,
                _ => AdjudicationActions.Reject, // needs_human u. Ä. → nicht auto-entscheidbar
            },
            _ => AdjudicationActions.Reject,
        };
    }

    /// <summary>Integration (Graph/CLI): Queue bauen → Aktionen auflösen → consumable projizieren.</summary>
    public static (AdjudicationOutcome Outcome, ConsumableLedger? Consumable, AdjudicationActionResolution Resolution) Resolve(
        ValidatedLedger validated,
        string? validatedRunId,
        string? missSignalJson,
        string? unitRunId,
        IReadOnlyDictionary<string, AtomicUnit>? units,
        GatePolicy policy,
        IReadOnlyDictionary<string, string>? recordedActions = null)
    {
        var queue = LedgerAdjudicationAdapter.Build(validated, validatedRunId, missSignalJson, unitRunId);
        var resolution = ResolveActions(queue.Items, policy, recordedActions);
        if (resolution.Outcome == AdjudicationOutcome.Pause)
            return (AdjudicationOutcome.Pause, null, resolution);

        var filled = queue with { Items = resolution.FilledItems };
        var (_, consumable) = AdjudicationCompletenessGate.Project(filled, validated, units);
        return (AdjudicationOutcome.Resolved, consumable, resolution);
    }
}
