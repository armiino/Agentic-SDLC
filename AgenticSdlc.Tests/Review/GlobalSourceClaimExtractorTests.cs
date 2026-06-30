using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class GlobalSourceClaimExtractorTests
{
    [Fact]
    public void Parse_Reads_Global_SourceClaims()
    {
        var text = """
            {
              "claims": [
                {
                  "sourceClaim": "Push Notifications sind nicht im MVP entschieden.",
                  "requiredTreatment": "Als Scope-Frage und Risiko behandeln.",
                  "evidence": ["Anna: Vielleicht spaeter.", "Ben: Nie besprochen."],
                  "kind": "status",
                  "priority": "high",
                  "relevantFor": ["requirements", "open-questions", "risks"]
                }
              ]
            }
            """;

        var claims = GlobalSourceClaimExtractor.Parse(text);

        Assert.Single(claims);
        Assert.Equal("Push Notifications sind nicht im MVP entschieden.", claims[0].SourceClaim);
        Assert.Contains("requirements", claims[0].RelevantFor);
        Assert.Contains("open-questions", claims[0].RelevantFor);
        Assert.Equal(2, claims[0].Evidence.Count);
    }

    [Fact]
    public void Parse_Bad_Json_Returns_Empty()
        => Assert.Empty(GlobalSourceClaimExtractor.Parse("kein json"));
}
