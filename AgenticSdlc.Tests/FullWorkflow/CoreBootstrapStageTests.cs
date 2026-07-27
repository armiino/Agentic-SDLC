using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B1 (pipeline-full-bootstrap-plan §7): der pure core-bootstrap-Kern des Bootstrap-Zweigs —
// Seed → Save → Baseline-Triplet. Der „Core existiert"-Fall ist ein LAUTER Abbruch (kein Überschreiben).
public sealed class CoreBootstrapStageTests
{
    private static ProjectStateItem Req(string id, string text) => new(
        id, "requirement", text, "accepted", "test", null, 0,
        null, null, null, null, null, [], [], new Dictionary<string, string>());

    private static ProjectStateDocument Doc() => new(
        "boot-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch,
        Sources: [], Items: [Req("REQ-1", "Die App speichert Daten lokal.")],
        Relations: [], Provenance: [], Proposals: []);

    [Fact]
    public async Task ExecuteAsync_seedet_core_und_schreibt_baseline_triplet()
    {
        var root = Path.Combine(Path.GetTempPath(), $"boot-{Guid.NewGuid():N}");
        try
        {
            var outDir = Path.Combine(root, "runs", "fw", "05-core");
            var result = await CoreBootstrapStage.ExecuteAsync(root, outDir, Doc());

            Assert.True(File.Exists(Path.Combine(root, "state", "core", "project-state.json")));
            Assert.True(File.Exists(result.BaselinePath));
            Assert.True(File.Exists(Path.Combine(outDir, "provenance-map.json")));
            Assert.True(File.Exists(Path.Combine(outDir, "quality-report.json")));
            Assert.Equal(1, result.CoreItems);
            Assert.Equal(1, result.Requirements);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ExecuteAsync_bricht_bei_existierendem_core_LAUT_ab()
    {
        var root = Path.Combine(Path.GetTempPath(), $"boot-{Guid.NewGuid():N}");
        try
        {
            await CoreBootstrapStage.ExecuteAsync(root, Path.Combine(root, "out1"), Doc());
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CoreBootstrapStage.ExecuteAsync(root, Path.Combine(root, "out2"), Doc()));
            Assert.Contains("BOOTSTRAP_ABORTED_CORE_EXISTS", ex.Message);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
