using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// C1: steckt die besten Review-Achsen (Grounding + Coverage) zu EINEM <see cref="ReviewResult"/> pro
/// Artefakt zusammen — die erste aufrufbare Review-Einheit, offline. Lädt Run-Snapshot + Transkript +
/// eingefrorene Topic-Fixture, baut die Achsen, ruft den <see cref="ReviewService"/> und schreibt
/// <c>jury/_review/&lt;base&gt;.review.&lt;model&gt;.json</c> + die <see cref="GatePolicy"/>-Entscheidung.
/// </summary>
/// <remarks>
/// Aufruf: <c>review &lt;phase&gt; &lt;runId&gt; [artefakt.md] [judge] [transkript.txt]</c>.
/// Beide Achsen teilen sich hier denselben Judge (C1-Vereinfachung; getrennte Modelle später möglich).
/// Fehlt die Topic-Fixture, läuft der Review mit der Grounding-Achse allein (mit WARN). Basis für
/// EXP-REVIEW-1 (3 Runs × 2 Transkripte) und Vorform der späteren Inline-Kette (architektur-zwei-welten.md).
/// </remarks>
public static class ReviewRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: review <phase> <runId> [artifactFileName] [judgeModelOverride] [transcript.txt]");
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
            Console.Error.WriteLine($"[review] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var (transcript, transcriptName) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || transcriptName is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[review] Kein Transkript unter input/transcripts/ gefunden."
                : $"[review] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }

        // Eingefrorene Topic-Fixture (Coverage-Achse). Fehlt sie → Review nur mit Grounding (mit WARN).
        var transcriptBase = Path.GetFileNameWithoutExtension(transcriptName);
        var fixturePath = Path.Combine(repoRoot, "input", "topics", $"{transcriptBase}.topics.json");
        IReadOnlyList<TopicItem> topics = Array.Empty<TopicItem>();
        if (File.Exists(fixturePath))
        {
            var set = JsonSerializer.Deserialize<TopicSet>(
                await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), ReviewJson.Options);
            if (set is not null) topics = set.Topics;
        }
        else
        {
            Console.WriteLine($"[review] WARN: keine Topic-Fixture ({Path.GetRelativePath(repoRoot, fixturePath)}) → nur Grounding-Achse.");
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var client = ChatClientFactory.Create(judgeSettings);
        var axes = new List<IReviewAxis>
        {
            new GroundingAxis(new UnitClassifier(client, settings.JuryStructuredOutput), judgeSettings.ModelId)
        };
        if (topics.Count > 0)
            axes.Add(new CoverageAxis(new TopicCoverageClassifier(client, settings.JuryStructuredOutput), judgeSettings.ModelId));

        var service = new ReviewService(axes);
        var gate = new GatePolicy();

        var outDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_review");
        Directory.CreateDirectory(outDir);

        var artifacts = artifactArg is not null ? new[] { artifactArg } : DefaultArtifacts;
        Console.WriteLine(
            $"[review] Transkript: {transcriptName}  Fixture: {topics.Count} Topics  Judge: {judgeSettings.ModelId}  " +
            $"Achsen: {string.Join("+", axes.Select(a => a.Name))}");

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path)) { Console.WriteLine($"[review] uebersprungen (fehlt): {artifact}"); continue; }

            var artifactText = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var artifactType = JuryCategoryProfile.ArtifactType(artifact);
            var subject = new ReviewSubject(artifact, artifactType, artifactText, transcript, topics);

            var result = await service.ReviewAsync(subject, CancellationToken.None).ConfigureAwait(false);
            var decision = gate.Evaluate(result);

            var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.review.{modelSlug}.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(result, ReviewJson.Options)).ConfigureAwait(false);

            // Gate-Entscheidung als EIGENE Datei persistieren (statt nur Konsole) → direkt auswert-/zitierbar
            // (Decision/PolicyVersion/Reasons), ohne das ReviewResult-Format zu verändern.
            var gateFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.gate.{modelSlug}.json");
            await File.WriteAllTextAsync(gateFile, JsonSerializer.Serialize(decision, ReviewJson.Options)).ConfigureAwait(false);

            Console.WriteLine(
                $"[review] {artifact}: status={result.Status} axes=[{string.Join(",", result.EvaluatedAxes)}] " +
                $"grounding={result.Metrics.GroundingScore?.ToString() ?? "-"} coverage={result.Metrics.CoverageScore?.ToString() ?? "-"} " +
                $"defects={result.Defects.Count} (crit={result.Metrics.CriticalDefectCount}) gate={decision.Decision} " +
                $"-> {Path.GetRelativePath(repoRoot, outFile)} (+ .gate.{modelSlug}.json)");
        }

        return 0;
    }

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
