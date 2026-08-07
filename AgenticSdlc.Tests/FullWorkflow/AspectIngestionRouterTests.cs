using System.Collections.Concurrent;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A1b (05.08.): der Aspekt-Router — Registry statt switch; req läuft UNVERÄNDERT durch; unregistrierte
// Aspekte werden LAUT geparkt (ASPECT_UNROUTED, D-8: vorher stilles Durchreisen); die 9g-Fragen-Spur ist
// Querschnitt, kein Aspekt. In-Process am echten Router-Knoten (Lehre R-37: Verdrahtung nur per Lauf beweisbar).
public sealed class AspectIngestionRouterTests
{
    private static ProjectStateItem Item(string id, string type) => new ProjectStateItem(
        id, type, $"Text {id}", "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private sealed class FakeKernReceiver(ConcurrentBag<IngestionResolveInput> hits) : Executor<IngestionResolveInput>("FakeKern")
    {
        public override ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(input); return ValueTask.CompletedTask; }
    }

    private static async Task<(IReadOnlyList<IngestionResolveInput> Hits, string Events, IReadOnlyList<string> Failures)> RunAsync(ProjectStateDocument delta)
    {
        var run = new RunContext(RunId.New(), "test-aspect-router"); run.EnsureFolders();
        var hits = new ConcurrentBag<IngestionResolveInput>();
        var router = new AspectIngestionRouterExecutor(run, [AspectIngestionProfile.Requirement, AspectIngestionProfile.Architecture]);
        var kern = new FakeKernReceiver(hits);
        var b = new WorkflowBuilder(router).WithName("AspectRouterTest");
        b.AddEdge(router, kern);

        var wfRun = await InProcessExecution.Default.RunAsync(
            b.Build(), new IngestionResolveInput(delta, Doc(), "src", 2), run.RunId, CancellationToken.None);
        var failures = wfRun.OutgoingEvents.Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e.ToString() ?? "").ToList();
        var events = File.Exists(run.EventsPath) ? await File.ReadAllTextAsync(run.EventsPath) : "";
        return (hits.ToList(), events, failures);
    }

    [Fact]
    public async Task Req_und_Fragen_laufen_unveraendert_durch_ohne_Warnung()
    {
        var (hits, events, failures) = await RunAsync(Doc(Item("R-1", "requirement"), Item("OQ-1", "open_question")));

        Assert.Empty(failures);
        var forwarded = Assert.Single(hits);
        Assert.Equal(2, forwarded.MeetingDelta.Items.Count);            // Delta UNVERÄNDERT weitergereicht
        Assert.DoesNotContain("ASPECT_UNROUTED", events);               // Fragen = Querschnitt, kein Aspekt
    }

    [Fact]
    public async Task Unregistrierter_Aspekt_wird_LAUT_geparkt_und_req_laeuft_trotzdem()
    {
        // A1d: architecture ist REGISTRIERT (kein Unrouted mehr) — der unbekannte Aspekt ist jetzt z. B. "risk".
        var (hits, events, failures) = await RunAsync(Doc(
            Item("R-1", "requirement"), Item("A-1", "architecture"), Item("K-1", "risk"), Item("K-2", "risk")));

        Assert.Empty(failures);
        Assert.Single(hits);                                            // der req-Kern bekommt seinen Lauf
        Assert.Contains("ASPECT_UNROUTED", events);                     // D-8: nie wieder stilles Durchreisen
        Assert.Contains("risk", events);
        Assert.DoesNotContain("\"architecture\":", events);            // registriert => nicht geparkt
    }
}
