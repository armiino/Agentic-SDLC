namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>Ein formaler Befund am Topic (deterministisch, kein LLM).</summary>
public sealed record TopicFormalIssue(string TopicId, string Code, string Detail);

/// <summary>
/// Ergebnis des Jury-Cross-Checks (Capture-Recapture-Prinzip): ein Jury-MISSING-Fund + bestes Topic-Match.
/// <see cref="CompletenessCandidate"/> = kein Topic matcht hinreichend → das Raster hat das Thema evtl. übersehen.
/// </summary>
public sealed record CrossCheckHit(string JuryText, string? BestTopicId, double Score, bool CompletenessCandidate);

/// <summary>
/// Z7 (DISK-COV-2): deterministischer Topic-Fixture-Audit (kein LLM). Zwei Bausteine:
/// (1) formale Checks der Fixture; (2) Jury-Cross-Check = Capture-Recapture mit Quelle 1 (Topic-Extraktor)
/// ∩ Quelle 2 (open-ended Jury-MISSING_TOPIC). Jury-MISSING ohne passendes Topic = Completeness-Kandidat.
/// Reine, unit-testbare Funktionen; der Runner liest Fixture/Transkript/Jury und schreibt den Report.
/// </summary>
public static class TopicAudit
{
    private static readonly string[] ValidArtifactTypes = { "requirements", "risks", "architecture", "open-questions" };
    private static readonly string[] ValidStatus = { "resolved", "unresolved", "decision_open" };

    /// <summary>Formale Prüfung: IDs eindeutig/nicht-leer, summary, status, relevantFor, sourceTurns im Bereich.</summary>
    public static IReadOnlyList<TopicFormalIssue> CheckFormal(TopicSet set, int turnCount)
    {
        var issues = new List<TopicFormalIssue>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in set.Topics)
        {
            var id = t.TopicId ?? "?";
            if (string.IsNullOrWhiteSpace(t.TopicId)) issues.Add(new(id, "EMPTY_ID", "topicId leer"));
            else if (!seen.Add(t.TopicId)) issues.Add(new(id, "DUPLICATE_ID", "topicId doppelt"));

            if (string.IsNullOrWhiteSpace(t.Summary)) issues.Add(new(id, "EMPTY_SUMMARY", "summary leer"));

            if (!ValidStatus.Contains((t.Status ?? "").Trim().ToLowerInvariant()))
                issues.Add(new(id, "BAD_STATUS", $"status '{t.Status}'"));

            if (t.RelevantFor is null || t.RelevantFor.Count == 0)
                issues.Add(new(id, "EMPTY_RELEVANTFOR", "relevantFor leer"));
            else
                foreach (var r in t.RelevantFor)
                    if (!ValidArtifactTypes.Contains((r ?? "").Trim().ToLowerInvariant()))
                        issues.Add(new(id, "BAD_RELEVANTFOR", $"'{r}'"));

            if (t.SourceTurns is null || t.SourceTurns.Count == 0)
                issues.Add(new(id, "EMPTY_SOURCETURNS", "sourceTurns leer"));
            else
                foreach (var idx in t.SourceTurns)
                    if (idx < 0 || (turnCount > 0 && idx >= turnCount))
                        issues.Add(new(id, "SOURCETURN_OUT_OF_RANGE", $"turn {idx} (gültig 0..{turnCount - 1})"));
        }
        return issues;
    }

    /// <summary>
    /// Jury-Cross-Check: matcht jeden Jury-MISSING-Text per Token-Overlap gegen die Topic-Summaries.
    /// Heuristik → flaggt Kandidaten für menschliche Prüfung, kein Beweis.
    /// </summary>
    public static IReadOnlyList<CrossCheckHit> CrossCheckJuryMissing(
        IReadOnlyList<TopicItem> topics, IReadOnlyList<string> juryMissingTexts, double threshold = 0.30)
    {
        var topicTokens = topics.Select(t => (t.TopicId, Tokens: Tokenize($"{t.Summary} {t.TopicId}"))).ToList();
        var hits = new List<CrossCheckHit>();

        foreach (var jt in juryMissingTexts)
        {
            var jtok = Tokenize(jt);
            string? best = null;
            double bestScore = 0;
            foreach (var (id, toks) in topicTokens)
            {
                var score = Overlap(jtok, toks);
                if (score > bestScore) { bestScore = score; best = id; }
            }
            hits.Add(new CrossCheckHit(jt, bestScore >= threshold ? best : null, bestScore, bestScore < threshold));
        }
        return hits;
    }

    private static readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
    {
        "nicht","vorhanden","wird","werden","sind","eine","einen","einem","oder","und","das","der","die",
        "den","dem","für","mit","von","auf","aber","auch","noch","sowie","bzw","etc","artefakt","topic",
        "thema","transkript","brauchen","haben","keinen","sollen","muss","müssen"
    };

    // Auf JEDES Nicht-Buchstaben/Ziffer-Zeichen splitten — fängt auch Unicode-Bindestriche (‑),
    // typografische Quotes („ " ") etc., die ein fester Zeichensatz übersieht.
    private static HashSet<string> Tokenize(string s) =>
        System.Text.RegularExpressions.Regex.Split(s.ToLowerInvariant(), @"[^\p{L}\p{N}]+")
        .Where(w => w.Length >= 4 && !Stop.Contains(w))
        .ToHashSet();

    // Overlap-Koeffizient (Schnitt / kleinere Menge): toleranter als Jaccard für kurze Summaries.
    private static double Overlap(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        var inter = a.Count(x => b.Contains(x));
        return (double)inter / Math.Min(a.Count, b.Count);
    }
}
