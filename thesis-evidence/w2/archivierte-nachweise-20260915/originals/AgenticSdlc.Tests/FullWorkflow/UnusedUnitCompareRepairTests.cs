using AgenticSdlc.Host.FullWorkflow.Ledger;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-1-Netz (E2E-Reibungs-Log 2026-07-23): Deckungs-Urteile ohne gueltige Candidate-Referenz muessen als
// Verstoss erkannt und — wenn unreparierbar — deterministisch zu needs_human umgestuft werden.
public sealed class UnusedUnitCompareRepairTests
{
    private static readonly IReadOnlySet<string> Known = new HashSet<string>(["R1", "R2"], StringComparer.Ordinal);

    private static UnusedUnitLedgerCompareItem Item(string verdict, params string[] refs) => new(
        "AU-0001", verdict, "attach_evidence", "Begruendung", null, refs);

    [Fact]
    public void Deckungs_Urteil_ohne_Referenz_ist_Verstoss()
    {
        Assert.True(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("attach_as_evidence"), Known));
        Assert.True(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("already_covered_indirectly"), Known));
    }

    [Fact]
    public void Nur_halluzinierte_Referenzen_zaehlen_wie_keine()
    {
        Assert.True(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("attach_as_evidence", "R-GHOST"), Known));
        // eine gueltige reicht (die halluzinierte fliegt spaeter im Sanitize raus):
        Assert.False(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("attach_as_evidence", "R-GHOST", "R1"), Known));
    }

    [Fact]
    public void Nicht_Deckungs_Verdicts_brauchen_keine_Referenz()
    {
        Assert.False(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("missing_claim"), Known));
        Assert.False(UnusedUnitCompareRepair.ClaimsCoverageWithoutValidReference(Item("needs_human"), Known));
    }

    [Fact]
    public void Downgrade_stuft_ehrlich_zu_needs_human_und_hinterlaesst_Spur()
    {
        var d = UnusedUnitCompareRepair.Downgrade(Item("attach_as_evidence"));
        Assert.Equal("needs_human", d.Verdict);
        Assert.Equal("human_review", d.SuggestedAction);
        Assert.StartsWith(UnusedUnitCompareRepair.DowngradePrefix, d.Reason);
        Assert.EndsWith("Begruendung", d.Reason);                  // Original-Begruendung bleibt lesbar
        Assert.Empty(d.RelatedCandidateIds);
        Assert.Equal("AU-0001", d.UnitId);
    }
}
