using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

// CLI: github-inbound [--issues <snapshot.json>] [--draft]  — die ZWISCHENBAHN (c2-inbound-plan §2/§13):
// isoliert testbare Ernte über die geteilte Naht GithubInboundHarvest (dieselbe Quelle wie der Ein-Graph-
// Eingang `pipeline-full run --from-github`). --draft = der bewusste LLM-Schritt (Kosten-Regel).
public static class GithubInboundRunner
{
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? issuesArg = null; var draft = false;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) issuesArg = args[++i];
            else if (string.Equals(args[i], "--draft", StringComparison.OrdinalIgnoreCase)) draft = true;
        }
        return await RunHarvestAsync(settings, repoRoot, issuesArg, draft).ConfigureAwait(false);
    }

    /// <summary>K13-2: typisierte Naht — Ernte-Lauf ohne CLI-Args (Steward + CLI-Haut; Kern = GithubInboundHarvest).</summary>
    public static async Task<int> RunHarvestAsync(HostSettings settings, string repoRoot, string? issuesArg, bool draft)
    {
        var run = new RunContext(RunId.New(), "github-inbound");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        var (exit, result) = await GithubInboundHarvest.RunAsync(settings, repoRoot, run, issuesArg, draft, outDir)
            .ConfigureAwait(false);
        if (exit != 0 || result is null) return exit;

        if (result.Delta is { Items.Count: > 0 })
            Console.WriteLine($"[github-inbound] naechster Schritt: pipeline-full run --from-delta "
                + $"{Path.GetRelativePath(repoRoot, Path.Combine(outDir, "inbound-delta.json"))} — ODER direkt: pipeline-full run --from-github");
        else
            Console.WriteLine($"[github-inbound] -> {Path.GetRelativePath(repoRoot, outDir)}/harvest-report.json"
                + (draft ? "" : " (Drafting: --draft)"));
        return 0;
    }
}
