using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// C4-Kreislauf (⚖ Autor 22.08.): die ASPEKT-Färbung einer Unklarheit — generisches, ADDITIVES Metadatum
/// an DECs („diese offene Entscheidung ist eine ARCHITEKTUR-Unklarheit"), gesetzt von der Quelle, die es
/// weiß (Steward-Einkipp, Analyst-arch-Linse). GELESEN wird es nur von drei additiven Konsumenten:
/// §3-Projektion im C4 (<see cref="C4GapSection"/>), Steward-Klär-Angebot, dritte Auflöse-Option.
/// Alle übrigen DEC-Pfade (decision-gate, Parkplatz, Kangal, Auflösungs-Maschine) lesen es NIE —
/// eine DEC ohne Färbung verhält sich byte-identisch zu vorher (Neutralitäts-Pin).
/// </summary>
public static class DecisionAspectMeta
{
    public const string Key = "aspect";
    public const string Architecture = "architecture";

    public static bool IsArchitecture(ProjectStateItem item)
        => string.Equals(item.Metadata.GetValueOrDefault(Key), Architecture, StringComparison.OrdinalIgnoreCase);

    /// <summary>No-op für ungefärbte Items — geteilte Carry-Naht (Muster GithubOriginMeta).</summary>
    public static void CarryOver(ProjectStateItem incoming, Dictionary<string, string> meta)
    {
        if (incoming.Metadata.TryGetValue(Key, out var v) && !string.IsNullOrWhiteSpace(v)) meta[Key] = v;
    }
}

/// <summary>
/// Der ANTWORT-ANKER des Kreislaufs: ein Wahrheits-Item (ARCH/REQ), das aus der Beantwortung einer DEC
/// entstand, trägt `answersDecision: DEC-x` — gesetzt am Antwort-Diktat (`decisionRef`, Muster
/// githubIssueNumber), getragen durch den gated Tor-1-Apply. Damit ist DETERMINISTISCH prüfbar, ob eine
/// geschlossene Architektur-Unklarheit Wahrheit erzeugt hat (§3: saubere Schließung vs. ⚠-Zeile).
/// </summary>
public static class DecisionAnswerMeta
{
    public const string Key = "answersDecision";
    /// <summary>Stempel an der DEC selbst: „bewusst OHNE Festlegung geschlossen" (NO_TRUTH_NEEDED) —
    /// die §3-Projektion unterscheidet damit sauberen Verzicht von ⚠ „nur weggeredet".</summary>
    public const string WaivedKey = "resolvedWithoutTruthNeeded";

    public static string? Of(ProjectStateItem item) => item.Metadata.GetValueOrDefault(Key);

    public static void CarryOver(ProjectStateItem incoming, Dictionary<string, string> meta)
    {
        if (incoming.Metadata.TryGetValue(Key, out var v) && !string.IsNullOrWhiteSpace(v)) meta[Key] = v;
    }

    /// <summary>Existiert AKTIVE Wahrheit, die diese DEC beantwortet?</summary>
    public static bool HasAnsweringTruth(ProjectStateDocument core, string decisionId)
        => core.Items.Any(i => string.Equals(Of(i), decisionId, StringComparison.OrdinalIgnoreCase)
                               && i.ReadStatus().Validity == Validity.Active);
}

/// <summary>Ziel-Anker einer DEC (Widerspruchs-/Ziel-Semantik der Derivation): auf WELCHES Wahrheits-Item
/// sich die Entscheidung bezieht. Kleinvieh-Hygiene 23.08.: vorher als Magic-String an ~10 Lese-/Schreib-
/// Stellen — jetzt EINE Quelle (die JSON-Vertrags-Attribute in den Modellen bleiben bewusst Literale).</summary>
public static class DecisionTargetMeta
{
    public const string Key = "targetEntityId";
}
