using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-33 S2: der ECHTE Cluster-Graph (agent -> gate --[Repair]--> repair -> gate / sonst review -> finalize)
// laeuft in-process mit skriptierten Agenten (LLM-frei). Bewiesen wird die LOOP-MECHANIK + die Review-Regel:
// der fachliche Kritiker laeuft NUR bei bestandenem Gate (sonst ehrlicher skipped-Report, kein Agent-Call).
public sealed class ReClarifyClusterLoopTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    // Skriptierter Maker/Repair-Agent: ruft direkt save_clusters mit dem naechsten Drehbuch-Payload auf.
    private sealed class ScriptedAgent(IReadOnlyList<AITool> tools, Func<FeatureCluster[]> nextClusters) : AIAgent
    {
        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
        {
            var save = tools.OfType<AIFunction>().First(f => f.Name == "save_clusters");
            await save.InvokeAsync(new AIFunctionArguments { ["clusters"] = nextClusters() }, cancellationToken).ConfigureAwait(false);
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

    // Stummer Review-Agent: speichert nichts -> Executor faellt auf den ehrlichen revise-Default zurueck.
    private sealed class SilentAgent : AIAgent
    {
        protected override Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
            => Task.FromResult(new AgentResponse(new ChatMessage(ChatRole.Assistant, "ok")));

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

    private static ReClarifyClusterInput Input(int maxAttempts) => new(
        new CanonicalRequirementsBaseline("b1", "p1", 1, T, "state.json", [Req("CAN-REQ-1"), Req("CAN-REQ-2")], [], []),
        "baseline.json", maxAttempts);

    private static FeatureCluster[] Incomplete() => [new("FC-001", "key-1", "Feature 1", ["CAN-REQ-1"], [], null)];   // CAN-REQ-2 fehlt -> UNPLACED_REQUIREMENT
    private static FeatureCluster[] Complete() => [new("FC-001", "key-1", "Feature 1", ["CAN-REQ-1", "CAN-REQ-2"], [], null)];

    private static async Task<(JsonElement Summary, int ReviewCalls)> RunGraphAsync(Func<int, FeatureCluster[]> script, int maxAttempts)
    {
        var run = new RunContext(RunId.New(), "test-reclarify-cluster-loop");
        run.EnsureFolders();
        var outDir = Path.Combine(run.RunDir, "clusters");

        var makerCall = 0;
        Func<IReadOnlyList<AITool>, AIAgent> makerFactory = tools => new ScriptedAgent(tools, () => script(Interlocked.Increment(ref makerCall)));
        var reviewCalls = 0;
        Func<IReadOnlyList<AITool>, AIAgent> reviewFactory = _ => { Interlocked.Increment(ref reviewCalls); return new SilentAgent(); };

        var workflow = ReClarifyClusterWorkflow.Build(
            new ClusterAgentExecutor(makerFactory, Path.GetTempPath(), run),
            new ClusterGateExecutor(run),
            new ClusterRepairExecutor(makerFactory, Path.GetTempPath(), run),
            new ClusterReviewExecutor(reviewFactory, run),
            new ClusterFinalizeExecutor(run, outDir));

        await InProcessExecution.Default.RunAsync(workflow, Input(maxAttempts), run.RunId, CancellationToken.None);

        var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "cluster-summary.json"))).RootElement;
        return (summary, reviewCalls);
    }

    [Fact]
    public async Task Rotes_Gate_repariert_sich_Review_laeuft_erst_nach_Pass()
    {
        var (summary, reviewCalls) = await RunGraphAsync(call => call == 1 ? Incomplete() : Complete(), maxAttempts: 2);

        Assert.True(summary.GetProperty("gatePass").GetBoolean());
        Assert.Equal("Pass", summary.GetProperty("finalDecision").GetString());
        Assert.Equal(2, summary.GetProperty("attempts").GetInt32());
        Assert.Equal(1, reviewCalls);   // der Kritiker lief GENAU EINMAL — erst nach dem Pass, nicht auf dem kaputten Set
        Assert.Equal("revise", summary.GetProperty("reviewVerdict").GetString());   // SilentAgent -> ehrlicher Default
    }

    [Fact]
    public async Task MaxAttempts_Review_wird_uebersprungen_ohne_Agent_Call()
    {
        var (summary, reviewCalls) = await RunGraphAsync(_ => Incomplete(), maxAttempts: 2);

        Assert.False(summary.GetProperty("gatePass").GetBoolean());
        Assert.Equal("MaxAttemptsReached", summary.GetProperty("finalDecision").GetString());
        Assert.Equal(0, reviewCalls);   // KEIN LLM-Call auf strukturell kaputtem Input
        Assert.Equal("skipped", summary.GetProperty("reviewVerdict").GetString());
    }

    [Fact]
    public void RepairTask_enthaelt_die_Gate_Fehler_woertlich()
    {
        var report = ReClarifyClusterGate.Check(
            new CanonicalRequirementsBaseline("b1", "p1", 1, T, "state.json", [Req("CAN-REQ-1"), Req("CAN-REQ-2")], [], []),
            Incomplete());

        var task = ClusterRepairExecutor.BuildRepairTask(report);
        Assert.Contains("UNPLACED_REQUIREMENT", task);
        Assert.Contains("CAN-REQ-2", task);
        Assert.Contains("save_clusters", task);
    }
}
