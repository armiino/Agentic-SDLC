using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Front-Netz (Ledger-Deterministik): der Segmenter ist der bounded Anker der Coverage-Achse (DISK-14) —
// fixe, reproduzierbare Turns ohne LLM. Diese Tests frieren das Turn-Erkennungs-Verhalten ein.
public sealed class TranscriptSegmenterTests
{
    [Fact]
    public void Beide_Sprecherformate_werden_erkannt_und_fortlaufend_indiziert()
    {
        var turns = TranscriptSegmenter.Segment("[Anna] Hallo zusammen.\nBernd: Guten Tag.");
        Assert.Equal(2, turns.Count);
        Assert.Equal((0, "Anna", "Hallo zusammen."), (turns[0].Index, turns[0].Speaker, turns[0].Text));
        Assert.Equal((1, "Bernd", "Guten Tag."), (turns[1].Index, turns[1].Speaker, turns[1].Text));
    }

    [Fact]
    public void Fortsetzungszeilen_gehoeren_zum_Turn_und_Whitespace_wird_normalisiert()
    {
        var turns = TranscriptSegmenter.Segment("[Anna] Erste   Zeile\n   zweite Zeile\n\n[Bernd] Ok");
        Assert.Equal(2, turns.Count);
        Assert.Equal("Erste Zeile zweite Zeile", turns[0].Text);
    }

    [Fact]
    public void Praeambel_vor_dem_ersten_Sprecher_wird_ignoriert()
    {
        var turns = TranscriptSegmenter.Segment("Kickoff Meeting Notizen 2026\nAgenda folgt\n[Anna] Los geht es.");
        Assert.Single(turns);
        Assert.Equal("Anna", turns[0].Speaker);
    }

    [Fact]
    public void Turn_ohne_Text_wird_nicht_emittiert()
    {
        var turns = TranscriptSegmenter.Segment("[Anna]\n[Bernd] Inhalt");
        Assert.Single(turns);
        Assert.Equal((0, "Bernd"), (turns[0].Index, turns[0].Speaker));
    }

    [Fact]
    public void CRLF_und_CR_Zeilenenden_werden_wie_LF_behandelt()
    {
        var turns = TranscriptSegmenter.Segment("[Anna] Eins\r\n[Bernd] Zwei\r[Carla] Drei");
        Assert.Equal(["Anna", "Bernd", "Carla"], turns.Select(t => t.Speaker));
    }
}
