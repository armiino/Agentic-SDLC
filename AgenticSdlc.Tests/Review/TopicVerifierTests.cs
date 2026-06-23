using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z8-Nachweis (DISK-COV-5): die deterministische Parsing-/Validierungslogik des begrenzten
/// Topic-Completeness-Verifiers (suggestedRelevantFor gegen die Artefakt-Menge gefiltert, leere Summaries
/// verworfen, robust gegen Müll/Code-Fences) + Round-Trip des TopicVerifyResult mit Provenienz.
/// Der eigentliche LLM-Verify-Pass ist ein Run (nicht unit-getestet).
/// </summary>
public sealed class TopicVerifierTests
{
    [Fact]
    public void ParseCandidates_Maps_And_Filters_RelevantFor()
    {
        var json =
            "{\"missingTopicCandidates\":[{\"summary\":\"Push-Notifications fehlen\",\"sourceTurns\":[32,35]," +
            "\"suggestedRelevantFor\":[\"requirements\",\"foobar\",\"risks\"],\"reason\":\"Kein Topic deckt Push ab.\"}]}";

        var cands = TopicVerifier.ParseCandidates(json);

        var c = Assert.Single(cands);
        Assert.Equal("Push-Notifications fehlen", c.Summary);
        Assert.Equal(new[] { 32, 35 }, c.SourceTurns);
        Assert.Equal(new[] { "requirements", "risks" }, c.SuggestedRelevantFor);  // "foobar" verworfen
        Assert.Equal("Kein Topic deckt Push ab.", c.Reason);
    }

    [Fact]
    public void ParseCandidates_Empty_List_Is_Valid()
        => Assert.Empty(TopicVerifier.ParseCandidates("{\"missingTopicCandidates\":[]}"));

    [Fact]
    public void ParseCandidates_Bad_Json_Returns_Empty()
        => Assert.Empty(TopicVerifier.ParseCandidates("kein json hier"));

    [Fact]
    public void ParseCandidates_Skips_Candidate_Without_Summary()
    {
        var json = "{\"missingTopicCandidates\":[{\"summary\":\"  \",\"sourceTurns\":[1],\"suggestedRelevantFor\":[\"risks\"],\"reason\":\"x\"}]}";
        Assert.Empty(TopicVerifier.ParseCandidates(json));
    }

    [Fact]
    public void ParseCandidates_Extracts_Json_From_Code_Fence()
    {
        var fenced = "```json\n{\"missingTopicCandidates\":[{\"summary\":\"Monitoring\",\"sourceTurns\":[556]," +
                     "\"suggestedRelevantFor\":[\"architecture\"],\"reason\":\"r\"}]}\n```";
        var c = Assert.Single(TopicVerifier.ParseCandidates(fenced));
        Assert.Equal("Monitoring", c.Summary);
    }

    [Fact]
    public void TopicVerifyResult_RoundTrips_With_Provenance()
    {
        var result = new TopicVerifyResult("T9999_chaos.txt", "openai/gpt-5.4", "anthropic/claude-x",
            new[] { new MissingTopicCandidate("Push fehlt", new[] { 32 }, new[] { "requirements" }, "kein Topic") });

        var json = JsonSerializer.Serialize(result, ReviewJson.Options);
        var back = JsonSerializer.Deserialize<TopicVerifyResult>(json, ReviewJson.Options)!;

        Assert.Equal("openai/gpt-5.4", back.ExtractModel);
        Assert.Equal("anthropic/claude-x", back.VerifyModel);
        var c = Assert.Single(back.Candidates);
        Assert.Equal("Push fehlt", c.Summary);
        Assert.Equal(new[] { "requirements" }, c.SuggestedRelevantFor);
    }
}
