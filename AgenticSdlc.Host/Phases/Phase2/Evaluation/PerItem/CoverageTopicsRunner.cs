using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z6.3 (DISK-COV V1): Topic-basierte Coverage gegen ein Artefakt. Lädt die eingefrorene Topic-Fixture,
/// gated auf Artefakt-Relevanz, klassifiziert die relevanten Topics (covered/partial/missing/…) und
/// schreibt ein konsolidiertes <c>&lt;base&gt;.topic-coverage.&lt;model&gt;.review.json</c> via
/// <see cref="TopicCoverageMapper"/>. Aufruf:
/// <c>coverage-topics &lt;phase&gt; &lt;runId&gt; [artefakt.md] [judge] [transkript.txt]</c>.
/// </summary>
public static class CoverageTopicsRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: coverage-topics <phase> <runId> [artifactFileName] [judgeModelOverride] [transcript.txt]");
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
            Console.Error.WriteLine($"[coverage-topics] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        // Eingefrorene Topic-Fixture laden (per Transkript-Basisname).
        var transcriptBase = Path.GetFileNameWithoutExtension(transcriptArg ?? FirstTranscript(repoRoot) ?? "");
        var fixturePath = Path.Combine(repoRoot, "input", "topics", $"{transcriptBase}.topics.json");
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[coverage-topics] Topic-Fixture fehlt: {Path.GetRelativePath(repoRoot, fixturePath)} — zuerst 'extract-topics' laufen lassen.");
            return 2;
        }
        var topicSet = JsonSerializer.Deserialize<TopicSet>(await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), ReviewJson.Options);
        if (topicSet is null || topicSet.Topics.Count == 0)
        {
            Console.Error.WriteLine($"[coverage-topics] Fixture leer/ungültig: {Path.GetRelativePath(repoRoot, fixturePath)}");
            return 2;
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var classifier = new TopicCoverageClassifier(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        var outDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_units");
        Directory.CreateDirectory(outDir);

        var artifacts = artifactArg is not null ? new[] { artifactArg } : DefaultArtifacts;
        Console.WriteLine($"[coverage-topics] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)} ({topicSet.Topics.Count} Topics)  Judge: {judgeSettings.ModelId}");

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path)) { Console.WriteLine($"[coverage-topics] uebersprungen (fehlt): {artifact}"); continue; }

            var artifactText = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var artifactType = JuryCategoryProfile.ArtifactType(artifact);
            var relevant = topicSet.Topics
                .Where(t => t.RelevantFor.Any(r => string.Equals(r, artifactType, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var verdicts = await classifier.ClassifyAsync(artifactText, relevant, CancellationToken.None).ConfigureAwait(false);
            var review = TopicCoverageMapper.Map(artifact, artifactType, relevant, verdicts, judgeSettings.ModelId);

            var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.topic-coverage.{modelSlug}.review.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(review, ReviewJson.Options)).ConfigureAwait(false);

            var counts = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count());
            string c(string k) => counts.TryGetValue(k, out var n) ? n.ToString() : "0";
            Console.WriteLine(
                $"[coverage-topics] {artifact}: relevant={relevant.Count} -> missing={c("missing")} partial={c("partial")} " +
                $"covered={c("covered")} n/a={c("not_applicable")} | CoverageScore={review.Metrics.CoverageScore} " +
                $"rate={review.Metrics.MissingCoverageRate:0.00} status={review.Status} -> {Path.GetRelativePath(repoRoot, outFile)}");
            foreach (var v in verdicts.Where(v => v.Verdict == "missing"))
                Console.WriteLine($"      [missing] {v.TopicId}: {Truncate(v.Reason, 70)}");
        }

        return 0;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    private static string? FirstTranscript(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "input", "transcripts");
        if (!Directory.Exists(dir)) return null;
        var f = Directory.GetFiles(dir, "*.txt").OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
        return f is null ? null : Path.GetFileName(f);
    }
}
