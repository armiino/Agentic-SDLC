using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>LLM-Matcher fuer erwartete Semantic-Ledger-Eintraege gegen automatisch extrahierte Eintraege.</summary>
public sealed class SemanticLedgerRecallMatcher
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du vergleichst EINEN erwarteten Semantic-Ledger-Eintrag mit einem automatisch extrahierten Ledger.

        Aufgabe:
        - Finde den oder die besten Matches im extrahierten Ledger.
        - Bewerte nicht nur die Proposition, sondern auch status, modality, scope, timeScope und evidence.

        verdict:
        - exact: Proposition und relevante Facetten sind erhalten.
        - partial: Proposition/Kern ist vorhanden, aber eine wichtige Facette fehlt oder ist grober.
        - missed: Kein ausreichender Eintrag vorhanden.

        Facetten:
        - propositionMatch: exact|partial|missed
        - statusMatch: exact|partial|missed
        - modalityMatch: exact|partial|missed
        - scopeMatch: exact|partial|missed
        - timeScopeMatch: exact|partial|missed
        - evidenceMatch: exact|partial|missed
        - dispositionMatch: exact|partial|missed

        Strenge Regeln:
        - "offen/undecided" vs "planned/required/decided" ist NICHT exact.
        - "nicht MVP/spaeter moeglich" vs "MVP" ist NICHT exact.
        - Ein grobes Topic ohne konkrete Facette ist maximal partial.
        - Paraphrasen sind exact, wenn Status/Scope/Modalitaet erhalten sind.

        Antworte ausschliesslich mit JSON:
        {
          "verdict": "exact|partial|missed",
          "matchedIds": ["..."],
          "propositionMatch": "exact|partial|missed",
          "statusMatch": "exact|partial|missed",
          "modalityMatch": "exact|partial|missed",
          "scopeMatch": "exact|partial|missed",
          "timeScopeMatch": "exact|partial|missed",
          "evidenceMatch": "exact|partial|missed",
          "dispositionMatch": "exact|partial|missed",
          "reason": "kurz"
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "verdict": { "type": "string" },
            "matchedIds": { "type": "array", "items": { "type": "string" } },
            "propositionMatch": { "type": "string" },
            "statusMatch": { "type": "string" },
            "modalityMatch": { "type": "string" },
            "scopeMatch": { "type": "string" },
            "timeScopeMatch": { "type": "string" },
            "evidenceMatch": { "type": "string" },
            "dispositionMatch": { "type": "string" },
            "reason": { "type": "string" }
          },
          "required": ["verdict", "matchedIds", "propositionMatch", "statusMatch", "modalityMatch", "scopeMatch", "timeScopeMatch", "evidenceMatch", "dispositionMatch", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "semantic_ledger_recall_match",
        "Recall-Match eines Semantic-Ledger-Eintrags gegen extrahierten Ledger.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SemanticLedgerRecallMatcher(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<SemanticLedgerRecallVerdict> MatchAsync(
        SemanticLedgerEntry expected,
        IReadOnlyList<SemanticLedgerEntry> extracted,
        CancellationToken ct)
    {
        var listing = string.Join("\n\n", extracted.Select(e => $"""
            ID: {e.Id}
            PROPOSITION: {e.Proposition}
            KIND: {e.Kind}
            STATUS: {e.Status}
            MODALITY: {e.Modality}
            SCOPE: {e.Scope}
            TIME_SCOPE: {e.TimeScope}
            EVIDENCE: {string.Join(" | ", e.Evidence.Select(ev => ev.Quote))}
            DISPOSITION: {JsonSerializer.Serialize(e.Disposition, Json)}
            """));

        var user = $"""
            ERWARTETER LEDGER-EINTRAG:
            ID: {expected.Id}
            PROPOSITION: {expected.Proposition}
            KIND: {expected.Kind}
            STATUS: {expected.Status}
            MODALITY: {expected.Modality}
            SCOPE: {expected.Scope}
            TIME_SCOPE: {expected.TimeScope}
            EVIDENCE: {string.Join(" | ", expected.Evidence.Select(e => e.Quote))}
            DISPOSITION: {JsonSerializer.Serialize(expected.Disposition, Json)}

            EXTRAHIERTER LEDGER:
            {listing}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)?.Normalized() ?? new SemanticLedgerRecallVerdict(
            "unclassified", [], "missed", "missed", "missed", "missed", "missed", "missed", "missed", "parse failed");
    }

    public static SemanticLedgerRecallVerdict? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SemanticLedgerRecallVerdict>(json, Json); }
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

public sealed record SemanticLedgerRecallVerdict(
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("matchedIds")] IReadOnlyList<string> MatchedIds,
    [property: JsonPropertyName("propositionMatch")] string PropositionMatch,
    [property: JsonPropertyName("statusMatch")] string StatusMatch,
    [property: JsonPropertyName("modalityMatch")] string ModalityMatch,
    [property: JsonPropertyName("scopeMatch")] string ScopeMatch,
    [property: JsonPropertyName("timeScopeMatch")] string TimeScopeMatch,
    [property: JsonPropertyName("evidenceMatch")] string EvidenceMatch,
    [property: JsonPropertyName("dispositionMatch")] string DispositionMatch,
    [property: JsonPropertyName("reason")] string Reason)
{
    public SemanticLedgerRecallVerdict Normalized()
        => this with
        {
            Verdict = Normalize(Verdict),
            MatchedIds = MatchedIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList(),
            PropositionMatch = Normalize(PropositionMatch),
            StatusMatch = Normalize(StatusMatch),
            ModalityMatch = Normalize(ModalityMatch),
            ScopeMatch = Normalize(ScopeMatch),
            TimeScopeMatch = Normalize(TimeScopeMatch),
            EvidenceMatch = Normalize(EvidenceMatch),
            DispositionMatch = Normalize(DispositionMatch),
            Reason = Reason.Trim()
        };

    public static string Normalize(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "exact" => "exact",
        "partial" => "partial",
        "missed" => "missed",
        _ => "missed"
    };
}
