using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Observability;

/// <summary>
/// Position dieser Middleware in der Chat-Pipeline bestimmt folgendes:
/// WELCHE Ebene beobachtet wird und unter welchen Event-Namen das geloggt wird
/// </summary>
/// <remarks>
/// Die Unterscheidung ist nötig, weil dieselbe Middleware-Klasse je nach Pipeline-Position
/// semantisch unterschiedliche Dinge sieht:
/// - <see cref="AgentChat"/>: außerhalb von FunctionInvocation. Sieht EINEN Aufruf pro
///   Agent-Chat (die ganze Pipeline). Markiert Start/Ende des Agenten-Chats.
/// - <see cref="ModelRound"/>: innerhalb von FunctionInvocation. Sieht JEDEN einzelnen
///   LLM-Roundtrip (fs_list-Runde, fs_read-Runde, fs_write-Runde, Abschluss-Runde).
/// Siehe observability-pipeline.md.
/// </remarks>
public enum LoggingScope
{
    /// <summary>Äußere Ebene: ein Marker pro Agent-Chat (Pipeline-Start/-Ende).</summary>
    AgentChat,

    /// <summary>Innere Ebene: ein Marker pro LLM-Roundtrip innerhalb des FunctionInvocation-Loops.</summary>
    ModelRound
}

/// <summary>
/// Beobachtet das Verhalten des Modells. Je nach <see cref="LoggingScope"/> auf
/// Agent-Chat-Ebene (Pipeline-Marker) oder Modell-Runden-Ebene (pro LLM-Roundtrip).
/// </summary>
/// <remarks>
/// Verantwortlichkeit dieser Klasse:
/// - Scope AgentChat: CHAT_STARTED / CHAT_FINISHED loggen (Pipeline-Marker, 1x pro Agent-Chat)
/// - Scope ModelRound: MODEL_ROUND_STARTED / MODEL_ROUND_FINISHED loggen (1x pro LLM-Roundtrip)
/// - CHAT_FAILED loggen (Timing, Fehlerdiagnose)
/// - Wenn writeResponseText: *_RESPONSE_TEXT / MODEL_ROUND_TEXT loggen + response-text.md schreiben
///
/// Warum zwei Scopes: Eine feste Benennung kann nicht für beide Pipeline-Positionen korrekt sein. 
/// Außen ist ein Aufruf "der ganze Agent-Chat", innen ist ein Aufruf "eine Modell-Runde".
/// Die Verdrahtung beider Instanzen passiert generisch im AgentChatPipelineBuilder.
///
/// Klare Grenze:
/// Diese Middleware beweist nicht warum das Modell so gehandelt hat.
/// Sie erfasst ausschließlich beobachtbare Ausgaben.
/// Die verlässliche Quelle für tatsächlich ausgeführte Tools ist ToolCallLoggerMiddleware.
///
/// Wichtig für MAF-Agents: beide Pfade müssen überschrieben sein.
/// MAF nutzt GetStreamingResponseAsync.. ohne dieses Override wäre die gesamte Chat-Observability wirkungslos.
/// </remarks>
public sealed class ChatDecisionLoggerMiddleware : DelegatingChatClient
{
    private readonly RunContext _run;
    private readonly string? _agentName;
    private readonly LoggingScope _scope;

    /*
     * Ob diese Instanz den Reasoning-Text erfasst (Event mit Text + response-text.md).
     * Im per-cycle-Modus erfasst nur die innere ModelRound-Instanz Text..
     * die äußere AgentChat-Instanz markiert dann nur Start/Ende, um Text-Doppelung zu vermeiden.
     * Im Blob-Modus erfasst die einzige (AgentChat-)Instanz den Text als Blob.
     */
    private readonly bool _writeResponseText;

    /*
     * Maximale Zeichenzahl für den geloggten Reasoning-Text.
     * Schützt vor riesigen Transkript-Echos in den Logs.
     * Kommt aus HostSettings (konfigurierbar über: run-config.json -> llmPreview.chars)
     */
    private readonly int _previewChars;

    /*
     * Scope AgentChat: zählt Agent-Chat-Aufrufe (in Phase 2.1 genau 1; bei Repair-Loops >1).
     * Scope ModelRound: zählt LLM-Roundtrips innerhalb des FunctionInvocation-Loops (1..N).
     */
    private int _round;

    public ChatDecisionLoggerMiddleware(
        IChatClient innerClient,
        RunContext run,
        string? agentName = null,
        LoggingScope scope = LoggingScope.AgentChat,
        bool writeResponseText = true,
        int previewChars = 1200)
        : base(innerClient)
    {
        _run = run;
        _agentName = string.IsNullOrWhiteSpace(agentName) ? null : agentName;
        _scope = scope;
        _writeResponseText = writeResponseText;
        _previewChars = previewChars;
    }

    // Event-Namen je Scope - eine Stelle, damit die Benennung konsistent bleibt.
    private string StartedEventType => _scope == LoggingScope.ModelRound ? "MODEL_ROUND_STARTED" : "CHAT_STARTED";
    private string FinishedEventType => _scope == LoggingScope.ModelRound ? "MODEL_ROUND_FINISHED" : "CHAT_FINISHED";
    private string TextEventType => _scope == LoggingScope.ModelRound ? "MODEL_ROUND_TEXT" : "CHAT_RESPONSE_TEXT";

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var round = Interlocked.Increment(ref _round);
        SetOtelTags(round);
        LogChatStarted(options, round);

        ChatResponse response;
        try
        {
            response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogChatFailed(options, ex);
            throw;
        }

        var toolCallNames = ExtractToolCallNames(response);
        var assistantText = response.Messages?.LastOrDefault()?.Text ?? string.Empty;
        LogObservations(round, options?.ModelId, assistantText, toolCallNames, response.FinishReason?.ToString());

        return response;
    }

    /// <summary>
    /// Überwacht den Streaming-Pfad, den MAF-Agents standardmäßig verwenden.
    /// </summary>
    /// <remarks>
    /// yield return ist in C# in try/finally erlaubt, aber nicht in try/catch.
    /// completedNormally unterscheidet daher normalen Abschluss von Abbruch.
    /// </remarks>
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var round = Interlocked.Increment(ref _round);
        SetOtelTags(round);
        LogChatStarted(options, round);

        var textBuilder = new StringBuilder();
        var toolCallNames = new List<string>();
        string? finishReason = null;
        var completedNormally = false;

        try
        {
            await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
            {
                // Text-Chunks über alle inneren Runden akkumulieren.
                if (!string.IsNullOrEmpty(update.Text))
                    textBuilder.Append(update.Text);

                // FinishReason erscheint typischerweise nur im letzten Update.
                if (update.FinishReason.HasValue)
                    finishReason = update.FinishReason.Value.ToString();

                AccumulateToolCallNamesFromUpdate(update, toolCallNames);

                yield return update;
            }

            completedNormally = true;
        }
        finally
        {
            if (!completedNormally)
            {
                /*
                 * Stream wurde unterbrochen zb Provider-Fehler oder CancellationToken.
                 * Exception kann im finally nicht mehr geworfen werden..
                 * der fehlende CHAT_FINISHED macht den Abbruch in den Logs sichtbar.
                 */
                _run.AppendEvent(new
                {
                    type = "CHAT_FAILED",
                    agentName = _agentName,
                    scope = _scope.ToString(),
                    model = options?.ModelId,
                    errorType = "streaming_interrupted",
                    error = "Streaming response interrupted before completion.",
                    round,
                    timestampUtc = DateTime.UtcNow
                });
            }
            else
            {
                LogObservations(round, options?.ModelId, textBuilder.ToString(), toolCallNames, finishReason);
            }
        }
    }

    //hilfsmethodn

    private void SetOtelTags(int round)
    {
        // Tag-Name je Scope: model_round innen, agent_chat außen.
        Activity.Current?.SetTag(_scope == LoggingScope.ModelRound ? "agent.model_round" : "agent.chat_round", round);
        Activity.Current?.SetTag("agent.step.kind", _scope == LoggingScope.ModelRound ? "model_round" : "agent_chat");
        Activity.Current?.SetTag("agent.name", _agentName);
    }

    private void LogChatStarted(ChatOptions? options, int round)
    {
        var evt = new
        {
            type = StartedEventType,
            agentName = _agentName,
            scope = _scope.ToString(),
            round,
            model = options?.ModelId,
            temperature = options?.Temperature,
            maxOutputTokens = options?.MaxOutputTokens,
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendEvent(evt);
        if (_agentName is not null)
            _run.AppendAgentEvent(_agentName, evt);   // Konsistenz: auch im Agent-Spiegel (vorher fehlte das)
    }

    private void LogChatFailed(ChatOptions? options, Exception ex)
    {
        /*
         * CHAT_FAILED: Die Chat-Pipeline selbst ist fehlgeschlagen
         * Nicht zu verwechseln mit Tool-Fehlern, die ToolCallLoggerMiddleware behandelt
         */
        var evt = new
        {
            type = "CHAT_FAILED",
            agentName = _agentName,
            scope = _scope.ToString(),
            model = options?.ModelId,
            errorType = ex.GetType().FullName,
            error = ex.Message,
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendEvent(evt);
        if (_agentName is not null)
            _run.AppendAgentEvent(_agentName, evt);
        Activity.Current?.AddException(ex);
    }

    /// <summary>
    /// Kernlogik für Chat-Beobachtungen: geteilt zwischen non-streaming und streaming
    /// </summary>
    /// <remarks>
    /// ACHTUNG: assistantText ist beobachtbarer Model-Output, kein Beweis für interne Kausalität. 
    /// Das Modell kann plausiblen Text schreiben ohne die genannten Quellen tatsächlich verarbeitet zu haben..
    /// </remarks>
    private void LogObservations(
        int round,
        string? modelId,
        string assistantText,
        List<string> toolCallNames,
        string? finishReason)
    {
        var distinctToolCallNames = toolCallNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var observedResponseType = DetermineObservedResponseType(toolCallNames, assistantText);
        var observedToolGroups = DetermineObservedToolGroups(toolCallNames);

        var hasText = assistantText.Length > 0;
        var hasToolCalls = toolCallNames.Count > 0;

        // Text-Event (CHAT_RESPONSE_TEXT bzw. MODEL_ROUND_TEXT):
        // nur wenn diese Instanz für Text zuständig ist (_writeResponseText) und Text vorhanden ist.
        // Im per-cycle-Modus erfasst nur die innere ModelRound-Instanz Text..
        // die äußere AgentChat-Instanz markiert nur Start/Ende, um Doppelerfassung zu vermeiden.
        if (_writeResponseText && hasText)
        {
            var sha = Sha256Hex(assistantText);
            var truncatedEvt = assistantText.Length > _previewChars;
            var contentEvt = Truncate(assistantText.Trim(), _previewChars);

            var responseTextEvent = new
            {
                type = TextEventType,
                agentName = _agentName,
                scope = _scope.ToString(),
                round,
                // Kontext: neben welchen Tool-Calls entstand dieser Text?
                toolCallsInThisResponse = distinctToolCallNames,
                hasToolCalls,
                textLength = assistantText.Length,
                // SHA erlaubt Vergleich zwischen Runs ohne den vollen Text zu laden.
                textSha256 = sha,
                text = contentEvt,
                truncated = truncatedEvt,
                timestampUtc = DateTime.UtcNow
            };

            _run.AppendEvent(responseTextEvent);
            if (_agentName is not null)
                _run.AppendAgentEvent(_agentName, responseTextEvent);
        }

        // response-text.md-Abschnitt: für JEDE beobachtbare Runde (Text ODER Tool-Calls), damit auch
        // Tool-only-Runden (zB stille fs_write ohne Reasoning-Text) sichtbar bleiben. Die Lücke
        // wurde in Run 20260613_200805_b4fb45 (Phase 2.1B) sichtbar: der RisksAgent schrieb docs/risks.md
        // in zwei fs_write-Runden OHNE Assistant-Text -> beide fehlten in response-text.md, nur die finale
        // Status-Runde war zu sehen. Damit war der zweite (kaputte) Write im lesbaren Log unsichtbar.
        // Das "Warum" jedes Writes bleibt zusätzlich in tool-calls.jsonl (reason/evidence der fs_write-Args).
        if (_writeResponseText && _agentName is not null && (hasText || hasToolCalls))
        {
            var truncated = assistantText.Length > _previewChars;
            var content = Truncate(assistantText.Trim(), _previewChars);
            _run.AppendAgentResponseText(
                _agentName,
                BuildResponseTextMarkdown(round, distinctToolCallNames, content, assistantText.Length, truncated));
        }

        var finishedEvent = new
        {
            type = FinishedEventType,
            agentName = _agentName,
            scope = _scope.ToString(),
            round,
            model = modelId,
            finishReason,
            toolCallsCount = toolCallNames.Count,
            toolCallNames = distinctToolCallNames,
            assistantChars = assistantText.Length,
            // Diese Felder beschreiben die beobachtbare Antwortform, keine Modell-Absichten.
            observedResponseType,
            observedToolGroups,
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendEvent(finishedEvent);
        if (_agentName is not null)
            _run.AppendAgentEvent(_agentName, finishedEvent);   // Konsistenz: auch im Agent-Spiegel

        // Kompakte OTel-Metadaten: bewusst klein, damit Traces auswertbar bleiben.
        AddOtelEvent("llm.tool_decision", new Dictionary<string, object?>
        {
            ["agent.name"] = _agentName,
            ["agent.scope"] = _scope.ToString(),
            ["agent.round"] = round,
            ["llm.toolcalls.count"] = toolCallNames.Count,
            ["llm.toolcalls.names"] = string.Join(",", distinctToolCallNames),
            ["llm.response.observed_type"] = observedResponseType,
            ["llm.toolcalls.observed_groups"] = string.Join(",", observedToolGroups)
        });
    }

    //Toolcall name extrahieren

    private static List<string> ExtractToolCallNames(ChatResponse response)
    {
        var names = new List<string>();
        if (response.Messages is null) return names;

        foreach (var msg in response.Messages)
        {
            var contentsProp = msg.GetType().GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance);
            if (contentsProp?.GetValue(msg) is System.Collections.IEnumerable contents)
                ExtractNamesFromContents(contents, names);
        }

        return names;
    }

    private static void AccumulateToolCallNamesFromUpdate(ChatResponseUpdate update, List<string> names)
    {
        var contentsProp = update.GetType().GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance);
        if (contentsProp?.GetValue(update) is System.Collections.IEnumerable contents)
            ExtractNamesFromContents(contents, names);
    }

    /// <summary>
    /// Gemeinsame Reflection-Logik für GetResponseAsync und GetStreamingResponseAsync.
    /// </summary>
    /// <remarks>
    /// Tool-Call-Typen werden per Reflection erkannt,
    /// weil Microsoft.Extensions.AI den konkreten AIContent-Typ zwischen Versionen ändern kann.
    /// Diese Beobachtung ist sekundär..ToolCallLoggerMiddleware ist die verlässliche Quelle.
    /// </remarks>
    private static void ExtractNamesFromContents(System.Collections.IEnumerable contents, List<string> names)
    {
        foreach (var item in contents)
        {
            if (item is null) continue;
            var t = item.GetType();

            // AIContent-Typen für Tool-Calls enthalten "Call" im Typnamen.
            if (!t.Name.Contains("Call", StringComparison.OrdinalIgnoreCase))
                continue;

            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            var n = nameProp?.GetValue(item)?.ToString();
            names.Add(!string.IsNullOrWhiteSpace(n) ? n! : "<unknown_call>");
        }
    }

    private static string DetermineObservedResponseType(List<string> toolCallNames, string assistantText)
    {
        // Beschreibt nur was beobachtet wurde..keine Aussage über das "Warum"
        return (toolCallNames.Count > 0, !string.IsNullOrWhiteSpace(assistantText)) switch
        {
            (true, true)  => "tool_calls_with_text",
            (true, false) => "tool_calls_only",
            (false, true) => "text_only",
            _             => "empty_response"
        };
    }

    private static string[] DetermineObservedToolGroups(List<string> toolCallNames)
    {
        // Beobachtete Kategorien für schnelle Auswertung — keine Modell-Absichten.
        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in toolCallNames)
        {
            if (name.Equals("fs_read", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("fs_list", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("fs_exists", StringComparison.OrdinalIgnoreCase))
                groups.Add("file_inspection");
            else if (name.Equals("fs_write", StringComparison.OrdinalIgnoreCase))
                groups.Add("file_write");
            else
                groups.Add("other_tool");
        }
        return groups.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    // md report

    /// <summary>
    /// Erzeugt einen lesbaren Abschnitt für response-text.md des Agenten.
    /// </summary>
    /// <remarks>
    /// Beantwortet: "Was hat das Modell in Iteration N formuliert, und neben welchen Tool-Calls stand dieser Text?"
    /// Der Text ist model-declared.. kein Beweis für interne Kausalität.
    /// </remarks>
    private string BuildResponseTextMarkdown(
        int round,
        string[] toolCallsInResponse,
        string text,
        int totalLength,
        bool truncated)
    {
        var sb = new StringBuilder();
        // Überschrift je Scope: pro Modell-Runde nummeriert, oder ein Block für den ganzen Agent-Chat.
        var heading = _scope == LoggingScope.ModelRound
            ? $"## Model Round {round}"
            : (round == 1 ? "## Agent Chat" : $"## Agent Chat (Turn {round})");
        sb.AppendLine(heading);
        sb.AppendLine();

        if (toolCallsInResponse.Length > 0)
            sb.AppendLine($"- Tool calls in this response: `{string.Join("`, `", toolCallsInResponse)}`");
        else
            sb.AppendLine("- No tool calls (standalone text response)");

        sb.AppendLine($"- Text length: {totalLength} chars" +
                      (truncated ? $" *(truncated to {text.Length})*" : ""));
        sb.AppendLine();

        // Tool-only-Runde: kein Assistant-Text, aber die Runde (zb ein fs_write) bleibt sichtbar.
        if (string.IsNullOrWhiteSpace(text))
        {
            sb.AppendLine("> *(kein sichtbarer Assistant-Text — Tool-only-Runde; reason/evidence siehe tool-calls.jsonl)*");
        }
        else
        {
            foreach (var line in text.Split('\n'))
                sb.AppendLine($"> {line}");

            if (truncated)
                sb.AppendLine("> *...[truncated]*");
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    // Hilfsmethode

    private static void AddOtelEvent(string name, Dictionary<string, object?> tags)
    {
        var a = Activity.Current;
        if (a is null) return;

        // OTel-Tags dürfen keine null-Werte enthalten.
        var col = new ActivityTagsCollection();
        foreach (var kv in tags)
        {
            if (kv.Value is null) continue;
            col.Add(kv.Key, kv.Value);
        }
        a.AddEvent(new ActivityEvent(name, DateTimeOffset.UtcNow, col));
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + " ...(truncated)";

    private static string Sha256Hex(string s)
    {
        // SHA erlaubt Vergleich von Antworten ohne vollständigen Text zu speichern
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
