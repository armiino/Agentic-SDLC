using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.FanOut;
using AgenticSdlc.Host.FullWorkflow.Load;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ⑤ (05.08.): der Load-Modus mit typisierter Bestellung (LoadBaselineRequest als Workflow-INPUT,
// R-37-Muster) statt "LOAD"-String-Sentinel + Konstruktions-Zeit-Bindung. Erster Graph-Test des Load-Wegs
// überhaupt: ECHTER Executor, In-Process-Lauf, Fixture-Artefakte auf Platte (deterministisch, kein LLM).
public sealed class LoadBaselineGraphTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private static string WriteSourceRun(params (string Type, int Items)[] artifacts)
    {
        var dir = Directory.CreateTempSubdirectory("load-src-").FullName;
        foreach (var (type, count) in artifacts)
        {
            var items = Enumerable.Range(1, count)
                .Select(i => new ArtifactItem($"{type.ToUpperInvariant()}-{i:D3}", ArtifactOrigin.Extracted, $"Item {i}", [], []))
                .ToList();
            var doc = new ArtifactDocument($"{type}-fixture", type, 1, ArtifactDocument.StageEvidenceBaseline,
                new ProducerMetadata("test-run", "none", null), items);
            var artifactDir = Path.Combine(dir, "baselines", type);
            Directory.CreateDirectory(artifactDir);
            File.WriteAllText(Path.Combine(artifactDir, "artifact.json"), JsonSerializer.Serialize(doc, Json));
        }
        return dir;
    }

    private sealed class CollectExecutor(ConcurrentBag<VerifiedBaselineSet> hits) : Executor<VerifiedBaselineSet>("LoadCollect")
    {
        public override ValueTask HandleAsync(VerifiedBaselineSet set, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(set); return ValueTask.CompletedTask; }
    }

    private static (Microsoft.Agents.AI.Workflows.Workflow Wf, ConcurrentBag<VerifiedBaselineSet> Hits, RunContext Run) BuildGraph()
    {
        var run = new RunContext(RunId.New(), "test-load-baseline"); run.EnsureFolders();
        var hits = new ConcurrentBag<VerifiedBaselineSet>();
        var load = new LoadBaselineExecutor(run);
        var collect = new CollectExecutor(hits);
        var b = new WorkflowBuilder(load).WithName("LoadBaselineTest");
        b.AddEdge(load, collect);
        return (b.Build(), hits, run);
    }

    [Fact]
    public async Task Typisierte_Bestellung_laedt_kopiert_und_sendet_das_Set()
    {
        var source = WriteSourceRun(("requirements", 3), ("open-questions", 2));
        var (wf, hits, run) = BuildGraph();

        var wfRun = await InProcessExecution.Default.RunAsync(
            wf, new LoadBaselineRequest(["requirements", "open-questions"], source), run.RunId, CancellationToken.None);

        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);
        var set = Assert.Single(hits);
        Assert.Equal(["open-questions", "requirements"], set.Artifacts.Select(a => a.ArtifactType));  // sortiert
        Assert.Equal([2, 3], set.Artifacts.Select(a => a.Items));
        // Disk-Vertrag wie im Build-Modus: Kopien im Run-Layout + baseline-set.json.
        Assert.True(File.Exists(Path.Combine(run.RunDir, "baselines", "requirements", "artifact.json")));
        Assert.True(File.Exists(Path.Combine(run.RunDir, "baselines", "open-questions", "artifact.json")));
        Assert.True(File.Exists(Path.Combine(run.RunDir, "baseline-set.json")));
        var events = await File.ReadAllTextAsync(run.EventsPath);
        Assert.Contains("BASELINE_LOADED", events);
        Assert.Contains("BASELINE_SET_READY", events);
    }

    [Fact]
    public async Task Fehlendes_Artefakt_scheitert_LAUT_mit_klarer_Diagnose()
    {
        var source = WriteSourceRun(("requirements", 3));                       // architecture fehlt bewusst
        var (wf, hits, run) = BuildGraph();

        var wfRun = await InProcessExecution.Default.RunAsync(
            wf, new LoadBaselineRequest(["requirements", "architecture"], source), run.RunId, CancellationToken.None);

        Assert.Empty(hits);                                                     // kein halbes Set
        Assert.Contains(wfRun.OutgoingEvents.OfType<ExecutorFailedEvent>(),
            f => f.Data?.Message?.Contains("architecture") == true && f.Data.Message.Contains("nicht im Quell-Run"));
    }
}
