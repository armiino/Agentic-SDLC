using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// CLI: core-seed-backlog <l4-re-clarify-run|dir>
// Hebt Cluster (Features) + PBIs eines re-clarify-Laufs als persistente Core-Entitaeten in den Core (Inc 1c-1).
// Deterministisch, kein LLM. Idempotent (primaer via legacyId). Core wird NUR ueber den Port geschrieben.
public static class CoreSeedBacklogRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

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
        var backlog = await LoadAsync<ProductBacklogDocument>(backlogPath).ConfigureAwait(false);

        var clustersFull = Path.IsPathRooted(backlog.SourcePath) ? backlog.SourcePath : Path.Combine(repoRoot, backlog.SourcePath);
        if (!File.Exists(clustersFull))
        {
            Console.Error.WriteLine($"[core-seed-backlog] Quell-Cluster nicht gefunden: {backlog.SourcePath}");
            return 2;
        }
        var clusters = await LoadAsync<FeatureClusterSet>(clustersFull).ConfigureAwait(false);

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[core-seed-backlog] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var sourceRunId = InferRunId(backlogPath);
        var (updated, report) = CoreBacklogSeeder.Seed(core, clusters, backlog, sourceRunId);

        var problems = ValidatePayloadItemType(updated);
        if (problems.Count > 0)
        {
            Console.Error.WriteLine($"[core-seed-backlog] ABBRUCH: {problems.Count} Payload/itemType-Verstoesse (z.B. {problems[0]}).");
            return 1;
        }

        await repo.SaveAsync(updated).ConfigureAwait(false);

        Console.WriteLine($"[core-seed-backlog] featuresAdded={report.FeaturesAdded} pbisAdded={report.PbisAdded} relations={report.RelationsAdded} skippedExisting={report.SkippedExisting} unresolvedReqRefs={report.RequirementRefsUnresolved}");
        Console.WriteLine($"[core-seed-backlog] Core items {core.Items.Count}->{updated.Items.Count} -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))} (sourceRun {sourceRunId})");
        if (report.FeaturesAdded == 0 && report.PbisAdded == 0)
            Console.WriteLine("[core-seed-backlog] nichts Neues (idempotent: bereits geseedet).");
        return 0;
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
