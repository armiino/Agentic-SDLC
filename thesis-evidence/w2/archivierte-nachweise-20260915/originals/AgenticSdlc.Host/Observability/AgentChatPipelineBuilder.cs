using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Observability;

/// <summary>
/// Baut die gemeinsame Observability-Chat-Pipeline für einen Agenten
/// </summary>
/// <remarks>
/// Phasenunabhängig: Phase 1, Phase 2.1 und spätere Phasen (falls welche kommen) nutzen die Agenten dieselbe Methode.
/// Dadurch erbt jede neue Phase automatisch das korrekte Logging-Verhalten und die richtige Middleware-Reihenfolge,
/// (vorher war sie in jeder AgentFactory kopiert)
/// Zwei Modi, gesteuert über run-config.json -> observability.innerCycleLogging:
///
/// Blob-Modus (false) — eine ChatDecisionLogger-Instanz auf Agent-Chat-Ebene:
///   AgentChat -> InputContext -> FunctionInvocation -> OTel -> LLM
///   Ergebnis: CHAT_* 1x, response-text.md als ein Blob über alle inneren Runden.
///
/// Per-cycle-Modus (true) zusätzlich eine innere Instanz auf Modell-Runden-Ebene:
///   AgentChat -> InputContext -> FunctionInvocation ->ModelRound -> OTel ->LLM
///   Ergebnis: CHAT_* 1x (Pipeline-Marker) + MODEL_ROUND_* Nx (pro LLM-Roundtrip),
///   response-text.md mit einem Abschnitt pro Modell-Runde.
///
/// Reihenfolge-Regel (Microsoft.Extensions.AI): zuerst hinzugefügt = am weitesten außen.
/// InputContextLogger bleibt in beiden Modi außerhalb von FunctionInvocation,
/// -> damit er den vollständigen Upstream-Message-Kontext aller vorherigen Agenten sieht.
/// Details: observability-pipeline.md.
/// </remarks>
public static class AgentChatPipelineBuilder
{
    public static IChatClient Build(
        IChatClient baseChatClient,
        HostSettings settings,
        RunContext run,
        string agentName,
        string sourceName)
    {
        if (settings.InnerCycleLogging)
        {
            // ChatDecisionLogger(ModelRound) sitzt INNERHALB FunctionInvocation und sieht
            // jeden LLM-Roundtrip einzeln. Er erfasst auch den Reasoning-Text pro "Runde"
            var chat = new ChatClientBuilder(baseChatClient)
                .UseFunctionInvocation()
                .Use(inner => new ChatDecisionLoggerMiddleware(
                    inner, run, agentName,
                    scope: LoggingScope.ModelRound,
                    writeResponseText: true,
                    previewChars: settings.LlmPreviewChars,
                    responseTextMaxChars: settings.ResponseTextMaxChars))
                .UseOpenTelemetry(
                    sourceName: sourceName,
                    configure: cfg => cfg.EnableSensitiveData = settings.OtelSensitive)
                .Build();

            chat = new InputContextLoggerMiddleware(chat, run, agentName);

            // AgentChat-Instanz markiert nur Pipeline-Start/-Ende (1x). writeResponseText:false,
            // damit der Text nicht doppelt erfasst wird (die ModelRound-Instanz macht das schon).
            chat = new ChatDecisionLoggerMiddleware(
                chat, run, agentName,
                scope: LoggingScope.AgentChat,
                writeResponseText: false,
                previewChars: settings.LlmPreviewChars);

            return chat;
        }
        else
        {
            // Blob-Modus: eine einzige ChatDecisionLogger-Instanz auf Agent-Chat-Ebene.
            // Sie erfasst den gesamten Agenten-Chat als einen angesammelnten Block
            var chat = new ChatClientBuilder(baseChatClient)
                .UseFunctionInvocation()
                .UseOpenTelemetry(
                    sourceName: sourceName,
                    configure: cfg => cfg.EnableSensitiveData = settings.OtelSensitive)
                .Build();

            chat = new InputContextLoggerMiddleware(chat, run, agentName);

            chat = new ChatDecisionLoggerMiddleware(
                chat, run, agentName,
                scope: LoggingScope.AgentChat,
                writeResponseText: true,
                previewChars: settings.LlmPreviewChars,
                responseTextMaxChars: settings.ResponseTextMaxChars);

            return chat;
        }
    }
}
