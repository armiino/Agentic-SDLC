using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z10 / v02 (Call 2, separat): klassifiziert <c>relevantFor</c> je Topic in EINEM eigenen LLM-Call —
/// getrennt von der breiten v01-Extraktion (<see cref="TopicExtractor"/>), die relevantFor heute im selben
/// Call mitentscheidet. Hypothese (Test B): eine fokussierte, begruendete Relevanz-Klassifikation ist fairer
/// (weniger permissiv), weil <c>relevantFor</c> das Relevanz-Gate der Coverage-Achse ist
/// (<see cref="TopicCoverageMapper"/>) → hoechster Einzelhebel. Bewusst eng: KEINE neuen Topics, KEINE
/// Score-/Severity-/Repair-Entscheidung. Der Instruktions-Prompt ist ausgelagert + versioniert
/// (<c>Prompts/topics/classify-topic-relevance-v01.txt</c>); das JSON-Schema bleibt struktur-nah im Code.
/// </summary>
public sealed class TopicRelevanceClassifier
{
    private static readonly string[] ArtifactTypes = { "requirements", "risks", "architecture", "open-questions" };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "topics": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "topicId": { "type": "string" },
                  "relevantFor": { "type": "array", "items": { "type": "string" } },
                  "reasons": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "artifactType": { "type": "string" },
                        "reason": { "type": "string" }
                      },
                      "required": ["artifactType", "reason"],
                      "additionalProperties": false
                    }
                  }
                },
                "required": ["topicId", "relevantFor", "reasons"],
                "additionalProperties": false
              }
            }
          },
          "required": ["topics"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "topic_relevance",
        "Artefakt-Relevanz (relevantFor) je Topic, separat klassifiziert und je Typ begruendet.");

    private readonly IChatClient _client;
    private readonly string _systemPrompt;
    private readonly bool _structuredOutput;

    public TopicRelevanceClassifier(IChatClient client, string systemPrompt, bool structuredOutput = true)
    {
        _client = client;
        _systemPrompt = systemPrompt;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<TopicRelevance>> ClassifyAsync(
        IReadOnlyList<TopicItem> topics, IReadOnlyList<TranscriptTurn> turns, CancellationToken ct)
    {
        var byIndex = turns.ToDictionary(t => t.Index);
        var listing = string.Join("\n\n", topics.Select(t =>
        {
            var excerpts = string.Join("\n", t.SourceTurns
                .Where(byIndex.ContainsKey)
                .Take(8)
                .Select(i => $"    {i}: [{byIndex[i].Speaker}] {byIndex[i].Text}"));
            return $"TOPIC {t.TopicId}: {t.Summary}\n  Turns:\n{excerpts}";
        }));

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, _systemPrompt),
            new ChatMessage(ChatRole.User, $"TOPICS (klassifiziere relevantFor je Topic):\n\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        return ParseRelevance(response.Text);
    }

    /// <summary>Deterministisches Parsen + Validieren der LLM-Antwort (testbar ohne LLM).</summary>
    public static IReadOnlyList<TopicRelevance> ParseRelevance(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<TopicRelevance>();

        Envelope? parsed;
        try { parsed = JsonSerializer.Deserialize<Envelope>(json, Json); }
        catch (JsonException) { return Array.Empty<TopicRelevance>(); }
        if (parsed?.Topics is null) return Array.Empty<TopicRelevance>();

        var result = new List<TopicRelevance>();
        foreach (var t in parsed.Topics)
        {
            if (string.IsNullOrWhiteSpace(t.TopicId)) continue;

            var rel = (t.RelevantFor ?? new List<string>())
                .Select(r => r?.Trim().ToLowerInvariant())
                .Where(r => r is not null && ArtifactTypes.Contains(r))
                .Select(r => r!)
                .Distinct()
                .ToList();

            // Begruendungen nur fuer gueltige + tatsaechlich gewaehlte Artefakttypen behalten (Audit-Konsistenz).
            var reasons = new Dictionary<string, string>();
            foreach (var r in t.Reasons ?? new List<ReasonDto>())
            {
                var type = r.ArtifactType?.Trim().ToLowerInvariant();
                if (type is null || !rel.Contains(type) || string.IsNullOrWhiteSpace(r.Reason)) continue;
                reasons[type] = r.Reason!.Trim();
            }

            result.Add(new TopicRelevance(t.TopicId.Trim(), rel, reasons));
        }
        return result;
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("topics")] List<TopicDto>? Topics);

    private sealed record TopicDto(
        [property: JsonPropertyName("topicId")] string? TopicId,
        [property: JsonPropertyName("relevantFor")] List<string>? RelevantFor,
        [property: JsonPropertyName("reasons")] List<ReasonDto>? Reasons);

    private sealed record ReasonDto(
        [property: JsonPropertyName("artifactType")] string? ArtifactType,
        [property: JsonPropertyName("reason")] string? Reason);
}
