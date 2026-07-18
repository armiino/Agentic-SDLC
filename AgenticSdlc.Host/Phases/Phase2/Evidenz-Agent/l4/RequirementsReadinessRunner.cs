using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class RequirementsReadinessRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: requirements-readiness <l4-applied-dir|runId|canonical-requirements-baseline.json> [--out <dir>]");
            return 2;
        }

        string? outArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outArg = args[++i];
                continue;
            }
            if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[requirements-readiness] unbekanntes Argument: {args[i]}");
                return 2;
            }
        }

        var viewRepository = new JsonProjectStateViewRepository(repoRoot);
        CanonicalRequirementsView view;
        try
        {
            view = await viewRepository.GetCanonicalRequirementsViewAsync(
                ProjectScope.FromSourcePath(args[1], "requirements-readiness", "current_baseline")).ConfigureAwait(false);
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"[requirements-readiness] {ex.Message}");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[requirements-readiness] L4-Applied-View konnte nicht geladen werden: {ex.Message}");
            return 2;
        }

        var (report, issuePlanningInput) = RequirementsReadinessBuilder.Build(view.Baseline, view.Provenance, view.Quality);
        var outputDir = ResolvePath(repoRoot, outArg ?? view.SourceDirectory);
        Directory.CreateDirectory(outputDir);

        await File.WriteAllTextAsync(Path.Combine(outputDir, "requirements-readiness.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "requirements-readiness.md"), RenderMarkdown(report)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "issue-planning-input.json"), JsonSerializer.Serialize(issuePlanningInput, Json)).ConfigureAwait(false);

        Console.WriteLine($"[requirements-readiness] requirements={report.Summary.Requirements} ready={report.Summary.ReadyForIssuePlanning}");
        Console.WriteLine($"[requirements-readiness] needsDecision={report.Summary.NeedsDecision} needsBreakdown={report.Summary.NeedsBreakdown} deferredOrOptional={report.Summary.DeferredOrOptional} blockedByTraceability={report.Summary.BlockedByTraceability}");
        Console.WriteLine($"[requirements-readiness] issuePlanningInput={issuePlanningInput.Items.Count}");
        Console.WriteLine($"[requirements-readiness] -> {Path.GetRelativePath(repoRoot, outputDir)}");
        return report.Summary.BlockedByTraceability == 0 ? 0 : 1;
    }

    private static string RenderMarkdown(RequirementsReadinessReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Requirements Readiness");
        sb.AppendLine();
        sb.AppendLine($"Baseline: `{report.BaselineId}`");
        sb.AppendLine($"Project: `{report.ProjectId}`");
        sb.AppendLine($"Requirements: `{report.Summary.Requirements}`");
        sb.AppendLine($"Ready for issue planning: `{report.Summary.ReadyForIssuePlanning}`");
        sb.AppendLine($"Needs decision: `{report.Summary.NeedsDecision}`");
        sb.AppendLine($"Needs breakdown: `{report.Summary.NeedsBreakdown}`");
        sb.AppendLine($"Deferred or optional: `{report.Summary.DeferredOrOptional}`");
        sb.AppendLine($"Blocked by traceability: `{report.Summary.BlockedByTraceability}`");
        sb.AppendLine();

        foreach (var group in report.Items.GroupBy(i => i.Readiness).OrderBy(g => ReadinessRank(g.Key)).ThenBy(g => g.Key, StringComparer.Ordinal))
        {
            sb.AppendLine($"## {group.Key}");
            sb.AppendLine();
            foreach (var item in group.OrderBy(i => i.RequirementId, StringComparer.Ordinal))
            {
                sb.AppendLine($"- `{item.RequirementId}` {item.Title}");
                foreach (var reason in item.Reasons)
                    sb.AppendLine($"  - `{reason.Code}` ({reason.Severity}): {reason.Message}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int ReadinessRank(string readiness)
        => readiness switch
        {
            "blocked_by_traceability" => 0,
            "needs_decision" => 1,
            "needs_breakdown" => 2,
            "deferred_or_optional" => 3,
            "ready_for_issue_planning" => 4,
            _ => 5
        };

}
