using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter SourceClaim-Extraction-Spike: extrahiert konkrete, evidence-gebundene Quellpflichten
/// fuer genau einen Artefakttyp. Nicht Teil der produktiven Topic/Coverage-Pipeline.
/// </summary>
public sealed class SourceClaimExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du extrahierst SOURCE-CLAIMS aus einem Stakeholder-Transkript fuer genau EIN Zielartefakt
        (requirements, risks, architecture oder open-questions).

        Ein SourceClaim ist eine konkrete quellengetragene Pflicht, offene Frage, Risikoaussage oder
        Entscheidungs-/Statusinformation, die im Zielartefakt behandelt werden sollte.

        Regeln:
        - Extrahiere konkrete Claims, keine groben Themenueberschriften.
        - Erhalte Status/Modalitaet: offen, entschieden, MVP, spaeter, optional, zwingend.
        - Nimm auch negative/Scope-Claims auf: "nicht im MVP", "noch nicht entschieden".
        - Jeder Claim braucht 1-4 Evidence-Spans aus dem Transkript.
        - Formuliere eine kurze requiredTreatment fuer das Zielartefakt.
        - Priorisiere Claims, die fuer Review/Coverage relevant sind: Missing, Partial, Contradicted moeglich.
        - Beachte den im User-Prompt genannten Modus: bounded Top-Auswahl oder wide recall-first Extraction.

        Antworte ausschliesslich mit JSON:
        {
          "claims": [
            {
              "sourceClaim": "...",
              "requiredTreatment": "...",
              "evidence": ["..."],
              "kind": "decision|open_question|risk|constraint|requirement|status",
              "priority": "high|medium|low"
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
                  "priority": { "type": "string" }
                },
                "required": ["sourceClaim", "requiredTreatment", "evidence", "kind", "priority"],
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
        "source_claim_extraction",
        "SourceClaims fuer ein Zielartefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<ExtractedSourceClaim>> ExtractAsync(
        string artifactType,
        string transcript,
        bool wideRecall,
        CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;
        var modeInstruction = wideRecall
            ? """
              MODUS: WIDE_RECALL.
              Extrahiere moeglichst ALLE konkreten SourceClaims fuer dieses Zielartefakt.
              Nicht auf Top-N priorisieren. Lieber mehr Claims als kritische Claims verlieren.
              Vermeide aber reine Duplikate; mehrere Facetten duerfen getrennte Claims sein.
              """
            : """
              MODUS: BOUNDED_TOP.
              Extrahiere maximal etwa 14 besonders review-relevante SourceClaims fuer dieses Zielartefakt.
              Priorisiere Claims, die wahrscheinlich Missing/Partial/Contradicted pruefbar machen.
              """;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, $"{modeInstruction}\n\nZIELARTEFAKT: {artifactType}\n\nTRANSKRIPT:\n{transcript}")
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)
            .Select((c, i) => c with { Id = $"{artifactType.ToUpperInvariant()}-SC-{i + 1:D3}", Artifact = artifactType })
            .ToList();
    }

    public static IReadOnlyList<ExtractedSourceClaim> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<ExtractedSourceClaim>();
        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (env?.Claims is null) return Array.Empty<ExtractedSourceClaim>();
            return env.Claims
                .Where(c => !string.IsNullOrWhiteSpace(c.SourceClaim))
                .Select(c => c.ToClaim().Trimmed())
                .ToList();
        }
        catch (JsonException)
        {
            return Array.Empty<ExtractedSourceClaim>();
        }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("claims")] List<ExtractedSourceClaimDto>? Claims);

    private sealed record ExtractedSourceClaimDto(
        [property: JsonPropertyName("sourceClaim")] string? SourceClaim,
        [property: JsonPropertyName("requiredTreatment")] string? RequiredTreatment,
        [property: JsonPropertyName("evidence")] List<string>? Evidence,
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("priority")] string? Priority)
    {
        public ExtractedSourceClaim ToClaim()
            => new("", "", SourceClaim ?? "", RequiredTreatment ?? "", Evidence ?? [], Kind ?? "", Priority ?? "");
    }
}

public sealed record ExtractedSourceClaim(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("artifact")] string Artifact,
    [property: JsonPropertyName("sourceClaim")] string SourceClaim,
    [property: JsonPropertyName("requiredTreatment")] string RequiredTreatment,
    [property: JsonPropertyName("evidence")] IReadOnlyList<string> Evidence,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("priority")] string Priority)
{
    public ExtractedSourceClaim Trimmed()
        => this with
        {
            Id = Id?.Trim() ?? "",
            Artifact = Artifact?.Trim() ?? "",
            SourceClaim = SourceClaim.Trim(),
            RequiredTreatment = RequiredTreatment?.Trim() ?? "",
            Evidence = Evidence.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList(),
            Kind = Kind?.Trim() ?? "",
            Priority = Priority?.Trim() ?? ""
        };
}
