using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Derivation;

/// <summary>
/// I-b — der erste Derivation-Agent. Liest die geprüfte, id-adressierte Requirements-Baseline und leitet NEUE
/// Risiken ab, jedes verankert an <c>sourceArtifactItemIds</c> (REQ-ids) und mit expliziten Annahmen. Open-world
/// (neuer Inhalt), aber ANKER-gebunden — der Übergang von Extraktion zu Inferenz.
/// </summary>
/// <remarks>
/// Bewusst bounded (structured output, temp niedrig): der Agent bekommt NUR die Baseline-Items (Projektion), nicht
/// den Ledger oder das Transkript — die Inferenz ist auf die geprüfte Upstream-Quelle begrenzt (kleinere
/// Fabrikationsfläche). Das System-Prompt ist extern/versioniert (PromptProvider); das Schema ist hier in Code.
/// Der Agent generiert nur — die Anker-Validierung (deterministisch) und der Relevanz-/Widerspruchs-Check (I-c)
/// laufen separat.
/// </remarks>
public sealed class DerivedRisksAgent
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "decision": { "type": "string" },
            "risks": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "text": { "type": "string" },
                  "sourceArtifactItemIds": { "type": "array", "items": { "type": "string" } },
                  "assumptions": { "type": "array", "items": { "type": "string" } },
                  "rationale": { "type": "string" }
                },
                "required": ["text", "sourceArtifactItemIds", "assumptions", "rationale"],
                "additionalProperties": false
              }
            }
          },
          "required": ["decision", "risks"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "derived_risks",
        "Abgeleitete Risiken mit Anker (sourceArtifactItemIds) und expliziten Annahmen.");

    private readonly IChatClient _client;
    private readonly string _systemPrompt;
    private readonly bool _structuredOutput;

    public DerivedRisksAgent(IChatClient client, string systemPrompt, bool structuredOutput = true)
    {
        _client = client;
        _systemPrompt = systemPrompt;
        _structuredOutput = structuredOutput;
    }

    /// <summary>Leitet Risiken aus den Baseline-Items ab. Liefert die Roh-Risiken + die Entscheidung
    /// (create|no_new_risk); die Verankerungs-Validierung macht der Aufrufer deterministisch.</summary>
    public async Task<(string Decision, IReadOnlyList<RawDerivedRisk> Risks)> DeriveAsync(
        IReadOnlyList<ArtifactItem> baseline, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.2f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, _systemPrompt), new ChatMessage(ChatRole.User, BuildUser(baseline))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        return (parsed?.Decision ?? "unknown", parsed?.Risks ?? []);
    }

    private static string BuildUser(IReadOnlyList<ArtifactItem> baseline)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GEPRÜFTE ANFORDERUNGEN ({baseline.Count}) — leite hieraus (und NUR hieraus) neue Risiken ab:");
        sb.AppendLine();
        foreach (var it in baseline)
            sb.Append("- ").Append(it.ItemId).Append(": ").AppendLine(it.Text);
        return sb.ToString();
    }

    private static RawResult? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<RawResult>(json, Json); }
        catch (JsonException) { return null; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record RawResult(
        [property: JsonPropertyName("decision")] string? Decision,
        [property: JsonPropertyName("risks")] IReadOnlyList<RawDerivedRisk>? Risks);
}

/// <summary>Ein vom LLM abgeleitetes Roh-Risiko (vor Anker-Validierung + ID-Vergabe).</summary>
public sealed record RawDerivedRisk(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds,
    [property: JsonPropertyName("assumptions")] IReadOnlyList<string> Assumptions,
    [property: JsonPropertyName("rationale")] string Rationale);
