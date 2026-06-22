namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

// V1-Vertrag (neutrales Datenmodell, kein Verhalten) — siehe NextStep/ReviewResult-vertrag.md.

/// <summary>Achse, auf der ein Defekt gefunden wurde.</summary>
public enum ReviewAxis
{
    /// <summary>Artefakt → Quelle: Ist eine Artefaktaussage belegt?</summary>
    Grounding,
    /// <summary>Quelle/Topic/Evidence → Artefakt: Fehlt ein relevantes Thema?</summary>
    Coverage,
    /// <summary>Stimmen Quellenstatus und Artefaktstatus überein (offen vs. entschieden)?</summary>
    Certainty,
    /// <summary>Deterministische/technische Probleme: leeres Artefakt, Parsefehler, Review fehlgeschlagen.</summary>
    Validity
}

/// <summary>Geschlossenes Severity-Vokabular (steuert die Gewichtung) — kein freier String.</summary>
public enum DefectSeverity
{
    Info,       // nicht blockierend
    Low,        // = Legacy GERING
    Medium,     // = Legacy MITTEL
    Critical    // = Legacy KRITISCH
}

/// <summary>Verifikationsstand eines einzelnen Defekt-Kandidaten.</summary>
public enum VerificationStatus
{
    Confirmed,
    Partial,
    Rejected,
    Unverified,
    Failed
}

/// <summary>Lief die Review technisch durch? (Nicht zu verwechseln mit der Gate-Entscheidung.)</summary>
public enum ReviewStatus
{
    Succeeded,
    Partial,
    Failed
}

/// <summary>Was soll der Workflow mit dem Artefakt tun? (Von der GatePolicy berechnet.)</summary>
public enum GateDecision
{
    Pass,
    PassWithWarnings,
    Repair,
    HumanReview,
    Failed
}
