using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// A′ Schritt 1 — die EINE LLM-Alignment-Naht für AUTOR-ANTWORTEN, extrahiert aus dem Standalone-Sweep, damit
// Sweep (Werkbank) UND der durable Graph-Eingang (Betrieb) dieselbe Naht nutzen (Zwei-Bahnen-Regel; kein Duplikat).
// Gleicher Prompt/Agent/Tools wie R-26-C. Die deterministische Kernlogik (ClarifyEntryPlan) bleibt dadurch
// settings-/LLM-frei — nur diese Naht kennt Host/LLM.
public static class PbiAnswerAlignment
{
    /// <summary>Betriebs-Naht: gleicht die Ziel-PBIs per <c>PbiAlignmentAgent</c> an die Autor-Antworten an.</summary>
    public static AnswerAlignSeam Llm(Configuration.HostSettings settings, string repoRoot, RunContext run) =>
        async (targets, ct) =>
        {
            if (targets.Count == 0) return [];
            var prompt = Prompts.PromptProvider.Load(repoRoot, "phase2_evidence", "PbiAlignmentAgent", "PbiAlignmentAgent1",
                new Dictionary<string, string> { ["runId"] = run.RunId });
            // voll qualifiziert: der Methodenname Llm würde sonst den Namespace AgenticSdlc.Host.Llm verdecken.
            var client = Observability.AgentChatPipelineBuilder.Build(AgenticSdlc.Host.Llm.ChatClientFactory.Create(settings), settings, run, "PbiAlignmentAgent", "AgenticSdlc.Host");
            var tools = new PbiAlignTools(targets, run);
            var agent = client.AsAIAgent(instructions: prompt, name: "PbiAlignmentAgent", tools: [.. tools.Build()])
                .AsBuilder().Use(new Observability.ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            await agent.RunAsync(
                "Gleiche die PBIs an die AUTOR-ANTWORTEN an (Trigger = Antwort-Text, keine Requirement-Aenderung): "
                + "get_alignment_targets -> je PBI EIN vollstaendiger Vorschlag (Titel/Statement/Akzeptanzkriterien aus der Antwort, belegtreu). save_alignments GENAU EINMAL.")
                .ConfigureAwait(false);
            return tools.SavedAlignments ?? [];
        };
}
