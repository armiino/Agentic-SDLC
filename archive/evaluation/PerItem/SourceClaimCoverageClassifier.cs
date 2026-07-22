using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Coverage-Spike: prueft source-native Claims gegen EIN Artefakt.
/// Richtung: Quelle -> Artefakt. Anders als TopicCoverage wird nicht ein aggregiertes Topic,
/// sondern ein konkreter SourceClaim mit Evidence und erwarteter Artefaktpflicht bewertet.
/// </summary>
public sealed class SourceClaimCoverageClassifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst, ob ein SDLC-Artefakt einen SOURCE-CLAIM aus dem Transkript angemessen behandelt.
        Du bekommst:
        - den sourceClaim,
        - Evidence-Spans aus dem Transkript,
        - die erwartete Behandlung/Pflicht im Artefakt,
        - das vollstaendige Artefakt.

        Beurteile GENAU diesen SourceClaim fuer GENAU dieses Artefakt.

        VERDIKTE:
        covered = Der SourceClaim ist im Artefakt vollstaendig und angemessen behandelt
                  (auch sinngemaess/paraphrasiert).
        partial = Der SourceClaim ist erkennbar behandelt, aber eine wichtige Facette/Status/Modalitaet fehlt
                  oder ist zu grob.
        missing = Der SourceClaim muesste in diesem Artefakt behandelt werden, ist aber nicht vorhanden.
        contradicted = Das Artefakt behandelt den Claim, aber widerspricht der Source-Evidence oder stellt ihn
                       falsch/finaler dar.
        not_applicable = Der SourceClaim ist fuer diesen Artefakttyp trotz Fixture-Hinweis nicht sinnvoll zu erwarten.

        Strenge Regeln:
        - Vor missing das GANZE Artefakt auf sinngemaesse Entsprechung pruefen.
        - Status und Modalitaet sind wichtig: offen vs. entschieden, MVP vs. spaeter, Muss vs. Option.
        - Eine kurze Erwaehnung reicht fuer covered nur, wenn die erwartete Behandlung wirklich erfuellt ist.
        - Wenn der fachliche Kern da ist, aber Status/Scope/Repair-Hinweis fehlt: eher partial als missing.

        Antworte ausschliesslich mit JSON:
        {
          "verdict": "covered|partial|missing|contradicted|not_applicable",
          "support": "full|partial|none|contradicted",
          "artifactQuote": "kurze Artefaktstelle oder leer",
          "missingFacet": "kurz, was fehlt oder leer",
          "reason": "kurz, evidence- und artefaktgebunden"
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "verdict": { "type": "string" },
            "support": { "type": "string" },
            "artifactQuote": { "type": "string" },
            "missingFacet": { "type": "string" },
            "reason": { "type": "string" }
          },
          "required": ["verdict", "support", "artifactQuote", "missingFacet", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "source_claim_coverage",
        "Coverage-Verdikt eines SourceClaims gegen ein Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SourceClaimCoverageClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<SourceClaimCoverageVerdict> ClassifyAsync(
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

            ERWARTETE BEHANDLUNG IM ARTEFAKT:
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
            return new SourceClaimCoverageVerdict(
                testCase.Id,
                "unclassified",
                "none",
                false,
                "",
                "parse failed",
                "kein parsebares Verdict erhalten");
        }

        var verdict = NormalizeVerdict(parsed.Verdict);
        var expected = NormalizeVerdict(testCase.ExpectedVerdict);
        return new SourceClaimCoverageVerdict(
            testCase.Id,
            verdict,
            NormalizeSupport(parsed.Support),
            string.Equals(verdict, expected, StringComparison.Ordinal),
            parsed.ArtifactQuote?.Trim() ?? "",
            parsed.MissingFacet?.Trim() ?? "",
            parsed.Reason?.Trim() ?? "");
    }

    public static SourceClaimCoveragePayload? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SourceClaimCoveragePayload>(json, Json); }
        catch (JsonException) { return null; }
    }

    public static string NormalizeVerdict(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "covered" => "covered",
        "partial" => "partial",
        "missing" => "missing",
        "contradicted" => "contradicted",
        "not_applicable" => "not_applicable",
        _ => "unclassified"
    };

    private static string NormalizeSupport(string? support) => support?.Trim().ToLowerInvariant() switch
    {
        "full" => "full",
        "partial" => "partial",
        "none" => "none",
        "contradicted" => "contradicted",
        _ => "none"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}

public sealed record SourceClaimCoverageCase(
    string Id,
    string Artifact,
    string SourceClaim,
    string ExpectedVerdict,
    string RequiredTreatment,
    IReadOnlyList<string> Evidence,
    string? TopicCoverageBaseline = null,
    string? DirectReviewBaseline = null,
    string? Note = null);

public sealed record SourceClaimCoverageVerdict(
    string Id,
    string Verdict,
    string Support,
    bool MatchesExpected,
    string ArtifactQuote,
    string MissingFacet,
    string Reason);

public sealed record SourceClaimCoveragePayload(
    [property: JsonPropertyName("verdict")] string? Verdict,
    [property: JsonPropertyName("support")] string? Support,
    [property: JsonPropertyName("artifactQuote")] string? ArtifactQuote,
    [property: JsonPropertyName("missingFacet")] string? MissingFacet,
    [property: JsonPropertyName("reason")] string? Reason);
