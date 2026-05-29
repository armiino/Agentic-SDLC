namespace AgenticSdlc.Host.Prompts;

/// <summary>
/// Lädt Prompt-Templates aus txt dateien und ersetzt einfache Platzhalter
/// </summary>
/// <remarks>
/// Prompts sollen sichtbar versioniert sein damit auch alter prompts wieder verwednet werden können.
/// Deshalb liegen sie als Textdateien im jeweiligen Phasenordner und werden pro Run über `run-config.json` ausgewählt. 
/// Der Loader ersetzt nur deterministische Platzhalter wie `{{runId}}` oder `{{contextPath}}`.
/// </remarks>
public static class PromptTemplateLoader
{
    public static string Load(
        string repoRoot,
        string relativePromptDirectory,
        string promptName,
        IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(promptName))
            throw new InvalidOperationException("Prompt name must not be empty.");

        if (promptName.Contains('/') || promptName.Contains('\\'))
        {
            throw new InvalidOperationException(
                $"Prompt name '{promptName}' must be a file name, not a path. Put the file into {relativePromptDirectory}.");
        }

        var fileName = Path.HasExtension(promptName) ? promptName : $"{promptName}.txt";
        var promptDirectory = Path.GetFullPath(Path.Combine(repoRoot, relativePromptDirectory));
        var promptPath = Path.GetFullPath(Path.Combine(promptDirectory, fileName));

        /*
         * Schutz gegen versehentliches oder absichtliches Verlassen des Prompt-Ordners.
         * Die Config soll nur Promptdateien innerhalb des Agent-Ordners auswählen,
         *  und nicht irgendwelche Dateien im Repository
         */
        if (!promptPath.StartsWith(promptDirectory, StringComparison.Ordinal))
            throw new InvalidOperationException($"Prompt path '{promptPath}' escapes the prompt directory.");

        if (!File.Exists(promptPath))
        {
            throw new InvalidOperationException(
                $"Prompt file '{fileName}' was not found in '{relativePromptDirectory}'. Add the .txt file or change run-config.json.");
        }

        var prompt = File.ReadAllText(promptPath);

        /*
         * Bewusst einfache Template-Logik: Es werden nur bekannte Platzhalter ersetzt. 
         * Der Loader führt keine eigene Promptlogik aus und verändert die eigentliche Agentenanweisung nicht.

         */
        foreach (var (key, value) in variables)
            prompt = prompt.Replace("{{" + key + "}}", value, StringComparison.Ordinal);

        return prompt;
    }
}
