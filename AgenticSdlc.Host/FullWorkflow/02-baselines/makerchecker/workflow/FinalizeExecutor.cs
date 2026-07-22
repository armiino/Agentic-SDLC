using System.Text.Json;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow;

/// <summary>
/// Terminale Stufe „Finalize" des Checker-Repair-Workflows: erreicht über die konditionale Kante
/// <c>Decision != Repair</c> (Pass / MaxIterationsReached / HumanReview). Schreibt das finale Artefakt
/// (<c>final.md</c>) + das Abschluss-Zertifikat (<c>final-report.json</c>) in den Scope-Ordner und yieldet ein typisiertes
/// <see cref="CheckerRepairResult"/> als Workflow-Output — direkt weiterverwendbar, wenn der Workflow als Knoten
/// gebunden wird.
/// </summary>
/// <remarks>
/// Diese Stufe prüft und repariert NICHT — sie friert das Ergebnis der Schleife ein. Die Entscheidung im Verdikt
/// ist das Zertifikat: <c>Pass</c> = vertragskonform; <c>MaxIterationsReached</c>/<c>HumanReview</c> = Restverstöße
/// bleiben und gehen an den Menschen (die ehrliche, bounded Grenze des Auto-Repair, Plan §11).
/// </remarks>
[YieldsOutput(typeof(CheckerRepairResult))]
internal sealed class FinalizeExecutor : Executor<CheckVerdictMessage>
{
    public const string ExecutorName = "CheckerRepairFinalize";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    private readonly RunContext _run;
    private readonly string _artifactName;
    private readonly string _outDir;

    public FinalizeExecutor(RunContext run, string artifactName, string? outputScope = null)
        : base(ExecutorName)
    {
        _run = run;
        _artifactName = artifactName;
        _outDir = run.OutputDir(outputScope);
    }

    public override async ValueTask HandleAsync(
        CheckVerdictMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var finalMd = Path.Combine(_outDir, "final.md");
        await File.WriteAllTextAsync(finalMd, message.Markdown, cancellationToken).ConfigureAwait(false);

        var mc0Errors = message.Structural.Violations.Count(v => v.Severity == ContractSeverity.Error);
        var c7Residual = message.Evidence.Violations.Count;
        var report = new
        {
            workflow = CheckerRepairWorkflow.WorkflowName,
            runId = _run.RunId,
            artifact = _artifactName,
            iterations = message.Iteration,
            decision = message.Decision.ToString(),
            pass = message.Decision == ContractDecision.Pass,
            mc0 = new { message.Structural.Pass, errors = mc0Errors, violations = message.Structural.Violations.Count },
            c7 = new { message.Evidence.Pass, violations = c7Residual, k = message.Evidence.K },
            residualViolations = message.Evidence.Violations,
            timestampUtc = DateTime.UtcNow
        };
        await File.WriteAllTextAsync(
            Path.Combine(_outDir, "final-report.json"),
            JsonSerializer.Serialize(report, Json), cancellationToken).ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "FINALIZED",
            runId = _run.RunId,
            workflow = CheckerRepairWorkflow.WorkflowName,
            decision = message.Decision.ToString(),
            iterations = message.Iteration,
            timestampUtc = DateTime.UtcNow
        });

        await context
            .YieldOutputAsync(new CheckerRepairResult(message.Markdown, message.Decision, message.Iteration, mc0Errors, c7Residual))
            .ConfigureAwait(false);
    }
}
