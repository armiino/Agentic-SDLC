using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Claim-Pilot Stufe 3 (ImplementClaimAnsatz §10): die letzte ungetestete Komponente — Atomisierung.
/// Zerlegt EINE (oft zusammengesetzte) Markdown-Unit in atomare, einzeln prüfbare Claims. Motiviert durch
/// den E2E-Befund 2d7b09-FP-BESTELLUEBERSICHT: ein Compound-Claim („zeigt Angebote, Bestellungen UND
/// Rechnungen an") wurde wegen EINES schwachen Teils ganz geflaggt — Atomisierung würde die gedeckten Teile
/// retten und nur den ungedeckten flaggen. Isoliert, additiv, nicht in der produktiven GroundingAxis.
/// </summary>
public sealed class ClaimSplitter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du zerlegst EINE Aussage (Unit) eines SDLC-Artefakts in ATOMARE, einzeln pruefbare Claims.
        Ein atomarer Claim enthaelt GENAU EINE pruefbare Aussage.

        Regeln:
        - Aufzaehlungen ('A, B und C') in mehrere Claims trennen, WENN A/B/C unabhaengig pruefbar sind.
        - Eine Modalitaet/Bedingung, die zum Kern gehoert ('nur', 'ausschliesslich', 'im MVP', 'spaeter'),
          BLEIBT im jeweiligen atomaren Claim erhalten — sie ist pruef-relevant, nicht abtrennen.
        - Ist die Unit bereits atomar: gib sie UNVERAENDERT als einen Claim zurueck.
        - Keine Aussage erfinden, keine weglassen, nicht umdeuten. Reines Zerlegen.
        - Pro Claim ein vollstaendiger, fuer sich verstaendlicher Satz (keine blossen Stichworte).

        Antworte ausschliesslich mit JSON:
        { "claims": ["...", "..."] }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "claims": { "type": "array", "items": { "type": "string" } }
          },
          "required": ["claims"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "claim_split",
        "Atomare Claims einer Artefakt-Unit.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public ClaimSplitter(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<string>> SplitAsync(string unit, string artifactType, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt),
             new ChatMessage(ChatRole.User, $"ARTEFAKTTYP: {artifactType}\n\nUNIT:\n{unit}")],
            options, ct).ConfigureAwait(false);

        var claims = ParseClaims(response.Text);
        // Fallback: unzerlegbar/Parsefehler → die Unit selbst als ein Claim (kein stiller Verlust).
        return claims.Count > 0 ? claims : new[] { unit.Trim() };
    }

    /// <summary>Deterministisches Parsen (testbar ohne LLM). Leere/leerzeilige Claims verworfen.</summary>
    public static IReadOnlyList<string> ParseClaims(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<string>();
        try
        {
            var p = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (p?.Claims is null) return Array.Empty<string>();
            return p.Claims.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList();
        }
        catch (JsonException) { return Array.Empty<string>(); }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("claims")] List<string>? Claims);
}
