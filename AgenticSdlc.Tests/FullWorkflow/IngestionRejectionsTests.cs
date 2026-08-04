using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-35 Teil 2 (Wiedervorlage): menschliche Tor-1-Skips werden als ingest_rejection-Proposals in den Core gehoben
// (Projekt-Wissen, keine Wahrheits-Items), der Resolver kann sie via search_rejections abfragen, das Gate prüft
// den Hinweis fail-open, und das Review-UI warnt deterministisch („schon einmal abgelehnt") — über Resolver-Hinweis
// ODER wortgleichen IdentityKey-Match. KEIN Auto-Skip.
public sealed class IngestionRejectionsTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(IReadOnlyList<ProjectStateProposal>? proposals = null, params ProjectStateItem[] items)
        => new("p", 4, T, [], [.. items], [], [], proposals ?? []);

    private static StateChangeOperation Op(string incoming, string statement, string kind = StateChangeKind.New, string? relatedRejectionId = null)
        => new(incoming, kind, statement, null, null, ["claim-1"], "aus dem Meeting", relatedRejectionId);

    private static StateChangePlanDocument Plan(string planId, params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, planId, T, "delta.json", [.. ops]);

    private static IngestionHumanDecision Skip(string id, string? reason = "bewusst nicht gewollt")
        => new(id, "skip", reason);

    [Fact]
    public void Skips_landen_als_Proposal_mit_vollem_Wissen_Applies_nicht()
    {
        var plan = Plan("plan-1", Op("IN-1", "Die App soll eine Chat-Funktion bieten"), Op("IN-2", "Export als PDF"));
        var (core, recorded) = IngestionRejections.Record(Core(), plan,
            [Skip("IN-1", "Datenschutz-Bedenken, Team-Entscheid"), new("IN-2", "apply", null)], "runs/x/plan.json");

        var rej = Assert.Single(recorded);
        Assert.Equal("REJ-001", rej);
        var p = Assert.Single(core.Proposals);
        Assert.Equal(IngestionRejections.ProposalType, p.ProposalType);
        Assert.Equal("rejected", p.Status);
        Assert.Equal("plan-1", p.SourceRunId);
        Assert.Equal("runs/x/plan.json", p.PayloadPath);
        Assert.Equal("Die App soll eine Chat-Funktion bieten", p.Metadata["statement"]);
        Assert.Equal("Datenschutz-Bedenken, Team-Entscheid", p.Metadata["reason"]);
        Assert.Equal(IdentityKey.From("Die App soll eine Chat-Funktion bieten"), p.Metadata["identityKey"]);
        Assert.Equal("IN-1", p.Metadata["incomingItemId"]);
    }

    [Fact]
    public void Record_ist_idempotent_je_Plan_und_zaehlt_ueber_Plaene_weiter()
    {
        var plan1 = Plan("plan-1", Op("IN-1", "Chat-Funktion"));
        var (core, _) = IngestionRejections.Record(Core(), plan1, [Skip("IN-1")], "p1.json");

        var (again, recordedAgain) = IngestionRejections.Record(core, plan1, [Skip("IN-1")], "p1.json");
        Assert.Empty(recordedAgain);                       // zweiter Apply desselben Plans: kein Duplikat
        Assert.Single(again.Proposals);

        var plan2 = Plan("plan-2", Op("IN-9", "Gamification-Punkte"));
        var (after2, recorded2) = IngestionRejections.Record(again, plan2, [Skip("IN-9")], "p2.json");
        Assert.Equal("REJ-002", Assert.Single(recorded2)); // Nummerierung laeuft ueber Plaene weiter
        Assert.Equal(2, after2.Proposals.Count);
    }

    [Fact]
    public void Gate_warnt_fail_open_bei_unbekanntem_Rejection_Verweis()
    {
        var delta = Core(null, Req("IN-1", "Chat"));
        var core = Core();
        var report = IngestionGate.Check(delta, core, Plan("plan-1", Op("IN-1", "Chat", relatedRejectionId: "REJ-999")));

        Assert.True(report.Pass);   // Hinweis blockt nie
        Assert.Contains(report.Warnings, w => w.Code == "UNKNOWN_REJECTION_REF");
    }

    [Fact]
    public void Gate_ist_still_bei_gueltigem_Rejection_Verweis()
    {
        var (coreWithRej, _) = IngestionRejections.Record(Core(), Plan("plan-0", Op("IN-0", "Chat-Funktion")), [Skip("IN-0")], "p0.json");
        var delta = Core(null, Req("IN-1", "Chat"));
        var report = IngestionGate.Check(delta, coreWithRej, Plan("plan-1", Op("IN-1", "Chat", relatedRejectionId: "REJ-001")));

        Assert.DoesNotContain(report.Warnings, w => w.Code == "UNKNOWN_REJECTION_REF");
    }

    [Fact]
    public void Review_warnt_ueber_Resolver_Hinweis_mit_Datum_und_Grund()
    {
        var (core, _) = IngestionRejections.Record(Core(), Plan("plan-0", Op("IN-0", "Die App soll eine Chat-Funktion bieten")),
            [Skip("IN-0", "Datenschutz-Bedenken")], "p0.json");
        var delta = Core(null, Req("IN-1", "Nutzer wollen Direktnachrichten"));
        var plan = Plan("plan-1", Op("IN-1", "Nutzer wollen Direktnachrichten", relatedRejectionId: "REJ-001"));

        var session = IngestionReviewAdapter.BuildSession("r1", plan, delta, core);
        var note = Assert.Single(session.Items[0].Notes, n => n.Label == "Schon einmal abgelehnt");
        Assert.Contains("REJ-001", note.Text);
        Assert.Contains("Datenschutz-Bedenken", note.Text);
        Assert.Contains("kein automatisches Überspringen", note.Text);
    }

    [Fact]
    public void Review_warnt_deterministisch_bei_wortgleicher_Wiederholung_ohne_Hinweis()
    {
        // Der Agent hat KEINEN Hinweis gesetzt — der IdentityKey-Exakt-Match fängt es trotzdem (LLM-frei).
        var (core, _) = IngestionRejections.Record(Core(), Plan("plan-0", Op("IN-0", "Die App soll eine Chat-Funktion bieten")),
            [Skip("IN-0", "Datenschutz")], "p0.json");
        var delta = Core(null, Req("IN-1", "Chat"));
        var plan = Plan("plan-1", Op("IN-1", "Die App soll eine Chat-Funktion bieten"));

        var session = IngestionReviewAdapter.BuildSession("r1", plan, delta, core);
        var note = Assert.Single(session.Items[0].Notes, n => n.Label == "Schon einmal abgelehnt");
        Assert.Contains("wortgleicher Vorschlag", note.Text);
    }

    [Fact]
    public void Review_ohne_Treffer_keine_Note()
    {
        var (core, _) = IngestionRejections.Record(Core(), Plan("plan-0", Op("IN-0", "Chat-Funktion")), [Skip("IN-0")], "p0.json");
        var delta = Core(null, Req("IN-1", "Export"));
        var session = IngestionReviewAdapter.BuildSession("r1", Plan("plan-1", Op("IN-1", "Export als PDF")), delta, core);

        Assert.DoesNotContain(session.Items[0].Notes, n => n.Label == "Schon einmal abgelehnt");
    }

    [Fact]
    public void SearchRejections_Tool_leer_alle_mit_Query_gefiltert()
    {
        var (core, _) = IngestionRejections.Record(Core(),
            Plan("plan-0", Op("IN-0", "Chat-Funktion fuer Nutzer"), Op("IN-1", "Gamification-Punkte")),
            [Skip("IN-0", "Datenschutz"), Skip("IN-1", "kein Mehrwert")], "p0.json");

        var run = new RunContext(RunId.New(), "test-rejections-tool");
        run.EnsureFolders();
        var tools = new IngestionTools(Core(), core, new ShowAllRequirementRetriever(), run);

        var all = tools.Build();   // Tool-Registrierung kompiliert/bindet
        Assert.Contains(all, t => t is Microsoft.Extensions.AI.AIFunction f && f.Name == "search_rejections");

        // Verhalten direkt ueber die interne Methode (InternalsVisibleTo): leer = alle, Query filtert.
        var everything = tools.GetType().GetMethod("SearchRejections",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var resAll = (string)everything.Invoke(tools, [null, 30])!;
        Assert.Contains("REJ-001", resAll);
        Assert.Contains("REJ-002", resAll);
        var resChat = (string)everything.Invoke(tools, ["Chat", 30])!;
        Assert.Contains("REJ-001", resChat);
        Assert.DoesNotContain("REJ-002", resChat);
    }
}
