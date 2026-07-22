using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// D1 / R0 (DirectTranscriptReview): prüft ein Artefakt in EINEM LLM-Call **direkt gegen das Roh-Transkript**
/// — OHNE Topic-Fixture. Zwei Befund-Arten: <c>missing</c> (Achse coverage: ein wichtiges Transkript-Thema
/// fehlt im Artefakt) und <c>false_claim</c> (Achse grounding: eine Artefakt-Aussage ist nicht durchs
/// Transkript gedeckt). Bewusst die Alternative zum TopicCoverage-Pfad, um die Closure-Frage D1 zu klären:
/// braucht die Mess-Schicht die Fixture überhaupt? (Plan: NextStep/D1-direct-vs-topic-plan.md.)
/// </summary>
public sealed class DirectReviewClassifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du prüfst ein bereits erzeugtes frühes SDLC-Artefakt DIREKT gegen das vollständige Roh-Transkript
        eines Stakeholder-/Kickoff-Meetings. Es gibt KEINE vorgegebene Themenliste — du arbeitest direkt
        am Transkript. Du bewertest GENAU dieses eine Artefakt (Typ wird genannt).

        Melde zwei Arten von Befunden:
        - axis="coverage", type="missing": ein fachlich wichtiges Thema/Anliegen aus dem Transkript, das in
          DIESEM Artefakttyp stehen müsste, aber im Artefakt fehlt (auch sinngemäß nicht vorhanden).
        - axis="grounding", type="false_claim": eine Aussage IM ARTEFAKT, die das Transkript nicht stützt
          (erfunden, überzogen, oder als sicher dargestellt obwohl im Transkript offen).

        Strenge Regeln:
        - Nur fachlich relevante Befunde. KEIN Smalltalk, KEINE reine Formulierungs-Kritik.
        - Vor "missing": das GANZE Artefakt auf eine sinngemäße Entsprechung prüfen. Beurteile auf Themen-Ebene.
        - Erfinde nichts, um etwas zu melden. Wenn nichts Wichtiges auffällt: gib eine LEERE Liste zurück.
        - severity: "critical" | "medium" | "low".
        - transcriptQuote: kurzer Beleg aus dem Transkript (für missing UND false_claim).
        - artifactQuote: die betroffene Stelle im Artefakt (bei false_claim; bei missing leer lassen).

        Antworte ausschliesslich mit JSON:
        { "findings": [ { "axis": "coverage|grounding", "type": "missing|false_claim",
          "severity": "critical|medium|low", "description": "...", "transcriptQuote": "...",
          "artifactQuote": "..." } ] }
        """;

    private const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "findings": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "axis": { "type": "string" },
                  "type": { "type": "string" },
                  "severity": { "type": "string" },
                  "description": { "type": "string" },
                  "transcriptQuote": { "type": "string" },
                  "artifactQuote": { "type": "string" }
                },
                "required": ["axis", "type", "severity", "description", "transcriptQuote", "artifactQuote"],
                "additionalProperties": false
              }
            }
          },
          "required": ["findings"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "direct_review_findings",
        "Direkte Befunde eines Artefakts gegen das Roh-Transkript (missing / false_claim).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public DirectReviewClassifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<DirectFinding>?> ReviewAsync(
        string artifactType, string artifactText, string transcript, CancellationToken ct)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User,
                $"ARTEFAKTTYP: {artifactType}\n\nARTEFAKT:\n{artifactText}\n\nTRANSKRIPT:\n{transcript}")
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        return ParseFindings(response.Text);
    }

    /// <summary>
    /// Deterministisches Parsen (testbar ohne LLM). <c>null</c> = Antwort nicht parsebar (Fake-0-Schutz:
    /// darf NICHT wie ein sauberer Pass aussehen); leere Liste = valide „nichts gefunden".
    /// </summary>
    public static IReadOnlyList<DirectFinding>? ParseFindings(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;

        Envelope? parsed;
        try { parsed = JsonSerializer.Deserialize<Envelope>(json, Json); }
        catch (JsonException) { return null; }
        if (parsed?.Findings is null) return null;

        var result = new List<DirectFinding>();
        foreach (var f in parsed.Findings)
        {
            var axis = f.Axis?.Trim().ToLowerInvariant();
            if (axis is not ("coverage" or "grounding")) continue;          // unbekannte Achse verwerfen
            if (string.IsNullOrWhiteSpace(f.Description)) continue;

            result.Add(new DirectFinding(
                axis,
                (f.Type ?? string.Empty).Trim().ToLowerInvariant(),
                NormalizeSeverity(f.Severity),
                f.Description!.Trim(),
                string.IsNullOrWhiteSpace(f.TranscriptQuote) ? null : f.TranscriptQuote!.Trim(),
                string.IsNullOrWhiteSpace(f.ArtifactQuote) ? null : f.ArtifactQuote!.Trim()));
        }
        return result;
    }

    private static string NormalizeSeverity(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "critical" => "critical",
        "low" => "low",
        _ => "medium"   // default + "medium"
    };

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record Envelope([property: JsonPropertyName("findings")] List<FindingDto>? Findings);

    private sealed record FindingDto(
        [property: JsonPropertyName("axis")] string? Axis,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("severity")] string? Severity,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("transcriptQuote")] string? TranscriptQuote,
        [property: JsonPropertyName("artifactQuote")] string? ArtifactQuote);
}

/// <summary>Ein direkt am Transkript gefundener Befund (R0). Achse: coverage|grounding.</summary>
public sealed record DirectFinding(
    string Axis,
    string Type,
    string Severity,
    string Description,
    string? TranscriptQuote,
    string? ArtifactQuote);
