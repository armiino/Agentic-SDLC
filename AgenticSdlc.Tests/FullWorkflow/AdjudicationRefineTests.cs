using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// A10-Integration (09.09.): der geteilte Refine-Kern (CLI-Bahn ledger-adjudicate-refine UND
// Ein-Graph-Knoten PipelineAdjudicationRefine). Diese Tests frieren die vier Zusagen des
// Bauplans (todo-facet) ein: ① ohne pending-Claims KEIN Zuweisungs-Aufruf und unveränderter
// Bestand ② nur pending-Claims werden ersetzt, bestehende bleiben referenzidentisch
// ③ unvollständige Zuweisung bleibt SICHTBAR (stillPending) ④ Zuweisungs-Fehler schlagen durch.
public sealed class AdjudicationRefineTests
{
    private static SemanticLedgerEntry Claim(string id, string? facetStatus = null) =>
        new(Id: id, Proposition: $"prop-{id}", Kind: "requirement", Status: "open", Modality: "must",
            Scope: "test", TimeScope: null, Evidence: [new SemanticLedgerEvidence("transcript", $"quote-{id}")],
            Disposition: new Dictionary<string, ArtifactDisposition> { ["requirements"] = new("required", "requirement") },
            RiskLevel: "low", Notes: null, FacetStatus: facetStatus);

    // deferCount = ConsumableLedger.PendingCount-Semantik des ERZEUGERS (vertagte Positionen) —
    // bewusst NICHT aus facetStatus abgeleitet (Kollegen-Fund 09.09.: zwei getrennte pending-Begriffe).
    private static ConsumableLedger Ledger(int deferCount, params SemanticLedgerEntry[] claims) =>
        new(SourceAdjudicatedAt: "test", Claims: claims, PendingCount: deferCount, Note: "basis");

    private static ConsumableLedger Ledger(params SemanticLedgerEntry[] claims) => Ledger(0, claims);

    [Fact]
    public async Task OhnePending_keinZuweisungsAufruf_und_Bestand_unveraendert()
    {
        var ledger = Ledger(Claim("C-1"), Claim("C-2"));
        RefineAssign assign = (_, _, _) => throw new InvalidOperationException("darf nicht gerufen werden");

        var result = await AdjudicationRefine.RefineAsync(ledger, assign, transcript: null, CancellationToken.None);

        Assert.Same(ledger, result.Consumable); // unverändert durchgereicht, keine Kopie/Note-Änderung
        Assert.Equal(0, result.PendingBefore);
        Assert.Equal(0, result.StillPending);
        Assert.Equal(0, AdjudicationRefine.CountPending(result.Consumable));
    }

    [Fact]
    public async Task NurPendingClaims_werden_ersetzt_bestehende_bleiben_referenzidentisch()
    {
        var bestehend = Claim("C-1");
        var pending = Claim("ADJ-GAP-1", facetStatus: "pending");
        var ledger = Ledger(bestehend, pending, Claim("C-2"));

        IReadOnlyList<SemanticLedgerEntry>? gesehen = null;
        RefineAssign assign = (claims, transcript, _) =>
        {
            gesehen = claims;
            Assert.Equal("kontext", transcript);
            return Task.FromResult<IReadOnlyList<SemanticLedgerEntry>>(
                [Claim("ADJ-GAP-1") with { Kind = "requirement", Status = "decided" }]); // facetStatus=null = fertig
        };

        var result = await AdjudicationRefine.RefineAsync(ledger, assign, "kontext", CancellationToken.None);

        Assert.Equal(["ADJ-GAP-1"], gesehen!.Select(c => c.Id)); // NUR der pending-Claim ging zur Zuweisung
        Assert.Equal(1, result.PendingBefore);
        Assert.Equal(1, result.RefinedCount);
        Assert.Equal(0, result.StillPending);
        Assert.Same(bestehend, result.Consumable.Claims[0]); // bestehende Claims unangetastet
        Assert.Equal("decided", result.Consumable.Claims[1].Status); // ersetzt an gleicher Position
        Assert.Null(result.Consumable.Claims[1].FacetStatus);
        Assert.Contains("A10-refine", result.Consumable.Note);
    }

    [Fact]
    public async Task UnvollstaendigeZuweisung_bleibt_sichtbar_pending()
    {
        var ledger = Ledger(Claim("ADJ-GAP-1", "pending"), Claim("ADJ-GAP-2", "pending"));
        RefineAssign assign = (_, _, _) => Task.FromResult<IReadOnlyList<SemanticLedgerEntry>>(
            [Claim("ADJ-GAP-1")]); // GAP-2 fehlt in der Antwort

        var result = await AdjudicationRefine.RefineAsync(ledger, assign, null, CancellationToken.None);

        Assert.Equal(2, result.PendingBefore);
        Assert.Equal(1, result.RefinedCount);
        Assert.Equal(1, result.StillPending);           // LAUT sichtbar, nicht still verschluckt
        Assert.Equal("pending", result.Consumable.Claims[1].FacetStatus);
    }

    [Fact]
    public async Task Regression_DeferZaehler_bleibt_vom_Refine_unberuehrt()
    {
        // Kollegen-Fund 09.09.: PendingCount = VERTAGTE Positionen (defer), nicht Facetten-Stand.
        // Kombination „ein neuer Gap-Claim + eine vertagte Entscheidung": der Zähler muss 1 bleiben.
        var ledger = Ledger(deferCount: 1, Claim("C-1"), Claim("ADJ-GAP-1", "pending"));
        RefineAssign assign = (_, _, _) => Task.FromResult<IReadOnlyList<SemanticLedgerEntry>>([Claim("ADJ-GAP-1")]);

        var result = await AdjudicationRefine.RefineAsync(ledger, assign, null, CancellationToken.None);

        Assert.Equal(0, result.StillPending);            // Facetten vollständig
        Assert.Equal(1, result.Consumable.PendingCount); // Defer-Zähler UNVERÄNDERT
    }

    [Fact]
    public async Task ZuweisungsFehler_schlaegt_durch()
    {
        var ledger = Ledger(Claim("ADJ-GAP-1", "pending"));
        RefineAssign assign = (_, _, _) => throw new InvalidOperationException("modell kaputt");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => AdjudicationRefine.RefineAsync(ledger, assign, null, CancellationToken.None));
    }
}
