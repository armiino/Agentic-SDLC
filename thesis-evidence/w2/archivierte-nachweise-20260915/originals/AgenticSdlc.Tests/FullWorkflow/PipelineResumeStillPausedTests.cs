using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-52 (17.08., Block-E-Live-Fund, 2× Hänger): ein `pipeline-full resume` an einem Gate OHNE Entscheid-Quelle
// wartete EWIG — die Pause-Materialisierung wartet auf einen NEUEN Checkpoint, beim Resume schreitet aber ohne
// Antwort kein Superstep voran. Wächter: der Resume TERMINIERT (Exit 6), die BESTEHENDE Pause bleibt identisch
// gültig (gleicher Checkpoint im pointer), und der Fall ist LAUT als PIPELINE_STILL_PAUSED unterscheidbar.
// (Standalone-Bahn HitlShell.ResumeAsync ist bewusst NICHT betroffen: dort ist die Antwort Pflicht-Parameter.)
public sealed class PipelineResumeStillPausedTests
{
    [SendsMessage(typeof(PbiUpdateReviewRequest))]
    private sealed class GateOpener(string runId) : Executor<string>("GateOpener")
    {
        public override async ValueTask HandleAsync(string _, IWorkflowContext context, CancellationToken ct = default)
            => await context.SendMessageAsync(new PbiUpdateReviewRequest(runId, "r52-test", [])).ConfigureAwait(false);
    }

    private static Workflow BuildGateWorkflow(string runId)
    {
        var opener = new GateOpener(runId);
        var port = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-gate");
        var b = new WorkflowBuilder(opener).WithName("R52Gate");
        b.AddEdge(opener, port);
        return b.Build();
    }

    [Fact]
    public async Task Resume_ohne_Antwort_TERMINIERT_und_erhaelt_die_Pause_unveraendert()
    {
        var run = new RunContext(RunId.New(), "r52"); run.EnsureFolders();
        var fw = FullWorkflowSettings.FromConfig(null);                        // Default = interactive, keine Antwortquellen
        var checkpointDir = run.OutputDir("checkpoints");

        // 1) Frischer Lauf: pausiert sauber (Exit 6) und materialisiert pointer.json mit Checkpoint.
        int exit1;
        using (var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir)))
        {
            var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
            exit1 = await PipelineFullRunner.RunWorkflowStreamingAsync(
                BuildGateWorkflow(run.RunId), "los", run.RunId, run, fw, manager, CancellationToken.None);
        }
        Assert.Equal(6, exit1);
        var p1 = await HitlShell.LoadPointerAsync("r52-test", checkpointDir);
        Assert.NotNull(p1);

        // 2) Resume OHNE Entscheid-Datei/Flags: MUSS terminieren (vorher: Endlos-Hänger; der cts ist der
        //    Regressions-Wächter) — und die Pause bleibt auf DEMSELBEN Checkpoint bestehen.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        int exit2;
        using (var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir)))
        {
            var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
            exit2 = await PipelineFullRunner.RunWorkflowStreamingAsync<string>(
                BuildGateWorkflow(run.RunId), default!, run.RunId, run, fw, manager, cts.Token,
                restoreFrom: new CheckpointInfo(p1!.SessionId, p1.CheckpointId));
        }
        Assert.Equal(6, exit2);
        Assert.False(cts.IsCancellationRequested);                             // terminiert, nicht per Timeout gestorben

        var p2 = await HitlShell.LoadPointerAsync("r52-test", checkpointDir);
        Assert.Equal(p1.CheckpointId, p2!.CheckpointId);                       // Pause UNVERÄNDERT (kein neuer Checkpoint)
        Assert.Equal(p1.SessionId, p2.SessionId);

        var events = File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl"));
        Assert.Contains("PIPELINE_STILL_PAUSED", events);                      // LAUT unterscheidbar, nie still
        Assert.Contains("PIPELINE_PAUSED", events);                            // die Erst-Pause bleibt die normale Form
    }

    // ---- R-55 (18.08., Block-L-Live-Fund): das decision-gate vertagte beim Resume STUMM alles selbst
    // (das „answers is null → warten"-Recht war tot: der Resume baut IMMER ein Antwort-Objekt) — der Autor
    // bediente die UI, seine KEEP/ADOPT_NEW-Entscheide blieben unangewendet. Wächter: Defer-all ist jetzt an
    // das EXPLIZITE --accept-all gebunden; ohne Flag re-pausiert der Resume (R-52-Mechanik) statt zu vertagen.
    [SendsMessage(typeof(PipelineDecisionReviewRequest))]
    private sealed class DecisionOpener(string runId) : Executor<string>("DecisionOpener")
    {
        public override async ValueTask HandleAsync(string _, IWorkflowContext context, CancellationToken ct = default)
            => await context.SendMessageAsync(new PipelineDecisionReviewRequest(runId,
                [new PipelineDecisionItemView("DEC-T1", "Widerspruch T1", "REQ-1", "alt", [], "neu")])).ConfigureAwait(false);
    }

    private sealed class DecisionSink : Executor<PipelineDecisionReviewResponse>
    {
        public PipelineDecisionReviewResponse? Seen;
        public DecisionSink() : base("DecisionSink") { }
        public override ValueTask HandleAsync(PipelineDecisionReviewResponse r, IWorkflowContext c, CancellationToken ct = default)
        { Seen = r; return ValueTask.CompletedTask; }
    }

    private static Workflow BuildDecisionWorkflow(string runId, DecisionSink sink)
    {
        var opener = new DecisionOpener(runId);
        var port = RequestPort.Create<PipelineDecisionReviewRequest, PipelineDecisionReviewResponse>("decision-gate");
        var b = new WorkflowBuilder(opener).WithName("R55Gate");
        b.AddEdge(opener, port);
        b.AddEdge(port, sink);
        return b.Build();
    }

    [Fact]
    public async Task R55_decision_gate_vertagt_NUR_bei_explizitem_accept_all_sonst_re_pausiert_der_Resume()
    {
        var run = new RunContext(RunId.New(), "r55"); run.EnsureFolders();
        var fw = FullWorkflowSettings.FromConfig(null);
        var checkpointDir = run.OutputDir("checkpoints");
        var sink = new DecisionSink();

        // 1) Frischer Lauf pausiert unverändert am decision-gate (kein Auto-Defer beim Erst-Halt).
        int exit1;
        using (var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir)))
        {
            var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
            exit1 = await PipelineFullRunner.RunWorkflowStreamingAsync(
                BuildDecisionWorkflow(run.RunId, sink), "los", run.RunId, run, fw, manager, CancellationToken.None);
        }
        Assert.Equal(6, exit1);
        var p1 = await HitlShell.LoadPointerAsync("r55-test", checkpointDir);
        Assert.NotNull(p1);

        // 2) Resume OHNE Flags/Datei: früher stummes Defer-all — jetzt bleibt das Gate unbeantwortet und
        //    re-pausiert sauber (R-52); KEIN GATE_ANSWERED, der Autor-Entscheid bleibt möglich.
        int exit2;
        using (var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir)))
        {
            var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
            exit2 = await PipelineFullRunner.RunWorkflowStreamingAsync<string>(
                BuildDecisionWorkflow(run.RunId, sink), default!, run.RunId, run, fw, manager, CancellationToken.None,
                restoreFrom: new CheckpointInfo(p1!.SessionId, p1.CheckpointId),
                answers: new PipelineFullRunner.ResumeAnswers(false, []));
        }
        Assert.Equal(6, exit2);
        Assert.Null(sink.Seen);                                                // nichts beantwortet, nichts vertagt
        var events = File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl"));
        Assert.Contains("PIPELINE_STILL_PAUSED", events);
        Assert.DoesNotContain("GATE_ANSWERED", events);

        // 3) Resume MIT explizitem --accept-all: Defer-all als DEKLARIERTER Experiment-Akt, laut im Event.
        int exit3;
        using (var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir)))
        {
            var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
            exit3 = await PipelineFullRunner.RunWorkflowStreamingAsync<string>(
                BuildDecisionWorkflow(run.RunId, sink), default!, run.RunId, run, fw, manager, CancellationToken.None,
                restoreFrom: new CheckpointInfo(p1.SessionId, p1.CheckpointId),
                answers: new PipelineFullRunner.ResumeAnswers(true, []));
        }
        Assert.NotEqual(6, exit3);                                             // keine Pause mehr — Gate beantwortet
        Assert.NotNull(sink.Seen);
        Assert.All(sink.Seen!.Resolutions, r => Assert.Equal("defer", r.Action, ignoreCase: true));
        events = File.ReadAllText(Path.Combine(run.LogsDir, "events.jsonl"));
        Assert.Contains("accept-all-flag", events);                            // Herkunft des Vertagens ist LAUT gestempelt
    }
}
