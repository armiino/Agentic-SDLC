using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z6.2 (DISK-COV V1): Extrahiert die Topics eines Transkripts EINMAL und friert sie als Fixture
/// <c>input/topics/&lt;transkript&gt;.topics.json</c> ein (wird über Artefakte/Runs wiederverwendet).
/// Ein LLM-Call. Aufruf: <c>dotnet run -- extract-topics [transkript.txt] [judgeModelOverride]</c>.
/// </summary>
public static class ExtractTopicsRunner
{
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? transcriptArg = null, judgeArg = null;
        foreach (var a in args.Skip(1))
        {
            if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else judgeArg = a;
        }

        var (transcript, name) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || name is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[extract-topics] Kein Transkript unter input/transcripts/ gefunden."
                : $"[extract-topics] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }

        var turns = TranscriptSegmenter.Segment(transcript);

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        Console.WriteLine($"[extract-topics] Transkript: {name} ({turns.Count} Turns)");
        Console.WriteLine($"[extract-topics] Judge: {judgeSettings.LlmProvider} / {judgeSettings.ModelId}");

        var extractor = new TopicExtractor(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
        var topics = await extractor.ExtractAsync(turns, CancellationToken.None).ConfigureAwait(false);

        var set = new Review.TopicSet(name, judgeSettings.ModelId, topics);
        var outDir = Path.Combine(repoRoot, "input", "topics");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(name)}.topics.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(set, Review.ReviewJson.Options)).ConfigureAwait(false);

        var byArtifact = topics
            .SelectMany(t => t.RelevantFor)
            .GroupBy(a => a)
            .ToDictionary(g => g.Key, g => g.Count());
        Console.WriteLine($"[extract-topics] {topics.Count} Topics -> {Path.GetRelativePath(repoRoot, outFile)}");
        Console.WriteLine($"[extract-topics] relevantFor: {string.Join("  ", byArtifact.Select(kv => $"{kv.Key}={kv.Value}"))}");
        foreach (var t in topics.Take(8))
            Console.WriteLine($"      {t.TopicId} [{t.Status}] [{string.Join(",", t.RelevantFor)}] {Truncate(t.Summary, 70)}");
        if (topics.Count == 0)
            Console.WriteLine("[extract-topics] WARN: 0 Topics — Antwort nicht parsebar oder leer.");

        return 0;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    private static (string? Content, string? Name) LoadTranscript(string repoRoot, string? transcriptName)
    {
        var dir = Path.Combine(repoRoot, "input", "transcripts");
        if (!Directory.Exists(dir)) return (null, null);

        if (transcriptName is not null)
        {
            var p = Path.Combine(dir, transcriptName);
            return File.Exists(p) ? (File.ReadAllText(p), transcriptName) : (null, null);
        }

        var f = Directory.GetFiles(dir, "*.txt").OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
        return f is null ? (null, null) : (File.ReadAllText(f), Path.GetFileName(f));
    }
}
