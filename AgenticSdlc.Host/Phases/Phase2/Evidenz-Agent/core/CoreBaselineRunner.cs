using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// CLI: core-baseline [--out <dir>]
// Materialisiert das L4-Applied-Triplet aus dem lebenden Core, damit die re-clarify-Kette (cluster ->
// clarify -> PBIs -> issues) den Core als Requirement-Quelle nutzt. Deterministisch, kein LLM. Inc 1b.
public static class CoreBaselineRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        string? outArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { outArg = args[++i]; continue; }
            if (args[i].StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[core-baseline] unbekanntes Argument: {args[i]}"); return 2; }
        }

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[core-baseline] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var coreSourceRel = Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot));
        var (baseline, provenance, quality) = CoreToBaseline.Project(core, coreSourceRel);

        var outDir = outArg is null
            ? Path.Combine(repoRoot, "runs", "core-baseline", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"))
            : (Path.IsPathRooted(outArg) ? outArg : Path.Combine(repoRoot, outArg));
        Directory.CreateDirectory(outDir);

        await File.WriteAllTextAsync(Path.Combine(outDir, "canonical-requirements-baseline.json"), JsonSerializer.Serialize(baseline, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "provenance-map.json"), JsonSerializer.Serialize(provenance, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "quality-report.json"), JsonSerializer.Serialize(quality, Json)).ConfigureAwait(false);

        Console.WriteLine($"[core-baseline] requirements={baseline.Requirements.Count} openDecisions={baseline.OpenDecisions.Count} (aus Core {coreSourceRel})");
        Console.WriteLine($"[core-baseline] -> {Path.GetRelativePath(repoRoot, outDir)}");
        Console.WriteLine($"[core-baseline] naechster Schritt: l4-re-clarify cluster {Path.GetRelativePath(repoRoot, Path.Combine(outDir, "canonical-requirements-baseline.json"))} <model>");
        return 0;
    }
}
