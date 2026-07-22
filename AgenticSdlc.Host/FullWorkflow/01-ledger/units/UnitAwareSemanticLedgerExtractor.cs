using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

internal sealed class UnitAwareSemanticLedgerExtractor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private const string SystemPrompt = """
        Du extrahierst einen SEMANTIC SOURCE LEDGER aus deterministischen Atomic Units eines Stakeholder-Transkripts.

        Ziel:
        - Recall-first, aber nur SDLC-relevante, konkrete und pruefbare SourceClaims.
        - Bewahre Status, Modalitaet, Scope und TimeScope.
        - Jede extrahierte Aussage MUSS sourceUnitIds enthalten.
        - sourceUnitIds duerfen ausschliesslich IDs aus dem Input sein.
        - Wenn eine Aussage mehrere Units benoetigt, nenne alle relevanten IDs.
        - Nicht jede Unit muss verwendet werden; irrelevante Smalltalk-/Meta-Units duerfen ungenutzt bleiben.
        - Keine Status-, Scope- oder Modalitaetsverstaerkung.

        FACETTEN-TAXONOMIE (verbindlich, keine anderen Werte):
        status: decided | open | rejected | uncertain | required
          - required NUR fuer extern vorgeschriebene, nicht-verhandelbare Pflicht (Gesetz/Policy/Compliance).
          - Team-internes "wir muessen X" ist status=open|decided + modality=must.
        modality: must | must_clarify | must_consider | must_note | must_not | desired | optional

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
                { "source": "transcript", "quote": "Speaker: ..." }
              ],
              "disposition": {
                "requirements": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "architecture": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "risks": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "open-questions": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" }
              },
              "riskLevel": "high|medium|low",
              "notes": "kurz",
              "sourceUnitIds": ["AU-0001"]
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
                      "requirements": { "$ref": "#/$defs/disposition" },
                      "architecture": { "$ref": "#/$defs/disposition" },
                      "risks": { "$ref": "#/$defs/disposition" },
                      "open-questions": { "$ref": "#/$defs/disposition" }
                    },
                    "required": ["requirements", "architecture", "risks", "open-questions"],
                    "additionalProperties": false
                  },
                  "riskLevel": { "type": "string", "enum": ["high", "medium", "low"] },
                  "notes": { "type": ["string", "null"] },
                  "sourceUnitIds": {
                    "type": "array",
                    "items": { "type": "string" }
                  }
                },
                "required": ["id", "proposition", "kind", "status", "modality", "scope", "timeScope", "evidence", "disposition", "riskLevel", "notes", "sourceUnitIds"],
                "additionalProperties": false
              }
            }
          },
          "$defs": {
            "disposition": {
              "type": "object",
              "properties": {
                "applicability": { "type": "string" },
                "representationMode": { "type": "string" }
              },
              "required": ["applicability", "representationMode"],
              "additionalProperties": false
            }
          },
          "required": ["entries"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "unit_aware_semantic_ledger_extraction",
        "Facettierter Semantic Source Ledger mit Atomic-Unit-Trace.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public UnitAwareSemanticLedgerExtractor(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SemanticLedgerEntry>> ExtractAsync(
        IReadOnlyList<AtomicUnit> units,
        CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(new AtomicUnitFixture(units), Json))
            ],
            options, ct).ConfigureAwait(false);

        return SemanticLedgerExtractor.Parse(response.Text)
            .Select((e, i) => string.IsNullOrWhiteSpace(e.Id) ? e with { Id = $"SEM-SC-{i + 1:D3}" } : e)
            .ToList();
    }
}
