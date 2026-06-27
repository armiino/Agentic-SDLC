using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Claim-Pilot Stufe 2 (ImplementClaimAnsatz §10): der bisher UNGETESTETE Engpass — automatische
/// Evidence-Auswahl. Bekommt EINEN Claim + das ganze (nummerierte) Transkript und wählt die Turn-Indizes,
/// die den Claim stützen ODER ihm widersprechen. Bewusst getrennt von der Verifikation: erst auswählen,
/// dann lokal urteilen. Der Spike misst, ob diese Trennung die Verifier-Gewinne (manuelle Evidenz) HÄLT.
/// Isoliert, additiv, nicht in der produktiven GroundingAxis.
/// </summary>
public sealed class EvidenceSelector
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du bekommst EINEN Claim aus einem SDLC-Artefakt und das VOLLSTAENDIGE, nummerierte Transkript.
        Deine EINZIGE Aufgabe: waehle die Turn-Indizes (Nummern), die als Evidence diesen Claim PRUEFEN —
        also ihn stuetzen ODER ihm widersprechen ODER die zugehoerige Offenheit/Modalitaet zeigen.

        Regeln:
        - Waehle FOKUSSIERT, nicht erschoepfend: nur Turns, die wirklich zur Pruefung dieses Claims beitragen.
        - Nimm auch Turns auf, die zeigen, dass etwas OFFEN/unentschieden ist (wichtig fuer Modalitaets-/
          Status-Pruefung), nicht nur bestaetigende Turns.
        - Erfinde keine Indizes. Wenn nichts relevant ist: leere Liste.
        - Maximal ~8 Turns.

        Antworte ausschliesslich mit JSON:
        { "turns": [0], "reason": "kurz, warum diese Turns den Claim pruefen" }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "turns": { "type": "array", "items": { "type": "integer" } },
            "reason": { "type": "string" }
          },
          "required": ["turns", "reason"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "evidence_selection",
        "Auswahl der relevanten Transkript-Turns für einen Claim.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _maxTurns;

    public EvidenceSelector(IChatClient client, bool structuredOutput = true, int maxTurns = 8)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _maxTurns = maxTurns;
    }

    public async Task<EvidenceSelection> SelectAsync(
        string claim, string artifactType, IReadOnlyList<TranscriptTurn> turns, CancellationToken ct)
    {
        var listing = string.Join("\n", turns.Select(t => $"{t.Index}: [{t.Speaker}] {t.Text}"));
        var user = $"ARTEFAKTTYP: {artifactType}\n\nCLAIM:\n{claim}\n\nTRANSKRIPT-TURNS:\n{listing}";

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        var parsed = ParseSelection(response.Text);
        if (parsed is null) return new EvidenceSelection(Array.Empty<int>(), "kein parsebares Ergebnis");

        // Indizes validieren (in Range, distinct, gedeckelt) — Halluzinationen verwerfen.
        var valid = parsed.Turns
            .Where(i => i >= 0 && i < turns.Count)
            .Distinct()
            .Take(_maxTurns)
            .OrderBy(i => i)
            .ToList();
        return new EvidenceSelection(valid, parsed.Reason);
    }

    /// <summary>Baut die Evidence-Spans (Verifier-Eingabe) aus den gewählten Turns.</summary>
    public static IReadOnlyList<string> ToEvidence(EvidenceSelection selection, IReadOnlyList<TranscriptTurn> turns)
        => selection.Turns.Select(i => $"[{turns[i].Speaker}] (T{i}): {turns[i].Text}").ToList();

    /// <summary>Deterministisches Parsen (testbar ohne LLM).</summary>
    public static EvidenceSelection? ParseSelection(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try
        {
            var p = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (p?.Turns is null) return null;
            return new EvidenceSelection(p.Turns, (p.Reason ?? string.Empty).Trim());
        }
        catch (JsonException) { return null; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope(
        [property: JsonPropertyName("turns")] List<int>? Turns,
        [property: JsonPropertyName("reason")] string? Reason);
}

/// <summary>Auswahl relevanter Transkript-Turns für einen Claim (validierte Indizes).</summary>
public sealed record EvidenceSelection(IReadOnlyList<int> Turns, string Reason);
