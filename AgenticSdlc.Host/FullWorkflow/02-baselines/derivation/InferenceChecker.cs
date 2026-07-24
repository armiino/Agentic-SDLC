using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Derivation;

/// <summary>
/// I-c — der Inference-Checker: der SEMANTISCHE Teil, den die deterministische Anker-Validierung (I-b, C1'/C2')
/// nicht leisten kann. Prüft je abgeleitetem Risiko gegen seine ZITIERTEN Anker-Requirements: folgt es plausibel,
/// widerspricht/übertreibt es sie nicht, bleibt es im Scope. Fängt den „dekorativen Anker" (Risiko zitiert REQ-12,
/// handelt aber von etwas anderem). KEIN Coverage-Check (open-world) — Vollständigkeit ist hier nicht messbar.
/// </summary>
/// <remarks>
/// Bounded LLM (temp=0, structured output, gebündelt — wie <see cref="MakerChecker.ContractCritic"/>, aber
/// inference-mode statt extraction-mode). EHRLICH: eine LLM-Relevanzprüfung = SCHWACHE Garantie → der Mensch bleibt
/// tragend (I-d). Der Checker ist ein FILTER (fängt „widerspricht"/„unpassend"), kein Korrektheitsbeweis.
/// Verdikt je Risiko: supported | contradicts | unrelated | unclear.
/// </remarks>
public sealed class InferenceChecker
{
    public const int DefaultBatchSize = 8;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Eingebauter Default-Maßstab (Risiko-Fidelity, inkl. Scope-Creep-Regel). Wird verwendet, wenn kein
    /// spec-gewählter Judge-Prompt übergeben wird → alle Alt-Aufrufer bleiben verhaltensidentisch. Für andere
    /// Ableitungs-Ziele (z. B. Requirements-Elaboration, wo Scope-Erweiterung der SINN ist) wird ein anderer Maßstab
    /// per Spec/Prompt-Datei gewählt und über den Konstruktor injiziert.</summary>
    private const string DefaultSystemPrompt = """
        Du prüfst ABGELEITETE Risiken gegen ihre zitierten ANFORDERUNGEN (die einzige erlaubte Grundlage).
        Für JEDES Risiko bekommst du: den Risikotext, seine Annahmen, seine Begründung UND die zitierten
        Anforderung(en) (Text). Deine EINZIGE Frage: Ist dieses abgeleitete Risiko gegenüber SEINEN zitierten
        Anforderungen tragfähig?

        Verdikt je Risiko (genau EINES):
        - "supported": das Risiko FOLGT plausibel aus der/den zitierten Anforderung(en), WIDERSPRICHT ihnen NICHT
          und WEITET ihren Scope NICHT unzulässig aus; die Annahmen sind nachvollziehbar und explizit.
        - "contradicts": das Risiko widerspricht der/den Anforderung(en) ODER verstärkt/erweitert deren Scope
          unzulässig (z. B. behauptet Verbindlichkeit/Umfang, den die Anforderung nicht hergibt).
        - "unrelated": das Risiko folgt NICHT aus der/den zitierten Anforderung(en) (dekorativer/falscher Anker) —
          es handelt von etwas anderem.
        - "unclear": nicht entscheidbar.

        WICHTIG:
        - Prüfe NUR gegen die gelieferten Anforderungen. KEIN Weltwissen, KEIN Transkript, KEINE Vollständigkeits-
          bewertung („fehlt noch ein Risiko?" ist NICHT deine Frage).
        - Ein Risiko darf NEUEN Inhalt enthalten (das ist der Sinn der Ableitung) — solange er plausibel aus den
          Anforderungen folgt und explizite Annahmen trägt. Neuer Inhalt allein ist KEIN Fehler.
        - Erfundene konkrete Zahlen/Fristen/Gesetze/Schadenshöhen ohne Grundlage -> Richtung contradicts/unrelated.

        Für JEDE Input-ref GENAU EIN Objekt mit DERSELBEN ref (gleiche Anzahl).
        Antworte ausschliesslich mit JSON:
        { "items": [ { "ref": "<exakt die ref>", "verdict": "supported", "rationale": "<kurz>" } ] }
        """;

    private static readonly string SchemaTemplate = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  __REASONING_PROP__"ref": { "type": "string" },
                  "verdict": { "type": "string" },
                  "rationale": { "type": "string" }
                },
                "required": [__REASONING_REQ__"ref", "verdict", "rationale"],
                "additionalProperties": false
              }
            }
          },
          "required": ["items"],
          "additionalProperties": false
        }
        """;

    // W1a: Schema je ReasoningCapture-Modus (Marker-Ersetzung, kein Brace-Doubling).
    private static string BuildSchemaJson(ReasoningCapture reasoning) => SchemaTemplate
        .Replace("__REASONING_PROP__", ReasoningSchema.PropertyJson(reasoning))
        .Replace("__REASONING_REQ__", ReasoningSchema.RequiredToken(reasoning));

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;
    private readonly string _systemPrompt;
    private readonly ChatResponseFormat _responseFormat;

    /// <param name="systemPrompt">Der Judge-Maßstab. <c>null</c> → eingebauter Risiko-Default (<see cref="DefaultSystemPrompt"/>).
    /// Ein spec-gewählter Prompt macht den Maßstab task-abhängig (die vierte Config-Achse neben Tooling/Input/Prompt).</param>
    public InferenceChecker(IChatClient client, bool structuredOutput = true, int batchSize = DefaultBatchSize, string? systemPrompt = null, ReasoningCapture reasoning = ReasoningCapture.Enforced)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 16);
        // W1a: reasoning log-only via Prompt-Appendix + Schema-Feld (bei off beides leer).
        _systemPrompt = (string.IsNullOrWhiteSpace(systemPrompt) ? DefaultSystemPrompt : systemPrompt) + ReasoningSchema.PromptAppendix(reasoning);
        _responseFormat = ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(BuildSchemaJson(reasoning)).RootElement.Clone(),
            "inference_check_batch",
            "Relevanz-/Widerspruchs-Verdikt je abgeleitetem Risiko gegen seine zitierten Anforderungen.");
    }

    public async Task<InferenceCheckReport> CheckAsync(
        IReadOnlyList<ArtifactItem> derivedRisks, IReadOnlyDictionary<string, ArtifactItem> baselineById, CancellationToken ct)
    {
        var verdicts = new List<InferenceVerdict>();
        for (var i = 0; i < derivedRisks.Count; i += _batchSize)
        {
            var batch = derivedRisks.Skip(i).Take(_batchSize).ToList();
            verdicts.AddRange(await CheckBatchAsync(batch, baselineById, ct).ConfigureAwait(false));
        }

        var byVerdict = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var flagged = verdicts.Where(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated).ToList();
        return new InferenceCheckReport(
            Pass: flagged.Count == 0,
            Total: derivedRisks.Count,
            ByVerdict: byVerdict,
            Verdicts: verdicts,
            Flagged: flagged);
    }

    private async Task<IReadOnlyList<InferenceVerdict>> CheckBatchAsync(
        IReadOnlyList<ArtifactItem> batch, IReadOnlyDictionary<string, ArtifactItem> baselineById, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = _responseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, _systemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch, baselineById))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text).ToDictionary(v => v.Ref, v => v, StringComparer.OrdinalIgnoreCase);

        var results = new List<InferenceVerdict>(batch.Count);
        foreach (var risk in batch)
        {
            var kind = parsed.TryGetValue(risk.ItemId, out var raw) ? MapVerdict(raw.Verdict) : InferenceVerdictKind.Unclear;
            var rationale = raw?.Rationale ?? "Kein Verdikt vom Modell (fail-open zu unclear).";
            results.Add(new InferenceVerdict(risk.ItemId, risk.SourceArtifactItemIds, kind, rationale));
        }
        return results;
    }

    private static string BuildUser(IReadOnlyList<ArtifactItem> batch, IReadOnlyDictionary<string, ArtifactItem> baselineById)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ABGELEITETE RISIKEN ({batch.Count}) — prüfe jedes gegen SEINE zitierten Anforderungen:");
        sb.AppendLine();
        foreach (var r in batch)
        {
            sb.AppendLine($"ref: {r.ItemId}");
            sb.AppendLine($"  risiko: {r.Text}");
            if (r.Assumptions is { Count: > 0 }) sb.AppendLine($"  annahmen: {string.Join(" | ", r.Assumptions)}");
            if (!string.IsNullOrWhiteSpace(r.DerivationRationale)) sb.AppendLine($"  begründung: {r.DerivationRationale}");
            sb.AppendLine("  zitierte anforderungen:");
            foreach (var id in r.SourceArtifactItemIds)
                sb.AppendLine(baselineById.TryGetValue(id, out var req)
                    ? $"    - {id}: {req.Text}"
                    : $"    - {id}: (NICHT in der Baseline)");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static InferenceVerdictKind MapVerdict(string? v) => (v ?? "").Trim().ToLowerInvariant() switch
    {
        "supported" => InferenceVerdictKind.Supported,
        "contradicts" => InferenceVerdictKind.Contradicts,
        "unrelated" => InferenceVerdictKind.Unrelated,
        _ => InferenceVerdictKind.Unclear
    };

    private static IReadOnlyList<RawVerdict> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try { return JsonSerializer.Deserialize<RawResult>(json, Json)?.Items?.Where(v => !string.IsNullOrWhiteSpace(v.Ref)).ToList() ?? []; }
        catch (JsonException) { return []; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record RawResult([property: JsonPropertyName("items")] IReadOnlyList<RawVerdict> Items);
    private sealed record RawVerdict(
        [property: JsonPropertyName("ref")] string Ref,
        [property: JsonPropertyName("verdict")] string Verdict,
        [property: JsonPropertyName("rationale")] string? Rationale);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InferenceVerdictKind { Supported, Contradicts, Unrelated, Unclear }

public sealed record InferenceVerdict(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("anchors")] IReadOnlyList<string> Anchors,
    [property: JsonPropertyName("verdict")] InferenceVerdictKind Verdict,
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record InferenceCheckReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("byVerdict")] IReadOnlyDictionary<string, int> ByVerdict,
    [property: JsonPropertyName("verdicts")] IReadOnlyList<InferenceVerdict> Verdicts,
    [property: JsonPropertyName("flagged")] IReadOnlyList<InferenceVerdict> Flagged);
