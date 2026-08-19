using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// Politur-Feinschliff (18.08., Abnahme-Feil-Punkt #2): der Chat trennt SICHTBAR, wer spricht — jede
/// Steward-Antwort beginnt als eigener Block mit dem `steward>`-Marker (Leerzeile davor/danach), damit
/// nichts mehr optisch am `du>`-Prompt klebt. Reine Darstellungs-Schicht, kein Verhalten.
/// </summary>
public static class StewardChatRendering
{
    /// <summary>Antwort-Block: Leerzeile · `steward>`-Marker · Text · Leerzeile (vor dem nächsten du>).</summary>
    public static string RenderAnswer(string text)
        => Environment.NewLine + "steward> " + text.TrimEnd() + Environment.NewLine;
}

/// <summary>
/// Politur-Feinschliff (Autor-Wunsch „sehen, wie viele Steps er liefert und ob er noch arbeitet"):
/// eine DEZENTE Fortschritts-Zeile je Werkzeug-Schritt („  ⋯ get_run_status"), gedruckt VOR der
/// Ausführung. NUR am Steward-Agenten eingehängt (die geteilte Logger-Middleware bleibt unberührt);
/// Pipeline-Agenten, die der Steward startet, laufen unter der Konsolen-Weiche — ihre Schritt-Zeilen
/// landen damit automatisch im Lauf-`console.log` (Beleg-Bonus statt Chat-Lärm).
/// </summary>
public sealed class StewardToolEchoMiddleware
{
    private int _step;

    public async ValueTask<object?> InvokeAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        var n = Interlocked.Increment(ref _step);
        Console.WriteLine($"  ⋯ {n}: {context.Function?.Name ?? "?"}{ArgHint(context.Arguments)}");
        return await next(context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Kompakter Kontext je Schritt („(status=needs_clarify)") — sonst sind z. B. zwei
    /// list_core_items-Aufrufe nicht unterscheidbar. Nur bekannte Kurz-Schluessel, hart gekappt.</summary>
    internal static string ArgHint(IDictionary<string, object?>? args)
    {
        if (args is null || args.Count == 0) return "";
        string[] keys = ["runId", "query", "status", "itemType", "itemId", "proposalId", "deltaPath", "repo", "answersPath", "sourceRunId", "issueNumber"];
        var parts = keys.Where(k => args.TryGetValue(k, out var v) && v is not null && v.ToString() is { Length: > 0 })
                        .Select(k => $"{k}={Truncate(args[k]!.ToString()!, 40)}").Take(2).ToList();
        return parts.Count == 0 ? "" : $" ({string.Join(", ", parts)})";
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
}
