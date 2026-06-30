using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// V2 Matrix-Judge: trennt Applicability und Coverage und schaerft partial/contradicted.
/// Isolierter Spike, nicht produktive Review-Pipeline.
/// </summary>
public sealed class SourceClaimCoverageMatrixJudgeV2
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst einen SourceClaim aus einem Stakeholder-Transkript gegen EIN SDLC-Artefakt.

        Wichtig: Entscheide in ZWEI getrennten Schritten.

        Schritt 1: applicability
        - required: Dieses Artefakt muss den Claim explizit behandeln; sonst ist es unvollstaendig/irrefuehrend.
        - optional: Der Claim ist sinnvoll/relevant, aber keine harte Coverage-Pflicht fuer dieses Artefakt.
        - context: Der Claim hilft als Hintergrund, ist aber kein eigener Coverage-Check fuer dieses Artefakt.
        - not_applicable: Der Claim gehoert fachlich nicht in dieses Artefakt.
        - unclear: Nicht sicher entscheidbar.

        Schritt 2: coverage
        - covered: Alle fuer dieses Artefakt erforderlichen Facetten sind vorhanden.
        - partial: NUR wenn eine konkrete Teilfacette im Artefakt nachweislich vorhanden ist, aber wichtige
          Facetten fehlen. Benenne vorhandene und fehlende Facetten.
        - missing: Wenn keine ausreichende artefaktspezifische Behandlung vorhanden ist. Verwende missing statt
          partial, wenn nur ein sehr grobes Thema oder Kontext vorkommt.
        - contradicted: Wenn das Artefakt Status, Modalitaet, Scope oder Bedeutung veraendert:
          offen -> entschieden, unentschieden -> geplant, MVP -> spaeter, spaeter -> MVP, optional -> Pflicht.
        - not_applicable: Wenn applicability not_applicable oder context ist.
        - unclear: Wenn die Relation nicht verlaesslich entscheidbar ist.

        Strenge Regeln:
        - partial ist kein Ausweichlabel.
        - Status/Modalitaet/Scope sind entscheidend.
        - Ein allgemeines DSGVO-/Risiko-/Architekturthema reicht nicht fuer partial, wenn die konkrete SourceClaim-
          Facette fehlt.
        - Bei Widerspruch zu Status/Modalitaet/Scope immer contradicted statt partial.
        - Zitiere die relevante Artefaktstelle, wenn covered/partial/contradicted.

        Antworte ausschliesslich mit JSON:
        {
          "applicability": "required|optional|context|not_applicable|unclear",
          "coverage": "covered|partial|missing|contradicted|not_applicable|unclear",
          "support": "full|partial|none|contradicted|not_applicable",
          "artifactQuote": "kurze Artefaktstelle oder leer",
          "presentFacets": ["..."],
          "missingFacets": ["..."],
          "reason": "kurz"
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "applicability": { "type": "string" },
            "coverage": { "type": "string" },
            "support": { "type": "string" },
            "artifactQuote": { "type": "string" },
            "presentFacets": { "type": "array", "items": { "type": "string" } },
            "missingFacets": { "type": "array", "items": { "type": "string" } },
            "reason": { "type": "string" }
          },
          "required": ["applicability", "coverage", "support", "artifactQuote", "presentFacets", "missingFacets", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_claim_coverage_matrix_v2",
        "Applicability und Coverage eines SourceClaims gegen ein Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimCoverageMatrixJudgeV2(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<SourceClaimMatrixVerdictV2> ClassifyAsync(
        SourceClaimCoverageCase testCase,
        string artifactText,
        CancellationToken ct)
    {
        var evidence = string.Join("\n", testCase.Evidence.Select((e, i) => $"E{i + 1}: {e}"));
        var user = $"""
            ID: {testCase.Id}
            ARTEFAKT: {testCase.Artifact}

            SOURCE-CLAIM:
            {testCase.SourceClaim}

            ERWARTETE BEHANDLUNG / PRUEFHINWEIS:
            {testCase.RequiredTreatment}

            EVIDENCE:
            {evidence}

            ARTEFAKT:
            {artifactText}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        if (parsed is null)
        {
            return new SourceClaimMatrixVerdictV2("unclassified", "unclassified", "none", "", [], [], "parse failed");
        }

        return parsed.Normalized();
    }

    public static SourceClaimMatrixVerdictV2? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SourceClaimMatrixVerdictV2>(json, Json); }
        catch (JsonException) { return null; }
    }

    public static string NormalizeApplicability(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "required" => "required",
        "optional" => "optional",
        "context" => "context",
        "context_only" => "context",
        "not_applicable" => "not_applicable",
        "unclear" => "unclear",
        _ => "unclear"
    };

    public static string NormalizeCoverage(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "covered" => "covered",
        "partial" => "partial",
        "missing" => "missing",
        "contradicted" => "contradicted",
        "not_applicable" => "not_applicable",
        "unclear" => "unclear",
        _ => "unclear"
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

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    public sealed record SourceClaimMatrixVerdictV2(
        [property: JsonPropertyName("applicability")] string Applicability,
        [property: JsonPropertyName("coverage")] string Coverage,
        [property: JsonPropertyName("support")] string Support,
        [property: JsonPropertyName("artifactQuote")] string ArtifactQuote,
        [property: JsonPropertyName("presentFacets")] IReadOnlyList<string> PresentFacets,
        [property: JsonPropertyName("missingFacets")] IReadOnlyList<string> MissingFacets,
        [property: JsonPropertyName("reason")] string Reason)
    {
        public SourceClaimMatrixVerdictV2 Normalized()
            => this with
            {
                Applicability = NormalizeApplicability(Applicability),
                Coverage = NormalizeCoverage(Coverage),
                Support = NormalizeSupport(Support),
                ArtifactQuote = ArtifactQuote?.Trim() ?? "",
                PresentFacets = PresentFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
                MissingFacets = MissingFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
                Reason = Reason?.Trim() ?? ""
            };
    }
}
