using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Sonderling 2: DerivedRisks-Review (Inferenz-Schicht, Capstone-nah). Abweichendes Apply:
// liefert (Approved-Items, Audit-Protokoll) statt einer Decisions-Datei; nur "approve" landet in Approved.
public sealed class DerivedRisksReviewAdapterTests
{
    private static ArtifactItem Risk(string id = "RISK-1") => new(
        ItemId: id, Origin: ArtifactOrigin.Derived, Text: $"Risiko {id}",
        SourceClaimIds: [], SourceArtifactItemIds: ["REQ-1"],
        Assumptions: ["Annahme A"], DerivationRationale: "folgt aus REQ-1");

    private static ArtifactDocument Doc(params ArtifactItem[] items) => new(
        ArtifactId: "risks-1", ArtifactType: "risks", Version: 1, Stage: "derived",
        Producer: new ProducerMetadata("run-1", "test-model", null), Items: items);

    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_je_Risiko_und_Resolved_braucht_nur_Entscheidung()
    {
        var session = DerivedRisksReviewAdapter.BuildSession(Doc(Risk("RISK-1"), Risk("RISK-2")),
            new Dictionary<string, InferenceVerdict>());
        Assert.Equal(["RISK-1", "RISK-2"], session.Items.Select(i => i.ItemId));

        var it = session.Items[0];
        Set(it, DerivedRisksReviewAdapter.FieldDecision, "");
        Assert.False(DerivedRisksReviewAdapter.Resolved(it));
        Set(it, DerivedRisksReviewAdapter.FieldDecision, "approve");
        Assert.True(DerivedRisksReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_liefert_nur_approve_Items_aber_das_volle_Protokoll()
    {
        var doc = Doc(Risk("RISK-1"), Risk("RISK-2"), Risk("RISK-3"));
        var session = DerivedRisksReviewAdapter.BuildSession(doc, new Dictionary<string, InferenceVerdict>());
        Set(session.Items[0], DerivedRisksReviewAdapter.FieldDecision, "approve");
        Set(session.Items[1], DerivedRisksReviewAdapter.FieldDecision, "reject");
        Set(session.Items[2], DerivedRisksReviewAdapter.FieldDecision, "needs_revision");

        var byId = doc.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var (approved, decisions) = DerivedRisksReviewAdapter.Apply(session, byId);

        Assert.Equal(["RISK-1"], approved.Select(a => a.ItemId));   // nur approve
        Assert.Equal(3, decisions.Count);                            // aber ALLE protokolliert (Audit)
    }
}
