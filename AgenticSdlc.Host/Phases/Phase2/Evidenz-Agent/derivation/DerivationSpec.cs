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
    string PromptName,                          // Prompt-Datei (versioniert) — strukturierter Modus (Host füttert Items)
    string ItemIdPrefix,                        // stabiler ID-Präfix der abgeleiteten Items, z. B. "DRISK"
    string? AgenticPromptName = null,           // Prompt für den agentischen Modus (Agent liest/schreibt via Tools); null = kein agentic-Support
    string? AgenticDiagnosticPromptName = null, // Diagnose-Prompt (Narrations-Pflicht, --narrate); NICHT die Mess-Default
    string? AgenticExplorerPromptName = null,   // Explorer-Prompt (Ziel-only, Entdeckungs-Tools, --explore)
    string? AgenticVerifyPromptName = null)      // Verify-Loop-Prompt (Explorer + Selbstkorrektur via verify_derived, --verify)
{
    /// <summary>Unterstützt diese Ableitung den agentischen Modus (Agent nutzt Tools selbst)?</summary>
    public bool SupportsAgentic => !string.IsNullOrWhiteSpace(AgenticPromptName);

    /// <summary>Unterstützt diese Ableitung den Explorer-Modus (Ziel-only, Selbst-Entdeckung der Umwelt)?</summary>
    public bool SupportsExplorer => !string.IsNullOrWhiteSpace(AgenticExplorerPromptName);

    /// <summary>Unterstützt diese Ableitung den Verify-Loop-Modus (Selbstkorrektur gegen eine Definition of Done)?</summary>
    public bool SupportsVerify => !string.IsNullOrWhiteSpace(AgenticVerifyPromptName);

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

            // Multi-Source-Demo (Bau-Punkt 2): Risiken aus requirements + architecture (Zusammenspiel).
            // AgenticPromptName gesetzt → --agentic-Modus verfügbar (Agent liest/schreibt via Tools selbst).
            ["derived-risks-multi"] = new(
                Id: "derived-risks-multi", SourceArtifactTypes: ["requirements", "architecture"], TargetArtifactType: "risks",
                AgentName: "EvidenceDerivedRisksAgent", PromptName: "DerivedRisksFromReqArch1", ItemIdPrefix: "DRISK",
                // v2 = Mess-Default (nennt Drill-down-Tools); v3 = Diagnose (Narrations-Pflicht, via --narrate). v1 bleibt erhalten.
                AgenticPromptName: "DerivedRisksFromReqArchAgentic2",
                AgenticDiagnosticPromptName: "DerivedRisksFromReqArchAgentic3",
                AgenticExplorerPromptName: "DerivedRisksExplorer1",
                // --verify: Explorer + Selbstkorrektur gegen eine Definition of Done (verify_derived im Loop).
                AgenticVerifyPromptName: "DerivedRisksVerifyLoop1"),
        };

    public static bool TryGet(string id, out DerivationSpec spec) => Specs.TryGetValue(id, out spec!);
}
