using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Normalisiert einen breit extrahierten Candidate Semantic Ledger zu kanonischen Eintraegen.
/// Generischer Spike-Fix fuer Split-/Facet-/Disposition-Fehler, ohne Fixture-Wissen.
/// </summary>
public sealed class SemanticLedgerCanonicalizer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du normalisierst einen breit extrahierten Candidate Semantic Ledger aus einem Stakeholder-Transkript.

        Ziel:
        - Mache aus vielen richtigen, aber verteilten Kandidaten einen kanonischen Semantic Ledger.
        - Merge Eintraege, die gemeinsam EINEN fachlichen Claim bilden.
        - Erhalte und repariere Status, Modalitaet, Scope, TimeScope und Disposition.
        - Arbeite generisch fuer beliebige Transkripte. Nutze keine erwartete Fixture.

        Bekannte generische Fehlermodi, die du beheben sollst:
        1. Split-Claims:
           Ein fachlicher Claim ist auf mehrere Kandidaten verteilt.
           Beispielmuster: Login + Double-Opt-In, Rabattgrenze + Workflow-Status,
           Nutzerzahl-Spanne + Skalierungsfolge, kein Ticketsystem + Datenschutzrisiko.

        2. Abgeschwaechte Statusfacetten:
           "nie besprochen", "nicht entschieden", "offen", "vielleicht spaeter" muessen erhalten bleiben.
           Nicht zu "planned", "decided", "required" verstaerken.

        3. Scope-Verschiebung:
           "nicht im MVP", "spaeter moeglich", "MVP oder spaeter unklar" muessen getrennt bleiben.
           Nicht zu MVP-Anforderung machen.

        4. Modalitaets-Verschiebung:
           "gewuenscht", "optional", "muss geklaert werden", "muss beruecksichtigt werden"
           nicht miteinander verwechseln.

        5. Disposition:
           Entscheide pro Artefakt, ob der kanonische Claim required, optional, context oder not_applicable ist.
           Verwende fuer applicability NUR: required | optional | context | not_applicable.
           Verwende fuer representationMode NUR:
           requirement | constraint | open_decision | assumption | risk_reference | question | open_question | consciously_omitted.
           Verwende NIEMALS representationMode="decision"; nutze open_decision fuer offene Entscheidungen
           oder constraint/requirement fuer verbindliche Entscheidungen.
           Requirements: fachliche Anforderungen, Constraints, Scope-/MVP-Entscheidungen, offene Anforderungen.
           Architecture: Architekturentscheidungen, Integrations-/Skalierungs-/technische Offenheiten.
           Risks: Risiken, Tradeoffs, Compliance-/Datenschutz-/Terminrisiken.
           Open-questions: offene Entscheidungen/Klaerungsbedarfe.

        Was du NICHT tun sollst:
        - Keine wichtigen Kandidaten loeschen, nur weil sie unbequem sind.
        - Keine neuen fachlichen Claims ohne Candidate-Evidence erfinden.
        - Keine unabhängigen Claims zusammenwerfen, wenn dadurch Status/Scope unscharf wird.
        - Keine transkriptspezifischen Sonderregeln verwenden.

        Output:
        - 30-80 kanonische Eintraege.
        - Jeder Eintrag hat Evidence aus den zusammengefuehrten Kandidaten.
        - CLUSTER-TRACE (Pflicht, strukturiert):
          - candidateIds: ALLE Candidate-IDs, die in diesen kanonischen Eintrag eingeflossen sind
            (auch bei nur einem Kandidaten genau diese eine ID). KEINE Candidate-ID darf still verschwinden:
            jeder Eingangskandidat muss in genau einem kanonischen Eintrag unter candidateIds auftauchen.
          - assumedRelation: WIE die Kandidaten zusammengehoeren. Erlaubt:
            same_proposition (gleiche Aussage / Duplikat-Merge) |
            refines (ein Kandidat praezisiert den anderen) |
            temporal_sequence (zeitlicher/prozessualer Zusammenhang) |
            elaborates (ergaenzende Facette desselben Claims) |
            standalone (genau ein Kandidat, kein Merge).
        - notes weiterhin kurz fuer Facet-Repair-Hinweise (nicht fuer die Candidate-Liste, die steht in candidateIds).

        Antworte ausschliesslich mit JSON im exakt gleichen Schema:
        {
          "entries": [
            {
              "id": "canonical-stable-id",
              "proposition": "...",
              "kind": "decision|requirement|constraint|risk|open_requirement|open_question|scope|compliance_constraint|process_constraint|non_functional_requirement|meta",
              "status": "decided|open|rejected|uncertain|required",
              "modality": "must|must_clarify|must_consider|must_note|must_not|desired|optional",
              "scope": "...",
              "timeScope": "mvp|later_possible|mvp_or_later_unclear|null",
              "evidence": [{ "source": "...", "quote": "..." }],
              "disposition": {
                "requirements": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "architecture": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "risks": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "open-questions": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" }
              },
              "riskLevel": "high|medium|low",
              "notes": "facet repair: ...",
              "candidateIds": ["cand-id1", "cand-id2"],
              "assumedRelation": "same_proposition|refines|temporal_sequence|elaborates|standalone"
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
                  "kind": { "type": "string", "enum": ["decision", "requirement", "constraint", "risk", "open_requirement", "open_question", "scope", "compliance_constraint", "process_constraint", "non_functional_requirement", "meta"] },
                  "status": { "type": "string", "enum": ["decided", "open", "rejected", "uncertain", "required"] },
                  "modality": { "type": "string", "enum": ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"] },
                  "scope": { "type": "string" },
                  "timeScope": { "type": ["string", "null"], "enum": ["mvp", "later_possible", "mvp_or_later_unclear", null] },
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
                      "requirements": { "type": "object", "properties": { "applicability": { "type": "string", "enum": ["required", "optional", "context", "not_applicable"] }, "representationMode": { "type": "string", "enum": ["requirement", "constraint", "open_decision", "assumption", "risk_reference", "question", "open_question", "consciously_omitted"] } }, "required": ["applicability", "representationMode"], "additionalProperties": false },
                      "architecture": { "type": "object", "properties": { "applicability": { "type": "string", "enum": ["required", "optional", "context", "not_applicable"] }, "representationMode": { "type": "string", "enum": ["requirement", "constraint", "open_decision", "assumption", "risk_reference", "question", "open_question", "consciously_omitted"] } }, "required": ["applicability", "representationMode"], "additionalProperties": false },
                      "risks": { "type": "object", "properties": { "applicability": { "type": "string", "enum": ["required", "optional", "context", "not_applicable"] }, "representationMode": { "type": "string", "enum": ["requirement", "constraint", "open_decision", "assumption", "risk_reference", "question", "open_question", "consciously_omitted"] } }, "required": ["applicability", "representationMode"], "additionalProperties": false },
                      "open-questions": { "type": "object", "properties": { "applicability": { "type": "string", "enum": ["required", "optional", "context", "not_applicable"] }, "representationMode": { "type": "string", "enum": ["requirement", "constraint", "open_decision", "assumption", "risk_reference", "question", "open_question", "consciously_omitted"] } }, "required": ["applicability", "representationMode"], "additionalProperties": false }
                    },
                    "required": ["requirements", "architecture", "risks", "open-questions"],
                    "additionalProperties": false
                  },
                  "riskLevel": { "type": "string", "enum": ["high", "medium", "low"] },
                  "notes": { "type": ["string", "null"] },
                  "candidateIds": { "type": "array", "items": { "type": "string" } },
                  "assumedRelation": { "type": "string", "enum": ["same_proposition", "refines", "temporal_sequence", "elaborates", "standalone"] }
                },
                "required": ["id", "proposition", "kind", "status", "modality", "scope", "timeScope", "evidence", "disposition", "riskLevel", "notes", "candidateIds", "assumedRelation"],
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
        "semantic_ledger_canonicalization",
        "Kanonischer Semantic Ledger mit Cluster-Trace (candidateIds + assumedRelation).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SemanticLedgerCanonicalizer(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SemanticLedgerEntry>> CanonicalizeAsync(
        IReadOnlyList<SemanticLedgerEntry> candidateLedger,
        CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        // L2: eigener, BENANNTER Schema (semantic_ledger_canonicalization) — vermeidet die früher
        // befürchtete Schema-Namens-Kollision mit dem Extractor und erzwingt den Cluster-Trace.
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, $"CANDIDATE_LEDGER:\n{JsonSerializer.Serialize(new SemanticLedgerFixture(candidateLedger), Json)}")
            ],
            options, ct).ConfigureAwait(false);

        return SemanticLedgerExtractor.Parse(response.Text);
    }
}
