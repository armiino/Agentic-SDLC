using System.Text.Json;
using AgenticSdlc.Host.Configuration;
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

        DISPOSITIONS-VERGABE (verbindlich — die Disposition ist die Weiche, WOHIN ein Claim spaeter reist):
        - open-questions=required NUR fuer echte OFFENE Punkte: modality=must_clarify/must_consider ODER die
          Evidenz benennt die Offenheit woertlich ("noch offen", "klaeren wir mit ...", "weiss nicht genau").
        - Eine im Meeting FESTGELEGTE Sache (modality=must/must_not, zugesagt/beschlossen) bekommt
          open-questions=not_applicable — sie ist Anforderung, keine Frage. Doppel-Natur NUR, wenn ein explizit
          OFFENER Rest woertlich belegt ist (dann requirements=required UND open-questions=required, und notes
          benennt den offenen Rest).
        - representationMode der open-questions-Spur: question (bzw. open_decision, wenn die Quelle eine
          anstehende Entscheidung benennt).
        - Keine Dispositions-Verstaerkung: nicht mehr Spuren als die Quelle belegt.

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
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
        """;

    private const string SchemaTemplate = """
        {
          "type": "object",
          "properties": {
            "entries": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  __REASONING_PROP__"id": { "type": "string" },
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
                "required": [__REASONING_REQ__"id", "proposition", "kind", "status", "modality", "scope", "timeScope", "evidence", "disposition", "riskLevel", "notes", "sourceUnitIds"],
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

    // W1a: Schema je ReasoningCapture-Modus (reasoning-Property/required via Marker-Ersetzung, kein Brace-Doubling).
    private static string BuildSchemaJson(ReasoningCapture reasoning) => SchemaTemplate
        .Replace("__REASONING_PROP__", ReasoningSchema.PropertyJson(reasoning))
        .Replace("__REASONING_REQ__", ReasoningSchema.RequiredToken(reasoning));

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly string _systemPrompt;
    private readonly ChatResponseFormat _responseFormat;

    public UnitAwareSemanticLedgerExtractor(IChatClient client, bool structuredOutput = true, ReasoningCapture reasoning = ReasoningCapture.Enforced)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        // W1a: reasoning log-only via Prompt-Appendix + Schema-Feld (bei off beides leer).
        _systemPrompt = SystemPrompt + ReasoningSchema.PromptAppendix(reasoning);
        _responseFormat = ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(BuildSchemaJson(reasoning)).RootElement.Clone(),
            "unit_aware_semantic_ledger_extraction",
            "Facettierter Semantic Source Ledger mit Atomic-Unit-Trace.");
    }

    public async Task<IReadOnlyList<SemanticLedgerEntry>> ExtractAsync(
        IReadOnlyList<AtomicUnit> units,
        CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = _responseFormat;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, _systemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(new AtomicUnitFixture(units), Json))
            ],
            options, ct).ConfigureAwait(false);

        return SemanticLedgerExtractor.Parse(response.Text)
            .Select((e, i) => string.IsNullOrWhiteSpace(e.Id) ? e with { Id = $"SEM-SC-{i + 1:D3}" } : e)
            .ToList();
    }
}
