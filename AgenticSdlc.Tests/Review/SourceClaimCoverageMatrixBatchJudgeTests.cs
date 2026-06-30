using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceClaimCoverageMatrixBatchJudgeTests
{
    [Fact]
    public void Parse_Reads_Batch_Payload()
    {
        var text = """
            {
              "items": [
                {
                  "id": "SC-1",
                  "applicability": "required",
                  "coverage": "partial",
                  "support": "partial",
                  "artifactQuote": "KPI-Messung ist zu konkretisieren.",
                  "presentFacets": ["KPI-Messung"],
                  "missingFacets": ["Tracking-Details"],
                  "reason": "Teilweise enthalten."
                }
              ]
            }
            """;

        var parsed = SourceClaimCoverageMatrixBatchJudge.Parse(text)!;
        var item = parsed.Items.Single().Normalized();

        Assert.Equal("SC-1", item.Id);
        Assert.Equal("required", item.Applicability);
        Assert.Equal("partial", item.Coverage);
        Assert.Single(item.PresentFacets);
        Assert.Single(item.MissingFacets);
    }
}
