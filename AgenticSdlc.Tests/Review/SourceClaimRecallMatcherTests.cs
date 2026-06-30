using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimRecallMatcherTests
{
    [Theory]
    [InlineData("exact", "exact")]
    [InlineData("PARTIAL", "partial")]
    [InlineData("missed", "missed")]
    [InlineData("x", "unclassified")]
    public void Normalize_Maps_Known_Labels(string input, string expected)
        => Assert.Equal(expected, SourceClaimRecallMatcher.Normalize(input));

    [Fact]
    public void Parse_Reads_Payload()
    {
        var parsed = SourceClaimRecallMatcher.Parse("""
            {"verdict":"partial","matchedIds":["ARCH-SC-001"],"reason":"grober Claim vorhanden"}
            """);

        Assert.NotNull(parsed);
        Assert.Equal("partial", parsed!.Verdict);
        Assert.Equal("ARCH-SC-001", Assert.Single(parsed.MatchedIds!));
    }

    [Fact]
    public void Parse_Bad_Json_Returns_Null()
        => Assert.Null(SourceClaimRecallMatcher.Parse("nope"));
}
