using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;

/// <summary>
/// Stufe „Repair" des MAF-Workflows: patcht die vom Checker bestätigten Zeilen-Verstöße und schickt das
/// reparierte Artefakt als <see cref="CheckArtifactMessage"/> (Iteration+1) über die Loop-Back-Kante zurück
/// an den Checker (Re-Check). Kapselt den bestehenden bounded <see cref="ContractRepair"/> (zeilenweise,
/// Citations erhalten, kein ganzes Artefakt).
/// </summary>
/// <remarks>
/// Wird NUR über die konditionale Kante <c>Decision == Repair</c> erreicht. Der Executor patcht genau die
/// reparierbaren C7-Verstöße (EUD/FO) aus dem Verdikt und schreibt je Iteration <c>step-repair-&lt;iter&gt;/</c>
/// (repariertes md + repair-log.json). Sein LLM-Client ist im Runner mit der Observability-Pipeline
/// (Executor-Name <see cref="ExecutorName"/>) umhüllt → Calls unter <c>logs/agents/MakerCheckerRepair/</c>.
/// </remarks>
[SendsMessage(typeof(CheckArtifactMessage))]
internal sealed class RepairExecutor : Executor<CheckVerdictMessage>
{
    public const string ExecutorName = "MakerCheckerRepair";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly ContractRepair _repair;
    private readonly ConsumableLedger _ledger;
    private readonly RunContext _run;
    private readonly string _outDir;

    public RepairExecutor(ContractRepair repair, ConsumableLedger ledger, RunContext run, string? outputScope = null)
        : base(ExecutorName)
    {
        _repair = repair;
        _ledger = ledger;
        _run = run;
        _outDir = run.OutputDir(outputScope);
    }

    public override async ValueTask HandleAsync(
        CheckVerdictMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var (patched, repairs) = await _repair
            .RepairAsync(message.Markdown, _ledger, message.Evidence.Violations, cancellationToken)
            .ConfigureAwait(false);

        var changed = repairs.Count(r => r.Changed);
        WriteStep(message.Iteration, patched, repairs);

        _run.AppendEvent(new
        {
            type = "REPAIR_APPLIED",
            runId = _run.RunId,
            workflow = CheckerRepairWorkflow.WorkflowName,
            iteration = message.Iteration,
            targets = repairs.Count,
            changed,
            timestampUtc = DateTime.UtcNow
        });

        await context
            .SendMessageAsync(new CheckArtifactMessage(patched, message.Iteration + 1))
            .ConfigureAwait(false);
    }

    private void WriteStep(int iteration, string patched, IReadOnlyList<RepairResult> repairs)
    {
        var dir = Path.Combine(_outDir, $"step-repair-{iteration:D2}");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "repaired.md"), patched);
        File.WriteAllText(Path.Combine(dir, "repair-log.json"),
            JsonSerializer.Serialize(new { iteration, repairs }, Json));
    }
}
