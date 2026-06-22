using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Kanonisches Wire-Format für <see cref="ReviewResult"/> (zB künftige <c>jury/*.json</c>):
/// camelCase-Properties + Enums als lesbare Strings. Eine Stelle, damit Producer und Konsumenten
/// nicht je eigene Optionen ableiten (Drift-Vermeidung).
/// </summary>
public static class ReviewJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
