using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimCoverageClassifierTests
{
    [Theory]
    [InlineData("covered", "covered")]
    [InlineData("PARTIAL", "partial")]
    [InlineData("missing", "missing")]
    [InlineData("contradicted", "contradicted")]
    [InlineData("not_applicable", "not_applicable")]
    [InlineData("???", "unclassified")]
    public void NormalizeVerdict_Normalizes_Known_Labels(string input, string expected)
        => Assert.Equal(expected, SourceClaimCoverageClassifier.NormalizeVerdict(input));

    [Fact]
    public void Parse_Reads_Payload_From_CodeFence()
    {
        var text = """
            ```json
            {
              "verdict": "partial",
              "support": "partial",
              "artifactQuote": "Skalierbarkeit wird erwaehnt.",
              "missingFacet": "Nutzerzahl 200 bis 20.000 fehlt.",
              "reason": "Nur grob behandelt."
            }
            ```
            """;

        var parsed = SourceClaimCoverageClassifier.Parse(text);

        Assert.NotNull(parsed);
        Assert.Equal("partial", parsed!.Verdict);
        Assert.Equal("partial", parsed.Support);
        Assert.Equal("Nutzerzahl 200 bis 20.000 fehlt.", parsed.MissingFacet);
    }

    [Fact]
    public void Parse_Bad_Json_Returns_Null()
        => Assert.Null(SourceClaimCoverageClassifier.Parse("kein json"));
}
