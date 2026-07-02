using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>Ein konkretes Facetten-Problem an einem Ledger-Eintrag inkl. Repair-Vorschlag.</summary>
/// <remarks>
/// facet ∈ {proposition,status,modality,scope,timeScope,evidence,disposition}. <c>suggested</c> ist der
/// vorgeschlagene korrigierte Wert (Repair), z. B. status observed="decided" → suggested="open".
/// </remarks>
public sealed record FacetIssue(
    [property: JsonPropertyName("facet")] string Facet,
    [property: JsonPropertyName("observed")] string Observed,
    [property: JsonPropertyName("problem")] string Problem,
    [property: JsonPropertyName("suggested")] string? Suggested)
{
    private static readonly Dictionary<string, string[]> SuggestedValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = ["decided", "open", "rejected", "uncertain", "required"],
        ["modality"] = ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"],
        ["timescope"] = ["mvp", "later_possible", "mvp_or_later_unclear"]
    };

    public FacetIssue Normalized() => this with
    {
        Facet = Facet?.Trim().ToLowerInvariant() ?? "",
        Observed = Observed?.Trim() ?? "",
        Problem = Problem?.Trim() ?? "",
        Suggested = NormalizeSuggested(Facet, Suggested)
    };

    private static string? NormalizeSuggested(string? facet, string? suggested)
    {
        if (string.IsNullOrWhiteSpace(suggested)) return null;
        var normalizedFacet = facet?.Trim().ToLowerInvariant() ?? "";
        var normalized = suggested.Trim().ToLowerInvariant();
        return SuggestedValues.TryGetValue(normalizedFacet, out var allowed) && !allowed.Contains(normalized, StringComparer.Ordinal)
            ? null
            : normalized;
    }
}

/// <summary>Verdict des Validators für EINEN Ledger-Eintrag (bounded Per-Item-Urteil gegen das Transcript).</summary>
public sealed record EntryValidation(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("facetIssues")] IReadOnlyList<FacetIssue> FacetIssues,
    [property: JsonPropertyName("reason")] string Reason)
{
    // grounded: Proposition + Facetten transcript-gedeckt · partial: Kern gedeckt, einzelne Facette schwächer/ab
    // overstated: stärker formuliert als die Quelle hergibt (Status-/Modalitäts-Verstärkung) · unsupported: nicht gedeckt.
    public static string NormalizeVerdict(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "grounded" => "grounded",
        "partial" => "partial",
        "overstated" => "overstated",
        "unsupported" => "unsupported",
        var invalid => throw new InvalidOperationException($"LEDGER_FACET_INVALID_VERDICT: '{invalid ?? "<null>"}'")
    };

    public EntryValidation Normalized() => this with
    {
        Id = Id?.Trim() ?? "",
        Verdict = NormalizeVerdict(Verdict),
        FacetIssues = (FacetIssues ?? []).Select(i => i.Normalized()).Where(i => i.Facet.Length > 0).ToList(),
        Reason = Reason?.Trim() ?? ""
    };
}

public sealed record BatchValidationResult(
    [property: JsonPropertyName("items")] IReadOnlyList<EntryValidation> Items);

/// <summary>Ein kanonischer Claim + sein Validierungs-Verdict + der daraus abgeleitete Freigabe-Status.</summary>
public sealed record ValidatedLedgerEntry(
    [property: JsonPropertyName("entry")] SemanticLedgerEntry Entry,
    [property: JsonPropertyName("validation")] EntryValidation Validation,
    [property: JsonPropertyName("claimStatus")] string ClaimStatus)
{
    /// <summary>Freigabe-Status DETERMINISTISCH aus dem Verdict abgeleitet (smallVersion.md §Freigabe-Status).</summary>
    public static string DeriveStatus(EntryValidation v) => v.Verdict switch
    {
        "grounded" => "approved",
        _ => "review_required" // partial / overstated / unsupported → Mensch/Repair
    };
}

public sealed record ValidatedLedger(
    [property: JsonPropertyName("entries")] IReadOnlyList<ValidatedLedgerEntry> Entries);
