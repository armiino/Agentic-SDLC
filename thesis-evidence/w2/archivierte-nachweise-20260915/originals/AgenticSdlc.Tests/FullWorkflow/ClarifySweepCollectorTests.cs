using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C4a (09.08., c4-plan §5 a/e) — der deterministische Katalog-Collector: Lücken-Diagnose je Art,
// nur needs_clarify-PBIs, zweig-agnostisch (Origin egal — auch Bootstrap-geseedete Kandidaten).
public sealed class ClarifySweepCollectorTests
{
    private static ProjectStateItem Pbi(string id, string status, string? goal, IReadOnlyList<string>? ak, string origin = "re-clarify") =>
        (new ProjectStateItem(id, "pbi", $"{id} Text", origin, null, 1, "r", null, null, null, null, [], [],
            new Dictionary<string, string>()) with
        { Pbi = new PbiPayload(goal, $"{id} Titel", ak ?? [], ["REQ-1"], [], null, "active", null, null) })
        .WithStatus(CoreStatus.From(status));

    [Fact]
    public void Diagnose_je_Lueckenart_und_nur_needs_clarify()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [Pbi("PBI-1", "needs_clarify", goal: null, ak: ["AK"]),                    // Statement fehlt
             Pbi("PBI-2", "needs_clarify", goal: "Ziel", ak: null),                    // AK leer
             Pbi("PBI-3", "needs_clarify", goal: "Ziel", ak: ["AK"], origin: "bootstrap-seed"), // e) Zweig-agnostisch
             Pbi("PBI-4", "active", goal: null, ak: null),                             // aktiv → NICHT im Katalog
             new ProjectStateItem("REQ-1", "requirement", "Das System muss X.", "test", null, 1, "r",
                 null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"))],
            [], [], []);

        var k = ClarifySweepCollector.Collect(core);

        Assert.Equal(3, k.OffeneKlaerungen);
        Assert.Contains(k.Eintraege.Single(e => e.PbiId == "PBI-1").Luecken, l => l.StartsWith("STATEMENT_FEHLT"));
        Assert.Contains(k.Eintraege.Single(e => e.PbiId == "PBI-2").Luecken, l => l.StartsWith("AK_LEER"));
        var boot = k.Eintraege.Single(e => e.PbiId == "PBI-3");
        Assert.Equal("bootstrap-seed", boot.Origin);                                    // e) Origin egal, Diagnose läuft
        Assert.Contains(boot.Luecken, l => l.StartsWith("UNSPEZIFISCH"));               // keine Einzel-Lücke → ehrlich benannt
        Assert.Equal("Das System muss X.", boot.VerlinkteRequirements.Single().Text);   // REQ-Kontext aufgelöst
        Assert.DoesNotContain(k.Eintraege, e => e.PbiId == "PBI-4");
    }
}
