using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>LLM-basierter Matcher fuer den isolierten SourceClaim-Extraction-Spike.</summary>
public sealed class SourceClaimRecallMatcher
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du vergleichst EINEN erwarteten SourceClaim mit einer Liste automatisch extrahierter SourceClaims.
        Entscheide, ob die Liste den erwarteten Claim inhaltlich enthaelt.

        VERDIKTE:
        exact = gleicher fachlicher Kern UND gleicher Status/Modalitaet/Artefaktpflicht
        partial = fachlicher Kern enthalten, aber Status/Modalitaet/Pflicht nur teilweise oder grober
        missed = nicht enthalten

        Wichtige Regeln:
        - Paraphrasen gelten als exact, wenn Status/Modalitaet erhalten bleiben.
        - Wenn nur ein grobes Topic enthalten ist, aber die spezifische Pflicht fehlt: partial.
        - Wenn ein Claim fuer ein anderes Artefakt/andere Pflicht steht: missed oder partial, nicht exact.

        Antworte ausschliesslich mit JSON:
        { "verdict": "exact|partial|missed", "matchedIds": ["..."], "reason": "kurz" }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "verdict": { "type": "string" },
            "matchedIds": { "type": "array", "items": { "type": "string" } },
            "reason": { "type": "string" }
          },
          "required": ["verdict", "matchedIds", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_claim_recall_match",
        "Recall-Match eines erwarteten SourceClaims gegen extrahierte Claims.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimRecallMatcher(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<SourceClaimRecallVerdict> MatchAsync(
        SourceClaimCoverageCase expected,
        IReadOnlyList<ExtractedSourceClaim> extracted,
        CancellationToken ct)
    {
        var listing = string.Join("\n\n", extracted.Select(c => $"""
            ID: {c.Id}
            CLAIM: {c.SourceClaim}
            TREATMENT: {c.RequiredTreatment}
            EVIDENCE: {string.Join(" | ", c.Evidence)}
            """));

        var user = $"""
            ERWARTETER CLAIM:
            ID: {expected.Id}
            ARTEFAKT: {expected.Artifact}
            CLAIM: {expected.SourceClaim}
            REQUIRED_TREATMENT: {expected.RequiredTreatment}
            EVIDENCE: {string.Join(" | ", expected.Evidence)}

            EXTRAHIERTE CLAIMS:
            {listing}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        if (parsed is null)
            return new SourceClaimRecallVerdict("unclassified", Array.Empty<string>(), "kein parsebares Match-Ergebnis");

        return new SourceClaimRecallVerdict(
            Normalize(parsed.Verdict),
            parsed.MatchedIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList() ?? [],
            parsed.Reason?.Trim() ?? "");
    }

    public static SourceClaimRecallPayload? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SourceClaimRecallPayload>(json, Json); }
        catch (JsonException) { return null; }
    }

    public static string Normalize(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "exact" => "exact",
        "partial" => "partial",
        "missed" => "missed",
        _ => "unclassified"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}

public sealed record SourceClaimRecallVerdict(string Verdict, IReadOnlyList<string> MatchedIds, string Reason);

public sealed record SourceClaimRecallPayload(
    [property: JsonPropertyName("verdict")] string? Verdict,
    [property: JsonPropertyName("matchedIds")] List<string>? MatchedIds,
    [property: JsonPropertyName("reason")] string? Reason);
