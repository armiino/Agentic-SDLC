using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/Completion): DISK-Proposals freigeben.
// ItemId = ProposalItemId; Default-Scope zeigt nur RequiresHumanDecision-Items.
public sealed class L4CompletionReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static L4CompletionProposalItem Item(string id = "PROP-1", bool needsHuman = true, string op = "ADD_OPEN_DECISION") => new(
        ProposalItemId: id, Operation: op, Category: "scope", Title: $"Titel {id}",
        Problem: "Unklar", SuggestedResolution: "Nachfragen", WhyItMatters: "Risiko",
        EvidenceState: "stated", SourceRequirementIds: ["CAN-REQ-1"], SourceFindingIds: ["F-1"],
        RequiresHumanDecision: needsHuman, Metadata: new Dictionary<string, string>());

    private static L4CompletionProposalDocument Proposals(params L4CompletionProposalItem[] items)
        => new(1, "prop-1", "p1", "b1", T, "adeq-1", "doc.md", items);

    private static L4AdequacyReport Adequacy() => new(1, "adeq-1", "p1", "b1", T, "doc.md", [], "ok");

    private static CanonicalRequirementsBaseline Baseline() => new("b1", "p1", 1, T, "state.json", [], [], []);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void Scope_Filter_zeigt_RequiresHumanDecision_ODER_kritische_Operationen()
    {
        // Reale Semantik (durch diesen Test aufgedeckt/fixiert): ausserhalb von "all" bleiben sichtbar:
        // RequiresHumanDecision ODER Operation in {ADD_DISK_POINT, ADD_OPEN_DECISION, MARK_NEEDS_BREAKDOWN}.
        var proposals = Proposals(
            Item("PROP-1", needsHuman: true, op: "EXTEND_REQUIREMENT"),
            Item("PROP-2", needsHuman: false, op: "ADD_OPEN_DECISION"),   // kritische Op -> trotzdem sichtbar
            Item("PROP-3", needsHuman: false, op: "EXTEND_REQUIREMENT")); // weder noch -> gefiltert
        var all = L4CompletionReviewAdapter.BuildSession("r1", Adequacy(), proposals, Baseline(), "all");
        Assert.Equal(3, all.Items.Count);

        var scoped = L4CompletionReviewAdapter.BuildSession("r1", Adequacy(), proposals, Baseline(), "needs-human");
        Assert.Equal(["PROP-1", "PROP-2"], scoped.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Resolved_verlangt_Entscheidung_UND_Begruendung()
    {
        var session = L4CompletionReviewAdapter.BuildSession("r1", Adequacy(), Proposals(Item()), Baseline(), "all");
        var it = session.Items[0];
        Set(it, L4CompletionReviewAdapter.FieldDecision, "accept");
        Set(it, L4CompletionReviewAdapter.FieldReason, "");
        Assert.False(L4CompletionReviewAdapter.Resolved(it));
        Set(it, L4CompletionReviewAdapter.FieldReason, "sinnvoll");
        Assert.True(L4CompletionReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var proposals = Proposals(Item("PROP-1"), Item("PROP-2"));
        var session = L4CompletionReviewAdapter.BuildSession("r1", Adequacy(), proposals, Baseline(), "all");
        Set(session.Items[0], L4CompletionReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], L4CompletionReviewAdapter.FieldReason, "ok");
        Set(session.Items[1], L4CompletionReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], L4CompletionReviewAdapter.FieldReason, "kein Bedarf");

        var file = L4CompletionReviewAdapter.Apply("r1", session, proposals);
        Assert.Equal(["PROP-1", "PROP-2"], file.Decisions.Select(d => d.ProposalItemId));
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));

        var fresh = L4CompletionReviewAdapter.BuildSession("r1", Adequacy(), proposals, Baseline(), "all");
        L4CompletionReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], L4CompletionReviewAdapter.FieldDecision));
        Assert.True(L4CompletionReviewAdapter.Resolved(fresh.Items[1]));
    }
}
