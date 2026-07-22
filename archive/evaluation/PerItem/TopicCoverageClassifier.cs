using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z6.3 (DISK-COV V1): Bewertet bereits aggregierte, relevante <see cref="TopicItem"/>s gegen EIN
/// Artefakt — covered | partial | missing | not_applicable. Bounded (fixe Topic-Menge) + gechunkt →
/// stabil, kein Turn-Rauschen, kein Over-Counting (das löst die Topic-Stufe davor).
/// </summary>
public sealed class TopicCoverageClassifier
{
    public const int ChunkSize = 12;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst die ABDECKUNG eines SDLC-Artefakts gegen eine Liste bereits aggregierter, fuer diesen
        Artefakttyp RELEVANTER Themen (Topics). Du bekommst das VOLLSTAENDIGE Artefakt und eine NUMMERIERTE
        Liste von Topics (je mit Kurzbeschreibung). Klassifiziere JEDES Topic mit GENAU EINEM Verdikt:

        covered        = Das Topic ist im Artefakt vorhanden (auch sinngemaess/paraphrasiert).
        partial        = Das Topic ist nur angerissen/teilweise abgedeckt (eine Facette fehlt).
        missing        = Das Topic kommt im Artefakt NIRGENDWO vor (auch nicht sinngemaess).
        not_applicable = Das Topic ist fuer DIESEN Artefakttyp doch nicht als Inhalt zu erwarten.

        Wichtig: Bevor du 'missing' vergibst, durchsuche das GANZE Artefakt nach einer sinngemaessen
        Entsprechung. Beurteile auf Topic-Ebene, nicht wortwoertlich.

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "verdict": "covered|partial|missing|not_applicable", "reason": "kurz" } ] }
        Genau ein Ergebnis pro Topic (Index = Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "results": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "index": { "type": "integer" },
                  "verdict": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["index", "verdict", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["results"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "topic_coverage",
        "Coverage-Klassifikation aggregierter Topics gegen ein SDLC-Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public TopicCoverageClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<TopicVerdict>> ClassifyAsync(
        string artifactText, IReadOnlyList<TopicItem> relevantTopics, CancellationToken ct)
    {
        var verdicts = new Dictionary<string, TopicVerdict>();

        for (var offset = 0; offset < relevantTopics.Count; offset += ChunkSize)
        {
            var chunk = relevantTopics.Skip(offset).Take(ChunkSize).ToList();
            await ClassifyChunkAsync(artifactText, chunk, verdicts, ct).ConfigureAwait(false);
        }

        foreach (var t in relevantTopics)
            if (!verdicts.ContainsKey(t.TopicId))
                verdicts[t.TopicId] = new TopicVerdict(t.TopicId, "unclassified", "kein Verdikt erhalten");

        return relevantTopics.Select(t => verdicts[t.TopicId]).ToList();
    }

    private async Task ClassifyChunkAsync(
        string artifactText, List<TopicItem> chunk, Dictionary<string, TopicVerdict> result, CancellationToken ct)
    {
        var listing = string.Join("\n", chunk.Select((t, k) => $"{k}: {t.Summary}"));
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, $"ARTEFAKT:\n{artifactText}\n\nTOPICS:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        foreach (var (topicId, verdict, reason) in ParseChunk(response.Text, chunk))
            result[topicId] = new TopicVerdict(topicId, verdict, reason);
    }

    /// <summary>Deterministisches Parsen einer Chunk-Antwort → (topicId, verdict, reason) (testbar).</summary>
    public static IReadOnlyList<(string TopicId, string Verdict, string Reason)> ParseChunk(
        string? text, IReadOnlyList<TopicItem> chunk)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<(string, string, string)>();

        Envelope? parsed;
        try { parsed = JsonSerializer.Deserialize<Envelope>(json, Json); }
        catch (JsonException) { return Array.Empty<(string, string, string)>(); }
        if (parsed?.Results is null) return Array.Empty<(string, string, string)>();

        var output = new List<(string, string, string)>();
        foreach (var r in parsed.Results)
            if (r.Index >= 0 && r.Index < chunk.Count)
                output.Add((chunk[r.Index].TopicId, Normalize(r.Verdict), r.Reason ?? string.Empty));
        return output;
    }

    private static string Normalize(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "covered" => "covered",
        "partial" => "partial",
        "missing" => "missing",
        "not_applicable" => "not_applicable",
        _ => "not_applicable"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("results")] List<Result>? Results);
    private sealed record Result(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("verdict")] string? Verdict,
        [property: JsonPropertyName("reason")] string? Reason);
}
