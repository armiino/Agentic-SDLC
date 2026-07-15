using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static partial class L4QualityGate
{
    private static readonly string[] DecisionMarkers =
    [
        "zu entscheiden", "ist zu entscheiden", "zu klären", "soll geklärt", "offen",
        "festzulegen", "verbindlich festzulegen", "rechtsgrundlage", "mvp", "noch nicht",
        "optional", "gegebenenfalls"
    ];

    private static readonly string[] BreakdownMarkers =
    [
        "angemessen", "geeignet", "robust", "schnell", "einfach", "komfortabel",
        "barrierearm", "barrierefrei", "sicher", "vollständig", "flexibel",
        "möglichst", "sollte"
    ];

    public static L4QualityReport Check(CanonicalRequirementsBaseline baseline, L4ProvenanceMap? provenanceMap)
    {
        var findings = new List<L4QualityFinding>();
        var reqIds = new HashSet<string>(StringComparer.Ordinal);
        var traceReqIds = baseline.TraceLinks.Select(t => t.RequirementId).ToHashSet(StringComparer.Ordinal);
        var provenanceByReq = (provenanceMap?.Requirements ?? [])
            .GroupBy(p => p.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var requirement in baseline.Requirements)
        {
            if (!reqIds.Add(requirement.RequirementId))
            {
                findings.Add(Finding(
                    "duplicate_requirement_id",
                    "error",
                    "invalid",
                    requirement,
                    $"Requirement-ID kommt mehrfach vor: {requirement.RequirementId}",
                    new Dictionary<string, string>()));
            }

            if (string.IsNullOrWhiteSpace(requirement.Title))
                findings.Add(Finding("missing_title", "error", "invalid", requirement, "Requirement hat keinen Titel.", new Dictionary<string, string>()));

            if (string.IsNullOrWhiteSpace(requirement.Text))
                findings.Add(Finding("missing_text", "error", "invalid", requirement, "Requirement hat keinen Text.", new Dictionary<string, string>()));

            if (requirement.SourceItemIds.Count == 0)
                findings.Add(Finding("missing_source_items", "error", "traceability_gap", requirement, "Requirement hat keine sourceItemIds.", new Dictionary<string, string>()));

            if (!traceReqIds.Contains(requirement.RequirementId))
                findings.Add(Finding("missing_trace_links", "error", "traceability_gap", requirement, "Requirement hat keine TraceLinks.", new Dictionary<string, string>()));

            if (!provenanceByReq.TryGetValue(requirement.RequirementId, out var provenanceRecords) || provenanceRecords.Count == 0)
            {
                findings.Add(Finding("missing_provenance", "error", "traceability_gap", requirement, "Requirement fehlt in provenance-map.json.", new Dictionary<string, string>()));
            }
            else
            {
                foreach (var provenance in provenanceRecords)
                {
                    if (string.IsNullOrWhiteSpace(provenance.L4OperationId))
                        findings.Add(Finding("missing_l4_operation", "error", "traceability_gap", requirement, "Provenienz enthält keine L4-Operation.", new Dictionary<string, string>()));

                    if (provenance.SourceProjectItems.Count == 0)
                        findings.Add(Finding("missing_project_item_provenance", "error", "traceability_gap", requirement, "Provenienz enthält keine ProjectState-Items.", new Dictionary<string, string>()));
                }
            }

            var classification = ClassifyRequirement(requirement);
            if (requirement.Status == "active" && ContainsAny(requirement.Text, DecisionMarkers))
            {
                findings.Add(Finding(
                    "decision_marker_in_active_requirement",
                    "warning",
                    "needs_decision",
                    requirement,
                    "Requirement enthält Entscheidungsmarker; prüfen, ob es als offene Entscheidung statt aktive Anforderung geführt werden sollte.",
                    new Dictionary<string, string> { ["markers"] = MatchingMarkers(requirement.Text, DecisionMarkers) }));
            }
            else if (classification == "needs_breakdown")
            {
                findings.Add(Finding(
                    "operationalization_risk",
                    "warning",
                    classification,
                    requirement,
                    "Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.",
                    new Dictionary<string, string> { ["markers"] = MatchingMarkers(requirement.Text, BreakdownMarkers) }));
            }
        }

        AddDuplicateRisks(baseline, findings);
        AddOpenDecisionConsistencyFindings(baseline, findings);

        var errors = findings.Count(f => f.Severity == "error");
        var warnings = findings.Count(f => f.Severity == "warning");
        var infos = findings.Count(f => f.Severity == "info");
        var classifications = baseline.Requirements.Select(ClassifyRequirement).ToList();
        var duplicateRiskReqIds = findings
            .Where(f => f.Classification == "duplicate_risk" && !string.IsNullOrWhiteSpace(f.RequirementId))
            .Select(f => f.RequirementId!)
            .Distinct(StringComparer.Ordinal)
            .Count();

        var checks = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["hasProvenanceMap"] = provenanceMap is not null,
            ["baselineSchemaVersion"] = baseline.SchemaVersion,
            ["uniqueRequirementIds"] = reqIds.Count == baseline.Requirements.Count,
            ["requirementsWithTraceLinks"] = baseline.Requirements.Count(r => traceReqIds.Contains(r.RequirementId)),
            ["requirementsWithProvenance"] = baseline.Requirements.Count(r => provenanceByReq.ContainsKey(r.RequirementId)),
            ["activeRequirements"] = baseline.Requirements.Count(r => r.Status == "active"),
            ["openDecisionRequirements"] = baseline.Requirements.Count(r => r.Status == "open_decision")
        };

        return new L4QualityReport(
            Pass: errors == 0,
            Decision: errors == 0 ? "pass" : "fail",
            BaselineId: baseline.BaselineId,
            ProjectId: baseline.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            Summary: new L4QualitySummary(
                Requirements: baseline.Requirements.Count,
                OpenDecisions: baseline.OpenDecisions.Count,
                TraceLinks: baseline.TraceLinks.Count,
                Errors: errors,
                Warnings: warnings,
                Infos: infos,
                Ready: classifications.Count(c => c == "ready"),
                NeedsBreakdown: classifications.Count(c => c == "needs_breakdown"),
                NeedsDecision: classifications.Count(c => c == "needs_decision"),
                DuplicateRisk: duplicateRiskReqIds),
            Findings: findings
                .OrderBy(f => SeverityRank(f.Severity))
                .ThenBy(f => f.RequirementId, StringComparer.Ordinal)
                .ThenBy(f => f.Code, StringComparer.Ordinal)
                .ToList(),
            Checks: checks);
    }

    private static void AddDuplicateRisks(CanonicalRequirementsBaseline baseline, List<L4QualityFinding> findings)
    {
        var byNormalizedText = baseline.Requirements
            .GroupBy(r => NormalizeForDuplicateCheck(r.Text), StringComparer.Ordinal)
            .Where(g => g.Key.Length > 40 && g.Count() > 1);

        foreach (var group in byNormalizedText)
        {
            var ids = group.Select(r => r.RequirementId).Order(StringComparer.Ordinal).ToList();
            foreach (var requirement in group)
            {
                findings.Add(Finding(
                    "duplicate_text_risk",
                    "warning",
                    "duplicate_risk",
                    requirement,
                    "Mehrere Requirements haben nahezu identischen Text.",
                    new Dictionary<string, string> { ["relatedRequirementIds"] = string.Join(", ", ids) }));
            }
        }
    }

    private static void AddOpenDecisionConsistencyFindings(CanonicalRequirementsBaseline baseline, List<L4QualityFinding> findings)
    {
        var reqOpenDecisionIds = baseline.Requirements
            .Where(r => r.Status == "open_decision")
            .Select(r => r.RequirementId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var decision in baseline.OpenDecisions)
        {
            if (string.IsNullOrWhiteSpace(decision.Text))
            {
                findings.Add(new L4QualityFinding(
                    "missing_open_decision_text",
                    "error",
                    "invalid",
                    decision.SourceRequirementId,
                    decision.SourceItemIds,
                    $"Open Decision {decision.DecisionId} hat keinen Text.",
                    new Dictionary<string, string>()));
            }

            if (!string.IsNullOrWhiteSpace(decision.SourceRequirementId) && !reqOpenDecisionIds.Contains(decision.SourceRequirementId))
            {
                findings.Add(new L4QualityFinding(
                    "open_decision_source_status_mismatch",
                    "warning",
                    "needs_decision",
                    decision.SourceRequirementId,
                    decision.SourceItemIds,
                    $"Open Decision {decision.DecisionId} verweist auf ein Requirement, das nicht den Status open_decision hat.",
                    new Dictionary<string, string>()));
            }
        }
    }

    private static string ClassifyRequirement(CanonicalRequirement requirement)
    {
        if (requirement.Status == "open_decision") return "needs_decision";
        if (ContainsAny(requirement.Text, DecisionMarkers)) return "needs_decision";
        if (ContainsAny(requirement.Text, BreakdownMarkers)) return "needs_breakdown";
        return "ready";
    }

    private static L4QualityFinding Finding(
        string code,
        string severity,
        string classification,
        CanonicalRequirement requirement,
        string message,
        IReadOnlyDictionary<string, string> evidence)
        => new(code, severity, classification, requirement.RequirementId, requirement.SourceItemIds, message, evidence);

    private static bool ContainsAny(string text, IReadOnlyList<string> markers)
        => markers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));

    private static string MatchingMarkers(string text, IReadOnlyList<string> markers)
        => string.Join(", ", markers.Where(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal));

    private static string NormalizeForDuplicateCheck(string text)
        => NonWordRegex().Replace(text.ToLowerInvariant(), " ").Trim();

    private static int SeverityRank(string severity)
        => severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    [GeneratedRegex("[^\\p{L}\\p{Nd}]+", RegexOptions.Compiled)]
    private static partial Regex NonWordRegex();
}
