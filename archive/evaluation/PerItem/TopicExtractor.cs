using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z6.2 (DISK-COV V1): Extrahiert aus einem Transkript stabile <see cref="TopicItem"/>s (ein LLM-Call,
/// EINMAL pro Transkript → eingefrorene Fixture). Aggregiert Turns zum selben fachlichen Thema und taggt
/// die Artefakt-Relevanz, damit die Coverage-Achse nicht mehr turn-basiert über-zählt (B34).
/// </summary>
public sealed class TopicExtractor
{
    private static readonly string[] ArtifactTypes = { "requirements", "risks", "architecture", "open-questions" };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du verdichtest ein Stakeholder-/Kickoff-Transkript in STABILE, nicht-ueberlappende Topics fuer die
        SDLC-Coverage-Pruefung. Ein Topic buendelt ALLE Aeusserungen zum selben fachlichen Thema — auch
        wenn sie ueber mehrere Sprecher und Stellen verteilt sind.

        Regeln:
        - Smalltalk, Organisatorisches, reine Zustimmung/Wiederholung wird KEIN Topic.
        - Jeder fachliche Punkt gehoert in GENAU EIN Topic (keine Ueberlappung).
        - summary: ein praegnanter Satz.
        - status: "resolved" (entschieden/abgeschlossen) | "unresolved" (offen/strittig) |
          "decision_open" (Entscheidung explizit offen gelassen).
        - sourceTurns: die Nummern (Indizes) der zugehoerigen Turns.
        - relevantFor: welche fruehen SDLC-Artefakte das Thema betrifft, Teilmenge von
          [requirements, risks, architecture, open-questions]. Anforderung→requirements;
          Risiko/Konflikt/Unsicherheit→risks; technische Festlegung/Komponente→architecture; explizit
          offene Entscheidung→open-questions. Mehrfachnennung erlaubt.
        - topicId: "TOPIC-" + kurzer Slug + laufende Nummer (z. B. TOPIC-CURRENCY-001).

        Antworte ausschliesslich mit JSON:
        { "topics": [ { "topicId": "...", "summary": "...", "status": "...",
          "sourceTurns": [0], "relevantFor": ["requirements"] } ] }
        """;

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
                  "summary": { "type": "string" },
                  "status": { "type": "string" },
                  "sourceTurns": { "type": "array", "items": { "type": "integer" } },
                  "relevantFor": { "type": "array", "items": { "type": "string" } }
                },
                "required": ["topicId", "summary", "status", "sourceTurns", "relevantFor"],
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
        "transcript_topics",
        "Stabile, nicht-ueberlappende Topics eines Transkripts mit Artefakt-Relevanz.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public TopicExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<TopicItem>> ExtractAsync(IReadOnlyList<TranscriptTurn> turns, CancellationToken ct)
    {
        var listing = string.Join("\n", turns.Select(t => $"{t.Index}: [{t.Speaker}] {t.Text}"));
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, $"TRANSKRIPT-TURNS:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        return ParseTopics(response.Text);
    }

    /// <summary>Deterministisches Parsen + Validieren der LLM-Antwort (testbar ohne LLM).</summary>
    public static IReadOnlyList<TopicItem> ParseTopics(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<TopicItem>();

        Envelope? parsed;
        try { parsed = JsonSerializer.Deserialize<Envelope>(json, Json); }
        catch (JsonException) { return Array.Empty<TopicItem>(); }
        if (parsed?.Topics is null) return Array.Empty<TopicItem>();

        var result = new List<TopicItem>();
        foreach (var t in parsed.Topics)
        {
            if (string.IsNullOrWhiteSpace(t.TopicId) || string.IsNullOrWhiteSpace(t.Summary))
                continue;

            var rel = (t.RelevantFor ?? new List<string>())
                .Select(r => r?.Trim().ToLowerInvariant())
                .Where(r => r is not null && ArtifactTypes.Contains(r))
                .Select(r => r!)
                .Distinct()
                .ToList();

            result.Add(new TopicItem(
                t.TopicId.Trim(),
                t.Summary.Trim(),
                string.IsNullOrWhiteSpace(t.Status) ? "unresolved" : t.Status!.Trim().ToLowerInvariant(),
                (t.SourceTurns ?? new List<int>()).ToList(),
                rel));
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
        [property: JsonPropertyName("summary")] string? Summary,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("sourceTurns")] List<int>? SourceTurns,
        [property: JsonPropertyName("relevantFor")] List<string>? RelevantFor);
}
