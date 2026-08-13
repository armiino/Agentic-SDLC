using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// A′ Schritt 2 (steward/graph-entry-vs-werkbank.md §10/§11) — der clarify-GRAPH-Eingang. Aus einer Antworten-Batch
// entsteht über den GETEILTEN Kern (ClarifyEntryPlan) der PBI-Update-Plan + Alignment (injizierte Naht). Dann läuft
// ZWINGEND der Checker (Validate); der Verdict wird NUR aus dessen Ergebnis gebaut (AsVerdict — kein plan→verdict-
// Shortcut). Bei Pass: Verdict an den VORHANDENEN durablen Schwanz (pbiFinalize → pbiPort HUMAN → pbiApply → Forward)
// — kein neuer Apply/Gate/Forward. Bei Non-Pass: SICHTBARER terminaler Stop (Event + gate-report-Artefakt + Klartext-
// Output), KEIN Inject — der Lauf endet fachlich ehrlich statt still „fertig" zu wirken (Auflage 11.08.).
[SendsMessage(typeof(PbiUpdateVerdict))]
[YieldsOutput(typeof(string))]
internal sealed class ClarifyEntryExecutor(RunContext run, string repoRoot, string pbiOutDir, AnswerAlignSeam align)
    : Executor<ClarifySweepInput>("ClarifyEntry")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public override async ValueTask HandleAsync(ClarifySweepInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        Directory.CreateDirectory(pbiOutDir);

        var (plan, _, skipped) = await ClarifyEntryPlan.AssembleAsync(core, input.Answers, run.RunId, align, ct).ConfigureAwait(false);
        foreach (var sk in skipped) Console.WriteLine($"[clarify-entry] SKIP {sk}");

        // Keine gültigen Antworten (alle übersprungen/leer) → TERMINAL + sichtbar, KEIN Inject. Semantik wie der
        // Standalone-Sweep („keine gueltigen Antworten"). WICHTIG: ein leerer Plan bestünde den Checker mit 0 Ops
        // (0 Fehler = Pass) — würde also fälschlich als valider Verdict in den PBI-Schwanz gereicht. Deshalb HIER stoppen.
        if (plan.Operations.Count == 0)
        {
            run.AppendEvent(new { type = "CLARIFY_ENTRY_NO_OPS", runId = run.RunId, skipped = skipped.Count, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(
                $"clarify-entry: keine gültigen Antworten (skipped={skipped.Count}) — bewusst gestoppt, nichts projiziert.")
                .ConfigureAwait(false);
            return;
        }

        // ctx ECHT befüllen — downstream (Finalize/Apply) nutzt OutDir/SourceIngestionRun für Artefakte + Summary.
        // Applied = [] : clarify sind KEINE Ingestion-Ops (nur MARK_CHANGED auf bestehende PBIs).
        // DryRun = false : IDENTISCH zum operativen PBI-Pfad (IngestPbiBridgeExecutor). Dieses Flag steuert NICHT das
        //   GitHub-Execute — ob real geschrieben oder dry-run projiziert wird, entscheidet SPÄTER die Forward-Policy separat.
        // MaxAttempts = 1 : single-shot, kein Repair-Loop (anders als der agentische Placement-Pfad).
        var ctx = new PbiUpdateWfContext(core, [], run.RunId, pbiOutDir, DryRun: false, MaxAttempts: 1);

        // Governance-Leitplanke A′: der Checker läuft ZWINGEND; der Verdict entsteht NUR aus seinem Ergebnis.
        var validation = ClarifyEntryPlan.Validate(core, plan, ctx.MaxAttempts);

        if (validation.Decision == GateDecision.Pass)
        {
            run.AppendEvent(new { type = "CLARIFY_ENTRY_PASS", runId = run.RunId, ops = plan.Operations.Count, alignments = plan.Alignments?.Count ?? 0, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(ClarifyEntryPlan.AsVerdict(validation, ctx)).ConfigureAwait(false);
            return;
        }

        // Non-Pass: SICHTBARER terminaler Stop (Auflage 1) — gate-report-Artefakt + Event + Klartext-Output, KEIN Inject.
        var reportPath = Path.Combine(pbiOutDir, "pbi-update-gate-report.json");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(validation.Report, Json), ct).ConfigureAwait(false);
        var relReport = Path.GetRelativePath(repoRoot, reportPath);
        run.AppendEvent(new
        {
            type = "CLARIFY_ENTRY_GATE_NOT_PASS", runId = run.RunId, decision = validation.Decision.ToString(),
            errors = validation.Report.Errors.Count, gateReportPath = relReport, timestampUtc = DateTime.UtcNow
        });
        await context.YieldOutputAsync(
            $"clarify-entry: bewusst gestoppt — kein valider PBI-Plan (decision={validation.Decision}, errors={validation.Report.Errors.Count}). Siehe {relReport}.")
            .ConfigureAwait(false);
    }
}
