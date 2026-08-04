using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Recipes;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' Schritt 4 — Graph-Knoten der 02-baselines-Stufe. Nimmt den <see cref="ConsumableLedgerOutput"/>,
/// erzeugt daraus die Baseline-Artefakte über den <see cref="RecipeRunner"/> und sendet einen
/// <see cref="ProjectStateBuildRequest"/> an die Delta-Stufe.
/// </summary>
/// <remarks>
/// <b>ConfigBridge im Speicher (Design-Note §4):</b> statt run-config zu editieren, wird der consumable per
/// Settings-Override (<c>EvidenceLedgerRun</c>) an den Recipe gereicht — das ist die eine „Front-Bridge", die
/// die manuelle CLI-Übergabe ersetzt. <b>Slice v2:</b> minimales requirements-only-Rezept (build); weitere
/// Baselines/Derivations später. Der Recipe läuft in seinem eigenen <c>runs/recipe/&lt;id&gt;/</c> (wie der
/// Ledger); in den Faden-Ordner kommt eine Referenz (<c>02-baselines/baseline-stage.json</c>). Reuse des
/// erprobten RecipeRunner statt Neubau; ein sauberer factored Core ist ein späteres Refactor.
/// </remarks>
[SendsMessage(typeof(ProjectStateBuildRequest))]
internal sealed class BaselineStageExecutor(HostSettings settings, string? baselineModel, RunContext parentRun, string repoRoot)
    : Executor<ConsumableLedgerOutput>("PipelineBaselines")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(ConsumableLedgerOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        // ConfigBridge (im Speicher): consumable als EvidenceLedgerRun, statt run-config zu editieren.
        var recipeSettings = settings with { EvidenceSource = "ledger", EvidenceLedgerRun = input.ConsumablePath };

        var outDir = parentRun.OutputDir("02-baselines");
        var recipeJsonPath = Path.Combine(outDir, "recipe.json");
        await File.WriteAllTextAsync(recipeJsonPath,
            """{ "baseline": { "mode": "build", "artifacts": ["requirements"] } }""", ct).ConfigureAwait(false);

        var before = ExistingRecipeRuns(repoRoot);
        string[] recipeArgs = !string.IsNullOrWhiteSpace(baselineModel)
            ? ["recipe", recipeJsonPath, baselineModel]
            : ["recipe", recipeJsonPath];
        var exit = await RecipeRunner.RunAsync(recipeArgs, recipeSettings, repoRoot).ConfigureAwait(false);

        var recipeRunDir = NewRecipeRun(repoRoot, before);
        var artifactPath = recipeRunDir is null ? null : Path.Combine(recipeRunDir, "baselines", "requirements", "artifact.json");
        if (exit != 0 || artifactPath is null || !File.Exists(artifactPath))
        {
            parentRun.AppendEvent(new { type = "STAGE_BASELINES_FAILED", exit, recipeRunDir, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync($"02-baselines fehlgeschlagen (exit {exit}) — requirements/artifact.json fehlt.").ConfigureAwait(false);
            return;
        }

        // SP2-Fix (= R-32): das Checker-Verdikt AUFDECKEN statt still weiterreichen. Ein "Rest -> Mensch"-Artefakt
        // (MaxIterationsReached/HumanReview) beendet die Stufe laut (Muster des FAILED-Pfads oben) — kein stiller
        // Fluss Richtung Core. Fehlender/unlesbarer Report (Alt-Laeufe) = Warn-Event, weiter (fail-open).
        var verdict = BaselineFinalReport.TryRead(Path.GetDirectoryName(artifactPath)!);
        if (BaselineFinalReport.NeedsHuman(verdict))
        {
            parentRun.AppendEvent(new { type = "STAGE_BASELINES_NEEDS_HUMAN", decision = verdict!.Decision, recipeRun = Path.GetFileName(recipeRunDir!), artifactPath, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(
                $"02-baselines: Checker-Verdikt '{verdict.Decision}' (Rest -> Mensch) — Artefakt wird NICHT still in den Core gereicht. " +
                $"final-report: {Path.Combine(Path.GetDirectoryName(artifactPath)!, BaselineFinalReport.FileName)}").ConfigureAwait(false);
            return;
        }
        if (verdict is null)
            parentRun.AppendEvent(new { type = "STAGE_BASELINES_VERDICT_MISSING", recipeRun = Path.GetFileName(recipeRunDir!), timestampUtc = DateTime.UtcNow });

        await File.WriteAllTextAsync(Path.Combine(outDir, "baseline-stage.json"),
            JsonSerializer.Serialize(new { recipeRunDir, artifactPaths = new[] { artifactPath } }, Json), ct).ConfigureAwait(false);
        parentRun.AppendEvent(new
        {
            type = "STAGE_BASELINES_DONE",
            recipeRun = Path.GetFileName(recipeRunDir!),
            artifacts = 1,
            timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(new ProjectStateBuildRequest([artifactPath])).ConfigureAwait(false);
    }

    private static HashSet<string> ExistingRecipeRuns(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "runs", "recipe");
        return Directory.Exists(dir)
            ? Directory.GetDirectories(dir).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
    }

    private static string? NewRecipeRun(string repoRoot, HashSet<string> before)
    {
        var dir = Path.Combine(repoRoot, "runs", "recipe");
        if (!Directory.Exists(dir)) return null;
        return Directory.GetDirectories(dir).Where(d => !before.Contains(d)).OrderByDescending(d => d, StringComparer.Ordinal).FirstOrDefault();
    }
}
