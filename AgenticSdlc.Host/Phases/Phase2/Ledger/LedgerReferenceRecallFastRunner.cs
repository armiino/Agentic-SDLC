using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// <b>Deterministischer</b> (kein LLM) Recall-Screen: findet der Auto-Ledger die Referenz-Claims?
/// Beide Ledger zitieren Evidence aus DEMSELBEN Transkript → ein Referenz-Eintrag gilt als „abgedeckt",
/// wenn ein Auto-Eintrag Evidence im selben Transkript-Segment hat (Segment-Overlap).
/// </summary>
/// <remarks>
/// Gratis + reproduzierbar; ersetzt die teure 1-LLM-Call-pro-Eintrag-Variante für die reine Recall-Frage.
/// Ehrliche Grenze: Segment-Overlap **überschätzt** Recall (ein Segment kann mehrere Claims enthalten) →
/// „covered" ist eine OBERGRENZE. Verlässlich sind die **Misses**: ein Referenz-Eintrag, dessen Segmente
/// KEIN Auto-Eintrag berührt, ist ein echter Recall-Miss (Untergrenze für Misses). Facetten werden NICHT
/// bewertet (das ist L3, auf kleiner Menge). Strittige „covered" ggf. separat mit günstigem Modell prüfen.
/// Befehl: <c>ledger-reference-recall-fast &lt;transcript.txt&gt; &lt;reference.json&gt; &lt;auto-ledger.json&gt; [out.json]</c>.
/// </remarks>
public static class LedgerReferenceRecallFastRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly Regex WhitespaceRx = new(@"\s+", RegexOptions.Compiled);
    // Strippt den Sprecher-Prefix eines Evidence-Quotes, damit der Resttext gegen den (bereits
    // sprecher-freien) Turn-Text substring-matchen kann. Deckt beide beobachteten Formate ab:
    //   [Speaker 2]: …            (bracketed, wie Transkript/Referenz)
    //   Speaker 2: … / Anna: … / Ben Müller: …  (unbracketed; die Atomic-Unit-Pipeline rendert den
    //                                             Prefix OHNE Klammern -> sonst poisont "speaker 2:" das Fragment)
    // Der unbracketed-Zweig erlaubt neben dem ersten Großwort optionale weitere Groß-/Ziffern-Tokens
    // (Namen mehrwortig, "Speaker <n>"). Bekannter, seltener + symmetrischer Rand: ein Quote, der mit
    // zwei Großwörtern + Doppelpunkt BEGINNT ("Die App:") verliert diese – bei Referenz UND Auto gleich.
    private static readonly Regex SpeakerPrefixRx = new(@"^\s*(\[[^\]]*\]|\p{Lu}[\p{L}]*(?:\s+[\p{Lu}\p{N}][\p{L}\p{N}]*)*)\s*:?\s*", RegexOptions.Compiled);

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: ledger-reference-recall-fast <transcript.txt> <reference.json> <auto-ledger.json> [out.json]");
            return Task.FromResult(2);
        }

        var transcriptPath = Resolve(repoRoot, args[1]);
        var refPath = Resolve(repoRoot, args[2]);
        var autoPath = Resolve(repoRoot, args[3]);
        foreach (var (label, p) in new[] { ("Transkript", transcriptPath), ("Referenz", refPath), ("Auto-Ledger", autoPath) })
            if (!File.Exists(p)) { Console.Error.WriteLine($"[recall-fast] {label} fehlt: {p}"); return Task.FromResult(2); }

        var outPath = args.Length >= 5
            ? Resolve(repoRoot, args[4])
            : Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike",
                $"ledger-reference-recall-fast.{Path.GetFileNameWithoutExtension(refPath)}.json");

        var turns = TranscriptSegmenter.Segment(File.ReadAllText(transcriptPath));
        var normTurns = turns.Select(t => Norm(t.Text)).ToArray();
        // kind=="meta" sind Facilitator-/Output-Anweisungen, KEINE Quell-Claims -> aus der Recall-Messung
        // ausgenommen (sonst zählen sie fälschlich als Miss).
        var allReference = LoadEntries(refPath);
        var reference = allReference.Where(e => !string.Equals(e.Kind?.Trim(), "meta", StringComparison.OrdinalIgnoreCase)).ToList();
        var skippedMeta = allReference.Count - reference.Count;
        var auto = LoadEntries(autoPath);
        if (reference.Count == 0) { Console.Error.WriteLine("[recall-fast] leere Referenz."); return Task.FromResult(2); }

        // Auto-Segmente einmal vorab: welche Turns sind durch IRGENDEINEN Auto-Eintrag belegt.
        var autoTurnSets = auto.Select(e => SegmentsOf(e, normTurns)).ToArray();

        var covered = 0;
        var missedEntries = new List<object>();
        var perEntry = new List<object>();

        foreach (var r in reference)
        {
            var refSegs = SegmentsOf(r, normTurns);
            var hitAutoIds = new List<string>();
            for (var i = 0; i < auto.Count; i++)
                if (refSegs.Count > 0 && refSegs.Overlaps(autoTurnSets[i])) hitAutoIds.Add(auto[i].Id);

            var isCovered = hitAutoIds.Count > 0;
            if (isCovered) covered++;
            else missedEntries.Add(new { id = r.Id, proposition = r.Proposition, status = r.Status, modality = r.Modality, scope = r.Scope, segments = refSegs.OrderBy(x => x).ToArray() });

            perEntry.Add(new { id = r.Id, covered = isCovered, refSegments = refSegs.OrderBy(x => x).ToArray(), matchedAutoIds = hitAutoIds });
        }

        var missed = reference.Count - covered;
        var noSegRef = reference.Count(r => SegmentsOf(r, normTurns).Count == 0);

        var payload = new
        {
            mode = "ledger-reference-recall-fast (deterministic segment-overlap, KEIN LLM)",
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            reference = Path.GetRelativePath(repoRoot, refPath),
            autoLedger = Path.GetRelativePath(repoRoot, autoPath),
            referenceCount = reference.Count,
            skippedMetaEntries = skippedMeta,
            autoCount = auto.Count,
            note = "covered = OBERGRENZE (Segment-Overlap überschätzt); missed = verlässliche Recall-Misses. kind=meta ausgenommen. Keine Facettenbewertung.",
            metrics = new
            {
                covered,
                missed,
                recallUpperBound = Math.Round(covered / (double)reference.Count, 4),
                referenceEntriesWithoutSegmentMatch = noSegRef
            },
            missedEntries,
            perEntry
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, JsonSerializer.Serialize(payload, Json));

        Console.WriteLine($"[recall-fast] reference={reference.Count} (meta ausgenommen: {skippedMeta}) auto={auto.Count} segments={turns.Count}");
        Console.WriteLine($"[recall-fast] covered(Obergrenze)={covered}/{reference.Count}  missed(verlässlich)={missed}/{reference.Count}  recallUpperBound={Math.Round(covered / (double)reference.Count, 4)}");
        if (noSegRef > 0) Console.WriteLine($"[recall-fast] HINWEIS: {noSegRef} Referenz-Einträge ohne Segment-Match (paraphrasierte Evidence) -> als missed gezählt, manuell prüfen.");
        Console.WriteLine($"[recall-fast] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return Task.FromResult(0);
    }

    /// <summary>Turn-Indizes, in denen ein Eintrag laut seiner Evidence-Quotes verankert ist.</summary>
    private static HashSet<int> SegmentsOf(SemanticLedgerEntry e, string[] normTurns)
    {
        var segs = new HashSet<int>();
        foreach (var minLen in new[] { 20, 10 })
        {
            foreach (var ev in e.Evidence)
            {
                var q = SpeakerPrefixRx.Replace(ev.Quote ?? "", "").Replace("…", "...");
                foreach (var frag in q.Split("...", StringSplitOptions.RemoveEmptyEntries))
                {
                    var nf = Norm(frag);
                    if (nf.Length < minLen) continue;
                    for (var i = 0; i < normTurns.Length; i++)
                        if (normTurns[i].Contains(nf)) segs.Add(i);
                }
            }
            if (segs.Count > 0) break;
        }
        return segs;
    }

    private static IReadOnlyList<SemanticLedgerEntry> LoadEntries(string path)
    {
        var text = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.ValueKind == JsonValueKind.Array
            ? JsonSerializer.Deserialize<List<SemanticLedgerEntry>>(text, Json) ?? []
            : JsonSerializer.Deserialize<SemanticLedgerFixture>(text, Json)?.Entries ?? [];
    }

    private static string Norm(string s) => WhitespaceRx.Replace(s ?? "", " ").Trim().ToLowerInvariant();
    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
