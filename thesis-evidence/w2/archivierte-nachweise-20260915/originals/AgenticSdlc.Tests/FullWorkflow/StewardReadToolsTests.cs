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
        // 1b-Rest (18.08.): Zeile 0 ist jetzt der Checkpoint-Kompass — der Review-Befehl folgt danach.
        Assert.Contains("Checkpoint", st.GetProperty("nextRequiredAction")[0].GetString());
        Assert.Contains("arch-classify-review", st.GetProperty("nextRequiredAction")[1].GetString());

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

    [Fact]
    public async Task read_run_report_findet_Apply_Reports_JEDER_Stufe_auch_OHNE_Ingest()
    {
        // Steward-UX-Fund 11.08.: ein clarify-Graph-Lauf hat KEINEN Ingest → der alte hartkodierte Ingest-Pfad
        // fand nie einen Report (REPORT_NOT_FOUND am Abschluss). Jetzt: sammelt die vorhandenen Apply-Reports.
        var repo = Directory.CreateTempSubdirectory("c1a-report-").FullName;
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "20260811_clarify");
        Directory.CreateDirectory(Path.Combine(runDir, "07-pbi-update", "applied"));
        File.WriteAllText(Path.Combine(runDir, "07-pbi-update", "applied", "pbi-update-apply-report.json"),
            """{"newPbis":[],"updatedPbis":["PBI-010"],"finalStatus":{"PBI-010":"active"}}""");
        Directory.CreateDirectory(Path.Combine(runDir, "07-github", "applied"));
        File.WriteAllText(Path.Combine(runDir, "07-github", "applied", "github-forward-apply-report.json"),
            """{"executed":false,"dryRun":true}""");

        var rep = await InvokeAsync(repo, "read_run_report", new Dictionary<string, object?> { ["runId"] = "20260811_clarify" });
        var stages = rep.GetProperty("stages");
        Assert.True(stages.TryGetProperty("pbiUpdate", out _));               // gefunden trotz fehlendem Ingest (war der Bug)
        Assert.True(stages.TryGetProperty("forward", out _));
        Assert.Equal("active", stages.GetProperty("pbiUpdate").GetProperty("finalStatus").GetProperty("PBI-010").GetString());

        // gar kein Apply -> ehrlich REPORT_NOT_FOUND
        var empty = Directory.CreateTempSubdirectory("c1a-empty-").FullName;
        Directory.CreateDirectory(Path.Combine(empty, "runs", "fullworkflow", "r-empty"));
        var miss = await InvokeAsync(empty, "read_run_report", new Dictionary<string, object?> { ["runId"] = "r-empty" });
        Assert.Equal("REPORT_NOT_FOUND", miss.GetProperty("error").GetString());
    }

    // ⚖ 13.08. (ersetzt „Slice 1"): das Ernte-Ergebnis eines --from-github-Laufs ist im Chat LESBAR — auch wenn
    // der Lauf leer stoppte („ohne Tor-fähige Funde"). Der Steward berichtet die harvest-Zahlen statt REPORT_NOT_FOUND.
    [Fact]
    public async Task read_run_report_liest_das_Ernte_Ergebnis_eines_from_github_Laufs_auch_wenn_leer()
    {
        var repo = Directory.CreateTempSubdirectory("c1a-harvest-").FullName;
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "20260813_ernte");
        Directory.CreateDirectory(Path.Combine(runDir, "00-github-inbound"));
        File.WriteAllText(Path.Combine(runDir, "00-github-inbound", "harvest-report.json"),
            """{"snapshot":"runs/x/issues.json","report":{"totalIssues":39,"finds":[],"skipped":[],"unchanged":39}}""");

        var rep = await InvokeAsync(repo, "read_run_report", new Dictionary<string, object?> { ["runId"] = "20260813_ernte" });
        var harvest = rep.GetProperty("stages").GetProperty("harvest").GetProperty("report");
        Assert.Equal(39, harvest.GetProperty("totalIssues").GetInt32());
        Assert.Equal(39, harvest.GetProperty("unchanged").GetInt32());        // „nichts zu ernten" ist BERICHTBAR, kein NOT_FOUND
        Assert.Equal(0, harvest.GetProperty("finds").GetArrayLength());
    }

    // Fix A zu R-48 (13.08.): ein Forward, der am eigenen Checker scheitert (MaxAttemptsReached, KEIN Apply),
    // war im Bericht unsichtbar — der Steward meldete „fertig ✓". Jetzt ist der Plan-Ausgang lesbar.
    [Fact]
    public async Task read_run_report_zeigt_den_Forward_Plan_Ausgang_auch_ohne_Apply()
    {
        var repo = Directory.CreateTempSubdirectory("c1a-fwdplan-").FullName;
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "20260813_f");
        Directory.CreateDirectory(Path.Combine(runDir, "07-github"));
        File.WriteAllText(Path.Combine(runDir, "07-github", "github-forward-summary.json"),
            """{"runId":"20260813_f","gatePass":false,"gateErrors":1,"attempts":2,"finalDecision":"MaxAttemptsReached"}""");

        var rep = await InvokeAsync(repo, "read_run_report", new Dictionary<string, object?> { ["runId"] = "20260813_f" });
        var fwd = rep.GetProperty("stages").GetProperty("forwardPlan");
        Assert.False(fwd.GetProperty("gatePass").GetBoolean());              // der Fehlschlag ist SICHTBAR
        Assert.Equal("MaxAttemptsReached", fwd.GetProperty("finalDecision").GetString());
    }
}
