using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Ingestion;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1d (18.08., R-57 + N3-Fund): EINE Lese-Naht für Tor-1-Entscheide — Chat-Vertrag und UI-Vertrag sind an
// Rekorder UND Werkbank gleichberechtigt; jede begründete NICHT-Übernahme (UI „skip" wie Chat „reject")
// landet als ingest_rejection im Projekt-Gedächtnis. Vorher: Chat-Ablehnungen verschwanden still (R-57,
// Politur-Abnahme live belegt), die Werkbank wies Chat-Läufe ab (N3).
public sealed class IngestDecisionSeamTests
{
    private static string StageDir(string? chatJson = null, string? uiJson = null)
    {
        var dir = Directory.CreateTempSubdirectory("seam-").FullName;
        if (chatJson is not null) File.WriteAllText(Path.Combine(dir, IngestGateDecisions.FileName), chatJson);
        if (uiJson is not null) File.WriteAllText(Path.Combine(dir, "human-decisions.json"), uiJson);
        return dir;
    }

    private const string ChatReject = """
        { "runId":"r1", "reviewer":"author via steward-chat",
          "decisions":[ { "incomingItemId":"AF-1", "decision":"reject", "reason":"Status-Detail fehlt" } ] }
        """;
    private const string UiSkip = """
        { "runId":"r1", "reviewer":"human (review-ui)",
          "decisions":[ { "incomingItemId":"AF-1", "decision":"skip", "reason":"UI-Grund" } ] }
        """;

    [Fact]
    public void TryLoadAnyFile_liest_den_Chat_Vertrag_und_bevorzugt_ihn_vor_der_UI_Datei()
    {
        var beide = IngestGateDecisions.TryLoadAnyFile(StageDir(ChatReject, UiSkip))!;
        Assert.Equal("author via steward-chat", beide.Reviewer);           // Chat-Datei gewinnt (Pipeline-Vertrag zuerst)

        var nurChat = IngestGateDecisions.TryLoadAnyFile(StageDir(chatJson: ChatReject))!;
        Assert.Equal("reject", nurChat.Decisions.Single().Decision);
        Assert.Equal("Status-Detail fehlt", nurChat.Decisions.Single().Reason);

        var nurUi = IngestGateDecisions.TryLoadAnyFile(StageDir(uiJson: UiSkip))!;
        Assert.Equal("human (review-ui)", nurUi.Reviewer);                 // UI-Weg unverändert (Regressionsschutz)

        Assert.Null(IngestGateDecisions.TryLoadAnyFile(StageDir()));       // keine Datei = ehrlich null
    }

    [Fact]
    public void Chat_reject_landet_als_ingest_rejection_im_Projekt_Gedaechtnis()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
        var plan = new StateChangePlanDocument(1, "plan-t1", DateTime.UnixEpoch, "delta.json",
            [new StateChangeOperation("AF-1", "NEW_RELATED", "Besuchs-Bestätigung durch Pflegende", null, null, [], "test")]);
        var decisions = IngestGateDecisions.TryLoadAnyFile(StageDir(chatJson: ChatReject))!.Decisions;

        var (updated, recorded) = IngestionRejections.Record(core, plan, decisions, "plan.json");

        var rej = Assert.Single(IngestionRejections.Of(updated));          // R-57 tot: Chat-„reject" wird erinnert
        Assert.Equal(Assert.Single(recorded), rej.ProposalId);
        Assert.Equal("Status-Detail fehlt", rej.Metadata["reason"]);       // die P2a-Begründung wörtlich
        Assert.Equal("Besuchs-Bestätigung durch Pflegende", rej.Metadata["statement"]);
    }

    [Fact]
    public void UI_skip_wird_weiterhin_erinnert_und_apply_nie()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
        var plan = new StateChangePlanDocument(1, "plan-t2", DateTime.UnixEpoch, "delta.json",
            [new StateChangeOperation("AF-1", "NEW", "A", null, null, [], "t"),
             new StateChangeOperation("AF-2", "NEW", "B", null, null, [], "t")]);
        var decisions = new List<IngestionHumanDecision>
        { new("AF-1", "skip", "UI-Grund"), new("AF-2", "apply", null) };

        var (updated, recorded) = IngestionRejections.Record(core, plan, decisions, "plan.json");

        Assert.Single(recorded);                                           // nur die Nicht-Übernahme
        Assert.Equal("A", IngestionRejections.Of(updated).Single().Metadata["statement"]);
    }
}

// R-58 (18.08., Abnahme-3.0-Fund): Rejections-Suche = Stichwort-Recall (Token-ODER, Umlaut-Faltung,
// Bindestrich-Split, Ranking) — die Phrasen-Suche fand die Warnung nicht, obwohl sie existierte.
public sealed class RejectionSearchTests
{
    private static ProjectStateDocument CoreWithRejections()
    {
        var plan = new StateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "d",
            [new StateChangeOperation("AF-1", "NEW", "Angehörige sollen am Vortag eine Erinnerung an ihren bestätigten Besuch erhalten.", null, null, [], "t"),
             new StateChangeOperation("AF-2", "NEW", "Die Wochenübersicht bekommt einen Export.", null, null, [], "t")]);
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
        (core, _) = IngestionRejections.Record(core, plan,
            [new IngestionHumanDecision("AF-1", "reject", "Erinnerungen erst nach dem Pilotbetrieb."),
             new IngestionHumanDecision("AF-2", "skip", "Export später.")], "plan.json");
        return core;
    }

    [Fact]
    public void Stichwort_Suche_findet_die_Warnung_trotz_anderer_Wortform()
    {
        var core = CoreWithRejections();
        // exakt die Live-Anfrage des Stewards, die mit Phrasen-Match 0 Treffer hatte:
        var hits = IngestionRejections.Search(core, "Besuchs-Erinnerung Angehörige bestätigt");
        var top = Assert.Single(hits, h => h.Score >= 2);                   // erinnerung+angehoerige+bestaetigt treffen
        Assert.Contains("bestätigten Besuch", top.Proposal.Metadata["statement"]);
        Assert.Empty(IngestionRejections.Search(core, "Raketenantrieb"));   // Unpassendes bleibt still
    }

    [Fact]
    public void Ranking_und_Leer_Query_Verhalten()
    {
        var core = CoreWithRejections();
        var beste = IngestionRejections.Search(core, "Erinnerung Besuch bestätigt Vortag")[0];
        Assert.Contains("Erinnerung", beste.Proposal.Metadata["statement"]); // meiste Token-Treffer gewinnt
        Assert.Equal(2, IngestionRejections.Search(core, null).Count);       // ohne Query: alle (Tor-1-Vertrag)
    }
}
