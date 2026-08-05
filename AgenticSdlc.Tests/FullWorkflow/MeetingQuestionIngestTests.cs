using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 9g S2: der Kern der Fragen-Rampe — eine im Meeting gestellte Frage wird via Tor 1 zur offenen Entscheidung
// im VORHANDENEN DEC-Topf. Getestet: Gate-Kategorien-Wachen (Frage<->Anforderung, beide Richtungen), Coverage
// fuer Fragen, Apply-Mint (Provenienz/I7, keine Relation, Kangal-konform), Platzierungs-Neutralitaet und der
// idempotente Bootstrap-Mint (geteilte Semantik, Zwei-Bahnen-Regel).
public sealed class MeetingQuestionIngestTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;
    private const string IngestRun = "20260805_ingest_run";

    private static ProjectStateItem Item(string id, string type, string text, string run = "delta-run", string status = "accepted") => new ProjectStateItem(
        id, type, text, "MEETING", null, 1,
        run, null, null, null, null, ["claim-7"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items) => new("p", 4, T, [], [.. items], [], [], []);

    private static StateChangeOperation Op(string incoming, string kind, string? target = null, string statement = "Dürfen Angehörige eintragen?") =>
        new(incoming, kind, statement, target, null, ["claim-7"], "aus dem Meeting");

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", T, "delta-path", [.. ops]);

    [Fact]
    public void Gate_akzeptiert_OPEN_QUESTION_fuer_eingehende_Frage_und_verlangt_Coverage()
    {
        var delta = Doc(Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"));
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));

        var ok = IngestionGate.Check(delta, core, Plan(Op("OQ-01", StateChangeKind.OpenQuestion)));
        Assert.True(ok.Pass);

        var unplaced = IngestionGate.Check(delta, core, Plan());
        var issue = Assert.Single(unplaced.Errors, e => e.Code == "UNPLACED_INCOMING");
        Assert.Equal("OQ-01", issue.IncomingItemId);                            // Fragen sind Coverage-Buerger
    }

    [Fact]
    public void Gate_Kategorien_Wache_wirkt_in_beide_Richtungen()
    {
        var delta = Doc(
            Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"),
            Item("IN-1", "requirement", "Angehörige erhalten Leserechte", status: "baseline"));
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));

        // Frage mit Requirement-Op -> Fehler; Requirement mit Frage-Op -> Fehler (beides repairable).
        var report = IngestionGate.Check(delta, core, Plan(
            Op("OQ-01", StateChangeKind.New),
            Op("IN-1", StateChangeKind.OpenQuestion)));

        Assert.False(report.Pass);
        Assert.Equal(2, report.Errors.Count(e => e.Code == "QUESTION_KIND_MISMATCH"));
        Assert.All(report.Errors.Where(e => e.Code == "QUESTION_KIND_MISMATCH"),
            e => Assert.Equal(Repairability.Repairable, e.Repairability));
    }

    [Fact]
    public void Gate_erlaubt_ALREADY_DECIDED_fuer_bereits_bekannte_Frage()
    {
        var delta = Doc(Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"));
        var core = Doc(Item("DEC-001", "decision", "Dürfen Angehörige eintragen?", status: "open_decision"));

        var report = IngestionGate.Check(delta, core, Plan(Op("OQ-01", StateChangeKind.AlreadyDecided, target: "DEC-001")));
        Assert.True(report.Pass);
    }

    [Fact]
    public void Apply_praegt_aus_der_Frage_eine_offene_DEC_mit_voller_Provenienz_ohne_Relation()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"), Item("DEC-004", "decision", "alt", status: "resolved"));
        var delta = Doc(Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline"));

        var (updated, report, affected) = IngestionApply.Apply(core, delta, Plan(Op("OQ-01", StateChangeKind.OpenQuestion)),
            new HashSet<string>(StringComparer.Ordinal) { "OQ-01" }, IngestRun);

        var op = Assert.Single(report.Applied);
        Assert.Equal("question_opened", op.Outcome);
        Assert.Equal(1, report.Delta.Questions);

        var dec = updated.Items.Single(i => i.ItemId == op.EntityId);
        Assert.Equal("DEC-005", dec.ItemId);                                   // saubere naechste Muenze (nach DEC-004)
        Assert.True(dec.ReadStatus().IsOpenDecision);
        Assert.Equal(MeetingQuestionMint.Origin, dec.Origin);
        Assert.Equal(IngestRun, dec.SourceRunId);                              // I7: Ausloeser-Lauf
        Assert.Equal("OQ-01", dec.Metadata["ingestedFrom"]);
        Assert.Equal("delta-run", dec.Metadata["ingestedFromRun"]);
        Assert.Contains("claim-7", dec.SourceClaimIds);                        // Beleg-Kette bis zum Zitat
        Assert.Empty(updated.Relations);                                        // BEWUSST keine Kante (kein Widerspruch)
        Assert.Contains(dec.ItemId, affected);

        Assert.True(CoreKangal.Check(updated).Pass);                           // Kangal-konform ohne Sonderregel
    }

    [Fact]
    public void Derivation_Frage_hat_keine_Platzierungs_Wirkung()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var result = PbiUpdateDerivation.Derive(core, [new AppliedOperation("OQ-01", StateChangeKind.OpenQuestion, "DEC-005", "question_opened")]);

        Assert.Empty(result.DeterministicOps);
        Assert.Empty(result.Unplaced);
    }

    [Fact]
    public void Bootstrap_Mint_ist_idempotent_per_IdentityKey_und_nennt_Bekanntes()
    {
        var core = Doc(Item("REQ-1", "requirement", "bestehend"));
        var q1 = Item("OQ-01", "open_question", "Dürfen Angehörige eintragen?", status: "baseline");
        var q2 = Item("OQ-02", "open_question", "Gilt die Frist auch offline?", status: "baseline");

        var (once, minted, skipped) = MeetingQuestionMint.Mint(core, [q1, q2], "bootstrap-run");
        Assert.Equal(2, minted.Count);
        Assert.Empty(skipped);
        Assert.All(minted, id => Assert.True(once.Items.Single(i => i.ItemId == id).ReadStatus().IsOpenDecision));

        // Zweiter Lauf mit derselben Frage: kein Duplikat — auch nicht, wenn die DEC inzwischen aufgeloest waere.
        var (twice, mintedAgain, skippedAgain) = MeetingQuestionMint.Mint(once, [q1], "bootstrap-run-2");
        Assert.Empty(mintedAgain);
        Assert.Contains(skippedAgain, s => s.Contains("OQ-01"));
        Assert.Equal(once.Items.Count, twice.Items.Count);
    }
}
