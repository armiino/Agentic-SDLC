using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// DISK-14 Phase 2: Offline-Kommando, das die deterministisch geparsten Einheiten eines Artefakts
/// per Per-Item-Klassifikation gegen das Transkript bewertet und einen PARALLELEN
/// <c>GroundingScore</c> schreibt (NICHT den alten ErrorScore überschreiben).
/// </summary>
/// <remarks>
/// Aufruf: <c>dotnet run -- classify-units &lt;phase&gt; &lt;runId&gt; [artefakt.md] [judgeModelOverride]</c>.
/// Vollständig additiv + reversibel: nutzt nur den isolierten <see cref="ArtifactUnitParser"/> +
/// <see cref="UnitClassifier"/>; kein Eingriff in Evaluator/Phase2JuryRunner. Verwerfen = PerItem-Ordner
/// + die zwei Dispatch-Zeilen (parse-units/classify-units) löschen.
/// </remarks>
public static class ClassifyUnitsRunner
{
    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: classify-units <phase> <runId> [artifactFileName] [judgeModelOverride]");
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
            Console.Error.WriteLine($"[classify-units] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var transcript = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[classify-units] Kein Transkript unter input/transcripts/ gefunden."
                : $"[classify-units] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }
        Console.WriteLine($"[classify-units] Transkript: {transcriptArg ?? "(erstes alphabetisch)"}");

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var client = ChatClientFactory.Create(judgeSettings);
        var classifier = new UnitClassifier(client, settings.JuryStructuredOutput);

        var outDir = Path.Combine(repoRoot, "runs", phase, runId, "jury", "_units");
        Directory.CreateDirectory(outDir);

        var artifacts = artifactArg is not null ? [artifactArg] : DefaultArtifacts;
        Console.WriteLine($"[classify-units] Judge: {judgeSettings.LlmProvider} / {judgeSettings.ModelId}  (chunk={UnitClassifier.ChunkSize})");

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path))
            {
                Console.WriteLine($"[classify-units] uebersprungen (fehlt): {artifact}");
                continue;
            }

            var units = ArtifactUnitParser.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
            var artifactType = JuryCategoryProfile.ArtifactType(artifact);
            var verdicts = await classifier.ClassifyAsync(transcript, artifactType, units, CancellationToken.None)
                .ConfigureAwait(false);

            var byIndex = verdicts.ToDictionary(v => v.Index);
            var groundingScore = verdicts.Sum(v => UnitClassifier.Weight(v.Verdict));
            var counts = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count());

            var rows = units.Select(u =>
            {
                var v = byIndex[u.Index];
                return new { index = u.Index, section = u.Section, kind = u.Kind, verdict = v.Verdict, reason = v.Reason, text = u.Text };
            }).ToList();

            var payload = new
            {
                artifact, phase, runId,
                judge = new { provider = judgeSettings.LlmProvider, model = judgeSettings.ModelId },
                unitCount = units.Count,
                groundingScore,
                counts,
                verdicts = rows
            };

            var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(artifact)}.grounding.{modelSlug}.json");
            await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

            var c = (string k) => counts.TryGetValue(k, out var n) ? n : 0;
            Console.WriteLine(
                $"[classify-units] {artifact}: GroundingScore={groundingScore}  " +
                $"grounded={c("grounded")} overstated={c("overstated")} fabricated={c("fabricated")} " +
                $"not_a_claim={c("not_a_claim")} unclassified={c("unclassified")} -> {Path.GetRelativePath(repoRoot, outFile)}");
            foreach (var v in verdicts.Where(v => v.Verdict is "overstated" or "fabricated"))
                Console.WriteLine($"      [{v.Verdict}] {Truncate(units[v.Index].Text, 80)}  ({Truncate(v.Reason, 70)})");
        }

        return 0;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    private static string? LoadTranscript(string repoRoot, string? transcriptName)
    {
        var dir = Path.Combine(repoRoot, "input", "transcripts");
        if (!Directory.Exists(dir)) return null;
        if (transcriptName is not null)
        {
            var explicitPath = Path.Combine(dir, transcriptName);
            return File.Exists(explicitPath) ? File.ReadAllText(explicitPath) : null;
        }
        var file = Directory.GetFiles(dir, "*.txt").OrderBy(f => f, StringComparer.Ordinal).FirstOrDefault();
        return file is null ? null : File.ReadAllText(file);
    }
}
