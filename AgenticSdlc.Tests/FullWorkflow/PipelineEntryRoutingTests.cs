using System.Collections.Concurrent;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ③ (05.08.): der Eingangs-Vertrag des Ein-Graphen — TYPISIERTES Input-Routing am Start
// (Transkript -> Front | fertiges Delta -> BranchDetector). In-Process-Lauf mit dem ECHTEN Dispatcher
// und derselben Kanten-Form wie in Assemble; Fehler-Events werden laut assertiert (Lehre aus R-37:
// Build()-bar beweist keine Verdrahtung, und Kontrakte greifen erst zur Laufzeit).
public sealed class PipelineEntryRoutingTests
{
    private sealed class FakeTranscriptReceiver(ConcurrentBag<string> hits) : Executor<TranscriptInput>("FakeFront")
    {
        public override ValueTask HandleAsync(TranscriptInput t, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add($"front:{t.TranscriptPath}"); return ValueTask.CompletedTask; }
    }

    private sealed class FakeBranchReceiver(ConcurrentBag<string> hits) : Executor<ProjectStateDocument>("FakeBranch")
    {
        public override ValueTask HandleAsync(ProjectStateDocument d, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add($"branch:{d.Items.Count}"); return ValueTask.CompletedTask; }
    }

    // A′ Schritt 2: dritter Empfänger für den clarify-Zweig (Typ-Routing des Entry).
    private sealed class FakeClarifyReceiver(ConcurrentBag<string> hits) : Executor<ClarifySweepInput>("FakeClarify")
    {
        public override ValueTask HandleAsync(ClarifySweepInput i, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add($"clarify:{i.Answers.Count}"); return ValueTask.CompletedTask; }
    }

    // ONLY-STEWARD: vierter Empfänger für den Recovery-Zweig (Typ-Routing des Entry).
    private sealed class FakeReprojectReceiver(ConcurrentBag<string> hits) : Executor<ReprojectRequest>("FakeReproject")
    {
        public override ValueTask HandleAsync(ReprojectRequest r, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add("reproject"); return ValueTask.CompletedTask; }
    }

    private static (Microsoft.Agents.AI.Workflows.Workflow Wf, ConcurrentBag<string> Hits, RunContext Run) BuildMiniGraph()
    {
        var hits = new ConcurrentBag<string>();
        var run = new RunContext(RunId.New(), "test-entry-routing");
        run.EnsureFolders();
        var entry = new PipelineEntryExecutor(run);
        var b = new WorkflowBuilder(entry).WithName("EntryRoutingTest");
        b.AddEdge(entry, new FakeTranscriptReceiver(hits));
        b.AddEdge(entry, new FakeBranchReceiver(hits));
        b.AddEdge(entry, new FakeClarifyReceiver(hits));
        b.AddEdge(entry, new FakeReprojectReceiver(hits));
        return (b.Build(), hits, run);
    }

    private static async Task<IReadOnlyList<string>> RunAndCollectFailures(Microsoft.Agents.AI.Workflows.Workflow wf, PipelineFullEntry entry, string runId)
    {
        var wfRun = await InProcessExecution.Default.RunAsync(wf, entry, runId, CancellationToken.None);
        return wfRun.OutgoingEvents
            .Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e switch
            {
                ExecutorFailedEvent f => $"{f.ExecutorId}: {f.Data?.Message}",
                WorkflowErrorEvent w => $"{w.GetType().Name}: {w.Exception?.Message}",
                _ => e.ToString() ?? ""
            }).ToList();
    }

    [Fact]
    public async Task Transkript_Eintrag_geht_zur_Front_Delta_Eintrag_zum_Branch()
    {
        var (wf, hits, run) = BuildMiniGraph();
        var failures = await RunAndCollectFailures(wf,
            new PipelineFullEntry(new TranscriptInput("t.txt", "text"), null), run.RunId);
        Assert.Empty(failures);
        Assert.Equal("front:t.txt", Assert.Single(hits));                       // NUR die Front, nie der Branch

        var (wf2, hits2, run2) = BuildMiniGraph();
        var delta = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
        var failures2 = await RunAndCollectFailures(wf2, new PipelineFullEntry(null, delta), run2.RunId);
        Assert.Empty(failures2);
        Assert.Equal("branch:0", Assert.Single(hits2));                         // NUR der Branch — Front übersprungen
    }

    [Fact]
    public async Task Clarify_Eintrag_geht_NUR_zum_ClarifyEntry()
    {
        var (wf, hits, run) = BuildMiniGraph();
        var input = new ClarifySweepInput([new ClarifySweepAnswer("PBI-01", "Antwort"), new ClarifySweepAnswer("PBI-02", "Antwort")]);
        var failures = await RunAndCollectFailures(wf, new PipelineFullEntry(null, null, input), run.RunId);
        Assert.Empty(failures);
        Assert.Equal("clarify:2", Assert.Single(hits));                         // NUR clarify — Front/Branch übersprungen
    }

    // ONLY-STEWARD Recovery: der reproject-Eintrag geht NUR zum ReprojectEntry (kein Front/Branch/Clarify).
    [Fact]
    public async Task Reproject_Eintrag_geht_NUR_zum_ReprojectEntry()
    {
        var (wf, hits, run) = BuildMiniGraph();
        var failures = await RunAndCollectFailures(wf, new PipelineFullEntry(null, null, null, new ReprojectRequest()), run.RunId);
        Assert.Empty(failures);
        Assert.Equal("reproject", Assert.Single(hits));                         // NUR reproject — alle anderen Zweige übersprungen
    }

    [Fact]
    public async Task Ungueltiger_Vertrag_scheitert_LAUT()
    {
        var (wf, hits, run) = BuildMiniGraph();
        var failures = await RunAndCollectFailures(wf, new PipelineFullEntry(null, null), run.RunId);
        Assert.Contains(failures, f => f.Contains("PIPELINE_ENTRY_INVALID"));   // R-18: kein stilles Raten (genau EINES von vier)
        Assert.Empty(hits);
    }
}
