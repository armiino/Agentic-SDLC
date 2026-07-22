using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Extrahiert maschinenlesbare ArtifactClaims aus einem bestehenden freien Artefakt.</summary>
public sealed class ArtifactClaimExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du extrahierst pruefbare Claims aus einem bestehenden SDLC-Artefakt.

        Ziel:
        - Erzeuge atomare ArtifactClaims.
        - Bewahre Status, Modalitaet, Scope und TimeScope.
        - Gib keine sourceClaimIds an, ausser sie stehen explizit im Artefakt.
        - Formuliere nicht um zu einer besseren Version; extrahiere, was im Artefakt steht.

        Antworte ausschliesslich mit JSON:
        {
          "claims": [
            {
              "artifactClaimId": "OLD-REQ-001",
              "artifact": "requirements",
              "text": "...",
              "sourceClaimIds": [],
              "status": "undecided|open|decided|required|planned|uncertain|...",
              "modality": "open|must|desired|planned|...",
              "scope": "...",
              "timeScope": "mvp|later|...",
              "assumption": false
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "claims": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "artifactClaimId": { "type": "string" },
                  "artifact": { "type": "string" },
                  "text": { "type": "string" },
                  "sourceClaimIds": { "type": "array", "items": { "type": "string" } },
                  "status": { "type": "string" },
                  "modality": { "type": "string" },
                  "scope": { "type": "string" },
                  "timeScope": { "type": ["string", "null"] },
                  "assumption": { "type": "boolean" }
                },
                "required": ["artifactClaimId", "artifact", "text", "sourceClaimIds", "status", "modality", "scope", "timeScope", "assumption"],
                "additionalProperties": false
              }
            }
          },
          "required": ["claims"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "artifact_claim_extraction",
        "ArtifactClaims aus einem bestehenden SDLC-Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public ArtifactClaimExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<GeneratedArtifactClaim>> ExtractAsync(
        string artifact,
        string artifactText,
        CancellationToken ct)
    {
        var user = $"""
            ARTEFAKT: {artifact}

            TEXT:
            {artifactText}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)?.Claims.Select(c => c.Normalized()).ToList() ?? [];
    }

    public static ArtifactClaimExtractionResult? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<ArtifactClaimExtractionResult>(json, Json); }
        catch (JsonException) { return null; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}
