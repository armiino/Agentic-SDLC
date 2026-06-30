using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimExtractorTests
{
    [Fact]
    public void Parse_Reads_SourceClaims()
    {
        var text = """
            {
              "claims": [
                {
                  "sourceClaim": "Push Notifications sind nicht im MVP entschieden.",
                  "requiredTreatment": "Als offene Scope-Frage erfassen.",
                  "evidence": ["Anna: Vielleicht spaeter.", "Ben: Nie besprochen."],
                  "kind": "status",
                  "priority": "high"
                }
              ]
            }
            """;

        var claims = SourceClaimExtractor.Parse(text);

        Assert.Single(claims);
        Assert.Equal("Push Notifications sind nicht im MVP entschieden.", claims[0].SourceClaim);
        Assert.Equal(2, claims[0].Evidence.Count);
        Assert.Equal("status", claims[0].Kind);
    }

    [Fact]
    public void Parse_Bad_Json_Returns_Empty()
        => Assert.Empty(SourceClaimExtractor.Parse("kein json"));
}
