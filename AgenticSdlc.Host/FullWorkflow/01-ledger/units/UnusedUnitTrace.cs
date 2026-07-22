using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>Ein aufgelöster Verweis eines unused-Unit-Compare-Items: Kandidat → finaler kanonischer Claim.</summary>
public sealed record UnusedUnitTraceReference(
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("candidateExists")] bool CandidateExists,
    [property: JsonPropertyName("candidateProposition")] string? CandidateProposition,
    [property: JsonPropertyName("canonicalId")] string? CanonicalId,
    [property: JsonPropertyName("canonicalProposition")] string? CanonicalProposition);

public sealed record UnusedUnitTraceItem(
    [property: JsonPropertyName("unitId")] string UnitId,
    [property: JsonPropertyName("speaker")] string? Speaker,
    [property: JsonPropertyName("unitText")] string? UnitText,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("references")] IReadOnlyList<UnusedUnitTraceReference> References);

public sealed record UnusedUnitTraceResult(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("note")] string Note,
    [property: JsonPropertyName("brokenReferenceCount")] int BrokenReferenceCount,
    [property: JsonPropertyName("missingTargetCount")] int MissingTargetCount,
    [property: JsonPropertyName("brokenReferences")] IReadOnlyList<object> BrokenReferences,
    [property: JsonPropertyName("missingTargetUnitIds")] IReadOnlyList<string> MissingTargetUnitIds,
    [property: JsonPropertyName("items")] IReadOnlyList<UnusedUnitTraceItem> Items);

/// <summary>
/// Baut aus den unused-Unit-Compare-Ergebnissen einen selbst-prüfbaren Trace (Unit → Kandidat → finaler
/// Claim) UND validiert die Verweise: <c>relatedCandidateIds</c> müssen existieren, und `attach_as_evidence`/
/// `already_covered_indirectly` brauchen mindestens einen gültigen Zielverweis. (Fix für halluzinierte/
/// fehlende Verweise im LLM-Compare-Schritt — analog zu <c>unknownSourceUnitIds</c>.)
/// </summary>
public static class UnusedUnitTrace
{
    // Verdicts, die einen konkreten Ziel-Claim behaupten und daher einen gültigen Verweis brauchen.
    private static readonly string[] TargetVerdicts = ["attach_as_evidence", "already_covered_indirectly"];

    public static UnusedUnitTraceResult Build(
        IReadOnlyList<AtomicUnit> units,
        IReadOnlyList<SemanticLedgerEntry> candidates,
        IReadOnlyList<SemanticLedgerEntry> canonical,
        IReadOnlyList<UnusedUnitLedgerCompareItem> compareItems)
    {
        var unitById = units.GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var candById = candidates.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var candToCanon = new Dictionary<string, SemanticLedgerEntry>(StringComparer.Ordinal);
        foreach (var c in canonical)
            foreach (var cid in c.CandidateIds ?? [])
                candToCanon[cid] = c;

        var items = new List<UnusedUnitTraceItem>();
        var broken = new List<object>();
        var missingTarget = new List<string>();

        foreach (var i in compareItems)
        {
            var refs = new List<UnusedUnitTraceReference>();
            foreach (var r in i.RelatedCandidateIds ?? [])
            {
                var exists = candById.TryGetValue(r, out var cand);
                candToCanon.TryGetValue(r, out var canon);
                refs.Add(new UnusedUnitTraceReference(r, exists, cand?.Proposition, canon?.Id, canon?.Proposition));
                if (!exists) broken.Add(new { unitId = i.UnitId, candidateId = r });
            }

            var isTarget = TargetVerdicts.Contains(i.Verdict, StringComparer.OrdinalIgnoreCase);
            if (isTarget && (i.RelatedCandidateIds is null || i.RelatedCandidateIds.Count == 0))
                missingTarget.Add(i.UnitId);

            var u = unitById.GetValueOrDefault(i.UnitId);
            items.Add(new UnusedUnitTraceItem(i.UnitId, u?.Speaker, u?.Text, i.Verdict, i.Reason, refs));
        }

        var pass = broken.Count == 0 && missingTarget.Count == 0;
        return new UnusedUnitTraceResult(
            pass,
            "Trace Unit -> candidate -> canonical. pass=false => LLM-Compare erzeugte halluzinierte oder fehlende Verweise (INVALID_ATTACH_REFERENCE).",
            broken.Count, missingTarget.Count, broken, missingTarget, items);
    }
}
