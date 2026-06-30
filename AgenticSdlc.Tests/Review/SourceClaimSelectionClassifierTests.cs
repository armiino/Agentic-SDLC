using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimSelectionClassifierTests
{
    [Fact]
    public void Parse_Reads_Selections()
    {
        var text = """
            {
              "selections": [
                {
                  "sourceClaimId": "GLOBAL-SC-034",
                  "selection": "must_check",
                  "expectedRepresentation": "open_question",
                  "importance": "high",
                  "reason": "Skalierung ist eine harte Architekturpflicht."
                }
              ]
            }
            """;

        var selections = SourceClaimSelectionClassifier.Parse(text);

        Assert.Single(selections);
        Assert.Equal("GLOBAL-SC-034", selections[0].SourceClaimId);
        Assert.Equal("must_check", selections[0].Selection);
    }

    [Theory]
    [InlineData("must_check", "must_check")]
    [InlineData("Should_Check", "should_check")]
    [InlineData("bad", "context_only")]
    public void NormalizeSelection_Uses_Known_Values(string input, string expected)
        => Assert.Equal(expected, SourceClaimSelectionClassifier.NormalizeSelection(input));
}
