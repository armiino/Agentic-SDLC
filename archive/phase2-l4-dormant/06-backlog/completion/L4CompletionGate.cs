namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4CompletionGate
{
    private static readonly HashSet<string> AllowedAdequacy = new(StringComparer.OrdinalIgnoreCase)
    {
        "substantive", "thin", "missing", "uncertain", "conflicting", "blocked_by_decision", "not_applicable"
    };

    private static readonly HashSet<string> AllowedOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADD_OPEN_DECISION", "ADD_DISK_POINT", "MARK_NEEDS_BREAKDOWN", "REVISE_REQUIREMENT",
        "SPLIT_REQUIREMENT", "LINK_RELATED_ITEMS", "DEFER", "NO_CHANGE"
    };

    private static readonly HashSet<string> AllowedEvidenceStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "stated", "derived", "weakly_inferred", "not_stated"
    };

    public static L4CompletionGateReport Check(
        L4CompletionInput input,
        L4AdequacyReport adequacy,
        L4CompletionProposalDocument proposals)
    {
        var errors = new List<L4CompletionGateIssue>();
        var warnings = new List<L4CompletionGateIssue>();
        var requirementIds = input.Baseline.Requirements.Select(r => r.RequirementId).ToHashSet(StringComparer.Ordinal);
        var findingIds = adequacy.Findings.Select(f => f.FindingId).ToHashSet(StringComparer.Ordinal);

        if (!string.Equals(adequacy.ProjectId, input.Baseline.ProjectId, StringComparison.Ordinal))
            errors.Add(Issue("L4C-001", "error", "AdequacyReport projectId passt nicht zur Baseline.", null, []));
        if (!string.Equals(adequacy.BaselineId, input.Baseline.BaselineId, StringComparison.Ordinal))
            errors.Add(Issue("L4C-002", "error", "AdequacyReport baselineId passt nicht zur Baseline.", null, []));
        if (!string.Equals(proposals.ProjectId, input.Baseline.ProjectId, StringComparison.Ordinal))
            errors.Add(Issue("L4C-003", "error", "CompletionProposal projectId passt nicht zur Baseline.", null, []));
        if (!string.Equals(proposals.BaselineId, input.Baseline.BaselineId, StringComparison.Ordinal))
            errors.Add(Issue("L4C-004", "error", "CompletionProposal baselineId passt nicht zur Baseline.", null, []));
        if (!string.Equals(proposals.SourceAdequacyReportId, adequacy.ReportId, StringComparison.Ordinal))
            errors.Add(Issue("L4C-005", "error", "CompletionProposal referenziert nicht den genutzten AdequacyReport.", null, []));

        foreach (var finding in adequacy.Findings)
        {
            if (string.IsNullOrWhiteSpace(finding.FindingId))
                errors.Add(Issue("L4C-010", "error", "AdequacyFinding ohne findingId.", null, []));
            if (!AllowedAdequacy.Contains(finding.Adequacy))
                errors.Add(Issue("L4C-011", "error", $"Ungueltige adequacy-Klasse: {finding.Adequacy}", finding.FindingId, []));
            foreach (var reqId in finding.RelatedRequirementIds.Where(r => !requirementIds.Contains(r)))
                errors.Add(Issue("L4C-012", "error", $"Finding referenziert unbekanntes Requirement: {reqId}", finding.FindingId, [reqId]));
            if (string.Equals(finding.Adequacy, "thin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(finding.Adequacy, "missing", StringComparison.OrdinalIgnoreCase)
                || string.Equals(finding.Adequacy, "blocked_by_decision", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(finding.RecommendedAction))
                    warnings.Add(Issue("L4C-013", "warning", "Finding mit Luecke ohne recommendedAction.", finding.FindingId, finding.RelatedRequirementIds));
            }
        }

        var proposalIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in proposals.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProposalItemId))
                errors.Add(Issue("L4C-020", "error", "CompletionProposalItem ohne proposalItemId.", null, []));
            else if (!proposalIds.Add(item.ProposalItemId))
                errors.Add(Issue("L4C-021", "error", $"Doppelte proposalItemId: {item.ProposalItemId}", item.ProposalItemId, []));
            if (!AllowedOperations.Contains(item.Operation))
                errors.Add(Issue("L4C-022", "error", $"Ungueltige Completion-Operation: {item.Operation}", item.ProposalItemId, []));
            if (!AllowedEvidenceStates.Contains(item.EvidenceState))
                errors.Add(Issue("L4C-023", "error", $"Ungueltiger evidenceState: {item.EvidenceState}", item.ProposalItemId, []));
            foreach (var reqId in item.SourceRequirementIds.Where(r => !requirementIds.Contains(r)))
                errors.Add(Issue("L4C-024", "error", $"Proposal referenziert unbekanntes Requirement: {reqId}", item.ProposalItemId, [reqId]));
            foreach (var findingId in item.SourceFindingIds.Where(f => !findingIds.Contains(f)))
                errors.Add(Issue("L4C-025", "error", $"Proposal referenziert unbekanntes Finding: {findingId}", item.ProposalItemId, [findingId]));

            if (string.Equals(item.Operation, "ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase))
            {
                if (!item.RequiresHumanDecision)
                    errors.Add(Issue("L4C-026", "error", "DISK-Punkte muessen requiresHumanDecision=true tragen.", item.ProposalItemId, item.SourceRequirementIds));
                if (!string.Equals(item.EvidenceState, "not_stated", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.EvidenceState, "weakly_inferred", StringComparison.OrdinalIgnoreCase))
                    errors.Add(Issue("L4C-027", "error", "DISK-Punkte muessen evidenceState=not_stated oder weakly_inferred tragen.", item.ProposalItemId, item.SourceRequirementIds));
            }

            if ((string.Equals(item.Operation, "ADD_OPEN_DECISION", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(item.Operation, "ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrWhiteSpace(item.WhyItMatters))
            {
                warnings.Add(Issue("L4C-028", "warning", "Open-World Proposal ohne whyItMatters.", item.ProposalItemId, item.SourceRequirementIds));
            }
        }

        var blockingFindings = adequacy.Findings
            .Where(f => !string.Equals(f.Adequacy, "substantive", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(f.Adequacy, "not_applicable", StringComparison.OrdinalIgnoreCase))
            .Select(f => f.FindingId)
            .ToHashSet(StringComparer.Ordinal);
        var coveredFindings = proposals.Items.SelectMany(i => i.SourceFindingIds).ToHashSet(StringComparer.Ordinal);
        var uncoveredFindings = blockingFindings.Except(coveredFindings, StringComparer.Ordinal).ToArray();
        foreach (var findingId in uncoveredFindings)
            warnings.Add(Issue("L4C-029", "warning", "Nicht-substantielles Finding hat keinen Completion-Vorschlag.", findingId, [findingId]));

        return new L4CompletionGateReport(
            Pass: errors.Count == 0,
            Decision: errors.Count == 0 ? "pass" : "fail",
            Errors: errors,
            Warnings: warnings,
            Checks: new Dictionary<string, object>
            {
                ["requirements"] = input.Baseline.Requirements.Count,
                ["findings"] = adequacy.Findings.Count,
                ["proposalItems"] = proposals.Items.Count,
                ["blockingFindings"] = blockingFindings.Count,
                ["uncoveredBlockingFindings"] = uncoveredFindings.Length
            });
    }

    private static L4CompletionGateIssue Issue(string code, string severity, string message, string? itemId, IReadOnlyList<string> sourceIds)
        => new(code, severity, message, itemId, sourceIds);
}
