using AgenticSdlc.Host.FullWorkflow.Recipes;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// SP2-Fix (= R-32): das Checker-Verdikt eines Baseline-Artefakts wird gelesen und "Rest -> Mensch"
// (MaxIterationsReached/HumanReview) korrekt erkannt; fehlende/kaputte Reports sind fail-open (null),
// damit Alt-Laeufe nicht brechen — der Executor warnt dann nur.
public sealed class BaselineFinalReportTests
{
    private static string Dir(string? json)
    {
        var dir = Directory.CreateTempSubdirectory("sp2-test").FullName;
        if (json is not null) File.WriteAllText(Path.Combine(dir, BaselineFinalReport.FileName), json);
        return dir;
    }

    [Fact]
    public void Pass_wird_gelesen_und_eskaliert_nicht()
    {
        var v = BaselineFinalReport.TryRead(Dir("""{"decision":"Pass","pass":true,"iterations":1}"""));
        Assert.NotNull(v);
        Assert.Equal("Pass", v!.Decision);
        Assert.True(v.Pass);
        Assert.False(BaselineFinalReport.NeedsHuman(v));
    }

    [Theory]
    [InlineData("MaxIterationsReached")]
    [InlineData("HumanReview")]
    public void Rest_zu_Mensch_wird_erkannt(string decision)
    {
        // exakt die Form des realen Belegs runs/recipe/20260727_055907_0f5dd9 (decision=MaxIterationsReached, pass=false)
        var v = BaselineFinalReport.TryRead(Dir($$"""{"workflow":"CheckerRepair","decision":"{{decision}}","pass":false}"""));
        Assert.True(BaselineFinalReport.NeedsHuman(v));
    }

    [Fact]
    public void Fehlender_Report_ist_fail_open()
    {
        var v = BaselineFinalReport.TryRead(Dir(null));
        Assert.Null(v);
        Assert.False(BaselineFinalReport.NeedsHuman(v));   // Executor warnt nur, blockt nicht
    }

    [Fact]
    public void Kaputter_Report_ist_fail_open()
    {
        Assert.Null(BaselineFinalReport.TryRead(Dir("{nicht json")));
        Assert.Null(BaselineFinalReport.TryRead(Dir("""{"iterations":3}""")));   // ohne decision-Feld
    }
}
