using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Erzeugt aus ArtifactClaims ein lesbares Human-Artefakt mit sichtbaren SourceClaim-Referenzen.</summary>
public sealed class HumanArtifactGenerator
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du erzeugst aus maschinenlesbaren ArtifactClaims ein lesbares SDLC-Artefakt.

        Regeln:
        - Bewahre die Semantik der Claims.
        - Erzeuge ein professionelles Markdown-Artefakt fuer Menschen.
        - Jeder Bullet/Absatz, der aus Claims entsteht, muss sichtbare SourceClaim-Referenzen enthalten:
          [SC-...]
        - SourceClaim-Referenzen duerfen nicht verloren gehen.
        - Keine neuen fachlichen Aussagen ohne Quelle.
        - Keine langen Evidence-Zitate im Human-Artefakt.
        - Nutze sinnvolle Abschnitte statt Claim-Tabellen.
        - Das Ergebnis soll lesbar und als echtes SDLC-Artefakt brauchbar sein.

        Antworte ausschliesslich mit JSON:
        {
          "markdown": "# ...",
          "usedSourceClaimIds": ["SC-..."],
          "readabilityNotes": "kurz"
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "markdown": { "type": "string" },
            "usedSourceClaimIds": { "type": "array", "items": { "type": "string" } },
            "readabilityNotes": { "type": "string" }
          },
          "required": ["markdown", "usedSourceClaimIds", "readabilityNotes"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "human_artifact_generation",
        "Lesbares SDLC-Artefakt mit sichtbaren SourceClaim-Referenzen.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public HumanArtifactGenerator(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<HumanArtifactResult?> GenerateAsync(
        IReadOnlyList<GeneratedArtifactClaim> claims,
        string artifact,
        CancellationToken ct)
    {
        var user = $"""
            ARTEFAKT: {artifact}

            ARTIFACT_CLAIMS:
            {JsonSerializer.Serialize(claims, Json)}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
    }

    public static HumanArtifactResult? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try
        {
            var parsed = JsonSerializer.Deserialize<HumanArtifactResult>(json, Json);
            return parsed is null ? null : parsed with
            {
                Markdown = parsed.Markdown.Trim(),
                UsedSourceClaimIds = parsed.UsedSourceClaimIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                ReadabilityNotes = parsed.ReadabilityNotes.Trim()
            };
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

public sealed record HumanArtifactResult(
    string Markdown,
    IReadOnlyList<string> UsedSourceClaimIds,
    string ReadabilityNotes);
