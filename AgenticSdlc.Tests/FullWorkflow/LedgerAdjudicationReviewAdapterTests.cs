using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Sonderling 1: die Ledger-Adjudikation arbeitet auf der QUEUE (nicht human-decisions.json).
// Fixiert: Resolved verlangt eine gueltige Aktion; die drei Ziel-Aktionen (merge_existing, mark_covered_by,
// attach_evidence) verlangen ZUSAETZLICH ein referenceTarget; MergeInto traegt UI-Werte ins Queue-Item zurueck.
public sealed class LedgerAdjudicationReviewAdapterTests
{
    private static AdjudicationItem Item(string id = "ADJ-1") => new(
        ItemId: id, ItemType: "review_required_claim", SourceMode: "normal",
        ClaimId: "canon_x", UnitId: null, Proposition: "Der Nutzer wuenscht X.",
        EvidenceRefs: ["AU-0009"], SystemSuggestion: null, Reason: "FacetValidation unsicher");

    private static AdjudicationQueue Queue(params AdjudicationItem[] items)
        => new(SourceValidatedRunId: "run-1", SourceUnitRunId: null, GeneratedAt: "2026-07-22", Items: items);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_aus_der_Queue()
    {
        var session = LedgerAdjudicationReviewAdapter.BuildSession(Queue(Item("ADJ-1"), Item("ADJ-2")));
        Assert.Equal(["ADJ-1", "ADJ-2"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Ziel_Aktionen_verlangen_referenceTarget()
    {
        var session = LedgerAdjudicationReviewAdapter.BuildSession(Queue(Item()));
        var it = session.Items[0];

        Set(it, LedgerAdjudicationReviewAdapter.FieldAction, "reject");
        Assert.True(LedgerAdjudicationReviewAdapter.Resolved(it)); // reject braucht kein Ziel

        Set(it, LedgerAdjudicationReviewAdapter.FieldAction, "merge_existing");
        Set(it, LedgerAdjudicationReviewAdapter.FieldTarget, "");
        Assert.False(LedgerAdjudicationReviewAdapter.Resolved(it)); // Ziel-Aktion ohne Ziel

        Set(it, LedgerAdjudicationReviewAdapter.FieldTarget, "canon_y");
        Assert.True(LedgerAdjudicationReviewAdapter.Resolved(it));

        Set(it, LedgerAdjudicationReviewAdapter.FieldAction, "quatsch");
        Assert.False(LedgerAdjudicationReviewAdapter.Resolved(it)); // ungueltige Aktion
    }

    [Fact]
    public void MergeInto_traegt_UI_Werte_ins_Queue_Item_zurueck()
    {
        var original = Item("ADJ-1");
        var session = LedgerAdjudicationReviewAdapter.BuildSession(Queue(original));
        var it = session.Items[0];
        Set(it, LedgerAdjudicationReviewAdapter.FieldAction, "accept_gap");
        Set(it, LedgerAdjudicationReviewAdapter.FieldReason, "echte Luecke");

        var merged = LedgerAdjudicationReviewAdapter.MergeInto(original, it);
        Assert.Equal("accept_gap", merged.Action);
        Assert.Equal("echte Luecke", merged.ActionReason);
        Assert.Equal(original.ItemId, merged.ItemId); // Identitaet unangetastet
    }
}
