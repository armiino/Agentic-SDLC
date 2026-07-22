using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Z7 (DISK-COV-2): LLM-freier Topic-Fixture-Audit. Formale Checks + Jury-Cross-Check
/// (Capture-Recapture: Topic-Extraktor ∩ open-ended Jury-MISSING) → markiert Befunde + Completeness-
/// Kandidaten und schreibt <c>input/topics/&lt;base&gt;.audit.json</c>.
/// Aufruf: <c>topic-audit [transkript.txt] [phase] [runId]</c> (phase+runId = Jury-Quelle für den Cross-Check).
/// </summary>
public static class TopicAuditRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static Task<int> RunAsync(string[] args, string repoRoot)
    {
        string? transcriptArg = null;
        var positional = new List<string>();
        foreach (var a in args.Skip(1))
        {
            if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else positional.Add(a);
        }
        var phase = positional.Count >= 1 ? positional[0] : null;
        var runId = positional.Count >= 2 ? positional[1] : null;

        var (transcript, name) = LoadTranscript(repoRoot, transcriptArg);
        if (transcript is null || name is null)
        {
            Console.Error.WriteLine("[topic-audit] Kein Transkript gefunden.");
            return Task.FromResult(2);
        }

        var fixturePath = Path.Combine(repoRoot, "input", "topics", $"{Path.GetFileNameWithoutExtension(name)}.topics.json");
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[topic-audit] Fixture fehlt: {Path.GetRelativePath(repoRoot, fixturePath)} — zuerst 'extract-topics'.");
            return Task.FromResult(2);
        }
        var set = JsonSerializer.Deserialize<TopicSet>(File.ReadAllText(fixturePath), ReviewJson.Options);
        if (set is null || set.Topics.Count == 0)
        {
            Console.Error.WriteLine("[topic-audit] Fixture leer/ungültig.");
            return Task.FromResult(2);
        }

        var turnCount = TranscriptSegmenter.Segment(transcript).Count;

        // (1) Formale Checks
        var issues = TopicAudit.CheckFormal(set, turnCount);

        Console.WriteLine($"[topic-audit] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)} ({set.Topics.Count} Topics, {turnCount} Turns)");
        Console.WriteLine($"[topic-audit][formal] {issues.Count} Befund(e)" + (issues.Count == 0 ? " — sauber." : ":"));
        foreach (var i in issues)
            Console.WriteLine($"      {i.Code}  {i.TopicId}: {i.Detail}");

        // (2) Jury-Cross-Check (Capture-Recapture), nur wenn phase+runId gegeben
        IReadOnlyList<CrossCheckHit> hits = Array.Empty<CrossCheckHit>();
        var juryMissing = new List<string>();
        if (phase is not null && runId is not null)
        {
            juryMissing = ReadJuryMissing(repoRoot, phase, runId);
            hits = TopicAudit.CrossCheckJuryMissing(set.Topics, juryMissing);
            var candidates = hits.Where(h => h.CompletenessCandidate).ToList();
            var matched = hits.Count - candidates.Count;
            Console.WriteLine($"[topic-audit][cross-check] Quelle runs/{phase}/{runId}: {juryMissing.Count} Jury-MISSING-Funde -> {matched} matchen ein Topic, {candidates.Count} Completeness-Kandidaten");
            foreach (var c in candidates)
                Console.WriteLine($"      KANDIDAT (kein Topic): {Truncate(c.JuryText, 95)}");
        }
        else
        {
            Console.WriteLine("[topic-audit][cross-check] übersprungen (kein phase/runId) — für Completeness-Check: topic-audit <transkript.txt> <phase> <runId>");
        }

        var report = new
        {
            transcript = name,
            topicCount = set.Topics.Count,
            turnCount,
            formal = new { issueCount = issues.Count, issues },
            crossCheck = new
            {
                source = phase is not null && runId is not null ? $"{phase}/{runId}" : null,
                juryMissingCount = juryMissing.Count,
                matched = hits.Count(h => !h.CompletenessCandidate),
                completenessCandidates = hits.Where(h => h.CompletenessCandidate).Select(h => h.JuryText).ToList(),
                hits = hits.Select(h => new { h.JuryText, h.BestTopicId, score = Math.Round(h.Score, 2), h.CompletenessCandidate })
            }
        };
        var outFile = Path.Combine(repoRoot, "input", "topics", $"{Path.GetFileNameWithoutExtension(name)}.audit.json");
        File.WriteAllText(outFile, JsonSerializer.Serialize(report, JsonOptions));
        Console.WriteLine($"[topic-audit] Report -> {Path.GetRelativePath(repoRoot, outFile)}");
        return Task.FromResult(0);
    }

    /// <summary>
    /// Liest die ECHTEN MISSING_TOPIC-Findings aller jury/*.evaluator.json eines Runs: ohne VERWORFEN
    /// (rejected) und ohne HERABGESTUFT (partial), Boilerplate-Präfix entfernt (nur ab „Transkript:"),
    /// auf der Evidenz dedupliziert. So bekommt der Cross-Check sauberes Signal statt Mess-Rauschen.
    /// </summary>
    private static List<string> ReadJuryMissing(string repoRoot, string phase, string runId)
    {
        var juryDir = Path.Combine(repoRoot, "runs", phase, runId, "jury");
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        if (!Directory.Exists(juryDir)) return result;

        foreach (var f in Directory.GetFiles(juryDir, "*.evaluator.json"))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(f));
                if (!doc.RootElement.TryGetProperty("categories", out var cats) || cats.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var c in cats.EnumerateArray())
                {
                    if (!c.TryGetProperty("key", out var k) || k.GetString() != "MISSING_TOPIC") continue;
                    if (!c.TryGetProperty("findings", out var fs) || fs.ValueKind != JsonValueKind.Array) continue;
                    foreach (var find in fs.EnumerateArray())
                    {
                        var msg = find.TryGetProperty("message", out var m) ? m.GetString() : null;
                        if (string.IsNullOrWhiteSpace(msg)) continue;
                        // Nur echte MISSING: rejected + partial (downgraded) raus.
                        if (msg.StartsWith("VERWORFEN", StringComparison.Ordinal) ||
                            msg.StartsWith("HERABGESTUFT", StringComparison.Ordinal)) continue;
                        // Boilerplate-Präfix („[SEV] Artefakt: … |") entfernen → ab „Transkript:".
                        var idx = msg.IndexOf("Transkript:", StringComparison.Ordinal);
                        var evidence = (idx >= 0 ? msg[(idx + "Transkript:".Length)..] : msg).Trim();
                        var key = (evidence.Length > 80 ? evidence[..80] : evidence).ToLowerInvariant();
                        if (seen.Add(key)) result.Add(evidence);
                    }
                }
            }
            catch (JsonException) { /* defekte Datei überspringen */ }
        }
        return result;
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
