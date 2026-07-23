using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Stufe 3 (Kern): <b>chunked Per-Item-Validierung</b> kanonischer Ledger-Einträge gegen das Transcript.
/// </summary>
/// <remarks>
/// Design (bewusst, vgl. Phase2B/B16-B17): NICHT ein Call pro Claim (zu teuer), NICHT ein Call für alle
/// (Trennschärfe kollabiert) — sondern kleine deterministische Batches (Default 8) mit <b>fixem Nenner</b>:
/// pro Input-Eintrag GENAU EIN Verdict, gleiche id. Fehlt ein Verdict oder taucht eine fremde id auf →
/// Batch UNGÜLTIG → fail-fast (LEDGER_FACET_BATCH_INVALID). Ziel = Qualität der vorhandenen Einträge prüfen
/// (grounded/partial/overstated/unsupported + Facet-Issues + Repair), NICHT neue Claims finden.
/// Kontext = VOLLES Transcript pro Batch (erster stabiler Spike; Retrieval-Optimierung später, damit sich
/// Retrieval-Fehler nicht mit der Validierungslogik mischen).
/// </remarks>
public sealed class FacetValidator
{
    public const int DefaultBatchSize = 8;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du VALIDIERST vorhandene Ledger-Einträge gegen das TRANSCRIPT. Du findest KEINE neuen Aussagen und
        fügst KEINE Einträge hinzu. Du prüfst NUR die dir gegebenen Einträge.

        Für JEDEN Input-Eintrag gib GENAU EIN Verdict-Objekt mit DERSELBEN id zurück. Gleiche Anzahl wie Input.
        Keine zusätzlichen ids, keine fehlenden ids.

        Prüfe pro Eintrag gegen das Transcript:
        - proposition: durch das Transcript gedeckt? übertrieben/zu stark formuliert?
        - status, modality, scope, timeScope: passen sie zur Quelllage?
          (Achte auf Verstärkung: "offen" darf nicht "decided" sein, "gewünscht" nicht "must",
           "später/nicht MVP" nicht "mvp".)
        - evidence: stützen die Zitate den Eintrag?
        - disposition: sinnvoll fürs jeweilige Zielartefakt?

        verdict (genau einer):
        - grounded:    Proposition und Facetten sind vom Transcript gedeckt.
        - partial:     Kern gedeckt, aber eine Facette ist schwächer/gröber/leicht daneben.
        - overstated:  Stärker/entschiedener formuliert, als die Quelle hergibt (Status-/Modalitäts-/Scope-Verstärkung).
        - unsupported: Nicht durch das Transcript gedeckt.

        facetIssues: NUR für Facetten, die nicht passen. Pro Issue: facet, observed (aktueller Wert),
        problem (kurz), suggested (korrigierter Wert oder null). Wenn alles passt: leere Liste.

        suggested MUSS bei geschlossenen Facetten ein OFFIZIELLER Taxonomie-Wert sein (keine Freitexte wie
        "should", "proposed", "known", "target", "noted", "unspecified"):
          status:    decided | open | rejected | uncertain | required
          modality:  must | must_clarify | must_consider | must_note | must_not | desired | optional
          timeScope: mvp | later_possible | mvp_or_later_unclear
        Nur abschwächen, nie verstärken (z. B. decided->open, must->desired/must_clarify, mvp->mvp_or_later_unclear).

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "id": "<exakt die Input-id>",
              "verdict": "grounded|partial|overstated|unsupported",
              "facetIssues": [
                { "facet": "status", "observed": "decided", "problem": "Quelle lässt es offen", "suggested": "open" }
              ],
              "reason": "kurz"
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
                  "id": { "type": "string" },
                  "verdict": { "type": "string" },
                  "facetIssues": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "facet": { "type": "string" },
                        "observed": { "type": "string" },
                        "problem": { "type": "string" },
                        "suggested": { "type": ["string", "null"] }
                      },
                      "required": ["facet", "observed", "problem", "suggested"],
                      "additionalProperties": false
                    }
                  },
                  "reason": { "type": "string" }
                },
                "required": ["id", "verdict", "facetIssues", "reason"],
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
        "facet_validation_batch",
        "Chunked Per-Item-Validierung von Ledger-Einträgen gegen das Transcript (fixer Nenner).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;

    public FacetValidator(IChatClient client, bool structuredOutput = true, int batchSize = DefaultBatchSize)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 12);
    }

    public int BatchSize => _batchSize;

    /// <summary>Validiert ALLE Einträge in Batches (Default 8) gegen das Transcript und fügt die Verdicts zusammen.</summary>
    public async Task<IReadOnlyList<EntryValidation>> ValidateAllAsync(
        IReadOnlyList<SemanticLedgerEntry> entries, string transcript, CancellationToken ct)
    {
        var all = new List<EntryValidation>(entries.Count);
        for (var i = 0; i < entries.Count; i += _batchSize)
        {
            var batch = entries.Skip(i).Take(_batchSize).ToList();
            all.AddRange(await ValidateBatchAsync(batch, transcript, ct).ConfigureAwait(false));
        }
        return all;
    }

    /// <summary>Ein Batch → ein Call. Fixer Nenner: exakt ein Verdict je Input-id, sonst fail-fast.</summary>
    public async Task<IReadOnlyList<EntryValidation>> ValidateBatchAsync(
        IReadOnlyList<SemanticLedgerEntry> batch, string transcript, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch, transcript))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        var groupedById = parsed.GroupBy(v => v.Id).ToList();
        var byId = groupedById.ToDictionary(g => g.Key, g => g.First());

        var inputIds = batch.Select(e => e.Id).ToList();
        var inputSet = inputIds.ToHashSet(StringComparer.Ordinal);

        // Fixer Nenner: jede Input-id genau einmal, keine fremden ids.
        var missing = inputIds.Where(id => !byId.ContainsKey(id)).ToList();
        var unknown = parsed.Select(v => v.Id).Where(id => !inputSet.Contains(id)).Distinct().ToList();
        var duplicate = groupedById.Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (missing.Count > 0 || unknown.Count > 0 || duplicate.Count > 0 || parsed.Count != inputIds.Count)
            throw new InvalidOperationException(
                $"LEDGER_FACET_BATCH_INVALID: expected {inputIds.Count} verdicts (1 per input id); " +
                $"actual={parsed.Count} missing=[{string.Join(",", missing)}] unknown=[{string.Join(",", unknown)}] duplicate=[{string.Join(",", duplicate)}]");

        // Reihenfolge = Input-Reihenfolge.
        return inputIds.Select(id => byId[id]).ToList();
    }

    private static string BuildUser(IReadOnlyList<SemanticLedgerEntry> batch, string transcript)
    {
        var sb = new StringBuilder();
        // Kontext-Modus: mit Transkript = Volltranscript-Prüfung; ohne (leer) = Evidenz-basiert
        // (jeder Eintrag trägt seine Evidence unten selbst — reicht seine Evidence NICHT für die Facette,
        //  ist das ein legitimes Finding = unvollständige/schwache Provenienz).
        if (!string.IsNullOrWhiteSpace(transcript))
        {
            sb.AppendLine("TRANSCRIPT:");
            sb.AppendLine(transcript);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("Prüfe jeden Eintrag NUR gegen SEINE eigene Evidence (kein Volltranscript).");
            sb.AppendLine("Wenn die angehängte Evidence einen Facetten-Wert nicht trägt: sag es (overstated/unsupported/partial).");
            sb.AppendLine();
        }
        sb.AppendLine($"ZU PRÜFENDE EINTRÄGE ({batch.Count}) — gib GENAU {batch.Count} Verdicts zurück, eins pro id:");
        foreach (var e in batch)
        {
            sb.AppendLine($"- id: {e.Id}");
            sb.AppendLine($"  proposition: {e.Proposition}");
            sb.AppendLine($"  status: {e.Status} | modality: {e.Modality} | scope: {e.Scope} | timeScope: {e.TimeScope ?? "null"}");
            var ev = e.Evidence.Count == 0 ? "(keine)" : string.Join(" | ", e.Evidence.Select(x => x.Quote));
            sb.AppendLine($"  evidence: {ev}");
            sb.AppendLine($"  disposition: {JsonSerializer.Serialize(e.Disposition, Json)}");
        }
        return sb.ToString();
    }

    public static IReadOnlyList<EntryValidation> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var r = JsonSerializer.Deserialize<BatchValidationResult>(json, Json);
            return r?.Items.Select(v => v.Normalized()).Where(v => v.Id.Length > 0).ToList() ?? [];
        }
        catch (JsonException) { return []; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}
