using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.FullWorkflow.Ledger.Core;

/// <summary>Eine deterministisch segmentierte Transkript-äusserung (Sprecher-Turn).</summary>
public sealed record TranscriptTurn(int Index, string Speaker, string Text);

/// <summary>
/// DISK-14 Coverage-Achse: zerlegt ein Transkript deterministisch in Sprecher-Turns (KEIN LLM →
/// fixe, reproduzierbare Einheiten). Das ist der bounded Anker für die MISSING-Prüfung (Transkript →
/// Artefakt), damit sie NICHT in offene "finde was fehlt"-Generierung zurückfällt.
/// </summary>
public static class TranscriptSegmenter
{
    // Turn-Start: "[Name] ..." ODER "Name: ..." (Sprechername + Doppelpunkt).
    private static readonly Regex SpeakerRx =
        new(@"^\s*(?:\[(?<sp>[^\]]+)\]|(?<sp2>\p{Lu}[\p{L}]+(?:-[\p{L}]+)*):)\s*(?<rest>.*)$", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRx = new(@"\s+", RegexOptions.Compiled);

    public static IReadOnlyList<TranscriptTurn> Segment(string transcript)
    {
        var turns = new List<TranscriptTurn>();
        string? speaker = null;
        var buffer = new List<string>();
        var idx = 0;

        void Flush()
        {
            if (speaker is not null)
            {
                var text = Normalize(string.Join(" ", buffer));
                if (text.Length > 0) turns.Add(new TranscriptTurn(idx++, speaker, text));
            }
            buffer.Clear();
        }

        foreach (var raw in (transcript ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            if (raw.Trim().Length == 0) continue;

            var m = SpeakerRx.Match(raw);
            if (m.Success)
            {
                Flush();
                speaker = (m.Groups["sp"].Success ? m.Groups["sp"].Value : m.Groups["sp2"].Value).Trim();
                buffer.Add(m.Groups["rest"].Value);
            }
            else if (speaker is not null)
            {
                buffer.Add(raw.Trim()); // Fortsetzungszeile desselben Turns
            }
            // Zeilen vor dem ersten Sprecher = Präambel -> ignoriert
        }

        Flush();
        return turns;
    }

    private static string Normalize(string s) => WhitespaceRx.Replace(s, " ").Trim();
}
