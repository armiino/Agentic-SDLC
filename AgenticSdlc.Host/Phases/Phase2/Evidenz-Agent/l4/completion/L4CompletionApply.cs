namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class L4CompletionApply
{
    public static L4CompletionApplyResult Apply(
        CanonicalRequirementsBaseline baseline,
        L4ProvenanceMap provenance,
        L4CompletionProposalDocument proposals,
        L4CompletionHumanDecisionsFile decisions)
    {
        var proposalById = proposals.Items.ToDictionary(p => p.ProposalItemId, StringComparer.Ordinal);
        var reqById = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var effectiveProposals = BuildEffectiveProposals(proposals, decisions);
        var requirements = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var traceLinks = baseline.TraceLinks.ToList();
        var addedOpenDecisions = 0;
        var markedNeedsBreakdown = 0;
        var nextCompletionIndex = 1;

        foreach (var proposal in effectiveProposals)
        {
            if (proposal.Operation.Equals("ADD_OPEN_DECISION", StringComparison.OrdinalIgnoreCase)
                || proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase))
            {
                var reqId = NextId(requirements.Keys, proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase)
                    ? "DISK"
                    : "OPEN", ref nextCompletionIndex);
                var sourceItemIds = SourceItemIdsFor(proposal, reqById);
                var metadata = CompletionMetadata(proposal);
                var requirement = new CanonicalRequirement(
                    RequirementId: reqId,
                    Title: proposal.Title,
                    Text: BuildOpenDecisionText(proposal),
                    Status: "open_decision",
                    SourceItemIds: sourceItemIds,
                    OriginSummary: proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase)
                        ? "L4_COMPLETION_DISK"
                        : "L4_COMPLETION_OPEN_DECISION",
                    Version: 1,
                    Metadata: metadata);
                requirements[reqId] = requirement;
                AddCompletionTraceLinks(traceLinks, reqId, proposal, sourceItemIds);
                addedOpenDecisions++;
                continue;
            }

            if (proposal.Operation.Equals("MARK_NEEDS_BREAKDOWN", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var reqId in proposal.SourceRequirementIds)
                {
                    if (!requirements.TryGetValue(reqId, out var requirement)) continue;
                    var metadata = new Dictionary<string, string>(requirement.Metadata, StringComparer.Ordinal)
                    {
                        ["completion.needsBreakdown"] = "true",
                        [$"completion.{proposal.ProposalItemId}.operation"] = proposal.Operation,
                        [$"completion.{proposal.ProposalItemId}.title"] = proposal.Title,
                        [$"completion.{proposal.ProposalItemId}.problem"] = proposal.Problem,
                        [$"completion.{proposal.ProposalItemId}.suggestedResolution"] = proposal.SuggestedResolution,
                        [$"completion.{proposal.ProposalItemId}.whyItMatters"] = proposal.WhyItMatters,
                        [$"completion.{proposal.ProposalItemId}.evidenceState"] = proposal.EvidenceState
                    };
                    requirements[reqId] = requirement with { Metadata = metadata };
                    traceLinks.Add(new CanonicalTraceLink(reqId, "l4_completion_proposal", proposal.ProposalItemId, "marked_needs_breakdown", new Dictionary<string, string>
                    {
                        ["operation"] = proposal.Operation,
                        ["evidenceState"] = proposal.EvidenceState
                    }));
                    markedNeedsBreakdown++;
                }
            }
        }

        var orderedRequirements = requirements.Values
            .OrderBy(r => SortKey(r.RequirementId), StringComparer.Ordinal)
            .ThenBy(r => r.RequirementId, StringComparer.Ordinal)
            .ToList();
        var openDecisions = orderedRequirements
            .Where(r => r.Status.Equals("open_decision", StringComparison.OrdinalIgnoreCase))
            .Select((r, index) => new CanonicalOpenDecision(
                DecisionId: $"OPEN-{index + 1:D3}",
                Text: r.Text,
                SourceRequirementId: r.RequirementId,
                SourceItemIds: r.SourceItemIds,
                Reason: r.Metadata.TryGetValue("completion.operation", out var op)
                    ? $"Aus L4 Completion Proposal ({op})."
                    : "Status open_decision aus L4 Baseline."))
            .ToList();

        var resultBaseline = baseline with
        {
            BaselineId = $"baseline-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            CreatedUtc = DateTime.UtcNow,
            Requirements = orderedRequirements,
            OpenDecisions = openDecisions,
            TraceLinks = traceLinks
        };
        var resultProvenance = BuildProvenance(resultBaseline, provenance, effectiveProposals);
        return new L4CompletionApplyResult(
            Baseline: resultBaseline,
            Provenance: resultProvenance,
            EffectiveProposals: effectiveProposals,
            AddedOpenDecisions: addedOpenDecisions,
            MarkedNeedsBreakdown: markedNeedsBreakdown);
    }

    private static IReadOnlyList<L4CompletionProposalItem> BuildEffectiveProposals(
        L4CompletionProposalDocument proposals,
        L4CompletionHumanDecisionsFile decisions)
    {
        var byProposal = proposals.Items.ToDictionary(p => p.ProposalItemId, StringComparer.Ordinal);
        var effective = new List<L4CompletionProposalItem>();
        foreach (var decision in decisions.Decisions)
        {
            if (!byProposal.TryGetValue(decision.ProposalItemId, out var original)) continue;
            switch ((decision.Decision ?? "").Trim().ToLowerInvariant())
            {
                case "accept":
                    effective.Add(original);
                    break;
                case "edit":
                    if (string.IsNullOrWhiteSpace(decision.EditedProposalJson)) break;
                    var edited = System.Text.Json.JsonSerializer.Deserialize<L4CompletionProposalItem>(decision.EditedProposalJson, ProjectState.ProjectStateJson.Options);
                    if (edited is not null) effective.Add(edited);
                    break;
            }
        }
        return effective;
    }

    private static IReadOnlyList<string> SourceItemIdsFor(
        L4CompletionProposalItem proposal,
        IReadOnlyDictionary<string, CanonicalRequirement> reqById)
        => proposal.SourceRequirementIds
            .SelectMany(reqId => reqById.TryGetValue(reqId, out var req) ? req.SourceItemIds : [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    private static Dictionary<string, string> CompletionMetadata(L4CompletionProposalItem proposal)
        => new(StringComparer.Ordinal)
        {
            ["completion.proposalItemId"] = proposal.ProposalItemId,
            ["completion.operation"] = proposal.Operation,
            ["completion.category"] = proposal.Category,
            ["completion.evidenceState"] = proposal.EvidenceState,
            ["completion.requiresHumanDecision"] = proposal.RequiresHumanDecision.ToString(),
            ["completion.sourceRequirementIds"] = string.Join(",", proposal.SourceRequirementIds),
            ["completion.sourceFindingIds"] = string.Join(",", proposal.SourceFindingIds),
            ["completion.problem"] = proposal.Problem,
            ["completion.suggestedResolution"] = proposal.SuggestedResolution,
            ["completion.whyItMatters"] = proposal.WhyItMatters
        };

    private static string BuildOpenDecisionText(L4CompletionProposalItem proposal)
        => $"{proposal.Problem}\n\nVorgeschlagene Klaerung:\n{proposal.SuggestedResolution}\n\nWarum wichtig:\n{proposal.WhyItMatters}";

    private static void AddCompletionTraceLinks(
        List<CanonicalTraceLink> traceLinks,
        string requirementId,
        L4CompletionProposalItem proposal,
        IReadOnlyList<string> sourceItemIds)
    {
        traceLinks.Add(new CanonicalTraceLink(requirementId, "l4_completion_proposal", proposal.ProposalItemId, "proposed_by", new Dictionary<string, string>
        {
            ["operation"] = proposal.Operation,
            ["evidenceState"] = proposal.EvidenceState
        }));
        foreach (var findingId in proposal.SourceFindingIds)
            traceLinks.Add(new CanonicalTraceLink(requirementId, "l4_adequacy_finding", findingId, "addresses", new Dictionary<string, string>()));
        foreach (var sourceReqId in proposal.SourceRequirementIds)
            traceLinks.Add(new CanonicalTraceLink(requirementId, "canonical_requirement", sourceReqId, "derived_from_requirement", new Dictionary<string, string>()));
        foreach (var sourceItemId in sourceItemIds)
            traceLinks.Add(new CanonicalTraceLink(requirementId, "project_item", sourceItemId, "derived_from", new Dictionary<string, string>()));
    }

    private static L4ProvenanceMap BuildProvenance(
        CanonicalRequirementsBaseline baseline,
        L4ProvenanceMap sourceProvenance,
        IReadOnlyList<L4CompletionProposalItem> effectiveProposals)
    {
        var sourceByReq = sourceProvenance.Requirements
            .ToDictionary(p => p.RequirementId, StringComparer.Ordinal);
        var proposalForGeneratedReq = baseline.Requirements
            .Where(r => r.Metadata.TryGetValue("completion.proposalItemId", out _))
            .ToDictionary(
                r => r.RequirementId,
                r => effectiveProposals.First(p => p.ProposalItemId == r.Metadata["completion.proposalItemId"]),
                StringComparer.Ordinal);

        var records = new List<L4RequirementProvenance>();
        foreach (var req in baseline.Requirements)
        {
            if (sourceByReq.TryGetValue(req.RequirementId, out var existing))
            {
                records.Add(existing with { Status = req.Status, Title = req.Title });
                continue;
            }

            var proposal = proposalForGeneratedReq[req.RequirementId];
            var sourceProjectItems = proposal.SourceRequirementIds
                .SelectMany(id => sourceByReq.TryGetValue(id, out var prov) ? prov.SourceProjectItems : [])
                .GroupBy(i => i.ItemId, StringComparer.Ordinal)
                .Select(g => g.First())
                .ToList();
            records.Add(new L4RequirementProvenance(
                RequirementId: req.RequirementId,
                Status: req.Status,
                Title: req.Title,
                L4OperationId: proposal.ProposalItemId,
                L4Operation: proposal.Operation,
                SourceProjectItems: sourceProjectItems,
                LedgerClaimIds: sourceProjectItems.SelectMany(i => i.SourceClaimIds).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                L3CandidateIds: sourceProjectItems.Select(i => i.SourceCandidateId).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                HumanDecisionIds: sourceProjectItems.Select(i => i.SourceDecisionId).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id!).Append($"l4-completion:{proposal.ProposalItemId}").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                ReplacesProjectItemIds: [],
                LinkedClusters: []));
        }

        return new L4ProvenanceMap(
            BaselineId: baseline.BaselineId,
            ProjectId: baseline.ProjectId,
            CreatedUtc: DateTime.UtcNow,
            Requirements: records);
    }

    private static string NextId(IEnumerable<string> existingIds, string prefix, ref int next)
    {
        var existing = existingIds.ToHashSet(StringComparer.Ordinal);
        string id;
        do id = $"CAN-{prefix}-{next++:D3}";
        while (existing.Contains(id));
        return id;
    }

    private static string SortKey(string itemId)
    {
        var prefix = new string(itemId.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(itemId.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return $"{prefix}{(int.TryParse(digits, out var n) ? n : 0):D6}:{itemId}";
    }
}

public sealed record L4CompletionApplyResult(
    CanonicalRequirementsBaseline Baseline,
    L4ProvenanceMap Provenance,
    IReadOnlyList<L4CompletionProposalItem> EffectiveProposals,
    int AddedOpenDecisions,
    int MarkedNeedsBreakdown);
