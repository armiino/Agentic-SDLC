using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (E4-Stufe): Reverse-Vorschlaege muessen Entscheidungen verlustfrei tragen.
public sealed class GithubReverseReviewAdapterTests
{
    private static GithubReverseOp Op(string kind = "PBI_DONE", string pbi = "PBI-1") => new(
        Kind: kind, PbiId: pbi, IssueNumber: 60, CurrentPbiStatus: "active",
        ProposedPbiStatus: "done", RequiresVerification: true, Rationale: "Issue closed");

    private static GithubReversePlanDocument Plan(params GithubReverseOp[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, Snapshot: null, Operations: ops);

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";
    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_je_Op()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op("FLAG_REOPENED", "PBI-2")));
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op()));
        var it = session.Items[0];
        it.FieldValues.RemoveAll(f => f.FieldKey == GithubReverseReviewAdapter.FieldDecision);
        it.FieldValues.Add(new ReviewFieldValue(GithubReverseReviewAdapter.FieldDecision, "skip"));
        it.FieldValues.RemoveAll(f => f.FieldKey == GithubReverseReviewAdapter.FieldReason);
        it.FieldValues.Add(new ReviewFieldValue(GithubReverseReviewAdapter.FieldReason, "nicht verifiziert"));

        var file = GithubReverseReviewAdapter.Apply("r1", session);

        var fresh = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op()));
        GithubReverseReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[0], GithubReverseReviewAdapter.FieldDecision));
        Assert.Equal("nicht verifiziert", FieldOf(fresh.Items[0], GithubReverseReviewAdapter.FieldReason));
        Assert.True(fresh.Items[0].Resolved);
    }

    [Fact]
    public void Leere_Entscheidung_ist_nicht_resolved()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op()));
        GithubReverseReviewAdapter.MergeExistingDecisions(session,
            new GithubReverseDecisionsFile("r1", "x", [new GithubReverseDecision("op-0", "", null)]));
        Assert.False(session.Items[0].Resolved);
    }

    // E0.5: Klartext-Optionen je Op-Art (interner Value bleibt apply/skip; Label ist op-spezifisch).
    [Fact]
    public void Entscheidungs_Optionen_sind_je_OpArt_in_Klartext()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op(), Op("FLAG_REOPENED", "PBI-2")));
        var done = session.Items[0].FieldOptions[GithubReverseReviewAdapter.FieldDecision];
        var flag = session.Items[1].FieldOptions[GithubReverseReviewAdapter.FieldDecision];
        Assert.Equal(["apply", "skip"], done.Select(o => o.Value));         // Werte unveraendert (Apply-Vertrag)
        Assert.Contains("fertig", done[0].Label, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(done[0].Label, flag[0].Label);                      // Fertig-Meldung != Drift-Warnung
    }

    // E0.5: mit Kontext zeigt die Summary den PBI-Titel und dass das Issue geschlossen ist.
    [Fact]
    public void Summary_zeigt_PBI_Titel_und_geschlossenes_Issue_mit_Kontext()
    {
        var ctx = new ReverseReviewContext(
            new Dictionary<string, string> { ["PBI-1"] = "Medikamentengabe protokollieren" },
            new Dictionary<int, GithubIssueSnapshot>
            {
                [60] = new(60, "http://x/60", "Doku-Issue", null, "closed", [], null, null)
            });
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op()), ctx);
        Assert.Contains("Medikamentengabe protokollieren", session.Items[0].Summary);
        Assert.Contains("geschlossen", session.Items[0].Summary);
    }

    // E0.5: die deklarierte Experiment-Bulk-Linie ist vorhanden und setzt apply.
    [Fact]
    public void Experiment_Bulk_Linie_ist_gesetzt()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op()));
        Assert.NotNull(session.BulkAction);
        Assert.Contains(session.BulkAction!.Set, s => s.FieldKey == GithubReverseReviewAdapter.FieldDecision && s.Value == "apply");
    }

    // E0.9-P2a: Ablehnen ohne Begründung gilt nicht als erledigt (Audit-Symmetrie).
    [Fact]
    public void Skip_ohne_Begruendung_ist_nicht_resolved()
    {
        var session = GithubReverseReviewAdapter.BuildSession("r1", Plan(Op()));
        var it = session.Items[0];
        Set(it, GithubReverseReviewAdapter.FieldDecision, "skip");
        Assert.False(GithubReverseReviewAdapter.Resolved(it));
        Set(it, GithubReverseReviewAdapter.FieldReason, "Vorschlag passt fachlich nicht");
        Assert.True(GithubReverseReviewAdapter.Resolved(it));
    }
}
