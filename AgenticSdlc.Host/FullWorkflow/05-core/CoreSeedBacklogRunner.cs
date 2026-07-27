using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// CLI: core-seed-backlog <l4-re-clarify-run|dir>
// Hebt Cluster (Features) + PBIs eines re-clarify-Laufs als persistente Core-Entitaeten in den Core (Inc 1c-1).
// Deterministisch, kein LLM. Idempotent (primaer via legacyId). Core wird NUR ueber den Port geschrieben.
public static class CoreSeedBacklogRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: core-seed-backlog <l4-re-clarify-run|dir>");
            return 2;
        }

        var backlogPath = ResolveBacklogPath(repoRoot, args[1]);
        if (backlogPath is null)
        {
            Console.Error.WriteLine($"[core-seed-backlog] product-backlog.json fuer '{args[1]}' nicht gefunden.");
            return 2;
        }
        var sourceRunId = InferRunId(backlogPath);
        CoreBacklogSeeder.Report report;
        int itemsBefore, itemsAfter;
        try
        {
            (report, itemsBefore, itemsAfter) = await ExecuteAsync(backlogPath, repoRoot, sourceRunId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[core-seed-backlog] {ex.Message}");
            return ex is InvalidOperationException && ex.Message.StartsWith("ABBRUCH", StringComparison.Ordinal) ? 1 : 2;
        }

        Console.WriteLine($"[core-seed-backlog] featuresAdded={report.FeaturesAdded} pbisAdded={report.PbisAdded} relations={report.RelationsAdded} skippedExisting={report.SkippedExisting} unresolvedReqRefs={report.RequirementRefsUnresolved}");
        Console.WriteLine($"[core-seed-backlog] Core items {itemsBefore}->{itemsAfter} -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))} (sourceRun {sourceRunId})");
        if (report.FeaturesAdded == 0 && report.PbisAdded == 0)
            Console.WriteLine("[core-seed-backlog] nichts Neues (idempotent: bereits geseedet).");
        return 0;
    }

    // pipeline-full (B4): der EINE Seed-Kern fuer CLI-Runner UND Graph-Knoten. Idempotent (CoreBacklogSeeder);
    // Payload/itemType-Verstoesse => Abbruch VOR dem Save (kein kaputter Core).
    internal static async Task<(CoreBacklogSeeder.Report Report, int ItemsBefore, int ItemsAfter)> ExecuteAsync(
        string backlogPath, string repoRoot, string sourceRunId)
    {
        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);

        var clustersFull = Path.IsPathRooted(backlog.SourcePath) ? backlog.SourcePath : Path.Combine(repoRoot, backlog.SourcePath);
        if (!File.Exists(clustersFull))
            throw new InvalidOperationException($"Quell-Cluster nicht gefunden: {backlog.SourcePath}");
        var clusters = await LoadAsync<FeatureClusterSet>(clustersFull).ConfigureAwait(false);

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Core fehlt - erst 'core-seed' fahren.");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var (updated, report) = CoreBacklogSeeder.Seed(core, clusters, backlog, sourceRunId);

        var problems = ValidatePayloadItemType(updated);
        if (problems.Count > 0)
            throw new InvalidOperationException($"ABBRUCH: {problems.Count} Payload/itemType-Verstoesse (z.B. {problems[0]}).");

        await repo.SaveAsync(updated).ConfigureAwait(false);
        return (report, core.Items.Count, updated.Items.Count);
    }

    // Payload passt zu itemType: feature-Payload nur bei feature, pbi-Payload nur bei pbi; nie beide.
    private static List<string> ValidatePayloadItemType(ProjectStateDocument core)
    {
        var problems = new List<string>();
        foreach (var i in core.Items)
        {
            var isFeature = string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase);
            var isPbi = string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase);
            if (i.Feature is not null && !isFeature) problems.Add($"{i.ItemId}: feature-Payload bei itemType={i.ItemType}");
            if (i.Pbi is not null && !isPbi) problems.Add($"{i.ItemId}: pbi-Payload bei itemType={i.ItemType}");
            if (i.Feature is not null && i.Pbi is not null) problems.Add($"{i.ItemId}: beide Payloads gesetzt");
            if (isFeature && i.Feature is null) problems.Add($"{i.ItemId}: feature ohne Payload");
            if (isPbi && i.Pbi is null) problems.Add($"{i.ItemId}: pbi ohne Payload");
        }
        return problems;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveBacklogPath(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "product-backlog.json", StringComparison.OrdinalIgnoreCase)) return full;

        string[] candidates =
        [
            Path.Combine(full, "backlog", "applied", "product-backlog.json"),
            Path.Combine(full, "backlog", "product-backlog.json"),
            Path.Combine(full, "applied", "product-backlog.json"),
            Path.Combine(full, "product-backlog.json"),
        ];
        foreach (var c in candidates) if (File.Exists(c)) return c;

        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var applied = Path.Combine(dir, "backlog", "applied", "product-backlog.json");
            if (File.Exists(applied)) return applied;
            var plain = Path.Combine(dir, "backlog", "product-backlog.json");
            if (File.Exists(plain)) return plain;
        }
        return null;
    }

    private static string InferRunId(string backlogPath)
    {
        // …/runs/l4-re-clarify/<runId>/backlog/applied/product-backlog.json -> <runId>
        var dir = new DirectoryInfo(Path.GetDirectoryName(backlogPath)!);
        while (dir is not null && !string.Equals(dir.Parent?.Name, "l4-re-clarify", StringComparison.OrdinalIgnoreCase))
            dir = dir.Parent;
        return dir?.Name ?? "unknown";
    }
}
