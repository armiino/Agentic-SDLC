using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// A10 (Refine): weist einem NEUEN, per Adjudikation geminteten Claim (facetStatus=pending) die vollen Facetten
/// ZU — Gegenstück zu <see cref="FacetValidator"/> (der nur prüft). Ziel: der reingeholte Claim ist danach
/// strukturell nicht mehr von einem Pipeline-Claim unterscheidbar (status/modality/scope/timeScope/kind +
/// disposition + riskLevel), damit der spätere Artifact-Agent ihn über den Consumer-Contract korrekt routet.
/// </summary>
/// <remarks>
/// Bewusst konservativ: bei dünner Quelllage lieber <c>open/uncertain</c> + schwächere Modalität; NIE
/// <c>decided/must</c> erfinden. Grounding = die (bereits von A8 angehängte) echte Transkript-Evidenz des Claims,
/// optional das Volltranscript. Batched (fixer Nenner: 1 Zuweisung pro id) wie der Validator.
/// </remarks>
public sealed class FacetAssigner
{
    public const int DefaultBatchSize = 8;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du WEIST einem Ledger-Claim seine Facetten ZU (assignment), basierend auf seiner Proposition und der
        angehängten Evidence. Du erfindest KEINE neuen Claims und änderst die Proposition NICHT.

        Für JEDEN Input-Claim gib GENAU EIN Objekt mit DERSELBEN id zurück. Gleiche Anzahl wie Input,
        keine zusätzlichen ids, keine fehlenden ids.

        Weise pro Claim zu (geschlossene Taxonomie — nur diese Werte):
        - kind:      requirement | non_functional_requirement | constraint | compliance_constraint |
                     process_constraint | decision | scope | risk | open_question | open_requirement | context
        - status:    decided | open | rejected | uncertain | required
                     (required NUR für extern vorgeschriebene, nicht-verhandelbare Notwendigkeit = Gesetz/Policy/
                      Compliance; NICHT für team-internes "wir müssen X".)
        - modality:  must | must_clarify | must_consider | must_note | must_not | desired | optional
        - timeScope: mvp | later_possible | mvp_or_later_unclear
        - riskLevel: low | medium | high
        - disposition: pro Zielartefakt {requirements, architecture, risks, open-questions} je ein Objekt
          { "applicability": required | context | not_applicable,
            "representationMode": normative | constraint | risk_reference | question | assumption | consciously_omitted }
          disposition steuert, für WELCHES Artefakt der Claim relevant ist.

        KONSERVATIV: Wenn die Evidence einen starken Wert nicht klar trägt, wähle den schwächeren
        (open statt decided, desired/must_note statt must, mvp_or_later_unclear statt mvp). NIE verstärken.

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "id": "<exakt die Input-id>",
              "kind": "requirement",
              "status": "open",
              "modality": "must_note",
              "scope": "<kurzes Themenschlagwort>",
              "timeScope": "mvp_or_later_unclear",
              "riskLevel": "medium",
              "disposition": {
                "requirements":   { "applicability": "required",       "representationMode": "normative" },
                "architecture":   { "applicability": "context",        "representationMode": "assumption" },
                "risks":          { "applicability": "not_applicable", "representationMode": "consciously_omitted" },
                "open-questions": { "applicability": "not_applicable", "representationMode": "consciously_omitted" }
              }
            }
          ]
        }
        """;

    private const string DispositionSchema = """
        { "type":"object",
          "properties": {
            "applicability": { "type":"string" },
            "representationMode": { "type":"string" }
          },
          "required": ["applicability","representationMode"],
          "additionalProperties": false }
        """;

    private static readonly string SchemaJson = $$"""
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "id": { "type": "string" },
                  "kind": { "type": "string" },
                  "status": { "type": "string" },
                  "modality": { "type": "string" },
                  "scope": { "type": "string" },
                  "timeScope": { "type": "string" },
                  "riskLevel": { "type": "string" },
                  "disposition": {
                    "type": "object",
                    "properties": {
                      "requirements": {{DispositionSchema}},
                      "architecture": {{DispositionSchema}},
                      "risks": {{DispositionSchema}},
                      "open-questions": {{DispositionSchema}}
                    },
                    "required": ["requirements","architecture","risks","open-questions"],
                    "additionalProperties": false
                  }
                },
                "required": ["id","kind","status","modality","scope","timeScope","riskLevel","disposition"],
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
        "facet_assignment_batch",
        "Facetten-Zuweisung an neu geminteten Ledger-Claims (fixer Nenner).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;

    public FacetAssigner(IChatClient client, bool structuredOutput = true, int batchSize = DefaultBatchSize)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 12);
    }

    /// <summary>Weist ALLEN Einträgen (in Batches) Facetten zu und liefert die zugewiesenen, voll facettierten Claims.</summary>
    public async Task<IReadOnlyList<SemanticLedgerEntry>> AssignAllAsync(
        IReadOnlyList<SemanticLedgerEntry> entries, string? transcript, CancellationToken ct)
    {
        var all = new List<SemanticLedgerEntry>(entries.Count);
        for (var i = 0; i < entries.Count; i += _batchSize)
        {
            var batch = entries.Skip(i).Take(_batchSize).ToList();
            all.AddRange(await AssignBatchAsync(batch, transcript, ct).ConfigureAwait(false));
        }
        return all;
    }

    /// <summary>Ein Batch → ein Call. Fixer Nenner: exakt eine Zuweisung je Input-id, sonst fail-fast.</summary>
    public async Task<IReadOnlyList<SemanticLedgerEntry>> AssignBatchAsync(
        IReadOnlyList<SemanticLedgerEntry> batch, string? transcript, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch, transcript))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        var byId = parsed.GroupBy(v => v.Id).ToDictionary(g => g.Key, g => g.First());

        var inputIds = batch.Select(e => e.Id).ToList();
        var inputSet = inputIds.ToHashSet(StringComparer.Ordinal);
        var missing = inputIds.Where(id => !byId.ContainsKey(id)).ToList();
        var unknown = parsed.Select(v => v.Id).Where(id => !inputSet.Contains(id)).Distinct().ToList();
        if (missing.Count > 0 || unknown.Count > 0)
            throw new InvalidOperationException(
                $"LEDGER_FACET_ASSIGN_INVALID: expected {inputIds.Count} assignments (1 per input id); " +
                $"missing=[{string.Join(",", missing)}] unknown=[{string.Join(",", unknown)}]");

        // Facetten auf den bestehenden Claim anwenden (Proposition/Evidence/sourceUnitIds/Id BLEIBEN); pending löschen.
        return batch.Select(e =>
        {
            var a = byId[e.Id];
            return e with
            {
                Kind = a.Kind,
                Status = a.Status,
                Modality = a.Modality,
                Scope = string.IsNullOrWhiteSpace(a.Scope) ? e.Scope : a.Scope,
                TimeScope = a.TimeScope,
                RiskLevel = a.RiskLevel,
                Disposition = a.Disposition,
                FacetStatus = null,
                Notes = AppendNote(e.Notes, "Facetten zugewiesen via ledger-adjudicate-refine (A10)."),
            };
        }).ToList();
    }

    private static string AppendNote(string? notes, string add)
        => string.IsNullOrWhiteSpace(notes) ? add : $"{notes}\n{add}";

    private static string BuildUser(IReadOnlyList<SemanticLedgerEntry> batch, string? transcript)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(transcript))
        {
            sb.AppendLine("TRANSCRIPT (Kontext für scope/timeScope/disposition):");
            sb.AppendLine(transcript);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("Weise die Facetten NUR anhand der Proposition + der angehängten Evidence zu.");
            sb.AppendLine();
        }
        sb.AppendLine($"CLAIMS ({batch.Count}) — gib GENAU {batch.Count} Zuweisungen zurück, eine pro id:");
        foreach (var e in batch)
        {
            sb.AppendLine($"- id: {e.Id}");
            sb.AppendLine($"  proposition: {e.Proposition}");
            var ev = e.Evidence.Count == 0 ? "(keine)" : string.Join(" | ", e.Evidence.Select(x => x.Quote));
            sb.AppendLine($"  evidence: {ev}");
        }
        return sb.ToString();
    }

    public static IReadOnlyList<AssignedFacets> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            var r = JsonSerializer.Deserialize<AssignmentResult>(json, Json);
            return r?.Items.Where(v => !string.IsNullOrWhiteSpace(v.Id)).ToList() ?? [];
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

    private sealed record AssignmentResult([property: JsonPropertyName("items")] IReadOnlyList<AssignedFacets> Items);
}

public sealed record AssignedFacets(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("modality")] string Modality,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("timeScope")] string? TimeScope,
    [property: JsonPropertyName("riskLevel")] string RiskLevel,
    [property: JsonPropertyName("disposition")] IReadOnlyDictionary<string, ArtifactDisposition> Disposition);
