using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class L4CompletionApplyRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var completionDir = ResolveCompletionDir(repoRoot, args[1]);
        if (completionDir is null)
        {
            Console.Error.WriteLine($"[l4-completion-apply] Completion-Lauf '{args[1]}' nicht gefunden.");
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
                Console.Error.WriteLine($"[l4-completion-apply] unbekanntes Argument: {args[i]}");
                return 2;
            }
        }

        var proposalsPath = Path.Combine(completionDir, "l4-completion-proposals.json");
        var decisionsPath = Path.Combine(completionDir, "human-decisions.json");
        if (!File.Exists(proposalsPath)) { Console.Error.WriteLine("[l4-completion-apply] l4-completion-proposals.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[l4-completion-apply] human-decisions.json fehlt. Erst l4-completion-review ausfuehren."); return 2; }

        var proposals = await LoadAsync<L4CompletionProposalDocument>(proposalsPath).ConfigureAwait(false);
        var decisions = await LoadAsync<L4CompletionHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);
        var sourceAppliedDir = ResolveSourceAppliedDir(repoRoot, proposals.SourceRequirementsDocumentPath);
        if (sourceAppliedDir is null)
        {
            Console.Error.WriteLine("[l4-completion-apply] Source L4 applied dir konnte nicht aufgeloest werden.");
            return 2;
        }

        var baselinePath = Path.Combine(sourceAppliedDir, "canonical-requirements-baseline.json");
        var provenancePath = Path.Combine(sourceAppliedDir, "provenance-map.json");
        if (!File.Exists(baselinePath)) { Console.Error.WriteLine("[l4-completion-apply] canonical-requirements-baseline.json fehlt."); return 2; }
        if (!File.Exists(provenancePath)) { Console.Error.WriteLine("[l4-completion-apply] provenance-map.json fehlt."); return 2; }

        var baseline = await LoadAsync<CanonicalRequirementsBaseline>(baselinePath).ConfigureAwait(false);
        var provenance = await LoadAsync<L4ProvenanceMap>(provenancePath).ConfigureAwait(false);
        var outputDir = ResolvePath(repoRoot, outArg ?? Path.Combine(completionDir, "applied"));
        Directory.CreateDirectory(outputDir);

        var result = L4CompletionApply.Apply(baseline, provenance, proposals, decisions);
        var quality = L4QualityGate.Check(result.Baseline, result.Provenance);
        if (!quality.Pass)
        {
            await File.WriteAllTextAsync(Path.Combine(outputDir, "quality-report.json"), JsonSerializer.Serialize(quality, Json)).ConfigureAwait(false);
            Console.Error.WriteLine($"[l4-completion-apply] quality gate failed: errors={quality.Summary.Errors} warnings={quality.Summary.Warnings}");
            return 1;
        }

        var outputs = new List<string>
        {
            "canonical-requirements-baseline.json",
            "provenance-map.json",
            "requirements-baseline.md",
            "traceability-matrix.md",
            "open-decisions.json",
            "accepted-completion-proposals.json",
            "quality-report.json",
            "quality-report.md",
            "completion-apply-report.json"
        };
        await File.WriteAllTextAsync(Path.Combine(outputDir, "canonical-requirements-baseline.json"), JsonSerializer.Serialize(result.Baseline, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "provenance-map.json"), JsonSerializer.Serialize(result.Provenance, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "requirements-baseline.md"), L4BaselineRenderer.RenderMarkdown(result.Baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "traceability-matrix.md"), L4BaselineRenderer.RenderTraceabilityMarkdown(result.Baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "open-decisions.json"), JsonSerializer.Serialize(result.Baseline.OpenDecisions, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "accepted-completion-proposals.json"), JsonSerializer.Serialize(result.EffectiveProposals, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "quality-report.json"), JsonSerializer.Serialize(quality, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "quality-report.md"), L4QualityRunner.RenderMarkdown(quality)).ConfigureAwait(false);

        var byDecision = decisions.Decisions.GroupBy(d => d.Decision.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var report = new L4CompletionApplyReport(
            RunId: ParentRunId(completionDir),
            SourceCompletionDir: Path.GetRelativePath(repoRoot, completionDir),
            SourceBaselinePath: Path.GetRelativePath(repoRoot, baselinePath),
            SourceProposalsPath: Path.GetRelativePath(repoRoot, proposalsPath),
            SourceDecisionsPath: Path.GetRelativePath(repoRoot, decisionsPath),
            Accepted: byDecision.GetValueOrDefault("accept"),
            Edited: byDecision.GetValueOrDefault("edit"),
            Rejected: byDecision.GetValueOrDefault("reject"),
            Revise: byDecision.GetValueOrDefault("revise"),
            AddedOpenDecisions: result.AddedOpenDecisions,
            MarkedNeedsBreakdown: result.MarkedNeedsBreakdown,
            Requirements: result.Baseline.Requirements.Count,
            OpenDecisions: result.Baseline.OpenDecisions.Count,
            TraceLinks: result.Baseline.TraceLinks.Count,
            Outputs: outputs.Select(o => Path.GetRelativePath(repoRoot, Path.Combine(outputDir, o))).ToList());
        await File.WriteAllTextAsync(Path.Combine(outputDir, "completion-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-completion-apply] quality=pass requirements={report.Requirements} openDecisions={report.OpenDecisions} traceLinks={report.TraceLinks}");
        Console.WriteLine($"[l4-completion-apply] decisions accept={report.Accepted} edit={report.Edited} reject={report.Rejected} revise={report.Revise}");
        Console.WriteLine($"[l4-completion-apply] addedOpenDecisions={report.AddedOpenDecisions} markedNeedsBreakdown={report.MarkedNeedsBreakdown}");
        Console.WriteLine($"[l4-completion-apply] -> {Path.GetRelativePath(repoRoot, outputDir)}");
        return 0;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveCompletionDir(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "l4-completion-proposals.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "completion", "l4-completion-proposals.json"))) return Path.Combine(full, "completion");

        var root = Path.Combine(repoRoot, "runs", "l4-completion");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var completion = Path.Combine(dir, "completion");
            if (File.Exists(Path.Combine(completion, "l4-completion-proposals.json"))) return completion;
            if (File.Exists(Path.Combine(dir, "l4-completion-proposals.json"))) return dir;
        }
        return null;
    }

    private static string? ResolveSourceAppliedDir(string repoRoot, string sourceRequirementsDocumentPath)
    {
        var docPath = ResolvePath(repoRoot, sourceRequirementsDocumentPath);
        var dir = Path.GetDirectoryName(docPath);
        return dir is not null && File.Exists(Path.Combine(dir, "canonical-requirements-baseline.json")) ? dir : null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string completionDir)
    {
        var name = Path.GetFileName(completionDir);
        return string.Equals(name, "completion", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(completionDir) ?? completionDir)
            : name;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-completion-apply <l4-completion-dir|runId> [--out <dir>]");
    }
}
