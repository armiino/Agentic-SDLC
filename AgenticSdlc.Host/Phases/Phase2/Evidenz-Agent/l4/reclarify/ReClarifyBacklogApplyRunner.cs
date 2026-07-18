using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// Materialisiert die menschlich akzeptierten/edited PBIs deterministisch als ProductBacklogView
// (applied/product-backlog.json) + erneutes DoR-Gate. Analog l4-issuplanning-apply. Kein LLM.
public static class ReClarifyBacklogApplyRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-re-clarify-backlog-apply <l4-re-clarify-run|dir>");
            return 2;
        }

        var backlogDir = ResolveBacklogDir(repoRoot, args[1]);
        if (backlogDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-apply] Backlog-Lauf '{args[1]}' nicht gefunden.");
            return 2;
        }
        var backlogPath = Path.Combine(backlogDir, "product-backlog.json");
        var decisionsPath = Path.Combine(backlogDir, "human-decisions.json");
        if (!File.Exists(backlogPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-backlog-apply] product-backlog.json fehlt.");
            return 2;
        }
        if (!File.Exists(decisionsPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-backlog-apply] human-decisions.json fehlt - erst backlog-review fahren.");
            return 2;
        }

        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);
        var decisions = await LoadAsync<BacklogHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);

        // Cluster + Baseline fuer das erneute DoR-Gate (Coverage der Cluster-Cores).
        var clustersFull = Path.IsPathRooted(backlog.SourcePath) ? backlog.SourcePath : Path.Combine(repoRoot, backlog.SourcePath);
        if (!File.Exists(clustersFull))
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-apply] Quell-Cluster nicht gefunden: {backlog.SourcePath}");
            return 2;
        }
        var clusters = await LoadAsync<FeatureClusterSet>(clustersFull).ConfigureAwait(false);
        CanonicalRequirementsBaseline baseline;
        try
        {
            var view = await new JsonProjectStateViewRepository(repoRoot)
                .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
                .ConfigureAwait(false);
            baseline = view.Baseline;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-apply] Baseline nicht gefunden ({clusters.SourceBaselinePath}): {ex.Message}");
            return 2;
        }

        var byPbi = decisions.Decisions.GroupBy(d => d.PbiId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var applied = new List<ProductBacklogItem>();
        var dropped = new List<string>();
        foreach (var pbi in backlog.Items)
        {
            if (!byPbi.TryGetValue(pbi.PbiId, out var d) || string.Equals(d.Decision, "accept", StringComparison.OrdinalIgnoreCase))
            {
                applied.Add(pbi);
            }
            else if (string.Equals(d.Decision, "edit", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(d.EditedPbiJson))
            {
                try { applied.Add(JsonSerializer.Deserialize<ProductBacklogItem>(d.EditedPbiJson!, Json) ?? pbi); }
                catch { applied.Add(pbi); }
            }
            else
            {
                dropped.Add($"{pbi.PbiId}: {d.Decision}");
            }
        }

        var appliedDoc = backlog with
        {
            BacklogId = $"product-backlog-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            CreatedUtc = DateTime.UtcNow,
            Items = applied
        };

        // Traceability deterministisch aus der Provenienz setzen (statt dem Agentenfeld zu vertrauen):
        // CAN-REQ -> baseline.sourceItemIds (ProjectState-Items) -> item.sourceClaimIds (Ledger-Claims).
        IReadOnlyList<ProjectStateItem> stateItems = [];
        try
        {
            var psToken = baseline.SourceProjectStatePath;
            var psFull = Path.IsPathRooted(psToken) ? psToken : Path.Combine(repoRoot, psToken);
            if (!string.IsNullOrWhiteSpace(psToken) && File.Exists(psFull))
            {
                var ps = await JsonProjectStateRepository.LoadAsync(psFull).ConfigureAwait(false);
                stateItems = await ps.ListItemsAsync().ConfigureAwait(false);
            }
        }
        catch { /* best-effort: ohne ProjectState bleibt canonical erhalten, Claims leer */ }
        appliedDoc = ReClarifyTraceabilityEnricher.Enrich(appliedDoc, baseline, stateItems);

        var gate = ReClarifyBacklogGate.Check(clusters, baseline, appliedDoc);

        var appliedOutDir = Path.Combine(backlogDir, "applied");
        Directory.CreateDirectory(appliedOutDir);
        await File.WriteAllTextAsync(Path.Combine(appliedOutDir, "product-backlog.json"), JsonSerializer.Serialize(appliedDoc, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedOutDir, "backlog-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedOutDir, "backlog-apply-report.json"), JsonSerializer.Serialize(new
        {
            pbisBefore = backlog.Items.Count,
            pbisAfter = appliedDoc.Items.Count,
            dropped,
            gatePass = gate.Pass,
            gateErrors = gate.Errors.Count,
            gateWarnings = gate.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        }, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-re-clarify-backlog-apply] pbis {backlog.Items.Count}->{appliedDoc.Items.Count} dropped={dropped.Count} gate={(gate.Pass ? "pass" : "fail")} errors={gate.Errors.Count}");
        foreach (var d in dropped) Console.WriteLine($"[l4-re-clarify-backlog-apply]   drop {d}");
        Console.WriteLine($"[l4-re-clarify-backlog-apply] ProductBacklogView -> {Path.GetRelativePath(repoRoot, Path.Combine(appliedOutDir, "product-backlog.json"))}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveBacklogDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "product-backlog.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "backlog", "product-backlog.json"))) return Path.Combine(full, "backlog");

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var backlog = Path.Combine(dir, "backlog");
            if (File.Exists(Path.Combine(backlog, "product-backlog.json"))) return backlog;
            if (File.Exists(Path.Combine(dir, "product-backlog.json"))) return dir;
        }
        return null;
    }
}
