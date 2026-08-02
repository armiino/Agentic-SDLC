using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B5 / R-15 (pipeline-full-bootstrap-plan §6/§7): der deterministische initial-sync — Voll-CREATE-Delta aus
// der github-sync-Core-View. Gemappte und archivierte PBIs bleiben draußen; das Format ist exakt das
// pbi-update-Delta (GithubSyncDeltaDocument), damit Forward-Gate + Apply unverändert konsumieren.
public sealed class GithubInitialSyncTests
{
    private static ProjectStateItem Pbi(string id, string text, string status = "active", string? githubIssue = null,
        IReadOnlyList<string>? acceptance = null, string? goal = null) => new ProjectStateItem(
        id, "pbi", text, "test", null, 1,
        null, null, null, null, null, [], [],
        githubIssue is null ? new Dictionary<string, string>() : new Dictionary<string, string> { ["githubIssue"] = githubIssue },
        Pbi: new PbiPayload(Goal: goal, Title: text, AcceptanceCriteria: acceptance ?? [], LinkedRequirementIds: [],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "backlog_ready", Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Core(params ProjectStateItem[] items) => new(
        "sync-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch,
        Sources: [], Items: [.. items], Relations: [], Provenance: [], Proposals: []);

    [Fact]
    public void BuildDelta_nimmt_nur_unmapped_aktive_pbis()
    {
        var core = Core(
            Pbi("PBI-001", "Neu, kein Issue"),
            Pbi("PBI-002", "Schon gemappt", githubIssue: "#7"),
            Pbi("PBI-003", "Erledigt", status: "done"));

        var delta = GithubInitialSync.BuildDelta(core);

        Assert.Equal(["PBI-001"], delta.NewPbis);
        Assert.Empty(delta.UpdatedPbis);
        var entry = Assert.Single(delta.Entries);
        Assert.Equal("PBI-001", entry.PbiId);
        Assert.Null(entry.GithubIssue);
    }

    [Fact]
    public void BuildDelta_auf_leerem_core_ist_leer()
    {
        var delta = GithubInitialSync.BuildDelta(Core());
        Assert.Empty(delta.NewPbis);
        Assert.Empty(delta.Entries);
    }

    // R-27: der volle deterministische Erst-Sync-Pfad — Seed-Vorfilter (HOLD-Governance bleibt!) + CREATE-Ops
    // aus den Core-Payloads. Muss das ECHTE Forward-Gate bestehen (Coverage, Search-Evidenz, kein CREATE für
    // blockierte/geparkte PBIs).
    [Fact]
    public void BuildCreateOps_nach_Seed_bestehen_das_echte_forward_gate()
    {
        var core = Core(
            Pbi("PBI-001", "Normales neues PBI", acceptance: ["Login klappt mit 2FA", "Fehlerfall zeigt Meldung"],
                goal: "Als Kunde will ich mich sicher anmelden, damit meine Daten geschuetzt sind."),
            Pbi("PBI-002", "Blockiertes PBI", status: "blocked_by_decision"),
            Pbi("PBI-003", "Unklares neues PBI", status: "needs_clarify"));
        var delta = GithubInitialSync.BuildDelta(core);

        var seeded = GithubForwardSeed.Seed(delta.Entries,
            new Dictionary<string, AgenticSdlc.Host.FullWorkflow.Core.GithubMappingRecord>(), [], holdUnclearNewPbis: true);
        var ops = seeded.DeterministicOps.Concat(GithubInitialSync.BuildCreateOps(seeded.UnmappedPbis)).ToList();

        Assert.Contains(ops, o => o.Kind == GithubForwardKind.HoldBlocked && o.PbiId == "PBI-002");
        Assert.Contains(ops, o => o.Kind == GithubForwardKind.HoldClarify && o.PbiId == "PBI-003");
        var create = Assert.Single(ops, o => o.Kind == GithubForwardKind.CreateIssue);
        Assert.Equal("PBI-001", create.PbiId);
        Assert.Equal("deterministic", create.Origin);
        Assert.Contains("Initial-Sync", create.Body);
        // E0.1c/R-23: Statement (das WARUM) + Akzeptanzkriterien aus dem Core-Payload stehen im
        // deterministischen Issue-Body.
        Assert.Contains("Als Kunde will ich mich sicher anmelden", create.Body);
        Assert.Contains("Akzeptanzkriterien:", create.Body);
        Assert.Contains("- Login klappt mit 2FA", create.Body);

        var plan = new GithubForwardPlanDocument(GithubForwardPlanDocument.CurrentSchemaVersion,
            "plan-test", DateTime.UnixEpoch, "test", null, ops);
        var report = GithubForwardGate.Check(plan, delta.Entries, []);
        Assert.True(report.Pass, string.Join("; ", report.Errors.Select(e => e.Code + ":" + e.Message)));
    }
}
