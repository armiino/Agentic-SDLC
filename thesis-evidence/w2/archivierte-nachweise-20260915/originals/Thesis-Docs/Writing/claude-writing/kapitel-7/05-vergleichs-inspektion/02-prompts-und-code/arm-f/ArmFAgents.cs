using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.ArmF;

/// <summary>
/// Agent-Fabrik des Arm F — Klon der geteilten <c>PipelineAgents.Factory</c> (die ist auf die
/// Prompt-Phase <c>phase2_evidence</c> festgelegt; Arm F hat als Mess-Arm seine eigene Phase
/// <c>armf</c> und bleibt außerhalb des gemessenen Systems). Gleiche Komposition: Prompt +
/// W1a-Denk-Faden + Chat-Pipeline (OTel, Logging) + Tool-Logger. Zusätzlich zählt ein
/// Runden-Zähler am Basis-Client die Modellrunden für das Ebene-C-Prozessprofil (§14.5).
/// </summary>
public static class ArmFAgents
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "armf";
    public const string AgentName = "ArmFAgent";
    // F-v2 "missionsgleich" (Konzept §11, 05.09.): kanonischer Bestand + Quellenbilanz.
    // v1 (reine Extraktion) bleibt als Datei erhalten = eingefrorenes Pilot-Treatment.
    public const string PromptName = "ArmFAgent2";
    public const string Variant = "F_mission_v2";

    public static (AIAgent Agent, ModelRoundCounter Rounds) Build(
        string repoRoot, HostSettings settings, HostSettings genSettings, RunContext run, IReadOnlyList<AITool> tools)
    {
        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, PromptName, new Dictionary<string, string> { ["runId"] = run.RunId })
                     + ReasoningSchema.ToolAgentPromptAppendix(settings.ReasoningCapture);
        var rounds = new ModelRoundCounter(ChatClientFactory.Create(genSettings));
        var client = AgentChatPipelineBuilder.Build(rounds, settings, run, AgentName, SourceName);
        var agent = client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
            .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        return (agent, rounds);
    }
}

/// <summary>
/// Zählt Modellrunden am BASIS-Client (jede <c>GetResponseAsync</c>-Fahrt der Function-Invocation-
/// Schleife = eine Runde) — deterministisch und unabhängig vom InnerCycleLogging-Modus.
/// Reine Beobachtung, verändert nichts am Verkehr (§14.5: Prozessprofil rein beobachtbar).
/// </summary>
public sealed class ModelRoundCounter(IChatClient inner) : IChatClient
{
    public int Rounds { get; private set; }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Rounds++;
        return inner.GetResponseAsync(messages, options, cancellationToken);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Rounds++;
        return inner.GetStreamingResponseAsync(messages, options, cancellationToken);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => inner.GetService(serviceType, serviceKey);
    public void Dispose() => inner.Dispose();
}
