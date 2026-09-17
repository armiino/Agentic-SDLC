using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Backlog/ReClarify-Cluster): die vom ReviewAgent vorgeschlagenen Cluster-KORREKTUREN
// werden apply/skip-freigegeben (Items aus review.Operations, ItemId = OpId).
public sealed class ReClarifyClusterReviewAdapterTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ClusterOperation Op(string id = "op-1", string kind = "add_crosscutting")
        => new(OpId: id, Kind: kind, Rationale: "Querschnitt fehlt") { RequirementId = "CAN-REQ-1", ToClusterId = "CL-1" };

    private static ClusterReviewReport Review(params ClusterOperation[] ops)
        => new(1, "rev-1", "revise", "Korrekturen noetig", []) { Operations = ops };

    private static FeatureClusterSet Clusters()
        => new(1, "cs-1", "p1", "b1", T, "baseline.json",
               [new FeatureCluster("CL-1", "key-cl1", "No-Go-Management", ["CAN-REQ-1"], [], null)]);

    private static CanonicalRequirementsBaseline Baseline() => new("b1", "p1", 1, T, "state.json", [], [], []);

    private static ReClarifyGateReport Gate() => new(true, "pass", [], [], new Dictionary<string, object>());

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_aus_Review_Operationen()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op("op-1"), Op("op-2", "move_core")), "all");
        Assert.Equal(["op-1", "op-2"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var review = Review(Op("op-1"), Op("op-2"));
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), review, "all");
        Set(session.Items[1], ReClarifyClusterReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], ReClarifyClusterReviewAdapter.FieldReason, "Cluster passt schon");

        var file = ReClarifyClusterReviewAdapter.Apply("r1", session);
        Assert.Equal(["op-1", "op-2"], file.Decisions.Select(d => d.OpId));
        Assert.Equal("skip", file.Decisions[1].Decision);

        var fresh = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), review, "all");
        ReClarifyClusterReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], ReClarifyClusterReviewAdapter.FieldDecision));
    }

    [Fact]
    public void Leere_Entscheidung_ist_nicht_resolved()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op()), "all");
        ReClarifyClusterReviewAdapter.MergeExistingDecisions(session,
            new ClusterHumanDecisionsFile("r1", "x", [new ClusterHumanDecision("op-1", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }

    // E0.6: Default ist LEER (aktive Absegnung), nicht mehr vorbelegtes apply.
    [Fact]
    public void Default_Entscheidung_ist_leer()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op()), "all");
        Assert.Equal("", FieldOf(session.Items[0], ReClarifyClusterReviewAdapter.FieldDecision));
        Assert.False(session.Items[0].Resolved);
    }

    // E0.6: Klartext-Optionen je Op-Art (interner Value bleibt apply/skip).
    [Fact]
    public void Entscheidungs_Optionen_sind_je_OpArt_in_Klartext()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(),
            Review(Op("op-1", "move_core"), Op("op-2", "merge_clusters")), "all");
        var move = session.Items[0].FieldOptions[ReClarifyClusterReviewAdapter.FieldDecision];
        var merge = session.Items[1].FieldOptions[ReClarifyClusterReviewAdapter.FieldDecision];
        Assert.Equal(["apply", "skip"], move.Select(o => o.Value));
        Assert.Contains("Verschiebung", move[0].Label);
        Assert.NotEqual(move[0].Label, merge[0].Label);
    }

    // E0.6: die deklarierte Experiment-Bulk-Linie ist vorhanden und setzt apply.
    [Fact]
    public void Experiment_Bulk_Linie_ist_gesetzt()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op()), "all");
        Assert.NotNull(session.BulkAction);
        Assert.Contains(session.BulkAction!.Set, s => s.FieldKey == ReClarifyClusterReviewAdapter.FieldDecision && s.Value == "apply");
    }

    // E0.6: der Kontext ist ein lesbarer Vorher→Nachher-Diff (kein rohes JSON).
    [Fact]
    public void ResolveContext_zeigt_Vorher_Nachher_Diff()
    {
        var clusters = new FeatureClusterSet(1, "cs", "p1", "b1", T, "baseline.json",
        [
            new FeatureCluster("CL-1", "k1", "Profil", ["CAN-REQ-1"], [], null),
            new FeatureCluster("CL-2", "k2", "Medikamente", ["CAN-REQ-2"], [], null)
        ]);
        var move = new ClusterOperation("op-1", "move_core", "gehört zu Medikamente")
            { RequirementId = "CAN-REQ-1", FromClusterId = "CL-1", ToClusterId = "CL-2" };
        var review = Review(move);

        var text = ReClarifyClusterReviewAdapter.ResolveContext("op:op-1", Baseline(), clusters, Gate(), review);

        Assert.Contains("Vorher:", text);
        Assert.Contains("Nachher:", text);
        Assert.Contains("CAN-REQ-1", text);
        Assert.Contains("Medikamente", text);
        Assert.DoesNotContain("{", text); // kein rohes JSON mehr
    }

    // E0.9-P2a: Ablehnen ohne Begründung gilt nicht als erledigt (Audit-Symmetrie).
    [Fact]
    public void Skip_ohne_Begruendung_ist_nicht_resolved()
    {
        var session = ReClarifyClusterReviewAdapter.BuildSession("r1", Baseline(), Clusters(), Gate(), Review(Op()), "all");
        var it = session.Items[0];
        Set(it, ReClarifyClusterReviewAdapter.FieldDecision, "skip");
        Assert.False(ReClarifyClusterReviewAdapter.Resolved(it));
        Set(it, ReClarifyClusterReviewAdapter.FieldReason, "Vorschlag passt fachlich nicht");
        Assert.True(ReClarifyClusterReviewAdapter.Resolved(it));
    }
}
