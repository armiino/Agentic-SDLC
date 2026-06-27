using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// D1/R0-Nachweis: die deterministische Parse-/Validierungslogik des Direct-Review-Classifiers — Achse
/// gefiltert (nur coverage/grounding), Severity normalisiert, leere Description verworfen, robust gegen
/// Code-Fences. **Kern (Fake-0-Schutz):** unparsebar ⇒ <c>null</c> (≠ sauberer Pass), valides leeres
/// findings ⇒ leere Liste. Der eigentliche LLM-Review-Pass ist ein Run (D1).
/// </summary>
public sealed class DirectReviewClassifierTests
{
    [Fact]
    public void ParseFindings_Maps_Coverage_And_Grounding()
    {
        var json =
            "{\"findings\":[" +
            "{\"axis\":\"coverage\",\"type\":\"missing\",\"severity\":\"medium\",\"description\":\"Skalierbarkeit fehlt\",\"transcriptQuote\":\"200..20000 User\",\"artifactQuote\":\"\"}," +
            "{\"axis\":\"grounding\",\"type\":\"false_claim\",\"severity\":\"critical\",\"description\":\"Azure erfunden\",\"transcriptQuote\":\"keine Cloud entschieden\",\"artifactQuote\":\"Azure AD\"}]}";

        var findings = DirectReviewClassifier.ParseFindings(json);

        Assert.NotNull(findings);
        Assert.Equal(2, findings!.Count);
        Assert.Equal("coverage", findings[0].Axis);
        Assert.Null(findings[0].ArtifactQuote);                 // leerer artifactQuote → null
        Assert.Equal("Azure AD", findings[1].ArtifactQuote);
        Assert.Equal("critical", findings[1].Severity);
    }

    [Fact]
    public void ParseFindings_Unparseable_Returns_Null_Not_Empty()   // Fake-0-Schutz
    {
        Assert.Null(DirectReviewClassifier.ParseFindings("kein json"));
        Assert.Null(DirectReviewClassifier.ParseFindings("{\"foo\":1}"));   // kein findings-Key → null
    }

    [Fact]
    public void ParseFindings_Valid_Empty_Is_Empty_List()
    {
        var findings = DirectReviewClassifier.ParseFindings("{\"findings\":[]}");
        Assert.NotNull(findings);
        Assert.Empty(findings!);
    }

    [Fact]
    public void ParseFindings_Drops_Unknown_Axis_And_Empty_Description()
    {
        var json =
            "{\"findings\":[" +
            "{\"axis\":\"certainty\",\"type\":\"overstated\",\"severity\":\"low\",\"description\":\"x\",\"transcriptQuote\":\"q\",\"artifactQuote\":\"\"}," +
            "{\"axis\":\"coverage\",\"type\":\"missing\",\"severity\":\"low\",\"description\":\"  \",\"transcriptQuote\":\"q\",\"artifactQuote\":\"\"}]}";
        Assert.Empty(DirectReviewClassifier.ParseFindings(json)!);
    }

    [Fact]
    public void ParseFindings_Defaults_Unknown_Severity_To_Medium()
    {
        var json = "{\"findings\":[{\"axis\":\"coverage\",\"type\":\"missing\",\"severity\":\"bogus\",\"description\":\"d\",\"transcriptQuote\":\"q\",\"artifactQuote\":\"\"}]}";
        var f = Assert.Single(DirectReviewClassifier.ParseFindings(json)!);
        Assert.Equal("medium", f.Severity);
    }

    [Fact]
    public void ParseFindings_Extracts_From_Code_Fence()
    {
        var fenced = "```json\n{\"findings\":[{\"axis\":\"grounding\",\"type\":\"false_claim\",\"severity\":\"medium\",\"description\":\"d\",\"transcriptQuote\":\"q\",\"artifactQuote\":\"a\"}]}\n```";
        var f = Assert.Single(DirectReviewClassifier.ParseFindings(fenced)!);
        Assert.Equal("grounding", f.Axis);
    }
}
