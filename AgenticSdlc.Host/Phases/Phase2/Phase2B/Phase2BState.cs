namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Konstanten und Typen für den MAF-nativen Shared State der Phase-2.1B-Strategie (`artifact_state`).
/// </summary>
/// <remarks>
/// Transport zwischen den Executors läuft über den MAF-Shared-State
/// (<c>IWorkflowContext.QueueStateUpdateAsync</c> / <c>ReadStateAsync</c>) im Scope
/// <see cref="Scope"/>. Die Inter-Executor-Message (<see cref="Phase2BHandoff"/>) trägt nur
/// Referenzen/Signal; die Nutzlast liegt ausschließlich im State (idiomatisches MAF-Muster,
/// vgl. offizielles SharedStates-Sample: FileRead schreibt State, sendet fileID).
/// </remarks>
public static class Phase2BState
{
    /// <summary>Gemeinsamer State-Scope. Gleicher non-null Scope bei Write+Read → geteilt sichtbar.</summary>
    public const string Scope = "Phase2BProjectState";

    public const string ContextKey = "context";
    public const string RequirementsKey = "requirements";
    public const string RisksKey = "risks";
    public const string ArchitectureKey = "architecture";
    public const string OpenQuestionsKey = "open-questions";

    /// <summary>Lesbares Label eines State-Keys für die in die Agent-Eingabe injizierte Nachricht.</summary>
    public static string LabelForKey(string key) => key switch
    {
        ContextKey => "Projektkontext",
        RequirementsKey => "Artefakt: docs/requirements.md",
        RisksKey => "Artefakt: docs/risks.md",
        ArchitectureKey => "Artefakt: docs/architecture.md",
        OpenQuestionsKey => "Artefakt: docs/open-questions.md",
        _ => $"State-Key: {key}"
    };
}

/// <summary>
/// Typisierte Trigger-Message zwischen zwei Phase-2.1B-Executors.
/// </summary>
/// <param name="AvailableKeys">
/// State-Keys, die zu diesem Zeitpunkt im Shared State verfügbar sind (rein informativ/für Logs).
/// Die eigentliche Nutzlast liegt im Shared State, nicht in dieser Message.
/// </param>
public sealed record Phase2BHandoff(IReadOnlyList<string> AvailableKeys);
