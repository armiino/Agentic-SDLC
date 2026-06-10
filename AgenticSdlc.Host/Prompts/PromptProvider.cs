namespace AgenticSdlc.Host.Prompts;

/// <summary>
/// über die klasse auf prompts zugreifen
/// </summary>
/// <remarks>
/// Schema: `AgenticSdlc.Host/Prompts/<phase>/<agentName>/<promptName>.txt`
/// Neue Prompts (auch andere phasen später) werden als Textdatei abgelegt und in `run-config.json` referenziert.
/// </remarks>
public static class PromptProvider
{
    private const string PromptRoot = "AgenticSdlc.Host/Prompts";

    public static string Load(
        string repoRoot,
        string phase,
        string agentName,
        string promptName,
        IReadOnlyDictionary<string, string> variables)
    {
        ValidateSegment("phase", phase);
        ValidateSegment("agentName", agentName);
        ValidateSegment("promptName", promptName);

        var relativePromptDirectory = Path.Combine(PromptRoot, phase, agentName);

        return PromptTemplateLoader.Load(
            repoRoot,
            relativePromptDirectory,
            promptName,
            variables);
    }

    private static void ValidateSegment(string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{label} must not be empty.");

        if (value.Contains('/') || value.Contains('\\') || value == "." || value == "..")
            throw new InvalidOperationException($"{label} '{value}' must be a directory segment, not a path.");
    }
}
