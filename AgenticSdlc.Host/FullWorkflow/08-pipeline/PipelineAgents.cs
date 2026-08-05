using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// Geteilte Agent-Fabrik der Pipeline-Stufen (Schritt 5 ⑤, 05.08.): Prompt + Chat-Pipeline + Tool-Logger zu
/// EINER <see cref="AIAgent"/>-Factory je Stufen-Agent. Vorher wohnte sie im CLI-Runner `pipeline-hitl`
/// (`PipelineComposedRunner`) und wurde von pipeline-full quer mitbenutzt — jetzt an der neutralen Naht.
/// </summary>
public static class PipelineAgents
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";

    public static Func<IReadOnlyList<AITool>, AIAgent> Factory(
        string repoRoot, HostSettings settings, HostSettings genSettings, RunContext run, string agentName, string promptName)
    {
        var prompt = PromptProvider.Load(repoRoot, Phase, agentName, promptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, agentName, SourceName);
        return tools => client.AsAIAgent(instructions: prompt, name: agentName, tools: [.. tools]).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
    }
}
