using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-33 S1: der ECHTE Backlog-Graph (agent -> gate --[Repair]--> repair -> gate / sonst finalize) laeuft
// in-process mit einem skriptierten Agenten (LLM-frei; er ruft direkt save_pbis auf). Bewiesen wird die
// LOOP-MECHANIK: rotes Gate -> Repair-Knoten -> zweiter Versuch -> Pass; und die MaxAttempts-Schranke.
public sealed class ReClarifyBacklogLoopTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    // Skriptierter Agent: statt LLM ruft er das save_pbis-Tool mit dem naechsten Drehbuch-Payload auf.
    private sealed class ScriptedAgent(IReadOnlyList<AITool> tools, Func<ProductBacklogItem[]> nextItems) : AIAgent
    {
        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
        {
            var save = tools.OfType<AIFunction>().First(f => f.Name == "save_pbis");
            await save.InvokeAsync(new AIFunctionArguments { ["items"] = nextItems() }, cancellationToken).ConfigureAwait(false);
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

    private static CanonicalRequirement Req(string id)
        => new(id, $"Titel {id}", $"Text {id}", "accepted", [], "origin", 1, new Dictionary<string, string>());

    private static ReClarifyBacklogInput Input(int maxAttempts) => new(
        new FeatureClusterSet(1, "cs-1", "p1", "b1", T, "baseline.json",
            [new FeatureCluster("FC-001", "key-1", "Feature 1", ["CAN-REQ-1", "CAN-REQ-2"], [], null)]),
        new CanonicalRequirementsBaseline("b1", "p1", 1, T, "state.json", [Req("CAN-REQ-1"), Req("CAN-REQ-2")], [], []),
        "clusters.json", maxAttempts);

    private static ProductBacklogItem Pbi(string id, params string[] reqIds)
        => new ProductBacklogItem(PbiId: id, IdentityKey: $"key-{id}", Version: 1, Type: "delivery",
               Title: $"Titel {id}", RequirementIds: reqIds) with { AcceptanceCriteria = ["AK 1"] };

    private static ProductBacklogItem[] Incomplete() => [Pbi("PBI-1", "CAN-REQ-1")];                       // CAN-REQ-2 fehlt -> UNCOVERED_CORE
    private static ProductBacklogItem[] Complete() => [Pbi("PBI-1", "CAN-REQ-1"), Pbi("PBI-2", "CAN-REQ-2")];

    private static async Task<(string OutDir, JsonElement Summary)> RunGraphAsync(Func<int, ProductBacklogItem[]> script, int maxAttempts)
    {
        var run = new RunContext(RunId.New(), "test-reclarify-loop");
        run.EnsureFolders();
        var outDir = Path.Combine(run.RunDir, "backlog");

        var call = 0;
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools => new ScriptedAgent(tools, () => script(Interlocked.Increment(ref call)));

        var workflow = ReClarifyBacklogWorkflow.Build(
            new ClarifyAgentExecutor(factory, run),
            new BacklogGateExecutor(run),
            new BacklogRepairExecutor(factory, run),
            new BacklogFinalizeExecutor(run, outDir));

        await InProcessExecution.Default.RunAsync(workflow, Input(maxAttempts), run.RunId, CancellationToken.None);

        var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "backlog-summary.json"))).RootElement;
        return (outDir, summary);
    }

    [Fact]
    public async Task Rotes_Gate_repariert_sich_und_endet_Pass_in_zwei_Attempts()
    {
        // Drehbuch: Versuch 1 (Maker) unvollstaendig -> Gate Repair -> Versuch 2 (Repair) vollstaendig -> Pass.
        var (outDir, summary) = await RunGraphAsync(call => call == 1 ? Incomplete() : Complete(), maxAttempts: 2);

        Assert.True(summary.GetProperty("gatePass").GetBoolean());
        Assert.Equal("Pass", summary.GetProperty("finalDecision").GetString());
        Assert.Equal(2, summary.GetProperty("attempts").GetInt32());
        Assert.Equal(2, summary.GetProperty("pbis").GetInt32());

        // Attempt-Historie: maker (Repair) -> repair (Pass) — der W2-Messpunkt existiert.
        var attempts = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "backlog-attempts.json"))).RootElement;
        Assert.Equal(2, attempts.GetArrayLength());
        Assert.Equal("maker", attempts[0].GetProperty("source").GetString());
        Assert.Equal("Repair", attempts[0].GetProperty("decision").GetString());
        Assert.Equal("repair", attempts[1].GetProperty("source").GetString());
        Assert.Equal("Pass", attempts[1].GetProperty("decision").GetString());
    }

    [Fact]
    public async Task MaxAttempts_Schranke_stoppt_den_Loop_deterministisch()
    {
        // Drehbuch: immer unvollstaendig -> Attempt 2 erreicht die Schranke -> MaxAttemptsReached, kein Endlos-Loop.
        var (_, summary) = await RunGraphAsync(_ => Incomplete(), maxAttempts: 2);

        Assert.False(summary.GetProperty("gatePass").GetBoolean());
        Assert.Equal("MaxAttemptsReached", summary.GetProperty("finalDecision").GetString());
        Assert.Equal(2, summary.GetProperty("attempts").GetInt32());
    }

    [Fact]
    public void RepairTask_enthaelt_die_Gate_Fehler_woertlich()
    {
        var report = ReClarifyBacklogGate.Check(
            new FeatureClusterSet(1, "cs-1", "p1", "b1", T, "baseline.json",
                [new FeatureCluster("FC-001", "key-1", "Feature 1", ["CAN-REQ-1", "CAN-REQ-2"], [], null)]),
            new CanonicalRequirementsBaseline("b1", "p1", 1, T, "state.json", [Req("CAN-REQ-1"), Req("CAN-REQ-2")], [], []),
            new ProductBacklogDocument(1, "bl-1", "p1", "b1", T, "clusters.json", [Pbi("PBI-1", "CAN-REQ-1")]));

        var task = BacklogRepairExecutor.BuildRepairTask(report);
        Assert.Contains("UNCOVERED_CORE", task);
        Assert.Contains("CAN-REQ-2", task);   // der konkrete Fehlertext erreicht den Agenten woertlich
        Assert.Contains("save_pbis", task);
    }
}
