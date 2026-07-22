using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent;

// Adapter-Basis Welle 1 (2026-07-22): die Feld-Zugriffs-Mechanik der Review-Adapter — existierte als
// identische private Kopie in 13 Adaptern. Of() trimmt bewusst (Entscheidungen wie "apply " zaehlen).
public static class ReviewFields
{
    public static string Of(ReviewItem item, string key)
        => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";

    public static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }
}
