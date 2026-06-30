using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Evidence-first Artefaktgenerator fuer den Semantic-Ledger-Spike.</summary>
public sealed class EvidenceFirstArtifactGenerator
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du erzeugst ein SDLC-Artefakt evidence-first aus einem bestaetigten Semantic Ledger.

        Regeln:
        - Erzeuge nur Aussagen, die durch Ledger-Eintraege gestuetzt sind.
        - Jede fachliche Aussage braucht sourceClaimIds.
        - Neue Annahmen sind nur erlaubt, wenn sie explizit als assumption=true markiert sind.
        - undecided/open darf nicht zu planned/committed verstaerkt werden.
        - later/not_mvp darf nicht zu MVP-Requirement werden.
        - optional/desired darf nicht zu required/must verstaerkt werden.
        - Offene Punkte muessen offen formuliert werden.
        - Das Markdown soll als requirements.md lesbar bleiben, nicht wie ein JSON-Protokoll.

        Antworte ausschliesslich mit JSON:
        {
          "markdown": "# ...",
          "claims": [
            {
              "artifactClaimId": "REQ-001",
              "artifact": "requirements",
              "text": "...",
              "sourceClaimIds": ["SC-..."],
              "status": "undecided|open|decided|required|uncertain|...",
              "modality": "open|must|desired|must_consider|...",
              "scope": "...",
              "timeScope": "mvp|later_possible|...",
              "assumption": false
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "markdown": { "type": "string" },
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
          "required": ["markdown", "claims"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "evidence_first_artifact",
        "Evidence-first Artefakt mit maschinenlesbaren Claims.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public EvidenceFirstArtifactGenerator(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<EvidenceFirstGenerationResult?> GenerateAsync(
        IReadOnlyList<SemanticLedgerEntry> entries,
        string artifact,
        CancellationToken ct)
    {
        var user = $"""
            ARTEFAKT: {artifact}

            LEDGER:
            {JsonSerializer.Serialize(entries, Json)}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
    }

    public static EvidenceFirstGenerationResult? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try
        {
            var parsed = JsonSerializer.Deserialize<EvidenceFirstGenerationResult>(json, Json);
            return parsed is null ? null : parsed with { Claims = parsed.Claims.Select(c => c.Normalized()).ToList() };
        }
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
