using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z8 (DISK-COV-5): begrenzter Topic-Completeness-Verifier. EIN LLM-Pass über Transkript + bestehende
/// Fixture → <c>input/topics/&lt;base&gt;.verify-candidates.json</c> (nur Kandidaten, nie Auto-Merge).
/// Aufruf: <c>dotnet run -- topic-verify [transkript.txt] [verifyModelOverride]</c>.
/// Schärfung 1 (todos2 §2e): das Verify-Modell sollte sich vom Extraktions-Modell unterscheiden, sonst
/// korrelierte blinde Flecken → der Runner WARNT, wenn beide gleich sind. Stop-Regel: GENAU EIN Pass.
/// </summary>
public static class TopicVerifyRunner
{
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? transcriptArg = null, verifyArg = null;
        foreach (var a in args.Skip(1))
        {
            if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else verifyArg = a;
        }

        var (transcript, name) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || name is null)
        {
            Console.Error.WriteLine(transcriptArg is null
                ? "[topic-verify] Kein Transkript unter input/transcripts/ gefunden."
                : $"[topic-verify] Transkript nicht gefunden: {transcriptArg}");
            return 2;
        }

        var fixturePath = Path.Combine(repoRoot, "input", "topics", $"{Path.GetFileNameWithoutExtension(name)}.topics.json");
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[topic-verify] Fixture fehlt: {Path.GetRelativePath(repoRoot, fixturePath)} — zuerst 'extract-topics'.");
            return 2;
        }
        var set = JsonSerializer.Deserialize<TopicSet>(await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), ReviewJson.Options);
        if (set is null || set.Topics.Count == 0)
        {
            Console.Error.WriteLine("[topic-verify] Fixture leer/ungültig.");
            return 2;
        }

        var turns = TranscriptSegmenter.Segment(transcript);

        var verifySettings =
            verifyArg is not null ? settings with { ModelId = verifyArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        Console.WriteLine($"[topic-verify] Transkript: {name} ({turns.Count} Turns), Fixture: {set.Topics.Count} Topics");
        Console.WriteLine($"[topic-verify] Extract-Modell: {set.Model}  |  Verify-Modell: {verifySettings.LlmProvider} / {verifySettings.ModelId}");
        if (string.Equals(set.Model, verifySettings.ModelId, StringComparison.OrdinalIgnoreCase))
            Console.WriteLine("[topic-verify] WARN: Verify-Modell == Extract-Modell → korrelierte blinde Flecken möglich (DISK-COV-5 Schärfung 1: anderes Modell wählen).");

        var verifier = new TopicVerifier(ChatClientFactory.Create(verifySettings), settings.JuryStructuredOutput);
        var candidates = await verifier.VerifyAsync(turns, set.Topics, CancellationToken.None).ConfigureAwait(false);

        var result = new TopicVerifyResult(name, set.Model, verifySettings.ModelId, candidates);
        var outFile = Path.Combine(repoRoot, "input", "topics", $"{Path.GetFileNameWithoutExtension(name)}.verify-candidates.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(result, ReviewJson.Options)).ConfigureAwait(false);

        Console.WriteLine($"[topic-verify] {candidates.Count} Completeness-Kandidat(en) -> {Path.GetRelativePath(repoRoot, outFile)}");
        foreach (var c in candidates)
            Console.WriteLine($"      KANDIDAT [{string.Join(",", c.SuggestedRelevantFor)}] {Truncate(c.Summary, 80)}  (turns: {string.Join(",", c.SourceTurns.Take(6))})");
        if (candidates.Count == 0)
            Console.WriteLine("[topic-verify] Keine Kandidaten — Fixture wirkt vollständig (kein Beweis; open-world).");
        else if (candidates.Count > 8)
            Console.WriteLine("[topic-verify] HINWEIS: viele Kandidaten (>8) → evtl. zu fein; Prompt/Schwelle prüfen, Kandidaten bleiben markiert (kein Auto-Merge).");

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
