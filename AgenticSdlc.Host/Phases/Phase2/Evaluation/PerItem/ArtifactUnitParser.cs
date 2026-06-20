using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// DISK-14 (Per-Item-Redesign, Phase 1): eine deterministisch aus dem Artefakt extrahierte Prüfeinheit.
/// Rein strukturell (kein LLM) → gleicher Input ergibt immer dieselben Units (null Varianz).
/// </summary>
/// <remarks>
/// BEWUSST ISOLIERT: Dieser Code hängt an NICHTS im bestehenden Jury-Pfad und wird nur über das
/// Offline-Kommando <c>parse-units</c> aufgerufen. Verwerfen des Per-Item-Ansatzes = diesen Ordner
/// löschen + die eine Dispatch-Zeile in Program.cs entfernen. Keine änderung am Evaluator/Runner.
/// <para><c>LikelyNonClaim</c> ist nur ein ADVISORY-Hinweis für die Sichtprüfung (Metadaten/
/// Traceability/Quellen-Zeilen). Es wird hier NICHTS verworfen — der spätere Klassifikator fängt
/// Nicht-Claims über ein eigenes Verdikt <c>not_a_claim</c> ab (Parser-Unschärfe ist selbstkorrigierend).</para>
/// </remarks>
public sealed record ArtifactUnit(
    int Index,
    string Section,
    string Kind,            // table_row | bullet | numbered | paragraph
    string Text,
    IReadOnlyList<string>? Cells,
    bool LikelyNonClaim);

/// <summary>Deterministischer Markdown→Units-Parser. Element-basiert (nicht zeilennummer-/reihenfolgenabhängig).</summary>
public static class ArtifactUnitParser
{
    private static readonly Regex BulletRx = new(@"^[-*]\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex NumberedRx = new(@"^\d+[.)]\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRx = new(@"\s+", RegexOptions.Compiled);
    // Horizontal Rule (---, ***, ___) — reines Layout, kein Inhalt.
    private static readonly Regex HorizontalRuleRx = new(@"^[-*_]{3,}$", RegexOptions.Compiled);
    // Nicht-erste Mapping-Spalte gilt als "kurz" (Name/ID/Owner) bis zu dieser Länge.
    private const int MappingCellMaxLen = 25;
    // Metadaten/Traceability/Quellen-Hinweise — nur für das advisory Flag, nicht zum Verwerfen.
    private static readonly Regex MetaRx = new(
        @"(?i)\b(quelle|stakeholder|owner|verantwortlich|trace|traceab|referenz)\b", RegexOptions.Compiled);

    public static IReadOnlyList<ArtifactUnit> Parse(string markdown)
    {
        var units = new List<ArtifactUnit>();
        var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var section = "(root)";
        var paragraph = new List<string>();
        var idx = 0;

        void FlushParagraph()
        {
            if (paragraph.Count == 0) return;
            var text = Normalize(string.Join(" ", paragraph));
            if (text.Length > 0)
                units.Add(new ArtifactUnit(idx++, section, "paragraph", text, null, IsLikelyNonClaim(text)));
            paragraph.Clear();
        }

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.Length == 0) { FlushParagraph(); continue; }

            if (HorizontalRuleRx.IsMatch(line)) { FlushParagraph(); continue; } // ---/***/___ verwerfen

            if (line.StartsWith("#")) { FlushParagraph(); section = Normalize(line.TrimStart('#').Trim()); continue; }

            if (line.StartsWith("|"))
            {
                FlushParagraph();

                if (IsTableSeparator(line)) continue;                       // |---|---|
                // Header = Tabellenzeile, deren nächste Zeile eine Separatorzeile ist.
                if (i + 1 < lines.Length && IsTableSeparator(lines[i + 1].Trim())) continue;

                var cells = line.Trim('|').Split('|').Select(Normalize).ToList();
                if (cells.All(c => c.Length == 0)) continue;
                var text = string.Join(" | ", cells.Where(c => c.Length > 0));
                // Mapping-/Metadaten-Zeile: ≥2 Spalten, aber alle Nicht-erst-Spalten kurz (Name/ID/Owner)
                // → kein fachlicher Claim (z. B. Traceability "Thema | Stakeholder"). Inhaltliche Tabellen
                // (risks: lange Beschreibungs-/Auswirkungs-Zellen) bleiben Claims. Nur advisory-Flag.
                var nonFirst = cells.Skip(1).Where(c => c.Length > 0).ToList();
                var isMapping = nonFirst.Count > 0 && nonFirst.All(c => c.Length <= MappingCellMaxLen);
                units.Add(new ArtifactUnit(idx++, section, "table_row", text, cells, isMapping || IsLikelyNonClaim(text)));
                continue;
            }

            var mBullet = BulletRx.Match(line);
            var mNum = NumberedRx.Match(line);
            if (mBullet.Success || mNum.Success)
            {
                FlushParagraph();
                var body = Normalize((mBullet.Success ? mBullet : mNum).Groups[1].Value);
                if (body.Length > 0)
                    units.Add(new ArtifactUnit(idx++, section, mBullet.Success ? "bullet" : "numbered", body, null, IsLikelyNonClaim(body)));
                continue;
            }

            paragraph.Add(line);
        }

        FlushParagraph();
        return units;
    }

    private static bool IsTableSeparator(string line)
        => line.StartsWith("|") && line.Contains('-') && Regex.IsMatch(line, @"^\|?[\s:\-|]+\|?$");

    private static string Normalize(string s)
        => WhitespaceRx.Replace((s ?? string.Empty).Replace("**", "").Replace("`", ""), " ").Trim();

    // Advisory: könnte eine Nicht-Claim-Zeile sein (Metadaten/zu kurz). NUR Hinweis fuer die Sichtpruefung.
    private static bool IsLikelyNonClaim(string text)
    {
        if (text.Length < 12) return true;
        if (text.Length < 45 && MetaRx.IsMatch(text)) return true;
        return false;
    }
}
