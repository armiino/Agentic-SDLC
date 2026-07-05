using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Terminaler, config-gesteuerter Human-in-the-Loop-Schritt des Ledger-Workflows (Plan §4). Wird NUR bei
/// <see cref="AdjudicationMode.Manual"/> / <see cref="AdjudicationMode.Interactive"/> in den Graph gehängt
/// (bei <see cref="AdjudicationMode.Skip"/> terminiert bereits die FacetValidation-Stufe — Baseline-neutral).
/// </summary>
/// <remarks>
/// Bewusst DÜNN: der Executor orchestriert nur die bereits getesteten, deterministischen CLI-Runner auf den
/// kanonischen On-Disk-Artefakten des Runs (step-03 -> step-03b). Es gibt damit KEINE zweite Implementierung
/// von Queue-Bau / Server / Autosave / apply-Gate:
/// <list type="bullet">
///   <item><description><b>Manual</b>: <see cref="LedgerAdjudicateRunner.RunPrepareAsync"/> schreibt die
///     queue.json in <c>step-03b-adjudicated/</c>; der Prozess endet (die Naht). Autor füllt aus und ruft
///     später <c>ledger-adjudicate-apply</c> / <c>ledger-adjudicate-ui</c>.</description></item>
///   <item><description><b>Interactive</b>: Queue schreiben, dann <see cref="LedgerAdjudicateUiRunner.RunAsync"/>
///     (lokale Review-UI, Autosave, „Fertig" -> apply+Gate+consumable im selben Prozess). Blockiert bis der
///     Mensch fertig/abbricht — konzeptionell wie „auf einen Agenten warten" (ein <c>await</c> darf lange
///     dauern).</description></item>
/// </list>
/// Miss-Signale (coverage/unit) sind optional: im Normal-Build gibt es keine -> die Queue enthält dann nur
/// <c>review_required</c>-Claims; im Unit-Workflow kann <paramref name="missSignalPath"/> auf die
/// <c>step-01d</c>-Compare-Ausgabe zeigen.
/// </remarks>
[YieldsOutput(typeof(string))]
internal sealed class HumanAdjudicationExecutor : Executor<ValidatedLedgerMessage>
{
    public const string ExecutorName = "LedgerHumanAdjudication";

    private readonly AdjudicationMode _mode;
    private readonly RunContext _run;
    private readonly string _repoRoot;
    private readonly bool _openBrowser;
    private readonly string? _missSignalPath;

    public HumanAdjudicationExecutor(AdjudicationMode mode, RunContext run, string repoRoot,
        bool openBrowser, string? missSignalPath)
        : base(ExecutorName)
    {
        _mode = mode;
        _run = run;
        _repoRoot = repoRoot;
        _openBrowser = openBrowser;
        _missSignalPath = missSignalPath;
    }

    public override async ValueTask HandleAsync(
        ValidatedLedgerMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var validatedPath = Path.Combine(_run.RunDir, "step-03-facet-validation", "output.json");
        var outDir = Path.Combine(_run.RunDir, "step-03b-adjudicated");
        var queuePath = Path.Combine(outDir, "queue.json");

        // 1) Queue deterministisch bauen (schreibt nach step-03b — DefaultAdjudicationDir leitet das aus dem
        //    validated-Pfad ab). Wiederverwendung des bestehenden Prepare-Runners.
        var prepArgs = _missSignalPath is null
            ? new[] { "ledger-adjudicate", validatedPath }
            : new[] { "ledger-adjudicate", validatedPath, _missSignalPath };
        var prep = await LedgerAdjudicateRunner.RunPrepareAsync(prepArgs, _repoRoot).ConfigureAwait(false);
        if (prep != 0)
        {
            await context.YieldOutputAsync($"Adjudikation: Queue-Bau fehlgeschlagen (exit {prep}).").ConfigureAwait(false);
            return;
        }

        string summary;
        switch (_mode)
        {
            case AdjudicationMode.Manual:
                summary = $"Adjudikation (Manual): Queue geschrieben -> {Rel(queuePath)}. "
                        + $"Ausfüllen + `ledger-adjudicate-apply {Rel(queuePath)} {Rel(validatedPath)}` "
                        + $"oder `ledger-adjudicate-ui {Rel(queuePath)} {Rel(validatedPath)}`.";
                Console.WriteLine($"[adjudication] Manual — {summary}");
                break;

            case AdjudicationMode.Interactive:
                var uiArgs = _openBrowser
                    ? new[] { "ledger-adjudicate-ui", queuePath, validatedPath }
                    : new[] { "ledger-adjudicate-ui", queuePath, validatedPath, "--no-browser" };
                var code = await LedgerAdjudicateUiRunner.RunAsync(uiArgs, _repoRoot).ConfigureAwait(false);
                summary = $"Adjudikation (Interactive): UI beendet (exit {code}); Ergebnisse in {Rel(outDir)}.";
                break;

            default: // Skip sollte hier nie ankommen (Executor wird dann nicht gehängt).
                summary = "Adjudikation: Skip (keine Aktion).";
                break;
        }

        await context.YieldOutputAsync(summary).ConfigureAwait(false);
    }

    private string Rel(string p) => Path.GetRelativePath(_repoRoot, p);
}
