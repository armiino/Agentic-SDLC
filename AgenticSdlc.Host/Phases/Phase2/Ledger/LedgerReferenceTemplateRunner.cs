using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Erzeugt eine <b>deterministische</b> Annotations-Vorlage zum Bau eines HAND-vollständigen Referenz-
/// Ledgers (für die Completeness-/Recall-Messung „fehlen wirklich Sachen aus dem Transkript?").
/// </summary>
/// <remarks>
/// KEIN LLM — bewusst, damit das Hilfsmittel keine Modell-Blindflecken importiert (Zirkularität: ein LLM
/// darf nicht die Ground Truth für einen LLM bauen; vgl. grounding-spotcheck „kein LLM ist Ground Truth").
/// Der Mensch adjudiziert segmentweise; dieses Tool reicht nur Last (Segmentierung + Kandidaten-Zuordnung)
/// und markiert Segmente OHNE zugeordnete Kandidaten als Haupt-Miss-Verdacht.
///
/// Ablauf: Transkript → Sprecher-Turns (TranscriptSegmenter) · Kandidaten-Ledger → je Eintrag dessen
/// Evidence-Quotes deterministisch (Substring-Fragment) den Turns zuordnen · Markdown-Vorlage rendern.
/// Status der späteren Referenz = „modell-entworfen (Kandidaten), AUTOR-adjudiziert", Single-Labeler (E5).
/// Befehl: <c>ledger-reference-template &lt;transcript.txt&gt; &lt;candidate-ledger.json&gt; [out.md]</c>.
/// </remarks>
public static class LedgerReferenceTemplateRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Regex WhitespaceRx = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex SpeakerPrefixRx = new(@"^\s*(\[[^\]]*\]|\p{Lu}[\p{L}]*)\s*:?\s*", RegexOptions.Compiled);

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: ledger-reference-template <transcript.txt> <candidate-ledger.json> [out.md]");
            return Task.FromResult(2);
        }

        var transcriptPath = Resolve(repoRoot, args[1]);
        var ledgerPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[ref-template] Transkript fehlt: {transcriptPath}"); return Task.FromResult(2); }
        if (!File.Exists(ledgerPath)) { Console.Error.WriteLine($"[ref-template] Kandidaten-Ledger fehlt: {ledgerPath}"); return Task.FromResult(2); }

        var stem = Path.GetFileNameWithoutExtension(transcriptPath);
        var outPath = args.Length >= 4
            ? Resolve(repoRoot, args[3])
            : Path.Combine(repoRoot, "input", "eval-labels", $"{stem}.reference-template.md");

        var transcript = File.ReadAllText(transcriptPath);
        var turns = TranscriptSegmenter.Segment(transcript);
        var entries = LoadEntries(ledgerPath);

        // Pro Turn die zugeordneten Kandidaten (deterministisch via Evidence-Quote-Fragment-Substring).
        var perTurn = new List<List<SemanticLedgerEntry>>();
        for (var i = 0; i < turns.Count; i++) perTurn.Add([]);
        var normTurns = turns.Select(t => Norm(t.Text)).ToArray();
        var orphans = new List<SemanticLedgerEntry>();

        foreach (var e in entries)
        {
            var matched = MatchTurns(e, normTurns);
            if (matched.Count == 0) { orphans.Add(e); continue; }
            foreach (var idx in matched) perTurn[idx].Add(e);
        }

        var alignedCount = entries.Count - orphans.Count;
        var emptyTurns = perTurn.Count(p => p.Count == 0);

        var md = Render(stem, transcriptPath, ledgerPath, repoRoot, turns, perTurn, orphans, entries.Count, alignedCount, emptyTurns);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, md);

        Console.WriteLine($"[ref-template] segments={turns.Count} candidates={entries.Count} aligned={alignedCount} orphans={orphans.Count} emptySegments={emptyTurns}");
        Console.WriteLine($"[ref-template] template -> {Path.GetRelativePath(repoRoot, outPath)}");
        return Task.FromResult(0);
    }

    /// <summary>Findet alle Turns, deren Text ein hinreichend langes Quote-Fragment eines Eintrags enthält.</summary>
    private static HashSet<int> MatchTurns(SemanticLedgerEntry e, string[] normTurns)
    {
        var matched = new HashSet<int>();
        foreach (var minLen in new[] { 20, 10 }) // erst streng, dann lockerer falls nichts traf
        {
            foreach (var ev in e.Evidence)
            {
                var q = SpeakerPrefixRx.Replace(ev.Quote ?? "", "").Replace("…", "...");
                foreach (var frag in q.Split("...", StringSplitOptions.RemoveEmptyEntries))
                {
                    var nf = Norm(frag);
                    if (nf.Length < minLen) continue;
                    for (var i = 0; i < normTurns.Length; i++)
                        if (normTurns[i].Contains(nf)) matched.Add(i);
                }
            }
            if (matched.Count > 0) break;
        }
        return matched;
    }

    private static string Render(
        string stem, string transcriptPath, string ledgerPath, string repoRoot,
        IReadOnlyList<TranscriptTurn> turns, List<List<SemanticLedgerEntry>> perTurn,
        List<SemanticLedgerEntry> orphans, int candidateCount, int alignedCount, int emptyTurns)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Referenz-Ledger — Annotations-Vorlage: {stem}");
        sb.AppendLine();
        sb.AppendLine("> **Zweck:** hand-vollständigen Referenz-Ledger bauen, um Recall der Auto-Extraktion zu messen");
        sb.AppendLine("> (\"fehlen wirklich Sachen aus dem Transkript?\"). **Deterministisch erzeugt (kein LLM).**");
        sb.AppendLine("> Status der fertigen Referenz: *modell-entworfen (Kandidaten), AUTOR-adjudiziert*, Single-Labeler (E5).");
        sb.AppendLine(">");
        sb.AppendLine("> **So adjudizierst du (pro Segment):** Lies den Rohtext. Prüfe, ob die zugeordneten Kandidaten");
        sb.AppendLine("> alles **fachlich Relevante** des Segments abdecken. Trage unter *MISS* jeden relevanten Claim ein,");
        sb.AppendLine("> den KEIN Kandidat abdeckt. Markiere *NOISE* für irrelevante Kandidaten. Smalltalk/Wiederholung = ok leer.");
        sb.AppendLine("> **Segmente ohne Kandidaten (⚠) zuerst prüfen** — Haupt-Miss-Verdacht.");
        sb.AppendLine();
        sb.AppendLine("```text");
        sb.AppendLine($"Transkript : {Path.GetRelativePath(repoRoot, transcriptPath)}");
        sb.AppendLine($"Kandidaten : {Path.GetRelativePath(repoRoot, ledgerPath)}");
        sb.AppendLine($"Segmente   : {turns.Count}   davon OHNE Kandidat: {emptyTurns}");
        sb.AppendLine($"Kandidaten : {candidateCount}   zugeordnet: {alignedCount}   orphan (kein Segment-Match): {orphans.Count}");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var t in turns)
        {
            var cands = perTurn[t.Index];
            var flag = cands.Count == 0 ? " ⚠ KEIN KANDIDAT" : "";
            sb.AppendLine($"## Segment {t.Index} — {t.Speaker}{flag}");
            sb.AppendLine();
            sb.AppendLine(Blockquote(t.Text));
            sb.AppendLine();
            sb.AppendLine($"**Auto-Kandidaten ({cands.Count}):**");
            if (cands.Count == 0)
                sb.AppendLine("- _(keine — Segment besonders auf Misses prüfen)_");
            else
                foreach (var c in cands)
                    sb.AppendLine($"- `{c.Id}` {c.Proposition}  _[{c.Status}/{c.Modality}/{c.Scope}/{c.TimeScope ?? "-"}]_");
            sb.AppendLine();
            sb.AppendLine("**Adjudikation:** abgedeckt? ( ) ja  ( ) nein");
            sb.AppendLine("- MISS (relevanter Claim ohne Kandidat): ");
            sb.AppendLine("- NOISE (irrelevanter Kandidat): ");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        sb.AppendLine("## Orphan-Kandidaten (Evidence keinem Segment zugeordnet)");
        sb.AppendLine();
        sb.AppendLine("_Deterministisches Matching hat hier kein Segment gefunden (oft paraphrasierte/zusammengesetzte");
        sb.AppendLine("Evidence). Bitte manuell zuordnen oder als NOISE markieren._");
        sb.AppendLine();
        if (orphans.Count == 0)
            sb.AppendLine("- _(keine)_");
        else
            foreach (var c in orphans)
                sb.AppendLine($"- `{c.Id}` {c.Proposition}  _[{c.Status}/{c.Modality}/{c.Scope}/{c.TimeScope ?? "-"}]_");
        sb.AppendLine();
        return sb.ToString();
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

    private static string Blockquote(string text)
        => "> " + text.Replace("\n", "\n> ");

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
