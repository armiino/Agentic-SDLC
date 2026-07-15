using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class L4QualityRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-quality <l4-applied-dir|runId|canonical-requirements-baseline.json> [--out <dir>]");
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
                Console.Error.WriteLine($"[l4-quality] unbekanntes Argument: {args[i]}");
                return 2;
            }
        }

        var resolved = ResolveInput(repoRoot, args[1]);
        if (resolved is null)
        {
            Console.Error.WriteLine($"[l4-quality] L4-Applied-Artefakt '{args[1]}' nicht gefunden.");
            return 2;
        }

        var baseline = JsonSerializer.Deserialize<CanonicalRequirementsBaseline>(
            await File.ReadAllTextAsync(resolved.BaselinePath).ConfigureAwait(false), Json)
            ?? throw new InvalidOperationException($"Baseline konnte nicht gelesen werden: {resolved.BaselinePath}");

        L4ProvenanceMap? provenance = null;
        if (resolved.ProvenancePath is not null && File.Exists(resolved.ProvenancePath))
        {
            provenance = JsonSerializer.Deserialize<L4ProvenanceMap>(
                await File.ReadAllTextAsync(resolved.ProvenancePath).ConfigureAwait(false), Json)
                ?? throw new InvalidOperationException($"ProvenanceMap konnte nicht gelesen werden: {resolved.ProvenancePath}");
        }

        var report = L4QualityGate.Check(baseline, provenance);
        var outputDir = ResolvePath(repoRoot, outArg ?? resolved.OutputDir);
        Directory.CreateDirectory(outputDir);

        var reportPath = Path.Combine(outputDir, "quality-report.json");
        var markdownPath = Path.Combine(outputDir, "quality-report.md");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, RenderMarkdown(report)).ConfigureAwait(false);

        Console.WriteLine($"[l4-quality] decision={report.Decision} errors={report.Summary.Errors} warnings={report.Summary.Warnings}");
        Console.WriteLine($"[l4-quality] requirements={report.Summary.Requirements} ready={report.Summary.Ready} needsBreakdown={report.Summary.NeedsBreakdown} needsDecision={report.Summary.NeedsDecision} duplicateRisk={report.Summary.DuplicateRisk}");
        Console.WriteLine($"[l4-quality] -> {Path.GetRelativePath(repoRoot, outputDir)}");
        return report.Pass ? 0 : 1;
    }

    private static string RenderMarkdown(L4QualityReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# L4 Quality Report");
        sb.AppendLine();
        sb.AppendLine($"Baseline: `{report.BaselineId}`");
        sb.AppendLine($"Project: `{report.ProjectId}`");
        sb.AppendLine($"Decision: `{report.Decision}`");
        sb.AppendLine($"Errors: `{report.Summary.Errors}`");
        sb.AppendLine($"Warnings: `{report.Summary.Warnings}`");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- Requirements: `{report.Summary.Requirements}`");
        sb.AppendLine($"- Open decisions: `{report.Summary.OpenDecisions}`");
        sb.AppendLine($"- Trace links: `{report.Summary.TraceLinks}`");
        sb.AppendLine($"- Ready: `{report.Summary.Ready}`");
        sb.AppendLine($"- Needs breakdown: `{report.Summary.NeedsBreakdown}`");
        sb.AppendLine($"- Needs decision: `{report.Summary.NeedsDecision}`");
        sb.AppendLine($"- Duplicate risk: `{report.Summary.DuplicateRisk}`");
        sb.AppendLine();

        foreach (var group in report.Findings.GroupBy(f => f.Severity).OrderBy(g => SeverityRank(g.Key)))
        {
            sb.AppendLine($"## {group.Key}");
            sb.AppendLine();
            foreach (var finding in group)
            {
                var req = string.IsNullOrWhiteSpace(finding.RequirementId) ? "" : $" `{finding.RequirementId}`";
                var sources = finding.SourceItemIds.Count == 0 ? "" : $" sources={string.Join(", ", finding.SourceItemIds.Select(id => $"`{id}`"))}";
                sb.AppendLine($"- `{finding.Code}`{req}: {finding.Message}{sources}");
            }
            sb.AppendLine();
        }

        if (report.Findings.Count == 0)
        {
            sb.AppendLine("Keine Findings.");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static ResolvedL4QualityInput? ResolveInput(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "canonical-requirements-baseline.json", StringComparison.OrdinalIgnoreCase))
        {
            var dir = Path.GetDirectoryName(full) ?? repoRoot;
            return new ResolvedL4QualityInput(full, OptionalPath(Path.Combine(dir, "provenance-map.json")), dir);
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

    private static ResolvedL4QualityInput? ResolveDirectory(string dir)
    {
        var baselinePath = Path.Combine(dir, "canonical-requirements-baseline.json");
        if (!File.Exists(baselinePath)) return null;
        return new ResolvedL4QualityInput(baselinePath, OptionalPath(Path.Combine(dir, "provenance-map.json")), dir);
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string? OptionalPath(string path)
        => File.Exists(path) ? path : null;

    private static int SeverityRank(string severity)
        => severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private sealed record ResolvedL4QualityInput(string BaselinePath, string? ProvenancePath, string OutputDir);
}
