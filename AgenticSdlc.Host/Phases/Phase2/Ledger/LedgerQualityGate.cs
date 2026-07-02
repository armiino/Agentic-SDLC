using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>Ein einzelner Gate-Verstoß (Schweregrad + Code + betroffene ids).</summary>
public sealed record GateViolation(
    [property: JsonPropertyName("severity")] string Severity,   // error | warning
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("ids")] IReadOnlyList<string> Ids);

public sealed record GateResult(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("errorCount")] int ErrorCount,
    [property: JsonPropertyName("warningCount")] int WarningCount,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks,
    [property: JsonPropertyName("violations")] IReadOnlyList<GateViolation> Violations);

/// <summary>
/// L4: <b>deterministisches</b> Quality-Gate über den gebauten Ledger (KEIN LLM). Prüft Pipeline-Integrität
/// (Schema/Enum + Transformations-Invarianten), NICHT fachliche Qualität.
/// </summary>
/// <remarks>
/// Kernprinzip (smallVersion.md §LedgerQualityGate): Pipeline-Vollständigkeit ist deterministisch beweisbar
/// (auch wenn open-world-Transkript-Vollständigkeit es nicht ist). Das Gate erzeugt KEINE neue Semantik.
/// Trennung error (hart, lässt den Run scheitern) vs warning (gemeldet, nicht fatal).
/// </remarks>
public static class LedgerQualityGate
{
    private static readonly string[] ValidRelations = ["same_proposition", "refines", "temporal_sequence", "elaborates", "standalone"];
    private static readonly string[] ValidVerdicts = ["grounded", "partial", "overstated", "unsupported"];
    private static readonly string[] ValidClaimStatus = ["approved", "review_required", "contradicted", "draft"];
    private static readonly string[] ValidKinds =
    [
        "decision", "requirement", "constraint", "risk", "open_requirement", "open_question",
        "scope", "compliance_constraint", "process_constraint", "non_functional_requirement", "meta"
    ];
    private static readonly string[] ValidTimeScopes = ["mvp", "later_possible", "mvp_or_later_unclear"];
    private static readonly string[] ValidRiskLevels = ["high", "medium", "low"];
    // #3 ratifiziert (ledger-taxonomy.md): status = Entscheidungsstand, modality = Verbindlichkeit.
    private static readonly string[] ValidStatus = ["decided", "open", "rejected", "uncertain", "required"];
    private static readonly string[] ValidModality = ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"];
    // Geschlossene Facetten, für die ein Repair-suggested nur offizielle Werte enthalten darf.
    private static readonly Dictionary<string, string[]> SuggestableFacetValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = ValidStatus,
        ["modality"] = ValidModality,
        ["timescope"] = ["mvp", "later_possible", "mvp_or_later_unclear"]
        // scope = Freitext (kein Enum); kind/riskLevel/disposition sind keine FacetValidator-Facetten.
    };
    private static readonly string[] RequiredArtifacts = ["requirements", "architecture", "risks", "open-questions"];
    private static readonly string[] ValidApplicability = ["required", "optional", "context", "not_applicable"];
    private static readonly string[] ValidRepresentationModes =
    [
        "requirement", "constraint", "open_decision", "assumption", "risk_reference",
        "question", "open_question", "consciously_omitted"
    ];

    public static GateResult Evaluate(
        IReadOnlyList<SemanticLedgerEntry> candidates,
        IReadOnlyList<SemanticLedgerEntry> canonical,
        IReadOnlyList<ValidatedLedgerEntry> validated)
    {
        var v = new List<GateViolation>();

        // --- I0 (ERROR): Ledger-IDs muessen eindeutig sein. ---
        AddDuplicateIdViolation(v, "DUPLICATE_CANDIDATE_ID", "Candidate-IDs sind nicht eindeutig.", candidates.Select(c => c.Id));
        AddDuplicateIdViolation(v, "DUPLICATE_CANONICAL_ID", "Kanonische IDs sind nicht eindeutig.", canonical.Select(c => c.Id));
        AddDuplicateIdViolation(v, "DUPLICATE_VALIDATED_ID", "Validated-IDs sind nicht eindeutig.", validated.Select(x => x.Entry.Id));

        // --- I1 (ERROR): kein Candidate verschwindet still — jeder Candidate in genau einem canonical.candidateIds. ---
        var candidateIds = candidates.Select(c => c.Id).Where(s => !string.IsNullOrWhiteSpace(s)).ToHashSet(StringComparer.Ordinal);
        var referencePairs = canonical
            .SelectMany(c => (c.CandidateIds ?? []).Select(id => new { CandidateId = id, CanonicalId = c.Id }))
            .ToList();
        var referenced = referencePairs.Select(x => x.CandidateId).ToHashSet(StringComparer.Ordinal);
        var orphanCandidates = candidateIds.Where(id => !referenced.Contains(id)).ToList();
        if (orphanCandidates.Count > 0)
            v.Add(new("error", "CANDIDATE_SILENTLY_DROPPED",
                "Candidate-IDs, die in keinem kanonischen Eintrag (candidateIds) referenziert sind.", orphanCandidates));

        var unknownCandidateRefs = referenced.Where(id => !candidateIds.Contains(id)).ToList();
        if (unknownCandidateRefs.Count > 0)
            v.Add(new("error", "UNKNOWN_CANDIDATE_REFERENCE",
                "candidateIds referenzieren IDs, die im Candidate-Ledger nicht existieren.", unknownCandidateRefs));

        var duplicateCandidateRefs = referencePairs
            .GroupBy(x => x.CandidateId, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateCandidateRefs.Count > 0)
            v.Add(new("error", "CANDIDATE_REFERENCED_MULTIPLE_TIMES",
                "Candidate-IDs werden in mehreren kanonischen Einträgen referenziert.", duplicateCandidateRefs));

        // --- I2 (ERROR): jeder kanonische Eintrag hat einen Cluster-Trace (candidateIds nicht leer). ---
        var noTrace = canonical.Where(c => (c.CandidateIds?.Count ?? 0) == 0).Select(c => c.Id).ToList();
        if (noTrace.Count > 0)
            v.Add(new("error", "CANONICAL_WITHOUT_CLUSTER_TRACE", "Kanonische Einträge ohne candidateIds.", noTrace));

        // --- I2b (ERROR): assumedRelation gesetzt, gueltig und konsistent mit der Cluster-Groesse. ---
        var missingRelation = canonical.Where(c => string.IsNullOrWhiteSpace(c.AssumedRelation)).Select(c => c.Id).ToList();
        if (missingRelation.Count > 0)
            v.Add(new("error", "MISSING_ASSUMED_RELATION", "Kanonische Einträge ohne assumedRelation.", missingRelation));

        var badRelation = canonical.Where(c => !string.IsNullOrWhiteSpace(c.AssumedRelation)
            && !ValidRelations.Contains(c.AssumedRelation)).Select(c => c.Id).ToList();
        if (badRelation.Count > 0)
            v.Add(new("error", "INVALID_ASSUMED_RELATION", $"assumedRelation ausserhalb {{{string.Join("|", ValidRelations)}}}.", badRelation));

        var badStandalone = canonical
            .Where(c => c.AssumedRelation == "standalone" && (c.CandidateIds?.Count ?? 0) != 1)
            .Select(c => c.Id)
            .ToList();
        if (badStandalone.Count > 0)
            v.Add(new("error", "STANDALONE_RELATION_CARDINALITY",
                "assumedRelation=standalone verlangt genau eine candidateId.", badStandalone));

        var badMergeRelation = canonical
            .Where(c => !string.IsNullOrWhiteSpace(c.AssumedRelation) && c.AssumedRelation != "standalone" && (c.CandidateIds?.Count ?? 0) < 2)
            .Select(c => c.Id)
            .ToList();
        if (badMergeRelation.Count > 0)
            v.Add(new("error", "MERGE_RELATION_CARDINALITY",
                "Merge-Relationen verlangen mindestens zwei candidateIds.", badMergeRelation));

        // --- I3 (ERROR): jeder kanonische Eintrag hat >=1 Evidence (keine Provenienz -> kein finaler Claim). ---
        var noEvidence = canonical.Where(c => c.Evidence.Count == 0).Select(c => c.Id).ToList();
        if (noEvidence.Count > 0)
            v.Add(new("error", "CANONICAL_WITHOUT_EVIDENCE", "Kanonische Einträge ohne Evidence.", noEvidence));

        // --- I4 (ERROR): validated deckt canonical exakt (id-Menge identisch). ---
        var canonIds = canonical.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var valIds = validated.Select(x => x.Entry.Id).ToHashSet(StringComparer.Ordinal);
        var notValidated = canonIds.Where(id => !valIds.Contains(id)).ToList();
        if (notValidated.Count > 0)
            v.Add(new("error", "CANONICAL_NOT_VALIDATED", "Kanonische Einträge ohne Facet-Validation-Verdict.", notValidated));
        var extraValidated = valIds.Where(id => !canonIds.Contains(id)).ToList();
        if (extraValidated.Count > 0)
            v.Add(new("error", "VALIDATION_WITHOUT_CANONICAL",
                "Facet-Validation enthält IDs, die im kanonischen Ledger nicht existieren.", extraValidated));

        // --- I4b (ERROR): Validated Entry und Validation-Objekt muessen dieselbe id tragen. ---
        var validationIdMismatch = validated
            .Where(x => !string.Equals(x.Entry.Id, x.Validation.Id, StringComparison.Ordinal))
            .Select(x => x.Entry.Id)
            .ToList();
        if (validationIdMismatch.Count > 0)
            v.Add(new("error", "VALIDATION_ID_MISMATCH",
                "Validated entry id und validation.id stimmen nicht überein.", validationIdMismatch));

        // --- I5 (ERROR): Enum-Gültigkeit verdict + claimStatus. ---
        var badVerdict = validated.Where(x => !ValidVerdicts.Contains(x.Validation.Verdict)).Select(x => x.Entry.Id).ToList();
        if (badVerdict.Count > 0)
            v.Add(new("error", "INVALID_VERDICT", $"verdict ausserhalb {{{string.Join("|", ValidVerdicts)}}}.", badVerdict));
        var badStatus = validated.Where(x => !ValidClaimStatus.Contains(x.ClaimStatus)).Select(x => x.Entry.Id).ToList();
        if (badStatus.Count > 0)
            v.Add(new("error", "INVALID_CLAIM_STATUS", $"claimStatus ausserhalb {{{string.Join("|", ValidClaimStatus)}}}.", badStatus));

        // --- I5b (ERROR): kontrollierte Ledger-Taxonomie. ---
        AddEnumViolation(v, "INVALID_KIND", $"kind ausserhalb {{{string.Join("|", ValidKinds)}}}.", canonical, e => e.Kind, ValidKinds);
        AddEnumViolation(v, "INVALID_TIME_SCOPE", $"timeScope ausserhalb {{{string.Join("|", ValidTimeScopes)}}}.", canonical, e => e.TimeScope, ValidTimeScopes, allowNull: true);
        AddEnumViolation(v, "INVALID_RISK_LEVEL", $"riskLevel ausserhalb {{{string.Join("|", ValidRiskLevels)}}}.", canonical, e => e.RiskLevel, ValidRiskLevels);
        // #3: status/modality jetzt Gate-erzwungen.
        AddEnumViolation(v, "INVALID_STATUS", $"status ausserhalb {{{string.Join("|", ValidStatus)}}}.", canonical, e => e.Status, ValidStatus);
        AddEnumViolation(v, "INVALID_MODALITY", $"modality ausserhalb {{{string.Join("|", ValidModality)}}}.", canonical, e => e.Modality, ValidModality);

        // --- I5c (ERROR): required ist NUR extern-bindend -> Rasierklingen-Regel: modality muss must/must_not sein. ---
        var inconsistentRequired = canonical
            .Where(e => string.Equals(e.Status, "required", StringComparison.OrdinalIgnoreCase)
                && e.Modality is not null && e.Modality is not ("must" or "must_not"))
            .Select(e => e.Id)
            .ToList();
        if (inconsistentRequired.Count > 0)
            v.Add(new("error", "INCONSISTENT_REQUIRED_MODALITY",
                "status=required (externe Pflicht) verlangt modality=must|must_not.", inconsistentRequired));

        // --- I5d (ERROR): Validator-Repair taxonomie-konform (#4): suggested nur offizielle Werte je Facette. ---
        var badSuggested = validated
            .Where(x => x.Validation.FacetIssues.Any(i =>
                i.Suggested is not null
                && SuggestableFacetValues.TryGetValue(i.Facet, out var allowed)
                && !allowed.Contains(i.Suggested)))
            .Select(x => x.Entry.Id)
            .ToList();
        if (badSuggested.Count > 0)
            v.Add(new("error", "INVALID_SUGGESTED_VALUE",
                "FacetIssue.suggested liegt für eine geschlossene Facette ausserhalb der offiziellen Taxonomie.", badSuggested));

        var missingDispositionArtifacts = canonical
            .Where(e => RequiredArtifacts.Any(a => !e.Disposition.ContainsKey(a)))
            .Select(e => e.Id)
            .ToList();
        if (missingDispositionArtifacts.Count > 0)
            v.Add(new("error", "MISSING_DISPOSITION_ARTIFACT",
                $"Disposition muss alle Artefakte enthalten: {string.Join(",", RequiredArtifacts)}.", missingDispositionArtifacts));

        var invalidDispositionApplicability = canonical
            .Where(e => e.Disposition.Values.Any(d => !ValidApplicability.Contains(d.Applicability)))
            .Select(e => e.Id)
            .ToList();
        if (invalidDispositionApplicability.Count > 0)
            v.Add(new("error", "INVALID_DISPOSITION_APPLICABILITY",
                $"disposition.*.applicability ausserhalb {{{string.Join("|", ValidApplicability)}}}.", invalidDispositionApplicability));

        var invalidDispositionMode = canonical
            .Where(e => e.Disposition.Values.Any(d => !ValidRepresentationModes.Contains(d.RepresentationMode)))
            .Select(e => e.Id)
            .ToList();
        if (invalidDispositionMode.Count > 0)
            v.Add(new("error", "INVALID_DISPOSITION_REPRESENTATION_MODE",
                $"disposition.*.representationMode ausserhalb {{{string.Join("|", ValidRepresentationModes)}}}.", invalidDispositionMode));

        // --- I6 (WARNING): jedes FacetIssue mit suggested==observed ist folgenlos (kein echter Repair). ---
        var emptyRepair = validated.Where(x => x.Validation.FacetIssues.Any(i =>
            i.Suggested is not null && string.Equals(i.Suggested, i.Observed, StringComparison.OrdinalIgnoreCase)))
            .Select(x => x.Entry.Id).ToList();
        if (emptyRepair.Count > 0)
            v.Add(new("warning", "REPAIR_EQUALS_OBSERVED", "FacetIssue mit suggested == observed (kein wirksamer Repair).", emptyRepair));

        var errors = v.Count(x => x.Severity == "error");
        var warnings = v.Count(x => x.Severity == "warning");
        var checks = new Dictionary<string, object>
        {
            ["candidates"] = candidates.Count,
            ["canonical"] = canonical.Count,
            ["validated"] = validated.Count,
            ["candidatesReferenced"] = candidateIds.Count(id => referenced.Contains(id)),
            ["candidatesOrphan"] = orphanCandidates.Count,
            ["unknownCandidateReferences"] = unknownCandidateRefs.Count,
            ["duplicateCandidateReferences"] = duplicateCandidateRefs.Count,
            ["canonicalWithEvidence"] = canonical.Count(c => c.Evidence.Count > 0),
            ["approved"] = validated.Count(x => x.ClaimStatus == "approved"),
            ["reviewRequired"] = validated.Count(x => x.ClaimStatus == "review_required")
        };

        return new GateResult(errors == 0, errors, warnings, checks, v);
    }

    private static void AddDuplicateIdViolation(List<GateViolation> violations, string code, string message, IEnumerable<string?> ids)
    {
        var duplicate = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id!, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicate.Count > 0)
            violations.Add(new("error", code, message, duplicate));
    }

    private static void AddEnumViolation(
        List<GateViolation> violations,
        string code,
        string message,
        IReadOnlyList<SemanticLedgerEntry> entries,
        Func<SemanticLedgerEntry, string?> selector,
        IReadOnlyCollection<string> validValues,
        bool allowNull = false)
    {
        var badIds = entries
            .Where(e =>
            {
                var value = selector(e);
                if (string.IsNullOrWhiteSpace(value)) return !allowNull;
                return !validValues.Contains(value);
            })
            .Select(e => e.Id)
            .ToList();

        if (badIds.Count > 0)
            violations.Add(new("error", code, message, badIds));
    }
}
