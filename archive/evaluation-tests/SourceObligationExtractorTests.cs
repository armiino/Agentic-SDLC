using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class SourceObligationExtractorTests
{
    [Fact]
    public void Parse_Reads_SourceObligations()
    {
        var text = """
            {
              "obligations": [
                {
                  "sourceClaim": "Skalierung ist wegen 20 bis 20.000 Nutzern offen.",
                  "artifact": "architecture",
                  "expectedRepresentation": "open_question",
                  "requiredTreatment": "Als offene Architekturentscheidung behandeln.",
                  "evidence": ["Ben: Sind das 200 oder 20.000 Nutzer?"],
                  "kind": "open_question",
                  "importance": "high"
                }
              ]
            }
            """;

        var obligations = SourceObligationExtractor.Parse(text);

        Assert.Single(obligations);
        Assert.Equal("architecture", obligations[0].Artifact);
        Assert.Equal("open_question", obligations[0].ExpectedRepresentation);
        Assert.Contains("20.000", obligations[0].SourceClaim);
    }

    [Fact]
    public void Parse_Normalizes_OpenQuestions_Artifact()
    {
        var text = """
            {
              "obligations": [
                {
                  "sourceClaim": "Push ist offen.",
                  "artifact": "open_questions",
                  "expectedRepresentation": "open_question",
                  "requiredTreatment": "Als offene Frage behandeln.",
                  "evidence": ["Anna: Vielleicht später."],
                  "kind": "open_question",
                  "importance": "medium"
                }
              ]
            }
            """;

        var obligations = SourceObligationExtractor.Parse(text);

        Assert.Equal("open-questions", obligations[0].Artifact);
    }
}
