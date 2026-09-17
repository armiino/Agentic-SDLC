using AgenticSdlc.Host.FullWorkflow.Backlog;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

// Schlank seit dem Alt-Ketten-Rückbau (04.08.): nur die lebend konsumierten Views (Canonical fuer re-clarify/pipeline,
// IssuePlanning fuer issuplanning). Die Resolver kennen weiterhin die Alt-Lauf-Layouts (consolidation/completion/applied),
// damit archivierte Laeufe (runsArchive, Thesis-Evidenz) lesbar bleiben.
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
}
