using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow;

// Adapter-Basis Welle 2 (2026-07-22): das Merge-Geruest der Review-Adapter — existierte strukturgleich in
// 13 Adaptern: letzte Entscheidung je Key gewinnt (GroupBy→Last), Item per ItemId finden, Felder setzen
// (STUFEN-Callback — von simplen Decision/Reason-Sets bis edit-JSON-Deserialisierung), Resolved neu berechnen.
public static class ReviewMerge
{
    public static void ByItemId<TDecision>(
        ReviewSession session, IEnumerable<TDecision>? decisions,
        Func<TDecision, string?> keyOf,
        Action<ReviewItem, TDecision> applyFields,
        Func<ReviewItem, bool> resolved)
    {
        if (decisions is null) return;
        var byId = decisions
            .Where(d => !string.IsNullOrWhiteSpace(keyOf(d)))
            .GroupBy(d => keyOf(d)!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        foreach (var item in session.Items)
        {
            if (!byId.TryGetValue(item.ItemId, out var d)) continue;
            applyFields(item, d);
            item.Resolved = resolved(item);
        }
    }
}
