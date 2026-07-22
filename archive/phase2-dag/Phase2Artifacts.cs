using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2;

/// <summary>
/// Zentrale Konstanten und Pfade für Phase 2.1.
/// </summary>
/// <remarks>
/// Phase 2.1 ist der kleine, explorative Einstieg in MAF-Workflows:
/// ein statischer Workflow mit mehreren Specialist Agents
/// aber noch ohne Repair-Loops, Manager-Agent oder umfangreichen ProjectState.
///
/// Diese Klasse connected die Artefakte, damit Prompts, Workflow, Validation
/// und Approval später dieselben Pfade verwenden und nicht auseinanderlaufen.
/// </remarks>
public static class Phase2Artifacts
{
    /// <summary>
    /// Phasenname, der in Run-Config, Events und OTel-Tags verwendet wird
    /// </summary>
    public const string PhaseName = "Phase2.1";

    /// <summary>
    /// Governance-Aktion für das Host-seitige Approval am Ende eines erfolgreichen Phase-2.1-Runs.
    /// </summary>
    public const string ApprovalAction = "phase2_1_review";

    /// <summary>
    /// Pflichtartefakte, die Phase 2.1 weiterhin erzeugen muss.
    /// </summary>
    /// <remarks>
    /// Die Artefakte bleiben bewusst identisch zu Phase 1, damit der Vergleich
    /// zwischen Single-Agent und Workflow/Executor/(DAG)-Ansatz möglich bleibt.
    /// </remarks>
    public static readonly string[] RequiredDocs =
    [
        "docs/requirements.md",
        "docs/open-questions.md",
        "docs/risks.md",
        "docs/architecture.md"
    ];

    /// <summary>
    /// Run-interner State-Ordner für Phase-2.1-Zwischenergebnisse
    /// </summary>
    public static string StateDir(RunContext run)
        => Path.Combine(run.RunDir, "state");

    /// <summary>
    /// Minimales Kontextartefakt, das der ContextAgent erzeugen soll.
    /// </summary>
    public static string ContextPath(RunContext run)
        => Path.Combine(StateDir(run), "context.md");

    /// <summary>
    /// Run-interner Ordner für deterministische Validation-Reports
    /// </summary>
    public static string ValidationDir(RunContext run)
        => Path.Combine(run.RunDir, "validation");

    /// <summary>
    /// Minimaler Phase-2.1-Validation-Report
    /// </summary>
    public static string ValidationReportPath(RunContext run)
        => Path.Combine(ValidationDir(run), "phase2_1.report.json");
}
