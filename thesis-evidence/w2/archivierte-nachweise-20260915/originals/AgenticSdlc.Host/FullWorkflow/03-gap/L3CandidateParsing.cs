using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>Geteiltes Parsing der Generator-Antwort (RawCandidate → L3Candidate). EIN Ort für die Feld-Zuordnung, damit
/// Einzelpass-Generierung (<see cref="L3CandidateGenExecutor"/>) und Coverage-Repair-Loop (<see cref="L3CoverageGenExecutor"/>)
/// driftfrei bleiben.</summary>
internal static class L3CandidateParsing
{
    /// <summary>Parst die Kandidaten OHNE finale Id (CandidateId=""). Die stabile Nummerierung vergibt der Aufrufer per
    /// <see cref="AssignIds"/> — im Loop erst NACH dem Merge/Dedup über alle Runden.</summary>
    public static List<L3Candidate> ParseCores(string? responseText)
    {
        var raw = L3Json.Deserialize<RawCandidates>(responseText)?.Candidates ?? [];
        var items = new List<L3Candidate>(raw.Count);
        foreach (var c in raw)
        {
            if (string.IsNullOrWhiteSpace(c.Text)) continue;
            items.Add(new L3Candidate("", string.IsNullOrWhiteSpace(c.TargetType) ? "requirement" : c.TargetType!.Trim(),
                c.Text!.Trim(), c.Rationale, c.Assumptions ?? [],
                string.IsNullOrWhiteSpace(c.Intent) ? null : c.Intent!.Trim().ToLowerInvariant(), c.BasedOn ?? [],
                string.IsNullOrWhiteSpace(c.GapCategory) ? null : c.GapCategory!.Trim().ToLowerInvariant(),
                string.IsNullOrWhiteSpace(c.ImpactIfMissing) ? null : c.ImpactIfMissing!.Trim(), c.RequiresHumanDecision));
        }
        return items;
    }

    /// <summary>Vergibt stabile fortlaufende CandidateIds (CAND-001 …) in Reihenfolge.</summary>
    public static List<L3Candidate> AssignIds(IEnumerable<L3Candidate> cores)
        => cores.Select((c, i) => c with { CandidateId = $"CAND-{i + 1:D3}" }).ToList();

    /// <summary>Normalisierter Text für Dedup über Repair-Runden (kleingeschrieben, kollabierte Whitespaces).</summary>
    public static string NormText(string text)
        => string.Join(' ', text.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    internal sealed record RawCandidates([property: JsonPropertyName("candidates")] IReadOnlyList<RawCandidate>? Candidates);
    internal sealed record RawCandidate(
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("targetType")] string? TargetType,
        [property: JsonPropertyName("rationale")] string? Rationale,
        [property: JsonPropertyName("assumptions")] IReadOnlyList<string>? Assumptions,
        [property: JsonPropertyName("intent")] string? Intent,
        [property: JsonPropertyName("basedOn")] IReadOnlyList<string>? BasedOn,
        [property: JsonPropertyName("gapCategory")] string? GapCategory,
        [property: JsonPropertyName("impactIfMissing")] string? ImpactIfMissing,
        [property: JsonPropertyName("requiresHumanDecision")] bool? RequiresHumanDecision);
}
