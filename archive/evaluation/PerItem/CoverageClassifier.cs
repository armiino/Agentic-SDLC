using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Coverage-Verdikt für einen Transkript-Turn (DISK-14 MISSING-Achse).</summary>
public sealed record TurnVerdict(int Index, string Verdict, string Reason);

/// <summary>
/// DISK-14 MISSING/Coverage-Achse (Spiegel von <see cref="UnitClassifier"/>): pro Transkript-Turn ein
/// begrenztes Verdikt <c>not_actionable | covered | missing</c> bezogen auf EIN Artefakt. Bounded
/// (fixe Turn-Menge) + gechunkt → stabil, KEINE offene "finde was fehlt"-Generierung.
/// </summary>
/// <remarks>Isoliert + reversibel wie der Rest von PerItem/ (Prompts als Konstanten, kein Jury-Eingriff).</remarks>
public sealed class CoverageClassifier
{
    public const int ChunkSize = 12;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string BasePrompt = """
        Du pruefst die ABDECKUNG eines SDLC-Artefakts gegen das originale Transkript: Welche im
        Transkript besprochenen Punkte fehlen im Artefakt? Du bekommst das VOLLSTAENDIGE Artefakt und
        eine NUMMERIERTE Liste von Transkript-Aeusserungen (Turns). Klassifiziere JEDEN Turn mit GENAU
        EINEM Verdikt, bezogen auf DIESEN Artefakttyp:

        not_actionable = Der Turn enthaelt nichts, was dieses Artefakt erfassen muesste (Smalltalk,
          Organisatorisches, Zustimmung, Wiederholung, Meta-Diskussion).
        covered        = Der Turn bringt einen fuer dieses Artefakt relevanten Punkt, der im Artefakt
          VORHANDEN ist (auch sinngemaess/paraphrasiert).
        missing        = Der Turn bringt einen fuer dieses Artefakt relevanten Punkt, der im Artefakt
          NIRGENDWO vorkommt (auch nicht sinngemaess).

        [PROFIL]

        Wichtig: Bevor du 'missing' vergibst, durchsuche das GANZE Artefakt nach einer sinngemaessen
        Entsprechung. Mehrere Turns zum selben Thema sind unabhaengig zu bewerten.

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "verdict": "not_actionable|covered|missing", "reason": "kurz" } ] }
        Genau ein Ergebnis pro Turn (Index = Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    private static readonly IReadOnlyDictionary<string, string> Profiles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "PROFIL requirements: relevant sind funktionale/nicht-funktionale "
                + "Anforderungen, Rollen/Berechtigungen, Constraints, Integrationen, Compliance-/"
                + "Datenschutz-Anforderungen.",
            ["risks"] = "PROFIL risks: relevant sind Risiken, Konflikte, Unsicherheiten, Abhaengigkeiten, "
                + "Zeit-/Ressourcen-/Kostenprobleme.",
            ["architecture"] = "PROFIL architecture: relevant sind technische Komponenten, Schnittstellen, "
                + "Hosting/Infrastruktur, Datenfluss/Integration, Security-/Betriebs-Mechanismen.",
            ["open-questions"] = "PROFIL open-questions: relevant sind explizit OFFENE Entscheidungen, "
                + "ungeklaerte Fragen, strittige oder noch nicht festgelegte Punkte.",
            ["generic"] = "PROFIL generic: relevant ist alles fachlich Wesentliche aus dem Transkript."
        };

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "results": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "index": { "type": "integer" },
                  "verdict": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["index", "verdict", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["results"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "coverage_classification",
        "Coverage-Klassifikation von Transkript-Turns gegen ein SDLC-Artefakt.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public CoverageClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public static int Weight(string verdict) => verdict == "missing" ? 1 : 0;

    public async Task<IReadOnlyList<TurnVerdict>> ClassifyAsync(
        string artifactText, string artifactType, IReadOnlyList<TranscriptTurn> turns, CancellationToken ct)
    {
        var system = BasePrompt.Replace("[PROFIL]", Profiles.TryGetValue(artifactType, out var p) ? p : Profiles["generic"]);
        var verdicts = new Dictionary<int, TurnVerdict>();

        for (var offset = 0; offset < turns.Count; offset += ChunkSize)
        {
            var chunk = turns.Skip(offset).Take(ChunkSize).ToList();
            await ClassifyChunkAsync(system, artifactText, chunk, verdicts, ct).ConfigureAwait(false);
        }

        foreach (var t in turns)
            if (!verdicts.ContainsKey(t.Index))
                verdicts[t.Index] = new TurnVerdict(t.Index, "unclassified", "kein Verdikt erhalten");

        return turns.Select(t => verdicts[t.Index]).ToList();
    }

    private async Task ClassifyChunkAsync(
        string system, string artifactText, List<TranscriptTurn> chunk,
        Dictionary<int, TurnVerdict> result, CancellationToken ct)
    {
        var listing = string.Join("\n", chunk.Select((t, k) => $"{k}: [{t.Speaker}] {t.Text}"));
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, system),
            new ChatMessage(ChatRole.User, $"ARTEFAKT:\n{artifactText}\n\nTRANSKRIPT-TURNS:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        var json = ExtractJson(response.Text);
        if (json is null) return;

        try
        {
            var parsed = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (parsed?.Results is null) return;
            foreach (var r in parsed.Results)
                if (r.Index >= 0 && r.Index < chunk.Count)
                    result[chunk[r.Index].Index] = new TurnVerdict(chunk[r.Index].Index, Normalize(r.Verdict), r.Reason ?? string.Empty);
        }
        catch (JsonException) { /* unbeantwortet -> oben 'unclassified' */ }
    }

    private static string Normalize(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "missing" => "missing",
        "covered" => "covered",
        "not_actionable" => "not_actionable",
        _ => "not_actionable"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("results")] List<Result>? Results);
    private sealed record Result(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("verdict")] string? Verdict,
        [property: JsonPropertyName("reason")] string? Reason);
}
