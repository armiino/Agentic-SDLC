using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using AgenticSdlc.Host.Prompts;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z10 / v02 (Call 2, separat): klassifiziert <c>relevantFor</c> je Topic einer bestehenden Fixture in
/// EINEM eigenen LLM-Call und legt das Ergebnis als SIDECAR
/// <c>input/topics/&lt;base&gt;.relevance.&lt;model&gt;.json</c> daneben — die eingefrorene
/// <c>&lt;base&gt;.topics.json</c> bleibt UNVERAENDERT (v01 = Fallback). Aufruf:
/// <c>dotnet run -- classify-topic-relevance [transkript.txt] [classifyModelOverride]</c>.
/// Zweck = Test B: v01-relevantFor (aus der Fixture) vs. v02-relevantFor (separat) fair vergleichen; der
/// Runner druckt direkt die per-Artefakt-Zaehler beider Welten + meldet Topics ohne Klassifikation.
/// </summary>
public static class ClassifyTopicRelevanceRunner
{
    private static readonly string[] ArtifactTypes = { "requirements", "risks", "architecture", "open-questions" };
    private const string PromptDir = "AgenticSdlc.Host/Prompts/topics";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? transcriptArg = null, classifyArg = null, promptToken = null;
        foreach (var a in args.Skip(1))
        {
            if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else if (string.Equals(a, "v01", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(a, "v02", StringComparison.OrdinalIgnoreCase)) promptToken = a.ToLowerInvariant();
            else classifyArg = a;
        }
        promptToken ??= "v01";                                   // Default = v01 (Fallback); v02 = geschärftes open-questions-Profil.
        var promptName = $"classify-topic-relevance-{promptToken}";

        var (transcript, name) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || name is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[classify-topic-relevance] Kein Transkript unter input/transcripts/ gefunden."
                : $"[classify-topic-relevance] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }

        var fixturePath = Path.Combine(repoRoot, "input", "topics", $"{Path.GetFileNameWithoutExtension(name)}.topics.json");
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[classify-topic-relevance] Fixture fehlt: {Path.GetRelativePath(repoRoot, fixturePath)} — zuerst 'extract-topics'.");
            return 2;
        }
        var set = JsonSerializer.Deserialize<TopicSet>(await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), ReviewJson.Options);
        if (set is null || set.Topics.Count == 0)
        {
            Console.Error.WriteLine("[classify-topic-relevance] Fixture leer/ungültig.");
            return 2;
        }

        var turns = TranscriptSegmenter.Segment(transcript);

        var classifySettings =
            classifyArg is not null ? settings with { ModelId = classifyArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        string systemPrompt;
        try
        {
            systemPrompt = PromptTemplateLoader.Load(repoRoot, PromptDir, promptName, new Dictionary<string, string>());
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"[classify-topic-relevance] Prompt nicht ladbar: {ex.Message}");
            return 2;
        }

        Console.WriteLine($"[classify-topic-relevance] Transkript: {name} ({turns.Count} Turns), Fixture: {set.Topics.Count} Topics");
        Console.WriteLine($"[classify-topic-relevance] Extract-Modell (v01): {set.Model}  |  Classify-Modell (v02): {classifySettings.LlmProvider} / {classifySettings.ModelId}");
        Console.WriteLine($"[classify-topic-relevance] Prompt: {promptName}");

        var classifier = new TopicRelevanceClassifier(
            ChatClientFactory.Create(classifySettings), systemPrompt, settings.JuryStructuredOutput);
        var classified = await classifier.ClassifyAsync(set.Topics, turns, CancellationToken.None).ConfigureAwait(false);

        var modelSlug = classifySettings.ModelId.Replace('/', '_').Replace(':', '_');
        var promptTag = promptToken == "v01" ? "" : $".{promptToken}";   // v01 = rückwärtskompatibler Dateiname; v02 = eigenes Sidecar.
        var result = new TopicRelevanceSet(name, set.Model, classifySettings.ModelId, promptName, classified);
        var outDir = Path.Combine(repoRoot, "input", "topics");
        var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(name)}.relevance.{modelSlug}{promptTag}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(result, ReviewJson.Options)).ConfigureAwait(false);

        Console.WriteLine($"[classify-topic-relevance] {classified.Count}/{set.Topics.Count} Topics klassifiziert -> {Path.GetRelativePath(repoRoot, outFile)}");

        // Direkter Test-B-Augenschein: per-Artefakt-Zaehler v01 (Fixture) vs. v02 (separat).
        var v01 = CountByArtifact(set.Topics.SelectMany(t => t.RelevantFor));
        var v02 = CountByArtifact(classified.SelectMany(t => t.RelevantFor));
        Console.WriteLine("[classify-topic-relevance] relevantFor je Artefakt (v01 -> v02):");
        foreach (var art in ArtifactTypes)
            Console.WriteLine($"      {art,-14} {v01.GetValueOrDefault(art),3} -> {v02.GetValueOrDefault(art),3}");

        // Ehrlichkeit: welche Fixture-Topics hat der Classifier nicht (per topicId) zurueckgegeben?
        var classifiedIds = classified.Select(t => t.TopicId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = set.Topics.Where(t => !classifiedIds.Contains(t.TopicId)).Select(t => t.TopicId).ToList();
        if (missing.Count > 0)
            Console.WriteLine($"[classify-topic-relevance] WARN: {missing.Count} Fixture-Topic(s) ohne Klassifikation (topicId-Echo fehlt): {string.Join(", ", missing.Take(8))}");
        if (classified.Count == 0)
            Console.WriteLine("[classify-topic-relevance] WARN: 0 klassifiziert — Antwort nicht parsebar oder leer.");

        return 0;
    }

    private static Dictionary<string, int> CountByArtifact(IEnumerable<string> rels)
        => rels.GroupBy(a => a).ToDictionary(g => g.Key, g => g.Count());

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
