using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Spike-Extractor: liest ein Transkript einmal und erzeugt einen globalen SourceClaim-Ledger
/// mit Artefakt-Zuordnung. Nicht Teil der produktiven Coverage-Pipeline.
/// </summary>
public sealed class GlobalSourceClaimExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du extrahierst einen GLOBALEN SOURCE-CLAIM-LEDGER aus einem Stakeholder-Transkript.

        Ein SourceClaim ist eine konkrete quellengetragene Pflicht, offene Frage, Risikoaussage oder
        Entscheidungs-/Statusinformation, die in einem oder mehreren Zielartefakten behandelt werden sollte.

        Zielartefakte:
        - requirements
        - risks
        - architecture
        - open-questions

        Regeln:
        - Lies das Transkript als eine gemeinsame Quelle, nicht pro Artefakt getrennt.
        - Extrahiere konkrete Claims, keine groben Themenueberschriften.
        - Erhalte Status/Modalitaet: offen, entschieden, MVP, spaeter, optional, zwingend.
        - Nimm auch negative/Scope-Claims auf: "nicht im MVP", "noch nicht entschieden".
        - Jeder Claim braucht 1-4 Evidence-Spans aus dem Transkript.
        - Setze relevantFor auf alle Zielartefakte, in denen der Claim sinnvoll behandelt werden sollte.
        - Formuliere requiredTreatment kurz als artefaktuebergreifende Behandlungspflicht.
        - Recall-first: lieber mehr Claims als kritische Claims verlieren.
        - Vermeide reine Duplikate; mehrere wichtige Facetten duerfen getrennte Claims sein.

        Antworte ausschliesslich mit JSON:
        {
          "claims": [
            {
              "sourceClaim": "...",
              "requiredTreatment": "...",
              "evidence": ["..."],
              "kind": "decision|open_question|risk|constraint|requirement|status",
              "priority": "high|medium|low",
              "relevantFor": ["requirements", "risks"]
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
                  "sourceClaim": { "type": "string" },
                  "requiredTreatment": { "type": "string" },
                  "evidence": { "type": "array", "items": { "type": "string" } },
                  "kind": { "type": "string" },
                  "priority": { "type": "string" },
                  "relevantFor": { "type": "array", "items": { "type": "string" } }
                },
                "required": ["sourceClaim", "requiredTreatment", "evidence", "kind", "priority", "relevantFor"],
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
        "global_source_claim_extraction",
        "Globaler SourceClaim-Ledger aus einem Transkript.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public GlobalSourceClaimExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<GlobalSourceClaim>> ExtractAsync(string transcript, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, $"TRANSKRIPT:\n{transcript}")
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)
            .Select((c, i) => c with { Id = $"GLOBAL-SC-{i + 1:D3}" })
            .ToList();
    }

    public static IReadOnlyList<GlobalSourceClaim> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<GlobalSourceClaim>();
        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (env?.Claims is null) return Array.Empty<GlobalSourceClaim>();
            return env.Claims
                .Where(c => !string.IsNullOrWhiteSpace(c.SourceClaim))
                .Select(c => c.ToClaim().Trimmed())
                .ToList();
        }
        catch (JsonException)
        {
            return Array.Empty<GlobalSourceClaim>();
        }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("claims")] List<GlobalSourceClaimDto>? Claims);

    private sealed record GlobalSourceClaimDto(
        [property: JsonPropertyName("sourceClaim")] string? SourceClaim,
        [property: JsonPropertyName("requiredTreatment")] string? RequiredTreatment,
        [property: JsonPropertyName("evidence")] List<string>? Evidence,
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("priority")] string? Priority,
        [property: JsonPropertyName("relevantFor")] List<string>? RelevantFor)
    {
        public GlobalSourceClaim ToClaim()
            => new("", SourceClaim ?? "", RequiredTreatment ?? "", Evidence ?? [], Kind ?? "", Priority ?? "", RelevantFor ?? []);
    }
}

public sealed record GlobalSourceClaim(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("sourceClaim")] string SourceClaim,
    [property: JsonPropertyName("requiredTreatment")] string RequiredTreatment,
    [property: JsonPropertyName("evidence")] IReadOnlyList<string> Evidence,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("priority")] string Priority,
    [property: JsonPropertyName("relevantFor")] IReadOnlyList<string> RelevantFor)
{
    public GlobalSourceClaim Trimmed()
        => this with
        {
            Id = Id?.Trim() ?? "",
            SourceClaim = SourceClaim.Trim(),
            RequiredTreatment = RequiredTreatment?.Trim() ?? "",
            Evidence = Evidence.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList(),
            Kind = Kind?.Trim() ?? "",
            Priority = Priority?.Trim() ?? "",
            RelevantFor = RelevantFor
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
}
