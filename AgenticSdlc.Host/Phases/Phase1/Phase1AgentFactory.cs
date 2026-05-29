using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase1;

/// <summary>
/// Erstellt den lauffähigen "Phase-1-Agenten" für den aktuellen Single-Agent-Run.
/// SingleAgentRun weil zu Beginn nur ein Agent mit einem Prompt verwendet wird
/// </summary>
/// <remarks>
/// Diese "Factory" bündelt die Erzeugung eines komplexen Objekts an einer Stelle
/// Der Agent besteht hier nicht nur aus einem Prompt, sondern aus LLM-Client, Microsoft.Extensions.AI-Pipeline,
/// Function Invocation, OpenTelemetry, "Decision-Logging", Prompt und Tool-Logging.
/// In der Program.cs muss dadurch nur noch entschieden werden, dass ein "Phase-1-Agent" gestartet
/// werden soll. Wie dieser Agent intern zusammengesetzt wird, liegt in dieser Klasse.
/// Das ist wichtig für spätere Phasen/Erkentnisse, weil weitere Agenten dann mit
/// derselben Struktur ergänzt werden können, ohne das die Program.cs größer wird
/// </remarks>
public static class Phase1AgentFactory
{
    private const string AgentName = "Phase1SinglePass"; //Name: weil oneshot ein agent 1 phase..

    /// <summary>
    /// Baut den aktuell verwendeten Phase-1-Single-Pass-Agenten.
    /// </summary>
    /// <param name="settings">Technische Host-Konfiguration, zB LLM-Provider, Modell und OTel-Flags.</param>
    /// <param name="run">Run-Kontext für Logs, Decision-Logs und Events.</param>
    /// <param name="sourceName">OpenTelemetry-Source-Name fürr die Chat-Pipeline.</param>
    /// <param name="runId">Eindeutige Run-ID, die in den Prompt und die Logs einfliesst.</param>
    /// <param name="localTools">Vom lokalen MCP-Server geladene Tools, die der Agent verwenden darf.</param>
    public static AIAgent CreateSinglePassAgent(
        HostSettings settings,
        RunContext run,
        string sourceName,
        string runId,
        IReadOnlyList<AITool> localTools)
    {
        /*
         * Der Basis-ChatClient enthölt nur die Provider-Verbindung.
         * Ob dahinter Ollama oder iwas OpenRouter steckt, entscheidet ChatClientFactory
         * anhand der Settings. Die Agentenlogik bleibt davon getrennt.
         */
        IChatClient baseChatClient = ChatClientFactory.Create(settings);

        /*
         * Die Chat-Pipeline erweitert den Basis-Client um Function-Invocation.
         * Dadurch werden Tool Calls des Modells nicht nur als Text ausgegeben,
         * sondern vom Framework erkannt, ausgeführt und wieder in den Chatverlauf gegeben
         * ..
         * UseOpenTelemetry hängt zusätzlich technische Traces an die Chat-
         * Kommunikation. "SensitiveData" wird über die Settings gesteuert, damit
         * Run-Konfiguration und Logging-Verhalten weiterhin aus der .env kommen
         */
        IChatClient chat = new ChatClientBuilder(baseChatClient)
            .UseFunctionInvocation()
            .UseOpenTelemetry(
                sourceName: sourceName,
                configure: cfg => cfg.EnableSensitiveData = settings.OtelSensitive
            )
            .Build();

        /*
         * Diese Middleware ist projektspezifisch. Sie protokolliert Chat-
         * Iterationen und erkannte Tool-Call-Anfragen in die Run-Logs. Sie wird
         * nach der Standard-Pipeline hinzugefügt, damit sie den fertigen ChatClient beobachtet.
         */
        chat = new ChatDecisionLoggerMiddleware(chat, run);

        /*
         * Der Prompt gehört fachlich zur Phase 1, wird aber nicht mehr als
         * C#-Klasse fest verdrahtet. Der allgemeine PromptProvider lädt die
         * Textdatei nach dem Schema:
         * AgenticSdlc.Host/Prompts/<phase>/<agentName>/<promptName>.txt
         * Dadurch kann ein neuer Phase-1-Prompt später als Textdatei ergänzt
         * und über run-config.json ausgewählt werden, ohne diese Factory zu ändern.
         */
        var instructions = PromptProvider.Load(
            settings.RepoRoot,
            settings.AgentPhase,
            AgentName,
            settings.Phase1Prompt,
            new Dictionary<string, string>
            {
                ["runId"] = runId
            });

        /*
         * Aus dem ChatClient wird ein "MAF"-Agent. Erst hier werden Name, Prompt
         * und die lokal entdeckten Tools verbunden.
         */
        AIAgent baseAgent = chat.AsAIAgent(
            instructions: instructions,
            name: AgentName,
            tools: [.. localTools]
        );

        /*
         * ToolCallLoggerMiddleware beobachtet die tatsächliche Tool-Ausführung.
         * Das ist eine andere Ebene als ChatDecisionLoggerMiddleware:
         * - ChatDecisionLoggerMiddleware sieht die Chat-/Modellantwort.
         * - ToolCallLoggerMiddleware sieht den konkreten Tool-Aufruf inklusive Ergebnis
         */
        var toolLogger = new ToolCallLoggerMiddleware(run);

        return baseAgent
            .AsBuilder()
            .Use(toolLogger.InvokeAsync)
            .Build();
    }
}
