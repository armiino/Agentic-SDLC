using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z8 (DISK-COV-5): begrenzter Topic-Completeness-Verifier. EIN LLM-Call, der NUR die Referenzbasis prüft:
/// „Welche offensichtlich wichtigen fachlichen Inhalte aus dem Transkript fehlen in der Topic-Liste?"
/// Bewusst eng — KEIN Artefakt-Review, KEINE Scores/Severity/Repair (≠ alte Coverage-Jury, B30 find≠measure).
/// Begründung: was nicht in der Fixture steht, kann Coverage nie als missing melden (todos2 §2e). Der
/// Output sind <see cref="MissingTopicCandidate"/>s — markiert, nie blind als Topic übernommen.
/// </summary>
public sealed class TopicVerifier
{
    private static readonly string[] ArtifactTypes = { "requirements", "risks", "architecture", "open-questions" };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du bist ein BEGRENZTER Vollständigkeits-Pruefer fuer eine bereits extrahierte Topic-Liste eines
        Stakeholder-/Kickoff-Transkripts. Deine EINZIGE Aufgabe: nenne fachliche Inhalte aus dem
        Transkript, die in der vorhandenen Topic-Liste FEHLEN.

        Strenge Regeln:
        - Melde NUR offensichtlich wichtige fachliche Themen (Anforderung, Risiko, technische Festlegung,
          offene Entscheidung, relevante Prozess-/Meta-Luecke). KEIN Smalltalk, KEINE Wiederholung.
        - Melde NUR, was durch KEIN bestehendes Topic abgedeckt ist (auch nicht teilweise/anders benannt).
        - Wenn nichts Wichtiges fehlt: gib eine LEERE Liste zurueck. Erfinde nichts, um etwas zu melden.
        - Du bewertest KEINE Artefakte, vergibst KEINE Scores/Severity und triffst KEINE Repair-Entscheidung.
        - summary: ein praegnanter Satz zum fehlenden Thema.
        - sourceTurns: Turn-Indizes im Transkript, die das Thema belegen.
        - suggestedRelevantFor: Teilmenge von [requirements, risks, architecture, open-questions].
        - reason: kurze Begruendung, warum kein bestehendes Topic das abdeckt.

        Antworte ausschliesslich mit JSON:
        { "missingTopicCandidates": [ { "summary": "...", "sourceTurns": [0],
          "suggestedRelevantFor": ["risks"], "reason": "..." } ] }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "missingTopicCandidates": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "summary": { "type": "string" },
                  "sourceTurns": { "type": "array", "items": { "type": "integer" } },
                  "suggestedRelevantFor": { "type": "array", "items": { "type": "string" } },
                  "reason": { "type": "string" }
                },
                "required": ["summary", "sourceTurns", "suggestedRelevantFor", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["missingTopicCandidates"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "missing_topic_candidates",
        "Fachliche Themen aus dem Transkript, die in der Topic-Liste fehlen (nur Kandidaten, keine Bewertung).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public TopicVerifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<MissingTopicCandidate>> VerifyAsync(
        IReadOnlyList<TranscriptTurn> turns, IReadOnlyList<TopicItem> existingTopics, CancellationToken ct)
    {
        var listing = string.Join("\n", turns.Select(t => $"{t.Index}: [{t.Speaker}] {t.Text}"));
        var topicList = string.Join("\n", existingTopics.Select(t =>
            $"- {t.TopicId} [{string.Join(",", t.RelevantFor)}] {t.Summary}"));

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User,
                $"VORHANDENE TOPICS:\n{topicList}\n\nTRANSKRIPT-TURNS:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        return ParseCandidates(response.Text);
    }

    /// <summary>Deterministisches Parsen + Validieren der LLM-Antwort (testbar ohne LLM).</summary>
    public static IReadOnlyList<MissingTopicCandidate> ParseCandidates(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<MissingTopicCandidate>();

        Envelope? parsed;
        try { parsed = JsonSerializer.Deserialize<Envelope>(json, Json); }
        catch (JsonException) { return Array.Empty<MissingTopicCandidate>(); }
        if (parsed?.Candidates is null) return Array.Empty<MissingTopicCandidate>();

        var result = new List<MissingTopicCandidate>();
        foreach (var c in parsed.Candidates)
        {
            if (string.IsNullOrWhiteSpace(c.Summary)) continue;

            var rel = (c.SuggestedRelevantFor ?? new List<string>())
                .Select(r => r?.Trim().ToLowerInvariant())
                .Where(r => r is not null && ArtifactTypes.Contains(r))
                .Select(r => r!)
                .Distinct()
                .ToList();

            result.Add(new MissingTopicCandidate(
                c.Summary.Trim(),
                (c.SourceTurns ?? new List<int>()).ToList(),
                rel,
                (c.Reason ?? string.Empty).Trim()));
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

    private sealed record Envelope(
        [property: JsonPropertyName("missingTopicCandidates")] List<CandidateDto>? Candidates);

    private sealed record CandidateDto(
        [property: JsonPropertyName("summary")] string? Summary,
        [property: JsonPropertyName("sourceTurns")] List<int>? SourceTurns,
        [property: JsonPropertyName("suggestedRelevantFor")] List<string>? SuggestedRelevantFor,
        [property: JsonPropertyName("reason")] string? Reason);
}
