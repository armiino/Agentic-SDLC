using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class ClaimEvidenceVerifierTests
{
    [Theory]
    [InlineData("explicit", "grounded")]
    [InlineData("synthesis", "grounded")]
    [InlineData("permissible_inference", "grounded")]
    [InlineData("assumption", "violation")]
    [InlineData("overstated", "violation")]
    [InlineData("unsupported", "violation")]
    [InlineData("contradicted", "violation")]
    [InlineData("not_a_claim", "borderline")]
    public void LabelFor_Maps_Detailed_Verdicts_To_Review_Labels(string verdict, string expected)
        => Assert.Equal(expected, ClaimEvidenceVerifier.LabelFor(verdict));

    [Fact]
    public void Parse_Extracts_VerificationPayload_From_CodeFence()
    {
        var text = """
            ```json
            {
              "verdict": "overstated",
              "support": "partial",
              "modalityPreserved": false,
              "statusPreserved": true,
              "scopePreserved": true,
              "temporalContextPreserved": true,
              "reason": "Aus offen wurde geplant."
            }
            ```
            """;

        var parsed = ClaimEvidenceVerifier.Parse(text);

        Assert.NotNull(parsed);
        Assert.Equal("overstated", parsed!.Verdict);
        Assert.Equal("partial", parsed.Support);
        Assert.False(parsed.ModalityPreserved);
        Assert.Equal("Aus offen wurde geplant.", parsed.Reason);
    }

    [Fact]
    public void Parse_Unparseable_Returns_Null()
        => Assert.Null(ClaimEvidenceVerifier.Parse("kein json"));
}
