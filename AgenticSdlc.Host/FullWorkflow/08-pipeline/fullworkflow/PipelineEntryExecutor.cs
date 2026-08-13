using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>Der EHRLICHE EINGANGS-VERTRAG des Ein-Graphen (Schritt 5 ③, 05.08.; A′ Schritt 2, 11.08.): pipeline-full
/// akzeptiert ein Transkript ODER ein fertiges Meeting-Delta ODER eine clarify-Antworten-Batch. Genau EINES ist gesetzt.</summary>
/// <remarks>MAF-Erkenntnis (Feature-Matrix §N): Es gibt kein „Start-at-Node" für frische Läufe — native
/// Einstiegs-Flexibilität ist ① Checkpoint-Resume ② TYPISIERTES Input-Routing am Start. Dieses Record ist ②.</remarks>
public sealed record PipelineFullEntry(TranscriptInput? Transcript, ProjectStateDocument? Delta, ClarifySweepInput? Clarify = null, ReprojectRequest? Reproject = null);

/// <summary>ONLY-STEWARD Recovery-Eingang (steward/reprojektion-only-steward.md, 11.08.): re-projiziert die Wahrheit
/// (Core) nach GitHub, wenn ein Forward am externen Rand scheiterte (Core ≠ GitHub, z. B. 403 beim Write). Parameterlos —
/// der Reconcile leitet ALLES aus dem Core ab (truth-first, kein Run-Ordner). NUR der Steward ruft das in genau diesem
/// Fall auf; es ist kein regulärer Kettenschritt (der Forward läuft normal am Ende eines Laufs).</summary>
public sealed record ReprojectRequest();

/// <summary>
/// Start-Dispatcher des Ein-Graphen: routet den Eingangs-Vertrag typisiert — Transkript → Front (Ledger),
/// fertiges Delta → direkt an den existierenden <see cref="BranchDetectorExecutor"/> (der wie immer
/// Bootstrap- vs. Betriebs-Zweig entscheidet: der Delta-Einstieg gilt damit automatisch für BEIDE Bahnen).
/// Ersetzt den CLI-Zwilling `pipeline-hitl` (dessen Resume mit noAgent-Factories brach — hier gilt das
/// ECHTE pipeline-full-Resume). Kein Skip-Flag, kein Zweitgraph: die Quelle IST der Nachrichtenfluss.
/// </summary>
[SendsMessage(typeof(TranscriptInput))]
[SendsMessage(typeof(ProjectStateDocument))]
[SendsMessage(typeof(ClarifySweepInput))]
[SendsMessage(typeof(ReprojectRequest))]
internal sealed class PipelineEntryExecutor(RunContext run) : Executor<PipelineFullEntry>("PipelineEntry")
{
    public override async ValueTask HandleAsync(PipelineFullEntry entry, IWorkflowContext context, CancellationToken ct = default)
    {
        // R-18-Stil: ein ungültiger Vertrag ist ein Aufrufer-Fehler — LAUT scheitern, nie raten. Genau EINES von vieren.
        var hasTranscript = entry.Transcript is not null;
        var hasDelta = entry.Delta is not null;
        var hasClarify = entry.Clarify is not null;
        var hasReproject = entry.Reproject is not null;
        if ((hasTranscript ? 1 : 0) + (hasDelta ? 1 : 0) + (hasClarify ? 1 : 0) + (hasReproject ? 1 : 0) != 1)
            throw new InvalidOperationException("PIPELINE_ENTRY_INVALID: genau EINES von Transcript|Delta|Clarify|Reproject muss gesetzt sein.");

        var kind = hasDelta ? "delta" : hasClarify ? "clarify" : hasReproject ? "reproject" : "transcript";
        run.AppendEvent(new { type = "PIPELINE_ENTRY", runId = run.RunId, entry = kind, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] Einstieg: {kind.ToUpperInvariant()}");

        if (hasDelta)
        {
            // Datei-Vertrag der Faden-Sichtbarkeit (wie ProjectStateBuildExecutor): Downstream-Applies laden das
            // Delta über 04-delta/project-state.json — beim Delta-Einstieg materialisiert der Entry sie.
            var outDir = run.OutputDir("04-delta");
            await File.WriteAllTextAsync(Path.Combine(outDir, "project-state.json"),
                JsonSerializer.Serialize(entry.Delta, ProjectStateJson.Options), ct).ConfigureAwait(false);
            await context.SendMessageAsync(entry.Delta!).ConfigureAwait(false);
        }
        // A′ Schritt 2: clarify-Batch → ClarifyEntryExecutor (baut Plan+Alignment, ERZWINGT Validate, injiziert bei Pass an pbiFinalize).
        else if (hasClarify) await context.SendMessageAsync(entry.Clarify!).ConfigureAwait(false);
        // ONLY-STEWARD: reproject → ReprojectEntryExecutor (leitet das Sync-Delta AUS DEM CORE ab, speist den Forward-Schwanz).
        else if (hasReproject) await context.SendMessageAsync(entry.Reproject!).ConfigureAwait(false);
        else await context.SendMessageAsync(entry.Transcript!).ConfigureAwait(false);
    }
}
