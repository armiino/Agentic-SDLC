using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

internal sealed class UnusedUnitLedgerComparer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const int BatchSize = 30;

    private const string SystemPrompt = """
        Du vergleichst potenziell relevante, aber nicht direkt referenzierte Atomic Units gegen einen bestehenden Candidate Ledger.

        Ziel:
        - Entscheide, ob die Unit semantisch bereits im Ledger enthalten ist oder ob ein Claim/Evidence fehlt.
        - Nicht den Ledger neu schreiben.
        - Wenn die Unit nur zusaetzliche Evidence/Detail fuer einen bestehenden Claim ist: attach_as_evidence.
        - Wenn die Unit voll semantisch enthalten ist: already_covered_indirectly.
        - Wenn eine eigenstaendige wichtige Aussage fehlt: missing_claim.
        - Wenn unklar: needs_human.

        Erlaubte verdict-Werte:
        already_covered_indirectly
        attach_as_evidence
        missing_claim
        needs_human

        Erlaubte suggestedAction-Werte:
        link_existing_candidate
        attach_evidence
        create_candidate
        human_review

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "verdict": "already_covered_indirectly|attach_as_evidence|missing_claim|needs_human",
              "suggestedAction": "link_existing_candidate|attach_evidence|create_candidate|human_review",
              "reason": "kurz",
              "suggestedProposition": "nur bei missing_claim/needs_human, sonst null",
              "relatedCandidateIds": ["R1"]
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
                  "unitId": { "type": "string" },
                  "verdict": {
                    "type": "string",
                    "enum": ["already_covered_indirectly", "attach_as_evidence", "missing_claim", "needs_human"]
                  },
                  "suggestedAction": {
                    "type": "string",
                    "enum": ["link_existing_candidate", "attach_evidence", "create_candidate", "human_review"]
                  },
                  "reason": { "type": "string" },
                  "suggestedProposition": { "type": ["string", "null"] },
                  "relatedCandidateIds": {
                    "type": "array",
                    "items": { "type": "string" }
                  }
                },
                "required": ["unitId", "verdict", "suggestedAction", "reason", "suggestedProposition", "relatedCandidateIds"],
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
        "unused_unit_ledger_compare",
        "Vergleich potenziell relevanter unused Units gegen Candidate Ledger.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public UnusedUnitLedgerComparer(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<UnusedUnitLedgerCompareItem>> CompareAsync(
        IReadOnlyList<AtomicUnit> relevantUnits,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        var all = new List<UnusedUnitLedgerCompareItem>();
        foreach (var batch in relevantUnits.Chunk(BatchSize))
            all.AddRange(await CompareBatchAsync(batch, candidates, ct).ConfigureAwait(false));
        return all;
    }

    private async Task<IReadOnlyList<UnusedUnitLedgerCompareItem>> CompareBatchAsync(
        IReadOnlyList<AtomicUnit> relevantUnits,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        if (relevantUnits.Count == 0) return [];

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var payload = new
        {
            potentiallyRelevantUnusedUnits = relevantUnits.Select(u => new { u.Id, u.Speaker, u.Text }),
            existingCandidates = candidates.Select(c => new
            {
                c.Id,
                c.Proposition,
                c.Status,
                c.Modality,
                c.Scope,
                c.TimeScope,
                sourceUnitIds = c.SourceUnitIds ?? []
            })
        };

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(payload, Json))
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
    }

    private static IReadOnlyList<UnusedUnitLedgerCompareItem> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var fixture = JsonSerializer.Deserialize<UnusedUnitLedgerCompareFixture>(json, Json);
            return fixture?.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.UnitId))
                .Select(Normalize)
                .ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static UnusedUnitLedgerCompareItem Normalize(UnusedUnitLedgerCompareItem item)
        => item with
        {
            UnitId = NormalizeUnitId(item.UnitId),
            Verdict = NormalizeVerdict(item.Verdict),
            SuggestedAction = NormalizeAction(item.SuggestedAction),
            Reason = item.Reason.Trim(),
            SuggestedProposition = string.IsNullOrWhiteSpace(item.SuggestedProposition) ? null : item.SuggestedProposition.Trim(),
            RelatedCandidateIds = item.RelatedCandidateIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList()
        };

    private static string NormalizeVerdict(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "already_covered_indirectly" => "already_covered_indirectly",
        "attach_as_evidence" => "attach_as_evidence",
        "missing_claim" => "missing_claim",
        "needs_human" => "needs_human",
        _ => "needs_human"
    };

    private static string NormalizeAction(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "link_existing_candidate" => "link_existing_candidate",
        "attach_evidence" => "attach_evidence",
        "create_candidate" => "create_candidate",
        "human_review" => "human_review",
        _ => "human_review"
    };

    private static string NormalizeUnitId(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
        if (!trimmed.StartsWith("AU-", StringComparison.Ordinal)) return trimmed;
        return int.TryParse(trimmed[3..], out var number) ? $"AU-{number:D4}" : trimmed;
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}
