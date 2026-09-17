using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// ONLY-STEWARD Re-Projektion (steward/reprojektion-only-steward.md, 11.08.): der Recovery-Eingang leitet das
// Sync-Delta AUS DEM CORE ab (truth-first, KEIN Run-Ordner) und filtert auf gemappte PBIs (1:1-UPDATE-Reconcile,
// kein L4-all-vs-all). Der Wächter-Test nagelt genau das fest: Delta = gemappte Core-PBIs, unmapped bleibt draußen,
// ein abweichender sync-delta im Run-Ordner wird NICHT gelesen (Chronik-vs-Wahrheit-Auflage des Autors, 11.08.).
public sealed class ReprojectEntryTests
{
    private static ProjectStateItem Pbi(string id, string text, string? githubIssue = null) => new ProjectStateItem(
        id, "pbi", text, "test", null, 1, null, null, null, null, null, [], [],
        githubIssue is null ? new Dictionary<string, string>() : new Dictionary<string, string> { ["githubIssue"] = githubIssue },
        Pbi: new PbiPayload(Goal: "Ziel", Title: text, AcceptanceCriteria: ["AK1"], LinkedRequirementIds: [],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "backlog_ready", Mvp: null, Trace: null))
        .WithStatus(CoreStatus.From("active"));

    private static string WriteCore(params ProjectStateItem[] items)
    {
        var repoRoot = Directory.CreateTempSubdirectory("reproj-").FullName;
        var core = new ProjectStateDocument("reproj-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch,
            Sources: [], Items: [.. items], Relations: [], Provenance: [], Proposals: []);
        var file = CorePaths.CoreFile(repoRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, JsonSerializer.Serialize(core, ProjectStateJson.Options));
        return repoRoot;
    }

    private sealed class ForwardPrepSink(ConcurrentBag<ForwardPrep> hits) : Executor<ForwardPrep>("Sink")
    {
        public override ValueTask HandleAsync(ForwardPrep p, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(p); return ValueTask.CompletedTask; }
    }

    private static async Task<(IReadOnlyList<ForwardPrep> Preps, IReadOnlyList<string> Outputs)> RunReproject(string repoRoot)
    {
        var run = new RunContext(RunId.New(), "reproj-test");
        run.EnsureFolders();
        var entry = new ReprojectEntryExecutor(run, repoRoot);
        var hits = new ConcurrentBag<ForwardPrep>();
        var b = new WorkflowBuilder(entry).WithName("ReprojectTest");
        b.AddEdge(entry, new ForwardPrepSink(hits));
        b.WithOutputFrom(entry);
        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(), new ReprojectRequest(), run.RunId, CancellationToken.None);
        var outputs = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data?.ToString() ?? "").ToList();
        return (hits.ToList(), outputs);
    }

    [Fact]
    public async Task Delta_kommt_AUS_dem_Core_und_ist_gemappt_only()
    {
        var repoRoot = WriteCore(
            Pbi("PBI-012", "Profil-Detailansicht", githubIssue: "#12"),   // gemappt -> UPDATE-Kandidat
            Pbi("PBI-099", "Neu, kein Issue"));                            // unmapped -> bleibt draußen

        // Chronik-Falle: ein ABWEICHENDER sync-delta im Run-Ordner darf NICHT gelesen werden (truth-first-Wächter).
        var runsDelta = Path.Combine(repoRoot, "runs", "fullworkflow", "alt", "07-pbi-update", "applied");
        Directory.CreateDirectory(runsDelta);
        File.WriteAllText(Path.Combine(runsDelta, "github-sync-delta.json"),
            """{"newPbis":["PBI-099"],"updatedPbis":[],"entries":[{"pbiId":"PBI-099","title":"x","status":"active","coveredRequirementIds":[],"blockedByOpenDecision":false,"githubIssue":null}]}""");

        var (preps, _) = await RunReproject(repoRoot);

        var prep = Assert.Single(preps);
        Assert.Equal(["PBI-012"], prep.Delta.Entries.Select(e => e.PbiId).ToList());   // NUR gemappt, aus dem CORE (nicht runs/ PBI-099!)
        Assert.Equal("#12", Assert.Single(prep.Delta.Entries).GithubIssue);
        Assert.Equal(["PBI-012"], prep.Delta.UpdatedPbis);
        Assert.Empty(prep.Delta.NewPbis);
        Assert.False(prep.InitialSync);
    }

    [Fact]
    public async Task Kein_gemapptes_PBI_ist_terminaler_Stop_ohne_ForwardPrep()
    {
        var repoRoot = WriteCore(Pbi("PBI-099", "Neu, kein Issue"));   // nichts gemappt
        var (preps, outputs) = await RunReproject(repoRoot);
        Assert.Empty(preps);                                          // KEIN Forward
        Assert.Contains(outputs, o => o.Contains("keine gemappten PBIs"));  // sichtbarer terminaler Stop (kein stiller Skip)
    }
}
