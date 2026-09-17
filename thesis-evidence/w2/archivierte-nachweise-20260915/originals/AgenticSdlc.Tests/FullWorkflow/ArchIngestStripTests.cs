using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A1c+A1d (05.08.): der arch-Strip — Bridge-Typ-Routing (Leer-Skip vs. Kern-Start, DecisionScan-Muster)
// + Executor-ID-Golden (req-IDs WÖRTLICH wie vor der Parametrisierung = Checkpoint-Kompatibilität; arch eigene).
// Die Strip-Innereien (Resolve→Gate→Port→Apply) sind die graph-bewiesenen geteilten Kern-Komponenten;
// die Voll-Integration beweist der gemischte Echt-Lauf (Plan §A1c+A1d Verifikation ②).
public sealed class ArchIngestStripTests
{
    private static ProjectStateItem Item(string id, string type) => new ProjectStateItem(
        id, type, $"Text {id}", "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static IngestionApplyReport EmptyReport() => new([], [], new IngestionDeltaSummary(0, 0, 0, 0, 0, 0, 0, 0));

    private sealed class ReportSink(ConcurrentBag<string> hits) : Executor<IngestionApplyReport>("FakeScan")
    {
        public override ValueTask HandleAsync(IngestionApplyReport r, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add("report"); return ValueTask.CompletedTask; }
    }

    private sealed class ResolveSink(ConcurrentBag<string> hits) : Executor<IngestionResolveInput>("FakeArchResolve")
    {
        public override ValueTask HandleAsync(IngestionResolveInput i, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add($"resolve:{i.MeetingDelta.Items.Count(AspectIngestionProfile.Architecture.Matches)}"); return ValueTask.CompletedTask; }
    }

    private static async Task<(ConcurrentBag<string> Hits, string Events)> RunBridgeAsync(ProjectStateDocument delta, bool withCore)
    {
        var run = new RunContext(RunId.New(), "test-arch-bridge"); run.EnsureFolders();
        var repoRoot = Directory.CreateTempSubdirectory("arch-bridge-").FullName;
        await File.WriteAllTextAsync(Path.Combine(run.OutputDir("04-delta"), "project-state.json"),
            JsonSerializer.Serialize(delta, ProjectStateJson.Options));
        if (withCore)
        {
            Directory.CreateDirectory(Path.Combine(repoRoot, "state", "core"));
            await File.WriteAllTextAsync(Path.Combine(repoRoot, "state", "core", "project-state.json"),
                JsonSerializer.Serialize(Doc(Item("ARCH-01", "architecture")), ProjectStateJson.Options));
        }

        var hits = new ConcurrentBag<string>();
        var bridge = new ArchIngestBridgeExecutor(run, repoRoot, 2);
        var b = new WorkflowBuilder(bridge).WithName("ArchBridgeTest");
        b.AddEdge(bridge, new ReportSink(hits));
        b.AddEdge(bridge, new ResolveSink(hits));

        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(), EmptyReport(), run.RunId, CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);
        return (hits, await File.ReadAllTextAsync(run.EventsPath));
    }

    [Fact]
    public async Task Leer_Skip_reicht_den_Report_direkt_weiter_ohne_Kern_Start()
    {
        var (hits, events) = await RunBridgeAsync(Doc(Item("R-1", "requirement")), withCore: false);

        Assert.Equal("report", Assert.Single(hits));                    // Typ-Routing: direkt zum Scan
        Assert.Contains("ARCH_INGEST_SKIPPED", events);                 // laut, nicht still
    }

    [Fact]
    public async Task Arch_Items_starten_den_Kern_mit_frischem_Core()
    {
        var (hits, events) = await RunBridgeAsync(Doc(Item("R-1", "requirement"), Item("A-1", "architecture")), withCore: true);

        Assert.Equal("resolve:1", Assert.Single(hits));                 // genau der arch-Anteil zählt
        Assert.Contains("ARCH_INGEST_START", events);
    }

    [Fact]
    public void Kangal_supersedes_ist_aspekt_gleich_arch_arch_pass_und_cross_typ_faellt()
    {
        // D-7-Live-Fund (gemischter Echt-Lauf 05.08.): die supersedes-Regel war req-hart — der arch-Strip
        // superseded ARCH-Items. Spec-Erweiterung: Ablösung ist aspekt-GLEICH (Quelle.itemType == Ziel.itemType).
        ProjectStateRelation Rel(string from, string to) => new(from, to, "supersedes", "test", new Dictionary<string, string>());
        ProjectStateDocument With(ProjectStateRelation rel, params ProjectStateItem[] items)
            => new("p", 4, DateTime.UnixEpoch, [], [.. items], [rel], [], []);

        var archArch = AgenticSdlc.Host.FullWorkflow.Core.CoreKangal.Check(
            With(Rel("ARCH-2", "ARCH-1"), Item("ARCH-1", "architecture"), Item("ARCH-2", "architecture")));
        Assert.DoesNotContain(archArch.Errors, e => e.Code == "I1_TARGET_INVALID");

        var reqReq = AgenticSdlc.Host.FullWorkflow.Core.CoreKangal.Check(
            With(Rel("REQ-2", "REQ-1"), Item("REQ-1", "requirement"), Item("REQ-2", "requirement")));
        Assert.DoesNotContain(reqReq.Errors, e => e.Code == "I1_TARGET_INVALID");         // Alt-Verhalten erhalten

        var cross = AgenticSdlc.Host.FullWorkflow.Core.CoreKangal.Check(
            With(Rel("ARCH-2", "REQ-1"), Item("REQ-1", "requirement"), Item("ARCH-2", "architecture")));
        Assert.Contains(cross.Errors, e => e.Code == "I1_TARGET_INVALID");                // cross-Typ bleibt verboten
    }

    [Fact]
    public void Executor_IDs_req_woertlich_wie_bisher_arch_eigene()
    {
        var run = new RunContext(RunId.New(), "test-arch-bridge"); run.EnsureFolders();
        // Golden (Checkpoint-Kompatibilität): die req-IDs sind NACH der Profil-Parametrisierung byte-gleich.
        Assert.Equal("RequirementIngestionGate", new IngestionGateExecutor(run).Id);
        Assert.Equal("RequirementIngestionGate", new IngestionGateExecutor(run, AspectIngestionProfile.Requirement).Id);
        Assert.Equal("RequirementIngestionHitlFinalize", new IngestionHitlFinalizeExecutor(run, run.RunDir).Id);
        // arch: eigener Strip, eigene IDs (zweite Instanz im selben Graph).
        Assert.Equal("ArchitectureIngestionGate", new IngestionGateExecutor(run, AspectIngestionProfile.Architecture).Id);
        Assert.Equal("ArchitectureIngestionHitlFinalize", new IngestionHitlFinalizeExecutor(run, run.RunDir, AspectIngestionProfile.Architecture).Id);
        Assert.Equal("ARCH", AspectIngestionProfile.Architecture.IdPrefix);
        Assert.DoesNotContain("NEW_RELATED", AspectIngestionProfile.Architecture.ResolverTaskText.Split("KEIN NEW_RELATED")[0]); // Vokabular ohne NEW_RELATED
    }
}
