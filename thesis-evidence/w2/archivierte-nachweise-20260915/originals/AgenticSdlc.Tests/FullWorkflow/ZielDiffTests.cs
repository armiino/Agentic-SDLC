using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1c-① Ziel-Diff (18.08., Block-L-Kernfall REQ-42 + ⚖ Autor „Ersetzen bleibt"): bei REFINE/SUPERSEDE bringt
// die Gate-Karte das Wissen MIT — voller Ist-Text und voller Vorschlag untereinander. Vorher war Text-Verlust
// (der neue Text ersetzt vollständig!) nur mit Reviewer-Gedächtnis erkennbar. EINE Quelle (ZielDiff) für
// UI-Karte UND Chat-Vorlage; NIE gekürzt — Kürzung würde genau das Weggelassene verstecken.
public sealed class ZielDiffTests
{
    private static readonly string LangerIstText =
        "Pflegekräfte können an einem Besuch eine Bemerkung erfassen (optional, max. 200 Zeichen, nur bei der "
        + "Erfassung änderbar); die Wochenübersicht zeigt die letzten 7 Tage inklusive dieser Bemerkungen.";

    private static ProjectStateItem Item(string id, string type, string text) => new ProjectStateItem(
        id, type, text, "MEETING", null, 1, "run-alt", null, null, null, null, [], [],
        new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static StateChangeOperation Op(string kind, string statement, string? target) =>
        new("AF-1", kind, statement, target, null, ["claim-1"], "aus dem Meeting");

    [Fact]
    public void ZielDiff_liefert_bei_REFINE_und_SUPERSEDE_beide_VOLLEN_Texte()
    {
        var coreById = new Dictionary<string, ProjectStateItem> { ["REQ-42"] = Item("REQ-42", "requirement", LangerIstText) };

        var refine = IngestionReviewAdapter.ZielDiff(Op(StateChangeKind.Refine, "Bemerkungen werden versioniert.", "REQ-42"), coreById);
        Assert.Equal(LangerIstText, refine!.Value.GiltHeute);                  // voll, NICHT gekürzt
        Assert.Equal("Bemerkungen werden versioniert.", refine.Value.StuendeDanach);

        Assert.NotNull(IngestionReviewAdapter.ZielDiff(Op(StateChangeKind.Supersede, "Neu.", "REQ-42"), coreById));
        Assert.Null(IngestionReviewAdapter.ZielDiff(Op(StateChangeKind.New, "Neu.", null), coreById));       // nur Ersetz-Arten
        Assert.Null(IngestionReviewAdapter.ZielDiff(Op(StateChangeKind.Refine, "Neu.", "REQ-99"), coreById)); // fail-open ohne Ziel
    }

    [Fact]
    public void Gate_Karte_traegt_das_Diff_Paar_mit_vollen_Texten()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [Item("REQ-42", "requirement", LangerIstText)], [], [], []);
        var delta = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [Item("AF-1", "requirement", "Bemerkungen werden versioniert.")], [], [], []);
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "plan-1",
            DateTime.UnixEpoch, "delta", [Op(StateChangeKind.Refine, "Bemerkungen werden versioniert.", "REQ-42")]);

        var notes = IngestionReviewAdapter.BuildSession("r1", plan, delta, core).Items[0].Notes;

        var heute = Assert.Single(notes, n => n.Label == "Das gilt heute (REQ-42)");
        Assert.Equal(LangerIstText, heute.Text);                               // der Verlust-Wächter: voller Text
        var danach = Assert.Single(notes, n => n.Label.StartsWith("Das stünde danach"));
        Assert.Contains("Bemerkungen werden versioniert.", danach.Text);
        Assert.Contains("fallen aus der gültigen Wahrheit", danach.Text);      // die Ersetzen-Semantik steht dabei
    }
}
