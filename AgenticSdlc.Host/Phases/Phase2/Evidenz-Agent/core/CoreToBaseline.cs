using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Inc 1b: deterministische Projektion Core -> L4-Applied-Triplet (canonical-requirements-baseline +
// provenance-map + quality-report), das die bestehende re-clarify-Kette UNVERAENDERT konsumiert.
// Damit stammen Cluster/PBIs/Issues aus dem lebenden Core statt aus einer eingefrorenen L4-Baseline.
// Kein LLM. Aktive Requirements = alle ausser superseded/retired; Requirement-Identitaet = Core-ItemId.
public static class CoreToBaseline
{
    private static readonly HashSet<string> Inactive = new(StringComparer.OrdinalIgnoreCase) { "superseded", "retired" };

    public static (CanonicalRequirementsBaseline Baseline, L4ProvenanceMap Provenance, L4QualityReport Quality) Project(
        ProjectStateDocument core, string coreSourcePath)
    {
        var stamp = DateTime.UtcNow;
        var baselineId = $"core-baseline-{stamp:yyyyMMdd_HHmmss}";

        var reqItems = core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase) && !Inactive.Contains(i.Status))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();

        var requirements = reqItems.Select(i => new CanonicalRequirement(
            RequirementId: i.ItemId,
            Title: Truncate(i.Text, 80),
            Text: i.Text,
            Status: "active",
            SourceItemIds: [i.ItemId],
            OriginSummary: i.Origin,
            Version: i.Version,
            Metadata: i.Metadata)).ToList();

        var openDecisions = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(i.Status, "open_decision", StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new CanonicalOpenDecision(
                DecisionId: i.ItemId,
                Text: i.Text,
                SourceRequirementId: i.Metadata.GetValueOrDefault("targetEntityId"),
                SourceItemIds: [],
                Reason: "ingestion contradiction"))
            .ToList();

        var baseline = new CanonicalRequirementsBaseline(
            BaselineId: baselineId,
            ProjectId: core.ProjectId,
            SchemaVersion: CanonicalRequirementsBaseline.CurrentSchemaVersion,
            CreatedUtc: stamp,
            SourceProjectStatePath: coreSourcePath,
            Requirements: requirements,
            OpenDecisions: openDecisions,
            TraceLinks: []);

        var provenance = new L4ProvenanceMap(
            BaselineId: baselineId,
            ProjectId: core.ProjectId,
            CreatedUtc: stamp,
            Requirements: reqItems.Select(i => new L4RequirementProvenance(
                RequirementId: i.ItemId,
                Status: "active",
                Title: Truncate(i.Text, 80),
                L4OperationId: null,
                L4Operation: "core-ingestion",
                SourceProjectItems: [],
                LedgerClaimIds: i.SourceClaimIds,
                L3CandidateIds: [],
                HumanDecisionIds: [],
                ReplacesProjectItemIds: [],
                LinkedClusters: [])).ToList());

        var quality = new L4QualityReport(
            Pass: true,
            Decision: "accept",
            BaselineId: baselineId,
            ProjectId: core.ProjectId,
            CreatedUtc: stamp,
            Summary: new L4QualitySummary(
                Requirements: requirements.Count,
                OpenDecisions: openDecisions.Count,
                TraceLinks: 0, Errors: 0, Warnings: 0, Infos: 0,
                Ready: requirements.Count, NeedsBreakdown: 0, NeedsDecision: openDecisions.Count, DuplicateRisk: 0),
            Findings: [],
            Checks: new Dictionary<string, object>());

        return (baseline, provenance, quality);
    }

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}
