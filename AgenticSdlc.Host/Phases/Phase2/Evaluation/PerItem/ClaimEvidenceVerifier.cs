using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

public sealed record ClaimEvidenceCase(
    string Id,
    string Artifact,
    string Claim,
    string ExpectedLabel,
    IReadOnlyList<string> Evidence,
    string? Note = null,
    string? OriginHandLabel = null);

public sealed record ClaimEvidenceVerdict(
    string Id,
    string Verdict,
    string Label,
    bool MatchesExpected,
    string Support,
    bool ModalityPreserved,
    bool StatusPreserved,
    bool ScopePreserved,
    bool TemporalContextPreserved,
    string Reason);

public sealed class ClaimEvidenceVerifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst EINEN atomaren Claim eines SDLC-Artefakts gegen die explizit zugeordnete Evidence.
        Du bekommst NICHT das ganze Transkript, sondern nur Claim + Evidence-Spans. Entscheide streng,
        ob genau diese Evidence genau diesen Claim traegt.

        Pruefe lokal:
        - Wird der Claim explizit gestuetzt?
        - Ist er eine zulaessige Synthese mehrerer Evidence-Spans?
        - Ist er nur eine Inferenz? Wenn ja: sind die Praemissen ausreichend?
        - Wurden Modalitaet, Status, Scope oder Zeitbezug verschaerft?
        - Fuegt der Claim Entitaeten, Technologien, Zahlen oder Eigenschaften hinzu, die die Evidence nicht traegt?
        - Widerspricht der Claim der Evidence?

        VERDIKTE:
        explicit = Claim steht direkt oder nahezu direkt in der Evidence.
        synthesis = Claim verdichtet mehrere Evidence-Spans ohne Bedeutungsverschiebung.
        permissible_inference = Claim folgt nachvollziehbar aus Evidence, ist aber nicht explizit.
        assumption = fachlich moeglich, aber nicht durch Evidence getragen; muesste als Annahme markiert sein.
        overstated = thematischer Kern vorhanden, aber Claim macht Aussage staerker/finaler/allgemeiner.
        unsupported = Claim oder ein relevanter Claim-Bestandteil wird durch Evidence nicht getragen.
        contradicted = Claim widerspricht der Evidence.
        not_a_claim = kein pruefbarer fachlicher Claim.

        LABEL-MAPPING:
        explicit/synthesis/permissible_inference => grounded
        assumption/overstated/unsupported/contradicted => violation
        not_a_claim => borderline

        Antworte ausschliesslich mit JSON:
        {
          "verdict": "explicit|synthesis|permissible_inference|assumption|overstated|unsupported|contradicted|not_a_claim",
          "support": "full|partial|none|contradicted",
          "modalityPreserved": true,
          "statusPreserved": true,
          "scopePreserved": true,
          "temporalContextPreserved": true,
          "reason": "kurz, konkret, evidence-gebunden"
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "verdict": { "type": "string" },
            "support": { "type": "string" },
            "modalityPreserved": { "type": "boolean" },
            "statusPreserved": { "type": "boolean" },
            "scopePreserved": { "type": "boolean" },
            "temporalContextPreserved": { "type": "boolean" },
            "reason": { "type": "string" }
          },
          "required": ["verdict", "support", "modalityPreserved", "statusPreserved", "scopePreserved", "temporalContextPreserved", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "claim_evidence_verification",
        "Lokale Claim-Evidence-Grounding-Verifikation.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public ClaimEvidenceVerifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<ClaimEvidenceVerdict> VerifyAsync(ClaimEvidenceCase testCase, CancellationToken ct)
    {
        var evidence = string.Join("\n", testCase.Evidence.Select((e, i) => $"E{i + 1}: {e}"));
        var user = $"""
            ID: {testCase.Id}
            ARTEFAKT: {testCase.Artifact}

            CLAIM:
            {testCase.Claim}

            EVIDENCE:
            {evidence}

            ERWARTETES HANDLABEL:
            {testCase.ExpectedLabel}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput)
            options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options,
            ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        if (parsed is null)
        {
            return new ClaimEvidenceVerdict(
                testCase.Id,
                "unclassified",
                "borderline",
                string.Equals(NormalizeLabel(testCase.ExpectedLabel), "borderline", StringComparison.Ordinal),
                "none",
                false,
                false,
                false,
                false,
                "kein parsebares Verdict erhalten");
        }

        var verdict = NormalizeVerdict(parsed.Verdict);
        var label = LabelFor(verdict);
        var expected = NormalizeLabel(testCase.ExpectedLabel);

        return new ClaimEvidenceVerdict(
            testCase.Id,
            verdict,
            label,
            string.Equals(label, expected, StringComparison.Ordinal),
            NormalizeSupport(parsed.Support),
            parsed.ModalityPreserved,
            parsed.StatusPreserved,
            parsed.ScopePreserved,
            parsed.TemporalContextPreserved,
            parsed.Reason ?? string.Empty);
    }

    public static VerificationPayload? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;

        try { return JsonSerializer.Deserialize<VerificationPayload>(json, Json); }
        catch (JsonException) { return null; }
    }

    public static string LabelFor(string verdict) => verdict switch
    {
        "explicit" or "synthesis" or "permissible_inference" => "grounded",
        "assumption" or "overstated" or "unsupported" or "contradicted" => "violation",
        _ => "borderline"
    };

    private static string NormalizeVerdict(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "explicit" => "explicit",
        "synthesis" => "synthesis",
        "permissible_inference" => "permissible_inference",
        "inference" => "permissible_inference",
        "assumption" => "assumption",
        "recommendation" => "assumption",
        "overstated" => "overstated",
        "unsupported" => "unsupported",
        "fabricated" => "unsupported",
        "contradicted" => "contradicted",
        "not_a_claim" => "not_a_claim",
        _ => "not_a_claim"
    };

    private static string NormalizeLabel(string? label) => label?.Trim().ToLowerInvariant() switch
    {
        "grounded" => "grounded",
        "violation" => "violation",
        _ => "borderline"
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

    public sealed record VerificationPayload(
        [property: JsonPropertyName("verdict")] string? Verdict,
        [property: JsonPropertyName("support")] string? Support,
        [property: JsonPropertyName("modalityPreserved")] bool ModalityPreserved,
        [property: JsonPropertyName("statusPreserved")] bool StatusPreserved,
        [property: JsonPropertyName("scopePreserved")] bool ScopePreserved,
        [property: JsonPropertyName("temporalContextPreserved")] bool TemporalContextPreserved,
        [property: JsonPropertyName("reason")] string? Reason);
}
