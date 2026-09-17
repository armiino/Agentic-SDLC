using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Observability;

/// <summary>
/// Beobachtet den Input-Kontext eines Agenten unmittelbar vor dem Modellaufruf.
/// </summary>
/// <remarks>
/// Verantwortlichkeit: Beantworten was dieser Agent als Input-Kontext bekommen hat.
/// Das ist eine andere Frage als die anderen Middlewares:
///   - InputContextLoggerMiddleware  -> Was lag im Input-Kontext vor dem Call?
///   - ChatDecisionLoggerMiddleware  -> Was hat das Modell produziert?
///   - ToolCallLoggerMiddleware      -> Welche Tools hat dieser Agent tatsächlich ausgeführt?
///
/// WICHTIGE GRENZE - was diese Middleware nicht beweisen kann:
///   Ob das Modell den sichtbaren Inhalt intern verarbeitet oder gewichtet hat.
///   Das ist die fundamentale LLM-Black-Box-Grenze, die keine Middleware überwinden kann.
///
/// Extraktionsprinzip:
///   Alle Signale werden strukturell aus ChatMessage-Objekten und ihren Content-Items
///   (FunctionCallContent, FunctionResultContent, TextContent) gewonnen .. kein Regex, kein Text-Matching (wie zuvor)
///   Was nicht strukturell erkennbar ist, wird nicht behauptet.
/// </remarks>
public sealed partial class InputContextLoggerMiddleware : DelegatingChatClient
{
    private readonly RunContext _run;
    private readonly string? _agentName;
    private int _chatIteration;

    public InputContextLoggerMiddleware(IChatClient innerClient, RunContext run, string? agentName = null)
        : base(innerClient)
    {
        _run = run;
        _agentName = string.IsNullOrWhiteSpace(agentName) ? null : agentName;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var iteration = Interlocked.Increment(ref _chatIteration);
        LogInputContext(messages.ToList(), iteration);
        return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var iteration = Interlocked.Increment(ref _chatIteration);
        var inputMessages = messages.ToList();
        LogInputContext(inputMessages, iteration);

        await foreach (var update in base.GetStreamingResponseAsync(inputMessages, options, cancellationToken).ConfigureAwait(false))
            yield return update;
    }

    //Analyse

    private void LogInputContext(IReadOnlyList<ChatMessage> messages, int chatIteration)
    {
        if (messages.Count == 0 || _agentName is null)
            return;

        // Schritt 1: Fakten aus ChatMessage-Objekten extrahieren
        //
        // Alles hier ist direkt aus dem Objekt-Graph gelesen 
        // Reflection wird genutzt weil Microsoft.Extensions.AI keine stabile öffentliche API für alle Content-Typen bereitstellt.
        // 

        var roleCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var agentNamesInContext = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allToolCalls = new List<ToolCallObservation>();
        var allToolResults = new List<ToolResultObservation>();
        var messageSummaries = new List<MessageSummary>();

        for (var i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];
            var role = TryGetRole(msg) ?? "unknown";
            var agentName = TryGetMessageName(msg);

            if (!string.IsNullOrWhiteSpace(agentName))
                agentNamesInContext.Add(agentName!);

            Increment(roleCounts, role);

            var msgToolCalls = new List<ToolCallObservation>();
            var msgToolResults = new List<ToolResultObservation>();
            var totalTextLength = 0;

            // Direkte Text-Property (TextContent oder role-Nachricht mit Text)
            var directText = TryGetPropertyValue(msg, "Text")?.ToString();
            if (!string.IsNullOrEmpty(directText))
                totalTextLength += directText.Length;

            // Content-Items strukturell auswerten
            var contentsProp = msg.GetType().GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance);
            if (contentsProp?.GetValue(msg) is IEnumerable contents)
            {
                foreach (var item in contents)
                {
                    if (item is null) continue;
                    var typeName = item.GetType().Name;

                    if (typeName.Contains("Call", StringComparison.OrdinalIgnoreCase))
                    {
                        // FunctionCallContent: Tool-Name und Pfad-Argument strukturell extrahieren.
                        var toolName = TryGetPropertyValue(item, "Name")?.ToString() ?? "<unknown_tool>";
                        var path = TryExtractPathArgument(item);
                        var obs = new ToolCallObservation(i, agentName, toolName, path);
                        msgToolCalls.Add(obs);
                        allToolCalls.Add(obs);
                    }
                    else if (typeName.Contains("Result", StringComparison.OrdinalIgnoreCase) ||
                             typeName.Contains("Response", StringComparison.OrdinalIgnoreCase))
                    {
                        // FunctionResultContent: Länge des Ergebnisses als Größenindikator.
                        // Länge ist nicht gleich Beweis für Inhalt.. für Inhaltsverifikation: tool-calls.jsonl prüfen.
                        var resultText = TryGetPropertyValue(item, "Text")?.ToString()
                                      ?? TryGetPropertyValue(item, "Content")?.ToString()
                                      ?? TryGetPropertyValue(item, "Result")?.ToString()
                                      ?? string.Empty;
                        var obs = new ToolResultObservation(i, agentName, resultText.Length);
                        msgToolResults.Add(obs);
                        allToolResults.Add(obs);
                        totalTextLength += resultText.Length;
                    }
                    else
                    {
                        // TextContent oder andere Content-Typen: nur Länge messen.
                        var text = TryGetPropertyValue(item, "Text")?.ToString() ?? string.Empty;
                        totalTextLength += text.Length;
                    }
                }
            }

            messageSummaries.Add(new MessageSummary(
                Index: i,
                Role: role,
                AgentName: agentName,
                HasToolCall: msgToolCalls.Count > 0,
                HasToolResult: msgToolResults.Count > 0,
                TextLength: totalTextLength,
                ToolCalls: msgToolCalls,
                ToolResults: msgToolResults));
        }

        // Schritt 2: Signale strukturell ableiten
        //
        // Diese Signale folgen direkt aus den extrahierten Fakten oben.
        // Sie sind keine Heuristiken.. sie basieren auf konkreten FunctionCallContent-Objekten.
        //
        // Verbleibende Einschränkung: "Read im Kontext" bedeutet der Tool-Call ist sichtbar.
        // Es bedeutet nicht dass der fs_read erfolgreich war oder das Ergebnis korrekt ist.
        // Für Erfolgsverifikation: tool-calls.jsonl des jeweiligen Agenten prüfen.

        var transcriptReadInContext = allToolCalls.FirstOrDefault(c =>
            c.ToolName.Equals("fs_read", StringComparison.OrdinalIgnoreCase) &&
            c.Path?.StartsWith("input/transcripts/", StringComparison.OrdinalIgnoreCase) == true);

        var contextMdReadInContext = allToolCalls.FirstOrDefault(c =>
            c.ToolName.Equals("fs_read", StringComparison.OrdinalIgnoreCase) &&
            c.Path?.EndsWith("context.md", StringComparison.OrdinalIgnoreCase) == true);

        var visibleToolNames = allToolCalls
            .Select(c => c.ToolName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        //Schritt 3: Event schreiben

        var evt = new
        {
            type = "INPUT_CONTEXT_ANALYZED",
            runId = _run.RunId,
            agentName = _agentName,
            chatIteration,

            // Fakten
            inputMessageCount = messages.Count,
            roles = roleCounts,
            agentNamesInContext = agentNamesInContext.OrderBy(x => x).ToArray(),
            toolCallsInContext = allToolCalls.Select(c => new { c.MessageIndex, c.AgentName, c.ToolName, c.Path }).ToArray(),
            toolResultsInContext = allToolResults.Select(r => new { r.MessageIndex, r.AgentName, r.ContentLength }).ToArray(),

            // Abgeleitete Signale (strukturell, kein Regex)
            transcriptReadInContext = transcriptReadInContext is not null,
            transcriptReadPath = transcriptReadInContext?.Path,
            transcriptReadMessageIndex = transcriptReadInContext?.MessageIndex,
            contextMdReadInContext = contextMdReadInContext is not null,
            contextMdReadPath = contextMdReadInContext?.Path,
            contextMdReadMessageIndex = contextMdReadInContext?.MessageIndex,
            visibleToolNames,

            note = "Signals derived structurally from FunctionCallContent objects. Does not prove model used or weighted the content.",
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendEvent(evt);
        _run.AppendAgentEvent(_agentName, evt);
        _run.AppendAgentInputContext(_agentName, evt);
        _run.AppendAgentInputContextMarkdown(_agentName, BuildMarkdownReport(
            chatIteration, roleCounts, agentNamesInContext,
            messageSummaries, allToolCalls, allToolResults,
            transcriptReadInContext, contextMdReadInContext));

        AddOtelEvent("llm.input_context_analyzed", new Dictionary<string, object?>
        {
            ["agent.name"] = _agentName,
            ["agent.iteration"] = chatIteration,
            ["llm.input.messages.count"] = messages.Count,
            ["llm.input.roles"] = string.Join(",", roleCounts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}")),
            ["llm.input.agent_names"] = string.Join(",", agentNamesInContext.OrderBy(x => x)),
            ["llm.input.transcript_read_in_context"] = transcriptReadInContext is not null,
            ["llm.input.context_md_read_in_context"] = contextMdReadInContext is not null,
            ["llm.input.tool_calls_count"] = allToolCalls.Count,
            ["llm.input.tool_results_count"] = allToolResults.Count
        });
    }

    //Markdown-Report

    private string BuildMarkdownReport(
        int chatIteration,
        IReadOnlyDictionary<string, int> roleCounts,
        IReadOnlyCollection<string> agentNamesInContext,
        IReadOnlyList<MessageSummary> messageSummaries,
        IReadOnlyList<ToolCallObservation> allToolCalls,
        IReadOnlyList<ToolResultObservation> allToolResults,
        ToolCallObservation? transcriptRead,
        ToolCallObservation? contextMdRead)
    {
        var sb = new StringBuilder();

        // Header
        var heading = chatIteration == 1
            ? $"# Input Context — {_agentName}"
            : $"## Chat Iteration {chatIteration} — {_agentName}";
        sb.AppendLine(heading);
        sb.AppendLine();
        sb.AppendLine($"- **Run:** `{_run.RunId}`");
        sb.AppendLine();

        // ── Fakten ──────────────────────────────────────────────────────────
        sb.AppendLine("## Fakten");
        sb.AppendLine();
        sb.AppendLine("> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf");
        sb.AppendLine("> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).");
        sb.AppendLine("> Kein Regex, kein Text-Matching.");
        sb.AppendLine();
        sb.AppendLine($"- **Input Messages:** {messageSummaries.Count}");
        sb.AppendLine($"- **Roles:** {FormatCounts(roleCounts)}");

        if (agentNamesInContext.Count > 0)
            sb.AppendLine($"- **Agent-Namen im Kontext (ChatMessage.Name):** {string.Join(", ", agentNamesInContext.Select(n => $"`{n}`"))}");
        else
            sb.AppendLine("- **Agent-Namen im Kontext:** keine");

        sb.AppendLine();

        // Nachrichtenübersicht
        sb.AppendLine("### Nachrichten");
        sb.AppendLine();
        sb.AppendLine("| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |");
        sb.AppendLine("|---|------|-----------|-----------|-------------|------------|");
        foreach (var msg in messageSummaries)
        {
            sb.AppendLine($"| {msg.Index} " +
                          $"| {msg.Role} " +
                          $"| {msg.AgentName ?? "-"} " +
                          $"| {(msg.HasToolCall ? "ja" : "-")} " +
                          $"| {(msg.HasToolResult ? "ja" : "-")} " +
                          $"| {msg.TextLength} |");
        }
        sb.AppendLine();

        // Tool-Calls im Kontext
        if (allToolCalls.Count > 0)
        {
            sb.AppendLine("### Tool-Calls im Kontext (FunctionCallContent)");
            sb.AppendLine();
            sb.AppendLine("| Msg # | Von Agent | Tool | Pfad |");
            sb.AppendLine("|-------|-----------|------|------|");
            foreach (var tc in allToolCalls)
            {
                sb.AppendLine($"| #{tc.MessageIndex} " +
                              $"| {tc.AgentName ?? "-"} " +
                              $"| `{tc.ToolName}` " +
                              $"| {(tc.Path is not null ? $"`{tc.Path}`" : "-")} |");
            }
            sb.AppendLine();
        }

        // Tool-Results im Kontext
        if (allToolResults.Count > 0)
        {
            sb.AppendLine("### Tool-Results im Kontext (FunctionResultContent)");
            sb.AppendLine();
            sb.AppendLine("| Msg # | Von Agent | Länge |");
            sb.AppendLine("|-------|-----------|-------|");
            foreach (var tr in allToolResults)
            {
                sb.AppendLine($"| #{tr.MessageIndex} " +
                              $"| {tr.AgentName ?? "-"} " +
                              $"| {tr.ContentLength} Zeichen |");
            }
            sb.AppendLine();
        }

        //Abgeleitete Signale
        sb.AppendLine("## Abgeleitete Signale");
        sb.AppendLine();
        sb.AppendLine("> Aus den Fakten strukturell abgeleitet — kein Regex.");
        sb.AppendLine("> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.");
        sb.AppendLine("> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.");
        sb.AppendLine();

        if (transcriptRead is not null)
            sb.AppendLine($"- **Transkript-Read im Kontext:** ✓ JA  " +
                          $"→ `FunctionCallContent(fs_read, {transcriptRead.Path})` in Msg #{transcriptRead.MessageIndex}");
        else
            sb.AppendLine("- **Transkript-Read im Kontext:** — NEIN");

        if (contextMdRead is not null)
            sb.AppendLine($"- **Context.md-Read im Kontext:** ✓ JA  " +
                          $"→ `FunctionCallContent(fs_read, {contextMdRead.Path})` in Msg #{contextMdRead.MessageIndex}");
        else
            sb.AppendLine("- **Context.md-Read im Kontext:** — NEIN");

        sb.AppendLine();

        //Was nicht beweisbar ist
        sb.AppendLine("## Was diese Analyse nicht beweist");
        sb.AppendLine();
        sb.AppendLine("- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.");
        sb.AppendLine("  Das ist die fundamentale LLM-Black-Box-Grenze.");
        sb.AppendLine("- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.");
        sb.AppendLine("  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }

    //Hilfsmethoden

    /// <summary>
    /// Extrahiert das "path"-Argument aus einem FunctionCallContent strukturell.
    /// Versucht mehrere bekannte Dictionary-Typen die Microsoft.Extensions.AI nutzt.
    /// Gibt null zurück wenn kein Pfad-Argument gefunden wird->keine Annahmen.
    /// </summary>
    private static string? TryExtractPathArgument(object item)
    {
        var argsObj = TryGetPropertyValue(item, "Arguments");
        if (argsObj is null) return null;

        // IDictionary<string, object?> gängigster Typ in Microsoft.Extensions.AI
        if (argsObj is IDictionary<string, object?> d1 && d1.TryGetValue("path", out var v1))
            return v1?.ToString();

        if (argsObj is IDictionary<string, object> d2 && d2.TryGetValue("path", out var v2))
            return v2?.ToString();

        // JSON-serialisiertes Argument-Dictionary
        if (argsObj is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("path", out var el))
                    return el.GetString();
            }
            catch { /* kein valides JSON — ignorieren */ }
        }

        return null;
    }

    private static string? TryGetRole(ChatMessage msg)
        => TryGetPropertyValue(msg, "Role")?.ToString();

    private static string? TryGetMessageName(ChatMessage msg)
    {
        // MAF setzt den Agenten-Namen als ChatMessage.Name
        foreach (var prop in new[] { "Name", "AuthorName", "ParticipantName" })
        {
            var v = TryGetPropertyValue(msg, prop)?.ToString();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }
        return null;
    }

    private static object? TryGetPropertyValue(object? target, string propertyName)
    {
        if (target is null) return null;
        return target.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(target);
    }

    private static void Increment(IDictionary<string, int> counts, string key)
    {
        counts.TryGetValue(key, out var current);
        counts[key] = current + 1;
    }

    private static string FormatCounts(IReadOnlyDictionary<string, int> counts)
    {
        if (counts.Count == 0) return "keine";
        return string.Join(", ", counts.OrderBy(kv => kv.Key).Select(kv => $"`{kv.Key}={kv.Value}`"));
    }

    private static void AddOtelEvent(string name, Dictionary<string, object?> tags)
    {
        var a = Activity.Current;
        if (a is null) return;

        var col = new ActivityTagsCollection();
        foreach (var kv in tags)
        {
            if (kv.Value is null) continue;
            col.Add(kv.Key, kv.Value);
        }
        a.AddEvent(new ActivityEvent(name, DateTimeOffset.UtcNow, col));
    }
    

    /// <summary>Strukturell extrahierter Tool-Call aus einem FunctionCallContent.</summary>
    private sealed record ToolCallObservation(
        int MessageIndex,
        string? AgentName,
        string ToolName,
        string? Path);

    /// <summary>
    /// Strukturell extrahiertes Tool-Result aus einem FunctionResultContent.
    /// ContentLength ist ein Größenindikator.. kein Inhaltsbeweis.
    /// </summary>
    private sealed record ToolResultObservation(
        int MessageIndex,
        string? AgentName,
        int ContentLength);

    /// <summary>Zusammenfassung einer einzelnen ChatMessage für den Report.</summary>
    private sealed record MessageSummary(
        int Index,
        string Role,
        string? AgentName,
        bool HasToolCall,
        bool HasToolResult,
        int TextLength,
        IReadOnlyList<ToolCallObservation> ToolCalls,
        IReadOnlyList<ToolResultObservation> ToolResults);
}
