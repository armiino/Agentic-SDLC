using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

internal sealed class UnusedUnitTriageReviewer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const int BatchSize = 60;

    private const string SystemPrompt = """
        Du sichtest Atomic Units, die von keinem Candidate Claim direkt referenziert wurden.

        Ziel dieses Schritts ist NUR Triage, kein Ledger-Vergleich.
        Entscheide grob, ob eine Unit offensichtlich irrelevant/noise ist oder potenziell fachlich relevant sein kann.

        Markiere konservativ:
        - Wenn die Unit eine konkrete SDLC-relevante Aussage, Frage, Unsicherheit, Entscheidung, Constraint,
          Risiko, fachliche Regel, Compliance-Aussage, Architekturhinweis oder Scope-Aussage enthaelt:
          triage = potentially_relevant.
        - Wenn sie nur Zustimmung, Rueckfrage ohne Inhalt, Gespraechsfuellung oder reine Wiederholung ist:
          triage = acknowledgement | smalltalk | low_signal | repetition.
        - Wenn unklar: potentially_relevant.

        Erlaubte triage-Werte:
        trash
        smalltalk
        acknowledgement
        repetition
        low_signal
        potentially_relevant

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "triage": "potentially_relevant|low_signal|repetition|acknowledgement|smalltalk|trash",
              "reason": "kurz",
              "keywords": ["SAP", "Rabattlogik"]
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
                  "triage": {
                    "type": "string",
                    "enum": ["trash", "smalltalk", "acknowledgement", "repetition", "low_signal", "potentially_relevant"]
                  },
                  "reason": { "type": "string" },
                  "keywords": {
                    "type": "array",
                    "items": { "type": "string" }
                  }
                },
                "required": [__REASONING_REQ__"unitId", "triage", "reason", "keywords"],
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

    public UnusedUnitTriageReviewer(IChatClient client, bool structuredOutput = true, ReasoningCapture reasoning = ReasoningCapture.Enforced)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _systemPrompt = SystemPrompt + ReasoningSchema.PromptAppendix(reasoning);
        _responseFormat = ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(BuildSchemaJson(reasoning)).RootElement.Clone(),
            "unused_unit_triage",
            "Triage ungenutzter Atomic Units vor Ledger-Vergleich.");
    }

    public async Task<IReadOnlyList<UnusedUnitTriageItem>> TriageAsync(
        IReadOnlyList<AtomicUnit> unusedUnits,
        CancellationToken ct)
    {
        var all = new List<UnusedUnitTriageItem>();
        foreach (var batch in unusedUnits.Chunk(BatchSize))
            all.AddRange(await TriageBatchAsync(batch, ct).ConfigureAwait(false));
        return all;
    }

    private async Task<IReadOnlyList<UnusedUnitTriageItem>> TriageBatchAsync(
        IReadOnlyList<AtomicUnit> units,
        CancellationToken ct)
    {
        if (units.Count == 0) return [];

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = _responseFormat;

        var payload = new
        {
            unusedUnits = units.Select(u => new { u.Id, u.Speaker, u.Text })
        };

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, _systemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(payload, Json))
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
    }

    private static IReadOnlyList<UnusedUnitTriageItem> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var fixture = JsonSerializer.Deserialize<UnusedUnitTriageFixture>(json, Json);
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

    private static UnusedUnitTriageItem Normalize(UnusedUnitTriageItem item)
        => item with
        {
            UnitId = NormalizeUnitId(item.UnitId),
            Triage = NormalizeTriage(item.Triage),
            Reason = item.Reason.Trim(),
            Keywords = item.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };

    private static string NormalizeTriage(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "trash" => "trash",
        "smalltalk" => "smalltalk",
        "acknowledgement" => "acknowledgement",
        "repetition" => "repetition",
        "low_signal" => "low_signal",
        "potentially_relevant" => "potentially_relevant",
        _ => "potentially_relevant"
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
