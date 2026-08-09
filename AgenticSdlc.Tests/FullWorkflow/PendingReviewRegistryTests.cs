using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C4d (09.08., c4-plan §10/§11 + K12) — die Pending-Registry: Register/ListOpen/Close, ÜBERHOLT beim
// Lesen berechnet, und der K12-GRENZ-TEST: der Registrar schreibt NUR proposals, nie Items/Relationen.
public sealed class PendingReviewRegistryTests
{
    private static ProjectStateItem Pbi(string id, string status, int version = 1) =>
        new ProjectStateItem(id, "pbi", $"{id} Text", "test", null, version, "r", null, null, null, null, [], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Core(params ProjectStateItem[] items) =>
        new("p", 4, DateTime.UnixEpoch, [], items, [], [], []);

    private static JsonElement Payload() => JsonSerializer.SerializeToElement(new { plan = "x" });

    [Fact]
    public void Register_ListOpen_Close_Lebenszyklus_und_K12_Grenze()
    {
        var core = Core(Pbi("PBI-1", "needs_clarify"));
        var (reg, id) = PendingReviewRegistry.Register(core, "clarify-sweep", "pbi-update-review --pending PEND-r1",
            Payload(), ["PBI-1"], "r1");

        Assert.Equal("PEND-r1", id);
        Assert.Same(core.Items, reg.Items);                                   // K12-GRENZE: Items unberührt
        Assert.Same(core.Relations, reg.Relations);                           // K12-GRENZE: Relationen unberührt
        Assert.True(CoreKangal.Check(reg).Pass);                              // Wachhund bleibt grün

        var open = Assert.Single(PendingReviewRegistry.ListOpen(reg));
        Assert.False(open.Ueberholt);
        Assert.Equal("clarify-sweep", open.Bahn);

        var closed = PendingReviewRegistry.Close(reg, id, "applied");
        Assert.Empty(PendingReviewRegistry.ListOpen(closed));
        Assert.Empty(closed.Proposals);                                       // Anti-Zumüll: ENTFERNT, kein Stub
                                                                              // (Beleg: Run-applied + History + Snapshots)
    }

    [Fact]
    public void Ueberholt_wird_beim_Lesen_berechnet_nie_gespeichert()
    {
        var core = Core(Pbi("PBI-1", "needs_clarify", version: 3));
        var (reg, _) = PendingReviewRegistry.Register(core, "clarify-sweep", "cmd", Payload(), ["PBI-1"], "r1");

        // Anderweitig geklärt (Blocker weg) ⇒ ÜBERHOLT mit Grund.
        var geklaert = reg with { Items = [Pbi("PBI-1", "active", version: 4)] };
        var e1 = Assert.Single(PendingReviewRegistry.ListOpen(geklaert));
        Assert.True(e1.Ueberholt);
        Assert.Contains("nicht mehr needs_clarify", e1.UeberholtGrund);

        // Nur Version geändert (Meeting-Op, weiter needs_clarify) ⇒ ebenfalls ÜBERHOLT (alter Stand!).
        var geaendert = reg with { Items = [Pbi("PBI-1", "needs_clarify", version: 4)] };
        Assert.Contains("zwischenzeitlich geändert", Assert.Single(PendingReviewRegistry.ListOpen(geaendert)).UeberholtGrund);

        // Registry-Eintrag selbst blieb unverändert (nichts gespeichert): frischer Core ⇒ wieder frisch.
        Assert.False(Assert.Single(PendingReviewRegistry.ListOpen(reg)).Ueberholt);
    }
}
