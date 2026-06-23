using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z6.2-Nachweis: Die deterministische Parsing-/Validierungslogik des TopicExtractors (relevantFor
/// gegen die bekannte Artefakt-Menge gefiltert) und der Round-Trip der eingefrorenen TopicSet-Fixture.
/// Die eigentliche LLM-Extraktion ist ein Run (nicht unit-getestet).
/// </summary>
public sealed class TopicExtractorTests
{
    [Fact]
    public void ParseTopics_Maps_And_Filters_RelevantFor()
    {
        var json =
            "{\"topics\":[{\"topicId\":\"TOPIC-CUR-001\",\"summary\":\"Mehrwährung\",\"status\":\"unresolved\"," +
            "\"sourceTurns\":[10,12],\"relevantFor\":[\"requirements\",\"foobar\",\"risks\"]}]}";

        var topics = TopicExtractor.ParseTopics(json);

        var t = Assert.Single(topics);
        Assert.Equal("TOPIC-CUR-001", t.TopicId);
        Assert.Equal(new[] { 10, 12 }, t.SourceTurns);
        Assert.Equal(new[] { "requirements", "risks" }, t.RelevantFor);  // "foobar" verworfen
    }

    [Fact]
    public void ParseTopics_Bad_Json_Returns_Empty()
        => Assert.Empty(TopicExtractor.ParseTopics("kein json hier"));

    [Fact]
    public void ParseTopics_Skips_Topic_Without_Id_Or_Summary()
    {
        var json = "{\"topics\":[{\"topicId\":\"\",\"summary\":\"x\",\"status\":\"unresolved\",\"sourceTurns\":[],\"relevantFor\":[]}]}";
        Assert.Empty(TopicExtractor.ParseTopics(json));
    }

    [Fact]
    public void TopicSet_Fixture_RoundTrips()
    {
        var set = new TopicSet("T9999.txt", "openai/gpt-4.1-mini",
            new[] { new TopicItem("T1", "summary", "unresolved", new[] { 1, 2 }, new[] { "requirements" }) });

        var json = JsonSerializer.Serialize(set, ReviewJson.Options);
        var back = JsonSerializer.Deserialize<TopicSet>(json, ReviewJson.Options)!;

        Assert.Equal("T9999.txt", back.Transcript);
        var t = Assert.Single(back.Topics);
        Assert.Equal("T1", t.TopicId);
        Assert.Equal(new[] { "requirements" }, t.RelevantFor);
    }
}
