using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z10 / v02-Nachweis (Call 2 separat): die deterministische Parsing-/Validierungslogik des
/// relevantFor-Klassifikators (relevantFor gegen die Artefakt-Menge gefiltert, topicId als Join-Schluessel
/// erzwungen, reasons nur fuer tatsaechlich gewaehlte Typen behalten, robust gegen Müll/Code-Fences) +
/// Round-Trip des TopicRelevanceSet mit Provenienz. Der eigentliche LLM-Klassifikations-Pass ist ein Run.
/// </summary>
public sealed class TopicRelevanceClassifierTests
{
    [Fact]
    public void ParseRelevance_Maps_And_Filters_RelevantFor()
    {
        var json =
            "{\"topics\":[{\"topicId\":\"TOPIC-CURRENCY-001\",\"relevantFor\":[\"requirements\",\"foobar\",\"risks\"]," +
            "\"reasons\":[{\"artifactType\":\"requirements\",\"reason\":\"fachliche Anforderung\"}," +
            "{\"artifactType\":\"risks\",\"reason\":\"Wechselkursrisiko\"}]}]}";

        var rel = Assert.Single(TopicRelevanceClassifier.ParseRelevance(json));

        Assert.Equal("TOPIC-CURRENCY-001", rel.TopicId);
        Assert.Equal(new[] { "requirements", "risks" }, rel.RelevantFor);   // "foobar" verworfen
        Assert.Equal("fachliche Anforderung", rel.Reasons["requirements"]);
        Assert.Equal("Wechselkursrisiko", rel.Reasons["risks"]);
    }

    [Fact]
    public void ParseRelevance_Drops_Reason_For_Type_Not_In_RelevantFor()
    {
        // architecture-Begruendung, aber architecture NICHT in relevantFor → Reason verworfen (Audit-Konsistenz).
        var json =
            "{\"topics\":[{\"topicId\":\"T-1\",\"relevantFor\":[\"requirements\"]," +
            "\"reasons\":[{\"artifactType\":\"requirements\",\"reason\":\"ok\"}," +
            "{\"artifactType\":\"architecture\",\"reason\":\"sollte nicht bleiben\"}]}]}";

        var rel = Assert.Single(TopicRelevanceClassifier.ParseRelevance(json));

        Assert.Equal(new[] { "requirements" }, rel.RelevantFor);
        Assert.True(rel.Reasons.ContainsKey("requirements"));
        Assert.False(rel.Reasons.ContainsKey("architecture"));
    }

    [Fact]
    public void ParseRelevance_Allows_Empty_RelevantFor()
    {
        // Rein organisatorisches Topic → leere relevantFor ist erlaubt und korrekt.
        var json = "{\"topics\":[{\"topicId\":\"T-ORG\",\"relevantFor\":[],\"reasons\":[]}]}";
        var rel = Assert.Single(TopicRelevanceClassifier.ParseRelevance(json));
        Assert.Empty(rel.RelevantFor);
        Assert.Empty(rel.Reasons);
    }

    [Fact]
    public void ParseRelevance_Skips_Topic_Without_TopicId()
    {
        var json = "{\"topics\":[{\"topicId\":\"  \",\"relevantFor\":[\"risks\"],\"reasons\":[]}]}";
        Assert.Empty(TopicRelevanceClassifier.ParseRelevance(json));
    }

    [Fact]
    public void ParseRelevance_Deduplicates_RelevantFor()
    {
        var json = "{\"topics\":[{\"topicId\":\"T-1\",\"relevantFor\":[\"risks\",\"risks\",\"RISKS\"],\"reasons\":[]}]}";
        var rel = Assert.Single(TopicRelevanceClassifier.ParseRelevance(json));
        Assert.Equal(new[] { "risks" }, rel.RelevantFor);
    }

    [Fact]
    public void ParseRelevance_Bad_Json_Returns_Empty()
        => Assert.Empty(TopicRelevanceClassifier.ParseRelevance("kein json hier"));

    [Fact]
    public void ParseRelevance_Extracts_Json_From_Code_Fence()
    {
        var fenced = "```json\n{\"topics\":[{\"topicId\":\"T-9\",\"relevantFor\":[\"architecture\"]," +
                     "\"reasons\":[{\"artifactType\":\"architecture\",\"reason\":\"Komponente\"}]}]}\n```";
        var rel = Assert.Single(TopicRelevanceClassifier.ParseRelevance(fenced));
        Assert.Equal("T-9", rel.TopicId);
        Assert.Equal(new[] { "architecture" }, rel.RelevantFor);
    }

    [Fact]
    public void TopicRelevanceSet_RoundTrips_With_Provenance()
    {
        var set = new TopicRelevanceSet("T9999_chaos.txt", "openai/gpt-5.4", "openai/gpt-5.4",
            "classify-topic-relevance-v01",
            new[]
            {
                new TopicRelevance("TOPIC-AUTH-002", new[] { "architecture", "risks" },
                    new Dictionary<string, string> { ["architecture"] = "OAuth", ["risks"] = "Token-Leak" })
            });

        var json = JsonSerializer.Serialize(set, ReviewJson.Options);
        var back = JsonSerializer.Deserialize<TopicRelevanceSet>(json, ReviewJson.Options)!;

        Assert.Equal("openai/gpt-5.4", back.ExtractModel);
        Assert.Equal("classify-topic-relevance-v01", back.PromptVersion);
        var rel = Assert.Single(back.Topics);
        Assert.Equal("TOPIC-AUTH-002", rel.TopicId);
        Assert.Equal(new[] { "architecture", "risks" }, rel.RelevantFor);
        Assert.Equal("Token-Leak", rel.Reasons["risks"]);
    }
}
