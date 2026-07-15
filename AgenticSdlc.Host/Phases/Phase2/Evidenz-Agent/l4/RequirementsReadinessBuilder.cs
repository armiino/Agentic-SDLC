namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class RequirementsReadinessBuilder
{
    private static readonly string[] DeferredMarkers =
    [
        "optional", "optionale", "perspektivisch", "nicht mvp", "nicht priorisiert", "geringe priorität"
    ];

    public static (RequirementsReadinessReport Report, IssuePlanningInput IssuePlanningInput) Build(
        CanonicalRequirementsBaseline baseline,
        L4ProvenanceMap provenanceMap,
        L4QualityReport qualityReport)
    {
        var qualityByRequirement = qualityReport.Findings
            .Where(f => !string.IsNullOrWhiteSpace(f.RequirementId))
            .GroupBy(f => f.RequirementId!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var provenanceByRequirement = provenanceMap.Requirements
            .GroupBy(p => p.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var items = baseline.Requirements
            .OrderBy(r => r.RequirementId, StringComparer.Ordinal)
            .Select(r => BuildItem(r, qualityByRequirement.GetValueOrDefault(r.RequirementId) ?? [], provenanceByRequirement.GetValueOrDefault(r.RequirementId)))
            .ToList();

        var ready = items.Where(i => i.IssuePlanningAllowed).ToList();
        var report = new RequirementsReadinessReport(
            SchemaVersion: RequirementsReadinessReport.CurrentSchemaVersion,
            BaselineId: baseline.BaselineId,
            ProjectId: baseline.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            Summary: new RequirementsReadinessSummary(
                Requirements: items.Count,
                ReadyForIssuePlanning: ready.Count,
                NeedsDecision: Count(items, "needs_decision"),
                NeedsBreakdown: Count(items, "needs_breakdown"),
                DeferredOrOptional: Count(items, "deferred_or_optional"),
                BlockedByTraceability: Count(items, "blocked_by_traceability")),
            Items: items);

        var input = new IssuePlanningInput(
            SchemaVersion: IssuePlanningInput.CurrentSchemaVersion,
            BaselineId: baseline.BaselineId,
            ProjectId: baseline.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            Items: ready
                .Select(item =>
                {
                    var requirement = baseline.Requirements.First(r => r.RequirementId == item.RequirementId);
                    return new IssuePlanningInputItem(
                        RequirementId: requirement.RequirementId,
                        Title: requirement.Title,
                        Text: requirement.Text,
                        SourceItemIds: requirement.SourceItemIds,
                        OriginSummary: requirement.OriginSummary,
                        Provenance: provenanceByRequirement.GetValueOrDefault(requirement.RequirementId));
                })
                .ToList());

        return (report, input);
    }

    private static RequirementReadinessItem BuildItem(
        CanonicalRequirement requirement,
        IReadOnlyList<L4QualityFinding> qualityFindings,
        L4RequirementProvenance? provenance)
    {
        var reasons = new List<RequirementReadinessReason>();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["originSummary"] = requirement.OriginSummary
        };

        if (requirement.SourceItemIds.Count == 0)
            reasons.Add(Reason("missing_source_items", "blocker", "Requirement hat keine sourceItemIds."));
        if (provenance is null)
            reasons.Add(Reason("missing_provenance", "blocker", "Requirement fehlt in der L4-Provenienzkarte."));
        if (provenance is not null && provenance.SourceProjectItems.Count == 0)
            reasons.Add(Reason("missing_project_item_provenance", "blocker", "Provenienz enthaelt keine ProjectState-Items."));

        foreach (var finding in qualityFindings.Where(f => f.Severity == "error"))
            reasons.Add(Reason(finding.Code, "blocker", finding.Message));

        if (string.Equals(requirement.Status, "open_decision", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(Reason("open_decision_status", "blocker", "Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden."));
        }

        if (ContainsAny(requirement.Text, DeferredMarkers))
        {
            reasons.Add(Reason("deferred_or_optional_marker", "hold", "Requirement ist als optional, spaeter oder perspektivisch markiert und sollte nicht automatisch in Sprint-Issues laufen."));
        }

        foreach (var finding in qualityFindings.Where(f => f.Classification == "needs_decision"))
        {
            var severity = finding.Code == "decision_marker_in_active_requirement" && IsOptionalOnlyFinding(finding)
                ? "hold"
                : "blocker";
            reasons.Add(Reason(finding.Code, severity, finding.Message));
        }

        foreach (var finding in qualityFindings.Where(f => f.Classification == "needs_breakdown"))
            reasons.Add(Reason(finding.Code, "hold", finding.Message));

        var readiness = Classify(reasons);
        return new RequirementReadinessItem(
            RequirementId: requirement.RequirementId,
            Title: requirement.Title,
            Status: requirement.Status,
            Readiness: readiness,
            IssuePlanningAllowed: readiness == "ready_for_issue_planning",
            SourceItemIds: requirement.SourceItemIds,
            L4OperationId: provenance?.L4OperationId,
            L4Operation: provenance?.L4Operation,
            Reasons: reasons,
            QualityFindingCodes: qualityFindings.Select(f => f.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Metadata: metadata);
    }

    private static string Classify(IReadOnlyList<RequirementReadinessReason> reasons)
    {
        if (reasons.Any(r => r.Code.Contains("provenance", StringComparison.OrdinalIgnoreCase)
                             || r.Code.Contains("source_items", StringComparison.OrdinalIgnoreCase)
                             || r.Severity == "blocker" && r.Code.StartsWith("missing_", StringComparison.Ordinal)))
            return "blocked_by_traceability";
        if (reasons.Any(r => r.Code == "open_decision_status"))
            return "needs_decision";
        if (reasons.Any(r => r.Code == "deferred_or_optional_marker"))
            return "deferred_or_optional";
        if (reasons.Any(r => r.Code == "decision_marker_in_active_requirement"))
            return "needs_decision";
        if (reasons.Any(r => r.Code == "operationalization_risk"))
            return "needs_breakdown";
        if (reasons.Any(r => r.Severity == "blocker"))
            return "blocked_by_traceability";
        return "ready_for_issue_planning";
    }

    private static int Count(IReadOnlyList<RequirementReadinessItem> items, string readiness)
        => items.Count(i => string.Equals(i.Readiness, readiness, StringComparison.Ordinal));

    private static bool ContainsAny(string text, IReadOnlyList<string> markers)
        => markers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));

    private static bool IsOptionalOnlyFinding(L4QualityFinding finding)
        => finding.Evidence.TryGetValue("markers", out var markers)
           && markers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .All(m => m.Equals("optional", StringComparison.OrdinalIgnoreCase));

    private static RequirementReadinessReason Reason(string code, string severity, string message)
        => new(code, severity, message);
}
