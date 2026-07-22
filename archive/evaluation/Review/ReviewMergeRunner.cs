using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z4.2-light: Offline-Kommando, das die achsen-spezifischen <c>*.review.json</c> eines Runs (im
/// <c>jury/_units/</c>-Ordner, z. B. <c>…grounding…</c> + <c>…coverage…</c>) je Artefakt zu EINEM
/// konsolidierten <c>&lt;base&gt;.merged.review.json</c> zusammenführt. Reiner Datei-Merge, kein LLM.
/// </summary>
/// <remarks>Aufruf: <c>dotnet run -- merge-review &lt;phase&gt; &lt;runId&gt; [artefakt.md|base]</c>.</remarks>
public static class ReviewMergeRunner
{
    private static readonly string[] DefaultBases = ["requirements", "risks", "architecture", "open-questions"];

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: merge-review <phase> <runId> [artifactFileName|base]");
            return 2;
        }

        var phase = args[1];
        var runId = args[2];
        var unitsDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_units");
        if (!Directory.Exists(unitsDir))
        {
            Console.Error.WriteLine($"[merge-review] Keine _units gefunden: {unitsDir}");
            return 2;
        }

        var bases = args.Length >= 4
            ? new[] { Path.GetFileNameWithoutExtension(args[3]) }
            : DefaultBases;

        var any = false;
        foreach (var b in bases)
        {
            var files = Directory.GetFiles(unitsDir, $"{b}.*.review.json")
                .Where(f => !Path.GetFileName(f).Contains(".merged.", StringComparison.Ordinal))
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();
            if (files.Count == 0)
                continue;

            var jsons = new List<string>(files.Count);
            foreach (var f in files)
                jsons.Add(await File.ReadAllTextAsync(f).ConfigureAwait(false));

            var merged = ReviewMerge.FromJson(jsons);
            var outFile = Path.Combine(unitsDir, $"{b}.merged.review.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(merged, ReviewJson.Options)).ConfigureAwait(false);

            var gate = new GatePolicy().Evaluate(merged);
            var m = merged.Metrics;
            Console.WriteLine(
                $"[merge-review] {b}: {files.Count} Achsen-Result(s) -> axes=[{string.Join(",", merged.EvaluatedAxes)}] " +
                $"grounding={Show(m.GroundingScore)} coverage={Show(m.CoverageScore)} error={Show(m.ErrorScore)} " +
                $"defects={merged.Defects.Count} status={merged.Status} decision={gate.Decision} " +
                $"-> {Path.GetRelativePath(repoRoot, outFile)}");
            foreach (var src in files)
                Console.WriteLine($"        + {Path.GetFileName(src)}");
            any = true;
        }

        if (!any)
            Console.WriteLine($"[merge-review] Keine *.review.json in {Path.GetRelativePath(repoRoot, unitsDir)} gefunden.");
        return 0;
    }

    private static string Show(int? v) => v?.ToString() ?? "null";
}
