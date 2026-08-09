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
        var tools = new StewardRunTools(".", S(), (args, cb) => Task.FromResult(0)).Build();
        Assert.Equal(8, tools.OfType<ApprovalRequiredAIFunction>().Count());   // K3: kein Start ohne Zustimmung (C2c +4, C4c +1, C4d +1)
    }

    [Fact]
    public async Task K13_2_Seile_rufen_TYPISIERTE_Naehte_und_melden_Fehler_LAUT()
    {
        string? repo = null; bool? draft = null; var reverse = 0; string? review = null;
        var tools = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0),
            pullSnapshot: r => { repo = r; return Task.FromResult(0); },
            runInbound: d => { draft = d; return Task.FromResult(0); },
            runReverse: () => { reverse++; return Task.FromResult(0); },
            openReview: id => { review = id; return Task.FromResult(0); });

        var snap = await InvokeAsync(tools, "pull_github_snapshot", new Dictionary<string, object?> { ["repo"] = "owner/name" });
        Assert.True(snap.GetProperty("ok").GetBoolean());
        Assert.Equal("owner/name", repo);                                     // typisiert — keine CLI-Args mehr

        await InvokeAsync(tools, "run_github_inbound", new Dictionary<string, object?> { ["draft"] = true });
        Assert.True(draft);
        await InvokeAsync(tools, "run_github_reverse", new Dictionary<string, object?>());
        Assert.Equal(1, reverse);
        await InvokeAsync(tools, "open_review_ui", new Dictionary<string, object?> { ["proposalId"] = "PEND-x" });
        Assert.Equal("PEND-x", review);

        var failing = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0), runInbound: _ => Task.FromResult(2));
        var err = await InvokeAsync(failing, "run_github_inbound", new Dictionary<string, object?>());
        Assert.Equal("AUX_RUN_FAILED", err.GetProperty("error").GetString());
    }

    [Fact]
    public async Task C4c_save_sweep_answers_stempelt_Quelle_und_run_clarify_sweep_faehrt_die_Bahn()
    {
        var repo = Directory.CreateTempSubdirectory("c4c-").FullName;
        string[]? seen = null;
        var tools = new StewardRunTools(repo, S(), (a, cb) => Task.FromResult(0), runSweep: p => { seen = ["clarify-sweep", "run", "--answers", p]; return Task.FromResult(0); });

        var bad = await InvokeAsync(tools, "save_sweep_answers", new Dictionary<string, object?>
        { ["answers"] = new[] { new AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer("PBI-1", "") } });
        Assert.Equal("ANSWERS_INVALID", bad.GetProperty("error").GetString());       // leer = LAUT

        var ok = await InvokeAsync(tools, "save_sweep_answers", new Dictionary<string, object?>
        { ["answers"] = new[] { new AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer("PBI-1", "Flutter+SQLite") }, ["sessionName"] = "probe" });
        var path = ok.GetProperty("answersPath").GetString()!;
        var saved = System.Text.Json.JsonSerializer.Deserialize<List<AgenticSdlc.Host.FullWorkflow.PbiUpdate.ClarifySweepAnswer>>(
            File.ReadAllText(Path.Combine(repo, path)), AgenticSdlc.Host.FullWorkflow.JsonFiles.Json)!;
        Assert.Equal("author via steward-chat", saved.Single().Quelle);              // §8-Herkunft gestempelt
        Assert.Equal("probe", saved.Single().SessionName);

        await InvokeAsync(tools, "run_clarify_sweep", new Dictionary<string, object?> { ["answersPath"] = path });
        Assert.Equal(["clarify-sweep", "run", "--answers", path], seen!);            // Seil -> Betriebs-Bahn
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

    [Fact]
    public async Task Start_liefert_runId_SOFORT_und_BUSY_schuetzt_vor_Parallel_Start()
    {
        var release = new TaskCompletionSource<int>();
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { onRunId?.Invoke("run-42"); return release.Task; });

        var started = await InvokeAsync(tools, "run_pipeline_full", new Dictionary<string, object?> { ["deltaPath"] = "x.json" });
        Assert.True(started.GetProperty("started").GetBoolean());
        Assert.Equal("run-42", started.GetProperty("runId").GetString());      // runId kam VOR Lauf-Ende (start-async)

        var busy = await InvokeAsync(tools, "run_pipeline_full", new Dictionary<string, object?> { ["deltaPath"] = "y.json" });
        Assert.Equal("STEWARD_BUSY", busy.GetProperty("error").GetString());   // K2: ein aktiver Lauf je Sitzung

        release.SetResult(0);
        await tools.Started["run-42"];
        var again = await InvokeAsync(tools, "run_pipeline_full", new Dictionary<string, object?> { ["deltaPath"] = "z.json" });
        Assert.True(again.GetProperty("started").GetBoolean());               // nach Ende wieder frei
    }

    [Fact]
    public async Task Startfehler_ist_LAUT_und_resume_baut_die_richtigen_Args()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (args, onRunId) => { seen = args; return Task.FromResult(2); });

        var fail = await InvokeAsync(tools, "run_pipeline_full", new Dictionary<string, object?> { ["deltaPath"] = "fehlt.json" });
        Assert.Equal("RUN_START_FAILED", fail.GetProperty("error").GetString());
        Assert.Equal(2, fail.GetProperty("exitCode").GetInt32());              // Runner-Exit sichtbar, nichts still

        var res = await InvokeAsync(tools, "resume_run", new Dictionary<string, object?> { ["runId"] = "r-1", ["acceptAll"] = true });
        Assert.True(res.GetProperty("resuming").GetBoolean());
        await tools.Started["r-1"];
        Assert.Equal(["pipeline-full", "resume", "r-1", "--accept-all"], seen!);
    }
}
