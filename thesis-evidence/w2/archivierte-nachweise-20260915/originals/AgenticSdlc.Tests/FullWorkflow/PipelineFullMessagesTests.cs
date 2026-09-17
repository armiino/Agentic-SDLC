using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1e' Bauplan-Schritt 1: die Typed Messages sind die Stufengrenzen-Verträge + das metrics.json-Rohformat.
// Diese Tests frieren die Serialisierung ein (Round-Trip + stabile JSON-Feldnamen für den Messkontrakt).
public sealed class PipelineFullMessagesTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Stufen_Messages_ueberleben_den_Round_Trip()
    {
        var baseline = new BaselineStageOutput(["req.artifact.json", "arch.artifact.json"], 57, 39);
        var json = JsonSerializer.Serialize(baseline, Json);
        var back = JsonSerializer.Deserialize<BaselineStageOutput>(json, Json);

        Assert.NotNull(back);
        Assert.Equal(json, JsonSerializer.Serialize(back, Json)); // kanonischer Round-Trip (Record-Gleichheit taugt bei Listen nicht)
        Assert.Equal(baseline.ArtifactPaths, back!.ArtifactPaths); // strukturell (IEnumerable)
        Assert.Equal(57, back.RequirementCount);
    }

    [Fact]
    public void PipelineMetrics_Round_Trip_mit_verschachtelten_Records()
    {
        var metrics = new PipelineMetrics(
            RunId: "20260724_150000_abc123",
            BenchmarkVersion: "meeting-2-extended@v1",
            PolicyProfile: "replay",
            Models: new Dictionary<string, string> { ["01-ledger"] = "openai/gpt-5.4" },
            Stages:
            [
                new StageMetric("01-ledger", 8970, 1, new GateMetric(true, 0, 2), new TokenMetric(1120, 404)),
                new StageMetric("04-delta", 12, 1, null, new TokenMetric(0, 0))
            ],
            HumanGates: [new HumanGateMetric("adjudication-gate", "author (replay)")],
            CoreItemsBefore: 194,
            CoreItemsAfter: 196,
            IssuesCreated: 39,
            IssuesUpdated: 2,
            Violations: [],
            InternalQaRecall: new RecallMetric(0.91, "input/eval-labels/T9999_chaos.reference-ledger.json"));

        var json = JsonSerializer.Serialize(metrics, Json);
        var back = JsonSerializer.Deserialize<PipelineMetrics>(json, Json);

        Assert.NotNull(back);
        Assert.Equal(json, JsonSerializer.Serialize(back, Json)); // kanonischer Round-Trip über alle verschachtelten Records
        Assert.Equal(2, back!.Stages.Count);
        Assert.True(back.Stages[0].Gate!.Pass);
        Assert.Equal(404, back.Stages[0].Tokens.Out);
        Assert.Equal(0.91, back.InternalQaRecall!.Value, 3);
    }

    [Fact]
    public void Metrics_JSON_traegt_die_Messkontrakt_Pflichtfeldnamen()
    {
        var metrics = new PipelineMetrics(
            "r", "b", "interactive",
            new Dictionary<string, string>(), [], [], 0, 0, 0, 0, [], null);

        var json = JsonSerializer.Serialize(metrics, Json);

        // Stabile Feldnamen (der Messkontrakt v1 hängt daran — nicht versehentlich umbenennen).
        foreach (var field in new[] { "runId", "benchmarkVersion", "policyProfile", "models", "stages",
                                      "humanGates", "coreItemsBefore", "coreItemsAfter", "issuesCreated",
                                      "issuesUpdated", "violations", "internalQaRecall" })   // §27: Ex-"recall", umbenannt 04.09.
            Assert.Contains($"\"{field}\"", json);
    }

    [Fact]
    public void Optionale_Felder_sind_null_faehig()
    {
        var delta = new MeetingDeltaOutput("04-delta/project-state.json", 57, 59);
        var ledger = new LedgerStageOutput("validated.json", MissSignalPath: null, ClaimCount: 23, PendingCount: 3);

        Assert.Null(JsonSerializer.Deserialize<LedgerStageOutput>(JsonSerializer.Serialize(ledger, Json), Json)!.MissSignalPath);
        Assert.Equal(57, JsonSerializer.Deserialize<MeetingDeltaOutput>(JsonSerializer.Serialize(delta, Json), Json)!.ItemCount);
    }
}
