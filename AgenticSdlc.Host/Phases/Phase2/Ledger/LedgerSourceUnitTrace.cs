using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

internal static class LedgerSourceUnitTrace
{
    public static IReadOnlyList<SemanticLedgerEntry> ApplyFromCandidates(
        IReadOnlyList<SemanticLedgerEntry> candidates,
        IReadOnlyList<SemanticLedgerEntry> canonical)
    {
        var byId = candidates
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .GroupBy(c => c.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        return canonical
            .Select(entry =>
            {
                var sourceUnitIds = entry.CandidateIds?
                    .SelectMany(id => byId.TryGetValue(id, out var candidate) ? candidate.SourceUnitIds ?? [] : [])
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToList();

                return sourceUnitIds is { Count: > 0 }
                    ? entry with { SourceUnitIds = sourceUnitIds }
                    : entry;
            })
            .ToList();
    }
}
