using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z6.3-Nachweis: Die deterministische Parsing-/Mapping-Logik des Topic-Coverage-Classifiers
/// (Index→TopicId, Verdikt-Normalisierung). Der LLM-Schritt ist ein Run.
/// </summary>
public sealed class TopicCoverageClassifierTests
{
    private static readonly TopicItem[] Chunk =
    {
        new("T-1", "Mehrwährung", "unresolved", new[] { 1 }, new[] { "requirements" }),
        new("T-2", "PDF-Export", "unresolved", new[] { 2 }, new[] { "requirements" })
    };

    [Fact]
    public void ParseChunk_Maps_Index_To_TopicId_And_Normalizes()
    {
        var json = "{\"results\":[{\"index\":0,\"verdict\":\"MISSING\",\"reason\":\"fehlt\"}," +
                   "{\"index\":1,\"verdict\":\"covered\",\"reason\":\"da\"}]}";

        var verdicts = TopicCoverageClassifier.ParseChunk(json, Chunk);

        Assert.Equal(2, verdicts.Count);
        Assert.Equal(("T-1", "missing", "fehlt"), verdicts[0]);   // MISSING -> missing
        Assert.Equal(("T-2", "covered", "da"), verdicts[1]);
    }

    [Fact]
    public void ParseChunk_Unknown_Verdict_Defaults_NotApplicable()
    {
        var json = "{\"results\":[{\"index\":0,\"verdict\":\"???\",\"reason\":\"x\"}]}";
        var verdicts = TopicCoverageClassifier.ParseChunk(json, Chunk);
        Assert.Equal(("T-1", "not_applicable", "x"), Assert.Single(verdicts));
    }

    [Fact]
    public void ParseChunk_Bad_Json_Returns_Empty()
        => Assert.Empty(TopicCoverageClassifier.ParseChunk("nope", Chunk));

    [Fact]
    public void ParseChunk_Ignores_OutOfRange_Index()
    {
        var json = "{\"results\":[{\"index\":99,\"verdict\":\"missing\",\"reason\":\"x\"}]}";
        Assert.Empty(TopicCoverageClassifier.ParseChunk(json, Chunk));
    }
}
