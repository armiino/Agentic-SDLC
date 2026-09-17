using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Slice S ④ (21.08.): die Risiken-Rampe — Meeting-Risiken fahren die 9g-QuestionLane („Risiken gehen nicht
// verloren, sie werden Entscheidungen"): risks ist BESTELLT, das Gate behandelt risk-Items als Coverage-
// Bürger mit Frage-Vokabular, Apply/Bootstrap münzen eine DEC mit EIGENER Risiko-Herkunft.
public sealed class RiskLaneTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        "delta-run", null, null, null, null, ["claim-9"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items) => new("p", 4, T, [], [.. items], [], [], []);

    private static StateChangeOperation Op(string incoming, string kind, string statement = "Risiko: Datenverlust bei Offline-Sync.") =>
        new(incoming, kind, statement, null, null, ["claim-9"], "aus dem Meeting");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", T, "delta-path", [.. ops]);

    [Fact]
    public void Risks_Spur_ist_bestellt_und_die_Lane_kennt_beide_Typen()
    {
        Assert.Contains("risks", BaselineStageExecutor.OrderedArtifacts);       // Besteller existiert (tote Spur zu)
        Assert.True(QuestionLane.Carries("open_question"));
        Assert.True(QuestionLane.Carries("risk"));
        Assert.False(QuestionLane.Carries("requirement"));
    }

    [Fact]
    public void Gate_behandelt_Risiko_als_Coverage_Buerger_mit_Frage_Vokabular()
    {
        var delta = Doc(Item("RISK-01", "risk", "Risiko: Datenverlust bei Offline-Sync.", status: "baseline"));
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));

        Assert.True(IngestionGate.Check(delta, core, Plan(Op("RISK-01", StateChangeKind.OpenQuestion))).Pass);

        var unplaced = IngestionGate.Check(delta, core, Plan());
        Assert.Equal("RISK-01", Assert.Single(unplaced.Errors, e => e.Code == "UNPLACED_INCOMING").IncomingItemId);

        var wrong = IngestionGate.Check(delta, core, Plan(Op("RISK-01", StateChangeKind.New)));
        Assert.Contains(wrong.Errors, e => e.Code == "QUESTION_KIND_MISMATCH"); // Risiko nie als Anforderung umdeuten
    }

    [Fact]
    public void Apply_praegt_aus_dem_Risiko_eine_DEC_mit_Risiko_Herkunft()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var delta = Doc(Item("RISK-01", "risk", "Risiko: Datenverlust bei Offline-Sync.", status: "baseline"));

        var (updated, report, _) = IngestionApply.Apply(core, delta, Plan(Op("RISK-01", StateChangeKind.OpenQuestion)),
            new HashSet<string>(StringComparer.Ordinal) { "RISK-01" }, "ingest-run");

        var dec = updated.Items.Single(i => i.ItemId == Assert.Single(report.Applied).EntityId);
        Assert.True(dec.ReadStatus().IsOpenDecision);
        Assert.Equal(MeetingQuestionMint.OriginRisk, dec.Origin);               // Risiko ≠ Frage (W2-Herkunfts-Achse)
        Assert.Contains("claim-9", dec.SourceClaimIds);                         // Beleg-Kette bleibt
        Assert.Empty(updated.Relations);
        Assert.True(CoreKangal.Check(updated).Pass);
    }

    [Fact]
    public void Diktat_Risiko_faehrt_als_risk_Item_und_praegt_AUTHOR_RISK()
    {
        var (delta, errors) = AuthorFrontDeltaBuilder.Build(
            [new AuthorStatement("Risiko: PAT-Rotation wird vergessen.", "risk")], "sess-1");
        Assert.Empty(errors);
        var item = Assert.Single(delta!.Items);
        Assert.Equal("risk", item.ItemType);                                    // Diktat-Bahn trägt die Spur

        Assert.Equal(MeetingQuestionMint.OriginRiskAuthor,
            MeetingQuestionMint.NewDecision("DEC-001", item.Text, item, [], "run").Origin);
    }

    [Fact]
    public void Bootstrap_Seed_muenzt_Risiken_wie_Fragen_statt_sie_zu_seeden()
    {
        var source = Doc(
            Item("REQ-1", "requirement", "bestehend"),
            Item("RISK-01", "risk", "Risiko: Adoption im Pflege-Alltag.", status: "baseline"));

        var (core, report) = CoreSeeder.Seed(source, "seed-run");

        Assert.Equal(1, report.QuestionDecs);
        Assert.DoesNotContain(core.Items, i => i.ItemType == "risk");           // kein Risiko-Wahrheits-Typ (v1)
        var dec = Assert.Single(core.Items, i => i.ItemType == "decision");
        Assert.Equal(MeetingQuestionMint.OriginRisk, dec.Origin);
        Assert.True(dec.ReadStatus().IsOpenDecision);
    }
}
