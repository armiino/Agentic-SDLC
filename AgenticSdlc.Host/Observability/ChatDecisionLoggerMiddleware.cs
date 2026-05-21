using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Observability;

/// <summary>
/// Loggt beobachtbares Verhalten auf Chat-Ebene des Agenten.
///
/// Diese Middleware liegt um den Chat-Client herum. Sie sieht pro GetResponseAsync-Aufruf
/// eine abgeschlossene Modellantwort und speichert, was daran beobachtbar ist:
/// - ob das Modell Text geliefert hat,
/// - ob das Modell strukturierte Toolcalls geliefert hat,
/// - welche Toolcall-Namen enthalten waren,
/// - und optional einen kurzen Ausschnitt der Assistant-Antwort fuer Debugging.
///
/// Wichtige Grenze:
/// Diese Klasse führt keine Tools aus und beweist nicht, warum sich das Modell auf eine
/// bestimmte Weise verhalten hat. Sie speichert nur beobachtbare Antwortsignale. Die echte
/// Tool-Ausführung wird separat in der ToolCallLoggerMiddleware geloggt.
///
/// Warum es diese Klasse zusätzlich zu OpenTelemetry gibt:
/// Viele GenAI-Integrationen loggen Prompt- und Response-Inhalte standardmässig nicht
/// vollständig, unter anderem wegen Datenschutz und Provider-Unterschieden. Diese lokalen
/// Run-Events geben der Masterarbeit ein mögliches Artefakt, auch wenn OTel nur Tokens,
/// Modell-Metadaten oder Span-Zeiten enthält.
/// </summary>
public sealed class ChatDecisionLoggerMiddleware : DelegatingChatClient
{
    private readonly RunContext _run;

    /*
     * Zählt Chat-Client-Aufrufe, nicht einzelne Toolcalls.
     * Eine Chat-Iteration kann mehrere strukturierte Toolcalls enthalten.
     */
    private int _chatIteration = 0;

    /*
     * Optionaler Antwort-Ausschnitt.
     * Standardmässig deaktiviert, weil Modellantworten sensible Daten enthalten können
     * Aktivierung: ENABLE_LLM_ASSISTANT_PREVIEW=1.
     */
    private static readonly bool EnableAssistantPreview =
        ReadEnvBool("ENABLE_LLM_ASSISTANT_PREVIEW", defaultValue: false);

    /*
     * Begrenzt die Größe des Previews..
     * Dadurch bleibt der Ausschnitt für Debugging nutzbar, ohne vollständige
     * Modellantworten oder Transkripte in die Logs zu schreiben
     */
    private static readonly int PreviewChars =
        ReadEnvInt("LLM_PREVIEW_CHARS", defaultValue: 800, min: 100, max: 8000);

    /*
     * Standardmäßig wird der Preview nur geloggt, wenn keine Toolcalls erkannt wurden.
     * Das adressiert einen wichtigen Fehlerfall dieses Projekts: Das Modell beschreibt
     * einen Plan als Text, ruft aber kein echtes fs_write auf.
     */
    private static readonly bool PreviewOnlyWhenNoTools =
        ReadEnvBool("LLM_PREVIEW_ONLY_WHEN_NO_TOOLS", defaultValue: true);

    public ChatDecisionLoggerMiddleware(IChatClient innerClient, RunContext run)
        : base(innerClient)
    {
        _run = run;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        /*
         * Diese Zahl beschreibt Chat-Pipeline-Aufrufe, nicht Tool-Ausführungen.
         * Wenn chatIteration meistens 1 ist, bedeutet das: Der Agent hat nur einen
         * vollständigen Chat-Zyklus benötigt. Innerhalb dieses Zyklus werden mehrere toolcalls ausgeführt
         * eig ist es wie der lebenszyclus des agenten..
         */
        var chatIteration = Interlocked.Increment(ref _chatIteration);

        /*
         * Ergänzt den aktuell aktiven OpenTelemetry-Span um Chat-Metadaten.
         * Diese Tags helfen beim Abgleich zwischen OTel-Traces und lokalen Run-Logs
         */
        Activity.Current?.SetTag("agent.iteration", chatIteration);
        Activity.Current?.SetTag("agent.step.kind", "chat");

        /*
         * Schreibt ein lokales JSONL-Event in den aktuellen Run-Ordner.
         * CHAT_STARTED markiert den Zeitpunkt unmittelbar vor dem Modellaufruf
         */
        _run.AppendEvent(new
        {
            type = "CHAT_STARTED",
            model = options?.ModelId,
            temperature = options?.Temperature,
            maxOutputTokens = options?.MaxOutputTokens,
            timestampUtc = DateTime.UtcNow
        });

        ChatResponse response;
        try
        {
            response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            /*
             * CHAT_FAILED bedeutet, dass die Chat-Pipeline selbst fehlgeschlagen ist,
             * beispielsweise durch einen unvollständigen Provider-Stream.. 
             *
             * Das ist fachlich von Tool-Fehlern zu trennen: Ein Tool kann erfolgreich
             * aufgerufen werden und trotzdem ein Fehlerergebnis liefern. Solche Tool-
             * Ergebnisse werden in der ToolCallLoggerMiddleware behandelt.
             */
            _run.AppendEvent(new
            {
                type = "CHAT_FAILED",
                model = options?.ModelId,
                errorType = ex.GetType().FullName,
                error = ex.Message,
                timestampUtc = DateTime.UtcNow
            });

            /*
             * Hängt die Exception zusätzlich an den aktiven OTel-Span.
             * Dadurch bleibt derselbe Fehler auch in exportierten Traces sichtbar.
             */
            Activity.Current?.AddException(ex);
            throw;
        }

        /*
         * Extrahiert ausschließlich beobachtbare Fakten aus der Modellantwort.. 
         * Es wird keine versteckte Modell-Logik und keine echte Begründung abgeleitet.
         */
        var toolCallNames = ExtractToolCallNames(response);
        var distinctToolCallNames = toolCallNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var assistantText = response.Messages?.LastOrDefault()?.Text ?? string.Empty;
        var observedResponseType = DetermineObservedResponseType(toolCallNames, assistantText);
        var observedToolGroups = DetermineObservedToolGroups(toolCallNames);

        /*
         * Preview-Felder bleiben null, solange die Environment-Konfiguration sie nicht
         * erlaubt. Der SHA ermöglicht den Vergleich vollständiger Antworten, ohne den
         * kompletten Antworttext speichern zu muessen.
         */
        string? preview = null;
        string? previewSha = null;
        int? previewLen = null;

        var shouldPreview = EnableAssistantPreview &&
                            (!PreviewOnlyWhenNoTools || toolCallNames.Count == 0);

        if (shouldPreview && assistantText.Length > 0)
        {
            preview = Truncate(assistantText.Trim(), PreviewChars);
            previewSha = Sha256Hex(assistantText);
            previewLen = assistantText.Length;

            /*
             * Spiegelt den Preview zusätzlich in OpenTelemetry.
             * Dadurch ist dasselbe Debug-Signal sowohl in lokalen Run-Logs als auch
             * in exportierten Traces sichtbar. TODO: vllt nicht nötig alles überall zu haben.
             */
            AddOtelEvent("llm.assistant_preview", new Dictionary<string, object?>
            {
                ["llm.assistant.len"] = previewLen,
                ["llm.assistant.sha256"] = previewSha,
                ["llm.assistant.preview"] = preview
            });
        }

        _run.AppendEvent(new
        {
            type = "CHAT_FINISHED",
            model = options?.ModelId,
            finishReason = response.FinishReason?.ToString(),
            toolCallsCount = toolCallNames.Count,
            toolCallNames = distinctToolCallNames,
            assistantChars = assistantText.Length,
            chatIteration = chatIteration,

            /*
             * Keine vermeintliche Begründung:
             * Diese Felder beschreiben nur, welche Antwortform beobachtet wurde.
             */
            observedResponseType,
            observedToolGroups,

            /*
             * assistantPreview: kurzer Textausschnitt, falls per Environment aktiviert.
             * assistantPreviewSha256: Hash der kompletten Antwort zur Wiedererkennung
             * assistantPreviewLen: Länge der vollständigen Antwort
             */
            assistantPreview = preview,
            assistantPreviewLen = previewLen,
            assistantPreviewSha256 = previewSha,

            timestampUtc = DateTime.UtcNow
        });

        /*
         * Schreibt die kompakte Form derselben Beobachtung nach OpenTelemetry.
         * Die Daten bleiben bewusst klein und strukturiert, damit Traces auswertbar bleiben
         */
        AddOtelEvent("llm.tool_decision", new Dictionary<string, object?>
        {
            ["agent.iteration"] = chatIteration,
            ["llm.toolcalls.count"] = toolCallNames.Count,
            ["llm.toolcalls.names"] = string.Join(",", distinctToolCallNames),
            ["llm.response.observed_type"] = observedResponseType,
            ["llm.toolcalls.observed_groups"] = string.Join(",", observedToolGroups)
        });

        return response;
    }

    private static List<string> ExtractToolCallNames(ChatResponse response)
    {
        var names = new List<string>();
        if (response.Messages is null) return names;

        foreach (var msg in response.Messages)
        {
            /*
             * Microsoft.Extensions.AI speichert Nachrichteninhalte als AIContent-Elemente.
             * Je nach Package-Version oder Provider kann der konkrete Toolcall-Typ variieren.
             * Reflection entkoppelt diesen Logger deshalb von einer exakten Implementierung.
             *
             * Wichtig: Das ist nur eine Beobachtung auf Chat-Ebene. Die verlässliche Quelle
             * für wirklich ausgefuehrte Tools ist die ToolCallLoggerMiddleware, weil sie die
             * echte Function Invocation umschließt.
             */
            var contentsProp = msg.GetType().GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance);
            if (contentsProp?.GetValue(msg) is not System.Collections.IEnumerable contents)
                continue;

            foreach (var item in contents)
            {
                if (item is null) continue;

                var t = item.GetType();

                /*
                 * Die beobachteten AIContent-Typnamen für strukturierte Tool-/Function-Calls
                 * enthalten "Call". Wenn die Library ihre interne Benennung aendert, kann
                 * dieser Logger den Namen verpassen. Die echte Tool-Ausfuehrung wird weiterhin
                 * von der ToolCallLoggerMiddleware erfasst.
                 */
                if (!t.Name.Contains("Call", StringComparison.OrdinalIgnoreCase))
                    continue;

                /*
                 * Toolcall-Content stellt den Function-/Tool-Namen über eine öffentliche
                 * Name-Property bereit. Falls sie nicht verfügbar ist, bleibt das Event mit
                 * <unknown_call> trotzdem sichtbar.
                 */
                var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                var n = nameProp?.GetValue(item)?.ToString();

                names.Add(!string.IsNullOrWhiteSpace(n) ? n! : "<unknown_call>");
            }
        }

        return names;
    }

    private static string DetermineObservedResponseType(List<string> toolCallNames, string assistantText)
    {
        /*
         * Beschreibt nur, was in der Modellantwort beobachtet wurde.
         * Es wird nicht abgeleitet, warum das Modell so gehandelt hat.
         */
        var hasTools = toolCallNames.Count > 0;
        var hasText = !string.IsNullOrWhiteSpace(assistantText);

        return (hasTools, hasText) switch
        {
            (true, true) => "tool_calls_with_text",
            (true, false) => "tool_calls_only",
            (false, true) => "text_only",
            _ => "empty_response"
        };
    }

    private static string[] DetermineObservedToolGroups(List<string> toolCallNames)
    {
        /*
         * Gruppiert Tool-Nutzung fuer spaetere Auswertungen.
         * Die Gruppen sind beobachtete Kategorien, keine modellinternen Gedanken,
         * Absichten oder Entscheidungsursachen.
         */
        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in toolCallNames)
        {
            if (name.Equals("fs_read", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("fs_list", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("fs_exists", StringComparison.OrdinalIgnoreCase))
            {
                groups.Add("file_inspection");
            }
            else if (name.Equals("fs_write", StringComparison.OrdinalIgnoreCase))
            {
                groups.Add("file_write");
            }
            else
            {
                groups.Add("other_tool");
            }
        }

        return groups.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddOtelEvent(string name, Dictionary<string, object?> tags)
    {
        var a = Activity.Current;
        if (a is null) return;

        /*
         * OpenTelemetry-Tags können hier keine null-Werte enthalten.
         * Optionale Felder werden deshalb übersprungen, wenn sie null sind.
         */
        var col = new ActivityTagsCollection();
        foreach (var kv in tags)
        {
            if (kv.Value is null) continue;
            col.Add(kv.Key, kv.Value);
        }

        a.AddEvent(new ActivityEvent(name, DateTimeOffset.UtcNow, col));
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + " ...(truncated)";

    private static string Sha256Hex(string s)
    {
        /*
         * Der Hash der vollstaendigen Assistant-Antwort erlaubt den Vergleich von Antworten,
         * ohne den kompletten Text in jedem Event zu speichern.
         */
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool ReadEnvBool(string key, bool defaultValue)
    {
        var v = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(v)) return defaultValue;
        return v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase) || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private static int ReadEnvInt(string key, int defaultValue, int min, int max)
    {
        var v = Environment.GetEnvironmentVariable(key);
        if (!int.TryParse(v, out var n)) n = defaultValue;
        if (n < min) n = min;
        if (n > max) n = max;
        return n;
    }
}
