using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-50 (17.08., Fund Block G7): „Ein Human-Gate ruft nur, wenn es etwas zu entscheiden gibt — ein Skip ist
// immer LAUT." Wächter: (a) die Leer-Gate-Durchleiter beantworten ein 0-Ops-Gate mit der LEEREN Antwort auf
// dem NORMALEN Apply-Pfad + GATE_SKIPPED_EMPTY-Event (In-Process, echte Kanten-Form) · (b) die Forward-Bridge
// skippt SEMANTISCH (entries==0), statt nur „Datei existiert" zu prüfen — kein Snapshot-Pull für nichts.
public sealed class EmptyGateAutoResponderTests
{
    private sealed class PbiResponseSink(ConcurrentBag<object> hits) : Executor<PbiUpdateReviewResponse>("Sink")
    { public override ValueTask HandleAsync(PbiUpdateReviewResponse r, IWorkflowContext c, CancellationToken ct = default) { hits.Add(r); return ValueTask.CompletedTask; } }

    private sealed class FwdResponseSink(ConcurrentBag<object> hits) : Executor<ForwardReviewResponse>("Sink")
    { public override ValueTask HandleAsync(ForwardReviewResponse r, IWorkflowContext c, CancellationToken ct = default) { hits.Add(r); return ValueTask.CompletedTask; } }

    [Fact]
    public async Task Leeres_pbi_gate_wird_LAUT_zur_leeren_Antwort_auf_dem_normalen_Apply_Pfad()
    {
        var run = new RunContext(RunId.New(), "r50-pbi"); run.EnsureFolders();
        var hits = new ConcurrentBag<object>();
        var responder = new PbiUpdateEmptyGateResponder(run);
        var b = new WorkflowBuilder(responder).WithName("R50Pbi");
        b.AddEdge(responder, new PbiResponseSink(hits));
        await InProcessExecution.Default.RunAsync(b.Build(),
            new PbiUpdateGateEmpty(new PbiUpdateReviewRequest(run.RunId, "src", [])), run.RunId, CancellationToken.None);

        var resp = (PbiUpdateReviewResponse)Assert.Single(hits);
        Assert.Empty(resp.AcceptedOpIds);
        Assert.Contains("leeres Gate", resp.Reviewer);                                   // ehrlich gestempelt, kein Fake-Human
        Assert.Contains("GATE_SKIPPED_EMPTY", File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl")));  // LAUT, nie still
    }

    [Fact]
    public async Task Leeres_forward_gate_analog_mit_Execute_false()
    {
        var run = new RunContext(RunId.New(), "r50-fwd"); run.EnsureFolders();
        var hits = new ConcurrentBag<object>();
        var responder = new GithubForwardEmptyGateResponder(run);
        var b = new WorkflowBuilder(responder).WithName("R50Fwd");
        b.AddEdge(responder, new FwdResponseSink(hits));
        await InProcessExecution.Default.RunAsync(b.Build(),
            new GithubForwardGateEmpty(new ForwardReviewRequest(run.RunId, "src", [])), run.RunId, CancellationToken.None);

        var resp = (ForwardReviewResponse)Assert.Single(hits);
        Assert.Empty(resp.AcceptedOpIds);
        Assert.False(resp.Execute);                                                      // leeres Gate schreibt NIE
    }

    // R-50-Vervollständigung (17.08.): auch ingest/adjudication/cluster — die Familie ist KOMPLETT (Audit).
    [Fact]
    public async Task Ingest_Adjudication_Cluster_Leer_Gates_antworten_leer_und_LAUT()
    {
        var run = new RunContext(RunId.New(), "r50-rest"); run.EnsureFolders();
        var hits = new ConcurrentBag<object>();

        var ing = new IngestionEmptyGateResponder(run, "ingest-gate");
        var b1 = new WorkflowBuilder(ing).WithName("R50Ing");
        b1.AddEdge(ing, new IngSink(hits));
        await InProcessExecution.Default.RunAsync(b1.Build(),
            new IngestionGateEmpty(new AgenticSdlc.Host.FullWorkflow.Core.IngestionReviewRequest(run.RunId, []), "ingest-gate"), run.RunId, CancellationToken.None);

        var adj = new AdjudicationEmptyGateResponder(run);
        var b2 = new WorkflowBuilder(adj).WithName("R50Adj");
        b2.AddEdge(adj, new AdjSink(hits));
        await InProcessExecution.Default.RunAsync(b2.Build(),
            new AdjudicationGateEmpty(new AdjudicationReviewRequest([], "lr", "q", "v")), run.RunId, CancellationToken.None);

        var clu = new ClusterEmptyGateResponder(run);
        var b3 = new WorkflowBuilder(clu).WithName("R50Clu");
        b3.AddEdge(clu, new CluSink(hits));
        await InProcessExecution.Default.RunAsync(b3.Build(),
            new ClusterGateEmpty(new ClusterReviewRequest([], true, "ok")), run.RunId, CancellationToken.None);

        Assert.Equal(3, hits.Count);                       // alle drei liefern die leere Antwort auf dem Apply-Pfad
        var events = File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl"));
        Assert.Equal(3, events.Split("GATE_SKIPPED_EMPTY").Length - 1);   // und ALLE drei LAUT
    }

    private sealed class IngSink(ConcurrentBag<object> hits) : Executor<AgenticSdlc.Host.FullWorkflow.Core.IngestionReviewResponse>("IngSink")
    { public override ValueTask HandleAsync(AgenticSdlc.Host.FullWorkflow.Core.IngestionReviewResponse r, IWorkflowContext c, CancellationToken ct = default) { hits.Add(r); return ValueTask.CompletedTask; } }
    private sealed class AdjSink(ConcurrentBag<object> hits) : Executor<AdjudicationReviewResponse>("AdjSink")
    { public override ValueTask HandleAsync(AdjudicationReviewResponse r, IWorkflowContext c, CancellationToken ct = default) { hits.Add(r); return ValueTask.CompletedTask; } }
    private sealed class CluSink(ConcurrentBag<object> hits) : Executor<ClusterReviewResponse>("CluSink")
    { public override ValueTask HandleAsync(ClusterReviewResponse r, IWorkflowContext c, CancellationToken ct = default) { hits.Add(r); return ValueTask.CompletedTask; } }

    [Fact]
    public async Task Forward_Bridge_skippt_SEMANTISCH_bei_leerem_sync_delta()
    {
        var run = new RunContext(RunId.New(), "r50-bridge"); run.EnsureFolders();
        var pbiOut = Directory.CreateTempSubdirectory("r50-").FullName;
        Directory.CreateDirectory(Path.Combine(pbiOut, "applied"));
        File.WriteAllText(Path.Combine(pbiOut, "applied", "github-sync-delta.json"),
            """{"newPbis":[],"updatedPbis":[],"entries":[]}""");                          // Datei EXISTIERT, aber leer (der G7-Fall)

        var bridge = new OperationalForwardBridgeExecutor(run, pbiOut);
        var preps = new ConcurrentBag<object>();
        var b = new WorkflowBuilder(bridge).WithName("R50Bridge");
        b.AddEdge(bridge, new PrepSink(preps));
        b.WithOutputFrom(bridge);
        await InProcessExecution.Default.RunAsync(b.Build(),
            new PbiUpdateApplyReport([], [], new Dictionary<string, string>(), 0, 0, []), run.RunId, CancellationToken.None);

        Assert.Empty(preps);                                                             // KEIN ForwardPrep → kein Snapshot-Pull, kein Leer-Gate
        var events = File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl"));
        Assert.Contains("PIPELINE_FORWARD_BRIDGE_SKIPPED", events);                      // LAUT übersprungen (Event, nicht still)
        Assert.Contains("leeres sync-delta", events);
    }

    private sealed class PrepSink(ConcurrentBag<object> hits) : Executor<ForwardPrep>("PrepSink")
    { public override ValueTask HandleAsync(ForwardPrep p, IWorkflowContext c, CancellationToken ct = default) { hits.Add(p); return ValueTask.CompletedTask; } }
}
