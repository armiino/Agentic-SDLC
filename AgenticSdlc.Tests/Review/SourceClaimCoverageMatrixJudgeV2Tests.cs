using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimCoverageMatrixJudgeV2Tests
{
    [Fact]
    public void Parse_Reads_V2_Payload()
    {
        var text = """
            {
              "applicability": "required",
              "coverage": "partial",
              "support": "partial",
              "artifactQuote": "Skalierbarkeit wird erwaehnt.",
              "presentFacets": ["Skalierbarkeit"],
              "missingFacets": ["offene Nutzerzahl"],
              "reason": "Nur grob behandelt."
            }
            """;

        var parsed = SourceClaimCoverageMatrixJudgeV2.Parse(text)!.Normalized();

        Assert.Equal("required", parsed.Applicability);
        Assert.Equal("partial", parsed.Coverage);
        Assert.Single(parsed.PresentFacets);
        Assert.Single(parsed.MissingFacets);
    }

    [Theory]
    [InlineData("context_only", "context")]
    [InlineData("not_applicable", "not_applicable")]
    [InlineData("bad", "unclear")]
    public void NormalizeApplicability_Handles_Known_Values(string input, string expected)
        => Assert.Equal(expected, SourceClaimCoverageMatrixJudgeV2.NormalizeApplicability(input));
}
