using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Prueft Semantic Ledger Entries gegen ArtifactClaims facettenbasiert.</summary>
public sealed class SemanticPreservationVerifier
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du pruefst, ob ArtifactClaims die Semantik bestaetigter Source-Ledger-Eintraege erhalten.

        Entscheide fuer jeden SourceClaim gegen die Liste der ArtifactClaims.
        Waehle alle noetigen passenden ArtifactClaims oder markiere representation=missing.
        Wenn ein SourceClaim korrekt ueber mehrere ArtifactClaims verteilt ist, darfst du diese gemeinsam bewerten.

        Facetten:
        - representation: missing | present | duplicate | unclear
        - proposition: preserved | partial | missing | shifted | contradicted | unclear
        - status: preserved | missing | shifted | unclear
        - modality: preserved | missing | shifted | unclear
        - scope: preserved | missing | shifted | unclear
        - timeScope: preserved | missing | shifted | unclear

        Verletzungen:
        - STATUS_SHIFT: z.B. undecided/open -> planned/committed/required
        - MODALITY_SHIFT: z.B. desired/optional -> must
        - SCOPE_SHIFT: z.B. later/not_mvp/open -> MVP
        - TIME_SCOPE_SHIFT: z.B. later_possible -> mvp
        - PROPOSITION_SHIFT: Bedeutung wurde veraendert
        - CONTRADICTION: inhaltlich unvereinbar

        Wichtig:
        - Thematische Naehe reicht nicht fuer preserved.
        - Wenn die Proposition vorhanden ist, aber Status/Scope veraendert wurde, ist das MISREPRESENTED.
        - Generische DSGVO-/Risiko-/Architektur-Erwaehnungen reichen nicht fuer preserved.
        - Gib fuer jede SourceClaimId genau ein Item zurueck.

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "sourceClaimId": "SC-...",
              "artifactClaimIds": ["REQ-001", "REQ-002"],
              "representation": "missing|present|duplicate|unclear",
              "proposition": "preserved|partial|missing|shifted|contradicted|unclear",
              "status": "preserved|missing|shifted|unclear",
              "modality": "preserved|missing|shifted|unclear",
              "scope": "preserved|missing|shifted|unclear",
              "timeScope": "preserved|missing|shifted|unclear",
              "violations": ["STATUS_SHIFT"],
              "overallVerdict": "PRESERVED|OMITTED|PARTIALLY_PRESERVED|MISREPRESENTED|CONTRADICTED|UNCLEAR",
              "missingFacets": ["..."],
              "shiftedFacets": ["..."],
              "reason": "kurz"
            }
          ]
        }
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
                  "sourceClaimId": { "type": "string" },
                  "artifactClaimIds": { "type": "array", "items": { "type": "string" } },
                  "representation": { "type": "string" },
                  "proposition": { "type": "string" },
                  "status": { "type": "string" },
                  "modality": { "type": "string" },
                  "scope": { "type": "string" },
                  "timeScope": { "type": "string" },
                  "violations": { "type": "array", "items": { "type": "string" } },
                  "overallVerdict": { "type": "string" },
                  "missingFacets": { "type": "array", "items": { "type": "string" } },
                  "shiftedFacets": { "type": "array", "items": { "type": "string" } },
                  "reason": { "type": "string" }
                },
                "required": ["sourceClaimId", "artifactClaimIds", "representation", "proposition", "status", "modality", "scope", "timeScope", "violations", "overallVerdict", "missingFacets", "shiftedFacets", "reason"],
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
        "semantic_preservation_verification",
        "Facettenbasierte Semantik-Erhaltungspruefung.");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;

    public SemanticPreservationVerifier(IChatClient client, bool structuredOutput = true)
    {
        _client = client;
        _structuredOutput = structuredOutput;
    }

    public async Task<IReadOnlyList<SemanticVerificationItem>> VerifyAsync(
        IReadOnlyList<SemanticLedgerEntry> entries,
        IReadOnlyList<GeneratedArtifactClaim> artifactClaims,
        string artifact,
        CancellationToken ct)
    {
        var user = $"""
            ARTEFAKT: {artifact}

            SOURCE_LEDGER:
            {JsonSerializer.Serialize(entries, Json)}

            ARTIFACT_CLAIMS:
            {JsonSerializer.Serialize(artifactClaims, Json)}
            """;

        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, user)],
            options, ct).ConfigureAwait(false);

        return Parse(response.Text)?.Items.Select(i => i.Normalized()).ToList() ?? [];
    }

    public static SemanticVerificationPayload? Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize<SemanticVerificationPayload>(json, Json); }
        catch (JsonException) { return null; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }
}

public static class SemanticLedgerChecks
{
    public static IReadOnlyList<DispositionCheckItem> CheckDispositionCoverage(
        IReadOnlyList<SemanticLedgerEntry> entries,
        IReadOnlyList<GeneratedArtifactClaim> claims,
        string artifact)
    {
        var claimSourceIds = claims
            .SelectMany(c => c.SourceClaimIds)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return entries.Select(e =>
        {
            var disposition = e.Disposition.TryGetValue(artifact, out var found)
                ? found
                : new ArtifactDisposition("not_applicable", "consciously_omitted");
            var applicability = NormalizeApplicability(disposition.Applicability);
            var required = applicability == "required";
            var hasClaim = claimSourceIds.Contains(e.Id);
            var verdict = required && !hasClaim ? "required_disposition_missing"
                : !required && hasClaim ? "represented_without_required_disposition"
                : "ok";
            return new DispositionCheckItem(e.Id, applicability, required, hasClaim, verdict);
        }).ToList();
    }

    public static IReadOnlyList<BatchCompletenessIssue> CheckVerifierCompleteness(
        IReadOnlyList<SemanticLedgerEntry> entries,
        IReadOnlyList<SemanticVerificationItem> items)
    {
        var issues = new List<BatchCompletenessIssue>();
        var expected = entries.Select(e => e.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (!expected.Contains(item.SourceClaimId))
            {
                issues.Add(new BatchCompletenessIssue("unknown_source_claim_id", item.SourceClaimId));
            }
            if (!seen.Add(item.SourceClaimId))
            {
                issues.Add(new BatchCompletenessIssue("duplicate_source_claim_id", item.SourceClaimId));
            }
            if (item.Representation == "unclear" || item.Proposition == "unclear")
            {
                issues.Add(new BatchCompletenessIssue("unclear_core_facet", item.SourceClaimId));
            }
        }

        foreach (var missing in expected.Where(id => !seen.Contains(id)))
        {
            issues.Add(new BatchCompletenessIssue("missing_source_claim_id", missing));
        }

        return issues;
    }

    public static object Metrics(
        IReadOnlyList<SemanticLedgerEntry> entries,
        IReadOnlyList<GeneratedArtifactClaim> claims,
        IReadOnlyList<SemanticVerificationItem> verification,
        string artifact)
    {
        var disposition = CheckDispositionCoverage(entries, claims, artifact);
        var total = Math.Max(1, entries.Count);
        var required = Math.Max(1, disposition.Count(d => d.Required));
        var preserved = verification.Count(v => v.OverallVerdict == "PRESERVED");
        var omitted = verification.Count(v => v.OverallVerdict == "OMITTED");
        var misrepresented = verification.Count(v => v.OverallVerdict == "MISREPRESENTED");
        var contradicted = verification.Count(v => v.OverallVerdict == "CONTRADICTED");
        var partial = verification.Count(v => v.OverallVerdict == "PARTIALLY_PRESERVED");
        var unclear = verification.Count(v => v.OverallVerdict == "UNCLEAR");
        var statusShifts = verification.Count(v => v.Violations.Contains("STATUS_SHIFT"));
        var scopeShifts = verification.Count(v => v.Violations.Contains("SCOPE_SHIFT") || v.Violations.Contains("TIME_SCOPE_SHIFT"));
        var unsupportedAssumptions = claims.Count(c => c.Assumption || c.SourceClaimIds.Count == 0);
        var dispositionCovered = disposition.Count(d => !d.Required || d.HasArtifactClaim);
        var semanticallyRepresented = verification.Count(v => v.OverallVerdict != "OMITTED");
        var requiredIds = disposition
            .Where(d => d.Required)
            .Select(d => d.SourceClaimId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requiredVerification = verification
            .Where(v => requiredIds.Contains(v.SourceClaimId))
            .ToList();
        var requiredTotal = Math.Max(1, requiredVerification.Count);

        return new
        {
            total = entries.Count,
            artifactClaims = claims.Count,
            requiredDispositions = disposition.Count(d => d.Required),
            semanticRepresentationCoverage = Math.Round(semanticallyRepresented / (double)total, 4),
            sourceIdDispositionCoverage = Math.Round(dispositionCovered / (double)total, 4),
            requiredSourceIdDispositionCoverage = Math.Round(disposition.Count(d => d.Required && d.HasArtifactClaim) / (double)required, 4),
            semanticPreservation = Math.Round(preserved / (double)total, 4),
            requiredSemanticRepresentationCoverage = Math.Round(requiredVerification.Count(v => v.OverallVerdict != "OMITTED") / (double)requiredTotal, 4),
            requiredSemanticPreservation = Math.Round(requiredVerification.Count(v => v.OverallVerdict == "PRESERVED") / (double)requiredTotal, 4),
            requiredOmitted = requiredVerification.Count(v => v.OverallVerdict == "OMITTED"),
            requiredMisrepresented = requiredVerification.Count(v => v.OverallVerdict == "MISREPRESENTED"),
            preserved,
            partial,
            omitted,
            misrepresented,
            contradicted,
            unclear,
            statusShifts,
            scopeShifts,
            unsupportedAssumptions
        };
    }

    private static string NormalizeApplicability(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "required" => "required",
        "optional" => "optional",
        "context" => "context",
        "not_applicable" => "not_applicable",
        _ => "not_applicable"
    };
}
