using System.Text.Json;

namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// Beschreibt die public Runtime-config eines Runs.
/// </summary>
/// <remarks>
/// Diese Datei trennt bewusst experimentelle Run-Parameter von lokalen Secrets.
/// Phase, Modell, Provider und Observability-Flags sind Forschungsparameter und
/// sollen deshalb in `run-config.json` sichtbar und referenzierbar sein.
/// API Keys und lokale Endpunkte bleiben dagegen weiterhin in `.env` oder echten Umgebenungsvar

/// </remarks>
public sealed class RunConfig
{
    public string? AgentPhase { get; set; }
    public string? Phase2ContextStrategy { get; set; }
    public string? LlmProvider { get; set; }
    public string? AgentModel { get; set; }

    /*
     * Die Prompt-Auswahl ist bewusst phasenübergreifend generisch.
     * Key   = Agentname, zB "Phase2RequirementsAgent"
     * Value = Promptdateiname ohne .txt, zB "RequirementsPrompt3"
     *
     * Dadurch muss für neue Phasen kein neuer Config-Typ wie "Phase3Prompts" gebaut werden. 
     * Ein neuer Agent braucht nur einen Prompt-Ordner und einen Eintrag in run-config.json.
     */
    public Dictionary<string, string> Prompts { get; set; } = new(StringComparer.Ordinal);

    public ObservabilityConfig Observability { get; set; } = new();
    public LlmPreviewConfig LlmPreview { get; set; } = new();

    /// <summary>
    /// Lädt `run-config.json` aus dem Repo-Root
    /// </summary>
    public static RunConfig Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "run-config.json");

        if (!File.Exists(path))
            return new RunConfig();

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return JsonSerializer.Deserialize<RunConfig>(json, options) ?? new RunConfig();
    }
}

public sealed class ObservabilityConfig
{
    public bool? EnableOtel { get; set; }
    public bool? EnableOtelSensitive { get; set; }
    public bool? EnableOtelRaw { get; set; }
}

public sealed class LlmPreviewConfig
{
    public bool? Enabled { get; set; }
    public int? Chars { get; set; }
    public bool? OnlyWhenNoTools { get; set; }
}
