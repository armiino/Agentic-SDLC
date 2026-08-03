using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/ReClarify): PBIs freigeben (Initial-Backlog vor dem Core-Seed!).
// ItemId = PbiId; scope "blocked" filtert auf blockierende offene Entscheidungen/Readiness.
public sealed class ReClarifyBacklogReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProductBacklogItem Pbi(string id = "PBI-1", string? readiness = null)
        => new ProductBacklogItem(PbiId: id, IdentityKey: $"key-{id}", Version: 1, Type: "pbi",
               Title: $"Titel {id}", RequirementIds: ["CAN-REQ-1"]) with { Readiness = readiness };

    private static ProductBacklogDocument Backlog(params ProductBacklogItem[] items)
        => new(1, "bl-1", "p1", "b1", T, "clusters.json", items);

    private static CanonicalRequirementsBaseline Baseline() => new("b1", "p1", 1, T, "state.json", [], [], []);

    private static ReClarifyGateReport Gate() => new(true, "pass", [], [], new Dictionary<string, object>());

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_nutzt_PbiId_scope_blocked_filtert()
    {
        var backlog = Backlog(Pbi("PBI-1"), Pbi("PBI-2", readiness: "blocked_by_decision"));
        var all = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), backlog, Gate(), "all");
        Assert.Equal(["PBI-1", "PBI-2"], all.Items.Select(i => i.ItemId));

        var blocked = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), backlog, Gate(), "blocked");
        Assert.Equal(["PBI-2"], blocked.Items.Select(i => i.ItemId));
    }

    // E0.1-Retrofit: die Badge ist Klartext (Readiness zuerst), nie ein rohes Enum.
    [Fact]
    public void Badge_ist_Klartext_nicht_roh_Enum()
    {
        var backlog = Backlog(Pbi("PBI-1", readiness: "backlog_ready"), Pbi("PBI-2", readiness: "blocked_by_decision"));
        var s = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), backlog, Gate(), "all");
        Assert.Equal("bereit", s.Items[0].Badge);
        Assert.Equal("blockiert · Entscheidung", s.Items[1].Badge);
        Assert.DoesNotContain(s.Items, i => i.Badge is "backlog_ready" or "blocked_by_decision");
    }

    // E0.1b/i: Items starten OHNE Vorentscheid; Begruendung ist nur beim Verwerfen Pflicht (Audit-Gewicht) —
    // bei accept ist der explizite Entscheid selbst die Autorisierung.
    [Fact]
    public void Resolved_startet_offen_accept_ohne_Begruendung_reject_nur_mit()
    {
        var session = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), Backlog(Pbi()), Gate(), "all");
        var it = session.Items[0];
        Assert.False(it.Resolved);
        Assert.Equal("", FieldOf(it, ReClarifyBacklogReviewAdapter.FieldDecision));

        Set(it, ReClarifyBacklogReviewAdapter.FieldDecision, "accept");
        Assert.True(ReClarifyBacklogReviewAdapter.Resolved(it));

        Set(it, ReClarifyBacklogReviewAdapter.FieldDecision, "reject");
        Assert.False(ReClarifyBacklogReviewAdapter.Resolved(it));
        Set(it, ReClarifyBacklogReviewAdapter.FieldReason, "kein echtes PBI");
        Assert.True(ReClarifyBacklogReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var backlog = Backlog(Pbi("PBI-1"), Pbi("PBI-2"));
        var session = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), backlog, Gate(), "all");
        Set(session.Items[0], ReClarifyBacklogReviewAdapter.FieldDecision, "accept");
        Set(session.Items[0], ReClarifyBacklogReviewAdapter.FieldReason, "ok");
        Set(session.Items[1], ReClarifyBacklogReviewAdapter.FieldDecision, "reject");
        Set(session.Items[1], ReClarifyBacklogReviewAdapter.FieldReason, "kein echtes PBI");

        var file = ReClarifyBacklogReviewAdapter.Apply("r1", session, backlog);
        Assert.Equal(["PBI-1", "PBI-2"], file.Decisions.Select(d => d.PbiId));
        Assert.Equal(["accept", "reject"], file.Decisions.Select(d => d.Decision));

        var fresh = ReClarifyBacklogReviewAdapter.BuildSession("r1", Baseline(), backlog, Gate(), "all");
        ReClarifyBacklogReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("reject", FieldOf(fresh.Items[1], ReClarifyBacklogReviewAdapter.FieldDecision));
        Assert.True(ReClarifyBacklogReviewAdapter.Resolved(fresh.Items[1]));
    }
}
