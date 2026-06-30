using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Kleiner Coverage-v2-Spike: extrahiert direkt evidence-gebundene SourceObligations statt erst generische Claims.
/// Nicht Teil der produktiven Review-Pipeline.
/// </summary>
public sealed class SourceObligationExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du extrahierst SOURCE-OBLIGATIONS aus einem Stakeholder-Transkript.

        Eine SourceObligation ist eine konkrete, quellengetragene Pflicht oder offene Entscheidung, deren Fehlen,
        falsche Darstellung oder falscher Status in mindestens einem Zielartefakt ein Review-Defekt waere.

        Zielartefakte:
        - requirements
        - risks
        - architecture
        - open-questions

        Regeln:
        - Lies das Transkript einmal als gemeinsame Quelle.
        - Extrahiere nicht jede Gespraechsaussage, sondern alle review-relevanten Artefaktpflichten.
        - Kein enges Top-N-Cap: lieber mehr Obligations als zentrale Missing/Contradicted-Faelle verlieren.
        - Jede Obligation ist fuer genau EIN Zielartefakt formuliert.
        - Wenn derselbe SourceClaim in mehreren Artefakten unterschiedliche Pflichten erzeugt, erstelle mehrere Obligations.
        - Bewahre Status/Modalitaet: offen, entschieden, MVP, spaeter, optional, zwingend.
        - Nimm auch negative/Scope-Claims auf: "nicht im MVP", "noch nicht entschieden".
        - Jede Obligation braucht 1-4 Evidence-Spans aus dem Transkript.
        - expectedRepresentation beschreibt, WIE das Zielartefakt den Claim behandeln muesste.
        - Keine reinen Themenlabels. Jede Obligation muss als Coverage-Check pruefbar sein.

        expectedRepresentation:
        explicit_requirement | architecture_decision | architecture_constraint | risk_entry |
        open_question | explicit_deferment | context_only

        Antworte ausschliesslich mit JSON:
        {
          "obligations": [
            {
              "sourceClaim": "...",
              "artifact": "requirements|risks|architecture|open-questions",
              "expectedRepresentation": "...",
              "requiredTreatment": "...",
              "evidence": ["..."],
              "kind": "decision|open_question|risk|constraint|requirement|status",
              "importance": "high|medium|low"
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "obligations": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "sourceClaim": { "type": "string" },
                  "artifact": { "type": "string" },
                  "expectedRepresentation": { "type": "string" },
                  "requiredTreatment": { "type": "string" },
                  "evidence": { "type": "array", "items": { "type": "string" } },
                  "kind": { "type": "string" },
                  "importance": { "type": "string" }
                },
                "required": ["sourceClaim", "artifact", "expectedRepresentation", "requiredTreatment", "evidence", "kind", "importance"],
                "additionalProperties": false
              }
            }
          },
          "required": ["obligations"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_obligation_extraction",
        "Evidence-gebundene SourceObligations fuer Coverage-v2.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceObligationExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SourceObligation>> ExtractAsync(string transcript, CancellationToken ct)
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
            .Select((o, i) => o with { Id = $"OB-{NormalizeArtifactForId(o.Artifact)}-{i + 1:D3}" })
            .ToList();
    }

    public static IReadOnlyList<SourceObligation> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<SourceObligation>();
        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (env?.Obligations is null) return Array.Empty<SourceObligation>();
            return env.Obligations
                .Where(o => !string.IsNullOrWhiteSpace(o.SourceClaim))
                .Select(o => o.ToObligation().Trimmed())
                .ToList();
        }
        catch (JsonException)
        {
            return Array.Empty<SourceObligation>();
        }
    }

    private static string NormalizeArtifactForId(string? artifact)
        => NormalizeArtifact(artifact).Replace("-", "").ToUpperInvariant();

    private static string NormalizeArtifact(string? artifact) => artifact?.Trim().ToLowerInvariant() switch
    {
        "requirements" => "requirements",
        "risks" => "risks",
        "architecture" => "architecture",
        "open-questions" => "open-questions",
        "open_questions" => "open-questions",
        "openquestions" => "open-questions",
        _ => artifact?.Trim().ToLowerInvariant() ?? ""
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("obligations")] List<SourceObligationDto>? Obligations);

    private sealed record SourceObligationDto(
        [property: JsonPropertyName("sourceClaim")] string? SourceClaim,
        [property: JsonPropertyName("artifact")] string? Artifact,
        [property: JsonPropertyName("expectedRepresentation")] string? ExpectedRepresentation,
        [property: JsonPropertyName("requiredTreatment")] string? RequiredTreatment,
        [property: JsonPropertyName("evidence")] List<string>? Evidence,
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("importance")] string? Importance)
    {
        public SourceObligation ToObligation()
            => new("", SourceClaim ?? "", NormalizeArtifact(Artifact), ExpectedRepresentation ?? "", RequiredTreatment ?? "", Evidence ?? [], Kind ?? "", Importance ?? "");
    }
}

public sealed record SourceObligation(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("sourceClaim")] string SourceClaim,
    [property: JsonPropertyName("artifact")] string Artifact,
    [property: JsonPropertyName("expectedRepresentation")] string ExpectedRepresentation,
    [property: JsonPropertyName("requiredTreatment")] string RequiredTreatment,
    [property: JsonPropertyName("evidence")] IReadOnlyList<string> Evidence,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("importance")] string Importance)
{
    public SourceObligation Trimmed()
        => this with
        {
            Id = Id?.Trim() ?? "",
            SourceClaim = SourceClaim.Trim(),
            Artifact = Artifact?.Trim().ToLowerInvariant() ?? "",
            ExpectedRepresentation = ExpectedRepresentation?.Trim() ?? "",
            RequiredTreatment = RequiredTreatment?.Trim() ?? "",
            Evidence = Evidence.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList(),
            Kind = Kind?.Trim() ?? "",
            Importance = Importance?.Trim().ToLowerInvariant() ?? ""
        };
}
