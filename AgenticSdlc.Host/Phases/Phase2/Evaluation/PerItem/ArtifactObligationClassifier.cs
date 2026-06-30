using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Coverage-v2-Spike: ersetzt grobes relevantFor durch eine artefaktspezifische Pflichtbewertung.
/// Nicht Teil der produktiven Review-Pipeline.
/// </summary>
public sealed class ArtifactObligationClassifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du klassifizierst, ob SourceClaims aus einem Stakeholder-Transkript in einem bestimmten SDLC-Artefakt
        behandelt werden muessen.

        Zielartefakte:
        - requirements: fachliche/nichtfunktionale Anforderungen, Constraints, Scope-Status
        - risks: Risiken, Unsicherheiten, Abhaengigkeiten, Konflikte, Compliance-/Terminrisiken
        - architecture: technische Entscheidungen, Constraints, Integrationen, Daten, Security, Betrieb, offene Architekturentscheidungen
        - open-questions: offene fachliche/technische/organisatorische Fragen, die entschieden werden muessen

        Klassifiziere jeden Claim fuer GENAU DAS angegebene Zielartefakt.

        necessity:
        - required: Das Artefakt ist ohne explizite Behandlung dieses Claims fachlich unvollstaendig oder irrefuehrend.
        - optional: Der Claim waere hilfreich, ist aber keine harte Vollstaendigkeits-Pflicht fuer dieses Artefakt.
        - context_only: Der Claim beeinflusst das Verstaendnis, aber sollte nicht als eigener Coverage-Pflichtclaim geprueft werden.
        - not_applicable: Der Claim gehoert nicht sinnvoll in dieses Artefakt.
        - unclear: Nicht entscheidbar.

        expectedRepresentation:
        explicit_requirement | architecture_decision | architecture_constraint | risk_entry |
        open_question | explicit_deferment | context_only | not_applicable

        Regeln:
        - Sei recall-first bei required: lieber required/optional als not_applicable, wenn ein echtes Missing sonst unsichtbar wuerde.
        - Trotzdem darf "irgendwie relevant" nicht automatisch required sein.
        - Status/Modalitaet beachten: offen, entschieden, MVP, spaeter, optional, zwingend.
        - Begruende kurz artefaktspezifisch.

        Antworte ausschliesslich mit JSON:
        {
          "obligations": [
            {
              "sourceClaimId": "GLOBAL-SC-001",
              "necessity": "required|optional|context_only|not_applicable|unclear",
              "expectedRepresentation": "...",
              "importance": "high|medium|low",
              "reason": "kurz"
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
                  "sourceClaimId": { "type": "string" },
                  "necessity": { "type": "string" },
                  "expectedRepresentation": { "type": "string" },
                  "importance": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["sourceClaimId", "necessity", "expectedRepresentation", "importance", "reason"],
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
        "artifact_obligations",
        "Artefaktspezifische SourceClaim-Pflichten.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public ArtifactObligationClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<ArtifactObligation>> ClassifyAsync(
        string artifactType,
        IReadOnlyList<GlobalSourceClaim> claims,
        CancellationToken ct)
    {
        var listing = string.Join("\n\n", claims.Select(c => $"""
            ID: {c.Id}
            CLAIM: {c.SourceClaim}
            TREATMENT: {c.RequiredTreatment}
            KIND: {c.Kind}
            PRIORITY: {c.Priority}
            EVIDENCE: {string.Join(" | ", c.Evidence)}
            """));

        var user = $"""
            ZIELARTEFAKT: {artifactType}

            SOURCECLAIMS:
            {listing}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)
            .Select(o => o with { Artifact = artifactType })
            .ToList();
    }

    public static IReadOnlyList<ArtifactObligation> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<ArtifactObligation>();
        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(json, Json);
            return env?.Obligations?.Select(o => o.Trimmed()).ToList() ?? [];
        }
        catch (JsonException)
        {
            return Array.Empty<ArtifactObligation>();
        }
    }

    public static string NormalizeNecessity(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "required" => "required",
        "optional" => "optional",
        "context_only" => "context_only",
        "not_applicable" => "not_applicable",
        "unclear" => "unclear",
        _ => "unclear"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("obligations")] List<ArtifactObligation>? Obligations);
}

public sealed record ArtifactObligation(
    [property: JsonPropertyName("sourceClaimId")] string SourceClaimId,
    [property: JsonPropertyName("artifact")] string Artifact,
    [property: JsonPropertyName("necessity")] string Necessity,
    [property: JsonPropertyName("expectedRepresentation")] string ExpectedRepresentation,
    [property: JsonPropertyName("importance")] string Importance,
    [property: JsonPropertyName("reason")] string Reason)
{
    public ArtifactObligation Trimmed()
        => this with
        {
            SourceClaimId = SourceClaimId?.Trim() ?? "",
            Artifact = Artifact?.Trim().ToLowerInvariant() ?? "",
            Necessity = ArtifactObligationClassifier.NormalizeNecessity(Necessity),
            ExpectedRepresentation = ExpectedRepresentation?.Trim() ?? "",
            Importance = Importance?.Trim().ToLowerInvariant() ?? "",
            Reason = Reason?.Trim() ?? ""
        };
}
