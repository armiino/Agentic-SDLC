namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// Zentrale Runtimeconfig für den Host.
/// </summary>
/// <remarks>
/// Die Klasse kombiniert die "nicht"-geheime `run-config.json` mit lokalen Umgebungsvariablen aus `.env`.
/// Dadurch bleiben Forschungsparameter wie Phase, Modell und Observability sichtbar,
///  während Secrets weiterhin lokal bleiben
/// </remarks>
public sealed record HostSettings(
    string RepoRoot,
    bool OtelEnabled,
    bool OtelSensitive,
    bool OtelRawEnabled,
    bool AssistantPreviewEnabled,
    int LlmPreviewChars,
    bool LlmPreviewOnlyWhenNoTools,
    string AgentPhase,
    string Phase2ContextStrategy,
    IReadOnlyDictionary<string, string> Prompts,
    string LlmProvider,
    string ModelId,
    string OllamaBaseUrl,
    string OpenRouterBaseUrl,
    string? OpenRouterApiKey)
{
    /// <summary>
    /// Erstellt die Settings aus den aktuell gesetzten Umgebungsvariablen.
    /// </summary>
    public static HostSettings FromRuntimeConfig(RunConfig config, string repoRoot)
    {
        var settings = new HostSettings(
            RepoRoot: repoRoot,
            OtelEnabled: config.Observability.EnableOtel ?? ReadFlag("ENABLE_OTEL"),
            OtelSensitive: config.Observability.EnableOtelSensitive ?? ReadFlag("ENABLE_OTEL_SENSITIVE"),
            OtelRawEnabled: config.Observability.EnableOtelRaw ?? ReadFlag("ENABLE_OTEL_RAW"),
            AssistantPreviewEnabled: config.LlmPreview.Enabled ?? ReadFlag("ENABLE_LLM_ASSISTANT_PREVIEW"),
            LlmPreviewChars: Clamp(config.LlmPreview.Chars ?? ReadInt("LLM_PREVIEW_CHARS", 800), 100, 8000),
            LlmPreviewOnlyWhenNoTools: config.LlmPreview.OnlyWhenNoTools ?? ReadFlag("LLM_PREVIEW_ONLY_WHEN_NO_TOOLS", defaultValue: true),
            AgentPhase: ReadConfigString(config.AgentPhase, "AGENT_PHASE", "phase1").Trim().ToLowerInvariant(),
            Phase2ContextStrategy: ReadConfigString(config.Phase2ContextStrategy, "PHASE2_CONTEXT_STRATEGY", "message_passing").Trim().ToLowerInvariant(),
            Prompts: BuildPromptSelection(config.Prompts),
            LlmProvider: ReadConfigString(config.LlmProvider, "LLM_PROVIDER", "ollama").Trim().ToLowerInvariant(),
            ModelId: ReadConfigString(config.AgentModel, "AGENT_MODEL", "qwen2.5:14b"),
            OllamaBaseUrl: ReadString("OLLAMA_BASE_URL", "http://localhost:11434/"),
            OpenRouterBaseUrl: ReadString("OPENROUTER_BASE_URL", "https://openrouter.ai/api/v1"),
            OpenRouterApiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        );

        settings.ApplyEnvironmentCompatibility();
        return settings;
    }

    /// <summary>
    /// Liefert den in `run-config.json` ausgewählten Promptnamen für einen Agenten.
    /// </summary>
    /// <remarks>
    /// Der Host arbeitet dadurch nicht mehr mit phase-spezifischen Prompt-Properties.
    /// Jeder Runner kennt seine Agentnamen und fragt hier nur noch generisch nach,
    /// welcher Prompt für diesen Agenten aktiv ist.
    /// Fehlt ein Eintrag, wird bewusst früh abgebrochen, damit ein Run nicht
    /// versehentlich mit einem falschen oder nicht dokumentierten Prompt startet.
    /// </remarks>
    public string GetPromptName(string agentName)
    {
        if (Prompts.TryGetValue(agentName, out var promptName) && !string.IsNullOrWhiteSpace(promptName))
            return promptName;

        throw new InvalidOperationException(
            $"No prompt configured for agent '{agentName}'. Add an entry under 'prompts' in run-config.json.");
    }

    /// <summary>
    /// Spiegelt Config-Werte in Prozess-Environment-Variablen, solange einzelne
    /// bestehende Komponenten diese Werte noch direkt aus der Umgebung lesen.
    /// </summary>
    public void ApplyEnvironmentCompatibility()
    {
        Environment.SetEnvironmentVariable("ENABLE_LLM_ASSISTANT_PREVIEW", AssistantPreviewEnabled ? "1" : "0");
        Environment.SetEnvironmentVariable("LLM_PREVIEW_CHARS", LlmPreviewChars.ToString());
        Environment.SetEnvironmentVariable("LLM_PREVIEW_ONLY_WHEN_NO_TOOLS", LlmPreviewOnlyWhenNoTools ? "1" : "0");
    }

    private static bool ReadFlag(string key, bool defaultValue = false)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return value is null
            ? defaultValue
            : string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
              || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadString(string key, string defaultValue)
        => Environment.GetEnvironmentVariable(key) ?? defaultValue;

    private static string ReadConfigString(string? configValue, string envKey, string defaultValue)
        => string.IsNullOrWhiteSpace(configValue) ? ReadString(envKey, defaultValue) : configValue;

    private static IReadOnlyDictionary<string, string> BuildPromptSelection(IDictionary<string, string>? configuredPrompts)
    {
        /*
         * Defaults halten bestehende Runs lauffähig, falls run-config.json noch
         * keinen neuen `prompts`-Block enthält. Die aktive Auswahl wird trotzdem
         * zentral über Agentnamen modelliert und ist damit für spätere Phasen
         * erweiterbar, ohne HostSettings erneut phase-spezifisch zu ändern.
         */
        var prompts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Phase1SinglePass"] = "Phase1_3Prompt",
            ["Phase2ContextAgent"] = "ContextPrompt2",
            ["Phase2RequirementsAgent"] = "RequirementsPrompt4",
            ["Phase2RisksAgent"] = "RisksPrompt2",
            ["Phase2ArchitectureAgent"] = "ArchitecturePrompt2",
            ["Phase2OpenQuestionsAgent"] = "OpenQuestionsPrompt2"
        };

        if (configuredPrompts is null)
            return prompts;

        foreach (var (agentName, promptName) in configuredPrompts)
        {
            if (string.IsNullOrWhiteSpace(agentName) || string.IsNullOrWhiteSpace(promptName))
                continue;

            prompts[agentName.Trim()] = promptName.Trim();
        }

        return prompts;
    }

    private static int ReadInt(string key, int defaultValue)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : defaultValue;

    private static int Clamp(int value, int min, int max)
        => Math.Min(Math.Max(value, min), max);
}
