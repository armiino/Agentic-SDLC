using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Z7-Nachweis: Die deterministische Audit-Logik — formale Checks (Dupes, Status, relevantFor,
/// sourceTurns-Bereich) und der Jury-Cross-Check (Capture-Recapture: Jury-MISSING ohne Topic-Match
/// = Completeness-Kandidat).
/// </summary>
public sealed class TopicAuditTests
{
    [Fact]
    public void CheckFormal_Flags_Dupes_Status_RelevantFor_OutOfRange()
    {
        var set = new TopicSet("T.txt", "m", new[]
        {
            new TopicItem("T-1", "ok", "unresolved", new[] { 0, 5 }, new[] { "requirements" }),
            new TopicItem("T-1", "dup", "unresolved", new[] { 1 }, new[] { "risks" }),          // DUPLICATE_ID
            new TopicItem("T-2", "bad status", "irgendwas", new[] { 2 }, new[] { "risks" }),     // BAD_STATUS
            new TopicItem("T-3", "bad rel", "resolved", new[] { 3 }, new[] { "foobar" }),        // BAD_RELEVANTFOR
            new TopicItem("T-4", "oob", "unresolved", new[] { 999 }, new[] { "architecture" }),  // SOURCETURN_OUT_OF_RANGE
        });

        var issues = TopicAudit.CheckFormal(set, turnCount: 10);
        var codes = issues.Select(i => i.Code).ToHashSet();

        Assert.Contains("DUPLICATE_ID", codes);
        Assert.Contains("BAD_STATUS", codes);
        Assert.Contains("BAD_RELEVANTFOR", codes);
        Assert.Contains("SOURCETURN_OUT_OF_RANGE", codes);
    }

    [Fact]
    public void CheckFormal_Clean_Fixture_Has_No_Issues()
    {
        var set = new TopicSet("T.txt", "m", new[]
        {
            new TopicItem("T-1", "ok", "unresolved", new[] { 0, 5 }, new[] { "requirements", "risks" })
        });
        Assert.Empty(TopicAudit.CheckFormal(set, turnCount: 10));
    }

    [Fact]
    public void CrossCheck_Flags_Unmatched_Jury_Missing_As_Candidate()
    {
        var topics = new[]
        {
            new TopicItem("TOPIC-TEST-UMGEBUNGEN", "Dev-, Test- und Prod-Umgebungen sowie Testdaten", "unresolved", new[] { 1 }, new[] { "requirements" }),
            new TopicItem("TOPIC-RATE-LIMITING", "API Rate Limits und Pagination", "unresolved", new[] { 2 }, new[] { "requirements" }),
        };
        var juryMissing = new[]
        {
            "Transkript: \"Wir brauchen Umgebungen. Dev, Test, Prod. Testdaten\" | fehlt",   // -> matcht TEST-UMGEBUNGEN
            "Transkript: \"Wir brauchen einen Architekten. Wir haben keinen.\" | fehlt",     // -> kein Topic = Kandidat
        };

        var hits = TopicAudit.CrossCheckJuryMissing(topics, juryMissing);

        Assert.Equal(2, hits.Count);
        var umgebung = hits[0];
        Assert.False(umgebung.CompletenessCandidate);
        Assert.Equal("TOPIC-TEST-UMGEBUNGEN", umgebung.BestTopicId);

        var architekt = hits[1];
        Assert.True(architekt.CompletenessCandidate);   // kein passendes Topic
        Assert.Null(architekt.BestTopicId);
    }
}
