using AgenticSdlc.Host.FullWorkflow.Backlog;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

public sealed class JsonProjectStateViewRepository(string repoRoot) : IProjectStateViewRepository
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public async ValueTask<CanonicalRequirementsView> GetCanonicalRequirementsViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var dir = ResolveL4AppliedDirectory(scope)
                  ?? throw new FileNotFoundException($"L4 applied view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        var baselinePath = Path.Combine(dir, "canonical-requirements-baseline.json");
        var provenancePath = Path.Combine(dir, "provenance-map.json");
        var qualityPath = Path.Combine(dir, "quality-report.json");
        RequireFile(baselinePath, "canonical-requirements-baseline.json");
        RequireFile(provenancePath, "provenance-map.json");
        RequireFile(qualityPath, "quality-report.json");

        return new CanonicalRequirementsView(
            Scope: scope,
            SourceDirectory: dir,
            BaselinePath: baselinePath,
            ProvenancePath: provenancePath,
            QualityReportPath: qualityPath,
            Baseline: await LoadAsync<CanonicalRequirementsBaseline>(baselinePath, ct).ConfigureAwait(false),
            Provenance: await LoadAsync<L4ProvenanceMap>(provenancePath, ct).ConfigureAwait(false),
            Quality: await LoadAsync<L4QualityReport>(qualityPath, ct).ConfigureAwait(false));
    }

    public async ValueTask<ReadinessView> GetReadinessViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var dir = ResolveL4AppliedDirectory(scope)
                  ?? throw new FileNotFoundException($"Readiness view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        var reportPath = Path.Combine(dir, "requirements-readiness.json");
        var inputPath = Path.Combine(dir, "issue-planning-input.json");
        RequireFile(reportPath, "requirements-readiness.json");
        RequireFile(inputPath, "issue-planning-input.json");
        return new ReadinessView(
            Scope: scope,
            SourceDirectory: dir,
            ReadinessReportPath: reportPath,
            IssuePlanningInputPath: inputPath,
            Report: await LoadAsync<RequirementsReadinessReport>(reportPath, ct).ConfigureAwait(false),
            IssuePlanningInput: await LoadAsync<IssuePlanningInput>(inputPath, ct).ConfigureAwait(false));
    }

    public async ValueTask<IssuePlanningView> GetIssuePlanningViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var resolved = ResolveIssuePlanningInput(scope)
                       ?? throw new FileNotFoundException($"IssuePlanning view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        return new IssuePlanningView(
            Scope: scope,
            SourceDirectory: resolved.SourceDirectory,
            IssuePlanningInputPath: resolved.IssuePlanningInputPath,
            Input: await LoadAsync<IssuePlanningInput>(resolved.IssuePlanningInputPath, ct).ConfigureAwait(false));
    }

    public async ValueTask<ClarificationPlanningView> GetClarificationPlanningViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var resolved = ResolveClarificationPlanningInput(scope)
                       ?? throw new FileNotFoundException($"ClarificationPlanning view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        return new ClarificationPlanningView(
            Scope: scope,
            SourceDirectory: resolved.SourceDirectory,
            ClarificationPlanningInputPath: resolved.ClarificationPlanningInputPath,
            Input: await LoadAsync<ClarificationPlanningInput>(resolved.ClarificationPlanningInputPath, ct).ConfigureAwait(false));
    }

    public async ValueTask<AcceptedIssuePlanView> GetAcceptedIssuePlanViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var dir = ResolveIssuePlanAppliedDirectory(scope)
                  ?? throw new FileNotFoundException($"AcceptedIssuePlan view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        var planPath = Path.Combine(dir, "accepted-issue-plan.json");
        var gatePath = Path.Combine(dir, "accepted-issue-plan-gate-report.json");
        RequireFile(planPath, "accepted-issue-plan.json");
        RequireFile(gatePath, "accepted-issue-plan-gate-report.json");
        return new AcceptedIssuePlanView(
            Scope: scope,
            SourceDirectory: dir,
            AcceptedIssuePlanPath: planPath,
            GateReportPath: gatePath,
            Plan: await LoadAsync<IssuePlanDocument>(planPath, ct).ConfigureAwait(false),
            Gate: await LoadAsync<IssuePlanGateReport>(gatePath, ct).ConfigureAwait(false));
    }

    public async ValueTask<AcceptedClarificationPlanView> GetAcceptedClarificationPlanViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var dir = ResolveClarificationPlanAppliedDirectory(scope)
                  ?? throw new FileNotFoundException($"AcceptedClarificationPlan view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        var planPath = Path.Combine(dir, "accepted-clarification-plan.json");
        var gatePath = Path.Combine(dir, "accepted-clarification-plan-gate-report.json");
        RequireFile(planPath, "accepted-clarification-plan.json");
        RequireFile(gatePath, "accepted-clarification-plan-gate-report.json");
        return new AcceptedClarificationPlanView(
            Scope: scope,
            SourceDirectory: dir,
            AcceptedClarificationPlanPath: planPath,
            GateReportPath: gatePath,
            Plan: await LoadAsync<ClarificationPlanDocument>(planPath, ct).ConfigureAwait(false),
            Gate: await LoadAsync<ClarificationPlanGateReport>(gatePath, ct).ConfigureAwait(false));
    }

    public async ValueTask<ProductBacklogView> GetProductBacklogViewAsync(ProjectScope scope, CancellationToken ct = default)
    {
        var dir = ResolveProductBacklogDirectory(scope)
                  ?? throw new FileNotFoundException($"ProductBacklog view konnte nicht aufgeloest werden: {scope.SourcePath ?? scope.BaselineId ?? scope.ScopeType}");
        var backlogPath = Path.Combine(dir, "product-backlog.json");
        RequireFile(backlogPath, "product-backlog.json");
        return new ProductBacklogView(
            Scope: scope,
            SourceDirectory: dir,
            ProductBacklogPath: backlogPath,
            Backlog: await LoadAsync<ProductBacklogDocument>(backlogPath, ct).ConfigureAwait(false));
    }

    private string? ResolveProductBacklogDirectory(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "product-backlog.json", StringComparison.OrdinalIgnoreCase))
            return Path.GetDirectoryName(full);
        if (Directory.Exists(full))
        {
            if (File.Exists(Path.Combine(full, "product-backlog.json"))) return full;
            var applied = Path.Combine(full, "applied");
            if (File.Exists(Path.Combine(applied, "product-backlog.json"))) return applied;
            var backlogApplied = Path.Combine(full, "backlog", "applied");
            if (File.Exists(Path.Combine(backlogApplied, "product-backlog.json"))) return backlogApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var backlog = Path.Combine(runDir, "backlog");
            if (File.Exists(Path.Combine(backlog, "product-backlog.json"))) return backlog;
            var backlogApplied = Path.Combine(backlog, "applied");
            if (File.Exists(Path.Combine(backlogApplied, "product-backlog.json"))) return backlogApplied;
        }
        return null;
    }

    private ResolvedIssuePlanningInput? ResolveIssuePlanningInput(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "issue-planning-input.json", StringComparison.OrdinalIgnoreCase))
            return new ResolvedIssuePlanningInput(full, Path.GetDirectoryName(full) ?? repoRoot);
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "issue-planning-input.json");
            if (File.Exists(direct)) return new ResolvedIssuePlanningInput(direct, full);

            var consolidationApplied = Path.Combine(full, "consolidation", "applied", "issue-planning-input.json");
            if (File.Exists(consolidationApplied)) return new ResolvedIssuePlanningInput(consolidationApplied, Path.GetDirectoryName(consolidationApplied) ?? full);

            var completionApplied = Path.Combine(full, "completion", "applied", "issue-planning-input.json");
            if (File.Exists(completionApplied)) return new ResolvedIssuePlanningInput(completionApplied, Path.GetDirectoryName(completionApplied) ?? full);
        }

        foreach (var root in ExistingRunRoots("l4", "l4-completion"))
        {
            foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                var consolidationApplied = Path.Combine(runDir, "consolidation", "applied", "issue-planning-input.json");
                if (File.Exists(consolidationApplied)) return new ResolvedIssuePlanningInput(consolidationApplied, Path.GetDirectoryName(consolidationApplied) ?? runDir);

                var completionApplied = Path.Combine(runDir, "completion", "applied", "issue-planning-input.json");
                if (File.Exists(completionApplied)) return new ResolvedIssuePlanningInput(completionApplied, Path.GetDirectoryName(completionApplied) ?? runDir);
            }
        }
        return null;
    }

    private ResolvedClarificationPlanningInput? ResolveClarificationPlanningInput(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "clarification-planning-input.json", StringComparison.OrdinalIgnoreCase))
            return new ResolvedClarificationPlanningInput(full, Path.GetDirectoryName(full) ?? repoRoot);
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "clarification-planning-input.json");
            if (File.Exists(direct)) return new ResolvedClarificationPlanningInput(direct, full);

            var applied = Path.Combine(full, "applied", "clarification-planning-input.json");
            if (File.Exists(applied)) return new ResolvedClarificationPlanningInput(applied, Path.GetDirectoryName(applied) ?? full);

            var operationalizationApplied = Path.Combine(full, "plan", "applied", "operationalization-audit", "applied", "clarification-planning-input.json");
            if (File.Exists(operationalizationApplied)) return new ResolvedClarificationPlanningInput(operationalizationApplied, Path.GetDirectoryName(operationalizationApplied) ?? full);

            var nestedOperationalizationApplied = Path.Combine(full, "operationalization-audit", "applied", "clarification-planning-input.json");
            if (File.Exists(nestedOperationalizationApplied)) return new ResolvedClarificationPlanningInput(nestedOperationalizationApplied, Path.GetDirectoryName(nestedOperationalizationApplied) ?? full);
        }

        var root = Path.Combine(repoRoot, "runs", "github-reconciliation");
        if (!Directory.Exists(root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var path = Path.Combine(runDir, "plan", "applied", "operationalization-audit", "applied", "clarification-planning-input.json");
            if (File.Exists(path)) return new ResolvedClarificationPlanningInput(path, Path.GetDirectoryName(path) ?? runDir);
        }
        return null;
    }

    private string? ResolveL4AppliedDirectory(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "canonical-requirements-baseline.json", StringComparison.OrdinalIgnoreCase))
            return Path.GetDirectoryName(full);
        if (Directory.Exists(full))
        {
            if (File.Exists(Path.Combine(full, "canonical-requirements-baseline.json"))) return full;
            var consolidationApplied = Path.Combine(full, "consolidation", "applied");
            if (File.Exists(Path.Combine(consolidationApplied, "canonical-requirements-baseline.json"))) return consolidationApplied;
            var completionApplied = Path.Combine(full, "completion", "applied");
            if (File.Exists(Path.Combine(completionApplied, "canonical-requirements-baseline.json"))) return completionApplied;
        }

        foreach (var root in ExistingRunRoots("l4", "l4-completion"))
        {
            foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                var consolidationApplied = Path.Combine(runDir, "consolidation", "applied");
                if (File.Exists(Path.Combine(consolidationApplied, "canonical-requirements-baseline.json"))) return consolidationApplied;
                var completionApplied = Path.Combine(runDir, "completion", "applied");
                if (File.Exists(Path.Combine(completionApplied, "canonical-requirements-baseline.json"))) return completionApplied;
            }
        }
        return null;
    }

    private string? ResolveIssuePlanAppliedDirectory(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (Directory.Exists(full))
        {
            if (File.Exists(Path.Combine(full, "accepted-issue-plan.json"))) return full;
            var applied = Path.Combine(full, "applied");
            if (File.Exists(Path.Combine(applied, "accepted-issue-plan.json"))) return applied;
            var planApplied = Path.Combine(full, "plan", "applied");
            if (File.Exists(Path.Combine(planApplied, "accepted-issue-plan.json"))) return planApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "l4-issuplanning");
        if (!Directory.Exists(root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var planApplied = Path.Combine(runDir, "plan", "applied");
            if (File.Exists(Path.Combine(planApplied, "accepted-issue-plan.json"))) return planApplied;
        }
        return null;
    }

    private string? ResolveClarificationPlanAppliedDirectory(ProjectScope scope)
    {
        var token = scope.SourcePath ?? scope.BaselineId ?? "";
        var full = ResolvePath(token);
        if (Directory.Exists(full))
        {
            if (File.Exists(Path.Combine(full, "accepted-clarification-plan.json"))) return full;
            var applied = Path.Combine(full, "applied");
            if (File.Exists(Path.Combine(applied, "accepted-clarification-plan.json"))) return applied;
            var planApplied = Path.Combine(full, "plan", "applied");
            if (File.Exists(Path.Combine(planApplied, "accepted-clarification-plan.json"))) return planApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "clarification-agent");
        if (!Directory.Exists(root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var planApplied = Path.Combine(runDir, "plan", "applied");
            if (File.Exists(Path.Combine(planApplied, "accepted-clarification-plan.json"))) return planApplied;
        }
        return null;
    }

    private IEnumerable<string> ExistingRunRoots(params string[] names)
    {
        foreach (var name in names)
        {
            var path = Path.Combine(repoRoot, "runs", name);
            if (Directory.Exists(path)) yield return path;
        }
    }

    private async Task<T> LoadAsync<T>(string path, CancellationToken ct)
    {
        var json = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private string ResolvePath(string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static void RequireFile(string path, string name)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"{name} fehlt.", path);
    }

    private sealed record ResolvedIssuePlanningInput(string IssuePlanningInputPath, string SourceDirectory);
    private sealed record ResolvedClarificationPlanningInput(string ClarificationPlanningInputPath, string SourceDirectory);
}
