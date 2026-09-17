using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow;

// Adapter-Basis Welle 1 (2026-07-22): die Feld-Zugriffs-Mechanik der Review-Adapter — existierte als
// identische private Kopie in 13 Adaptern. Of() trimmt bewusst (Entscheidungen wie "apply " zaehlen).
// Adapter-Basis Welle 2 (2026-08-03, E0.9): die restlichen mechanischen Kopien der LEBENDEN Adapter
// hierher gehoben — Text-Kuerzung (zwei bewusst getrennte Varianten, bit-identisch zu den Alt-Kopien)
// und die P2a-Resolved-Regel (Ablehnen braucht Begruendung). Dormante Alt-L4-Adapter bleiben eingefroren;
// die Adjudikations-FieldOf bleibt bewusst eigen (anderer Vertrag: nullable, ungetrimmt).
public static class ReviewFields
{
    public static string Of(ReviewItem item, string key)
        => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";

    public static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    /// <summary>Whitespace normalisieren (Zeilenumbrueche/Mehrfach-Spaces zu einem Space), NICHTS abschneiden.</summary>
    public static string OneLine(string? value)
        => string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    /// <summary>Kuerzung Variante A: erst <see cref="OneLine"/>, dann bei Ueberlaenge mit "..." abschneiden.</summary>
    public static string TruncateOneLine(string? value, int max)
    {
        var normalized = OneLine(value);
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }

    /// <summary>Kuerzung Variante B: roh (Zeilenumbrueche bleiben), bei Ueberlaenge mit " …" abschneiden.</summary>
    public static string TruncateRaw(string value, int max)
        => value.Length <= max ? value : value[..max] + " …";

    /// <summary>E0.9-P2a als EINE Regel: Entscheid gueltig UND — falls er der ablehnende ist — Begruendung vorhanden.
    /// (Ein apply erklaert sich durch den Vorschlag selbst, ein skip nie.)</summary>
    public static bool ResolvedRequiringReason(ReviewItem item, HashSet<string> validDecisions,
        string decisionField, string reasonField, string decisionRequiringReason)
    {
        var decision = Of(item, decisionField);
        if (!validDecisions.Contains(decision)) return false;
        if (string.Equals(decision, decisionRequiringReason, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(Of(item, reasonField))) return false;
        return true;
    }
}
