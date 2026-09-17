using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.ArchClassify;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A2-2 (05.08.): der Klassifikations-Strip LLM-frei am ECHTEN Graphen — GateLoop-Mechanik (Repair mit
// wörtlichem Feedback, MaxAttempts terminal), Bridge-Typ-Routing beider Bahnen, Apply mit Kangal-Save und
// TYP-GENAUER Passagier-Re-Emission. ScriptedAgent = Haus-Fake (invoked die Tools direkt).
public sealed class ArchClassifyLoopTests
{
    private static ProjectStateItem Item(string id, string type = "architecture") => new ProjectStateItem(
        id, type, $"Text {id}", "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private sealed class ScriptedAgent(IReadOnlyList<AITool> tools, Func<ArchClassifyProposal[]> next) : AIAgent
    {
        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
        {
            var save = tools.OfType<AIFunction>().First(f => f.Name == "save_classification");
            await save.InvokeAsync(new AIFunctionArguments { ["proposals"] = next() }, cancellationToken).ConfigureAwait(false);
            return new AgentResponse(new ChatMessage(ChatRole.Assistant, "gespeichert"));
        }
        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        private sealed class FakeSession : AgentSession;
        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken)
            => ValueTask.FromResult<AgentSession>(new FakeSession());
        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(
            JsonElement serializedSession, JsonSerializerOptions? jsonSerializerOptions, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        protected override ValueTask<JsonElement> SerializeSessionCoreAsync(
            AgentSession session, JsonSerializerOptions? jsonSerializerOptions, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class RequestSink(ConcurrentBag<ArchClassifyReviewRequest> hits) : Executor<ArchClassifyReviewRequest>("FakePort")
    {
        public override ValueTask HandleAsync(ArchClassifyReviewRequest r, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(r); return ValueTask.CompletedTask; }
    }

    private static async Task<(ConcurrentBag<ArchClassifyReviewRequest> Requests, IReadOnlyList<string> Outputs, string Events)>
        RunLoopAsync(Queue<ArchClassifyProposal[]> script, params ProjectStateItem[] unclassified)
    {
        var run = new RunContext(RunId.New(), "test-arch-classify"); run.EnsureFolders();
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools => new ScriptedAgent(tools, () => script.Dequeue());
        var maker = new ArchClassifyMakerExecutor(factory, run);
        var gate = new ArchClassifyGateExecutor(run);
        var repair = new ArchClassifyRepairExecutor(factory, run);
        var finalize = new ArchClassifyFinalizeExecutor(run, run.OutputDir("07-arch-classify"));
        var hits = new ConcurrentBag<ArchClassifyReviewRequest>();

        var b = new WorkflowBuilder(maker).WithName("ClassifyLoopTest");
        b.AddEdge(maker, gate);
        b.AddEdge<ArchClassifyVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<ArchClassifyVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, new RequestSink(hits));
        b.WithOutputFrom(finalize);

        var wfRun = await InProcessExecution.Default.RunAsync(
            b.Build(), new ArchClassifyWork([.. unclassified], [new ArchPbiOption("PBI-1", "Titel PBI-1")], ArchClassifyLanes.Operational, 2),
            run.RunId, CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);
        var outputs = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data).OfType<string>().ToList();
        return (hits, outputs, await File.ReadAllTextAsync(run.EventsPath));
    }

    [Fact]
    public async Task Repair_Loop_heilt_fehlende_Klassifikation_und_Pass_erreicht_das_Human_Gate()
    {
        var a1 = Item("ARCH-1"); var a2 = Item("ARCH-2");
        var script = new Queue<ArchClassifyProposal[]>([
            [new("ARCH-1", [ArchRoles.Design], "nur eins")],                                        // Attempt 1: ARCH-2 fehlt -> Repair
            [new("ARCH-1", [ArchRoles.Design, ArchRoles.Constraint], "ok", ["PBI-1"]), new("ARCH-2", [ArchRoles.Work], "ok")]]);

        var (requests, outputs, events) = await RunLoopAsync(script, a1, a2);

        Assert.Empty(outputs);
        var req = Assert.Single(requests);
        Assert.Equal(2, req.Items.Count);                                  // Pass beim 2. Versuch
        // ①: die Ziel-Kette reist bis in die Review-Anfrage — Vorschlags-Ziel MIT Titel + die Options-Liste.
        var target = Assert.Single(req.Items.Single(i => i.ItemId == "ARCH-1").ProposedTargets!);
        Assert.Equal(("PBI-1", "Titel PBI-1"), (target.Id, target.Title));
        Assert.Equal("PBI-1", Assert.Single(req.ActivePbis!).Id);
        Assert.Contains("ARCH_CLASSIFY_REPAIR", events);                   // Repair feuerte mit wörtlichem Feedback
        Assert.Contains("\"attempt\":2", events.Replace(" ", ""));
    }

    [Fact]
    public async Task Wirkungsloser_Repair_endet_LAUT_terminal_nach_MaxAttempts()
    {
        var script = new Queue<ArchClassifyProposal[]>([
            Array.Empty<ArchClassifyProposal>(), Array.Empty<ArchClassifyProposal>()]);

        var (requests, outputs, _) = await RunLoopAsync(script, Item("ARCH-1"));

        Assert.Empty(requests);                                            // Human-Gate NIE erreicht
        Assert.Contains("MaxAttemptsReached", Assert.Single(outputs));
    }

    // ---- Bridges + Apply (Typ-Routing + Passagier) ----

    private static string TempRepoWithCore(ProjectStateDocument core)
    {
        var repo = Directory.CreateTempSubdirectory("classify-repo-").FullName;
        Directory.CreateDirectory(Path.Combine(repo, "state", "core"));
        File.WriteAllText(Path.Combine(repo, "state", "core", "project-state.json"),
            JsonSerializer.Serialize(core, ProjectStateJson.Options));
        return repo;
    }

    private static IngestionApplyReport Report() => new([], [], new IngestionDeltaSummary(0, 0, 0, 0, 0, 0, 0, 0));

    private sealed class ReportSink(ConcurrentBag<string> hits) : Executor<IngestionApplyReport>("FakeScan")
    { public override ValueTask HandleAsync(IngestionApplyReport r, IWorkflowContext c, CancellationToken ct = default) { hits.Add("report"); return ValueTask.CompletedTask; } }
    private sealed class BootSink(ConcurrentBag<string> hits) : Executor<CoreBootstrapOutput>("FakeCluster")
    { public override ValueTask HandleAsync(CoreBootstrapOutput r, IWorkflowContext c, CancellationToken ct = default) { hits.Add("boot"); return ValueTask.CompletedTask; } }
    private sealed class WorkSink(ConcurrentBag<string> hits) : Executor<ArchClassifyWork>("FakeMaker")
    { public override ValueTask HandleAsync(ArchClassifyWork w, IWorkflowContext c, CancellationToken ct = default) { hits.Add($"work:{w.Unclassified.Count}:{w.Lane}"); return ValueTask.CompletedTask; } }

    [Fact]
    public async Task Bridges_routen_per_Typ_Skip_vs_Arbeit_je_Bahn()
    {
        var run = new RunContext(RunId.New(), "test-arch-classify"); run.EnsureFolders();
        var hits = new ConcurrentBag<string>();
        var outDir = run.OutputDir("07-arch-classify");

        // Betrieb: KEIN arch in DIESEM Lauf (kein Delta-File) -> Report-Skip, egal wie viel Rückstau im Core
        // (Smoke-Fund 06.08.: sonst zündete jeder Leer-Lauf den LLM-Maker für den ganzen Bestand).
        var backlogRepo = TempRepoWithCore(Doc(Item("ARCH-1")));
        var op = new OperationalClassifyBridgeExecutor(run, backlogRepo, outDir, 2);
        var g1 = new WorkflowBuilder(op); g1.AddEdge(op, new ReportSink(hits)); g1.AddEdge(op, new WorkSink(hits));
        await InProcessExecution.Default.RunAsync(g1.Build(), Report(), RunId.New(), CancellationToken.None);
        Assert.Contains("report", hits);
        Assert.DoesNotContain(hits, h => h.StartsWith("work"));

        // Bootstrap: 1 unklassifiziert -> Work mit Bahn-Kennung.
        hits.Clear();
        var openRepo = TempRepoWithCore(Doc(Item("ARCH-1")));
        var boot = new BootstrapClassifyBridgeExecutor(run, openRepo, outDir, 2);
        var g2 = new WorkflowBuilder(boot); g2.AddEdge(boot, new BootSink(hits)); g2.AddEdge(boot, new WorkSink(hits));
        await InProcessExecution.Default.RunAsync(g2.Build(), new CoreBootstrapOutput("d", "p", 1, 0), RunId.New(), CancellationToken.None);
        Assert.Equal("work:1:bootstrap", Assert.Single(hits));
    }

    [Fact]
    public async Task Apply_schreibt_Payload_via_Kangal_Save_und_re_emittiert_den_Bahnen_Passagier()
    {
        var run = new RunContext(RunId.New(), "test-arch-classify"); run.EnsureFolders();
        var outDir = run.OutputDir("07-arch-classify");
        var repo = TempRepoWithCore(Doc(Item("ARCH-1")));
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), "{\"lane\":\"bootstrap\"}");
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-bootstrap.json"),
            JsonSerializer.Serialize(new CoreBootstrapOutput("d", "p", 1, 0), JsonFiles.Json));

        var hits = new ConcurrentBag<string>();
        var apply = new ArchClassifyApplyExecutor(run, repo, outDir);
        var b = new WorkflowBuilder(apply); b.AddEdge(apply, new BootSink(hits)); b.AddEdge(apply, new ReportSink(hits));
        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(),
            new ArchClassifyReviewResponse([new("ARCH-1", [ArchRoles.Design, ArchRoles.Constraint], "weil")], "test"),
            RunId.New(), CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);

        Assert.Equal("boot", Assert.Single(hits));                          // TYP-GENAU: bootstrap-Passagier
        var saved = JsonSerializer.Deserialize<ProjectStateDocument>(
            await File.ReadAllTextAsync(Path.Combine(repo, "state", "core", "project-state.json")), ProjectStateJson.Options)!;
        Assert.Equal([ArchRoles.Design, ArchRoles.Constraint], saved.Items.Single().Architecture!.Roles);
    }
}
