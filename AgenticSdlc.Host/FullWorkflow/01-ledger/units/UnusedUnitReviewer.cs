using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

internal sealed class UnusedUnitReviewer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const int BatchSize = 40;

    private const string SystemPrompt = """
        Du pruefst Atomic Units, die von keinem Candidate Claim referenziert wurden.

        Ziel:
        - Nicht den Ledger neu schreiben.
        - Nur klassifizieren, ob eine unused Unit fachlich wichtig sein koennte.
        - Konservativ sein: Wenn eine Unit SDLC-relevante Information enthaelt, markiere sie nicht als irrelevant.
        - Wenn sie schon indirekt durch bestehende Candidates abgedeckt ist, nenne relatedCandidateIds.
        - Wenn sie eine neue, wichtige Aussage enthaelt, markiere missing_claim.
        - Wenn unklar, markiere needs_human.

        Erlaubte verdict-Werte:
        irrelevant
        low_signal
        already_covered_indirectly
        missing_claim
        needs_human

        Erlaubte suggestedAction-Werte:
        ignore
        keep_for_context
        link_existing_candidate
        create_candidate
        human_review

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "verdict": "missing_claim|already_covered_indirectly|needs_human|irrelevant|low_signal",
              "suggestedAction": "create_candidate|link_existing_candidate|human_review|ignore|keep_for_context",
              "reason": "kurz",
              "suggestedProposition": "nur bei missing_claim/needs_human, sonst null",
              "relatedCandidateIds": ["C001"]
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
        """;

    private const string SchemaTemplate = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  __REASONING_PROP__"unitId": { "type": "string" },
                  "verdict": {
                    "type": "string",
                    "enum": ["irrelevant", "low_signal", "already_covered_indirectly", "missing_claim", "needs_human"]
                  },
                  "suggestedAction": {
                    "type": "string",
                    "enum": ["ignore", "keep_for_context", "link_existing_candidate", "create_candidate", "human_review"]
                  },
                  "reason": { "type": "string" },
                  "suggestedProposition": { "type": ["string", "null"] },
                  "relatedCandidateIds": {
                    "type": "array",
                    "items": { "type": "string" }
                  }
                },
                "required": [__REASONING_REQ__"unitId", "verdict", "suggestedAction", "reason", "suggestedProposition", "relatedCandidateIds"],
                "additionalProperties": false
              }
            }
          },
          "required": ["items"],
          "additionalProperties": false
        }
        """;

    // W1a: Schema je ReasoningCapture-Modus (Marker-Ersetzung, kein Brace-Doubling).
    private static string BuildSchemaJson(ReasoningCapture reasoning) => SchemaTemplate
        .Replace("__REASONING_PROP__", ReasoningSchema.PropertyJson(reasoning))
        .Replace("__REASONING_REQ__", ReasoningSchema.RequiredToken(reasoning));

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly string _systemPrompt;
    private readonly ChatResponseFormat _responseFormat;

    public UnusedUnitReviewer(IChatClient client, bool structuredOutput = true, ReasoningCapture reasoning = ReasoningCapture.Enforced)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        // W1a: reasoning log-only via Prompt-Appendix + Schema-Feld (bei off beides leer).
        _systemPrompt = SystemPrompt + ReasoningSchema.PromptAppendix(reasoning);
        _responseFormat = ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(BuildSchemaJson(reasoning)).RootElement.Clone(),
            "unused_unit_review",
            "Klassifikation ungenutzter Atomic Units fuer Ledger-Coverage.");
    }

    public async Task<IReadOnlyList<UnusedUnitReviewItem>> ReviewAsync(
        IReadOnlyList<AtomicUnit> unusedUnits,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        if (unusedUnits.Count == 0) return [];

        var all = new List<UnusedUnitReviewItem>();
        foreach (var batch in unusedUnits.Chunk(BatchSize))
        {
            var batchResult = await ReviewBatchAsync(batch, candidates, ct).ConfigureAwait(false);
            all.AddRange(batchResult);
        }

        return all;
    }

    private async Task<IReadOnlyList<UnusedUnitReviewItem>> ReviewBatchAsync(
        IReadOnlyList<AtomicUnit> unusedUnits,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        if (unusedUnits.Count == 0) return [];

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = _responseFormat;

        var payload = new
        {
            unusedUnits = unusedUnits.Select(u => new
            {
                u.Id,
                u.Speaker,
                u.Text
            }),
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
                new ChatMessage(ChatRole.System, _systemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(payload, Json))
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
    }

    private static IReadOnlyList<UnusedUnitReviewItem> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var fixture = JsonSerializer.Deserialize<UnusedUnitReviewFixture>(json, Json);
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

    private static UnusedUnitReviewItem Normalize(UnusedUnitReviewItem item)
        => item with
        {
            UnitId = item.UnitId.Trim().ToUpperInvariant(),
            Verdict = NormalizeVerdict(item.Verdict),
            SuggestedAction = NormalizeAction(item.SuggestedAction),
            Reason = item.Reason.Trim(),
            SuggestedProposition = string.IsNullOrWhiteSpace(item.SuggestedProposition) ? null : item.SuggestedProposition.Trim(),
            RelatedCandidateIds = item.RelatedCandidateIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList()
        };

    private static string NormalizeVerdict(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "irrelevant" => "irrelevant",
        "low_signal" => "low_signal",
        "already_covered_indirectly" => "already_covered_indirectly",
        "missing_claim" => "missing_claim",
        "needs_human" => "needs_human",
        _ => "needs_human"
    };

    private static string NormalizeAction(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "ignore" => "ignore",
        "keep_for_context" => "keep_for_context",
        "link_existing_candidate" => "link_existing_candidate",
        "create_candidate" => "create_candidate",
        "human_review" => "human_review",
        _ => "human_review"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}
