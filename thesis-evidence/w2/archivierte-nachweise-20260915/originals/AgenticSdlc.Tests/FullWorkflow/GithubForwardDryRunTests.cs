using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Run;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-48 (Fund Block F, 13.08., Lauf 20260813_131946): im Dry-Run lieferte die Agent-Bahn fuer unmapped PBIs
// [] -> das eigene Gate riss (PBI_NOT_ADDRESSED -> Repair -> leer -> MaxAttemptsReached) — JEDER Dry-Lauf mit
// unmapped aktivem PBI endete in der Sackgasse. Regel jetzt: Dry-Run plant ALLES (deterministischer CREATE ueber
// die R-27-Naht, ehrlich beschriftet, gate-konform), uebersprungen wird NUR der Write.
public sealed class GithubForwardDryRunTests
{
    private static GithubSyncEntry Unmapped() => new(
        PbiId: "PBI-042", Title: "Dunkelmodus mit manuellem Schalter bereitstellen", Status: "active",
        Readiness: "backlog_ready", CoveredRequirementIds: ["REQ-77"], BlockedByOpenDecision: false,
        GithubIssue: null, AcceptanceCriteria: ["Schalter in den Einstellungen"],
        Statement: "Als Nachtdienst will ich einen Dunkelmodus, damit das Display nicht blendet.");

    private static GithubForwardWfContext Ctx(bool dryRun) => new(
        Core: new ProjectStateDocument("t", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch, [], [], [], [], []),
        Entries: [Unmapped()],
        MappingByPbi: new Dictionary<string, GithubMappingRecord>(),
        Issues: [new GithubIssueSnapshot(1, null, "irgendein Issue", "Body", "open", [], null, null)],   // Snapshot NICHT leer
        Repository: "o/r", SourcePbiUpdateRun: "t", OutDir: Directory.CreateTempSubdirectory("fwd-dry-").FullName,
        SnapshotRel: null, DryRun: dryRun, MaxAttempts: 2);

    [Fact]
    public async Task DryRun_plant_deterministischen_CREATE_statt_leerem_Plan_und_besteht_das_Gate()
    {
        var unmapped = new[] { Unmapped() };
        var ops = await GithubForwardAgentRunner.RunAsync(Ctx(dryRun: true), unmapped,
            _ => throw new InvalidOperationException("Agent darf im Dry-Run NICHT gerufen werden (0 LLM)"),
            new RunContext(RunId.New(), "test-r48"), "task", CancellationToken.None);

        var op = Assert.Single(ops);                                        // DER Fix: nicht mehr leer
        Assert.Equal(GithubForwardKind.CreateIssue, op.Kind);
        Assert.Contains("Dry-Run", op.Rationale);                           // ehrlich beschriftet (kein Erst-Sync-Text)
        Assert.Contains("Dry-Run", op.SearchEvidence);

        // Und der Plan besteht das ECHTE Gate (frueher: PBI_NOT_ADDRESSED -> MaxAttemptsReached-Sackgasse).
        var plan = new GithubForwardPlanDocument(GithubForwardPlanDocument.CurrentSchemaVersion,
            "plan-r48", DateTime.UnixEpoch, "t", "o/r", ops);
        var report = GithubForwardGate.Check(plan, unmapped, Ctx(true).Issues);
        Assert.True(report.Pass, string.Join("; ", report.Errors.Select(e => e.Code + ":" + e.Message)));
    }
}
