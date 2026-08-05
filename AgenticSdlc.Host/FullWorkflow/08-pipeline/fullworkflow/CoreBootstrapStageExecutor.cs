using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// Bootstrap-Zweig B1 (pipeline-full-bootstrap-plan §7): deterministischer core-bootstrap als Graph-Knoten.
/// </summary>
/// <remarks>
/// Dünner Wrapper um die EXISTIERENDEN puren Kerne (§2b K1): <see cref="CoreSeeder.Seed"/> →
/// <c>ICoreRepository.SaveAsync</c> → <see cref="CoreToBaseline.Project"/>. Keine Fachlogik hier, keine CLI.
/// Input = das Phase-1-Delta (<see cref="ProjectStateDocument"/>, dieselbe Message wie beim Ingest); Output =
/// <see cref="CoreBootstrapOutput"/> mit dem Baseline-Triplet-Pfad (Input für re-clarify cluster, B3).
/// Der „Core existiert"-Fall ist hier ein LAUTER Abbruch (String-Output): der Branch (B0) wählt Bootstrap nur
/// bei leerem Core oder erzwungenem mode — ein existierender Core heißt Config-Fehler, nie Überschreiben.
/// </remarks>
public static class CoreBootstrapStage
{
    /// <summary>Der testbare Kern: Seed → Save → Baseline-Triplet nach <paramref name="outDir"/>.</summary>
    public static async Task<CoreBootstrapOutput> ExecuteAsync(
        string repoRoot, string outDir, ProjectStateDocument source, string sourceRun = "core-bootstrap", CancellationToken ct = default)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("BOOTSTRAP_ABORTED_CORE_EXISTS: Core existiert bereits — Bootstrap überschreibt keine Wahrheit.");

        var (core, report) = CoreSeeder.Seed(source, sourceRun);
        await repo.SaveAsync(core).ConfigureAwait(false);

        var coreSourceRel = Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot));
        var (baseline, provenance, quality) = CoreToBaseline.Project(core, coreSourceRel);

        Directory.CreateDirectory(outDir);
        var baselinePath = Path.Combine(outDir, "canonical-requirements-baseline.json");
        await File.WriteAllTextAsync(baselinePath, JsonSerializer.Serialize(baseline, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "provenance-map.json"), JsonSerializer.Serialize(provenance, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "quality-report.json"), JsonSerializer.Serialize(quality, JsonFiles.Json), ct).ConfigureAwait(false);

        return new CoreBootstrapOutput(outDir, baselinePath, report.Total, report.Requirements);
    }
}

[SendsMessage(typeof(CoreBootstrapOutput))]
internal sealed class CoreBootstrapStageExecutor(RunContext run, string repoRoot)
    : Executor<BootstrapDelta>("PipelineCoreBootstrap") // U1: konsumiert das vom Branch-Detector geroutete Delta
{
    public override async ValueTask HandleAsync(
        BootstrapDelta input, IWorkflowContext context, CancellationToken ct = default)
    {
        var source = input.Delta;
        // U1: der Bootstrap-Zweig beginnt HIER — Event bleibt STAGE_BOOTSTRAP_START (R-28-Metrik-Anker).
        run.AppendEvent(new { type = "STAGE_BOOTSTRAP_START", deltaItems = source.Items.Count, timestampUtc = DateTime.UtcNow });
        CoreBootstrapOutput output;
        try
        {
            output = await CoreBootstrapStage.ExecuteAsync(repoRoot, run.OutputDir("05-core"), source, run.RunId, ct).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await context.SendMessageAsync(ex.Message).ConfigureAwait(false);
            return;
        }

        run.AppendEvent(new
        {
            type = "STAGE_CORE_BOOTSTRAP_DONE",
            coreItems = output.CoreItems,
            requirements = output.Requirements,
            baselineDir = output.BaselineDir,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(output).ConfigureAwait(false);
    }
}
