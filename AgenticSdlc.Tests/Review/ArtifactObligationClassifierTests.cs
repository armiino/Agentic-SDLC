using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class ArtifactObligationClassifierTests
{
    [Fact]
    public void Parse_Reads_Obligations()
    {
        var text = """
            {
              "obligations": [
                {
                  "sourceClaimId": "GLOBAL-SC-034",
                  "necessity": "required",
                  "expectedRepresentation": "open_question",
                  "importance": "high",
                  "reason": "Skalierung ist eine offene Architekturentscheidung."
                }
              ]
            }
            """;

        var obligations = ArtifactObligationClassifier.Parse(text);

        Assert.Single(obligations);
        Assert.Equal("GLOBAL-SC-034", obligations[0].SourceClaimId);
        Assert.Equal("required", obligations[0].Necessity);
        Assert.Equal("open_question", obligations[0].ExpectedRepresentation);
    }

    [Theory]
    [InlineData("Required", "required")]
    [InlineData("context_only", "context_only")]
    [InlineData("weird", "unclear")]
    public void NormalizeNecessity_Uses_Known_Values(string input, string expected)
        => Assert.Equal(expected, ArtifactObligationClassifier.NormalizeNecessity(input));
}
