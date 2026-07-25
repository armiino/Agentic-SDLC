using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-26 / Option B: unmapped + needs_clarify wird im unbeaufsichtigten Lauf (holdUnclearNewPbis=true) deterministisch
// GEPARKT (HOLD_CLARIFY) statt agentisch CREATE zu erzwingen — und erfuellt damit die Gate-Coverage (kein
// PBI_NOT_ADDRESSED mehr). Flag aus (Default) = proven behavior unveraendert (bleibt agentischer unmapped-Fall).
public sealed class GithubForwardSeedClarifyTests
{
    private static GithubSyncEntry Entry(string pbi, string status) =>
        new(pbi, $"Titel {pbi}", status, "ready", [$"REQ-{pbi}"], false, null);

    private static Dictionary<string, GithubMappingRecord> Mapped(string pbi, int issue) =>
        new(StringComparer.Ordinal) { [pbi] = new GithubMappingRecord(pbi, issue, $"u/{issue}", null, "open") };

    private static readonly Dictionary<string, GithubMappingRecord> NoMappings = new(StringComparer.Ordinal);
    private static readonly List<GithubIssueSnapshot> NoIssues = [];

    private static GithubForwardPlanDocument PlanOf(IEnumerable<GithubForwardOp> ops) =>
        new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourcePbiUpdateRun: "src", Repository: "o/n", Operations: ops.ToList());

    [Fact]
    public void Unmapped_needs_clarify_mit_Flag_wird_HOLD_CLARIFY_und_nicht_unmapped()
    {
        var entries = new[] { Entry("PBI-1", "needs_clarify") };

        var seed = GithubForwardSeed.Seed(entries, NoMappings, NoIssues, holdUnclearNewPbis: true);

        Assert.Empty(seed.UnmappedPbis); // NICHT an den Agenten weitergereicht
        var op = Assert.Single(seed.DeterministicOps);
        Assert.Equal(GithubForwardKind.HoldClarify, op.Kind);
        Assert.Equal("PBI-1", op.PbiId);
        Assert.Null(op.TargetIssueNumber);
        Assert.Equal("deterministic", op.Origin);
    }

    [Fact]
    public void Unmapped_needs_clarify_ohne_Flag_bleibt_agentischer_unmapped_Fall()
    {
        var entries = new[] { Entry("PBI-1", "needs_clarify") };

        var seed = GithubForwardSeed.Seed(entries, NoMappings, NoIssues); // Default holdUnclearNewPbis=false

        Assert.Empty(seed.DeterministicOps);
        var e = Assert.Single(seed.UnmappedPbis);
        Assert.Equal("PBI-1", e.PbiId);
    }

    [Fact]
    public void Unmapped_active_bleibt_auch_mit_Flag_agentisch()
    {
        var entries = new[] { Entry("PBI-1", "active") };

        var seed = GithubForwardSeed.Seed(entries, NoMappings, NoIssues, holdUnclearNewPbis: true);

        Assert.Empty(seed.DeterministicOps); // nur needs_clarify wird geparkt, active nicht
        Assert.Single(seed.UnmappedPbis);
    }

    [Fact]
    public void Gemapptes_needs_clarify_bleibt_UPDATE_ISSUE_trotz_Flag()
    {
        var entries = new[] { Entry("PBI-1", "needs_clarify") };
        var issues = new List<GithubIssueSnapshot> { new(42, "u/42", "t", "b", "open", [], null, DateTime.UnixEpoch) };

        var seed = GithubForwardSeed.Seed(entries, Mapped("PBI-1", 42), issues, holdUnclearNewPbis: true);

        Assert.Empty(seed.UnmappedPbis);
        var op = Assert.Single(seed.DeterministicOps);
        Assert.Equal(GithubForwardKind.UpdateIssue, op.Kind); // gemappt -> UPDATE, NICHT geparkt
    }

    [Fact]
    public void HOLD_CLARIFY_erfuellt_Gate_Coverage_kein_PBI_NOT_ADDRESSED()
    {
        var entries = new[] { Entry("PBI-1", "needs_clarify") };
        var seed = GithubForwardSeed.Seed(entries, NoMappings, NoIssues, holdUnclearNewPbis: true);

        // Der geparkte Op ist der einzige Op im Plan — Coverage muss erfuellt sein (genau 1 Op je Delta-PBI).
        var report = GithubForwardGate.Check(PlanOf(seed.DeterministicOps), entries, NoIssues);

        Assert.True(report.Pass);
        Assert.Empty(report.Errors);
        Assert.DoesNotContain(report.Errors, e => e.Code == "PBI_NOT_ADDRESSED");
    }

    [Fact]
    public void Ohne_Flag_und_ohne_Agent_faellt_PBI_durch_die_Coverage()
    {
        // Beleg der Sackgasse VOR B: ohne Flag bleibt das PBI unmapped; ohne Agent-Op ist der Plan leer -> Gate blockt.
        var entries = new[] { Entry("PBI-1", "needs_clarify") };
        var seed = GithubForwardSeed.Seed(entries, NoMappings, NoIssues); // Flag aus

        var report = GithubForwardGate.Check(PlanOf(seed.DeterministicOps), entries, NoIssues);

        Assert.False(report.Pass);
        Assert.Contains(report.Errors, e => e.Code == "PBI_NOT_ADDRESSED");
    }

    [Fact]
    public void HoldClarify_ist_bekannte_Op_Art()
        => Assert.Contains(GithubForwardKind.HoldClarify, GithubForwardKind.All);
}
