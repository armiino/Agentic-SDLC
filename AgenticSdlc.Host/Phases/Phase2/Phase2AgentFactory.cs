using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2;

/// <summary>
/// Erstellt die "Specialist-Agents" (Executor im Graph) für Phase 2.1.
/// </summary>
/// <remarks>
/// Phase 2.1 untersucht mehrere "spezialisierte" Agenten in einem Workflow
/// -> DAG -> gerichteter Graph
/// Diese Factory erzeugt noch keinen Workflow. Sie stellt nur die Agenten bereit,
/// die später als MAF-Workflow-Executors verbunden werden können. Laut MAF-Doku
/// können <see cref="AIAgent"/>-Instanzen direkt an den WorkflowBuilder übergeben
/// und dadurch als Agent-Executors gebunden werden.
///
/// Alle Specialist Agents verwenden dieselbe technische Pipeline wie Phase 1:
/// Provider-Client, Function Invocation, OpenTelemetry, ChatDecisionLogger und
/// ToolCallLogger. Dadurch bleibt der Vergleich zwischen Phase 1 und Phase 2.1
/// methodisch klar ersichtlich: Geändert wird nur die Orchestrierung/Rollentrennung.
/// die grundlegende LLM- oder Logging-Infrastruktur bleibt gleich
/// </remarks>
public static class Phase2AgentFactory
{
    public const string ContextAgentName = "Phase2ContextAgent";
    public const string RequirementsAgentName = "Phase2RequirementsAgent";
    public const string RisksAgentName = "Phase2RisksAgent";
    public const string ArchitectureAgentName = "Phase2ArchitectureAgent";
    public const string OpenQuestionsAgentName = "Phase2OpenQuestionsAgent";

    /// <summary>
    /// Erstellt alle Phase-2.1-Specialist-Agents in der vorgesehenen Workflow-Reihenfolge
    /// </summary>
    public static Phase2Agents CreateSpecialists(
        HostSettings settings,
        RunContext run,
        string sourceName,
        IReadOnlyList<AITool> localTools)
    {
        var contextPath = Phase2Artifacts.ContextPath(run).Replace('\\', '/');
        var variables = new Dictionary<string, string>
        {
            ["runId"] = run.RunId,
            ["contextPath"] = contextPath
        };

        /*
         * Die Promptnamen kommen aus run-config.json. Der PromptProvider lädt
         * die jeweilige Textdatei nach einem allgemeinen Schema:
         *
         * AgenticSdlc.Host/Prompts/<phase>/<agentName>/<promptName>.txt
         *
         * Damit bleibt Phase 2.1 nicht von einer eigenen Prompt-Registry abhängig.
         * Für spätere Phasen reicht es, neue Agent-Ordner unter Prompts/<phase> anzulegen
         * und deren Promptnamen in der Run-Konfiguration zu referenzieren.
         */
        return new Phase2Agents(
            Context: CreateSpecialistAgent(
                settings,
                run,
                sourceName,
                ContextAgentName,
                LoadPrompt(settings, ContextAgentName, variables),
                localTools),
            Requirements: CreateSpecialistAgent(
                settings,
                run,
                sourceName,
                RequirementsAgentName,
                LoadPrompt(settings, RequirementsAgentName, variables),
                localTools),
            Risks: CreateSpecialistAgent(
                settings,
                run,
                sourceName,
                RisksAgentName,
                LoadPrompt(settings, RisksAgentName, variables),
                localTools),
            Architecture: CreateSpecialistAgent(
                settings,
                run,
                sourceName,
                ArchitectureAgentName,
                LoadPrompt(settings, ArchitectureAgentName, variables),
                localTools),
            OpenQuestions: CreateSpecialistAgent(
                settings,
                run,
                sourceName,
                OpenQuestionsAgentName,
                LoadPrompt(settings, OpenQuestionsAgentName, variables),
                localTools)
        );
    }

    private static string LoadPrompt(
        HostSettings settings,
        string agentName,
        IReadOnlyDictionary<string, string> variables)
    {
        /*
         * Die Factory kennt nur den Agentnamen. Welche Promptversion dieser
         * Agent im konkreten Run nutzt, kommt generisch aus HostSettings.
         * Dadurch bleibt die Factory unabhängig von phase-spezifischen Config-
         * Klassen und kann für spätere Promptversionen unverändert bleiben.
         */
        return PromptProvider.Load(
            settings.RepoRoot,
            settings.AgentPhase,
            agentName,
            settings.GetPromptName(agentName),
            variables);
    }

    private static AIAgent CreateSpecialistAgent(
        HostSettings settings,
        RunContext run,
        string sourceName,
        string agentName,
        string instructions,
        IReadOnlyList<AITool> localTools)
    {
        /*
         * Jeder Specialist bekommt einen eigenen ChatClient mit derselben Pipeline.
         * Das trennt Chat-Verlauf und Prompt pro Rolle (Agent) und macht in den Logs klarer,
         * welcher Agent welche Entscheidung/Toolnutzung erzeugt hat
         */
        IChatClient baseChatClient = ChatClientFactory.Create(settings);

        IChatClient chat = new ChatClientBuilder(baseChatClient)
            .UseFunctionInvocation()
            .UseOpenTelemetry(
                sourceName: sourceName,
                configure: cfg => cfg.EnableSensitiveData = settings.OtelSensitive
            )
            .Build();

        chat = new ChatDecisionLoggerMiddleware(chat, run);

        AIAgent baseAgent = chat.AsAIAgent(
            instructions: instructions,
            name: agentName,
            tools: [.. localTools]
        );

        var toolLogger = new ToolCallLoggerMiddleware(run);

        return baseAgent
            .AsBuilder()
            .Use(toolLogger.InvokeAsync)
            .Build();
    }
}

/// <summary>
/// Bündelt die Phase-2.1 Agenten in ihrer geplanten Workflow-Reihenfolge
/// </summary>
/// <remarks>
/// Der Typ ist bewusst klein. Er ist noch kein State-Modell und keine fachliche Projektdatenbank, 
/// sondern nur ein technischer Container für die Agenten, die der Phase-2.1-Workflow verbinden soll
/// </remarks>
public sealed record Phase2Agents(
    AIAgent Context,
    AIAgent Requirements,
    AIAgent Risks,
    AIAgent Architecture,
    AIAgent OpenQuestions);
