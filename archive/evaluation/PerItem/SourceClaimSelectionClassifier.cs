using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Coverage-v2-Spike: konservative Auswahl aus einem Global SourceClaim Ledger pro Zielartefakt.
/// Nicht Teil der produktiven Review-Pipeline.
/// </summary>
public sealed class SourceClaimSelectionClassifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du waehlst aus einem GLOBALEN SourceClaim-Ledger die Claims aus, die fuer EIN Zielartefakt als Coverage-Checks
        geprueft werden sollten.

        Ziel: Kandidaten reduzieren, ohne wichtige Missing/Partial/Contradicted-Faelle zu verlieren.

        Zielartefakte:
        - requirements: Anforderungen, Constraints, Scope, Status, MVP/spaeter
        - risks: Risiken, Unsicherheiten, Abhaengigkeiten, Konflikte, Compliance-/Terminrisiken
        - architecture: technische Entscheidungen, Constraints, Integrationen, Daten, Security, Betrieb, offene Architekturentscheidungen
        - open-questions: offene Fragen, die entschieden oder explizit geklaert werden muessen

        selection:
        - must_check: Wenn das Artefakt diesen Claim behandeln muss; Fehlen/Falschdarstellung waere ein relevanter Defekt.
        - should_check: Wenn der Claim plausibel coverage-relevant ist, aber nicht sicher harte Pflicht. Recall-first behalten.
        - context_only: Hilft zum Verstehen, sollte aber nicht als eigener Coverage-Check laufen.
        - skip: Fuer dieses Artefakt nicht sinnvoll.

        Regeln:
        - Recall-first: zentrale Claims duerfen nicht verloren gehen.
        - "Irgendwie relevant" ist nicht automatisch must_check.
        - Wenn unsicher zwischen should_check und context_only: nimm should_check, falls ein Missing sonst unsichtbar wuerde.
        - Status/Modalitaet beachten: offen, entschieden, MVP, spaeter, optional, zwingend.
        - Gib expectedRepresentation an, damit spaeter klar ist, was gesucht wird.

        expectedRepresentation:
        explicit_requirement | architecture_decision | architecture_constraint | risk_entry |
        open_question | explicit_deferment | context_only | not_applicable

        Antworte ausschliesslich mit JSON:
        {
          "selections": [
            {
              "sourceClaimId": "GLOBAL-SC-001",
              "selection": "must_check|should_check|context_only|skip",
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
            "selections": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "sourceClaimId": { "type": "string" },
                  "selection": { "type": "string" },
                  "expectedRepresentation": { "type": "string" },
                  "importance": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["sourceClaimId", "selection", "expectedRepresentation", "importance", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["selections"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_claim_selection",
        "Konservative Coverage-Auswahl pro Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimSelectionClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SourceClaimSelection>> ClassifyAsync(
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
            CURRENT_RELEVANT_FOR: {string.Join(", ", c.RelevantFor)}
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
            .Select(s => s with { Artifact = artifactType })
            .ToList();
    }

    public static IReadOnlyList<SourceClaimSelection> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<SourceClaimSelection>();
        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(json, Json);
            return env?.Selections?.Select(s => s.Trimmed()).ToList() ?? [];
        }
        catch (JsonException)
        {
            return Array.Empty<SourceClaimSelection>();
        }
    }

    public static string NormalizeSelection(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "must_check" => "must_check",
        "should_check" => "should_check",
        "context_only" => "context_only",
        "skip" => "skip",
        _ => "context_only"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("selections")] List<SourceClaimSelection>? Selections);
}

public sealed record SourceClaimSelection(
    [property: JsonPropertyName("sourceClaimId")] string SourceClaimId,
    [property: JsonPropertyName("artifact")] string Artifact,
    [property: JsonPropertyName("selection")] string Selection,
    [property: JsonPropertyName("expectedRepresentation")] string ExpectedRepresentation,
    [property: JsonPropertyName("importance")] string Importance,
    [property: JsonPropertyName("reason")] string Reason)
{
    public SourceClaimSelection Trimmed()
        => this with
        {
            SourceClaimId = SourceClaimId?.Trim() ?? "",
            Artifact = Artifact?.Trim().ToLowerInvariant() ?? "",
            Selection = SourceClaimSelectionClassifier.NormalizeSelection(Selection),
            ExpectedRepresentation = ExpectedRepresentation?.Trim() ?? "",
            Importance = Importance?.Trim().ToLowerInvariant() ?? "",
            Reason = Reason?.Trim() ?? ""
        };
}
