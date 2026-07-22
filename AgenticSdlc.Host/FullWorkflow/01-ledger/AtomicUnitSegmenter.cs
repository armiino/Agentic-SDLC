using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

internal static class AtomicUnitSegmenter
{
    public static IReadOnlyList<AtomicUnit> Segment(string transcript, string source)
        => TranscriptSegmenter.Segment(transcript)
            .Select((turn, i) => new AtomicUnit(
                Id: $"AU-{i + 1:D4}",
                Source: source,
                TurnIndex: turn.Index,
                Speaker: turn.Speaker,
                Text: turn.Text))
            .ToList();
}
