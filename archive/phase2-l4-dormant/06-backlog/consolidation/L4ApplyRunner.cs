using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4ApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-apply <l4-consolidation-dir|runId> [--out <dir>]");
            return 2;
        }

        var consolidationDir = ResolveConsolidationDir(repoRoot, args[1]);
        if (consolidationDir is null)
        {
            Console.Error.WriteLine($"[l4-apply] L4-Consolidation-Lauf '{args[1]}' nicht gefunden.");
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
                Console.Error.WriteLine($"[l4-apply] unbekanntes Argument: {args[i]}");
                return 2;
            }
        }

        var planPath = Path.Combine(consolidationDir, "consolidation-plan.json");
        var decisionsPath = Path.Combine(consolidationDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[l4-apply] consolidation-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[l4-apply] human-decisions.json fehlt. Erst l4-review ausführen."); return 2; }

        var plan = JsonSerializer.Deserialize<ConsolidationPlan>(await File.ReadAllTextAsync(planPath).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"ConsolidationPlan konnte nicht gelesen werden: {planPath}");
        var decisions = JsonSerializer.Deserialize<L4HumanDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Json)
                        ?? throw new InvalidOperationException($"Human decisions konnten nicht gelesen werden: {decisionsPath}");
        var statePath = ResolvePath(repoRoot, plan.SourceProjectStatePath);
        var state = JsonSerializer.Deserialize<ProjectStateDocument>(await File.ReadAllTextAsync(statePath).ConfigureAwait(false), Json)
                    ?? throw new InvalidOperationException($"ProjectState konnte nicht gelesen werden: {statePath}");

        var outputDir = ResolvePath(repoRoot, outArg ?? Path.Combine(consolidationDir, "applied"));
        Directory.CreateDirectory(outputDir);

        var (effectivePlan, fallbackKeeps) = L4Apply.BuildEffectivePlan(state, plan, decisions);
        var gate = L4ConsolidationGate.Check(state, effectivePlan);
        var gatePath = Path.Combine(outputDir, "effective-gate-report.json");
        await File.WriteAllTextAsync(gatePath, JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "effective-consolidation-plan.json"), JsonSerializer.Serialize(effectivePlan, Json)).ConfigureAwait(false);

        if (!gate.Pass)
        {
            Console.Error.WriteLine($"[l4-apply] effective plan gate failed: errors={gate.Errors.Count} warnings={gate.Warnings.Count}");
            Console.Error.WriteLine($"[l4-apply] -> {Path.GetRelativePath(repoRoot, gatePath)}");
            return 1;
        }

        var baseline = L4Apply.BuildBaseline(state, effectivePlan, decisions);
        var provenanceMap = L4ProvenanceBuilder.Build(baseline, state, effectivePlan);
        var outputs = new List<string>
        {
            "canonical-requirements-baseline.json",
            "provenance-map.json",
            "requirements-baseline.md",
            "traceability-matrix.md",
            "open-decisions.json",
            "apply-report.json",
            "effective-consolidation-plan.json",
            "effective-gate-report.json"
        };

        await File.WriteAllTextAsync(Path.Combine(outputDir, "canonical-requirements-baseline.json"), JsonSerializer.Serialize(baseline, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "provenance-map.json"), JsonSerializer.Serialize(provenanceMap, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "requirements-baseline.md"), L4BaselineRenderer.RenderMarkdown(baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "traceability-matrix.md"), L4BaselineRenderer.RenderTraceabilityMarkdown(baseline)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "open-decisions.json"), JsonSerializer.Serialize(baseline.OpenDecisions, Json)).ConfigureAwait(false);

        var byDecision = decisions.Decisions.GroupBy(d => d.Decision.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var report = new L4ApplyReport(
            RunId: ParentRunId(consolidationDir),
            SourcePlanPath: Path.GetRelativePath(repoRoot, planPath),
            SourceDecisionsPath: Path.GetRelativePath(repoRoot, decisionsPath),
            EffectiveOperations: effectivePlan.Operations.Count,
            Requirements: baseline.Requirements.Count,
            OpenDecisions: baseline.OpenDecisions.Count,
            TraceLinks: baseline.TraceLinks.Count,
            Accepted: byDecision.GetValueOrDefault("accept"),
            Edited: byDecision.GetValueOrDefault("edit"),
            Rejected: byDecision.GetValueOrDefault("reject"),
            Revise: byDecision.GetValueOrDefault("revise"),
            AutoFallbackKeeps: fallbackKeeps.Count,
            GateReportPath: Path.GetRelativePath(repoRoot, gatePath),
            Outputs: outputs.Select(o => Path.GetRelativePath(repoRoot, Path.Combine(outputDir, o))).ToList());
        await File.WriteAllTextAsync(Path.Combine(outputDir, "apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-apply] gate=pass effectiveOperations={effectivePlan.Operations.Count}");
        Console.WriteLine($"[l4-apply] requirements={baseline.Requirements.Count} openDecisions={baseline.OpenDecisions.Count} traceLinks={baseline.TraceLinks.Count}");
        Console.WriteLine($"[l4-apply] decisions accept={report.Accepted} edit={report.Edited} reject={report.Rejected} revise={report.Revise} fallbackKeeps={report.AutoFallbackKeeps}");
        Console.WriteLine($"[l4-apply] -> {Path.GetRelativePath(repoRoot, outputDir)}");
        return 0;
    }

    private static string? ResolveConsolidationDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "consolidation-plan.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "consolidation", "consolidation-plan.json"))) return Path.Combine(full, "consolidation");

        var l4Root = Path.Combine(repoRoot, "runs", "l4");
        if (!Directory.Exists(l4Root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(l4Root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var cons = Path.Combine(dir, "consolidation");
            if (File.Exists(Path.Combine(cons, "consolidation-plan.json"))) return cons;
            if (File.Exists(Path.Combine(dir, "consolidation-plan.json"))) return dir;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ParentRunId(string consolidationDir)
    {
        var name = Path.GetFileName(consolidationDir);
        return string.Equals(name, "consolidation", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(consolidationDir) ?? consolidationDir)
            : name;
    }
}
