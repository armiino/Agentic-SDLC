using System.Text.Json;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

/// <summary>
/// W1e'-Spike: Graph-Knoten-Form der DETERMINISTISCHEN Delta-Stufe (<c>project-state-build</c>).
/// </summary>
/// <remarks>
/// Dünner Wrapper um die REINE Funktion <see cref="ProjectStateBuilder.BuildAsync"/> — keine Fachlogik hier,
/// keine CLI. Nimmt die Baseline-Artefaktpfade (aus der Recipe-Bridge) und liefert das MeetingDelta
/// (<see cref="ProjectStateDocument"/>) als Message. Dieser Typ IST exakt der Ingest-Input
/// (<c>IngestionResolveInput.MeetingDelta</c>) — die Stufengrenze Delta→Ingest trägt also ohne Adapter.
/// Zusätzlich wird das Delta im Faden-Ordner (<c>04-delta/project-state.json</c>) sichtbar abgelegt.
/// Grund für die frühe Umsetzung (Spike): sonst blockiert die Graph-Kette später an genau dieser Grenze.
/// </remarks>
internal sealed record ProjectStateBuildRequest(
    IReadOnlyList<string> ArtifactPaths,
    string? L3RunDir = null,
    string? ProjectId = null);

internal sealed class ProjectStateBuildExecutor(RunContext run, string repoRoot)
    : Executor<ProjectStateBuildRequest>("PipelineDeltaBuild")
{
    public override async ValueTask HandleAsync(
        ProjectStateBuildRequest request, IWorkflowContext context, CancellationToken ct = default)
    {
        var doc = await ProjectStateBuilder
            .BuildAsync(repoRoot, request.ArtifactPaths, request.L3RunDir, request.ProjectId, ct)
            .ConfigureAwait(false);

        // Faden-Ordner-Sichtbarkeit: das Delta als Artefakt ablegen (Ingest bekommt es zusätzlich als Message).
        var outDir = run.OutputDir("04-delta");
        await File.WriteAllTextAsync(
            Path.Combine(outDir, "project-state.json"),
            JsonSerializer.Serialize(doc, ProjectStateJson.Options), ct).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "DELTA_BUILT",
            items = doc.Items.Count,
            relations = doc.Relations.Count,
            provenance = doc.Provenance.Count,
            timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(doc).ConfigureAwait(false);
    }
}
