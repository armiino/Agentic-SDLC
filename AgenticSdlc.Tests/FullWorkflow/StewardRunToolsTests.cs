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
        Assert.Equal(6, tools.OfType<ApprovalRequiredAIFunction>().Count());   // K3: kein Start ohne Zustimmung (C2c: +4 GitHub-Seile)
    }

    [Fact]
    public async Task Aux_Seile_bauen_die_richtigen_Bahn_Args_und_melden_Fehler_LAUT()
    {
        string[]? seen = null;
        var tools = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0), auxRunner: a => { seen = a; return Task.FromResult(0); });

        var snap = await InvokeAsync(tools, "pull_github_snapshot", new Dictionary<string, object?> { ["repo"] = "owner/name" });
        Assert.True(snap.GetProperty("ok").GetBoolean());
        Assert.Equal(["github-snapshot", "issues", "--repo", "owner/name"], seen!);

        await InvokeAsync(tools, "run_github_inbound", new Dictionary<string, object?> { ["draft"] = true });
        Assert.Equal(["github-inbound", "--draft"], seen!);
        await InvokeAsync(tools, "run_github_reverse", new Dictionary<string, object?>());
        Assert.Equal(["github-reverse"], seen!);

        var failing = new StewardRunTools(".", S(), (a, cb) => Task.FromResult(0), auxRunner: _ => Task.FromResult(2));
        var err = await InvokeAsync(failing, "run_github_inbound", new Dictionary<string, object?>());
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
