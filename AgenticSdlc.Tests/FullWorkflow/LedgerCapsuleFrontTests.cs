using System.Collections.Concurrent;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ② (05.08.): die neue sichtbare Ledger-Front — Intake (TranscriptInput -> string, Drift-Guard)
// -> [gebundene Kapsel] -> Summary (geteilte Auswertung). LLM-frei: die Kapsel ist hier ein Fake-Workflow
// mit identischem Vertrag (string rein, Summary-Yield raus); die ECHTE Kapsel beweist der Mini-Transkript-
// Real-Lauf. In-Process-Lauf mit lauten Fehler-Asserts (Lehre R-37: Build()-bar beweist keine Verdrahtung).
public sealed class LedgerCapsuleFrontTests
{
    [YieldsOutput(typeof(string))]
    private sealed class FakeLedgerCoreExecutor() : Executor<string>("FakeLedgerCore")
    {
        public override async ValueTask HandleAsync(string transcript, IWorkflowContext context, CancellationToken ct = default)
            => await context.YieldOutputAsync($"LEDGER-SUMMARY({transcript.Length})").ConfigureAwait(false);
    }

    [YieldsOutput(typeof(string))]
    private sealed class CollectExecutor(ConcurrentBag<string> hits) : Executor<string>("FakeCollect")
    {
        public override ValueTask HandleAsync(string s, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(s); return ValueTask.CompletedTask; }
    }

    private static Microsoft.Agents.AI.Workflows.Workflow FakeCapsuleWorkflow()
    {
        var core = new FakeLedgerCoreExecutor();
        var b = new WorkflowBuilder(core).WithName("FakeLedger");
        b.WithOutputFrom(core);
        return b.Build();
    }

    private static async Task<IReadOnlyList<string>> RunAndCollectFailures(Microsoft.Agents.AI.Workflows.Workflow wf, TranscriptInput input, string runId)
    {
        var wfRun = await InProcessExecution.Default.RunAsync(wf, input, runId, CancellationToken.None);
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
    public async Task Intake_reicht_das_Transkript_in_die_Kapsel_und_der_Yield_wird_zur_Nachricht()
    {
        var parentRun = new RunContext(RunId.New(), "test-ledger-front"); parentRun.EnsureFolders();
        var ledgerRun = new RunContext(RunId.New(), "test-ledger-front"); ledgerRun.EnsureFolders();
        var hits = new ConcurrentBag<string>();

        var intake = new LedgerIntakeExecutor(parentRun, ledgerRun, "das transkript");
        var capsule = FakeCapsuleWorkflow().BindGateFree("FakeKapsel");
        var collect = new CollectExecutor(hits);
        var b = new WorkflowBuilder(intake).WithName("LedgerFrontTest");
        b.AddEdge(intake, capsule);
        b.AddEdge(capsule, collect);

        var failures = await RunAndCollectFailures(b.Build(), new TranscriptInput("t.txt", "das transkript"), parentRun.RunId);

        Assert.Empty(failures);
        Assert.Equal($"LEDGER-SUMMARY({"das transkript".Length})", Assert.Single(hits));
        // Der Faden hat das STAGE_LEDGER_START-Event mit dem Kapsel-Sub-Run bekommen.
        var events = await File.ReadAllTextAsync(parentRun.EventsPath);
        Assert.Contains("STAGE_LEDGER_START", events);
        Assert.Contains(ledgerRun.RunId, events);
    }

    [Fact]
    public async Task Intake_Drift_Guard_scheitert_LAUT_bei_abweichendem_Transkript()
    {
        var parentRun = new RunContext(RunId.New(), "test-ledger-front"); parentRun.EnsureFolders();
        var ledgerRun = new RunContext(RunId.New(), "test-ledger-front"); ledgerRun.EnsureFolders();

        var intake = new LedgerIntakeExecutor(parentRun, ledgerRun, "konstruktions-transkript");
        var capsule = FakeCapsuleWorkflow().BindGateFree("FakeKapsel");
        var b = new WorkflowBuilder(intake).WithName("LedgerFrontDriftTest");
        b.AddEdge(intake, capsule);

        var failures = await RunAndCollectFailures(b.Build(), new TranscriptInput("t.txt", "ANDERES transkript"), parentRun.RunId);
        Assert.Contains(failures, f => f.Contains("LEDGER_TRANSCRIPT_MISMATCH"));
    }

    [Fact]
    public async Task Summary_ohne_Step_Outputs_endet_LAUT_terminal_statt_still_weiterzureichen()
    {
        // Kapsel „fertig", aber der Ledger-Lauf hat keine Step-Outputs (z. B. leerer/abgebrochener Lauf):
        // die geteilte Auswertung meldet LEDGER_UNIT_STEP_OUTPUT_MISSING und der Knoten yieldet terminal.
        var parentRun = new RunContext(RunId.New(), "test-ledger-front"); parentRun.EnsureFolders();
        var ledgerRun = new RunContext(RunId.New(), "test-ledger-front"); ledgerRun.EnsureFolders();

        var summary = new LedgerSummaryExecutor(parentRun, ledgerRun);
        var b = new WorkflowBuilder(summary).WithName("LedgerSummaryFailTest");
        b.WithOutputFrom(summary);

        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(), "kapsel-summary", parentRun.RunId, CancellationToken.None);
        var outputs = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data).OfType<string>().ToList();

        var terminal = Assert.Single(outputs);
        Assert.Contains("LEDGER_UNIT_STEP_OUTPUT_MISSING", terminal);
        Assert.Contains(ledgerRun.RunId, terminal);
        // Faden-Referenz + Abschluss-Event wurden trotzdem geschrieben (Beleg-Disziplin).
        Assert.True(File.Exists(Path.Combine(parentRun.RunDir, "01-ledger", "ledger-stage.json")));
        Assert.Contains("STAGE_LEDGER_DONE", await File.ReadAllTextAsync(parentRun.EventsPath));
    }

    [Fact]
    public void LedgerStageRun_Anker_liefert_bei_zweitem_Aufruf_denselben_Sub_Run()
    {
        // H1: run und resume bauen den Graph getrennt — der Anker (01-ledger/ledger-run.json) muss beiden
        // DENSELBEN runs/ledger-Ordner geben, sonst beginnt das Resume einen zweiten, halbleeren Lauf.
        var parentRun = new RunContext(RunId.New(), "test-ledger-front"); parentRun.EnsureFolders();

        var first = LedgerStageRun.GetOrCreate(parentRun);
        var second = LedgerStageRun.GetOrCreate(parentRun);

        Assert.Equal(first.RunId, second.RunId);
        Assert.Equal(first.RunDir, second.RunDir);
    }
}
