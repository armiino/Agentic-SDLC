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
/// Ledger); in den Faden-Ordner kommt eine Referenz (<c>02-baselines/baseline-stage.json</c>). Seit Schritt 5 ①
/// (05.08.) ruft die Stufe den faktorierten Kern `RecipeRunner.ExecuteAsync` TYPISIERT (RunContext als
/// Rückgabewert — der frühere Ordner-Diff-Hack ist gelöscht).
/// </remarks>
[SendsMessage(typeof(ProjectStateBuildRequest))]
internal sealed class BaselineStageExecutor(HostSettings settings, string? baselineModel, RunContext parentRun, string repoRoot)
    : Executor<ConsumableLedgerOutput>("PipelineBaselines")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    // 9g: der Bestellzettel der geteilten Front (Bootstrap- UND Betriebs-Zweig laufen durch diese Stufe).
    // requirements = Pflicht-Spur (ohne sie keine Kette); open-questions = Fragen-Spur (Konsument: Frage->DEC an
    // Tor 1) — fehlt sie, laeuft die Kern-Kette LAUT weiter (Zusatz-Spur darf die Kette nicht toeten).
    // R-11 A1c (05.08.): architecture bestellt — der Konsument (arch-Strip, A1d) steht im SELBEN Slice
    // („nur bestellte Spuren tragen weiter"). Zusatz-Spur wie open-questions: fehlt sie, laeuft die Kette LAUT weiter.
    // Slice S ④ (21.08.): risks BESTELLT — Konsument = Risiko→DEC über die QuestionLane an Tor 1
    // („nur bestellte Spuren tragen weiter" — die tote risks-Extraktion bekommt ihren Besteller).
    internal static readonly string[] OrderedArtifacts = ["requirements", "open-questions", "architecture", "risks"];
    internal const string RequiredArtifact = "requirements";

    public override async ValueTask HandleAsync(ConsumableLedgerOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        // ConfigBridge (im Speicher): consumable als EvidenceLedgerRun, statt run-config zu editieren.
        var recipeSettings = settings with { EvidenceSource = "ledger", EvidenceLedgerRun = input.ConsumablePath };

        var outDir = parentRun.OutputDir("02-baselines");
        var recipeJsonPath = Path.Combine(outDir, "recipe.json");

        // Schritt 5 ① (05.08.): das Rezept TYPISIERT bauen und den faktorierten Kern direkt rufen — der
        // Sub-RunContext kommt als RÜCKGABEWERT (frueher: CLI-Aufruf + Ordner-Diff `ExistingRecipeRuns`/
        // `NewRecipeRun`, fragil bei parallelen Recipe-Läufen). recipe.json bleibt als Faden-Audit erhalten.
        var recipe = new Recipe(new RecipeBaseline("build", OrderedArtifacts), Derivations: []);
        await File.WriteAllTextAsync(recipeJsonPath, JsonSerializer.Serialize(recipe, Json), ct).ConfigureAwait(false);

        var result = await RecipeRunner.ExecuteAsync(
            recipe, recipeJsonPath, string.IsNullOrWhiteSpace(baselineModel) ? null : baselineModel,
            dryRun: false, recipeSettings, repoRoot).ConfigureAwait(false);
        var exit = result.Exit;

        var recipeRunDir = result.Run?.RunDir;
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

}
