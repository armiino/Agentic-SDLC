using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Derivation;

/// <summary>
/// B0 · R3 — die EINZIGE Judge-Dimension der Ableitungsgüte: Scope-Creep / Halluzination. Prüft je abgeleitetem Item,
/// ob es eine KONKRETE, nicht aus den Ankern belegte Spezifik einführt (Zahl, Frist/Datum, Gesetz/Norm, Prozentsatz,
/// Schadenshöhe/Geldbetrag). Das ist die klassische Autonomie-Gefahr: je freier der Agent, desto größer die Versuchung,
/// über die Evidenz hinaus zu erfinden. Komplementär zu R1 (Treue-Verdikt) — R3 fängt subtile Erfindung, die (noch)
/// kein harter Widerspruch ist. Definition: <c>docs/B0-Ableitungsguete-Metrik.md</c>.
/// </summary>
/// <remarks>
/// Bounded Judge wie <see cref="InferenceChecker"/> (temp=0, structured output, batched). EHRLICH: eine LLM-Prüfung ist
/// eine SCHWACHE Garantie; R3 ist ein Halluzinations-Indikator, kein Beweis. Verdikt je Item: clean | invented.
/// </remarks>
public sealed class ScopeCreepChecker
{
    public const int DefaultBatchSize = 8;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du prüfst ABGELEITETE Aussagen auf SCOPE-CREEP / HALLUZINATION gegen ihre zitierten ANKER (die einzige erlaubte
        Grundlage). Für JEDE Aussage bekommst du: den Text, seine Annahmen, seine Begründung UND die zitierten Anker
        (Text). Deine EINZIGE Frage: Führt die Aussage eine KONKRETE Spezifik ein — eine Zahl, Frist/ein Datum, ein
        Gesetz/eine Norm, einen Prozentsatz, eine Schadenshöhe/einen Geldbetrag —, die NICHT aus den Ankern (oder einer
        EXPLIZIT benannten Annahme) hervorgeht?

        Verdikt je Aussage (genau EINES):
        - "clean": keine erfundene konkrete Spezifik; alle konkreten Werte stehen in den Ankern oder in einer explizit
          benannten Annahme. Qualitative Ableitung ohne konkrete Werte = clean.
        - "invented": mindestens eine konkrete Zahl/Frist/Gesetz/Prozent/Schadenshöhe ist WEDER durch einen Anker NOCH
          durch eine explizite Annahme gedeckt (aus der Luft gegriffen).

        WICHTIG:
        - NEUER qualitativer Inhalt ist ERLAUBT (das ist der Sinn der Ableitung) und allein KEIN "invented".
        - Es geht NUR um unbelegte KONKRETE Werte, nicht um Plausibilität/Vollständigkeit.
        - Eine als Annahme EXPLIZIT gekennzeichnete Zahl ist gedeckt → clean.

        Für JEDE Input-ref GENAU EIN Objekt mit DERSELBEN ref (gleiche Anzahl).
        Antworte ausschliesslich mit JSON:
        { "items": [ { "ref": "<exakt die ref>", "verdict": "clean", "rationale": "<kurz>" } ] }
        """;

    private static readonly string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "ref": { "type": "string" },
                  "verdict": { "type": "string" },
                  "rationale": { "type": "string" }
                },
                "required": ["ref", "verdict", "rationale"],
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
        "scope_creep_batch",
        "Scope-Creep-/Halluzinations-Verdikt je abgeleitetem Item gegen seine zitierten Anker.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;

    public ScopeCreepChecker(IChatClient client, bool structuredOutput = true, int batchSize = DefaultBatchSize)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 16);
    }

    public async Task<ScopeCreepReport> CheckAsync(
        IReadOnlyList<ArtifactItem> items, IReadOnlyDictionary<string, ArtifactItem> baselineById, CancellationToken ct)
    {
        var verdicts = new List<ScopeCreepVerdict>();
        for (var i = 0; i < items.Count; i += _batchSize)
            verdicts.AddRange(await CheckBatchAsync(items.Skip(i).Take(_batchSize).ToList(), baselineById, ct).ConfigureAwait(false));

        var invented = verdicts.Count(v => v.Invented);
        return new ScopeCreepReport(
            Total: items.Count,
            Invented: invented,
            InventedRate: items.Count == 0 ? 0.0 : Math.Round((double)invented / items.Count, 4),
            Verdicts: verdicts);
    }

    private async Task<IReadOnlyList<ScopeCreepVerdict>> CheckBatchAsync(
        IReadOnlyList<ArtifactItem> batch, IReadOnlyDictionary<string, ArtifactItem> baselineById, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch, baselineById))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text).ToDictionary(v => v.Ref, v => v, StringComparer.OrdinalIgnoreCase);

        var results = new List<ScopeCreepVerdict>(batch.Count);
        foreach (var it in batch)
        {
            // fail-open: kein Verdikt → clean (nicht als Halluzination werten, sonst Fehlmessung nach oben).
            var raw = parsed.GetValueOrDefault(it.ItemId);
            var invented = string.Equals(raw?.Verdict?.Trim(), "invented", StringComparison.OrdinalIgnoreCase);
            results.Add(new ScopeCreepVerdict(it.ItemId, invented, raw?.Rationale ?? "Kein Verdikt vom Modell (fail-open zu clean)."));
        }
        return results;
    }

    private static string BuildUser(IReadOnlyList<ArtifactItem> batch, IReadOnlyDictionary<string, ArtifactItem> baselineById)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ABGELEITETE AUSSAGEN ({batch.Count}) — prüfe jede auf unbelegte konkrete Spezifik gegen SEINE Anker:");
        sb.AppendLine();
        foreach (var it in batch)
        {
            sb.AppendLine($"ref: {it.ItemId}");
            sb.AppendLine($"  aussage: {it.Text}");
            if (it.Assumptions is { Count: > 0 }) sb.AppendLine($"  annahmen: {string.Join(" | ", it.Assumptions)}");
            if (!string.IsNullOrWhiteSpace(it.DerivationRationale)) sb.AppendLine($"  begründung: {it.DerivationRationale}");
            sb.AppendLine("  zitierte anker:");
            foreach (var id in it.SourceArtifactItemIds)
                sb.AppendLine(baselineById.TryGetValue(id, out var a) ? $"    - {id}: {a.Text}" : $"    - {id}: (NICHT in der Baseline)");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static IReadOnlyList<RawVerdict> Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var s = text.IndexOf('{'); var e = text.LastIndexOf('}');
        if (s < 0 || e <= s) return [];
        try { return JsonSerializer.Deserialize<RawResult>(text.Substring(s, e - s + 1), Json)?.Items?.Where(v => !string.IsNullOrWhiteSpace(v.Ref)).ToList() ?? []; }
        catch (JsonException) { return []; }
    }

    private sealed record RawResult([property: JsonPropertyName("items")] IReadOnlyList<RawVerdict> Items);
    private sealed record RawVerdict(
        [property: JsonPropertyName("ref")] string Ref,
        [property: JsonPropertyName("verdict")] string Verdict,
        [property: JsonPropertyName("rationale")] string? Rationale);
}

public sealed record ScopeCreepVerdict(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("invented")] bool Invented,
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record ScopeCreepReport(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("invented")] int Invented,
    [property: JsonPropertyName("r3_scopeCreepRate")] double InventedRate,
    [property: JsonPropertyName("verdicts")] IReadOnlyList<ScopeCreepVerdict> Verdicts);
