using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Materialisiert die menschlich akzeptierten/edited PBIs deterministisch als ProductBacklogView
// (applied/product-backlog.json) + erneutes DoR-Gate. Analog l4-issuplanning-apply. Kein LLM.
public static class ReClarifyBacklogApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

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

        var decisions = await LoadAsync<BacklogHumanDecisionsFile>(decisionsPath).ConfigureAwait(false);

        BacklogApplyExecResult exec;
        try
        {
            exec = await ExecuteAsync(backlogDir, repoRoot, decisions.Decisions).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-apply] {ex.Message}");
            return 2;
        }

        Console.WriteLine($"[l4-re-clarify-backlog-apply] pbis {exec.PbisBefore}->{exec.PbisAfter} dropped={exec.Dropped.Count} gate={(exec.Gate.Pass ? "pass" : "fail")} errors={exec.Gate.Errors.Count}");
        foreach (var d in exec.Dropped) Console.WriteLine($"[l4-re-clarify-backlog-apply]   drop {d}");
        Console.WriteLine($"[l4-re-clarify-backlog-apply] ProductBacklogView -> {Path.GetRelativePath(repoRoot, exec.AppliedBacklogPath)}");
        return exec.Gate.Pass ? 0 : 1;
    }

    public sealed record BacklogApplyExecResult(
        string AppliedBacklogPath, int PbisBefore, int PbisAfter, IReadOnlyList<string> Dropped, ReClarifyGateReport Gate);

    // pipeline-full (B4): der EINE Apply-Kern fuer CLI-Runner UND Graph-Knoten (Muster: IngestionApplyExec).
    // Semantik unveraendert: fehlender Entscheid = accept; edit ersetzt das PBI durch EditedPbiJson; reject/revise
    // droppt. Danach deterministisches Traceability-Enrichment + erneutes DoR-Gate + applied/-Materialisierung.
    internal static async Task<BacklogApplyExecResult> ExecuteAsync(
        string backlogDir, string repoRoot, IReadOnlyList<BacklogHumanDecision> decisions)
    {
        var backlogPath = Path.Combine(backlogDir, "product-backlog.json");
        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);

        // Cluster + Baseline fuer das erneute DoR-Gate (Coverage der Cluster-Cores).
        var clustersFull = Path.IsPathRooted(backlog.SourcePath) ? backlog.SourcePath : Path.Combine(repoRoot, backlog.SourcePath);
        if (!File.Exists(clustersFull))
            throw new InvalidOperationException($"Quell-Cluster nicht gefunden: {backlog.SourcePath}");
        var clusters = await LoadAsync<FeatureClusterSet>(clustersFull).ConfigureAwait(false);

        var view = await new JsonProjectStateViewRepository(repoRoot)
            .GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(clusters.SourceBaselinePath, "re-clarify", "current_baseline"))
            .ConfigureAwait(false);
        var baseline = view.Baseline;

        var byPbi = decisions.GroupBy(d => d.PbiId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
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
        var appliedBacklogPath = Path.Combine(appliedOutDir, "product-backlog.json");
        await File.WriteAllTextAsync(appliedBacklogPath, JsonSerializer.Serialize(appliedDoc, Json)).ConfigureAwait(false);
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

        return new BacklogApplyExecResult(appliedBacklogPath, backlog.Items.Count, appliedDoc.Items.Count, dropped, gate);
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
