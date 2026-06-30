using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Batch-Variante des Matrix-v2-Judges: viele SourceClaims gegen genau ein Artefakt.
/// Isolierter Kosten-/Robustheits-Spike, nicht produktive Pipeline.
/// </summary>
public sealed class SourceClaimCoverageMatrixBatchJudge
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst mehrere SourceClaims aus einem Stakeholder-Transkript gegen GENAU EIN SDLC-Artefakt.

        Gib fuer jeden SourceClaim ein eigenes Urteil aus. Trenne strikt:

        applicability:
        - required: Dieses Artefakt muss den Claim explizit behandeln.
        - optional: Sinnvoll/relevant, aber keine harte Coverage-Pflicht.
        - context: Hintergrund, aber kein eigener Coverage-Check.
        - not_applicable: Fachlich nicht Aufgabe dieses Artefakts.
        - unclear: Nicht sicher entscheidbar.

        coverage:
        - covered: Alle fuer dieses Artefakt erforderlichen Facetten sind vorhanden.
        - partial: Nur wenn eine konkrete Claim-Facette im Artefakt nachweislich vorhanden ist und andere
          erforderliche Facetten fehlen.
        - missing: Keine ausreichende artefaktspezifische Behandlung. Verwende missing statt partial, wenn nur
          ein grobes Thema oder eine generische Naehe vorkommt.
        - contradicted: Status, Modalitaet, Scope oder Bedeutung werden veraendert
          (offen -> entschieden, unentschieden -> geplant, MVP -> spaeter, spaeter -> MVP, optional -> Pflicht).
        - not_applicable: Wenn applicability not_applicable oder context ist.
        - unclear: Nicht verlaesslich entscheidbar.

        Nachgezogene Schwachstellen aus dem Einzelzellen-Run:
        - Thematische Naehe reicht nicht fuer required. Pruefe die Aufgabe des Artefakts.
        - Eine bereits entschiedene Sache ist nicht automatisch eine offene Frage.
        - Generische DSGVO-/Risiko-/Architektur-Erwaehnungen reichen nicht automatisch fuer partial.
        - Gleichzeitig darf ein Artefakt nicht ueberbestraft werden: Wenn es genau den noetigen Artefaktzweck
          erfuellt, ist covered auch ohne Detailtiefe anderer Artefakte moeglich.
        - Bei requirements.md sind Scope, MVP-Status, fachliche Regeln und offene fachliche/technische Anforderungen
          besonders wichtig. Reine Architektur-Implementierungsdetails sind nur optional/context, wenn kein
          Requirements-Entscheid gebraucht wird.

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "id": "SourceClaim-ID",
              "applicability": "required|optional|context|not_applicable|unclear",
              "coverage": "covered|partial|missing|contradicted|not_applicable|unclear",
              "support": "full|partial|none|contradicted|not_applicable",
              "artifactQuote": "kurze Artefaktstelle oder leer",
              "presentFacets": ["..."],
              "missingFacets": ["..."],
              "reason": "kurz"
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "id": { "type": "string" },
                  "applicability": { "type": "string" },
                  "coverage": { "type": "string" },
                  "support": { "type": "string" },
                  "artifactQuote": { "type": "string" },
                  "presentFacets": { "type": "array", "items": { "type": "string" } },
                  "missingFacets": { "type": "array", "items": { "type": "string" } },
                  "reason": { "type": "string" }
                },
                "required": ["id", "applicability", "coverage", "support", "artifactQuote", "presentFacets", "missingFacets", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["items"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_claim_coverage_matrix_batch",
        "Batch-Applicability und Coverage mehrerer SourceClaims gegen ein Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimCoverageMatrixBatchJudge(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SourceClaimMatrixBatchItem>> ClassifyAsync(
        IReadOnlyList<SourceClaimCoverageCase> cases,
        string artifact,
        string artifactText,
        CancellationToken ct)
    {
        var claims = string.Join("\n\n", cases.Select(c =>
            $"""
            ID: {c.Id}
            SOURCE-CLAIM: {c.SourceClaim}
            EVIDENCE:
            {string.Join("\n", c.Evidence.Select((e, i) => $"E{i + 1}: {e}"))}
            """));

        var user = $"""
            ARTEFAKT-TYP: {artifact}

            SOURCE-CLAIMS:
            {claims}

            ARTEFAKT:
            {artifactText}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)?.Items.Select(i => i.Normalized()).ToList() ?? [];
    }

    public static SourceClaimMatrixBatchPayload? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SourceClaimMatrixBatchPayload>(json, Json); }
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

public sealed record SourceClaimMatrixBatchPayload(
    [property: JsonPropertyName("items")] IReadOnlyList<SourceClaimMatrixBatchItem> Items);

public sealed record SourceClaimMatrixBatchItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("applicability")] string Applicability,
    [property: JsonPropertyName("coverage")] string Coverage,
    [property: JsonPropertyName("support")] string Support,
    [property: JsonPropertyName("artifactQuote")] string ArtifactQuote,
    [property: JsonPropertyName("presentFacets")] IReadOnlyList<string> PresentFacets,
    [property: JsonPropertyName("missingFacets")] IReadOnlyList<string> MissingFacets,
    [property: JsonPropertyName("reason")] string Reason)
{
    public SourceClaimMatrixBatchItem Normalized()
        => this with
        {
            Id = Id.Trim(),
            Applicability = SourceClaimCoverageMatrixJudgeV2.NormalizeApplicability(Applicability),
            Coverage = SourceClaimCoverageMatrixJudgeV2.NormalizeCoverage(Coverage),
            Support = NormalizeSupport(Support),
            ArtifactQuote = ArtifactQuote?.Trim() ?? "",
            PresentFacets = PresentFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
            MissingFacets = MissingFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
            Reason = Reason?.Trim() ?? ""
        };

    private static string NormalizeSupport(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "full" => "full",
        "partial" => "partial",
        "none" => "none",
        "contradicted" => "contradicted",
        "not_applicable" => "not_applicable",
        _ => "none"
    };
}
