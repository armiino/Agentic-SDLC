using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

public sealed record UnitCoverageGateResult(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("totalUnits")] int TotalUnits,
    [property: JsonPropertyName("usedUnits")] int UsedUnits,
    [property: JsonPropertyName("unusedUnits")] IReadOnlyList<string> UnusedUnits,
    [property: JsonPropertyName("candidateCount")] int CandidateCount,
    [property: JsonPropertyName("candidateWithoutSourceUnitIds")] IReadOnlyList<string> CandidateWithoutSourceUnitIds,
    [property: JsonPropertyName("unknownSourceUnitIds")] IReadOnlyList<string> UnknownSourceUnitIds);

internal static class UnitCoverageGate
{
    public static UnitCoverageGateResult Evaluate(
        IReadOnlyList<AtomicUnit> units,
        IReadOnlyList<SemanticLedgerEntry> candidates)
    {
        var known = units.Select(u => u.Id).ToHashSet(StringComparer.Ordinal);
        var used = candidates
            .SelectMany(c => c.SourceUnitIds ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);

        var missingTrace = candidates
            .Where(c => c.SourceUnitIds is null || c.SourceUnitIds.Count == 0)
            .Select(c => c.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList();

        var unknown = used
            .Where(id => !known.Contains(id))
            .Order(StringComparer.Ordinal)
            .ToList();

        var usedKnown = used.Where(known.Contains).ToHashSet(StringComparer.Ordinal);
        var unused = units
            .Select(u => u.Id)
            .Where(id => !usedKnown.Contains(id))
            .ToList();

        return new UnitCoverageGateResult(
            Pass: missingTrace.Count == 0 && unknown.Count == 0,
            TotalUnits: units.Count,
            UsedUnits: usedKnown.Count,
            UnusedUnits: unused,
            CandidateCount: candidates.Count,
            CandidateWithoutSourceUnitIds: missingTrace,
            UnknownSourceUnitIds: unknown);
    }
}
