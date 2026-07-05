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
}

/// <summary>
/// Lokaler Review-Server (ASP.NET Core Minimal API) + das generische Frontend (embedded index.html).
/// Blockiert bis der Mensch „Fertig" oder „Abbrechen" klickt (bzw. der CancellationToken feuert).
/// Domaenen-unabhaengig: alles Fachliche kommt ueber <see cref="ReviewServerOptions"/> als Delegate.
/// </summary>
public static class LocalReviewServerHost
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static async Task<ReviewServerResult> RunAsync(ReviewServerOptions options, CancellationToken ct = default)
    {
        var session = options.Session;
        var done = new TaskCompletionSource<ReviewOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool IsComplete() => (options.IsComplete ?? (s => s.AllResolved()))(session);
        bool Recompute(ReviewItem it) => (options.RecomputeResolved ?? (i => DefaultResolved(i, session)))(it);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // OS waehlt freien Port
        builder.Logging.ClearProviders();               // ruhige Konsole
        var app = builder.Build();

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

        await app.StartAsync(ct);
        var url = app.Urls.First();
        Console.WriteLine($"[review-ui] {session.Items.Count} Items — offen unter: {url}");
        if (options.OpenBrowser) OpenBrowser(url);

        ReviewOutcome outcome;
        await using (ct.Register(() => done.TrySetResult(ReviewOutcome.Cancelled)))
            outcome = await done.Task;

        await app.StopAsync(CancellationToken.None);
        return new ReviewServerResult(outcome, session);
    }

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
        catch { /* Browser-Oeffnen ist Komfort, kein harter Fehler — URL steht in der Konsole. */ }
    }

    private sealed record ItemUpdate(string FieldKey, string? Value);
    private sealed record SaveResult(bool Resolved, int ResolvedCount, int Total, bool AllResolved);
}
