using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// D1 / R0 (DirectTranscriptReview): bewertet Run-Artefakte **direkt gegen das Roh-Transkript** über die
/// <see cref="DirectReviewAxis"/> — OHNE Topic-Fixture, sonst identisch zum <c>review</c>-Pfad (gleicher
/// <see cref="ReviewService"/> + <see cref="GatePolicy"/> + Vertrag). Output landet im kuratierten
/// Thesis-Ordner <c>thesis-evidence/D1-direct-vs-topic/</c>. Aufruf:
/// <c>review-direct &lt;phase&gt; &lt;runId&gt; [artefakt.md] [judge] [transkript.txt]</c>.
/// Vergleichsgegner R1 = der bestehende <c>review</c>/<c>coverage-topics</c>-Pfad. Plan: D1-direct-vs-topic-plan.md.
/// </summary>
public static class ReviewDirectRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: review-direct <phase> <runId> [artifactFileName] [judgeModelOverride] [transcript.txt]");
            return 2;
        }

        var phase = args[1];
        var runId = args[2];
        string? artifactArg = null, judgeArg = null, transcriptArg = null;
        int? runIndex = null;
        foreach (var a in args.Skip(3))
        {
            if (a.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) artifactArg = a;
            else if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else if (TryParseRunIndex(a, out var ri)) runIndex = ri;   // Stabilitäts-Wiederholung: run2, run3, … (oder bare Zahl)
            else judgeArg = a;
        }
        var runTag = runIndex is null ? "" : $".run{runIndex}";   // leer = Lauf ohne Index (rückwärtskompatibel)

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[review-direct] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var (transcript, transcriptName) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || transcriptName is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[review-direct] Kein Transkript unter input/transcripts/ gefunden."
                : $"[review-direct] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var client = ChatClientFactory.Create(judgeSettings);
        var service = new ReviewService(new IReviewAxis[]
        {
            new DirectReviewAxis(new DirectReviewClassifier(client, settings.JuryStructuredOutput), judgeSettings.ModelId)
        });
        var gate = new GatePolicy();

        // Kuratierter Thesis-Evidence-Ordner (getrackt, getrennt vom runs/-Baum).
        var outDir = Path.Combine(repoRoot, "thesis-evidence", "D1-direct-vs-topic");
        Directory.CreateDirectory(outDir);

        var artifacts = artifactArg is not null ? new[] { artifactArg } : DefaultArtifacts;
        Console.WriteLine(
            $"[review-direct] R0 DirectTranscriptReview{(runIndex is null ? "" : $" (run {runIndex})")}  " +
            $"Run: {phase}/{runId}  Transkript: {transcriptName}  Judge: {judgeSettings.ModelId}");

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path)) { Console.WriteLine($"[review-direct] uebersprungen (fehlt): {artifact}"); continue; }

            var artifactText = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            var artifactType = JuryCategoryProfile.ArtifactType(artifact);
            var subject = new ReviewSubject(artifact, artifactType, artifactText, transcript, Array.Empty<TopicItem>());

            var result = await service.ReviewAsync(subject, CancellationToken.None).ConfigureAwait(false);
            var decision = gate.Evaluate(result);

            var baseName = Path.GetFileNameWithoutExtension(artifact);
            var outFile = Path.Combine(outDir, $"{baseName}.direct.{modelSlug}{runTag}.review.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(result, ReviewJson.Options)).ConfigureAwait(false);
            var gateFile = Path.Combine(outDir, $"{baseName}.direct.{modelSlug}{runTag}.gate.json");
            await File.WriteAllTextAsync(gateFile, JsonSerializer.Serialize(decision, ReviewJson.Options)).ConfigureAwait(false);

            var missing = result.Defects.Count(d => d.Axis == ReviewAxis.Coverage);
            var grounding = result.Defects.Count(d => d.Axis == ReviewAxis.Grounding);
            Console.WriteLine(
                $"[review-direct] {artifact}: status={result.Status} missing={missing} false_claim={grounding} " +
                $"defects={result.Defects.Count} (crit={result.Metrics.CriticalDefectCount}) gate={decision.Decision} " +
                $"-> {Path.GetRelativePath(repoRoot, outFile)}");
            foreach (var d in result.Defects.Where(d => d.Axis == ReviewAxis.Coverage))
                Console.WriteLine($"      [missing] {Truncate(d.Description, 80)}");
        }

        Console.WriteLine("[review-direct] Hinweis: R1 (TopicCoverage) = die .review/.topic-coverage-Dateien des review/coverage-topics-Pfads.");
        return 0;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    /// <summary>Erkennt einen Stabilitäts-Wiederholungs-Index: <c>run2</c>/<c>run3</c>/… oder eine reine Zahl.</summary>
    private static bool TryParseRunIndex(string a, out int idx)
    {
        if (a.StartsWith("run", StringComparison.OrdinalIgnoreCase) && int.TryParse(a.AsSpan(3), out idx)) return true;
        return int.TryParse(a, out idx);
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
