using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// U0 (ein-graph-vereinheitlichung §7): die B0-Verzweigung Betrieb vs. Bootstrap als GRAPH-KNOTEN statt
/// C#-Phasen-`if` im Runner. Prüft `ICoreRepository.ExistsAsync` im Handler (Laufzeit, wie MAF es vorsieht)
/// und routet das Delta als typisierte Message — <see cref="BootstrapDelta"/> bzw. <see cref="OperationalDelta"/>;
/// die Kanten des Ein-Graphen routen dann nach Typ. Die Entscheidungs-REGEL bleibt die getestete
/// <see cref="FullWorkflowSettings.UseBootstrapBranch"/> (B0), das Event bleibt <c>PIPELINE_BRANCH</c>.
/// Verdrahtung in den Assemble folgt in U1/U2.
/// </summary>
[SendsMessage(typeof(BootstrapDelta))]
[SendsMessage(typeof(OperationalDelta))]
internal sealed class BranchDetectorExecutor(RunContext run, string repoRoot, PipelineMode mode)
    : Executor<ProjectStateDocument>("PipelineBranch")
{
    public override async ValueTask HandleAsync(ProjectStateDocument delta, IWorkflowContext context, CancellationToken ct = default)
    {
        var coreExists = await new JsonCoreRepository(repoRoot).ExistsAsync().ConfigureAwait(false);
        var wrapped = BranchDetector.Wrap(mode, coreExists, delta);

        run.AppendEvent(new
        {
            type = "PIPELINE_BRANCH",
            mode = mode.ToString(),
            coreExists,
            branch = wrapped is BootstrapDelta ? "bootstrap" : "operational",
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[pipeline-full] Verzweigung: mode={mode} coreExists={coreExists} -> {(wrapped is BootstrapDelta ? "BOOTSTRAP-Zweig" : "Betriebs-Zweig")}");
        await context.SendMessageAsync(wrapped).ConfigureAwait(false);
    }
}

/// <summary>Der pure, testbare Kern des Detectors: Regel anwenden + Delta typisiert einpacken.</summary>
public static class BranchDetector
{
    public static object Wrap(PipelineMode mode, bool coreExists, ProjectStateDocument delta)
        => FullWorkflowSettings.UseBootstrapBranch(mode, coreExists)
            ? new BootstrapDelta(delta)
            : new OperationalDelta(delta);
}
