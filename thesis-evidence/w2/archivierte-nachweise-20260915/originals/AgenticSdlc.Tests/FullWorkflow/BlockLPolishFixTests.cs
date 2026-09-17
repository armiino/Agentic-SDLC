using AgenticSdlc.Host.FullWorkflow.Adr;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-54 + R-56 (18.08., Block-L-Funde — E2E-RUNBOOK): die zwei Politur-Wächter.
public sealed class BlockLPolishFixTests
{
    // ---- R-54: die Adjudikations-UI-Anreicherung kennt jetzt BEIDE Layouts (Standalone + Ein-Graph-Kapsel).
    [Fact]
    public void R54_Standalone_Layout_bleibt_der_Standard()
    {
        var root = Directory.CreateTempSubdirectory("r54a-").FullName;
        var runDir = Path.Combine(root, "runs", "ledger", "L1");
        Directory.CreateDirectory(Path.Combine(runDir, "step-00-atomic-units"));          // Standalone-Merkmal
        var queueDir = Path.Combine(runDir, "step-03b-adjudicated");
        Directory.CreateDirectory(queueDir);
        var queue = Path.Combine(queueDir, "queue.json"); File.WriteAllText(queue, "{}");

        Assert.Equal(runDir, LedgerAdjudicateUiRunner.ResolveEnrichmentRunDir(queue, root));
    }

    [Fact]
    public void R54_Kapsel_Layout_folgt_dem_H1_Anker_zum_Sub_Run()
    {
        var root = Directory.CreateTempSubdirectory("r54b-").FullName;
        var ledgerStage = Path.Combine(root, "runs", "fullworkflow", "RID", "01-ledger");  // Ein-Graph: KEIN step-00 daneben
        Directory.CreateDirectory(ledgerStage);
        var queue = Path.Combine(ledgerStage, "queue.json"); File.WriteAllText(queue, "{}");
        File.WriteAllText(Path.Combine(ledgerStage, "ledger-run.json"), """{ "ledgerRunId": "LR1" }""");
        var subRun = Path.Combine(root, "runs", "ledger", "LR1");
        Directory.CreateDirectory(subRun);                                                // der Kapsel-Sub-Run existiert

        Assert.Equal(subRun, LedgerAdjudicateUiRunner.ResolveEnrichmentRunDir(queue, root));   // Anker gewinnt
    }

    [Fact]
    public void R54_kaputter_Anker_faellt_best_effort_auf_den_Standard_zurueck()
    {
        var root = Directory.CreateTempSubdirectory("r54c-").FullName;
        var ledgerStage = Path.Combine(root, "runs", "fullworkflow", "RID", "01-ledger");
        Directory.CreateDirectory(ledgerStage);
        var queue = Path.Combine(ledgerStage, "queue.json"); File.WriteAllText(queue, "{}");
        File.WriteAllText(Path.Combine(ledgerStage, "ledger-run.json"), "KEIN JSON");      // kaputt — darf nie werfen

        Assert.Equal(Path.Combine(root, "runs", "fullworkflow", "RID"),
            LedgerAdjudicateUiRunner.ResolveEnrichmentRunDir(queue, root));
    }

    // ---- R-56: die ADR-Vorschau verspricht KEINE konkrete Nummer mehr (Apply vergibt nach Freigabe-
    // Reihenfolge — eine Positions-Prognose log bei Teil-Freigabe zwingend: Block-L 0018 → Datei 0001).
    [Fact]
    public void R56_Vorschau_traegt_Platzhalter_und_nennt_die_echte_naechste_freie_Nummer()
    {
        Assert.Equal("ADR-XXXX", AdrFinalizeExecutor.PreviewAdrId);                        // keine Positions-Prognose

        var header = AdrFinalizeExecutor.PreviewHeader(2);
        Assert.Contains("ADR-0002", header);                                               // echte nächste freie (Core-Quelle)
        Assert.Contains("erst mit deiner Freigabe", header);

        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
        var rendered = AdrProjection.Render(
            new AdrDraft("ARCH-1", "Testtitel", "K", "E", "F"), AdrFinalizeExecutor.PreviewAdrId, AdrStatus.Accepted, core);
        Assert.StartsWith("# ADR-XXXX: Testtitel", rendered);                              // Titelzeile ohne Nummern-Versprechen
    }
}
