namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// Beschreibt EINE Ableitung der config-gesteuerten Derivation-Familie: „aus Quelle X leite Ziel Y mit Prompt Z ab".
/// Damit sind DerivedRisks, RequirementsGap, UserStories … nur noch Einträge — gleicher Mechanismus (Generieren →
/// Anker-Check → Inference-Check), verschiedene Prompts/Ziele (Bewertung §3.8: gemeinsamer Contract + domänen-
/// spezifische Agenten, KEIN Allmacht-Agent).
/// </summary>
public sealed record DerivationSpec(
    string Id,                 // CLI-Selektor, z. B. "derived-risks"
    string SourceArtifactType, // erwarteter Quell-Artefakttyp (Doku/Guard), z. B. "requirements"
    string TargetArtifactType, // Ziel-Typ des erzeugten ArtifactDocument, z. B. "risks"
    string AgentName,          // Prompt-Ordner unter Prompts/phase2_evidence/
    string PromptName,         // Prompt-Datei (versioniert)
    string ItemIdPrefix);      // stabiler ID-Präfix der abgeleiteten Items, z. B. "DRISK"

/// <summary>Registry der verfügbaren Ableitungen. Code-Default; per CLI-Id gewählt. Additiv erweiterbar (neue Zeile
/// + Prompt-Datei = neue Ableitung, ohne Mechanismus-Änderung).</summary>
public static class DerivationRegistry
{
    public static readonly IReadOnlyDictionary<string, DerivationSpec> Specs =
        new Dictionary<string, DerivationSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["derived-risks"] = new(
                Id: "derived-risks", SourceArtifactType: "requirements", TargetArtifactType: "risks",
                AgentName: "EvidenceDerivedRisksAgent", PromptName: "DerivedRisksFromRequirements1", ItemIdPrefix: "DRISK"),

            ["requirements-gap"] = new(
                Id: "requirements-gap", SourceArtifactType: "requirements", TargetArtifactType: "requirements",
                AgentName: "EvidenceRequirementsGapAgent", PromptName: "RequirementsGapFromRequirements1", ItemIdPrefix: "DREQ"),
        };

    public static bool TryGet(string id, out DerivationSpec spec) => Specs.TryGetValue(id, out spec!);
}
