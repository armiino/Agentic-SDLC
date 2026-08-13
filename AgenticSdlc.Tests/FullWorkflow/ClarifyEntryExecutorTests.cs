using System.Collections.Concurrent;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// A′ Schritt 2 (steward/graph-entry-vs-werkbank.md §10/§11) — der clarify-GRAPH-Eingang, In-Process mit Fake-Alignment.
// Bewiesen: bei einem validen Sweep injiziert der Eingang einen PbiUpdateVerdict mit ECHTEM Checker-Befund an den
// Folge-Knoten (pbiFinalize-Position), und der PbiUpdateWfContext ist sauber befüllt (Applied leer = clarify hat keine
// Ingestion-Ops; OutDir/SourceIngestionRun/DryRun echt — downstream leitet daraus Artefakte/Summary/Forward ab).
// (Der Non-Pass-Zweig ist defense-in-depth: der Builder garantiert valide MARK_CHANGED-Ziele — daher hier über normale
// Eingaben nicht erreichbar; die Non-Pass-ERKENNUNG + Verdict-Treue sind in ClarifyEntryPlanTests bewiesen.)
public sealed class ClarifyEntryExecutorTests
{
    private sealed class VerdictCapture(ConcurrentBag<PbiUpdateVerdict> got) : Executor<PbiUpdateVerdict>("Capture")
    {
        public override ValueTask HandleAsync(PbiUpdateVerdict v, IWorkflowContext context, CancellationToken ct = default)
        { got.Add(v); return ValueTask.CompletedTask; }
    }

    private static ProjectStateItem Feature(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1, "run", null, null, null, null, [], [],
        new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1, "run", null, null, null, null, [], [],
        new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateItem ClarifyPbi(string id, string title) => new ProjectStateItem(
        id, "pbi", title, "test", null, 1, "run", null, null, null, null, [], [],
        new Dictionary<string, string>(),
        Pbi: new PbiPayload(null, title, [], ["REQ-1"], [], null, null, null, null))
        .WithStatus(CoreStatus.From("needs_clarify"));

    private static ProjectStateDocument Core() => new("p", 4, DateTime.UnixEpoch, [],
        [Feature("FC-01", "Feature"), ClarifyPbi("PBI-01", "Rechtemodell"), Req("REQ-1", "Anforderung")],
        [new("PBI-01", "REQ-1", "covers", "test", new Dictionary<string, string>()),
         new("PBI-01", "FC-01", "part_of_feature", "test", new Dictionary<string, string>())],
        [], []);

    [Fact]
    public async Task Valider_Sweep_injiziert_Verdict_mit_echtem_Checker_und_sauberem_Context()
    {
        var repoRoot = Directory.CreateTempSubdirectory("clarify-entry-").FullName;
        await new JsonCoreRepository(repoRoot).SaveAsync(Core());
        var pbiOutDir = Path.Combine(repoRoot, "07-pbi-update");
        var run = new RunContext(RunId.New(), "clarify-entry-test");
        run.EnsureFolders();

        var fake = new PbiAlignment("PBI-01", "Titel", "Statement", ["AK"], "r", ["chat:steward#1"]);
        AnswerAlignSeam seam = (t, ct) => Task.FromResult<IReadOnlyList<PbiAlignment>>([fake]);

        var got = new ConcurrentBag<PbiUpdateVerdict>();
        var entry = new ClarifyEntryExecutor(run, repoRoot, pbiOutDir, seam);
        var b = new WorkflowBuilder(entry).WithName("ClarifyEntryTest");
        b.AddEdge(entry, new VerdictCapture(got));
        var wf = b.Build();

        var wfRun = await InProcessExecution.Default.RunAsync(wf,
            new ClarifySweepInput([new ClarifySweepAnswer("PBI-01", "Es gibt drei Rollen …")]), run.RunId, CancellationToken.None);
        var failures = wfRun.OutgoingEvents.Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent).ToList();
        Assert.Empty(failures);

        var v = Assert.Single(got);
        Assert.True(v.Report.Pass);                                    // ECHTER Checker (nicht hardcodiert)
        Assert.Equal("MARK_CHANGED", Assert.Single(v.Plan.Operations).Kind);
        Assert.Equal("PBI-01", v.Plan.Operations[0].PbiId);
        Assert.NotNull(v.Plan.Alignments);                             // Alignment über die injizierte Naht
        // ctx ECHT befüllt (downstream vertraut darauf):
        Assert.Empty(v.Ctx.Applied);                                   // clarify = keine Ingestion-Ops
        Assert.Equal(pbiOutDir, v.Ctx.OutDir);
        Assert.Equal(run.RunId, v.Ctx.SourceIngestionRun);
        Assert.False(v.Ctx.DryRun);
    }

    [Fact]
    public async Task Keine_gueltigen_Antworten_stoppt_TERMINAL_ohne_Inject_und_ohne_LLM()
    {
        var repoRoot = Directory.CreateTempSubdirectory("clarify-entry-noops-").FullName;
        await new JsonCoreRepository(repoRoot).SaveAsync(Core());
        var pbiOutDir = Path.Combine(repoRoot, "07-pbi-update");
        var run = new RunContext(RunId.New(), "clarify-entry-noops");
        run.EnsureFolders();

        var called = false;
        AnswerAlignSeam seam = (t, ct) => { called = true; return Task.FromResult<IReadOnlyList<PbiAlignment>>([]); };

        var got = new ConcurrentBag<PbiUpdateVerdict>();
        var entry = new ClarifyEntryExecutor(run, repoRoot, pbiOutDir, seam);
        var b = new WorkflowBuilder(entry).WithName("ClarifyEntryNoOpsTest");
        b.AddEdge(entry, new VerdictCapture(got));
        b.WithOutputFrom(entry);
        var wf = b.Build();

        // Antwort für ein PBI, das es NICHT gibt → Builder überspringt → leerer Plan (bestünde den Checker als 0-Ops-Pass!).
        var wfRun = await InProcessExecution.Default.RunAsync(wf,
            new ClarifySweepInput([new ClarifySweepAnswer("PBI-UNBEKANNT", "egal")]), run.RunId, CancellationToken.None);

        Assert.Empty(wfRun.OutgoingEvents.Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent));
        Assert.Empty(got);                                             // KEIN Inject in den PBI-Schwanz (nicht leer durchgereicht)
        Assert.False(called);                                          // kein LLM-Schritt bei leerem Plan
        var outputs = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(o => o.Data as string ?? "").ToList();
        Assert.Contains(outputs, o => o.Contains("keine gültigen Antworten"));   // terminal + sichtbar
    }

    [Fact]
    public void ResolvePbiGateDecisions_fuehrt_akzeptierte_Angleichungen_mit_R26C_Fix()
    {
        // Regression zum 11.08.-E2E-Fund: der pipeline-full-pbi-gate-Resume ließ akzeptierte Angleichungen fallen
        // (alignments=0 am Apply) → needs_clarify fiel im Graphen nie. Der Fix führt sie über die geteilte Naht mit.
        var plan = new PbiStateChangePlanDocument(1, "p", DateTime.UnixEpoch, "run",
            [new PbiStateChangeOperation("MARK_CHANGED", "", "PBI-01", null, null, null, "r",
                AuthorAnswerRef: "chat#1", AuthorAnswerText: "A")],
            [new PbiAlignment("PBI-01", "Neuer Titel", "Statement", ["AK1", "AK2"], "r", ["chat#1"])]);
        var df = new PbiUpdateDecisionsFile("run", "human (review-ui)",
            [new PbiUpdateDecision("op-0", "apply", null, null)],
            [new PbiAlignmentDecision("PBI-01", "accept", null, null, null, null)]);

        var (ops, aligns, reviewer) = PipelineFullRunner.ResolvePbiGateDecisions(plan, df);

        Assert.Equal(["op-0"], ops);
        Assert.NotNull(aligns);
        Assert.Equal("Neuer Titel", Assert.Single(aligns!).ProposedTitle);   // MITGEFÜHRT (war der Bug)
        Assert.Equal("human (review-ui)", reviewer);
    }

    [Fact]
    public void ReadClarifyInput_liest_die_Antworten_Datei_duenn_zum_Eingangs_Vertrag()
    {
        var dir = Directory.CreateTempSubdirectory("clarify-read-").FullName;
        var path = Path.Combine(dir, "sweep-answers.json");
        File.WriteAllText(path, """[{"pbiId":"PBI-01","antwort":"A"},{"pbiId":"PBI-02","antwort":"B"}]""");

        var input = PipelineFullRunner.ReadClarifyInput(path);

        Assert.Equal(2, input.Answers.Count);
        Assert.Equal("PBI-01", input.Answers[0].PbiId);
        Assert.Equal("A", input.Answers[0].Antwort);
    }
}
