using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1e' Schritt 3: der zentrale Gate-Responder (R-25) ist die EINE Steuerstelle — Governance ist Architektur.
// Diese Tests frieren die Policy-Auflösung ein; besonders die Replay-Fallback-Regel (unmatcht ⇒ reject).
public sealed class GateResponderTests
{
    private static readonly IReadOnlyList<GateItem> ThreeItems =
    [
        new GateItem("A", NeedsHuman: true),
        new GateItem("B"),
        new GateItem("C", NeedsHuman: true)
    ];

    [Fact]
    public void Interactive_pausiert_und_entscheidet_nichts()
    {
        var r = GateResponder.Resolve(GatePolicy.Interactive, ThreeItems);

        Assert.Equal(GateOutcome.Pause, r.Outcome);
        Assert.Empty(r.AcceptedItemIds);
        Assert.Empty(r.RejectedItemIds);
    }

    [Fact]
    public void AcceptAll_akzeptiert_alle_Items()
    {
        var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.AcceptAll), ThreeItems);

        Assert.Equal(GateOutcome.Resolved, r.Outcome);
        Assert.Equal(new[] { "A", "B", "C" }, r.AcceptedItemIds);
        Assert.Empty(r.RejectedItemIds);
        Assert.Empty(r.UnmatchedItemIds);
    }

    [Fact]
    public void Replay_matcht_per_ItemId_accept_und_reject()
    {
        var decisions = new Dictionary<string, bool> { ["A"] = true, ["B"] = false, ["C"] = true };
        var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.Replay), ThreeItems, decisions);

        Assert.Equal(GateOutcome.Resolved, r.Outcome);
        Assert.Equal(new[] { "A", "C" }, r.AcceptedItemIds);
        Assert.Equal(new[] { "B" }, r.RejectedItemIds);
        Assert.Empty(r.UnmatchedItemIds);
    }

    [Fact]
    public void Replay_unmatchtes_Item_wird_rejected_und_dokumentiert()
    {
        // Nur A hat einen gespeicherten Entscheid; B und C fehlen -> Fallback reject, in Unmatched gelistet.
        var decisions = new Dictionary<string, bool> { ["A"] = true };
        var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.Replay), ThreeItems, decisions);

        Assert.Equal(GateOutcome.Resolved, r.Outcome);
        Assert.Equal(new[] { "A" }, r.AcceptedItemIds);
        Assert.Equal(new[] { "B", "C" }, r.RejectedItemIds);   // Governance-sicher: kein stiller Durchrutscher
        Assert.Equal(new[] { "B", "C" }, r.UnmatchedItemIds);  // dokumentiert fürs Log
    }

    [Fact]
    public void Replay_ohne_Entscheide_rejectet_alles()
    {
        var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.Replay), ThreeItems, replayDecisions: null);

        Assert.Equal(GateOutcome.Resolved, r.Outcome);
        Assert.Empty(r.AcceptedItemIds);
        Assert.Equal(3, r.RejectedItemIds.Count);
        Assert.Equal(3, r.UnmatchedItemIds.Count);
    }

    [Fact]
    public void Leere_Item_Liste_ist_unkritisch()
    {
        var r = GateResponder.Resolve(new GatePolicy(GatePolicyKind.Replay), [], new Dictionary<string, bool>());
        Assert.Equal(GateOutcome.Resolved, r.Outcome);
        Assert.Empty(r.AcceptedItemIds);
        Assert.Empty(r.RejectedItemIds);
    }

    [Fact]
    public void Replay_Pfad_aus_der_Policy_bleibt_erhalten()
    {
        // Der Responder nutzt den Pfad nicht selbst (Aufrufer lädt), aber die Policy trägt ihn — Kontrakt-Check.
        var policy = GatePolicy.Parse("replay:decisions/e2e.json");
        Assert.Equal(GatePolicyKind.Replay, policy.Kind);
        Assert.Equal("decisions/e2e.json", policy.ReplayPath);
    }
}
