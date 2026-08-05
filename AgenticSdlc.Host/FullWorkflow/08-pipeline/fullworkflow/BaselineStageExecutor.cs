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
/// die manuelle CLI-Übergabe ersetzt. <b>Bestellzettel (9g):</b> requirements (Pflicht) + open-questions
/// (Fragen-Spur; Konsument = Frage→DEC an Tor 1). architecture folgt mit R-11 (erst Konsument, dann Bestellung —
/// „nur bestellte Spuren tragen weiter"). Der Recipe läuft in seinem eigenen <c>runs/recipe/&lt;id&gt;/</c> (wie der
/// Ledger); in den Faden-Ordner kommt eine Referenz (<c>02-baselines/baseline-stage.json</c>). Reuse des
/// erprobten RecipeRunner statt Neubau; ein sauberer factored Core ist ein späteres Refactor.
/// </remarks>
[SendsMessage(typeof(ProjectStateBuildRequest))]
internal sealed class BaselineStageExecutor(HostSettings settings, string? baselineModel, RunContext parentRun, string repoRoot)
    : Executor<ConsumableLedgerOutput>("PipelineBaselines")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    // 9g: der Bestellzettel der geteilten Front (Bootstrap- UND Betriebs-Zweig laufen durch diese Stufe).
    // requirements = Pflicht-Spur (ohne sie keine Kette); open-questions = Fragen-Spur (Konsument: Frage->DEC an
    // Tor 1) — fehlt sie, laeuft die Kern-Kette LAUT weiter (Zusatz-Spur darf die Kette nicht toeten).
    internal static readonly string[] OrderedArtifacts = ["requirements", "open-questions"];
    internal const string RequiredArtifact = "requirements";

    public override async ValueTask HandleAsync(ConsumableLedgerOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        // ConfigBridge (im Speicher): consumable als EvidenceLedgerRun, statt run-config zu editieren.
        var recipeSettings = settings with { EvidenceSource = "ledger", EvidenceLedgerRun = input.ConsumablePath };

        var outDir = parentRun.OutputDir("02-baselines");
        var recipeJsonPath = Path.Combine(outDir, "recipe.json");
        await File.WriteAllTextAsync(recipeJsonPath,
            JsonSerializer.Serialize(new { baseline = new { mode = "build", artifacts = OrderedArtifacts } }, Json), ct).ConfigureAwait(false);

        var before = ExistingRecipeRuns(repoRoot);
        string[] recipeArgs = !string.IsNullOrWhiteSpace(baselineModel)
            ? ["recipe", recipeJsonPath, baselineModel]
            : ["recipe", recipeJsonPath];
        var exit = await RecipeRunner.RunAsync(recipeArgs, recipeSettings, repoRoot).ConfigureAwait(false);

        var recipeRunDir = NewRecipeRun(repoRoot, before);
        var (artifactPaths, missingOptional) = CollectArtifacts(recipeRunDir);
        if (exit != 0 || artifactPaths.Count == 0)
        {
            parentRun.AppendEvent(new { type = "STAGE_BASELINES_FAILED", exit, recipeRunDir, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync($"02-baselines fehlgeschlagen (exit {exit}) — {RequiredArtifact}/artifact.json fehlt.").ConfigureAwait(false);
            return;
        }
        foreach (var missing in missingOptional)
            parentRun.AppendEvent(new { type = "STAGE_BASELINES_ARTIFACT_MISSING", artifact = missing, recipeRun = Path.GetFileName(recipeRunDir!), timestampUtc = DateTime.UtcNow });

        // SP2-Fix (= R-32): das Checker-Verdikt AUFDECKEN statt still weiterreichen — je Artefakt-Zweig (9g: auch
        // die Fragen-Spur speist Wahrheit, gleiche Governance). Ein "Rest -> Mensch"-Artefakt (MaxIterationsReached/
        // HumanReview) beendet die Stufe laut; fehlender/unlesbarer Report (Alt-Laeufe) = Warn-Event, weiter (fail-open).
        foreach (var artifactPath in artifactPaths)
        {
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
                parentRun.AppendEvent(new { type = "STAGE_BASELINES_VERDICT_MISSING", artifact = Path.GetFileName(Path.GetDirectoryName(artifactPath)!), recipeRun = Path.GetFileName(recipeRunDir!), timestampUtc = DateTime.UtcNow });
        }

        await File.WriteAllTextAsync(Path.Combine(outDir, "baseline-stage.json"),
            JsonSerializer.Serialize(new { recipeRunDir, artifactPaths }, Json), ct).ConfigureAwait(false);
        parentRun.AppendEvent(new
        {
            type = "STAGE_BASELINES_DONE",
            recipeRun = Path.GetFileName(recipeRunDir!),
            artifacts = artifactPaths.Count,
            timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(new ProjectStateBuildRequest(artifactPaths)).ConfigureAwait(false);
    }

    // Pure + testbar: sammelt die bestellten Artefakt-Pfade eines Recipe-Laufs. Pflicht-Artefakt fehlt -> leere
    // Liste (Aufrufer stoppt hart); fehlende Zusatz-Artefakte werden benannt (Aufrufer meldet laut, faehrt weiter).
    internal static (IReadOnlyList<string> Paths, IReadOnlyList<string> MissingOptional) CollectArtifacts(string? recipeRunDir)
    {
        if (recipeRunDir is null) return ([], []);
        var paths = new List<string>();
        var missingOptional = new List<string>();
        foreach (var type in OrderedArtifacts)
        {
            var path = Path.Combine(recipeRunDir, "baselines", type, "artifact.json");
            if (File.Exists(path)) { paths.Add(path); continue; }
            if (string.Equals(type, RequiredArtifact, StringComparison.Ordinal)) return ([], []);
            missingOptional.Add(type);
        }
        return (paths, missingOptional);
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
