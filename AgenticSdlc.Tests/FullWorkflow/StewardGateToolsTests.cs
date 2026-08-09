using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
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
        Assert.Equal(2, tools_count(tools: new StewardGateTools(repo)));      // beide Submits ApprovalRequired (C5a+C5b)
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
        Assert.Contains("resume_run", ok.GetProperty("hint").GetString());

        var file = System.Text.Json.JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardDecisionsFile>(
            File.ReadAllText(Path.Combine(repo, "runs", "fullworkflow", runId, "07-github", "github-forward-decisions.json")),
            AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", file.Reviewer);               // gleicher Vertrag, neuer Kanal
        Assert.Equal(2, file.Decisions.Count);
    }
}
