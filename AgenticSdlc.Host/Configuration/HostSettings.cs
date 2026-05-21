namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// Zentrale Runtimeconfig für den Host.
/// </summary>
/// <remarks>
/// Die Klasse liest ausschließlich Umgebungsvariablen, die vorher aus der lokalen .env-Datei geladen wurden. 
/// Sie sammelt nur technische Hostsettings wie LLM-Provider, Modell und Observability-Flags.. 
/// </remarks>
public sealed record HostSettings(
    bool OtelEnabled,
    bool OtelSensitive,
    bool OtelRawEnabled,
    string LlmProvider,
    string ModelId,
    string OllamaBaseUrl,
    string OpenRouterBaseUrl,
    string? OpenRouterApiKey)
{
    /// <summary>
    /// Erstellt die Settings aus den aktuell gesetzten Umgebungsvariablen.
    /// </summary>
    public static HostSettings FromEnvironment()
    {
        return new HostSettings(
            OtelEnabled: ReadFlag("ENABLE_OTEL"),
            OtelSensitive: ReadFlag("ENABLE_OTEL_SENSITIVE"),
            OtelRawEnabled: ReadFlag("ENABLE_OTEL_RAW"),
            LlmProvider: ReadString("LLM_PROVIDER", "ollama").Trim().ToLowerInvariant(),
            ModelId: ReadString("AGENT_MODEL", "qwen2.5:14b"),
            OllamaBaseUrl: ReadString("OLLAMA_BASE_URL", "http://localhost:11434/"),
            OpenRouterBaseUrl: ReadString("OPENROUTER_BASE_URL", "https://openrouter.ai/api/v1"),
            OpenRouterApiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        );
    }

    private static bool ReadFlag(string key)
        => string.Equals(Environment.GetEnvironmentVariable(key), "1", StringComparison.Ordinal);

    private static string ReadString(string key, string defaultValue)
        => Environment.GetEnvironmentVariable(key) ?? defaultValue;
}
