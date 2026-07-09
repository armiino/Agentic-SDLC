namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// Beschreibt EINE Ableitung der config-gesteuerten Derivation-Familie: „aus Quelle(n) X leite Ziel Y mit Prompt Z ab".
/// Damit sind DerivedRisks, RequirementsGap, UserStories … nur noch Einträge — gleicher Mechanismus (Generieren →
/// Anker-Check → Inference-Check), verschiedene Prompts/Ziele (Bewertung §3.8: gemeinsamer Contract + domänen-
/// spezifische Agenten, KEIN Allmacht-Agent).
/// <para>MULTI-SOURCE: <see cref="SourceArtifactTypes"/> ist eine Liste (1..N). Aus der VEREINIGTEN Item-Menge aller
/// Quellen wird abgeleitet; die Anker-/Inference-Prüfung validiert gegen die Item-IDs ALLER Quellen. Einzelquelle =
/// Liste der Länge 1 (rückwärtskompatibel).</para>
/// </summary>
public sealed record DerivationSpec(
    string Id,                                  // CLI-Selektor, z. B. "derived-risks"
    IReadOnlyList<string> SourceArtifactTypes,  // 1..N Quell-Artefakttypen (Multi-Source); Reihenfolge = Anzeige/Doku
    string TargetArtifactType,                  // Ziel-Typ des erzeugten ArtifactDocument, z. B. "risks"
    string AgentName,                           // Prompt-Ordner unter Prompts/phase2_evidence/
    string PromptName,                          // Prompt-Datei (versioniert)
    string ItemIdPrefix)                        // stabiler ID-Präfix der abgeleiteten Items, z. B. "DRISK"
{
    /// <summary>Primärer (erster) Quelltyp — für Einzelquell-Pfade (Chain-Fan-out, CLI-Hinweise/Guards).</summary>
    public string PrimarySourceArtifactType => SourceArtifactTypes[0];

    /// <summary>Mehr als eine Quelle? Steuert Guards in Runnern, die (noch) nur Einzelquelle unterstützen.</summary>
    public bool IsMultiSource => SourceArtifactTypes.Count > 1;

    /// <summary>Kompakte Quellen-Anzeige, z. B. "requirements+architecture".</summary>
    public string SourceLabel => string.Join("+", SourceArtifactTypes);
}

/// <summary>Registry der verfügbaren Ableitungen. Code-Default; per CLI-Id gewählt. Additiv erweiterbar (neue Zeile
/// + Prompt-Datei = neue Ableitung, ohne Mechanismus-Änderung). Multi-Source = einfach mehrere Quelltypen in der
/// Liste (+ ein Prompt, der die vereinte Item-Menge verarbeitet).</summary>
public static class DerivationRegistry
{
    public static readonly IReadOnlyDictionary<string, DerivationSpec> Specs =
        new Dictionary<string, DerivationSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["derived-risks"] = new(
                Id: "derived-risks", SourceArtifactTypes: ["requirements"], TargetArtifactType: "risks",
                AgentName: "EvidenceDerivedRisksAgent", PromptName: "DerivedRisksFromRequirements1", ItemIdPrefix: "DRISK"),

            ["requirements-gap"] = new(
                Id: "requirements-gap", SourceArtifactTypes: ["requirements"], TargetArtifactType: "requirements",
                AgentName: "EvidenceRequirementsGapAgent", PromptName: "RequirementsGapFromRequirements1", ItemIdPrefix: "DREQ"),
        };

    public static bool TryGet(string id, out DerivationSpec spec) => Specs.TryGetValue(id, out spec!);
}
