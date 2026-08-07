using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// R-11 A1b (05.08.) — der ASPEKT-ROUTER des Betriebs-Zweigs: die eine Naht für N Aspekte ohne N Tore.
/// Registry (Profil-Liste) statt hartem switch (E-7); heute registriert: requirement. Ein neuer Aspekt
/// (arch, A1d) = ein Registry-Eintrag + sein Zweig — null Änderung am Bestehenden.
/// </summary>
/// <remarks>
/// D-8 (Teil 5, R-18-Stil): Vor diesem Knoten reisten unregistrierte Aspekte STUMM durchs Ingest-Gate
/// (Coverage prüft nur den req-Aspekt) — jetzt werden sie LAUT geparkt (Event <c>ASPECT_UNROUTED</c> +
/// Konsole), nie still verworfen. Die 9g-Fragen-Spur (<c>open_question</c>) ist KEIN eigener Aspekt,
/// sondern Querschnitt im req-Resolver (Teil 5/D-2) — bekannt, nicht „unrouted".
/// Zwei-Bahnen-Grenze (bewusst): die CLI-Kommandos sind einzel-aspektig (<c>ingest-requirements</c> IST
/// die req-Bahn) — der Router wohnt nur im Ein-Graph, wo das gemischte Delta ankommt.
/// A1d-Endform (Sequenz-Entscheid): architecture ist REGISTRIERT (kein ASPECT_UNROUTED), wird aber NICHT
/// hier dispatcht — der arch-Strip hängt SERIELL hinter dem req-Apply (<c>ArchIngestBridgeExecutor</c>,
/// Core-Mutationen nacheinander); der Router bleibt Wächter + req-Durchreiche.
/// </remarks>
[SendsMessage(typeof(IngestionResolveInput))]
internal sealed class AspectIngestionRouterExecutor(RunContext run, IReadOnlyList<AspectIngestionProfile> registry)
    : Executor<IngestionResolveInput>("AspectIngestionRouter")
{
    /// <summary>Delta-Item-Typen, die bewusst KEIN Aspekt sind (eigene Bahnen/Querschnitte).</summary>
    private static readonly IReadOnlySet<string> CrossCutting =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "open_question" };

    public override async ValueTask HandleAsync(IngestionResolveInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var unrouted = input.MeetingDelta.Items
            .Where(i => !CrossCutting.Contains(i.ItemType) && !registry.Any(p => p.Matches(i)))
            .GroupBy(i => i.ItemType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        if (unrouted.Count > 0)
        {
            run.AppendEvent(new
            {
                type = "ASPECT_UNROUTED",
                runId = run.RunId,
                aspects = unrouted,
                registered = registry.Select(p => p.Aspect).ToArray(),
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine(
                $"[ingest-router] LAUT GEPARKT: {string.Join(", ", unrouted.Select(kv => $"{kv.Key}×{kv.Value}"))} — " +
                "Aspekt(e) ohne registrierten Zweig werden in diesem Lauf NICHT verarbeitet (R-11 A1b; Zweig kommt mit A1d).");
        }

        // req-Zweig wie heute: das Delta unverändert an den (einzigen registrierten) Kern.
        await context.SendMessageAsync(input).ConfigureAwait(false);
    }
}
