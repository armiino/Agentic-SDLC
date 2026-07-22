using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// CLI: github-map apply <mappings.json>   -> traegt PBI<->Issue-Mappings als Core-Relation ein (Dedup-Basis).
//      github-map list                     -> zeigt die aktuellen Mappings aus dem Core.
//
// T3.1 (deterministisch, kein LLM, kein GitHub-Call): der Schreibpfad fuer das Mapping in den lebenden Core.
// Spaeter (T3.4) speist der gated Write-Apply seine erzeugten Mappings ueber genau diesen Baustein ein. Der Core
// wird NUR ueber den Repository-Port geschrieben; ein Audit-Snapshot (core-before) liegt unter runs/github-map/.
public static class CoreGithubMapRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly JsonSerializerOptions OpsJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var sub = args[1].ToLowerInvariant();

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[github-map] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        switch (sub)
        {
            case "list":
            {
                var mappings = CoreGithubMapping.CurrentMappings(core);
                Console.WriteLine($"[github-map] {mappings.Count} Mapping(s):");
                foreach (var m in mappings)
                    Console.WriteLine($"[github-map]   {m.PbiId} -> {CoreGithubMapping.IssueRef(m.IssueNumber)} [{m.OperationalStatus}]{(m.IssueUrl is null ? "" : $" {m.IssueUrl}")}");
                return 0;
            }
            case "apply":
            {
                if (args.Length < 3) { Usage(); return 2; }
                var opsPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
                if (!File.Exists(opsPath)) { Console.Error.WriteLine($"[github-map] Mapping-Datei nicht gefunden: {opsPath}"); return 2; }

                var ops = JsonSerializer.Deserialize<List<GithubMappingOp>>(await File.ReadAllTextAsync(opsPath).ConfigureAwait(false), OpsJson);
                if (ops is null || ops.Count == 0) { Console.Error.WriteLine("[github-map] keine Operationen in der Datei."); return 2; }

                var run = new RunContext(RunId.New(), "github-map");
                Directory.CreateDirectory(run.RunDir);
                await File.WriteAllTextAsync(Path.Combine(run.RunDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);

                var (updated, report) = CoreGithubMapping.Apply(core, ops);
                await coreRepo.SaveAsync(updated).ConfigureAwait(false);

                await File.WriteAllTextAsync(Path.Combine(run.RunDir, "mapping-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(run.RunDir, "mappings.json"), JsonSerializer.Serialize(CoreGithubMapping.CurrentMappings(updated), Json)).ConfigureAwait(false);

                Console.WriteLine($"[github-map] linked={report.Linked.Count} remapped={report.Remapped.Count} closed={report.Closed.Count} reopened={report.Reopened.Count} removed={report.Removed.Count} unchanged={report.Unchanged.Count} skipped={report.Skipped.Count}");
                foreach (var l in report.Linked) Console.WriteLine($"[github-map]   link    {l}");
                foreach (var l in report.Remapped) Console.WriteLine($"[github-map]   remap   {l}");
                foreach (var l in report.Closed) Console.WriteLine($"[github-map]   close   {l}");
                foreach (var l in report.Reopened) Console.WriteLine($"[github-map]   reopen  {l}");
                foreach (var l in report.Removed) Console.WriteLine($"[github-map]   unlink  {l}");
                foreach (var s in report.Skipped) Console.WriteLine($"[github-map]   skip    {s}");
                Console.WriteLine($"[github-map] Core -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))} | Audit -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
                return 0;
            }
            default:
                Usage();
                return 2;
        }
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-map apply <mappings.json>");
        Console.Error.WriteLine("       github-map list");
    }
}
