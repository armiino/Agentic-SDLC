using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// LENS-COVERAGE-GATE (Schritt 3) — DETERMINISTISCHER, closed-world Scorecard über die generierten Kandidaten gegen den
/// <see cref="CoverageSpec"/>: hat jede Prüflinse ≥1 Kandidaten (via <c>gapCategory</c>) — sonst <c>unaddressed</c>?
/// Kein LLM → reproduzierbar, nicht gamebar auf Existenz. Spiegel von <c>DerivationCoverage</c> (Rechenschaft, KEINE
/// Mengen-Quote), nur über Linsen statt Input-Items. Der Befund macht „eine ganze Linse lautlos leer" sichtbar (im
/// v10-Run war das <c>security-misuse</c>).
/// </summary>
/// <remarks>
/// EHRLICHKEIT (wie im Katalog): dies misst Abdeckung GEGENÜBER dem Rahmen (relativ), NICHT Vollständigkeit gegenüber
/// der Realität. „addressed" = Existenz ≥1, NICHT Adäquanz — ein Alibi-Kandidat genügt formal (Adäquanz-Schicht ist die
/// zurückgestellte optionale zweite Stufe). v1 ist reiner Report/Measure (kein Zwangs-Loop → das käme in Schritt 4).
/// </remarks>
public static class L3LensCoverage
{
    public static LensCoverageReport Evaluate(CoverageSpec spec, IReadOnlyList<L3Candidate> candidates)
    {
        var countById = spec.Lenses.ToDictionary(l => l.Id, _ => 0, StringComparer.Ordinal);
        var unknown = new List<string>();
        var untagged = 0;
        foreach (var c in candidates)
        {
            var cat = c.GapCategory;
            if (string.IsNullOrWhiteSpace(cat)) { untagged++; continue; } // ohne gapCategory → zählt für keine Linse
            if (countById.ContainsKey(cat)) countById[cat]++;
            else unknown.Add(cat);                                        // gapCategory nicht im Spec (Tippfehler/erfunden)
        }

        var perLens = spec.Lenses
            .Select(l => new LensCoverageItem(l.Id, l.Name, l.Mandatory, countById[l.Id],
                countById[l.Id] > 0 ? "addressed" : "unaddressed"))
            .ToList();
        var unaddressed = perLens.Where(p => p.Status == "unaddressed").ToList();
        var mandatoryUnaddressed = unaddressed.Where(p => p.Mandatory).ToList();

        return new LensCoverageReport(
            SpecId: spec.Id,
            TotalLenses: spec.Lenses.Count,
            AddressedLenses: perLens.Count(p => p.Status == "addressed"),
            UnaddressedLenses: unaddressed.Count,
            MandatoryUnaddressed: mandatoryUnaddressed.Count,
            CoverageComplete: unaddressed.Count == 0,
            MandatoryComplete: mandatoryUnaddressed.Count == 0,
            UntaggedCandidates: untagged,
            UnknownCategoryCount: unknown.Count,
            PerLens: perLens,
            UnaddressedLensIds: unaddressed.Select(p => p.LensId).ToList(),
            MandatoryUnaddressedLensIds: mandatoryUnaddressed.Select(p => p.LensId).ToList(),
            UnknownCategories: unknown.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList());
    }
}

/// <summary>Abdeckungs-Status EINER Linse. <see cref="Status"/> = addressed | unaddressed (Existenz ≥1). Der explizite
/// N/A-Status (Agent erklärt eine leere Linse begründet für nicht anwendbar) kommt erst mit dem Repair-Pass (Schritt 4).</summary>
public sealed record LensCoverageItem(
    [property: JsonPropertyName("lensId")] string LensId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("mandatory")] bool Mandatory,
    [property: JsonPropertyName("candidateCount")] int CandidateCount,
    [property: JsonPropertyName("status")] string Status);

/// <summary>Deterministischer Abdeckungs-Report über den generierten Kandidatensatz. <see cref="CoverageComplete"/> =
/// keine leere Linse; <see cref="MandatoryComplete"/> = keine Pflicht-Linse leer (das harte Signal).</summary>
public sealed record LensCoverageReport(
    [property: JsonPropertyName("specId")] string SpecId,
    [property: JsonPropertyName("totalLenses")] int TotalLenses,
    [property: JsonPropertyName("addressedLenses")] int AddressedLenses,
    [property: JsonPropertyName("unaddressedLenses")] int UnaddressedLenses,
    [property: JsonPropertyName("mandatoryUnaddressed")] int MandatoryUnaddressed,
    [property: JsonPropertyName("coverageComplete")] bool CoverageComplete,
    [property: JsonPropertyName("mandatoryComplete")] bool MandatoryComplete,
    // Daten-Hygiene: Kandidaten ohne gapCategory bzw. mit einer nicht im Spec vorhandenen Kategorie.
    [property: JsonPropertyName("untaggedCandidates")] int UntaggedCandidates,
    [property: JsonPropertyName("unknownCategoryCount")] int UnknownCategoryCount,
    [property: JsonPropertyName("perLens")] IReadOnlyList<LensCoverageItem> PerLens,
    [property: JsonPropertyName("unaddressedLensIds")] IReadOnlyList<string> UnaddressedLensIds,
    [property: JsonPropertyName("mandatoryUnaddressedLensIds")] IReadOnlyList<string> MandatoryUnaddressedLensIds,
    [property: JsonPropertyName("unknownCategories")] IReadOnlyList<string> UnknownCategories);
