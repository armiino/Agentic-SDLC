namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// W1a: Steuert, ob agentische/Judge-Knoten ein <c>reasoning</c>-Feld (model-declared Begründung) emittieren.
/// </summary>
/// <remarks>
/// reasoning ist LOG-ONLY (landet im Response-Text/<c>response-text.md</c>), wird NIE in die Wahrheit
/// persistiert. Der Schalter existiert, weil reasoning Output-Tokens kostet und das Verhalten leicht
/// verschieben kann → für saubere Baseline-/Effizienz-Läufe (Messkontrakt-Härtung M-1) muss es je Lauf
/// abschaltbar sein. Gesteuert über run-config: <c>observability.captureReasoning</c>.
/// </remarks>
public enum ReasoningCapture
{
    /// <summary>Kein reasoning-Feld im Schema. 0 Extra-Tokens, exakt Baseline-vergleichbar.</summary>
    Off,

    /// <summary>reasoning im Schema, aber NICHT required — Modell darf, muss nicht (weich).</summary>
    Optional,

    /// <summary>reasoning im Schema + required — mit Structured Output garantiert (Default).</summary>
    Enforced
}

public static class ReasoningCaptureParser
{
    public static ReasoningCapture Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "off" or "none" or "false" or "0" => ReasoningCapture.Off,
        "optional" => ReasoningCapture.Optional,
        _ => ReasoningCapture.Enforced // Default (inkl. null/leer/"enforced"/"on"/"true")
    };
}

/// <summary>
/// EIN Muster für den reasoning-Rollout über alle Knoten: dieselben Fragmente werden je Knoten in
/// Schema-Properties, <c>required</c>-Liste, Prompt-Regel und Beispiel-JSON eingesetzt. So bleibt der
/// Rollout mechanisch und divergiert nicht. Referenz-Implementierung: <c>FacetAssigner</c>.
/// </summary>
/// <remarks>
/// Einsatz im Schema-Baustring (raw-interpolated <c>$$"""..."""</c>):
/// <code>
/// "properties": { {{ReasoningSchema.PropertyJson(mode)}}"id": { "type":"string" }, ... },
/// "required": [{{ReasoningSchema.RequiredToken(mode)}}"id", ...]
/// </code>
/// und im Prompt-Template über Marker-Replace (<c>%%REASONING_RULE%%</c>, <c>%%REASONING_EXAMPLE%%</c>).
/// JSON ist whitespace-insensitiv → die Fragmente müssen nur gültigen Inhalt liefern, keine Einrückung.
/// </remarks>
public static class ReasoningSchema
{
    /// <summary>JSON-Property-Zeile, an den ANFANG der Item-<c>properties</c> gesetzt. Leer bei Off.</summary>
    public static string PropertyJson(ReasoningCapture mode)
        => mode == ReasoningCapture.Off ? "" : "\"reasoning\": { \"type\": \"string\" }, ";

    /// <summary>Token für den ANFANG der <c>required</c>-Liste. Nur bei Enforced, sonst leer.</summary>
    public static string RequiredToken(ReasoningCapture mode)
        => mode == ReasoningCapture.Enforced ? "\"reasoning\"," : "";

    /// <summary>Prompt-Regelzeile (in ein <c>%%REASONING_RULE%%</c>-Marker gesetzt). Leer bei Off.</summary>
    public static string PromptRule(ReasoningCapture mode)
        => mode == ReasoningCapture.Off
            ? ""
            : "- reasoning: EIN kurzer deutscher Satz, der die Zuweisung begründet — fülle ihn ZUERST, BEVOR du festlegst (nachvollziehbar; wird nur protokolliert).";

    /// <summary>Beispiel-JSON-Feld (in ein <c>%%REASONING_EXAMPLE%%</c>-Marker gesetzt). Leer bei Off.</summary>
    public static string PromptExampleField(ReasoningCapture mode)
        => mode == ReasoningCapture.Off ? "" : "\"reasoning\": \"<kurze deutsche Begründung, ZUERST>\",";

    /// <summary>
    /// Prompt-ANHANG (ans Ende des System-Prompts), damit heterogene Prompts NICHT chirurgisch editiert
    /// werden müssen — der Rollout-Standard. Leer bei Off. SPRACHE (Pflicht): Deutsch.
    /// </summary>
    public static string PromptAppendix(ReasoningCapture mode)
        => mode == ReasoningCapture.Off
            ? ""
            : "\n\nZUSATZ (nur Protokoll, KEINE Fachänderung): Gib je Ergebnis-Objekt zusätzlich ein Feld "
              + "\"reasoning\" aus — EIN kurzer deutscher Satz, der die Entscheidung begründet, ZUERST vor den "
              + "übrigen Feldern. Wird ausschließlich geloggt, nie weiterverarbeitet.";
}
