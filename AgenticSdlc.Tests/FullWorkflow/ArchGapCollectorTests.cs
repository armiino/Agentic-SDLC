using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Freeze-Schritt 3 (09.08.) — der Arch-Lücken-Katalog: drei Lücken-Arten deterministisch aus den
// R-11-Relationen/Payloads; superseded/gerahmte/verADRte Items bleiben draußen.
public sealed class ArchGapCollectorTests
{
    private static ProjectStateItem Item(string id, string type, string status = "active", ArchitecturePayload? arch = null) =>
        (new ProjectStateItem(id, type, $"{id} Text", "test", null, 1, "r", null, null, null, null, [], [],
            new Dictionary<string, string>()) with { Architecture = arch })
        .WithStatus(CoreStatus.From(status));

    [Fact]
    public void Drei_Lueckenarten_korrekt_diagnostiziert()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [Item("PBI-1", "pbi"),                                                     // ohne Rahmen ⇒ Lücke A
             Item("PBI-2", "pbi"),                                                     // gerahmt ⇒ keine Lücke
             Item("PBI-3", "pbi", "superseded"),                                       // superseded ⇒ raus
             Item("ARCH-1", "architecture", arch: new ArchitecturePayload(["design"], null)),          // design ohne ADR ⇒ B
             Item("ARCH-2", "architecture", arch: new ArchitecturePayload(["design"], null, "ADR-0007", "accepted")), // hat ADR
             Item("ARCH-3", "architecture")],                                          // unklassifiziert ⇒ C
            [new("PBI-2", "ARCH-2", "constrained_by", "test", new Dictionary<string, string>()),
             new("PBI-1", "FC-01", "part_of_feature", "test", new Dictionary<string, string>())],
            [], []);

        var k = ArchGapCollector.Collect(core);

        Assert.Equal(3, k.OffeneLuecken);
        var a = Assert.Single(k.ArbeitOhneRahmen);
        Assert.Equal(("PBI-1", "FC-01"), (a.ItemId, a.Feature));                       // Feature-Kontext aufgelöst
        Assert.Equal("ARCH-1", Assert.Single(k.DesignOhneAdr).ItemId);
        Assert.Equal("ARCH-3", Assert.Single(k.Unklassifiziert).ItemId);
    }
}
