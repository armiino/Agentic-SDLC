using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// CLI: core-view <active-backlog|archive|github-sync> [--out <file>]
// Materialisiert eine arbeitsfaehige View auf den Core (deterministisch, kein LLM). Die affected-items-view
// entsteht dagegen bei ingest-apply (Blast-Radius eines konkreten Deltas).
public static class CoreViewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }
        var name = args[1].ToLowerInvariant();

        string? outArg = null;
        for (var i = 2; i < args.Length; i++)
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) outArg = args[++i];

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[core-view] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await repo.LoadAsync().ConfigureAwait(false);

        object view;
        string summary;
        switch (name)
        {
            case "active-backlog":
            {
                var v = CoreViews.ActiveBacklog(core);
                view = v;
                summary = $"features={v.Features.Count} pbis={v.Pbis.Count} requirements={v.Requirements.Count} openDecisions={v.OpenDecisions.Count}";
                break;
            }
            case "archive":
            {
                var v = CoreViews.Archive(core);
                view = v;
                summary = $"archived={v.Items.Count} (done/superseded/retired)";
                break;
            }
            case "github-sync":
            {
                var v = CoreViews.GithubSync(core);
                view = v;
                summary = $"pbis={v.Entries.Count} blocked={v.Entries.Count(e => e.BlockedByOpenDecision)} mapped={v.Entries.Count(e => e.GithubIssue is not null)}";
                break;
            }
            default:
                Console.Error.WriteLine($"[core-view] unbekannte View '{name}'.");
                Usage();
                return 2;
        }

        var outPath = outArg is null
            ? Path.Combine(repoRoot, "state", "core", "views", $"{name}.json")
            : (Path.IsPathRooted(outArg) ? outArg : Path.Combine(repoRoot, outArg));
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? ".");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(view, Json)).ConfigureAwait(false);

        Console.WriteLine($"[core-view] {name}: {summary}");
        Console.WriteLine($"[core-view] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: core-view <active-backlog|archive|github-sync> [--out <file>]");
}
