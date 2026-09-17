using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Core-Haertung (Log #38): SaveAsync legt den BISHERIGEN Stand vor dem Ueberschreiben nach
// state/core/history/ — aber nur bei echter Aenderung (0-Ops-Applies stapeln keine Kopien).
public sealed class JsonCoreRepositoryHistoryTests
{
    private static ProjectStateDocument Doc(string projectId) => new(
        projectId, ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch, [], [], [], [], []);

    [Fact]
    public async Task Save_auf_bestehendem_Core_legt_Snapshot_des_alten_Stands_ab()
    {
        var root = Path.Combine(Path.GetTempPath(), $"core-hist-{Guid.NewGuid():N}");
        try
        {
            var repo = new JsonCoreRepository(root);
            await repo.SaveAsync(Doc("v1"));                       // Erst-Save: noch keine History
            var historyDir = Path.Combine(root, "state", "core", "history");
            Assert.False(Directory.Exists(historyDir));

            await repo.SaveAsync(Doc("v2"));                       // echte Aenderung -> Snapshot von v1
            var snapshots = Directory.GetFiles(historyDir, "project-state.*.json");
            var snap = Assert.Single(snapshots);
            Assert.Contains("\"v1\"", await File.ReadAllTextAsync(snap));
            Assert.Equal("v2", (await repo.LoadAsync()).ProjectId);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Identischer_Save_erzeugt_KEINEN_Snapshot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"core-hist-{Guid.NewGuid():N}");
        try
        {
            var repo = new JsonCoreRepository(root);
            await repo.SaveAsync(Doc("gleich"));
            await repo.SaveAsync(Doc("gleich"));                   // inhaltsgleich -> kein History-Ordner
            Assert.False(Directory.Exists(Path.Combine(root, "state", "core", "history")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
