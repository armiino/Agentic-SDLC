using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z6.1-Nachweis: Der TopicCoverageMapper bewertet aggregierte, relevanz-gegatete Topics (statt roher
/// Turns) und erzeugt ein ReviewResult mit interpretierbarem Coverage-Nenner. Adressiert die B34-Treiber
/// (Over-Counting, Falschzuordnung).
/// </summary>
public sealed class TopicCoverageMapperTests
{
    private static readonly TopicItem[] Topics =
    {
        new("T-REQ-1", "Mehrwährung (EUR/CHF/USD) für Angebote", "unresolved", new[] { 10, 12 }, new[] { "requirements" }),
        new("T-RISK-1", "SAP-Wartungsfenster am Wochenende", "unresolved", new[] { 40 }, new[] { "risks", "architecture" }),
        new("T-REQ-2", "OAuth-Authentifizierung", "decision_open", new[] { 5 }, new[] { "requirements" }),
        new("T-REQ-3", "Audit-Log pro Angebot", "unresolved", new[] { 60 }, new[] { "requirements", "risks" }),
        new("T-REQ-4", "Datenklassifikation", "unresolved", new[] { 70 }, new[] { "requirements" })
    };

    private static readonly TopicVerdict[] Verdicts =
    {
        new("T-REQ-1", "missing", "Mehrwährung fehlt im Artefakt."),
        new("T-RISK-1", "missing", "(für requirements irrelevant — wird gegated)"),
        new("T-REQ-2", "covered", "OAuth ist im Artefakt."),
        new("T-REQ-3", "partial", "nur Audit-Trail erwähnt, ohne Felder."),
        new("T-REQ-4", "unclassified", "kein Verdikt")
    };

    [Fact]
    public void Relevance_Gate_Excludes_NonRelevant_Topics()
    {
        var r = TopicCoverageMapper.Map("requirements.md", "requirements", Topics, Verdicts, "judge");

        // T-RISK-1 (relevantFor risks/architecture) ist trotz "missing" KEIN Defekt für requirements.
        Assert.DoesNotContain(r.Defects, d => d.Description.Contains("SAP"));
        Assert.DoesNotContain(r.Defects, d => (d.SourceQuote ?? "").Contains("SAP"));
    }

    [Fact]
    public void Score_And_Rate_Over_Relevant_Topics()
    {
        var r = TopicCoverageMapper.Map("requirements.md", "requirements", Topics, Verdicts, "judge");

        // relevant für requirements: T-REQ-1..4 (4 Topics). missing = 1 (T-REQ-1).
        Assert.Equal(1, r.Metrics.CoverageScore);
        Assert.Equal(0.25, r.Metrics.MissingCoverageRate);   // 1 missing / 4 relevant
        Assert.Null(r.Metrics.ErrorScore);
        Assert.Null(r.Metrics.GroundingScore);
        Assert.Equal(new[] { ReviewAxis.Coverage }, r.EvaluatedAxes);
        Assert.Equal("topic-coverage-v1", r.EvaluatorVersion);
    }

    [Fact]
    public void Missing_And_Partial_Become_Defects_Covered_Does_Not()
    {
        var r = TopicCoverageMapper.Map("requirements.md", "requirements", Topics, Verdicts, "judge");

        // missing (Medium) + partial (Low) = 2 Defects; covered erzeugt keinen.
        Assert.Equal(2, r.Defects.Count);
        Assert.Contains(r.Defects, d => d.Category == "Coverage.MissingTopic" && d.Severity == DefectSeverity.Medium);
        Assert.Contains(r.Defects, d => d.Category == "Coverage.PartialTopic" && d.Severity == DefectSeverity.Low);
    }

    [Fact]
    public void Unclassified_Topic_Yields_Partial_Status()
    {
        var r = TopicCoverageMapper.Map("requirements.md", "requirements", Topics, Verdicts, "judge");
        Assert.Equal(ReviewStatus.Partial, r.Status);   // T-REQ-4 unclassified
        Assert.Contains(r.Diagnostics, d => d.Code == "TOPIC_UNCLASSIFIED");
    }
}
