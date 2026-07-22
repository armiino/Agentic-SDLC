using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Ledger.Core;

public sealed record SemanticLedgerFixture(
    [property: JsonPropertyName("entries")] IReadOnlyList<SemanticLedgerEntry> Entries);

public sealed record SemanticLedgerEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("proposition")] string Proposition,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("modality")] string Modality,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("timeScope")] string? TimeScope,
    [property: JsonPropertyName("evidence")] IReadOnlyList<SemanticLedgerEvidence> Evidence,
    [property: JsonPropertyName("disposition")] IReadOnlyDictionary<string, ArtifactDisposition> Disposition,
    [property: JsonPropertyName("riskLevel")] string RiskLevel,
    [property: JsonPropertyName("notes")] string? Notes,
    // L2 Cluster-Trace (nur von der Canonicalization gesetzt): welche Candidate-IDs in diesen kanonischen
    // Claim eingingen + die angenommene Relation. Basis für die Gate-Invariante "kein Candidate verschwindet still".
    [property: JsonPropertyName("candidateIds")] IReadOnlyList<string>? CandidateIds = null,
    [property: JsonPropertyName("assumedRelation")] string? AssumedRelation = null,
    [property: JsonPropertyName("sourceUnitIds")] IReadOnlyList<string>? SourceUnitIds = null,
    // Adjudikation (A8): Claims, die per accept_gap/promote_to_claim NEU aus einer Unit entstehen, sind echt
    // evidenz-geerdet, aber noch nicht facettiert -> "pending". Ein separater Refine-Pass (A10) hebt sie auf
    // Pipeline-Niveau und löscht den Marker. Nur bei solchen Claims gesetzt (WhenWritingNull -> kein Output-Diff
    // für bestehende Pipeline-Claims).
    [property: JsonPropertyName("facetStatus")][property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FacetStatus = null);

public sealed record SemanticLedgerEvidence(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("quote")] string Quote);

public sealed record ArtifactDisposition(
    [property: JsonPropertyName("applicability")] string Applicability,
    [property: JsonPropertyName("representationMode")] string RepresentationMode);

public sealed record EvidenceFirstGenerationResult(
    [property: JsonPropertyName("markdown")] string Markdown,
    [property: JsonPropertyName("claims")] IReadOnlyList<GeneratedArtifactClaim> Claims);

public sealed record GeneratedArtifactClaim(
    [property: JsonPropertyName("artifactClaimId")] string ArtifactClaimId,
    [property: JsonPropertyName("artifact")] string Artifact,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceClaimIds")] IReadOnlyList<string> SourceClaimIds,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("modality")] string Modality,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("timeScope")] string? TimeScope,
    [property: JsonPropertyName("assumption")] bool Assumption = false)
{
    public GeneratedArtifactClaim Normalized()
        => this with
        {
            ArtifactClaimId = ArtifactClaimId.Trim(),
            Artifact = Artifact.Trim().ToLowerInvariant(),
            Text = Text.Trim(),
            SourceClaimIds = SourceClaimIds.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList(),
            Status = Status.Trim().ToLowerInvariant(),
            Modality = Modality.Trim().ToLowerInvariant(),
            Scope = Scope.Trim().ToLowerInvariant(),
            TimeScope = string.IsNullOrWhiteSpace(TimeScope) ? null : TimeScope.Trim().ToLowerInvariant()
        };
}

public sealed record ArtifactClaimExtractionResult(
    [property: JsonPropertyName("claims")] IReadOnlyList<GeneratedArtifactClaim> Claims);

public sealed record SemanticVerificationPayload(
    [property: JsonPropertyName("items")] IReadOnlyList<SemanticVerificationItem> Items);

public sealed record SemanticVerificationItem(
    [property: JsonPropertyName("sourceClaimId")] string SourceClaimId,
    [property: JsonPropertyName("artifactClaimIds")] IReadOnlyList<string> ArtifactClaimIds,
    [property: JsonPropertyName("representation")] string Representation,
    [property: JsonPropertyName("proposition")] string Proposition,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("modality")] string Modality,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("timeScope")] string TimeScope,
    [property: JsonPropertyName("violations")] IReadOnlyList<string> Violations,
    [property: JsonPropertyName("overallVerdict")] string OverallVerdict,
    [property: JsonPropertyName("missingFacets")] IReadOnlyList<string> MissingFacets,
    [property: JsonPropertyName("shiftedFacets")] IReadOnlyList<string> ShiftedFacets,
    [property: JsonPropertyName("reason")] string Reason)
{
    public SemanticVerificationItem Normalized()
    {
        var normalizedViolations = Violations
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(NormalizeViolation)
            .Where(v => v != "UNKNOWN")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var derived = DeriveOverall(
            NormalizeRepresentation(Representation),
            NormalizeFacet(Proposition),
            NormalizeFacet(Status),
            NormalizeFacet(Modality),
            NormalizeFacet(Scope),
            NormalizeFacet(TimeScope),
            normalizedViolations);

        return this with
        {
            SourceClaimId = SourceClaimId.Trim(),
            ArtifactClaimIds = ArtifactClaimIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList(),
            Representation = NormalizeRepresentation(Representation),
            Proposition = NormalizeFacet(Proposition),
            Status = NormalizeFacet(Status),
            Modality = NormalizeFacet(Modality),
            Scope = NormalizeFacet(Scope),
            TimeScope = NormalizeFacet(TimeScope),
            Violations = normalizedViolations,
            OverallVerdict = derived,
            MissingFacets = MissingFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
            ShiftedFacets = ShiftedFacets.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).ToList(),
            Reason = Reason.Trim()
        };
    }

    public static string NormalizeRepresentation(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "missing" => "missing",
        "present" => "present",
        "duplicate" => "duplicate",
        "unclear" => "unclear",
        _ => "unclear"
    };

    public static string NormalizeFacet(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "preserved" => "preserved",
        "partial" => "partial",
        "missing" => "missing",
        "shifted" => "shifted",
        "contradicted" => "contradicted",
        "unclear" => "unclear",
        _ => "unclear"
    };

    public static string NormalizeViolation(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "STATUS_SHIFT" => "STATUS_SHIFT",
        "MODALITY_SHIFT" => "MODALITY_SHIFT",
        "SCOPE_SHIFT" => "SCOPE_SHIFT",
        "TIME_SCOPE_SHIFT" => "TIME_SCOPE_SHIFT",
        "PROPOSITION_SHIFT" => "PROPOSITION_SHIFT",
        "CONTRADICTION" => "CONTRADICTION",
        _ => "UNKNOWN"
    };

    public static string DeriveOverall(
        string representation,
        string proposition,
        string status,
        string modality,
        string scope,
        string timeScope,
        IReadOnlyList<string> violations)
    {
        if (representation == "missing") return "OMITTED";
        if (representation == "unclear") return "UNCLEAR";
        if (proposition == "contradicted" || violations.Contains("CONTRADICTION")) return "CONTRADICTED";
        if (violations.Any(v => v is "STATUS_SHIFT" or "MODALITY_SHIFT" or "SCOPE_SHIFT" or "TIME_SCOPE_SHIFT" or "PROPOSITION_SHIFT")
            || status == "shifted" || modality == "shifted" || scope == "shifted" || timeScope == "shifted" || proposition == "shifted")
        {
            return "MISREPRESENTED";
        }

        if (new[] { proposition, status, modality, scope, timeScope }.Any(v => v is "partial" or "missing"))
        {
            return "PARTIALLY_PRESERVED";
        }

        return new[] { proposition, status, modality, scope, timeScope }.All(v => v == "preserved")
            ? "PRESERVED"
            : "UNCLEAR";
    }
}

public sealed record DispositionCheckItem(
    string SourceClaimId,
    string Applicability,
    bool Required,
    bool HasArtifactClaim,
    string Verdict);

public sealed record BatchCompletenessIssue(string Code, string Message);
