using AgenticSdlc.Host.FullWorkflow.Gap;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Gap/L3): Open-World-Kandidaten freigeben. Besonderheiten:
// ItemId = CandidateId; SupportedAnchored ist per Default AUSGEBLENDET (nur die Human-Review-Klassen);
// edit verlangt editedText, sonst nicht resolved.
public sealed class L3ReviewAdapterTests
{
    private static L3RoutedCandidate Routed(string id, L3Class cls) => new(
        Candidate: new L3Candidate(CandidateId: id, TargetType: "requirement", Text: $"Kandidat {id}",
            Rationale: "abgeleitet", Assumptions: []),
        Anchors: [], NoAnchorReason: cls == L3Class.SupportedAnchored ? null : "kein Anker",
        Class: cls, KeptAnchorIds: [], UnknownAnchorIds: []);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_blendet_SupportedAnchored_per_Default_aus()
    {
        var routed = new[] { Routed("CAND-1", L3Class.SupportedAnchored), Routed("CAND-2", L3Class.Unreferenced) };
        var session = L3ReviewAdapter.BuildSession("r1", routed);
        Assert.Equal(["CAND-2"], session.Items.Select(i => i.ItemId));

        var withSupported = L3ReviewAdapter.BuildSession("r1", routed, includeSupportedAnchored: true);
        Assert.Equal(2, withSupported.Items.Count);
    }

    [Fact]
    public void Edit_verlangt_editedText()
    {
        var session = L3ReviewAdapter.BuildSession("r1", [Routed("CAND-1", L3Class.Unreferenced)]);
        var it = session.Items[0];
        Set(it, L3ReviewAdapter.FieldDecision, "edit");
        Set(it, L3ReviewAdapter.FieldReason, "Formulierung schaerfen");
        Set(it, L3ReviewAdapter.FieldEdited, "");
        Assert.False(L3ReviewAdapter.Resolved(it));

        Set(it, L3ReviewAdapter.FieldEdited, "Der Nutzer kann X praezise.");
        Assert.True(L3ReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var routed = new[] { Routed("CAND-1", L3Class.Unreferenced), Routed("CAND-2", L3Class.Contradicted) };
        var session = L3ReviewAdapter.BuildSession("r1", routed);
        Set(session.Items[0], L3ReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], L3ReviewAdapter.FieldReason, "traegt");
        Set(session.Items[1], L3ReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], L3ReviewAdapter.FieldReason, "Widerspruch bestaetigt");

        var file = L3ReviewAdapter.Apply("r1", session);
        Assert.Equal(["CAND-1", "CAND-2"], file.Decisions.Select(d => d.CandidateId));
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));

        var fresh = L3ReviewAdapter.BuildSession("r1", routed);
        L3ReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], L3ReviewAdapter.FieldDecision));
        Assert.True(L3ReviewAdapter.Resolved(fresh.Items[1]));
    }
}
