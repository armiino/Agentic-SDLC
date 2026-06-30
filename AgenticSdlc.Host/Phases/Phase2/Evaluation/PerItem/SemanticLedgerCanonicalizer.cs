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
        - Jeder Eintrag nennt in notes kurz, welche Candidate-IDs zusammengefuehrt wurden.

        Antworte ausschliesslich mit JSON im exakt gleichen Schema:
        {
          "entries": [
            {
              "id": "canonical-stable-id",
              "proposition": "...",
              "kind": "...",
              "status": "...",
              "modality": "...",
              "scope": "...",
              "timeScope": "mvp|later_possible|mvp_or_later_unclear|null",
              "evidence": [{ "source": "...", "quote": "..." }],
              "disposition": {
                "requirements": { "applicability": "...", "representationMode": "..." },
                "architecture": { "applicability": "...", "representationMode": "..." },
                "risks": { "applicability": "...", "representationMode": "..." },
                "open-questions": { "applicability": "...", "representationMode": "..." }
              },
              "riskLevel": "high|medium|low",
              "notes": "merged candidates: id1, id2; facet repair: ..."
            }
          ]
        }
        """;

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
        if (_structuredOutput)
        {
            // Reuse the extractor schema by asking for the same JSON shape. Some providers reject
            // duplicated schema names per request less often when response format is omitted here,
            // so keep structured output optional via config.
        }

        var response = await _client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, $"CANDIDATE_LEDGER:\n{JsonSerializer.Serialize(new SemanticLedgerFixture(candidateLedger), Json)}")
            ],
            options, ct).ConfigureAwait(false);

        return SemanticLedgerExtractor.Parse(response.Text);
    }
}
