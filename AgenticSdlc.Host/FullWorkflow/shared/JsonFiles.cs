using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow;

// R3a Welle 2 (2026-07-22): EIN JSON-Datei-IO fuer die Kette — dieselben Optionen (Web + Indented) existierten
// als private Kopie in dutzenden Runnern/Adaptern. Verhalten identisch (gleiche Options-Werte).
public static class JsonFiles
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    public static Task SaveAsync<T>(string path, T value)
        => File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, Json));
}
