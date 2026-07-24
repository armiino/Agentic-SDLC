using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' Schritt 3 — Graph-Knoten-Form der 01-ledger-Stufe (Wrapper-Executor v1, Design-Note §2).
/// </summary>
/// <remarks>
/// Führt den 01-ledger-Kern (<see cref="LedgerBuildUnitsRunner.BuildAsync"/>) in-process aus — KEINE CLI —
/// und hebt das Ergebnis als <see cref="LedgerStageOutput"/> in den Graphen. Die eigentlichen Ledger-Artefakte
/// bleiben in ihrem natürlichen Lauf (<c>runs/ledger/&lt;id&gt;/</c>); in den Faden-Ordner
/// (<c>01-ledger/ledger-stage.json</c>) kommt die Referenz + Zusammenfassung (v1). v2 (Design-Note): Gate/Trace
/// als eigene Graph-Knoten. Bei Gate-/Workflow-Fehler wird ein terminaler Output geliefert (Graph endet sauber).
/// <paramref name="judgeSettings"/> trägt das Stufen-Modell (aus <c>run-config.fullworkflow.models</c>, vom
/// Assembler gesetzt — Schritt 4).
/// </remarks>
internal sealed class LedgerWrapperExecutor(
    HostSettings settings, HostSettings judgeSettings, RunContext parentRun)
    : Executor<TranscriptInput>("PipelineLedger")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(TranscriptInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var ledgerRun = new RunContext(RunId.New(), "ledger");
        ledgerRun.EnsureFolders();

        var result = await LedgerBuildUnitsRunner.BuildAsync(
            input.TranscriptText, Path.GetFileName(input.TranscriptPath), settings, judgeSettings, ledgerRun, ct)
            .ConfigureAwait(false);

        // Faden-Ordner: Referenz auf den Ledger-Lauf + Zusammenfassung (die Roh-Artefakte bleiben in runs/ledger/).
        var outDir = parentRun.OutputDir("01-ledger");
        await File.WriteAllTextAsync(
            Path.Combine(outDir, "ledger-stage.json"),
            JsonSerializer.Serialize(new { ledgerRunId = ledgerRun.RunId, ledgerRunDir = ledgerRun.RunDir, result }, Json), ct)
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
            MissSignalPath: null, // v1: Miss-Signal wird bei Bedarf im Adjudikations-Adapter aus dem Lauf gelesen.
            ClaimCount: result.ValidatedCount,
            PendingCount: result.NeedsHumanCount)).ConfigureAwait(false);
    }
}
