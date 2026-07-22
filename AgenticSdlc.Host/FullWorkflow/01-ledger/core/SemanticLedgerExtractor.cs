using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger.Core;

/// <summary>Extrahiert einen facettierten Semantic Ledger aus einem Transkript. Isolierter Spike.</summary>
public sealed class SemanticLedgerExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du extrahierst einen SEMANTIC SOURCE LEDGER aus einem Stakeholder-Transkript fuer SDLC-Artefakte.

        Ziel:
        - Recall-first, aber nicht beliebig generisch.
        - Extrahiere konkrete, pruefbare SourceClaims mit Evidence.
        - Bewahre besonders Status, Modalitaet, Scope und TimeScope.
        - Negative und offene Aussagen sind wichtig: "nicht im MVP", "noch nicht entschieden", "spaeter vielleicht".
        - Keine Statusverstaerkung: "offen" darf nicht zu "geplant" werden.
        - Keine Scopeverstaerkung: "spaeter/nicht MVP" darf nicht zu "MVP" werden.
        - Keine Modalitaetsverstaerkung: "gewuenscht/optional" darf nicht zu "muss" werden.

        Zielartefakte:
        - requirements
        - architecture
        - risks
        - open-questions

        Disposition:
        Fuer jedes Zielartefakt entscheide:
        applicability = required | optional | context | not_applicable
        representationMode = requirement | constraint | open_decision | assumption | risk_reference | question | consciously_omitted

        Extrahiere besonders:
        - Entscheidungen und offene Entscheidungen
        - Requirements und Constraints
        - Risiken und Risikokonflikte
        - Scope-Aussagen: MVP, nicht MVP, spaeter, unklar
        - Datenschutz-/Compliance-Aussagen
        - Zahlen, Fristen, Schwellwerte
        - Architektur- und Integrationsunsicherheiten

        Gib maximal 80 Ledger-Eintraege aus. Fasse keine unabhaengigen Facetten zusammen, wenn Status/Scope dadurch
        verloren geht. Vermeide reine Duplikate.

        FACETTEN-TAXONOMIE (verbindlich, keine anderen Werte):
        status (Entscheidungsstand): decided | open | rejected | uncertain | required
          - required NUR fuer extern vorgeschriebene, nicht-verhandelbare Pflicht (Gesetz/Policy/Compliance),
            NICHT fuer team-internes "wir muessen X" -> das ist status=open|decided + modality=must.
          - Ein Wunsch ist KEIN status; "gewuenscht/optional" -> status=open + modality=desired|optional.
        modality (Verbindlichkeit): must | must_clarify | must_consider | must_note | must_not | desired | optional
        Keine Verstaerkung: offen darf nicht decided/required werden, gewuenscht nicht must, spaeter nicht mvp.

        Antworte ausschliesslich mit JSON:
        {
          "entries": [
            {
              "id": "kurze stabile ID oder leer",
              "proposition": "...",
              "kind": "decision|requirement|constraint|risk|open_requirement|open_question|scope|compliance_constraint|process_constraint|non_functional_requirement",
              "status": "decided|open|rejected|uncertain|required",
              "modality": "must|must_clarify|must_consider|must_note|must_not|desired|optional",
              "scope": "kurzer_scope_string",
              "timeScope": "mvp|later_possible|mvp_or_later_unclear|null",
              "evidence": [
                { "source": "T9999_chaos.txt", "quote": "Speaker (T123): ..." }
              ],
              "disposition": {
                "requirements": { "applicability": "...", "representationMode": "..." },
                "architecture": { "applicability": "...", "representationMode": "..." },
                "risks": { "applicability": "...", "representationMode": "..." },
                "open-questions": { "applicability": "...", "representationMode": "..." }
              },
              "riskLevel": "high|medium|low",
              "notes": "kurz"
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "entries": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "id": { "type": "string" },
                  "proposition": { "type": "string" },
                  "kind": { "type": "string" },
                  "status": { "type": "string", "enum": ["decided", "open", "rejected", "uncertain", "required"] },
                  "modality": { "type": "string", "enum": ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"] },
                  "scope": { "type": "string" },
                  "timeScope": { "type": ["string", "null"] },
                  "evidence": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "source": { "type": "string" },
                        "quote": { "type": "string" }
                      },
                      "required": ["source", "quote"],
                      "additionalProperties": false
                    }
                  },
                  "disposition": {
                    "type": "object",
                    "properties": {
                      "requirements": {
                        "type": "object",
                        "properties": {
                          "applicability": { "type": "string" },
                          "representationMode": { "type": "string" }
                        },
                        "required": ["applicability", "representationMode"],
                        "additionalProperties": false
                      },
                      "architecture": {
                        "type": "object",
                        "properties": {
                          "applicability": { "type": "string" },
                          "representationMode": { "type": "string" }
                        },
                        "required": ["applicability", "representationMode"],
                        "additionalProperties": false
                      },
                      "risks": {
                        "type": "object",
                        "properties": {
                          "applicability": { "type": "string" },
                          "representationMode": { "type": "string" }
                        },
                        "required": ["applicability", "representationMode"],
                        "additionalProperties": false
                      },
                      "open-questions": {
                        "type": "object",
                        "properties": {
                          "applicability": { "type": "string" },
                          "representationMode": { "type": "string" }
                        },
                        "required": ["applicability", "representationMode"],
                        "additionalProperties": false
                      }
                    },
                    "required": ["requirements", "architecture", "risks", "open-questions"],
                    "additionalProperties": false
                  },
                  "riskLevel": { "type": "string" },
                  "notes": { "type": ["string", "null"] }
                },
                "required": ["id", "proposition", "kind", "status", "modality", "scope", "timeScope", "evidence", "disposition", "riskLevel", "notes"],
                "additionalProperties": false
              }
            }
          },
          "required": ["entries"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "semantic_ledger_extraction",
        "Facettierter Semantic Source Ledger aus einem Transkript.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SemanticLedgerExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SemanticLedgerEntry>> ExtractAsync(string transcript, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, $"TRANSKRIPT:\n{transcript}")
            ],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)
            .Select((e, i) => string.IsNullOrWhiteSpace(e.Id) ? e with { Id = $"SEM-SC-{i + 1:D3}" } : e)
            .ToList();
    }

    public static IReadOnlyList<SemanticLedgerEntry> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var env = JsonSerializer.Deserialize<SemanticLedgerFixture>(json, Json);
            return env?.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Proposition))
                .Select(Normalize)
                .ToList() ?? [];
        }
        catch (JsonException) { return []; }
    }

    private static SemanticLedgerEntry Normalize(SemanticLedgerEntry e)
    {
        var candidateIds = e.CandidateIds?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        var sourceUnitIds = e.SourceUnitIds?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(NormalizeSourceUnitId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var status = e.Status.Trim().ToLowerInvariant();
        var modality = e.Modality.Trim().ToLowerInvariant();
        if (status == "required" && modality is not ("must" or "must_not"))
            status = "open";
        var relation = string.IsNullOrWhiteSpace(e.AssumedRelation) ? null : e.AssumedRelation.Trim().ToLowerInvariant();
        // Deterministische Cluster-Trace-Normalisierung: EIN Kandidat kann definitionsgemäss kein Merge sein.
        // candidateIds.count <= 1  => assumedRelation = standalone (räumt LLM-Label-Rauschen aus; count>=2
        // mit standalone bleibt eine echte Inkonsistenz, die das Gate weiter meldet).
        if (candidateIds is not null && candidateIds.Count <= 1 && relation is not null && relation != "standalone")
            relation = "standalone";

        return e with
        {
            Id = e.Id.Trim(),
            Proposition = e.Proposition.Trim(),
            Kind = e.Kind.Trim().ToLowerInvariant(),
            Status = status,
            Modality = modality,
            Scope = e.Scope.Trim().ToLowerInvariant(),
            TimeScope = string.IsNullOrWhiteSpace(e.TimeScope) ? null : e.TimeScope.Trim().ToLowerInvariant(),
            Evidence = e.Evidence.Where(ev => !string.IsNullOrWhiteSpace(ev.Quote)).ToList(),
            RiskLevel = e.RiskLevel.Trim().ToLowerInvariant(),
            Notes = e.Notes?.Trim(),
            CandidateIds = candidateIds,
            AssumedRelation = relation,
            SourceUnitIds = sourceUnitIds
        };
    }

    private static string NormalizeSourceUnitId(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith("AU-", StringComparison.OrdinalIgnoreCase)) return trimmed;
        var suffix = trimmed[3..];
        return int.TryParse(suffix, out var number)
            ? $"AU-{number:D4}"
            : trimmed.ToUpperInvariant();
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}
