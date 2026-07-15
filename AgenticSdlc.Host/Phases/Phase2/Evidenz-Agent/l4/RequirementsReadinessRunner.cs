using System.Text;
using System.Text.Json;

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

        var resolved = ResolveInput(repoRoot, args[1]);
        if (resolved is null)
        {
            Console.Error.WriteLine($"[requirements-readiness] L4-Applied-Artefakt '{args[1]}' nicht gefunden.");
            return 2;
        }
        if (!File.Exists(resolved.ProvenancePath))
        {
            Console.Error.WriteLine("[requirements-readiness] provenance-map.json fehlt. Erst l4-apply mit Provenienz ausfuehren.");
            return 2;
        }
        if (!File.Exists(resolved.QualityReportPath))
        {
            Console.Error.WriteLine("[requirements-readiness] quality-report.json fehlt. Erst l4-quality ausfuehren.");
            return 2;
        }

        var baseline = JsonSerializer.Deserialize<CanonicalRequirementsBaseline>(
            await File.ReadAllTextAsync(resolved.BaselinePath).ConfigureAwait(false), Json)
            ?? throw new InvalidOperationException($"Baseline konnte nicht gelesen werden: {resolved.BaselinePath}");
        var provenance = JsonSerializer.Deserialize<L4ProvenanceMap>(
            await File.ReadAllTextAsync(resolved.ProvenancePath).ConfigureAwait(false), Json)
            ?? throw new InvalidOperationException($"ProvenanceMap konnte nicht gelesen werden: {resolved.ProvenancePath}");
        var quality = JsonSerializer.Deserialize<L4QualityReport>(
            await File.ReadAllTextAsync(resolved.QualityReportPath).ConfigureAwait(false), Json)
            ?? throw new InvalidOperationException($"QualityReport konnte nicht gelesen werden: {resolved.QualityReportPath}");

        var (report, issuePlanningInput) = RequirementsReadinessBuilder.Build(baseline, provenance, quality);
        var outputDir = ResolvePath(repoRoot, outArg ?? resolved.OutputDir);
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

    private static ResolvedRequirementsReadinessInput? ResolveInput(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "canonical-requirements-baseline.json", StringComparison.OrdinalIgnoreCase))
        {
            var dir = Path.GetDirectoryName(full) ?? repoRoot;
            return ResolveDirectory(dir);
        }

        if (Directory.Exists(full))
        {
            var direct = ResolveDirectory(full);
            if (direct is not null) return direct;
            var applied = Path.Combine(full, "consolidation", "applied");
            if (Directory.Exists(applied)) return ResolveDirectory(applied);
        }

        var l4Root = Path.Combine(repoRoot, "runs", "l4");
        if (!Directory.Exists(l4Root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(l4Root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var applied = Path.Combine(runDir, "consolidation", "applied");
            if (Directory.Exists(applied))
            {
                var resolved = ResolveDirectory(applied);
                if (resolved is not null) return resolved;
            }
        }

        return null;
    }

    private static ResolvedRequirementsReadinessInput? ResolveDirectory(string dir)
    {
        var baselinePath = Path.Combine(dir, "canonical-requirements-baseline.json");
        if (!File.Exists(baselinePath)) return null;
        return new ResolvedRequirementsReadinessInput(
            BaselinePath: baselinePath,
            ProvenancePath: Path.Combine(dir, "provenance-map.json"),
            QualityReportPath: Path.Combine(dir, "quality-report.json"),
            OutputDir: dir);
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

    private sealed record ResolvedRequirementsReadinessInput(
        string BaselinePath,
        string ProvenancePath,
        string QualityReportPath,
        string OutputDir);
}
