using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// DISK-14 MISSING/Coverage-Achse: Offline-Kommando, das pro Artefakt die Transkript-Turns gegen das
/// Artefakt klassifiziert (not_actionable | covered | missing) und einen <c>CoverageScore</c> (# missing)
/// schreibt. Spiegel zu <see cref="ClassifyUnitsRunner"/>; gleiche Isolation/Reversibilitaet.
/// </summary>
/// <remarks>Aufruf: <c>coverage-units &lt;phase&gt; &lt;runId&gt; [artefakt.md] [judgeModel] [transcript.txt]</c>.</remarks>
public static class CoverageRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: coverage-units <phase> <runId> [artifactFileName] [judgeModelOverride] [transcript.txt]");
            return 2;
        }

        var phase = args[1];
        var runId = args[2];

        string? artifactArg = null, judgeArg = null, transcriptArg = null;
        foreach (var a in args.Skip(3))
        {
            if (a.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) artifactArg = a;
            else if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else judgeArg = a;
        }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[coverage-units] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var transcript = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null)
        {
            Console.Error.WriteLine($"[coverage-units] Transkript nicht gefunden ({transcriptArg ?? "erstes alphabetisch"}).");
            return 2;
        }

        var turns = TranscriptSegmenter.Segment(transcript);
        Console.WriteLine($"[coverage-units] Transkript: {transcriptArg ?? "(erstes)"} -> {turns.Count} Turns (deterministisch)");

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var classifier = new CoverageClassifier(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        var outDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_units");
        Directory.CreateDirectory(outDir);

        var artifacts = artifactArg is not null ? [artifactArg] : DefaultArtifacts;
        Console.WriteLine($"[coverage-units] Judge: {judgeSettings.LlmProvider} / {judgeSettings.ModelId}  (chunk={CoverageClassifier.ChunkSize})");

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path)) { Console.WriteLine($"[coverage-units] uebersprungen (fehlt): {artifact}"); continue; }

            var artifactText = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var artifactType = JuryCategoryProfile.ArtifactType(artifact);
            var verdicts = await classifier.ClassifyAsync(artifactText, artifactType, turns, CancellationToken.None).ConfigureAwait(false);

            var coverageScore = verdicts.Sum(v => CoverageClassifier.Weight(v.Verdict));
            var counts = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count());
            var byIndex = verdicts.ToDictionary(v => v.Index);

            var rows = turns.Select(t => new
            {
                index = t.Index, speaker = t.Speaker,
                verdict = byIndex[t.Index].Verdict, reason = byIndex[t.Index].Reason, text = t.Text
            }).ToList();

            var payload = new
            {
                artifact, phase, runId,
                judge = new { provider = judgeSettings.LlmProvider, model = judgeSettings.ModelId },
                turnCount = turns.Count, coverageScore, counts, verdicts = rows
            };

            var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.coverage.{modelSlug}.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

            // Z3.2: zusätzlich das gemeinsame ReviewResult-Format (nur Coverage-Achse, ohne Grounding).
            var perItemCov = turns
                .Select(t => new Review.PerItemCoverage(t.Speaker, t.Text, byIndex[t.Index].Verdict, byIndex[t.Index].Reason))
                .ToList();
            var review = Review.PerItemReviewMapper.Map(artifact, artifactType, units: null, coverage: perItemCov, judgeModel: judgeSettings.ModelId);
            var reviewFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.coverage.{modelSlug}.review.json");
            await File.WriteAllTextAsync(reviewFile, JsonSerializer.Serialize(review, Review.ReviewJson.Options)).ConfigureAwait(false);

            var c = (string k) => counts.TryGetValue(k, out var n) ? n : 0;
            Console.WriteLine(
                $"[coverage-units] {artifact}: CoverageScore={coverageScore}  missing={c("missing")} " +
                $"covered={c("covered")} not_actionable={c("not_actionable")} -> {Path.GetRelativePath(repoRoot, outFile)}");
            foreach (var v in verdicts.Where(v => v.Verdict == "missing"))
                Console.WriteLine($"      [missing] [{turns[v.Index].Speaker}] {Truncate(turns[v.Index].Text, 70)}  ({Truncate(v.Reason, 60)})");
        }

        return 0;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    private static string? LoadTranscript(string repoRoot, string? name)
    {
        var dirs = new[]
        {
            Path.Combine(repoRoot, "input", "transcripts"),
            Path.Combine(repoRoot, "input", "transcripts_backup")
        };
        if (name is not null)
        {
            foreach (var d in dirs)
            {
                var p = Path.Combine(d, name);
                if (File.Exists(p)) return File.ReadAllText(p);
            }
            return null;
        }
        var first = Directory.Exists(dirs[0])
            ? Directory.GetFiles(dirs[0], "*.txt").OrderBy(f => f, StringComparer.Ordinal).FirstOrDefault()
            : null;
        return first is null ? null : File.ReadAllText(first);
    }
}
