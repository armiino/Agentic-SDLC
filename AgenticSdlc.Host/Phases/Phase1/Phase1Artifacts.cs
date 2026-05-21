namespace AgenticSdlc.Host.Phases.Phase1;

/// <summary>
/// Zentrale Phase-1-Konstanten für Pflichtartefakte und Review-Aktion.
/// </summary>
/// <remarks>
/// Diese Werte werden von mehreren Phase-1-Bausteinen verwendet: Prompt, Validierung und Approval.
/// Die Klasse verhindert, dass dieselben Pfade und Aktionsnamen an mehreren Stellen voneinander abweichen.
/// </remarks>
public static class Phase1Artifacts
{
    public const string PhaseName = "Phase1-Level1-SINGLE_PASS";
    public const string ApprovalAction = "phase1_review";

    public static readonly string[] RequiredDocs =
    [
        "docs/requirements.md",
        "docs/open-questions.md",
        "docs/risks.md",
        "docs/architecture.md"
    ];
}
