using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>Der EHRLICHE EINGANGS-VERTRAG des Ein-Graphen (Schritt 5 ③, 05.08.): pipeline-full akzeptiert
/// ein Transkript ODER ein fertiges Meeting-Delta. Genau EINES der Felder ist gesetzt.</summary>
/// <remarks>MAF-Erkenntnis (Feature-Matrix §N): Es gibt kein „Start-at-Node" für frische Läufe — native
/// Einstiegs-Flexibilität ist ① Checkpoint-Resume ② TYPISIERTES Input-Routing am Start. Dieses Record ist ②.</remarks>
public sealed record PipelineFullEntry(TranscriptInput? Transcript, ProjectStateDocument? Delta);

/// <summary>
/// Start-Dispatcher des Ein-Graphen: routet den Eingangs-Vertrag typisiert — Transkript → Front (Ledger),
/// fertiges Delta → direkt an den existierenden <see cref="BranchDetectorExecutor"/> (der wie immer
/// Bootstrap- vs. Betriebs-Zweig entscheidet: der Delta-Einstieg gilt damit automatisch für BEIDE Bahnen).
/// Ersetzt den CLI-Zwilling `pipeline-hitl` (dessen Resume mit noAgent-Factories brach — hier gilt das
/// ECHTE pipeline-full-Resume). Kein Skip-Flag, kein Zweitgraph: die Quelle IST der Nachrichtenfluss.
/// </summary>
[SendsMessage(typeof(TranscriptInput))]
[SendsMessage(typeof(ProjectStateDocument))]
internal sealed class PipelineEntryExecutor(RunContext run) : Executor<PipelineFullEntry>("PipelineEntry")
{
    public override async ValueTask HandleAsync(PipelineFullEntry entry, IWorkflowContext context, CancellationToken ct = default)
    {
        // R-18-Stil: ein ungültiger Vertrag ist ein Aufrufer-Fehler — LAUT scheitern, nie raten.
        var hasTranscript = entry.Transcript is not null;
        var hasDelta = entry.Delta is not null;
        if (hasTranscript == hasDelta)
            throw new InvalidOperationException("PIPELINE_ENTRY_INVALID: genau EINES von Transcript|Delta muss gesetzt sein.");

        run.AppendEvent(new { type = "PIPELINE_ENTRY", runId = run.RunId, entry = hasDelta ? "delta" : "transcript", timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] Einstieg: {(hasDelta ? "DELTA (Front übersprungen — Branch entscheidet Bootstrap|Betrieb)" : "TRANSKRIPT (volle Front)")}");

        if (hasDelta)
        {
            // Datei-Vertrag der Faden-Sichtbarkeit (wie ProjectStateBuildExecutor): Downstream-Applies laden das
            // Delta über 04-delta/project-state.json — beim Delta-Einstieg materialisiert der Entry sie.
            var outDir = run.OutputDir("04-delta");
            await File.WriteAllTextAsync(Path.Combine(outDir, "project-state.json"),
                JsonSerializer.Serialize(entry.Delta, ProjectStateJson.Options), ct).ConfigureAwait(false);
            await context.SendMessageAsync(entry.Delta!).ConfigureAwait(false);
        }
        else await context.SendMessageAsync(entry.Transcript!).ConfigureAwait(false);
    }
}
