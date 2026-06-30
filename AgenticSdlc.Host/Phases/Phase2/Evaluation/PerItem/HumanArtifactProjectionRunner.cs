using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Prueft die Projektion von ArtifactClaims in ein Human-Markdown entlang sichtbarer SourceClaim-Refs.</summary>
public static class HumanArtifactProjectionRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: human-artifact-projection <claims.json> <human.md>");
            return 2;
        }

        var claimsPath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var markdownPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        if (!File.Exists(claimsPath)) { Console.Error.WriteLine($"[projection] Claims fehlen: {claimsPath}"); return 2; }
        if (!File.Exists(markdownPath)) { Console.Error.WriteLine($"[projection] Markdown fehlt: {markdownPath}"); return 2; }

        var claims = JsonSerializer.Deserialize<List<GeneratedArtifactClaim>>(
            await File.ReadAllTextAsync(claimsPath).ConfigureAwait(false), JsonOptions)?
            .Select(c => c.Normalized())
            .ToList() ?? [];
        var markdown = await File.ReadAllTextAsync(markdownPath).ConfigureAwait(false);
        if (claims.Count == 0) { Console.Error.WriteLine("[projection] leere Claims."); return 2; }

        var report = HumanArtifactProjectionChecker.Check(claims, markdown);

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike");
        Directory.CreateDirectory(outDir);
        var stem = $"{Path.GetFileNameWithoutExtension(markdownPath)}.projection.json";
        var outPath = Path.Combine(outDir, stem);
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[projection] claims={claims.Count}");
        Console.WriteLine($"[projection] missingRefs={report.MissingRefs.Count} unknownRefs={report.UnknownRefs.Count} unreferencedBullets={report.UnreferencedBullets.Count}");
        Console.WriteLine($"[projection] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }
}
