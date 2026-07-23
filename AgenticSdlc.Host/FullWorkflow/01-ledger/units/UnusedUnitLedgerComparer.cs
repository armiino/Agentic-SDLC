using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

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

        STRENGE REGEL (Pflicht):
        - attach_as_evidence und already_covered_indirectly sind NUR erlaubt, wenn relatedCandidateIds
          mindestens EINE existierende Candidate-ID aus existingCandidates enthaelt.
        - Kannst du keinen konkreten Kandidaten benennen, waehle needs_human (oder missing_claim,
          wenn eine eigenstaendige Aussage fehlt). NIEMALS Deckung ohne Referenz behaupten.

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
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
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
        return await RepairCoverageReferencesAsync(all, relevantUnits, candidates, ct).ConfigureAwait(false);
    }

    // R-1 (2026-07-23): Deckungs-Urteile ohne gueltige Referenz -> EIN gezielter Nachfrage-Pass nur fuer die
    // Verstoss-Units; was danach immer noch referenzlos ist, wird deterministisch zu needs_human umgestuft
    // (landet via Miss-Signal in der Adjudikations-Queue) statt den ganzen Lauf am Trace-Check scheitern zu lassen.
    private async Task<IReadOnlyList<UnusedUnitLedgerCompareItem>> RepairCoverageReferencesAsync(
        List<UnusedUnitLedgerCompareItem> items,
        IReadOnlyList<AtomicUnit> relevantUnits,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        var candidateIds = candidates.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var offenders = items.Where(i => UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(i, candidateIds)).ToList();
        if (offenders.Count == 0) return items;

        var unitById = relevantUnits.GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var repairedById = new Dictionary<string, UnusedUnitLedgerCompareItem>(StringComparer.Ordinal);
        foreach (var batch in offenders.Chunk(BatchSize))
            foreach (var r in await RepairBatchAsync(batch, unitById, candidates, ct).ConfigureAwait(false))
                repairedById[r.UnitId] = r;

        var offenderIds = offenders.Select(o => o.UnitId).ToHashSet(StringComparer.Ordinal);
        var resolved = items.Select(item =>
        {
            if (!offenderIds.Contains(item.UnitId)) return item;
            if (repairedById.TryGetValue(item.UnitId, out var repaired))
            {
                // Nur existierende IDs zaehlen — halluzinierte Repair-Referenzen wuerden sonst als brokenRefs den Trace kippen.
                var sanitized = repaired with { RelatedCandidateIds = repaired.RelatedCandidateIds.Where(candidateIds.Contains).ToList() };
                if (!UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(sanitized, candidateIds)) return sanitized;
            }
            return UnusedUnitCompareRepair.Downgrade(item);
        }).ToList();

        var downgraded = resolved.Count(r => r.Reason.StartsWith(UnusedUnitCompareRepair.DowngradePrefix, StringComparison.Ordinal));
        Console.WriteLine($"[unused-compare] reference-repair: offenders={offenders.Count} repaired={offenders.Count - downgraded} downgraded->needs_human={downgraded}");
        return resolved;
    }

    private const string RepairSystemPrompt = """
        Du hast unused Units gegen einen Candidate Ledger verglichen und fuer die folgenden Units Deckung
        behauptet (attach_as_evidence oder already_covered_indirectly), aber KEINE existierende Candidate-ID
        benannt. Das ist unzulaessig. Korrigiere JEDE dieser Units:
        - Traegt ein konkreter Kandidat die Deckung wirklich: nenne seine ID(s) aus existingCandidates in relatedCandidateIds.
        - Sonst stufe ehrlich um: missing_claim (mit suggestedProposition) oder needs_human.
        Antworte ausschliesslich mit demselben JSON-Format ({"items":[...]}) und denselben erlaubten Werten wie zuvor.
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
        """;

    private async Task<IReadOnlyList<UnusedUnitLedgerCompareItem>> RepairBatchAsync(
        IReadOnlyList<UnusedUnitLedgerCompareItem> offenders,
        IReadOnlyDictionary<string, AtomicUnit> unitById,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var payload = new
        {
            unitsToFix = offenders.Select(o => new
            {
                id = o.UnitId,
                speaker = unitById.GetValueOrDefault(o.UnitId)?.Speaker,
                text = unitById.GetValueOrDefault(o.UnitId)?.Text,
                previousVerdict = o.Verdict,
                previousReason = o.Reason
            }),
            existingCandidates = candidates.Select(c => new { c.Id, c.Proposition, c.Status, c.Modality, c.Scope })
        };

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, RepairSystemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(payload, Json))
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text);
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
