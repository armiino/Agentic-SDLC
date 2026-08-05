using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// Schritt 5 ② (05.08.) — der H1-Anker der Ledger-Kapsel: die ledgerRunId wird im Faden-Ordner
/// (<c>01-ledger/ledger-run.json</c>) persistiert, damit run UND resume dieselbe Kapsel auf demselben
/// <c>runs/ledger/&lt;id&gt;/</c>-Ordner bauen (identischer Graph = Restore-Voraussetzung; ohne Anker
/// würde jedes Resume einen ZWEITEN, halbleeren Ledger-Lauf anfangen).
/// </summary>
internal static class LedgerStageRun
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private sealed record Pointer(string LedgerRunId);

    public static RunContext GetOrCreate(RunContext parentRun)
    {
        var path = Path.Combine(parentRun.OutputDir("01-ledger"), "ledger-run.json");
        string ledgerRunId;
        if (File.Exists(path))
            ledgerRunId = JsonSerializer.Deserialize<Pointer>(File.ReadAllText(path), Json)!.LedgerRunId;
        else
        {
            ledgerRunId = RunId.New();
            File.WriteAllText(path, JsonSerializer.Serialize(new Pointer(ledgerRunId), Json));
        }
        var ledgerRun = new RunContext(ledgerRunId, "ledger");
        ledgerRun.EnsureFolders();
        return ledgerRun;
    }
}

/// <summary>
/// Schritt 5 ② — Eingang der sichtbaren Ledger-Kapsel: adaptiert den typisierten Eingangs-Vertrag
/// (<see cref="TranscriptInput"/>) auf den Input des gebundenen LedgerBuilderUnitCoverage-Workflows
/// (Transkript-String) und wacht über Konstruktions-Drift.
/// </summary>
/// <remarks>
/// Der Drift-Guard: die Kapsel wurde zur GRAPH-BAUZEIT mit dem Transkript konstruiert (FacetValidation
/// braucht es im Konstruktor); die Nachricht bringt es zur LAUFZEIT erneut. Weichen beide ab (z. B.
/// Transkript-Datei zwischen run und resume editiert), ist der Lauf nicht mehr reproduzierbar —
/// LAUT scheitern statt still mit zwei Wahrheiten arbeiten (R-18-Stil).
/// </remarks>
[SendsMessage(typeof(string))]
internal sealed class LedgerIntakeExecutor(RunContext parentRun, RunContext ledgerRun, string constructionTranscript)
    : Executor<TranscriptInput>("PipelineLedgerIntake")
{
    public override async ValueTask HandleAsync(TranscriptInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        if (!string.Equals(input.TranscriptText, constructionTranscript, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "LEDGER_TRANSCRIPT_MISMATCH: das Transkript der Nachricht weicht vom Konstruktions-Transkript der " +
                "Kapsel ab (Datei zwischen run und resume geändert?) — Lauf nicht reproduzierbar, Abbruch.");

        parentRun.AppendEvent(new { type = "STAGE_LEDGER_START", ledgerRunId = ledgerRun.RunId, ledgerRunDir = ledgerRun.RunDir, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(input.TranscriptText).ConfigureAwait(false);
    }
}

/// <summary>
/// Schritt 5 ② — Abschluss der sichtbaren Ledger-Kapsel: empfängt den Summary-Yield des gebundenen
/// Workflows (FacetValidation), führt die GETEILTE Auswertung (<see cref="LedgerBuildUnitsRunner.EvaluateAsync"/>:
/// Quality-Gate, Unit-Trace, Diagnose) und hebt das Ergebnis als <see cref="LedgerStageOutput"/> in den Graphen.
/// Ersetzt den Wrapper-Tail (v1) — Faden-Referenz (<c>ledger-stage.json</c>) und STAGE_LEDGER_DONE unverändert.
/// </summary>
[SendsMessage(typeof(LedgerStageOutput))]
[YieldsOutput(typeof(string))]
internal sealed class LedgerSummaryExecutor(RunContext parentRun, RunContext ledgerRun)
    : Executor<string>("PipelineLedgerSummary")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(string capsuleSummary, IWorkflowContext context, CancellationToken ct = default)
    {
        var result = await LedgerBuildUnitsRunner.EvaluateAsync(ledgerRun).ConfigureAwait(false);

        var outDir = parentRun.OutputDir("01-ledger");
        await File.WriteAllTextAsync(
            Path.Combine(outDir, "ledger-stage.json"),
            JsonSerializer.Serialize(new { ledgerRunId = ledgerRun.RunId, ledgerRunDir = ledgerRun.RunDir, capsuleSummary, result }, Json), ct)
            .ConfigureAwait(false);

        parentRun.AppendEvent(new
        {
            type = "STAGE_LEDGER_DONE",
            ledgerRunId = ledgerRun.RunId,
            success = result.Success,
            gatePass = result.GatePass,
            validated = result.ValidatedCount,
            needsHuman = result.NeedsHumanCount,
            timestampUtc = DateTime.UtcNow
        });

        if (!result.Success)
        {
            await context.YieldOutputAsync(
                $"01-ledger fehlgeschlagen ({result.FailureCode}) — Pipeline-Abbruch. Lauf: {ledgerRun.RunId}")
                .ConfigureAwait(false);
            return;
        }

        await context.SendMessageAsync(new LedgerStageOutput(
            ValidatedPath: result.ValidatedLedgerPath,
            MissSignalPath: null, // wie v1: Miss-Signal wird bei Bedarf im Adjudikations-Adapter aus dem Lauf gelesen.
            ClaimCount: result.ValidatedCount,
            PendingCount: result.NeedsHumanCount)).ConfigureAwait(false);
    }
}
