using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgenticSdlc.HumanReview;

public enum ReviewOutcome
{
    /// <summary>„Weiter/Fertig" — alle Items entschieden; der aufrufende (blockierende) Prozess laeuft weiter.</summary>
    Finished,
    /// <summary>„Abbrechen/Spaeter" — bewusst beendet; der (autogespeicherte) Stand bleibt erhalten.</summary>
    Cancelled
}

public sealed record ReviewServerResult(ReviewOutcome Outcome, ReviewSession Session);

/// <summary>Konfiguration einer Review-Server-Sitzung. Alles Domaenen-Spezifische kommt als Delegate rein.</summary>
public sealed class ReviewServerOptions
{
    public required ReviewSession Session { get; init; }

    /// <summary>
    /// AUTOSAVE-Hook: wird nach JEDER Feldaenderung mit dem aktualisierten Item aufgerufen. Hier persistiert
    /// die Domaene den Stand auf Platte (das Sicherheitsnetz). Der Server ruft es best-effort auf.
    /// </summary>
    public Func<ReviewItem, Task>? OnItemSaved { get; init; }

    /// <summary>Lazy-Kontext-Aufloesung: (itemId, resolverKey) -> Text/Markdown. Optional.</summary>
    public Func<string, string, Task<string>>? ResolveContext { get; init; }

    /// <summary>Optional: Detaildaten fuer eine auswählbare Referenz aus einem Katalog.</summary>
    public Func<string, Task<ReviewReferenceDetails?>>? ResolveReference { get; init; }

    /// <summary>Optional: Lazy-Kontext fuer eine Referenz-Detailkarte (referenceId, resolverKey) -> Text.</summary>
    public Func<string, string, Task<string>>? ResolveReferenceContext { get; init; }

    /// <summary>
    /// Neuberechnung des Resolved-Flags eines Items nach einer Aenderung. Default: alle Required-Felder
    /// nicht-leer. Domaenen mit Bedingungs-Logik (z. B. „merge braucht referenceTarget") ueberschreiben das.
    /// </summary>
    public Func<ReviewItem, bool>? RecomputeResolved { get; init; }

    /// <summary>Ob „Fertig" erlaubt ist. Default: alle Items resolved.</summary>
    public Func<ReviewSession, bool>? IsComplete { get; init; }

    /// <summary>Browser automatisch oeffnen (macOS: <c>open</c>). Default true.</summary>
    public bool OpenBrowser { get; init; } = true;

    /// <summary>
    /// 1f-② (19.08., Zombie-Fund: 13 Sandbox-UIs vom 03.08. liefen noch und hielten Binary-Locks): ohne
    /// „Fertig/Abbrechen" lebt der Server sonst EWIG. Keine HTTP-Aktivität für diese Dauer ⇒ er beendet
    /// sich LAUT selbst (wie „Abbrechen": der Autosave-Stand bleibt, einfach neu oeffnen). null = aus.
    /// </summary>
    public TimeSpan? IdleTimeout { get; init; } = TimeSpan.FromHours(2);
}

/// <summary>
/// Lokaler Review-Server (ASP.NET Core Minimal API) + das generische Frontend (embedded index.html).
/// Blockiert bis der Mensch „Fertig" oder „Abbrechen" klickt (bzw. der CancellationToken feuert).
/// Domaenen-unabhaengig: alles Fachliche kommt ueber <see cref="ReviewServerOptions"/> als Delegate.
/// </summary>
public static class LocalReviewServerHost
{
    /// <summary>
    /// UI-Weg-Stille (18.08.): der deklarierte NUTZER-KANAL dieses Hosts — jede Konsolen-Zeile, die den
    /// Menschen erreichen MUSS (Server-Start, URL, Browser-Warnung), beginnt mit diesem Präfix. Die
    /// Steward-Konsolen-Weiche reicht solche Zeilen auch im Umleitungs-Scope an den Chat durch; alles
    /// andere gilt als Roh-Protokoll. Neue Nutzer-Zeilen hier IMMER mit diesem Präfix schreiben.
    /// </summary>
    public const string UserChannelPrefix = "[review-ui]";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static async Task<ReviewServerResult> RunAsync(ReviewServerOptions options, CancellationToken ct = default)
    {
        var session = options.Session;
        var done = new TaskCompletionSource<ReviewOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool IsComplete() => (options.IsComplete ?? (s => s.AllowPartialFinish || s.AllResolved()))(session);
        bool Recompute(ReviewItem it) => (options.RecomputeResolved ?? (i => DefaultResolved(i, session)))(it);

        Console.WriteLine($"{UserChannelPrefix} starte lokalen Review-Server...");
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // OS waehlt freien Port
        builder.Logging.ClearProviders();               // ruhige Konsole
        var app = builder.Build();

        // 1f-② Leerlauf-Wache: jede HTTP-Anfrage stempelt Aktivität; der Wächter unten beendet bei Stille.
        var lastActivityTicks = DateTime.UtcNow.Ticks;
        app.Use(async (httpCtx, next) => { Volatile.Write(ref lastActivityTicks, DateTime.UtcNow.Ticks); await next(httpCtx); });

        app.MapGet("/", () => Results.Content(IndexHtml(), "text/html; charset=utf-8"));

        app.MapGet("/api/session", () => Results.Json(session, Json));

        app.MapPost("/api/item/{itemId}", async (string itemId, ItemUpdate update) =>
        {
            var item = session.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item is null) return Results.NotFound();

            item.FieldValues.RemoveAll(f => f.FieldKey == update.FieldKey);
            item.FieldValues.Add(new ReviewFieldValue(update.FieldKey, update.Value));
            item.Resolved = Recompute(item);

            if (options.OnItemSaved is not null)
            {
                try { await options.OnItemSaved(item); }
                catch (Exception ex) { return Results.Problem($"Autosave fehlgeschlagen: {ex.Message}"); }
            }

            return Results.Json(new SaveResult(item.Resolved, session.ResolvedCount(), session.Items.Count, IsComplete()), Json);
        });

        app.MapGet("/api/context/{itemId}/{blockKey}", async (string itemId, string blockKey) =>
        {
            if (options.ResolveContext is null) return Results.Text("(kein Kontext-Resolver konfiguriert)");
            var text = await options.ResolveContext(itemId, blockKey);
            return Results.Text(text, "text/plain; charset=utf-8");
        });

        app.MapGet("/api/reference/{referenceId}", async (string referenceId) =>
        {
            if (options.ResolveReference is null) return Results.NotFound();
            var details = await options.ResolveReference(referenceId);
            return details is null ? Results.NotFound() : Results.Json(details, Json);
        });

        app.MapGet("/api/reference/{referenceId}/{blockKey}", async (string referenceId, string blockKey) =>
        {
            if (options.ResolveReferenceContext is null) return Results.Text("(kein Referenz-Kontext-Resolver konfiguriert)");
            var text = await options.ResolveReferenceContext(referenceId, blockKey);
            return Results.Text(text, "text/plain; charset=utf-8");
        });

        app.MapPost("/api/finish", () =>
            IsComplete() ? SetAndOk(done, ReviewOutcome.Finished) : Results.BadRequest("Noch offene Items."));

        app.MapPost("/api/cancel", () => SetAndOk(done, ReviewOutcome.Cancelled));

        Console.WriteLine($"{UserChannelPrefix} binde lokalen Port...");
        await app.StartAsync(ct);
        var url = app.Urls.First();
        Console.WriteLine($"{UserChannelPrefix} {session.Items.Count} Items — offen unter: {url}");
        if (options.OpenBrowser) OpenBrowser(url);

        // 1f-② Leerlauf-Wächter: prüft minütlich; bei Stille > IdleTimeout endet die Sitzung wie „Abbrechen"
        // (LAUT; Autosave-Stand bleibt) — kein Review-Server läuft mehr tagelang als Zombie weiter.
        if (options.IdleTimeout is { } idle)
            _ = Task.Run(async () =>
            {
                while (!done.Task.IsCompleted)
                {
                    await Task.WhenAny(done.Task, Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);
                    if (done.Task.IsCompleted) return;
                    if (IdleExceeded(new DateTime(Volatile.Read(ref lastActivityTicks), DateTimeKind.Utc), DateTime.UtcNow, idle))
                    {
                        Console.WriteLine($"{UserChannelPrefix} {idle.TotalMinutes:0} Minuten ohne Aktivität — Server beendet sich selbst (Stand ist gespeichert; Review einfach neu oeffnen).");
                        done.TrySetResult(ReviewOutcome.Cancelled);
                    }
                }
            });

        ReviewOutcome outcome;
        await using (ct.Register(() => done.TrySetResult(ReviewOutcome.Cancelled)))
            outcome = await done.Task;

        await app.StopAsync(CancellationToken.None);
        return new ReviewServerResult(outcome, session);
    }

    /// <summary>1f-②: die eine testbare Wächter-Regel — Stille länger als das Timeout? (public: die
    /// HumanReview-Assembly hat kein InternalsVisibleTo; die Regel ist ohnehin ein ehrlicher Vertrag.)</summary>
    public static bool IdleExceeded(DateTime lastActivityUtc, DateTime nowUtc, TimeSpan idleTimeout)
        => nowUtc - lastActivityUtc > idleTimeout;

    private static IResult SetAndOk(TaskCompletionSource<ReviewOutcome> tcs, ReviewOutcome o)
    {
        tcs.TrySetResult(o);
        return Results.Ok();
    }

    private static bool DefaultResolved(ReviewItem it, ReviewSession s)
    {
        foreach (var spec in s.FieldSchema.Where(f => f.Required))
        {
            var v = it.FieldValues.FirstOrDefault(f => f.FieldKey == spec.FieldKey)?.Value;
            if (string.IsNullOrWhiteSpace(v)) return false;
        }
        return true;
    }

    private static string IndexHtml()
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("index.html", StringComparison.OrdinalIgnoreCase))
                   ?? throw new InvalidOperationException("index.html nicht als Embedded Resource gefunden.");
        using var s = asm.GetManifestResourceStream(name)!;
        using var r = new StreamReader(s);
        return r.ReadToEnd();
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsMacOS()) Process.Start("open", url);
            else if (OperatingSystem.IsWindows()) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            else Process.Start("xdg-open", url);
        }
        catch (Exception ex)
        {
            // Feil 18.08.: NIE still schlucken — der Autor sass vor einem Browser, der nicht kam.
            Console.WriteLine($"{UserChannelPrefix} Browser-Start fehlgeschlagen ({ex.Message}) — bitte manuell oeffnen: {url}");
        }
    }

    private sealed record ItemUpdate(string FieldKey, string? Value);
    private sealed record SaveResult(bool Resolved, int ResolvedCount, int Total, bool AllResolved);
}
