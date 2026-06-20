using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Verdikt einer einzelnen Prüfeinheit (DISK-14 Per-Item-Klassifikation).</summary>
public sealed record UnitVerdict(int Index, string Verdict, string Reason);

/// <summary>
/// DISK-14 Phase 2: begrenzte Per-Item-KLASSIFIKATION (statt offener Fehler-Generierung). Pro Einheit
/// genau ein Verdikt aus <c>grounded | overstated | fabricated | not_a_claim</c>, gechunkt (DISK-9-Lektion).
/// </summary>
/// <remarks>
/// BEWUSST ISOLIERT + REVERSIBEL: Prompts sind hier als Konstanten eingebettet (kein externer Prompt-
/// Ordner) → Verwerfen = `PerItem/`-Ordner löschen + die `classify-units`-Dispatch-Zeile. Kein Eingriff
/// in Evaluator/Jury. `not_a_claim` ist das Netz gegen Parser-Unschärfe (Metadaten/Mapping → zählt nicht).
/// </remarks>
public sealed class UnitClassifier
{
    public const int ChunkSize = 12;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string BasePrompt = """
        Du pruefst Einheiten eines SDLC-Artefakts gegen das originale Stakeholder-Transkript
        (Ground Truth). Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von
        Artefakt-Einheiten. Klassifiziere JEDE Einheit mit GENAU EINEM Verdikt:

        grounded    = Der Sachverhalt/Konflikt/die Unsicherheit wird im Transkript besprochen (auch
          sinngemaess/paraphrasiert). Unsicherheits-/Offenheitssprache ("unklar", "koennte", "offen",
          "Annahme", "noch zu klaeren") ist KEIN Fehler, solange das Thema im Transkript vorkommt und
          die Offenheit korrekt markiert ist.
        overstated  = Das Thema kommt im Transkript vor, ABER die Einheit stellt etwas als
          entschieden/gesetzt dar, das offen ist, ODER nennt eine konkrete Zahl/Technologie/Frist als
          gesetzt, die das Transkript so nicht hergibt — OHNE sie als Annahme/offen zu kennzeichnen.
        fabricated  = Der Sachverhalt kommt im Transkript UEBERHAUPT NICHT vor (frei erfunden) oder
          widerspricht ihm direkt.
        not_a_claim = Die Einheit ist kein fachlicher Claim: Metadaten, Traceability-/Owner-Mapping
          ("Thema | Stakeholdername"), reine Ueberschrift/Label, Aufzaehlung ohne Aussage. Zaehlt NICHT.

        [PROFIL]

        Antworte ausschliesslich mit JSON:
        { "results": [ { "index": 0, "verdict": "grounded|overstated|fabricated|not_a_claim", "reason": "kurz" } ] }
        Genau ein Ergebnis pro Einheit (Index = die Nummer aus der Liste). Kein Text ausserhalb des JSON.
        """;

    // Artefakttypische Bewertungs-Schwerpunkte (generisch, kein Themen-Katalog).
    private static readonly IReadOnlyDictionary<string, string> Profiles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "PROFIL requirements: Behandle konkrete Zusagen (SLA/Uptime, Zahlen, "
                + "Technologien, Fristen) streng. Ohne Transkript-Grundlage und ohne Annahme-Markierung "
                + "→ fabricated; mit Annahme-/Offen-Markierung, aber im Transkript nicht besprochen → overstated.",
            ["risks"] = "PROFIL risks: Ein Risiko DARF Unsicherheit ausdruecken. Ein im Transkript "
                + "besprochener Konflikt/eine Unsicherheit als Risiko = grounded, auch mit ergaenztem "
                + "Trigger/Auswirkung/Klaerungsbedarf. Nur ein frei erfundenes Risiko = fabricated.",
            ["architecture"] = "PROFIL architecture: Pruefe technische Festlegungen (Protokolle, Services, "
                + "Crypto/KMS, konkrete Zahlen) hart gegen Transkript-Evidenz. Erfundene Konkretisierung → "
                + "fabricated; als 'geplant/optional/MVP-minimal' markierte Offenheit → grounded.",
            ["open-questions"] = "PROFIL open-questions: Dies ist ein Dokument OFFENER FRAGEN. Eine offene "
                + "Frage DARF konkrete Beispiel-Optionen, -Parameter, -Werte, -Fristen oder -Technologien "
                + "nennen, um die Frage zu praezisieren (z. B. 'Azure AD oder Google?', "
                + "'Loeschintervall z. B. 30 Tage?', 'Token-Lebensdauer?', 'Pagination/Rate-Limiting wie?'). "
                + "Solche Beispiele in einer Frage sind NICHT fabricated und NICHT overstated — die Frage "
                + "TRIFFT keine Festlegung, sie fragt nur. Verdikt fuer eine offene Frage: grounded, wenn ihr "
                + "THEMA im Transkript besprochen wird (auch nur allgemein/angerissen); fabricated NUR, wenn "
                + "das Thema der Frage im Transkript UEBERHAUPT NICHT vorkommt. overstated ist bei Fragen "
                + "praktisch nicht anwendbar.",
            ["generic"] = "PROFIL generic: Bewerte sachlich gegen das Transkript."
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
        "peritem_classification",
        "Per-Item-Klassifikation von SDLC-Artefakt-Einheiten gegen das Transkript.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public UnitClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    /// <summary>Gewicht je Verdikt fuer den GroundingScore.</summary>
    public static int Weight(string verdict) => verdict switch
    {
        "fabricated" => 2,
        "overstated" => 1,
        _ => 0 // grounded / not_a_claim
    };

    public async Task<IReadOnlyList<UnitVerdict>> ClassifyAsync(
        string transcript, string artifactType, IReadOnlyList<ArtifactUnit> units, CancellationToken ct)
    {
        var system = BasePrompt.Replace("[PROFIL]", ProfileFor(artifactType));
        var verdicts = new Dictionary<int, UnitVerdict>();

        for (var offset = 0; offset < units.Count; offset += ChunkSize)
        {
            var chunk = units.Skip(offset).Take(ChunkSize).ToList();
            await ClassifyChunkAsync(system, transcript, chunk, verdicts, ct).ConfigureAwait(false);
        }

        // Nicht beantwortete Einheiten (sollten bei ChunkSize=12 nicht vorkommen) -> sichtbar als
        // 'unclassified' (NICHT still als grounded kaschieren).
        foreach (var u in units)
            if (!verdicts.ContainsKey(u.Index))
                verdicts[u.Index] = new UnitVerdict(u.Index, "unclassified", "kein Verdikt erhalten");

        return units.Select(u => verdicts[u.Index]).ToList();
    }

    private async Task ClassifyChunkAsync(
        string system, string transcript, List<ArtifactUnit> chunk,
        Dictionary<int, UnitVerdict> result, CancellationToken ct)
    {
        var listing = string.Join("\n", chunk.Select((u, k) => $"{k}: {u.Text}"));
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, system),
            new ChatMessage(ChatRole.User, $"TRANSKRIPT:\n{transcript}\n\nARTEFAKT-EINHEITEN:\n{listing}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput)
            options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        var json = ExtractJson(response.Text);
        if (json is null) return;

        try
        {
            var parsed = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (parsed?.Results is null) return;
            foreach (var r in parsed.Results)
                if (r.Index >= 0 && r.Index < chunk.Count)
                    result[chunk[r.Index].Index] =
                        new UnitVerdict(chunk[r.Index].Index, Normalize(r.Verdict), r.Reason ?? string.Empty);
        }
        catch (JsonException) { /* Chunk unbeantwortet -> oben als 'unclassified' sichtbar */ }
    }

    private static string ProfileFor(string artifactType)
        => Profiles.TryGetValue(artifactType, out var p) ? p : Profiles["generic"];

    private static string Normalize(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "grounded" => "grounded",
        "overstated" => "overstated",
        "fabricated" => "fabricated",
        "not_a_claim" => "not_a_claim",
        "na" => "not_a_claim",
        _ => "not_a_claim"
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
