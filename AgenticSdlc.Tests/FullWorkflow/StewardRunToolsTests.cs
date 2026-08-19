using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Steward;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C1c (07.08.) — die startenden Tools, LLM-frei via Fake-Runner: ApprovalRequired-Wrapping, start-async
// (runId SOFORT über die neue Runner-Naht), STEWARD_BUSY-Schutz, Startfehler LAUT, resume-Args korrekt.
public sealed class StewardRunToolsTests
{
    private static HostSettings S() => HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());

    private static async Task<JsonElement> InvokeAsync(StewardRunTools tools, string name, IDictionary<string, object?> args)
    {
        var fn = tools.Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args));
        return JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public void Alle_Start_Tools_sind_ApprovalRequired_gewrappt()
    {
        // ⚖ 13.08. Klasse-Regel: die 4 blinden Werkbank-Seile (clarify_sweep/inbound/distill/reverse) sind ENTFERNT
        // (system-inventar §3) — bleiben 10 zustimmungspflichtige Tools (Graph-Starter, Pulls, UIs, post_comment).
        var tools = new StewardRunTools(".", S(), (args, cb) => Task.FromResult(0)).Build();
        Assert.Equal(10, tools.OfType<ApprovalRequiredAIFunction>().Count());
        // Wächter: keines der entfernten Seile darf zurückkommen, ohne dass sein Ergebnis im Chat lesbar ist.
        var names = tools.OfType<AIFunction>().Select(f => f.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var gone in new[] { "run_clarify_sweep", "run_github_inbound", "run_comment_distill", "run_github_reverse" })
            Assert.DoesNotContain(gone, names);
    }

    [Fact]
    public async Task K13_2_Seile_rufen_TYPISIERTE_Naehte_und_melden_Fehler_LAUT()
    {
        string? repo = null; string? comments = null; string? review = null;
        var tools = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0),
            pullSnapshot: r => { repo = r; return Task.FromResult(0); },
            pullComments: r => { comments = r; return Task.FromResult(0); },
            openReview: id => { review = id; return Task.FromResult(0); });

        var snap = await InvokeAsync(tools, "pull_github_snapshot", new Dictionary<string, object?> { ["repo"] = "owner/name" });
        Assert.True(snap.GetProperty("ok").GetBoolean());
        Assert.Equal("owner/name", repo);                                     // typisiert — keine CLI-Args mehr

        await InvokeAsync(tools, "pull_issue_comments", new Dictionary<string, object?> { ["repo"] = "owner/name" });
        Assert.Equal("owner/name", comments);
        await InvokeAsync(tools, "open_review_ui", new Dictionary<string, object?> { ["proposalId"] = "PEND-x" });
        Assert.Equal("PEND-x", review);

        var failing = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0), pullSnapshot: _ => Task.FromResult(2));
        var err = await InvokeAsync(failing, "pull_github_snapshot", new Dictionary<string, object?> { ["repo"] = "o/n" });
        Assert.Equal("AUX_RUN_FAILED", err.GetProperty("error").GetString());
    }

    [Fact]
    public async Task C4c_save_sweep_answers_stempelt_Quelle()
    {
        var repo = Directory.CreateTempSubdirectory("c4c-").FullName;
        // Feil ② (Abnahme 4.0, Vertragswechsel): der Session-Name kommt als HARNESS-Fakt aus dem ctor
        // (--session) — das Modell erfand vorher eigene Werte („steward-chat") und der Stempel log.
        var tools = new StewardRunTools(repo, S(), (a, cb) => Task.FromResult(0), sessionName: "probe");

        var bad = await InvokeAsync(tools, "save_sweep_answers", new Dictionary<string, object?>
        { ["answers"] = new[] { new AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer("PBI-1", "") } });
        Assert.Equal("ANSWERS_INVALID", bad.GetProperty("error").GetString());       // leer = LAUT

        var ok = await InvokeAsync(tools, "save_sweep_answers", new Dictionary<string, object?>
        { ["answers"] = new[] { new AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer("PBI-1", "Flutter+SQLite") } });
        var path = ok.GetProperty("answersPath").GetString()!;
        var saved = System.Text.Json.JsonSerializer.Deserialize<List<AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer>>(
            File.ReadAllText(Path.Combine(repo, path)), AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", saved.Single().Quelle);              // §8-Herkunft gestempelt
        Assert.Equal("probe", saved.Single().SessionName);
        // Betriebspfad danach: run_clarify_via_graph (eigener Test unten) — das Werkbank-Seil run_clarify_sweep ist entfernt.
    }

    // Feil ② (Abnahme 4.0): das Autor-Front-Delta trägt den ECHTEN --session-Namen als Herkunft — daraus
    // stempelt der Frage-DEC-Mint später `ingestedFromSession` (live log der Wert „steward-chat").
    [Fact]
    public async Task save_author_statements_stempelt_den_Harness_Session_Namen()
    {
        var repo = Directory.CreateTempSubdirectory("af-sess-").FullName;
        var tools = new StewardRunTools(repo, S(), (a, cb) => Task.FromResult(0), sessionName: "abnahme4");

        var ok = await InvokeAsync(tools, "save_author_statements", new Dictionary<string, object?>
        { ["statements"] = new[] { new AgenticSdlc.Host.FullWorkflow.Delta.AuthorStatement("Neue Anforderung X.", "requirement", null, null) } });

        var delta = System.Text.Json.JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Delta.ProjectStateDocument>(
            File.ReadAllText(Path.Combine(repo, ok.GetProperty("deltaPath").GetString()!)),
            AgenticSdlc.Host.FullWorkflow.Delta.ProjectStateJson.Options)!;
        Assert.Equal("abnahme4", delta.Items.Single().SourceRunId);
    }

    // Feil ③ (Abnahme 4.0): open_gate_ui BLOCKIERT durch UI + automatische Fortsetzung — das Ergebnis muss
    // das SAGEN, sonst bittet der Agent den Autor um eine „fertig"-Meldung, die nie nötig war (Session-Beleg).
    [Fact]
    public async Task UiRoundResult_meldet_Abschluss_und_neuen_Stand()
    {
        var repo = Directory.CreateTempSubdirectory("uiround-").FullName;
        var runId = "20260819_000001_test";
        var dir = Path.Combine(repo, "runs", "fullworkflow", runId, "checkpoints");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "pointer.json"),
            $"{{\"runId\":\"{runId}\",\"sessionId\":\"s\",\"checkpointId\":\"cp\",\"mode\":\"github-forward-gate\",\"savedUtc\":\"2026-08-19T09:00:00Z\"}}");

        var ok = System.Text.Json.JsonDocument.Parse(
            await StewardRunTools.UiRoundResultAsync(repo, runId, "pbi-update-review", 0)).RootElement;
        Assert.True(ok.GetProperty("uiRundeAbgeschlossen").GetBoolean());
        Assert.Equal("github-forward-gate", ok.GetProperty("pausedGate").GetString());  // der NEUE Stand reist mit
        Assert.Contains("warte NICHT", ok.GetProperty("hint").GetString());

        var err = System.Text.Json.JsonDocument.Parse(
            await StewardRunTools.UiRoundResultAsync(repo, runId, "pbi-update-review", 5)).RootElement;
        Assert.Equal("AUX_RUN_FAILED", err.GetProperty("error").GetString());
    }

    [Fact]
    public async Task run_pipeline_from_github_nutzt_die_Ein_Graph_Front()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { seen = args; onRunId?.Invoke("run-7"); return Task.FromResult(0); });
        var started = await InvokeAsync(tools, "run_pipeline_from_github", new Dictionary<string, object?>());
        Assert.True(started.GetProperty("started").GetBoolean());
        Assert.Equal(["pipeline-full", "run", "--from-github"], seen!);        // §13: Seil zeigt auf den Ein-Graph-Eingang
    }

    // ONLY-STEWARD Recovery: run_reproject startet den durablen Re-Projektions-Modus des Ein-Graphen.
    [Fact]
    public async Task run_reproject_startet_die_only_steward_re_projektion()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { seen = args; onRunId?.Invoke("run-rp"); return Task.FromResult(0); });
        var started = await InvokeAsync(tools, "run_reproject", new Dictionary<string, object?>());
        Assert.True(started.GetProperty("started").GetBoolean());
        Assert.Equal(["pipeline-full", "run", "--reproject"], seen!);          // Seil zeigt auf den Recovery-Eingang (Delta aus dem Core)
    }

    [Fact]
    public async Task run_clarify_via_graph_startet_den_durablen_Betriebspfad()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { seen = args; onRunId?.Invoke("run-9"); return Task.FromResult(0); });
        var started = await InvokeAsync(tools, "run_clarify_via_graph", new Dictionary<string, object?> { ["answersPath"] = "sweep.json" });
        Assert.True(started.GetProperty("started").GetBoolean());
        Assert.Equal(["pipeline-full", "run", "--from-clarify", "sweep.json"], seen!);   // A′-3a: Betriebspfad durch den durablen Graphen
    }

    [Fact]
    public async Task Start_liefert_runId_SOFORT_und_BUSY_schuetzt_vor_Parallel_Start()
    {
        var release = new TaskCompletionSource<int>();
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { onRunId?.Invoke("run-42"); return release.Task; });

        var started = await InvokeAsync(tools, "run_pipeline_from_delta", new Dictionary<string, object?> { ["deltaPath"] = "x.json" });
        Assert.True(started.GetProperty("started").GetBoolean());
        Assert.Equal("run-42", started.GetProperty("runId").GetString());      // runId kam VOR Lauf-Ende (start-async)

        var busy = await InvokeAsync(tools, "run_pipeline_from_delta", new Dictionary<string, object?> { ["deltaPath"] = "y.json" });
        Assert.Equal("STEWARD_BUSY", busy.GetProperty("error").GetString());   // K2: ein aktiver Lauf je Sitzung

        release.SetResult(0);
        await tools.Started["run-42"];
        var again = await InvokeAsync(tools, "run_pipeline_from_delta", new Dictionary<string, object?> { ["deltaPath"] = "z.json" });
        Assert.True(again.GetProperty("started").GetBoolean());               // nach Ende wieder frei
    }

    [Fact]
    public async Task Startfehler_ist_LAUT_und_resume_baut_die_richtigen_Args()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { seen = args; return Task.FromResult(2); });

        var fail = await InvokeAsync(tools, "run_pipeline_from_delta", new Dictionary<string, object?> { ["deltaPath"] = "fehlt.json" });
        Assert.Equal("RUN_START_FAILED", fail.GetProperty("error").GetString());
        Assert.Equal(2, fail.GetProperty("exitCode").GetInt32());              // Runner-Exit sichtbar, nichts still

        var res = await InvokeAsync(tools, "resume_run", new Dictionary<string, object?> { ["runId"] = "r-1", ["acceptAll"] = true });
        Assert.True(res.GetProperty("resuming").GetBoolean());
        await tools.Started["r-1"];
        Assert.Equal(["pipeline-full", "resume", "r-1", "--accept-all"], seen!);
    }

    // R-51-Wache (17.08., Block E): der MAF-Checkpoint-Store ist prozess-exklusiv — solange der gestartete Lauf
    // in DIESEM Prozess arbeitet, liefe ein zweiter Resume in den Store-Konflikt. Ehrliche Sofort-Ablehnung.
    [Fact]
    public async Task R51_resume_auf_noch_aktiven_Lauf_wird_ehrlich_abgelehnt_statt_Store_Konflikt()
    {
        var release = new TaskCompletionSource<int>();
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { onRunId?.Invoke("run-51"); return release.Task; });

        await InvokeAsync(tools, "run_pipeline_from_delta", new Dictionary<string, object?> { ["deltaPath"] = "x.json" });
        var res = await InvokeAsync(tools, "resume_run", new Dictionary<string, object?> { ["runId"] = "run-51" });
        Assert.Equal("RUN_STILL_ACTIVE", res.GetProperty("error").GetString());

        release.SetResult(6);                                                  // Lauf pausiert (Exit 6) → Store frei
        await tools.Started["run-51"];
        var again = await InvokeAsync(tools, "resume_run", new Dictionary<string, object?> { ["runId"] = "run-51" });
        Assert.True(again.GetProperty("resuming").GetBoolean());               // danach ist der Resume wieder legitim
    }

    // R-51b (17.08., Block E): ein sterbender Lauf-Task wurde spurlos verschluckt („resuming true", dann Stille).
    // Wächter: Fault → LAUTE stderr-Meldung + Task endet mit −1 (nie faulted — await wirft nicht).
    [Fact]
    public async Task R51b_sterbender_Lauf_Task_ist_LAUT_und_nie_still_verschluckt()
    {
        var tools = new StewardRunTools(".", S(),
            (args, onRunId) => throw new InvalidOperationException("The store is already in use by another process"));

        var prev = Console.Error;
        var stderr = new StringWriter();
        Console.SetError(stderr);
        try
        {
            var res = await InvokeAsync(tools, "resume_run", new Dictionary<string, object?> { ["runId"] = "r-dead" });
            Assert.True(res.GetProperty("resuming").GetBoolean());             // start-async: Rückgabe kommt sofort

            var exit = await tools.Started["r-dead"];                          // NIE faulted — der await wirft nicht
            Assert.Equal(-1, exit);
            Assert.Contains("LAUF-TASK GESTORBEN", stderr.ToString());         // LAUT statt Spurlosigkeit
            Assert.Contains("already in use", stderr.ToString());
        }
        finally { Console.SetError(prev); }
    }
}
