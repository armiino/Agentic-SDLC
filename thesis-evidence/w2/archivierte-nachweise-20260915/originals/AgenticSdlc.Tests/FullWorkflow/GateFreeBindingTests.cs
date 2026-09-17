using AgenticSdlc.Host.FullWorkflow;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ② (05.08.): der LAUTE Bind-Wächter — Konsequenz aus dem Spike-Befund (Ports in Kapseln
// deadlocken still, SubworkflowPortSpikeTests). BindGateFree ersetzt BindAsExecutor an allen Bind-Stellen;
// diese Tests beweisen: Port-haltig => Konstruktions-Fehler (auch VERSCHACHTELT), Port-frei => bindbar.
public sealed class GateFreeBindingTests
{
    public sealed record Ask(string Q);
    public sealed record Answer(string A);

    [SendsMessage(typeof(Ask))]
    private sealed class AskExecutor() : Executor<string>("GuardAsk")
    {
        public override async ValueTask HandleAsync(string s, IWorkflowContext context, CancellationToken ct = default)
            => await context.SendMessageAsync(new Ask(s)).ConfigureAwait(false);
    }

    [YieldsOutput(typeof(string))]
    private sealed class EchoExecutor(string id) : Executor<string>(id)
    {
        public override async ValueTask HandleAsync(string s, IWorkflowContext context, CancellationToken ct = default)
            => await context.YieldOutputAsync($"echo:{s}").ConfigureAwait(false);
    }

    [YieldsOutput(typeof(string))]
    private sealed class AnswerSinkExecutor() : Executor<Answer>("GuardSink")
    {
        public override async ValueTask HandleAsync(Answer a, IWorkflowContext context, CancellationToken ct = default)
            => await context.YieldOutputAsync(a.A).ConfigureAwait(false);
    }

    private static Microsoft.Agents.AI.Workflows.Workflow PortBearing()
    {
        var ask = new AskExecutor();
        var port = RequestPort.Create<Ask, Answer>("guard-gate");
        var sink = new AnswerSinkExecutor();
        var b = new WorkflowBuilder(ask).WithName("GuardPortWf");
        b.AddEdge(ask, port);
        b.AddEdge(port, sink);
        b.WithOutputFrom(sink);
        return b.Build();
    }

    private static Microsoft.Agents.AI.Workflows.Workflow PortFree()
    {
        var echo = new EchoExecutor("GuardEcho");
        var b = new WorkflowBuilder(echo).WithName("GuardCleanWf");
        b.WithOutputFrom(echo);
        return b.Build();
    }

    [Fact]
    public void Port_haltiger_Workflow_wird_LAUT_abgewiesen()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => PortBearing().BindGateFree("Kapsel"));
        Assert.Contains("PORT_IN_CAPSULE", ex.Message);
        Assert.Contains("guard-gate", ex.Message);          // die Diagnose nennt den konkreten Port
    }

    [Fact]
    public void Verschachtelter_Port_wird_ebenfalls_erkannt()
    {
        // Kapsel-in-Kapsel: der Port liegt eine Ebene tiefer — der Wächter muss rekursiv finden.
        var inner = PortBearing().BindAsExecutor("InnereKapsel"); // bewusst UNgeprüft gebunden (Testaufbau)
        var outerB = new WorkflowBuilder(inner).WithName("GuardNestedWf");
        var outer = outerB.Build();

        var ex = Assert.Throws<InvalidOperationException>(() => outer.BindGateFree("AeussereKapsel"));
        Assert.Contains("guard-gate", ex.Message);
    }

    [Fact]
    public async Task Port_freier_Workflow_ist_bindbar_und_laeuft()
    {
        var capsule = PortFree().BindGateFree("SaubereKapsel");
        var collect = new EchoExecutor("GuardCollect");
        var b = new WorkflowBuilder(capsule).WithName("GuardParent");
        b.AddEdge(capsule, collect);
        b.WithOutputFrom(collect);

        var run = await InProcessExecution.Default.RunAsync(b.Build(), "hallo", "guard-clean-run", CancellationToken.None);
        var outputs = run.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data).OfType<string>().ToList();
        Assert.Contains("echo:echo:hallo", outputs);        // Kapsel-Yield wurde zur Nachricht an den Collector
    }
}
