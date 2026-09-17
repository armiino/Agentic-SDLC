using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// SPIKE-BEFUND (05.08., Kapsel-Endbild-Frage des Autors) — KORRIGIERT nach Doku-/Quellen-Check
// (Autor-Einwand „100% sicher, dass es nicht an unserem Code liegt?"): Reicht MAF einen RequestPort aus
// einem per BindAsExecutor gebundenen Sub-Workflow zum ELTERN-Lauf durch?
//
// ANTWORT (MAF 1.15.0, zweiteilig):
// (a) NICHT AUTOMATISCH — die Kapsel emittiert den inneren Request als ExternalRequest-NACHRICHT in den
//     Eltern-Graph; OHNE passende Kante wird sie LAUTLOS gedroppt (unmatched-message-Semantik) und der
//     innere Port wartet ewig: stiller Deadlock (Befund_-Test, unsere ursprüngliche Falle).
// (b) MIT expliziter Verdrahtung FUNKTIONIERT es — offizielles Muster (Repo-Sample
//     09_Subworkflow_ExternalRequest + Learn „Sub-Workflows/Requests and Responses"):
//     ForwardMessage<ExternalRequest>(kapsel -> eltern-port) + ForwardMessage<ExternalResponse>(port ->
//     kapsel); der ELTERN-Port surfaced als normales RequestInfoEvent (Aufloesung_-Test). HITL in
//     Kapseln ist also MÖGLICH — eine bewusste Verdrahtungs-Entscheidung, kein Framework-Verbot.
//
// Konsequenz (Feature-Matrix §N, aufgefallen 9j): flach bleibt unsere BEWIESENE Form; das Kapsel-Endbild
// (Gate-Stufen als Kapseln) ist MÖGLICH und als Design-Entscheid geparkt. GateFreeBinding schützt weiter
// vor der stillen Falle (a): ein Port in einer Kapsel ohne bewusste Forward-Verdrahtung = Konstruktions-Fehler.
public sealed class SubworkflowPortSpikeTests
{
    public sealed record SpikeGateRequest(string Question);
    public sealed record SpikeGateResponse(string Answer);

    [SendsMessage(typeof(SpikeGateRequest))]
    private sealed class InnerAskExecutor() : Executor<string>("SpikeInnerAsk")
    {
        public override async ValueTask HandleAsync(string input, IWorkflowContext context, CancellationToken ct = default)
            => await context.SendMessageAsync(new SpikeGateRequest($"Frage zu '{input}'")).ConfigureAwait(false);
    }

    [YieldsOutput(typeof(string))]
    private sealed class InnerFinishExecutor() : Executor<SpikeGateResponse>("SpikeInnerFinish")
    {
        public override async ValueTask HandleAsync(SpikeGateResponse resp, IWorkflowContext context, CancellationToken ct = default)
            => await context.YieldOutputAsync($"INNER:{resp.Answer}").ConfigureAwait(false);
    }

    [YieldsOutput(typeof(string))]
    private sealed class ParentCollectExecutor() : Executor<string>("SpikeParentCollect")
    {
        public override async ValueTask HandleAsync(string innerOutput, IWorkflowContext context, CancellationToken ct = default)
            => await context.YieldOutputAsync($"PARENT:{innerOutput}").ConfigureAwait(false);
    }

    // Innerer Workflow = eine „Gate-Stufe" in Miniatur: Ask ─► [spike-inner-gate] ─► Finish (yield).
    private static Microsoft.Agents.AI.Workflows.Workflow BuildInner()
    {
        var ask = new InnerAskExecutor();
        var port = RequestPort.Create<SpikeGateRequest, SpikeGateResponse>("spike-inner-gate");
        var finish = new InnerFinishExecutor();
        var b = new WorkflowBuilder(ask).WithName("SpikeInner");
        b.AddEdge(ask, port);
        b.AddEdge(port, finish);
        b.WithOutputFrom(finish);
        return b.Build();
    }

    // Eltern-Graph: [SpikeInner].BindAsExecutor ─► Collect (der Yield des Sub-Workflows wird zur Nachricht).
    private static Microsoft.Agents.AI.Workflows.Workflow BuildParent()
    {
        var capsule = BuildInner().BindAsExecutor("SpikeCapsule");
        var collect = new ParentCollectExecutor();
        var b = new WorkflowBuilder(capsule).WithName("SpikeParent");
        b.AddEdge(capsule, collect);
        b.WithOutputFrom(collect);
        return b.Build();
    }

    // Watch-Protokoll: was kam oben an, wie endete der Lauf. Timeout-Wächter macht „hängt" zu einem
    // lauten, diagnostizierbaren Ergebnis statt eines ewigen Tests (der Naiv-Spike hing endlos).
    private sealed class WatchLog
    {
        public List<string> Events { get; } = [];
        public string? SeenPort;
        public string? SeenQuestion;
        public string? Output;
        public List<string> Failures { get; } = [];
        public bool TimedOut;
        public string Dump() => $"TimedOut={TimedOut} Port={SeenPort ?? "-"} Output={Output ?? "-"} " +
            $"Failures=[{string.Join("; ", Failures)}] Events=[{string.Join(" → ", Events)}]";
    }

    private static async Task<WatchLog> WatchAsync(StreamingRun handle, string responseAnswer, TimeSpan timeout)
    {
        var log = new WatchLog();
        var watch = Task.Run(async () =>
        {
            await foreach (var evt in handle.WatchStreamAsync().ConfigureAwait(false))
            {
                log.Events.Add(evt.GetType().Name);
                switch (evt)
                {
                    case RequestInfoEvent req:
                        log.SeenPort = req.Request.PortInfo.PortId;
                        if (req.Request.TryGetDataAs<SpikeGateRequest>(out var q) && q is not null) log.SeenQuestion = q.Question;
                        await handle.SendResponseAsync(req.Request.CreateResponse(new SpikeGateResponse(responseAnswer))).ConfigureAwait(false);
                        break;
                    case WorkflowOutputEvent { Data: string s }: log.Output = s; break;
                    case ExecutorFailedEvent f: log.Failures.Add($"{f.ExecutorId}: {f.Data?.Message}"); break;
                    case WorkflowErrorEvent w: log.Failures.Add(w.Exception?.Message ?? "WorkflowError"); break;
                }
            }
        });
        var finished = await Task.WhenAny(watch, Task.Delay(timeout)).ConfigureAwait(false);
        if (finished != watch) log.TimedOut = true; else await watch.ConfigureAwait(false);
        return log;
    }

    [Fact]
    public async Task Kontrolle_Port_top_level_funktioniert_im_Test_Harness()
    {
        // Derselbe innere Workflow, DIREKT gefahren — beweist die Port-Mechanik des Harness, damit der
        // Pinning-Test die Bindung als EINZIGE Variable isoliert.
        await using var handle = await InProcessExecution.RunStreamingAsync(BuildInner(), "kapsel-test");
        var log = await WatchAsync(handle, "JA", TimeSpan.FromSeconds(20));

        Assert.False(log.TimedOut, $"Kontroll-Lauf hing: {log.Dump()}");
        Assert.Empty(log.Failures);
        Assert.Equal("spike-inner-gate", log.SeenPort);
        Assert.Equal("Frage zu 'kapsel-test'", log.SeenQuestion);
        Assert.Equal("INNER:JA", log.Output);
    }

    [Fact]
    public async Task Befund_UNVERDRAHTETER_Port_in_Kapsel_wird_still_gedroppt_Deadlock()
    {
        // PINNING der Falle (1.15.0): die Kapsel SENDET den inneren Request als ExternalRequest-NACHRICHT
        // in den Eltern-Graph — ohne passende Kante wird sie LAUTLOS gedroppt (unmatched-message-Semantik)
        // und der innere Port wartet ewig. Kein Fehler, kein Event: der Deadlock ist unsichtbar.
        // Genau diese Falle macht BindGateFree zum Konstruktions-Fehler. (Der VERDRAHTETE Weg funktioniert —
        // siehe Aufloesung_-Test darunter, offizielles Muster Sample 09_Subworkflow_ExternalRequest.)
        await using var handle = await InProcessExecution.RunStreamingAsync(BuildParent(), "kapsel-test");
        var log = await WatchAsync(handle, "JA", TimeSpan.FromSeconds(5));

        Assert.Null(log.SeenPort);                       // kein RequestInfoEvent im Eltern-Stream
        Assert.Empty(log.Failures);                      // und KEIN Fehler-Event — das Verschwinden ist still
        Assert.Null(log.Output);                         // der Lauf kommt nie zum Output
        Assert.True(log.TimedOut, $"Verhalten geändert (Ports surfacen jetzt AUTOMATISCH?) — Wächter+9j neu bewerten: {log.Dump()}");
    }

    [Fact]
    public async Task Aufloesung_VERDRAHTETER_Kapsel_Port_erreicht_den_Eltern_Lauf_und_die_Antwort_fliesst_zurueck()
    {
        // Das OFFIZIELLE Muster (MAF-Sample 09_Subworkflow_ExternalRequest): die Kapsel emittiert
        // ExternalRequest als Nachricht -> explizite ForwardMessage-Kanten zu einem ELTERN-Port
        // (AllowWrapped default true) -> RequestInfoEvent surfaced -> Antwort als ExternalResponse
        // zurück in die Kapsel. HITL in Kapseln IST also möglich — nur nicht automatisch.
        var capsule = BuildInner().BindAsExecutor("SpikeCapsule"); // bewusst OHNE GateFree-Wächter: Port wird hier VERDRAHTET
        var parentPort = RequestPort.Create<SpikeGateRequest, SpikeGateResponse>("spike-parent-gate");
        var collect = new ParentCollectExecutor();
        var b = new WorkflowBuilder(capsule).WithName("SpikeParentWired");
        b.ForwardMessage<ExternalRequest>(capsule, [parentPort]);   // Kapsel-Anfrage -> Eltern-Port
        b.ForwardMessage<ExternalResponse>(parentPort, [capsule]);  // Antwort -> zurück in die Kapsel
        b.AddEdge(capsule, collect);
        b.WithOutputFrom(collect);

        await using var handle = await InProcessExecution.RunStreamingAsync(b.Build(), "kapsel-test");
        var log = await WatchAsync(handle, "JA", TimeSpan.FromSeconds(20));

        Assert.False(log.TimedOut, $"Verdrahtete Kapsel hing: {log.Dump()}");
        Assert.Empty(log.Failures);
        Assert.Equal("spike-parent-gate", log.SeenPort);             // die Tür kam OBEN an — als der ELTERN-Port (er re-raist die gewrappte Anfrage)
        Assert.Equal("Frage zu 'kapsel-test'", log.SeenQuestion);    // mit typisiertem Payload
        Assert.Equal("PARENT:INNER:JA", log.Output);                 // Antwort floss ZURÜCK durch die Kapselgrenze
    }
}
