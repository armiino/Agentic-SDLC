namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>Ein Gate-Item, über das entschieden wird (z. B. eine Ingest-Op, ein Adjudikations-Claim).</summary>
public sealed record GateItem(string ItemId, bool NeedsHuman = false);

public enum GateOutcome
{
    /// <summary>Interaktiv: pausieren + Checkpoint, auf den Menschen warten.</summary>
    Pause,
    /// <summary>Aufgelöst ohne Mensch (accept-all oder replay).</summary>
    Resolved
}

/// <summary>Ergebnis der Gate-Auflösung. Bei <see cref="GateOutcome.Pause"/> sind die Listen leer.</summary>
public sealed record GateResolution(
    GateOutcome Outcome,
    IReadOnlyList<string> AcceptedItemIds,
    IReadOnlyList<string> RejectedItemIds,
    IReadOnlyList<string> UnmatchedItemIds)
{
    public static GateResolution Pause { get; } = new(GateOutcome.Pause, [], [], []);
}

/// <summary>
/// W1e' R-25-Kern: der zentrale Gate-Responder. Übersetzt eine <see cref="GatePolicy"/> + die Gate-Items in
/// eine Entscheidung — die EINE Stelle, an der die Steuerbarkeit sitzt (statt in 13 CLI-Runnern).
/// </summary>
/// <remarks>
/// Rein, deterministisch, ohne LLM/Graph — dadurch vollständig unit-testbar (Governance ist Architektur).
/// Policies:
/// <list type="bullet">
///   <item><b>interactive</b> → <see cref="GateOutcome.Pause"/> (der Graph wartet nativ am RequestPort).</item>
///   <item><b>accept-all</b> → alle Items akzeptiert (deklarierter EXPERIMENT-Modus).</item>
///   <item><b>replay</b> → je Item per ItemId gegen die gespeicherten Entscheide matchen; <b>unmatchte Items
///     ⇒ reject</b> (dokumentiert in <see cref="GateResolution.UnmatchedItemIds"/>), damit N≥3 die menschlichen
///     Entscheide konstant hält und kein Item still durchrutscht (Gates sind heilig).</item>
/// </list>
/// Das Laden der <paramref name="replayDecisions"/> aus queue.json/decision-log (je Gate) macht der Aufrufer;
/// hier zählt nur die Auflösungs-Logik.
/// </remarks>
public static class GateResponder
{
    /// <param name="replayDecisions">ItemId → akzeptiert? (nur für <c>replay</c> genutzt).</param>
    public static GateResolution Resolve(
        GatePolicy policy,
        IReadOnlyList<GateItem> items,
        IReadOnlyDictionary<string, bool>? replayDecisions = null)
    {
        switch (policy.Kind)
        {
            case GatePolicyKind.Interactive:
                return GateResolution.Pause;

            case GatePolicyKind.AcceptAll:
                return new GateResolution(GateOutcome.Resolved, items.Select(i => i.ItemId).ToList(), [], []);

            case GatePolicyKind.Replay:
                var accepted = new List<string>();
                var rejected = new List<string>();
                var unmatched = new List<string>();
                var decisions = replayDecisions ?? new Dictionary<string, bool>();
                foreach (var item in items)
                {
                    if (decisions.TryGetValue(item.ItemId, out var accept))
                    {
                        (accept ? accepted : rejected).Add(item.ItemId);
                    }
                    else
                    {
                        // Kein gespeicherter Entscheid → sicherer Governance-Fallback: reject, dokumentiert.
                        unmatched.Add(item.ItemId);
                        rejected.Add(item.ItemId);
                    }
                }
                return new GateResolution(GateOutcome.Resolved, accepted, rejected, unmatched);

            default:
                return GateResolution.Pause;
        }
    }
}
