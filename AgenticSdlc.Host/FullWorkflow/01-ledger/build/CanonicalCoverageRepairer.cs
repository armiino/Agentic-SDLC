using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Repariert ausschliesslich Coverage-Luecken der Canonicalization: fehlende Candidate-IDs muessen in den
/// kanonischen Ledger integriert oder als standalone Claims uebernommen werden.
/// </summary>
public sealed class CanonicalCoverageRepairer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du reparierst einen bereits kanonisierten Semantic Ledger.

        Ziel:
        - Schliesse NUR die angegebenen Coverage-Luecken: missingCandidateIds.
        - Veraendere bestehende kanonische Eintraege nur, wenn ein fehlender Candidate dort fachlich eindeutig
          hineingehoert.
        - Wenn ein fehlender Candidate nicht eindeutig in einen bestehenden Claim passt: erzeuge einen neuen
          standalone canonical entry aus diesem Candidate.
        - Loesche keine bestehenden kanonischen Eintraege.
        - Erfinde keine neuen fachlichen Claims ohne Candidate-Evidence.
        - Jede Candidate-ID muss am Ende genau einmal in candidateIds vorkommen.

        Taxonomie:
        kind = decision | requirement | constraint | risk | open_requirement | open_question | scope |
               compliance_constraint | process_constraint | non_functional_requirement | meta
        timeScope = mvp | later_possible | mvp_or_later_unclear | null
        riskLevel = high | medium | low
        disposition.*.applicability = required | optional | context | not_applicable
        disposition.*.representationMode = requirement | constraint | open_decision | assumption |
                                             risk_reference | question | open_question | consciously_omitted
        assumedRelation = same_proposition | refines | temporal_sequence | elaborates | standalone
        Verwende NIEMALS representationMode="decision"; nutze open_decision, constraint oder requirement.

        Antworte ausschliesslich mit dem VOLLSTAENDIGEN reparierten Ledger:
        { "entries": [ ... kanonische Eintraege ... ] }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
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
        "semantic_ledger_canonical_coverage_repair",
        "Reparierter kanonischer Semantic Ledger ohne fehlende Candidate-IDs.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public CanonicalCoverageRepairer(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SemanticLedgerEntry>> RepairAsync(
        IReadOnlyList<SemanticLedgerEntry> candidates,
        IReadOnlyList<SemanticLedgerEntry> canonicalDraft,
        IReadOnlyList<string> missingCandidateIds,
        CancellationToken ct)
    {
        if (missingCandidateIds.Count == 0) return canonicalDraft;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var missing = candidates
            .Where(c => missingCandidateIds.Contains(c.Id, StringComparer.Ordinal))
            .ToList();

        var payload = new
        {
            missingCandidateIds,
            missingCandidates = missing,
            candidateLedger = new SemanticLedgerFixture(candidates),
            canonicalDraft = new SemanticLedgerFixture(canonicalDraft)
        };

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(payload, Json))
            ],
            options, ct).ConfigureAwait(false);

        return SemanticLedgerExtractor.Parse(response.Text);
    }
}
