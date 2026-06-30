using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Claim-Pilot Stufe 3 (ImplementClaimAnsatz §10): die letzte ungetestete Komponente — Atomisierung.
/// Zerlegt EINE (oft zusammengesetzte) Markdown-Unit in atomare, einzeln prüfbare Claims. Motiviert durch
/// den E2E-Befund 2d7b09-FP-BESTELLUEBERSICHT: ein Compound-Claim („zeigt Angebote, Bestellungen UND
/// Rechnungen an") wurde wegen EINES schwachen Teils ganz geflaggt — Atomisierung würde die gedeckten Teile
/// retten und nur den ungedeckten flaggen. Isoliert, additiv, nicht in der produktiven GroundingAxis.
/// </summary>
public sealed class ClaimSplitter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du zerlegst EINE Aussage (Unit) eines SDLC-Artefakts in ATOMARE, einzeln pruefbare Claims.
        Ein atomarer Claim enthaelt GENAU EINE pruefbare Aussage.

        Regeln:
        - Aufzaehlungen ('A, B und C') in mehrere Claims trennen, WENN A/B/C unabhaengig pruefbar sind.
        - Eine Modalitaet/Bedingung, die zum Kern gehoert ('nur', 'ausschliesslich', 'im MVP', 'spaeter'),
          BLEIBT im jeweiligen atomaren Claim erhalten — sie ist pruef-relevant, nicht abtrennen.
        - Erhalte pruef-relevante Facetten explizit: Komponente/Oberflaeche, Akteur/Nutzer, Aktion, Objekt,
          Modalitaet, Status und Scope. Diese Facetten duerfen nicht still verschwinden.
        - Du darfst den atomaren Claim minimal semantisch normalisieren, damit er gegen Transkript-Evidence
          pruefbar ist. Beispiel: "Frontend zeigt Bestellungen an" kann als "Kunden koennen Bestellungen im
          Kundenportal sehen" normalisiert werden, ABER die Facette component="Frontend/Kundenportal" muss
          erhalten bleiben, damit sie spaeter separat geprueft werden kann.
        - Ist die Unit bereits atomar: gib sie UNVERAENDERT als einen Claim zurueck.
        - Keine Aussage erfinden, keine weglassen, nicht umdeuten. Reines Zerlegen.
        - Pro Claim ein vollstaendiger, fuer sich verstaendlicher Satz (keine blossen Stichworte).

        Antworte ausschliesslich mit JSON:
        {
          "atomicClaims": [
            {
              "artifactQuote": "Originalaussage oder relevanter Ausschnitt",
              "normalizedClaim": "minimal normalisierter atomarer Claim",
              "facets": {
                "component": "Komponente/Oberflaeche oder null",
                "actor": "Akteur/Nutzer oder null",
                "action": "Aktion oder null",
                "object": "Objekt oder null",
                "modality": "Modalitaet/Bedingung oder null",
                "status": "Status/Entscheidungsstand oder null",
                "scope": "Scope/Zeitraum/Phase oder null"
              },
              "splitReason": "kurz"
            }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "atomicClaims": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "artifactQuote": { "type": "string" },
                  "normalizedClaim": { "type": "string" },
                  "facets": {
                    "type": "object",
                    "properties": {
                      "component": { "type": ["string", "null"] },
                      "actor": { "type": ["string", "null"] },
                      "action": { "type": ["string", "null"] },
                      "object": { "type": ["string", "null"] },
                      "modality": { "type": ["string", "null"] },
                      "status": { "type": ["string", "null"] },
                      "scope": { "type": ["string", "null"] }
                    },
                    "required": ["component", "actor", "action", "object", "modality", "status", "scope"],
                    "additionalProperties": false
                  },
                  "splitReason": { "type": "string" }
                },
                "required": ["artifactQuote", "normalizedClaim", "facets", "splitReason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["atomicClaims"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "claim_split",
        "Atomare Claims einer Artefakt-Unit.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public ClaimSplitter(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<string>> SplitAsync(string unit, string artifactType, CancellationToken ct)
    {
        var claims = await SplitDetailedAsync(unit, artifactType, ct).ConfigureAwait(false);
        return claims.Select(c => c.NormalizedClaim).ToList();
    }

    public async Task<IReadOnlyList<AtomicClaim>> SplitDetailedAsync(string unit, string artifactType, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt),
             new ChatMessage(ChatRole.User, $"ARTEFAKTTYP: {artifactType}\n\nUNIT:\n{unit}")],
            options, ct).ConfigureAwait(false);

        var claims = ParseAtomicClaims(response.Text);
        // Fallback: unzerlegbar/Parsefehler → die Unit selbst als ein Claim (kein stiller Verlust).
        return claims.Count > 0 ? claims : new[] { AtomicClaim.FromUnit(unit) };
    }

    /// <summary>Deterministisches Parsen (testbar ohne LLM). Leere/leerzeilige Claims verworfen.</summary>
    public static IReadOnlyList<string> ParseClaims(string? text)
        => ParseAtomicClaims(text).Select(c => c.NormalizedClaim).ToList();

    public static IReadOnlyList<AtomicClaim> ParseAtomicClaims(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return Array.Empty<AtomicClaim>();
        try
        {
            var p = JsonSerializer.Deserialize<Envelope>(json, Json);
            if (p?.AtomicClaims is not null)
            {
                return p.AtomicClaims
                    .Where(c => !string.IsNullOrWhiteSpace(c.NormalizedClaim))
                    .Select(c => c.Trimmed())
                    .ToList();
            }

            // Backward-compatible parser for old spike payloads/tests: { "claims": ["..."] }.
            if (p?.Claims is null) return Array.Empty<AtomicClaim>();
            return p.Claims
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => AtomicClaim.FromClaim(c))
                .ToList();
        }
        catch (JsonException) { return Array.Empty<AtomicClaim>(); }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope(
        [property: JsonPropertyName("atomicClaims")] List<AtomicClaim>? AtomicClaims,
        [property: JsonPropertyName("claims")] List<string>? Claims);
}

public sealed record AtomicClaim(
    [property: JsonPropertyName("artifactQuote")] string ArtifactQuote,
    [property: JsonPropertyName("normalizedClaim")] string NormalizedClaim,
    [property: JsonPropertyName("facets")] ClaimFacets Facets,
    [property: JsonPropertyName("splitReason")] string SplitReason)
{
    public static AtomicClaim FromUnit(string unit)
        => new(unit.Trim(), unit.Trim(), ClaimFacets.Empty, "fallback: unit unveraendert");

    public static AtomicClaim FromClaim(string claim)
        => new(claim.Trim(), claim.Trim(), ClaimFacets.Empty, "legacy claims-array");

    public AtomicClaim Trimmed()
        => this with
        {
            ArtifactQuote = ArtifactQuote.Trim(),
            NormalizedClaim = NormalizedClaim.Trim(),
            SplitReason = SplitReason.Trim(),
            Facets = Facets.Trimmed()
        };
}

public sealed record ClaimFacets(
    [property: JsonPropertyName("component")] string? Component,
    [property: JsonPropertyName("actor")] string? Actor,
    [property: JsonPropertyName("action")] string? Action,
    [property: JsonPropertyName("object")] string? Object,
    [property: JsonPropertyName("modality")] string? Modality,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("scope")] string? Scope)
{
    public static ClaimFacets Empty { get; } = new(null, null, null, null, null, null, null);

    public ClaimFacets Trimmed()
        => new(
            Normalize(Component),
            Normalize(Actor),
            Normalize(Action),
            Normalize(Object),
            Normalize(Modality),
            Normalize(Status),
            Normalize(Scope));

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        Add(map, "component", Component);
        Add(map, "actor", Actor);
        Add(map, "action", Action);
        Add(map, "object", Object);
        Add(map, "modality", Modality);
        Add(map, "status", Status);
        Add(map, "scope", Scope);
        return map;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Add(Dictionary<string, string> map, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) map[key] = value.Trim();
    }
}
