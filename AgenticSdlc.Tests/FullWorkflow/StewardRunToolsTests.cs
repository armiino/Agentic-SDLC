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
    public void Beide_Start_Tools_sind_ApprovalRequired_gewrappt()
    {
        var tools = new StewardRunTools(".", S(), (args, cb) => Task.FromResult(0)).Build();
        Assert.Equal(2, tools.OfType<ApprovalRequiredAIFunction>().Count());   // K3: kein Start ohne Zustimmung
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
