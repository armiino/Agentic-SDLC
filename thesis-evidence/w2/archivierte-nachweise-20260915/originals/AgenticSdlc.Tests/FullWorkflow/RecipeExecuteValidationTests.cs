using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;
using AgenticSdlc.Host.FullWorkflow.Recipes;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ① (05.08.): der faktorierte Rezept-Kern gibt sein Ergebnis TYPISIERT zurueck — bei Validierungs-
// Fehlern Exit!=0 und Run==null (es wird KEIN Sub-Run-Ordner angelegt; frueher haette der Ordner-Diff-Hack
// der Baseline-Stufe hier ins Leere gegriffen). LLM-frei: alle Pfade enden VOR der Client-Erzeugung.
public sealed class RecipeExecuteValidationTests
{
    private static HostSettings S() => HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());

    private static async Task<(int Exit, bool HasRun)> Exec(Recipe r)
    {
        var res = await RecipeRunner.ExecuteAsync(r, "test-recipe.json", null, dryRun: false, S(), Directory.GetCurrentDirectory());
        return (res.Exit, res.Run is not null);
    }

    [Fact]
    public async Task Leere_Artefakt_Liste_scheitert_ohne_Run()
    {
        var (exit, hasRun) = await Exec(new Recipe(new RecipeBaseline("build", []), null));
        Assert.Equal(2, exit);
        Assert.False(hasRun);
    }

    [Fact]
    public async Task Unbekannter_Modus_und_unbekannter_Artefakt_Typ_scheitern_ohne_Run()
    {
        var (exit1, hasRun1) = await Exec(new Recipe(new RecipeBaseline("banana", ["requirements"]), null));
        Assert.Equal(2, exit1); Assert.False(hasRun1);

        var (exit2, hasRun2) = await Exec(new Recipe(new RecipeBaseline("build", ["gibtsnicht"]), null));
        Assert.Equal(2, exit2); Assert.False(hasRun2);
    }

    [Fact]
    public async Task Load_ohne_fromRun_scheitert_ohne_Run()
    {
        var (exit, hasRun) = await Exec(new Recipe(new RecipeBaseline("load", ["requirements"]), null));
        Assert.Equal(2, exit);
        Assert.False(hasRun);
    }
}
