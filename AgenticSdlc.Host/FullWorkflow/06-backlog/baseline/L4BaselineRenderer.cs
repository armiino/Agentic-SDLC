using System.Text;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4BaselineRenderer
{
    public static string RenderMarkdown(CanonicalRequirementsBaseline baseline)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Canonical Requirements Baseline");
        sb.AppendLine();
        sb.AppendLine($"Baseline: `{baseline.BaselineId}`");
        sb.AppendLine($"Project: `{baseline.ProjectId}`");
        sb.AppendLine($"Created UTC: `{baseline.CreatedUtc:O}`");
        sb.AppendLine($"Requirements: `{baseline.Requirements.Count}`");
        sb.AppendLine($"Open decisions: `{baseline.OpenDecisions.Count}`");
        sb.AppendLine();

        foreach (var group in baseline.Requirements.GroupBy(r => r.Status).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            sb.AppendLine($"## {group.Key}");
            sb.AppendLine();
            foreach (var req in group)
            {
                sb.AppendLine($"### {req.RequirementId} - {req.Title}");
                sb.AppendLine();
                sb.AppendLine(req.Text);
                sb.AppendLine();
                sb.AppendLine($"Source items: {string.Join(", ", req.SourceItemIds.Select(id => $"`{id}`"))}");
                sb.AppendLine($"Origin: `{req.OriginSummary}`");
                sb.AppendLine();
            }
        }

        if (baseline.OpenDecisions.Count > 0)
        {
            sb.AppendLine("## Open Decisions");
            sb.AppendLine();
            foreach (var od in baseline.OpenDecisions)
            {
                sb.AppendLine($"- `{od.DecisionId}` ({od.SourceRequirementId ?? "no requirement"}): {od.Text}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string RenderTraceabilityMarkdown(CanonicalRequirementsBaseline baseline)
    {
        var byReq = baseline.TraceLinks.GroupBy(t => t.RequirementId).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var sb = new StringBuilder();
        sb.AppendLine("# Requirements Traceability Matrix");
        sb.AppendLine();
        sb.AppendLine("| Requirement | Project Items | Ledger Claims | Human Decisions | L3 Candidates |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var req in baseline.Requirements)
        {
            var links = byReq.GetValueOrDefault(req.RequirementId) ?? [];
            sb.Append("| `").Append(req.RequirementId).Append("` ");
            sb.Append("| ").Append(Format(links, "project_item")).Append(' ');
            sb.Append("| ").Append(Format(links, "ledger_claim")).Append(' ');
            sb.Append("| ").Append(Format(links, "human_decision")).Append(' ');
            sb.Append("| ").Append(Format(links, "l3_candidate")).AppendLine(" |");
        }
        return sb.ToString();
    }

    private static string Format(IReadOnlyList<CanonicalTraceLink> links, string type)
    {
        var ids = links.Where(l => string.Equals(l.SourceType, type, StringComparison.Ordinal))
            .Select(l => l.SourceId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(id => $"`{id}`")
            .ToList();
        return ids.Count == 0 ? "" : string.Join(", ", ids);
    }
}
