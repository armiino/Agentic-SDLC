using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class OperationalizationAuditRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var acceptedGithubActionPlanPath = ResolveAcceptedGithubActionPlanPath(repoRoot, args[1]);
        if (acceptedGithubActionPlanPath is null)
        {
            Console.Error.WriteLine($"[operationalization-audit] accepted-github-action-plan nicht gefunden: {args[1]}");
            return 2;
        }

        var outDir = ResolveOutputDir(repoRoot, args, acceptedGithubActionPlanPath);
        Directory.CreateDirectory(outDir);
        OperationalizationAuditDocument audit;
        try
        {
            var clarificationPlanPath = ResolveOptionalClarificationPlanPath(repoRoot, args);
            var input = await LoadInputAsync(repoRoot, acceptedGithubActionPlanPath, clarificationPlanPath).ConfigureAwait(false);
            audit = OperationalizationAuditBuilder.Build(input);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[operationalization-audit] fehlgeschlagen: {ex.Message}");
            return 2;
        }

        await File.WriteAllTextAsync(Path.Combine(outDir, "operationalization-audit.json"), JsonSerializer.Serialize(audit, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "operationalization-audit.md"), OperationalizationAuditBuilder.RenderMarkdown(audit)).ConfigureAwait(false);

        Console.WriteLine($"[operationalization-audit] canonical={audit.Summary.CanonicalRequirements} readiness={audit.Summary.ReadinessItems} issueInput={audit.Summary.IssuePlanningInputItems} issuePlan={audit.Summary.IssuePlanItems} githubActions={audit.Summary.GithubActions} clarificationItems={audit.Summary.ClarificationPlanItems}");
        Console.WriteLine($"[operationalization-audit] coveredIssueInput={audit.Summary.CoveredIssuePlanningInputItems}/{audit.Summary.IssuePlanningInputItems} deliveryCoverage={audit.Summary.RequirementsWithDeliveryCoverage} clarificationCoverage={audit.Summary.RequirementsWithClarificationCoverage} missingCoverage={audit.Summary.RequirementsWithoutOperationalCoverage}");
        Console.WriteLine($"[operationalization-audit] blockedRequirements={audit.Summary.BlockedRequirements} noChangeOnly={audit.Summary.NoChangeRequirements} readyForGithubWrite={audit.Summary.ReadyForGithubWrite}");
        var severities = string.Join(", ", audit.Summary.FindingsBySeverity.Select(kv => kv.Key + "=" + kv.Value));
        Console.WriteLine($"[operationalization-audit] findings={audit.Findings.Count} severities={severities}");
        Console.WriteLine($"[operationalization-audit] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return audit.Summary.ReadyForGithubWrite ? 0 : 1;
    }

    private static async Task<OperationalizationAuditInput> LoadInputAsync(string repoRoot, string acceptedGithubActionPlanPath, string? acceptedClarificationPlanPath)
    {
        var acceptedGithubActionPlan = await LoadAsync<GithubActionPlanDocument>(acceptedGithubActionPlanPath).ConfigureAwait(false);
        var githubAppliedDir = Path.GetDirectoryName(acceptedGithubActionPlanPath) ?? repoRoot;
        var acceptedGithubActionGatePath = Path.Combine(githubAppliedDir, "accepted-github-action-plan-gate-report.json");
        var githubActionGate = File.Exists(acceptedGithubActionGatePath)
            ? await LoadAsync<GithubActionPlanGateReport>(acceptedGithubActionGatePath).ConfigureAwait(false)
            : null;
        var dryRunPath = Path.Combine(githubAppliedDir, "github-write-dry-run", "github-write-dry-run.json");
        if (!File.Exists(dryRunPath))
            throw new FileNotFoundException("github-write-dry-run.json fehlt. Fuehre zuerst github-write dry-run aus.", dryRunPath);
        var dryRun = await LoadAsync<GithubWriteDryRunDocument>(dryRunPath).ConfigureAwait(false);

        var acceptedIssuePlanPath = ResolvePath(repoRoot, acceptedGithubActionPlan.SourceAcceptedIssuePlanPath);
        var acceptedIssuePlan = await LoadAsync<IssuePlanDocument>(acceptedIssuePlanPath).ConfigureAwait(false);
        var issuePlanAppliedDir = Path.GetDirectoryName(acceptedIssuePlanPath) ?? repoRoot;
        var acceptedIssuePlanGatePath = Path.Combine(issuePlanAppliedDir, "accepted-issue-plan-gate-report.json");
        var acceptedIssuePlanGate = File.Exists(acceptedIssuePlanGatePath)
            ? await LoadAsync<IssuePlanGateReport>(acceptedIssuePlanGatePath).ConfigureAwait(false)
            : null;
        var issuePlanningInputPath = ResolvePath(repoRoot, acceptedIssuePlan.SourceIssuePlanningInputPath);
        var issuePlanningInput = await LoadAsync<IssuePlanningInput>(issuePlanningInputPath).ConfigureAwait(false);
        var l4AppliedDir = Path.GetDirectoryName(issuePlanningInputPath) ?? repoRoot;
        var canonicalPath = Path.Combine(l4AppliedDir, "canonical-requirements-baseline.json");
        var readinessPath = Path.Combine(l4AppliedDir, "requirements-readiness.json");
        var canonical = File.Exists(canonicalPath)
            ? await LoadAsync<CanonicalRequirementsBaseline>(canonicalPath).ConfigureAwait(false)
            : null;
        var readiness = File.Exists(readinessPath)
            ? await LoadAsync<RequirementsReadinessReport>(readinessPath).ConfigureAwait(false)
            : null;
        var acceptedClarificationPlan = acceptedClarificationPlanPath is not null && File.Exists(acceptedClarificationPlanPath)
            ? await LoadAsync<ClarificationPlanDocument>(acceptedClarificationPlanPath).ConfigureAwait(false)
            : null;

        return new OperationalizationAuditInput(
            Canonical: canonical,
            Readiness: readiness,
            IssuePlanningInput: issuePlanningInput,
            AcceptedIssuePlan: acceptedIssuePlan,
            AcceptedIssuePlanGate: acceptedIssuePlanGate,
            AcceptedGithubActionPlan: acceptedGithubActionPlan,
            AcceptedGithubActionGate: githubActionGate,
            DryRun: dryRun,
            AcceptedClarificationPlan: acceptedClarificationPlan,
            SourcePaths: new OperationalizationAuditSourcePaths(
                CanonicalBaselinePath: File.Exists(canonicalPath) ? Path.GetRelativePath(repoRoot, canonicalPath) : null,
                ReadinessReportPath: File.Exists(readinessPath) ? Path.GetRelativePath(repoRoot, readinessPath) : null,
                IssuePlanningInputPath: Path.GetRelativePath(repoRoot, issuePlanningInputPath),
                AcceptedIssuePlanPath: Path.GetRelativePath(repoRoot, acceptedIssuePlanPath),
                AcceptedGithubActionPlanPath: Path.GetRelativePath(repoRoot, acceptedGithubActionPlanPath),
                GithubWriteDryRunPath: Path.GetRelativePath(repoRoot, dryRunPath),
                AcceptedClarificationPlanPath: acceptedClarificationPlanPath is not null ? Path.GetRelativePath(repoRoot, acceptedClarificationPlanPath) : null));
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveAcceptedGithubActionPlanPath(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && Path.GetFileName(full).Equals("accepted-github-action-plan.json", StringComparison.OrdinalIgnoreCase))
            return full;
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "accepted-github-action-plan.json");
            if (File.Exists(direct)) return direct;
            var applied = Path.Combine(full, "applied", "accepted-github-action-plan.json");
            if (File.Exists(applied)) return applied;
            var planApplied = Path.Combine(full, "plan", "applied", "accepted-github-action-plan.json");
            if (File.Exists(planApplied)) return planApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "github-reconciliation");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var candidate = Path.Combine(dir, "plan", "applied", "accepted-github-action-plan.json");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static string ResolveOutputDir(string repoRoot, string[] args, string acceptedPlanPath)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].Equals("--out", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) throw new ArgumentException("--out braucht einen Wert.");
            return ResolvePath(repoRoot, args[i + 1]);
        }
        return Path.Combine(Path.GetDirectoryName(acceptedPlanPath) ?? repoRoot, "operationalization-audit");
    }

    private static string? ResolveOptionalClarificationPlanPath(string repoRoot, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].Equals("--clarification-plan", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) throw new ArgumentException("--clarification-plan braucht einen Wert.");
            var path = ResolveAcceptedClarificationPlanPath(repoRoot, args[i + 1]);
            if (path is null) throw new FileNotFoundException("accepted-clarification-plan.json nicht gefunden.", args[i + 1]);
            return path;
        }
        return null;
    }

    private static string? ResolveAcceptedClarificationPlanPath(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && Path.GetFileName(full).Equals("accepted-clarification-plan.json", StringComparison.OrdinalIgnoreCase))
            return full;
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "accepted-clarification-plan.json");
            if (File.Exists(direct)) return direct;
            var applied = Path.Combine(full, "applied", "accepted-clarification-plan.json");
            if (File.Exists(applied)) return applied;
            var planApplied = Path.Combine(full, "plan", "applied", "accepted-clarification-plan.json");
            if (File.Exists(planApplied)) return planApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "clarification-agent");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var candidate = Path.Combine(dir, "plan", "applied", "accepted-clarification-plan.json");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: operationalization-audit <github-reconciliation-runId|accepted-github-action-plan.json> [--clarification-plan <clarification-runId|accepted-clarification-plan.json>] [--out <dir>]");
    }
}
