using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// CLI: core-seed <project-state.json>
// Hebt einen bestehenden ProjectState-Rebuild als Ausgangs-Wahrheit in den Core (state/core/project-state.json).
// Einmalig: bricht ab, falls der Core schon existiert (kein stilles Ueberschreiben der Wahrheit).
public static class CoreSeedRunner
{
    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: core-seed <project-state.json>");
            return 2;
        }

        var repo = new JsonCoreRepository(repoRoot);
        if (await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine($"[core-seed] Core existiert bereits: {CorePaths.CoreFile(repoRoot)} - Seed abgebrochen (kein Ueberschreiben).");
            return 1;
        }

        var sourcePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine($"[core-seed] Quelle nicht gefunden: {sourcePath}");
            return 2;
        }

        var source = (await JsonProjectStateRepository.LoadAsync(sourcePath).ConfigureAwait(false)).Document;
        var (core, report) = CoreSeeder.Seed(source);
        await repo.SaveAsync(core).ConfigureAwait(false);

        // Retrieval-Stufe-0-Selbstcheck (Sub-Phase 1.2): wie viele Requirement-Kandidaten liefert der Retriever?
        var candidates = new ShowAllRequirementRetriever().GetCandidates(string.Empty, core);

        Console.WriteLine($"[core-seed] Core -> {Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))}");
        Console.WriteLine($"[core-seed] items={report.Total} requirement={report.Requirements} architecture={report.Architecture} other={report.Other} identityKey={report.WithIdentityKey}");
        Console.WriteLine($"[core-seed] retriever(Stufe0) requirement-Kandidaten={candidates.Count}");
        return 0;
    }
}
