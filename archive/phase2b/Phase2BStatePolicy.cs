using AgenticSdlc.Host.Configuration;

namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Steuerbare Shared-State-Policy für Phase 2.1B: was geschrieben und von wem gelesen wird.
/// </summary>
/// <remarks>
/// Default = β-full (jeder Specialist liest Context + alle Upstream-Artefakte; alle schreiben ihr
/// Artefakt in den State). Über <c>run-config.json → phase2BState</c> umschaltbar (kein Recompile).
/// Begründung der Variante: siehe <c>Phases/Phase2/Phase2B/IMPLEMENTATION_PLAN.md</c> §0/§4a.
/// Hinweis: Der Context-Key wird IMMER vom ContextStateExecutor geschrieben (Kern von 2.1B,
/// nicht über <see cref="WriteArtifacts"/> abschaltbar).
/// </remarks>
public sealed record Phase2BStatePolicy(
    bool WriteArtifacts,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Reads)
{
    /// <summary>β-full Default: Read-Listen entlang der linearen SDLC-Abhängigkeitskette.</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultReads =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [Phase2AgentFactory.RequirementsAgentName] =
                [Phase2BState.ContextKey],
            [Phase2AgentFactory.RisksAgentName] =
                [Phase2BState.ContextKey, Phase2BState.RequirementsKey],
            [Phase2AgentFactory.ArchitectureAgentName] =
                [Phase2BState.ContextKey, Phase2BState.RequirementsKey, Phase2BState.RisksKey],
            [Phase2AgentFactory.OpenQuestionsAgentName] =
                [Phase2BState.ContextKey, Phase2BState.RequirementsKey, Phase2BState.RisksKey, Phase2BState.ArchitectureKey],
        };

    public static Phase2BStatePolicy Default() => new(WriteArtifacts: true, Reads: DefaultReads);

    /// <summary>Baut die Policy aus den Host-Settings; fehlende Teile fallen auf den β-full-Default zurück.</summary>
    public static Phase2BStatePolicy FromConfig(HostSettings settings) =>
        new(
            WriteArtifacts: settings.Phase2BWriteArtifacts,
            Reads: settings.Phase2BReads ?? DefaultReads);

    /// <summary>Liefert die Read-Keys für einen Agenten (leer, wenn nicht konfiguriert).</summary>
    public IReadOnlyList<string> ReadsFor(string agentName) =>
        Reads.TryGetValue(agentName, out var keys) ? keys : [];
}
