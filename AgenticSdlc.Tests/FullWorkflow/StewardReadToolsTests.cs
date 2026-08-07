using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Steward;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C1a (07.08.) — die Pipeline-Lese-Tools der Steward-Schicht, LLM-frei direkt invoked: sie liefern EXAKT
// den typisierten A3-Vertrag der Workflow-Seite — kein Drift, kein Write. (Core-Lesen: CoreQueryToolsTests.)
public sealed class StewardReadToolsTests
{
    private static string Repo()
    {
        var repo = Directory.CreateTempSubdirectory("c1a-").FullName;
        // pausierter Lauf (A3-Fixture-Muster)
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "20260807_111111_aaaaaa");
        Directory.CreateDirectory(Path.Combine(runDir, "checkpoints"));
        File.WriteAllText(Path.Combine(runDir, "checkpoints", "pointer.json"),
            """{"runId":"20260807_111111_aaaaaa","sessionId":"s","checkpointId":"cp-7","mode":"arch-classify-gate","savedUtc":"2026-08-07T09:00:00Z"}""");
        return repo;
    }

    private static async Task<JsonElement> InvokeAsync(string repo, string name, IDictionary<string, object?>? args = null)
    {
        var fn = new StewardReadTools(repo).Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args ?? new Dictionary<string, object?>()));
        return JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public async Task get_run_status_liefert_den_A3_Vertrag_und_RUN_NOT_FOUND_laut()
    {
        var repo = Repo();
        var st = await InvokeAsync(repo, "get_run_status", new Dictionary<string, object?> { ["runId"] = "20260807_111111_aaaaaa" });
        Assert.Equal("paused", st.GetProperty("state").GetString(), ignoreCase: true);
        Assert.Equal("arch-classify-gate", st.GetProperty("pausedGate").GetString());
        Assert.Contains("arch-classify-review", st.GetProperty("nextRequiredAction")[0].GetString());

        var miss = await InvokeAsync(repo, "get_run_status", new Dictionary<string, object?> { ["runId"] = "fehlt" });
        Assert.Equal("RUN_NOT_FOUND", miss.GetProperty("error").GetString());
    }

    [Fact]
    public async Task list_paused_findet_den_pausierten_Lauf()
    {
        var paused = await InvokeAsync(Repo(), "list_paused_runs");
        Assert.Equal(1, paused.GetProperty("pausedCount").GetInt32());
        Assert.Equal("arch-classify-gate", paused.GetProperty("runs")[0].GetProperty("pausedGate").GetString());
    }
}
