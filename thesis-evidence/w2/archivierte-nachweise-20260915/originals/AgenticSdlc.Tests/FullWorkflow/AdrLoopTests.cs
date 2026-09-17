using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Adr;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A5 Schicht 2 (06.08.): der ADR-Strip LLM-frei am ECHTEN Graphen (ScriptedAgent-Technik aus A2-2) —
// GateLoop-Mechanik (Repair mit wörtlichem Feedback, MaxAttempts terminal), Bridge-Skips (Smoke-Schutz),
// Apply mit Datei-Schreiben + Kangal-Save + TYP-GENAUER Passagier-Re-Emission.
public sealed class AdrLoopTests
{
    private static ProjectStateItem Design(string id) => new ProjectStateItem(
        id, "architecture", $"Entscheidung {id}", "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>())
        .WithStatus(CoreStatus.From("accepted")) with
        { Architecture = new ArchitecturePayload(["design"], "weil") };

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static AdrDraft Draft(string id) => new(id, $"Titel {id}", "Kontext.", "Entscheidung.", "Folgen.");

    private sealed class ScriptedAgent(IReadOnlyList<AITool> tools, Func<AdrDraft[]> next) : AIAgent
    {
        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
        {
            var save = tools.OfType<AIFunction>().First(f => f.Name == "save_adr_drafts");
            await save.InvokeAsync(new AIFunctionArguments { ["drafts"] = next() }, cancellationToken).ConfigureAwait(false);
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

    private sealed class RequestSink(ConcurrentBag<AdrReviewRequest> hits) : Executor<AdrReviewRequest>("FakePort")
    {
        public override ValueTask HandleAsync(AdrReviewRequest r, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(r); return ValueTask.CompletedTask; }
    }

    [Fact]
    public async Task Repair_Loop_heilt_fehlenden_Entwurf_und_Pass_traegt_Vorschau_und_Wahrheits_Katalog()
    {
        var run = new RunContext(RunId.New(), "test-adr"); run.EnsureFolders();
        var script = new Queue<AdrDraft[]>([
            [Draft("ARCH-1")],                                             // Attempt 1: ARCH-2 fehlt -> Repair
            [Draft("ARCH-1"), Draft("ARCH-2")]]);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools => new ScriptedAgent(tools, () => script.Dequeue());
        var hits = new ConcurrentBag<AdrReviewRequest>();

        var maker = new AdrMakerExecutor(factory, run);
        var b = new WorkflowBuilder(maker).WithName("AdrLoopTest");
        AdrTestGraph(b, maker, factory, run, hits);

        var core = Doc(Design("ARCH-1"), Design("ARCH-2"));
        var wfRun = await InProcessExecution.Default.RunAsync(
            b.Build(), new AdrWork(AdrProjection.PendingAdrItems(core), core, AdrLanes.Operational, 2), run.RunId, CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);

        var req = Assert.Single(hits);
        Assert.Equal(2, req.Items.Count);                                  // Pass beim 2. Versuch
        // R-56 (18.08., löst den U5-Feinschliff „prospektive Nummer" ab): die Vorschau verspricht KEINE
        // konkrete Nummer mehr (Positions-Prognose log bei Teil-Freigabe: Block-L 0018 → Datei 0001) —
        // Platzhalter im Titel + ehrliche Kopfzeile mit der echten nächsten freien Nummer aus dem Core.
        var preview = req.Items.Single(i => i.Draft.ItemId == "ARCH-1").Preview;
        Assert.StartsWith("> Vorschau auf Basis des System-Vorschlags", preview);
        Assert.Contains("fortlaufend ab ADR-0001", preview);               // echte nächste freie (Core-Quelle)
        Assert.Contains("# ADR-XXXX: Titel ARCH-1", preview);              // kein Nummern-Versprechen je Item
        Assert.Contains("Status: accepted", preview);
        Assert.Equal(2, req.Truth!.Count);                                 // Wahrheits-Katalog (ReferenceList-Futter)
        var events = await File.ReadAllTextAsync(run.EventsPath);
        Assert.Contains("ADR_REPAIR", events);
    }

    private static void AdrTestGraph(WorkflowBuilder b, AdrMakerExecutor maker,
        Func<IReadOnlyList<AITool>, AIAgent> factory, RunContext run, ConcurrentBag<AdrReviewRequest> hits)
    {
        var gate = new AdrGateExecutor(run);
        var repair = new AdrRepairExecutor(factory, run);
        var finalize = new AdrFinalizeExecutor(run, run.OutputDir("07-adr"));
        b.AddEdge(maker, gate);
        b.AddEdge<AdrVerdict>(gate, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge<AdrVerdict>(gate, finalize, m => m is not null && m.Decision != GateDecision.Repair);
        b.AddEdge(repair, gate);
        b.AddEdge(finalize, new RequestSink(hits));
        b.WithOutputFrom(finalize);
    }

    // ---- Apply: Datei + Kangal-Save + Passagier ----

    private sealed class ReportSink(ConcurrentBag<string> hits) : Executor<IngestionApplyReport>("FakeNext")
    { public override ValueTask HandleAsync(IngestionApplyReport r, IWorkflowContext c, CancellationToken ct = default) { hits.Add("report"); return ValueTask.CompletedTask; } }
    private sealed class BootSink(ConcurrentBag<string> hits) : Executor<CoreBootstrapOutput>("FakeCluster")
    { public override ValueTask HandleAsync(CoreBootstrapOutput r, IWorkflowContext c, CancellationToken ct = default) { hits.Add("boot"); return ValueTask.CompletedTask; } }

    [Fact]
    public async Task Apply_schreibt_ADR_Dateien_Index_Uebersicht_und_re_emittiert_den_Passagier()
    {
        var run = new RunContext(RunId.New(), "test-adr"); run.EnsureFolders();
        var outDir = run.OutputDir("07-adr");
        var repo = Directory.CreateTempSubdirectory("adr-repo-").FullName;
        Directory.CreateDirectory(Path.Combine(repo, "state", "core"));
        await File.WriteAllTextAsync(Path.Combine(repo, "state", "core", "project-state.json"),
            JsonSerializer.Serialize(Doc(Design("ARCH-1")), ProjectStateJson.Options));
        await File.WriteAllTextAsync(Path.Combine(outDir, "lane.json"), "{\"lane\":\"bootstrap\"}");
        await File.WriteAllTextAsync(Path.Combine(outDir, "passenger-bootstrap.json"),
            JsonSerializer.Serialize(new CoreBootstrapOutput("d", "p", 1, 0), JsonFiles.Json));

        var hits = new ConcurrentBag<string>();
        var apply = new AdrApplyExecutor(run, repo, outDir, "docs/adr");
        var b = new WorkflowBuilder(apply); b.AddEdge(apply, new BootSink(hits)); b.AddEdge(apply, new ReportSink(hits));
        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(),
            new AdrReviewResponse([Draft("ARCH-1")], "test"), RunId.New(), CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);

        Assert.Equal("boot", Assert.Single(hits));                          // TYP-GENAU
        Assert.True(File.Exists(Path.Combine(repo, "docs", "adr", "0001-titel-arch-1.md")));
        Assert.True(File.Exists(Path.Combine(repo, "docs", "adr", "README.md")));
        Assert.True(File.Exists(Path.Combine(repo, "docs", "architecture.md")));
        var saved = JsonSerializer.Deserialize<ProjectStateDocument>(
            await File.ReadAllTextAsync(Path.Combine(repo, "state", "core", "project-state.json")), ProjectStateJson.Options)!;
        Assert.Equal("ADR-0001", saved.Items.Single().Architecture!.AdrId);
    }
}
