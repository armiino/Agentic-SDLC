using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1e' Schritt 3: der Adjudikations-Gate-Kern. Adjudikation ist action-typisiert (nicht nur accept/reject) —
// diese Tests frieren die Policy-Aufloesung ein, v. a. den sauberen Replay-Pfad (aufgezeichnete Aktion je
// ItemId) + den Governance-Fallback (unmatcht ⇒ reject) und die accept-all-Experiment-Heuristik.
public sealed class AdjudicationResolverTests
{
    private static AdjudicationItem Item(string id, string itemType, AdjudicationSuggestion? suggestion = null) =>
        new(ItemId: id, ItemType: itemType, SourceMode: "normal", ClaimId: id, UnitId: null,
            Proposition: $"prop-{id}", EvidenceRefs: [], SystemSuggestion: suggestion, Reason: "test");

    private static AdjudicationSuggestion FacetRepair() => new("facet_repair", "modality", "desired", "must", null);
    private static AdjudicationSuggestion Compare(string classification) => new("compare_classification", null, null, null, classification);

    [Fact]
    public void Interactive_pausiert_ohne_Aktionen()
    {
        var r = AdjudicationResolver.ResolveActions([Item("A", "review_required_claim")], GatePolicy.Interactive);
        Assert.Equal(AdjudicationOutcome.Pause, r.Outcome);
        Assert.Empty(r.FilledItems);
    }

    [Fact]
    public void Replay_spielt_aufgezeichnete_Aktion_je_ItemId_ein()
    {
        var items = new[] { Item("A", "review_required_claim"), Item("B", "coverage_miss") };
        var recorded = new Dictionary<string, string> { ["A"] = AdjudicationActions.ApplyRepair, ["B"] = AdjudicationActions.AcceptGap };

        var r = AdjudicationResolver.ResolveActions(items, new GatePolicy(GatePolicyKind.Replay), recorded);

        Assert.Equal(AdjudicationOutcome.Resolved, r.Outcome);
        Assert.Equal(AdjudicationActions.ApplyRepair, r.FilledItems[0].Action);
        Assert.Equal(AdjudicationActions.AcceptGap, r.FilledItems[1].Action);
        Assert.Empty(r.UnmatchedItemIds);
        Assert.Equal(2, r.AcceptedCount);
    }

    [Fact]
    public void Replay_unmatchtes_Item_wird_rejected_und_dokumentiert()
    {
        var items = new[] { Item("A", "review_required_claim"), Item("B", "coverage_miss") };
        var recorded = new Dictionary<string, string> { ["A"] = AdjudicationActions.AcceptGap }; // B fehlt

        var r = AdjudicationResolver.ResolveActions(items, new GatePolicy(GatePolicyKind.Replay), recorded);

        Assert.Equal(AdjudicationActions.AcceptGap, r.FilledItems[0].Action);
        Assert.Equal(AdjudicationActions.Reject, r.FilledItems[1].Action);   // Fallback
        Assert.Equal(new[] { "B" }, r.UnmatchedItemIds);
        Assert.Equal(1, r.AcceptedCount);
        Assert.Equal(1, r.RejectedCount);
    }

    [Fact]
    public void Replay_ungueltige_Aktion_zaehlt_als_unmatcht()
    {
        var recorded = new Dictionary<string, string> { ["A"] = "voll_daneben" };
        var r = AdjudicationResolver.ResolveActions([Item("A", "coverage_miss")], new GatePolicy(GatePolicyKind.Replay), recorded);

        Assert.Equal(AdjudicationActions.Reject, r.FilledItems[0].Action);
        Assert.Equal(new[] { "A" }, r.UnmatchedItemIds);
    }

    [Theory]
    [InlineData("missing_claim", AdjudicationActions.AcceptGap)]
    [InlineData("attach_as_evidence", AdjudicationActions.AttachEvidence)]
    [InlineData("already_covered_indirectly", AdjudicationActions.MarkCoveredBy)]
    [InlineData("needs_human", AdjudicationActions.Reject)] // nicht auto-entscheidbar
    public void AcceptAll_folgt_der_Compare_Klassifikation(string classification, string expectedAction)
    {
        var r = AdjudicationResolver.ResolveActions(
            [Item("A", "coverage_miss", Compare(classification))], new GatePolicy(GatePolicyKind.AcceptAll));
        Assert.Equal(expectedAction, r.FilledItems[0].Action);
    }

    [Fact]
    public void AcceptAll_bei_facet_repair_wird_apply_repair()
    {
        var r = AdjudicationResolver.ResolveActions(
            [Item("A", "review_required_claim", FacetRepair())], new GatePolicy(GatePolicyKind.AcceptAll));
        Assert.Equal(AdjudicationActions.ApplyRepair, r.FilledItems[0].Action);
    }

    [Fact]
    public void AcceptAll_ohne_Vorschlag_review_required_wird_uebernommen_sonst_reject()
    {
        var review = AdjudicationResolver.ResolveActions([Item("A", "review_required_claim")], new GatePolicy(GatePolicyKind.AcceptAll));
        var miss = AdjudicationResolver.ResolveActions([Item("B", "coverage_miss")], new GatePolicy(GatePolicyKind.AcceptAll));

        Assert.Equal(AdjudicationActions.AcceptGap, review.FilledItems[0].Action);
        Assert.Equal(AdjudicationActions.Reject, miss.FilledItems[0].Action);
    }

    [Fact]
    public void Leere_Queue_ist_unkritisch()
    {
        var r = AdjudicationResolver.ResolveActions([], new GatePolicy(GatePolicyKind.Replay), new Dictionary<string, string>());
        Assert.Equal(AdjudicationOutcome.Resolved, r.Outcome);
        Assert.Empty(r.FilledItems);
        Assert.Equal(0, r.AcceptedCount);
    }
}
