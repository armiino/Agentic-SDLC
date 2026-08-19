using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Steward;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C5a (09.08., c5-plan §3/§4) — Gespräch als Gate-Kanal, LLM-frei am ECHTEN Kreis: treue Vorlage aus der
// Adapter-Naht (Autor-Antwort-Note!), P2a im Tool erzwungen, Submit schreibt den IDENTISCHEN Datei-Vertrag
// (reviewer=author via steward-chat) und kettet den echten Apply → Blocker fällt, Registry schließt.
public sealed class StewardGateToolsTests
{
    private static string RepoWithPending(out string proposalId)
    {
        var repo = Directory.CreateTempSubdirectory("c5a-").FullName;
        Directory.CreateDirectory(Path.Combine(repo, "state", "core"));
        var pbi = (new ProjectStateItem("PBI-1", "pbi", "PBI-1 Text", "re-clarify", null, 1, "r", null, null, null, null,
                [], [], new Dictionary<string, string>()) with
            { Pbi = new PbiPayload("Ziel", "Alter Titel", ["AK alt"], [], [], null, "active", null, null) })
            .WithStatus(CoreStatus.From("needs_clarify"));
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [pbi], [], [], []);

        var plan = new PbiStateChangePlanDocument(1, "sweep-t", DateTime.UnixEpoch, "run-t",
            [new PbiStateChangeOperation(PbiUpdateKind.MarkChanged, "", "PBI-1", null, null, null,
                "Klärungs-Sweep: author via steward-chat", AuthorAnswerRef: "chat:t#1", AuthorAnswerText: "Nur Admins sehen das Protokoll.")],
            [new PbiAlignment("PBI-1", "Neuer Titel", "Als Admin sehe ich das Protokoll.", ["Nur Admin-Rolle hat Zugriff"], "aus Antwort", ["chat:t#1"])]);
        (core, proposalId) = PendingReviewRegistry.Register(core, "clarify-sweep", "cmd",
            JsonSerializer.SerializeToElement(plan, AgenticSdlc.Host.FullWorkflow.JsonFiles.Json), ["PBI-1"], "run-t");
        File.WriteAllText(Path.Combine(repo, "state", "core", "project-state.json"),
            JsonSerializer.Serialize(core, AgenticSdlc.Host.FullWorkflow.JsonFiles.Json));
        return repo;
    }

    private static async Task<JsonElement> InvokeAsync(StewardGateTools tools, string name, IDictionary<string, object?> args)
    {
        var fn = tools.Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args));
        return JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public async Task Treue_Vorlage_traegt_die_Autor_Antwort_Note_und_die_Angleichung()
    {
        var repo = RepoWithPending(out var id);
        var v = await InvokeAsync(new StewardGateTools(repo), "get_pending_review", new Dictionary<string, object?> { ["proposalId"] = id });

        var notes = v.GetProperty("items")[0].GetProperty("notes").EnumerateArray().ToList();
        Assert.Contains(notes, n => n.GetProperty("label").GetString()!.Contains("Autor-Antwort via steward-chat"));
        Assert.Equal("Neuer Titel", v.GetProperty("alignments")[0].GetProperty("proposedTitle").GetString());
        Assert.Equal(4, tools_count(tools: new StewardGateTools(repo)));      // alle Submits ApprovalRequired (C5a+C5b+Schritt2)
        static int tools_count(StewardGateTools tools) => tools.Build().OfType<ApprovalRequiredAIFunction>().Count();
    }

    [Fact]
    public async Task Submit_erzwingt_P2a_und_vollzieht_dann_den_ECHTEN_Kreis()
    {
        var repo = RepoWithPending(out var id);
        var tools = new StewardGateTools(repo);

        var p2a = await InvokeAsync(tools, "submit_gate_decisions", new Dictionary<string, object?>
        { ["proposalId"] = id, ["decisions"] = new[] { new PbiUpdateDecision("op-0", "skip", null) } });
        Assert.Equal("DECISIONS_INVALID", p2a.GetProperty("error").GetString());
        Assert.Contains("P2a", p2a.GetProperty("details")[0].GetString());

        var unvollstaendig = await InvokeAsync(tools, "submit_gate_decisions", new Dictionary<string, object?>
        { ["proposalId"] = id, ["decisions"] = Array.Empty<PbiUpdateDecision>() });
        Assert.Contains("KEINE Entscheidung", unvollstaendig.GetProperty("details")[0].GetString());   // Sammel-Akt = vollständig

        var ok = await InvokeAsync(tools, "submit_gate_decisions", new Dictionary<string, object?>
        {
            ["proposalId"] = id,
            ["decisions"] = new[] { new PbiUpdateDecision("op-0", "apply", null) },
            ["alignmentDecisions"] = new[] { new PbiAlignmentDecision("PBI-1", "accept", null, null, null, null) },
        });
        Assert.True(ok.GetProperty("applied").GetBoolean());

        var core = JsonSerializer.Deserialize<ProjectStateDocument>(
            File.ReadAllText(Path.Combine(repo, "state", "core", "project-state.json")), AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        var pbi = core.Items.Single(i => i.ItemId == "PBI-1");
        Assert.Equal(Blocker.None, pbi.ReadStatus().Blocker);                 // Blocker fiel durch den ECHTEN Apply
        Assert.Equal("Neuer Titel", pbi.Pbi!.Title);
        Assert.Empty(PendingReviewRegistry.ListOpen(core));                   // Registry geschlossen (Anti-Zumüll)

        var planDir = ok.GetProperty("planDir").GetString()!;
        var file = JsonSerializer.Deserialize<PbiUpdateDecisionsFile>(
            File.ReadAllText(Path.Combine(Path.IsPathRooted(planDir) ? planDir : Path.Combine(repo, planDir), "human-decisions.json")),
            AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", file.Reviewer);               // EIN Datei-Vertrag, anderer Kanal
    }
}

// C5-Schritt2 — ingest- und decision-gate im Chat: Vorlage aus den persistierten Artefakten, Submits mit
// P2a/Vollständigkeit, Dateien = exakt die Verträge des resume-Responders (ingest = der NEUE R-44-Vertrag).
public sealed class StewardChatGateStep2Tests
{
    private static string Repo(string gate, string stageDir, string artifactName, string artifactJson, out string runId)
    {
        var repo = Directory.CreateTempSubdirectory("c5s2-").FullName;
        runId = "20260809_000001_test";
        var dir = Path.Combine(repo, "runs", "fullworkflow", runId);
        Directory.CreateDirectory(Path.Combine(dir, "checkpoints"));
        File.WriteAllText(Path.Combine(dir, "checkpoints", "pointer.json"),
            $"{{\"runId\":\"{runId}\",\"sessionId\":\"s\",\"checkpointId\":\"cp-1\",\"mode\":\"{gate}\",\"savedUtc\":\"2026-08-09T09:00:00Z\"}}");
        Directory.CreateDirectory(Path.Combine(dir, stageDir));
        File.WriteAllText(Path.Combine(dir, stageDir, artifactName), artifactJson);
        return repo;
    }

    private static async Task<System.Text.Json.JsonElement> InvokeAsync(StewardGateTools tools, string name, IDictionary<string, object?> args)
    {
        var fn = tools.Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args));
        return System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Deserialize<string>(System.Text.Json.JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public async Task Ingest_Gate_Vorlage_P2a_und_R44_Datei_Vertrag()
    {
        var repo = Repo("ingest-gate", "07-ingest", "plan.json",
            """{"operations":[{"incomingItemId":"GH-900","kind":"NEW_RELATED","statement":"PDF-Export","featureKey":"med","rationale":"neu"},{"incomingItemId":"REQ-N2","kind":"UPDATE","statement":"X","rationale":"y"}]}""", out var runId);
        var tools = new StewardGateTools(repo);

        var v = await InvokeAsync(tools, "get_paused_gate", new Dictionary<string, object?> { ["runId"] = runId });
        Assert.Equal(2, v.GetProperty("items").GetArrayLength());
        Assert.Equal("GH-900", v.GetProperty("items")[0].GetProperty("incomingItemId").GetString());

        var p2a = await InvokeAsync(tools, "submit_ingest_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecision("GH-900", "apply"),
              new AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecision("REQ-N2", "reject") } });
        Assert.Contains("P2a", p2a.GetProperty("details")[0].GetString());

        var ok = await InvokeAsync(tools, "submit_ingest_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecision("GH-900", "apply"),
              new AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecision("REQ-N2", "reject", "Duplikat von REQ-05") } });
        Assert.True(ok.GetProperty("saved").GetBoolean());

        // 3b-2-Brücke: TryLoadAny akzeptiert AUCH die human-decisions.json der bestehenden ingest-UI.
        var stage = Path.Combine(repo, "runs", "fullworkflow", runId, "07-ingest");
        File.Move(Path.Combine(stage, "ingest-gate-decisions.json"), Path.Combine(stage, "human-decisions.json"));
        var bridged = AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecisions.TryLoadAny(stage)!.Value;
        Assert.Equal(["GH-900"], bridged.Accepted);
        File.Move(Path.Combine(stage, "human-decisions.json"), Path.Combine(stage, "ingest-gate-decisions.json"));

        // R-44: der Responder-Loader liest EXAKT diese Datei — apply-Set + Chat-Reviewer.
        var loaded = AgenticSdlc.Host.FullWorkflow.Ingestion.IngestGateDecisions.TryLoad(
            Path.Combine(repo, "runs", "fullworkflow", runId, "07-ingest", "ingest-gate-decisions.json"))!.Value;
        Assert.Equal(["GH-900"], loaded.Accepted);
        Assert.Equal("author via steward-chat", loaded.Reviewer);
    }

    // 1c-③ (18.08., „nie gesichtet"-Fund): die Wiedervorlage-Warnungen der UI-Karten („Schon einmal
    // abgelehnt/geklärt") erreichen jetzt auch die CHAT-Vorlage — dieselbe Adapter-Quelle
    // (WiedervorlageNotes), fail-open ohne Core (voriger Test läuft core-los und bleibt grün).
    [Fact]
    public async Task Ingest_Chat_Vorlage_traegt_die_Wiedervorlage_Warnungen_der_UI()
    {
        var frage = "Dürfen Angehörige Besuche eintragen?";
        var abgelehnt = "Angehörige erhalten eine Push-Erinnerung am Vortag.";
        var repo = Repo("ingest-gate", "07-ingest", "plan.json", JsonSerializer.Serialize(new StateChangePlanDocument(
            StateChangePlanDocument.CurrentSchemaVersion, "plan-w", DateTime.UnixEpoch, "delta",
            [new StateChangeOperation("AF-1", StateChangeKind.OpenQuestion, frage, null, null, [], "aus dem Meeting"),
             new StateChangeOperation("AF-2", StateChangeKind.New, abgelehnt, null, null, [], "neu"),
             new StateChangeOperation("AF-3", StateChangeKind.New, "Unbelastetes Neues.", null, null, [], "neu"),
             new StateChangeOperation("AF-4", StateChangeKind.Refine, "Bemerkungen werden versioniert.", "REQ-42", null, [], "verfeinert")]),
            AgenticSdlc.Host.FullWorkflow.JsonFiles.Json), out var runId);

        // Core mit (a) bereits GEKLÄRTER wortgleicher Frage und (b) frueherer Ablehnung des Push-Vorschlags:
        var geklaert = new ProjectStateItem("DEC-001", "decision", frage, "MEETING", null, 1, "run-alt",
            null, null, null, null, [], [], new Dictionary<string, string>
            { ["resolutionOutcome"] = "KEEP_ORIGINAL", ["resolvedUtc"] = "2026-08-01T10:00:00Z" })
            .WithStatus(CoreStatus.From("resolved")) with { IdentityKey = IdentityKey.From(frage) };
        var bestand = new ProjectStateItem("REQ-42", "requirement", "Bemerkung optional, max. 200 Zeichen.",
            "MEETING", null, 1, "run-alt", null, null, null, null, [], [], new Dictionary<string, string>())
            .WithStatus(CoreStatus.From("accepted"));
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [geklaert, bestand], [], [], []);
        (core, _) = IngestionRejections.Record(core,
            new StateChangePlanDocument(1, "plan-alt", DateTime.UnixEpoch, "delta",
                [new StateChangeOperation("ALT-1", StateChangeKind.New, abgelehnt, null, null, [], "alt")]),
            [new IngestionHumanDecision("ALT-1", "reject", "Erinnerungen erst nach dem Pilotbetrieb.")], "plan.json");
        Directory.CreateDirectory(Path.Combine(repo, "state", "core"));
        File.WriteAllText(Path.Combine(repo, "state", "core", "project-state.json"),
            JsonSerializer.Serialize(core, AgenticSdlc.Host.FullWorkflow.JsonFiles.Json));

        var v = await InvokeAsync(new StewardGateTools(repo), "get_paused_gate", new Dictionary<string, object?> { ["runId"] = runId });
        var items = v.GetProperty("items");

        var w1 = Assert.Single(items[0].GetProperty("warnungen").EnumerateArray().ToList());
        Assert.Equal("Schon einmal geklärt", w1.GetProperty("label").GetString());
        Assert.Contains("KEEP_ORIGINAL", w1.GetProperty("text").GetString());     // damaliges Ergebnis reist mit

        var w2 = Assert.Single(items[1].GetProperty("warnungen").EnumerateArray().ToList());
        Assert.Equal("Schon einmal abgelehnt", w2.GetProperty("label").GetString());
        Assert.Contains("Pilotbetrieb", w2.GetProperty("text").GetString());      // wörtliche P2a-Begründung

        Assert.Empty(items[2].GetProperty("warnungen").EnumerateArray());         // Unbelastetes bleibt still

        // 1c-①: der Ziel-Diff reist auch im Chat — beide vollen Texte aus derselben Adapter-Quelle.
        var diff = items[3].GetProperty("zielDiff");
        Assert.Equal("Bemerkung optional, max. 200 Zeichen.", diff.GetProperty("giltHeute").GetString());
        Assert.Equal("Bemerkungen werden versioniert.", diff.GetProperty("stuendeDanach").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, items[2].GetProperty("zielDiff").ValueKind);
    }

    [Fact]
    public async Task Decision_Gate_Vorlage_Vokabular_und_Responder_kompatible_Datei()
    {
        var repo = Repo("decision-gate", "07-decision", "decision-gate-request.json",
            """{"runId":"r","decisions":[{"decisionId":"DEC-7","decisionText":"Widerspruch X?","targetRequirementText":"Alt","proposedStatement":"Neu","blockedPbis":["PBI-1"],"origin":"Meeting-Widerspruch — am Ingest-Gate bestätigt"},{"decisionId":"DEC-8","decisionText":"Offene Frage Y?","targetRequirementId":null,"targetRequirementText":"","proposedStatement":"Offene Frage Y?","blockedPbis":[],"origin":"Offene Frage — von dir diktiert (Autor-Front)"}]}""", out var runId);
        var tools = new StewardGateTools(repo);

        var v = await InvokeAsync(tools, "get_paused_gate", new Dictionary<string, object?> { ["runId"] = runId });
        Assert.Equal("DEC-7", v.GetProperty("decisions")[0].GetProperty("decisionId").GetString());
        Assert.Equal("Alt", v.GetProperty("decisions")[0].GetProperty("bestehendeWahrheit").GetString());

        // Abnahme-4.0-Feil ①: die HERKUNFT erreicht die Chat-Bahn (der DEC-Origin-Fix war vorher UI-only) —
        // und ziellose Frage-DECs tragen KEINE Null-Felder (der Agent las leere Anführungszeichen vor).
        Assert.Contains("Autor-Front", v.GetProperty("decisions")[1].GetProperty("herkunft").GetString());
        Assert.False(v.GetProperty("decisions")[1].TryGetProperty("bestehendeWahrheit", out _));
        Assert.False(v.GetProperty("decisions")[1].TryGetProperty("meetingVorschlag", out _));
        Assert.Contains("herkunft", v.GetProperty("hinweis").GetString());

        var bad = await InvokeAsync(tools, "submit_decision_gate_resolutions", new Dictionary<string, object?>
        { ["runId"] = runId, ["resolutions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineDecisionResolution("DEC-7", "resolve", "REFINE", null, null) } });
        Assert.Contains("REFINE OHNE newStatement", bad.GetProperty("details")[0].GetString());

        var adopt = await InvokeAsync(tools, "submit_decision_gate_resolutions", new Dictionary<string, object?>
        { ["runId"] = runId, ["resolutions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineDecisionResolution("DEC-7", "resolve", "ADOPT_NEW", null, null) } });
        Assert.Contains("ADOPT_NEW OHNE newStatement", adopt.GetProperty("details")[0].GetString());

        var ok = await InvokeAsync(tools, "submit_decision_gate_resolutions", new Dictionary<string, object?>
        { ["runId"] = runId, ["resolutions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineDecisionResolution("DEC-7", "defer", null, null, "erst Team fragen"),
              new AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineDecisionResolution("DEC-8", "defer", null, null, null) } });
        Assert.Equal(2, ok.GetProperty("deferred").GetInt32());

        // Responder-Kompatibilität: Datei deserialisiert als PipelineDecisionDecisionsFile (der resume-Vertrag).
        var file = System.Text.Json.JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Decision.PipelineDecisionDecisionsFile>(
            File.ReadAllText(Path.Combine(repo, "runs", "fullworkflow", runId, "07-decision", "decision-gate-decisions.json")),
            AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", file.Reviewer);
        Assert.All(file.Resolutions, r => Assert.Equal("defer", r.Action));
    }
}

// C5b-1 — das pausierte forward-Gate im Chat: treue Op-Vorlage aus dem persistierten Plan, Submit mit
// Vollständigkeits-/Vokabular-Zwang, Datei = exakt der Vertrag, den der resume-Responder liest.
public sealed class StewardGraphGateTests
{
    private static string RepoWithPausedForward(out string runId)
    {
        var repo = Directory.CreateTempSubdirectory("c5b-").FullName;
        runId = "20260809_000000_test";
        var dir = Path.Combine(repo, "runs", "fullworkflow", runId);
        Directory.CreateDirectory(Path.Combine(dir, "checkpoints"));
        File.WriteAllText(Path.Combine(dir, "checkpoints", "pointer.json"),
            $"{{\"runId\":\"{runId}\",\"sessionId\":\"s\",\"checkpointId\":\"cp-1\",\"mode\":\"github-forward-gate\",\"savedUtc\":\"2026-08-09T09:00:00Z\"}}");
        Directory.CreateDirectory(Path.Combine(dir, "07-github"));
        File.WriteAllText(Path.Combine(dir, "07-github", "github-forward-plan.json"), """
            {"schemaVersion":1,"planId":"p","createdUtc":"2026-08-09T09:00:00Z","sourcePbiUpdateRun":"r","repository":"o/r",
             "operations":[{"kind":"UPDATE_ISSUE","pbiId":"PBI-1","targetIssueNumber":12,"title":"T","rationale":"geaendert"},
                           {"kind":"FLAG_DRIFT","pbiId":"PBI-2","targetIssueNumber":13,"rationale":"DRIFT-SPERRE: manuell editiert"}]}
            """);
        return repo;
    }

    private static async Task<System.Text.Json.JsonElement> InvokeAsync(StewardGateTools tools, string name, IDictionary<string, object?> args)
    {
        var fn = tools.Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args));
        return System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Deserialize<string>(System.Text.Json.JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public void Pipeline_Stufen_Erkennung_verhindert_Doppel_Apply()
    {
        Assert.Equal("20260809_1_x", AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.TryGetPipelineRunId(
            Path.Combine("runs", "fullworkflow", "20260809_1_x", "07-pbi-update")));
        Assert.Null(AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.TryGetPipelineRunId(
            Path.Combine("runs", "pbi-update", "20260809_1_x", "plan")));       // Standalone bleibt Standalone-Apply
    }

    [Fact]
    public async Task Forward_Gate_zeigt_den_VOLLEN_Inhalt_aus_dem_sync_delta()
    {
        // Steward-UX-Fund 11.08. („warum sehe ich die aenderung nicht?"): das forward-Gate zeigt jetzt je Op den
        // vollen Inhalt (Statement/AK/Rahmen), der ans Issue geschrieben wuerde — aus dem sync-delta der pbi-update-Stufe.
        var repo = RepoWithPausedForward(out var runId);
        var syncDir = Path.Combine(repo, "runs", "fullworkflow", runId, "07-pbi-update", "applied");
        Directory.CreateDirectory(syncDir);
        File.WriteAllText(Path.Combine(syncDir, "github-sync-delta.json"), """
            {"newPbis":[],"updatedPbis":["PBI-1"],"entries":[
              {"pbiId":"PBI-1","title":"T","statement":"Als Admin sehe ich das Protokoll.","acceptanceCriteria":["Nur Admin-Rolle hat Zugriff","Protokoll ist read-only"],"constraints":["ARCH-1 — DSGVO"]}]}
            """);

        var v = await InvokeAsync(new StewardGateTools(repo), "get_paused_gate", new Dictionary<string, object?> { ["runId"] = runId });
        var inhalt = v.GetProperty("ops")[0].GetProperty("inhalt");
        Assert.Equal("Als Admin sehe ich das Protokoll.", inhalt.GetProperty("statement").GetString());
        Assert.Equal(2, inhalt.GetProperty("akzeptanzkriterien").GetArrayLength());   // voller Inhalt sichtbar
        Assert.Equal("ARCH-1 — DSGVO", inhalt.GetProperty("rahmen")[0].GetString());
    }

    [Fact]
    public async Task Vorlage_Submit_und_Datei_Vertrag_fuer_den_resume_Responder()
    {
        var repo = RepoWithPausedForward(out var runId);
        var tools = new StewardGateTools(repo);

        var v = await InvokeAsync(tools, "get_paused_gate", new Dictionary<string, object?> { ["runId"] = runId });
        Assert.Equal(2, v.GetProperty("ops").GetArrayLength());
        Assert.Contains("DRIFT-SPERRE", v.GetProperty("ops")[1].GetProperty("rationale").GetString());

        var fehlt = await InvokeAsync(tools, "submit_paused_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[] { new AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecision("op-0", "apply", null) } });
        Assert.Contains("KEINE Entscheidung", fehlt.GetProperty("details")[0].GetString());   // Sammel-Akt

        var p2a = await InvokeAsync(tools, "submit_paused_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecision("op-0", "apply", null),
              new AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecision("op-1", "skip", null) } });
        Assert.Contains("P2a", p2a.GetProperty("details")[0].GetString());                    // kein Chat-Bypass

        var ok = await InvokeAsync(tools, "submit_paused_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[]
            { new AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecision("op-0", "apply", null),
              new AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecision("op-1", "skip", "Drift erst ernten") } });
        Assert.True(ok.GetProperty("saved").GetBoolean());
        // 1c (18.08., Vertragswechsel — löst den C5b-Hinweis „danach resume_run" ab): der Submit kettet die
        // Fortsetzung automatisch (R-43 auf der Chat-Bahn); ohne verdrahteten Chain-Delegat wird nur gespeichert.
        Assert.Contains("Fortsetzung kettet automatisch", ok.GetProperty("hint").GetString());

        var file = System.Text.Json.JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecisionsFile>(
            File.ReadAllText(Path.Combine(repo, "runs", "fullworkflow", runId, "07-github", "github-forward-decisions.json")),
            AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", file.Reviewer);               // gleicher Vertrag, neuer Kanal
        Assert.Equal(2, file.Decisions.Count);
    }

    // Politur 1c (18.08., R-43 auf die Chat-Bahn): der Submit kettet die Fortsetzung automatisch über den
    // injizierten Delegat (= StewardRunTools.ChainResumeAsync in der Hülle); chainResume=false = nur speichern.
    private static string RepoWithForwardPause(string runId)
    {
        var repo = Directory.CreateTempSubdirectory("p1c-").FullName;
        var run = Path.Combine(repo, "runs", "fullworkflow", runId);
        Directory.CreateDirectory(Path.Combine(run, "checkpoints"));
        Directory.CreateDirectory(Path.Combine(run, "07-github"));
        File.WriteAllText(Path.Combine(run, "checkpoints", "pointer.json"),
            $$"""{ "runId":"{{runId}}", "sessionId":"s", "checkpointId":"c1", "mode":"github-forward-gate", "savedUtc":"2026-08-18T10:00:00Z" }""");
        File.WriteAllText(Path.Combine(run, "07-github", "github-forward-plan.json"),
            """{ "operations": [ { "kind": "UPDATE_ISSUE", "pbiId": "PBI-1" } ] }""");
        return repo;
    }

    [Fact]
    public async Task P1c_Submit_kettet_die_Fortsetzung_automatisch_und_Optout_speichert_nur()
    {
        var runId = "run-1c";
        var repo = RepoWithForwardPause(runId);
        var chained = new List<string>();
        var tools = new StewardGateTools(repo,
            chainResume: id => { chained.Add(id); return Task.FromResult("""{ "resuming": true }"""); });

        var res = await InvokeAsync(tools, "submit_paused_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[] { new GithubForwardDecision("op-0", "apply", null) } });
        Assert.Equal(runId, Assert.Single(chained));                       // EIN Urteil → Fortsetzung kettet
        Assert.True(res.GetProperty("resumed").GetProperty("resuming").GetBoolean());

        var res2 = await InvokeAsync(tools, "submit_paused_gate_decisions", new Dictionary<string, object?>
        { ["runId"] = runId, ["decisions"] = new[] { new GithubForwardDecision("op-0", "apply", null) }, ["chainResume"] = false });
        Assert.Single(chained);                                            // Opt-out: NICHT erneut gekettet
        Assert.False(res2.TryGetProperty("resumed", out _));
    }
}
