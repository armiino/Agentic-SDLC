namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Slice S Teil 2 (⚖ Autor 21.08.): DIE Feld-Naht für Prio + Schätzung eines PBI. Beide leben als
/// Metadata-Felder am PBI-Item (Haus-Muster: dort wohnt auch featureId) — gespeichert ENGLISCH
/// (Enum-Regel), angezeigt DEUTSCH. EINE Quelle für Wertebereich, Validierung und Anzeige; Konsumenten:
/// pbi-update-Gate/-Apply, Drafting-Vorschlag, Backlog-Tabelle, Issue-Labels/-Footer, Steward-Seil.
/// </summary>
public static class PbiFields
{
    public const string MetaPriority = "priority";
    public const string MetaEstimate = "estimate";

    public static readonly IReadOnlyList<string> Priorities = ["high", "medium", "low"];
    public static readonly IReadOnlyList<string> Estimates = ["S", "M", "L"];

    /// <summary>Normalisiert eine Prio-Eingabe (auch deutsche Sprech-Werte) auf den Speicher-Wert — null wenn ungültig.</summary>
    public static string? NormalizePriority(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "high" or "hoch" => "high",
        "medium" or "mittel" => "medium",
        "low" or "niedrig" => "low",
        _ => null,
    };

    /// <summary>Normalisiert eine Schätzungs-Eingabe (T-Shirt) auf S|M|L — null wenn ungültig.</summary>
    public static string? NormalizeEstimate(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "S" or "M" or "L" => value.Trim().ToUpperInvariant(),
        _ => null,
    };

    /// <summary>Deutsche Anzeige eines gespeicherten Prio-Werts (unbekannt ⇒ Wert selbst, nie Verlust).</summary>
    public static string PriorityDe(string value) => value switch
    {
        "high" => "hoch",
        "medium" => "mittel",
        "low" => "niedrig",
        _ => value,
    };
}
