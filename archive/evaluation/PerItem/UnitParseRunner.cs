using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// DISK-14 Phase 1: Offline-Kommando, das die Artefakte eines Runs deterministisch in Prüfeinheiten
/// zerlegt und als <c>jury/_units/&lt;artefakt&gt;.units.json</c> persistiert — OHNE LLM, OHNE Eingriff
/// in den bestehenden Jury-Pfad. Zweck: die Unit-Qualität vor jeder Klassifikation per Sichtprüfung
/// belegen (Parser zuerst, dann erst der Judge).
/// </summary>
/// <remarks>
/// Aufruf: <c>dotnet run --project AgenticSdlc.Host -- parse-units &lt;phase&gt; &lt;runId&gt; [artefakt]</c>.
/// Ohne [artefakt] werden alle 4 Standardartefakte geparst. Vollständig additiv: Verwerfen =
/// PerItem-Ordner löschen + Dispatch-Zeile in Program.cs entfernen.
/// </remarks>
public static class UnitParseRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: parse-units <phase> <runId> [artifactFileName]");
            return 2;
        }

        var phase = args[1];
        var runId = args[2];
        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[parse-units] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var outDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_units");
        Directory.CreateDirectory(outDir);

        var artifacts = args.Length >= 4 ? [args[3]] : DefaultArtifacts;

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path))
            {
                Console.WriteLine($"[parse-units] uebersprungen (fehlt): {artifact}");
                continue;
            }

            var markdown = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var units = ArtifactUnitParser.Parse(markdown);

            var payload = new
            {
                artifact,
                phase,
                runId,
                unitCount = units.Count,
                likelyNonClaimCount = units.Count(u => u.LikelyNonClaim),
                units
            };

            var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(artifact) + ".units.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

            var kinds = units.GroupBy(u => u.Kind).OrderBy(g => g.Key)
                .Select(g => $"{g.Key}={g.Count()}");
            Console.WriteLine(
                $"[parse-units] {artifact}: {units.Count} units ({string.Join(", ", kinds)}); " +
                $"{payload.likelyNonClaimCount} advisory-flagged -> {Path.GetRelativePath(repoRoot, outFile)}");
        }

        return 0;
    }
}
