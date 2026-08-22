using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Slice S (21.08., Autor-Idee „lebende PBI-Tabelle"): docs/backlog.md als deterministische
// Core-Projektion — je Feature eine Tabelle; Prio/Schätzung lesen die Teil-2-Felder (bis dahin „—").
public sealed class BacklogDocumentProjectionTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted",
        Dictionary<string, string>? meta = null)
        => new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, [], [],
            meta ?? new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    [Fact]
    public async Task RunAsync_schreibt_nur_bei_echter_Inhalts_Aenderung()   // R-72
    {
        var root = Path.Combine(Path.GetTempPath(), "blg-r72-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        try
        {
            var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
                [Item("PBI-1", "pbi", "Arbeitspaket")], [], [], []);
            await new JsonCoreRepository(root).SaveAsync(core);
            var (path, v1) = await BacklogDocumentProjection.RunAsync(root, DateTime.UnixEpoch);
            Assert.Equal(1, v1);

            // Fremde Wahrheits-Änderung (DEC-Schließung o. ä.): Backlog-Inhalt unberührt ⇒ KEIN Write, Version bleibt.
            await new JsonCoreRepository(root).SaveAsync(core with
            { Items = [.. core.Items, Item("DEC-001", "decision", "Frage?", "resolved")] });
            var vorher = File.ReadAllText(path);
            var (_, v2) = await BacklogDocumentProjection.RunAsync(root, DateTime.UnixEpoch.AddDays(1));
            Assert.Equal(1, v2);
            Assert.Equal(vorher, File.ReadAllText(path));                      // byte-identisch ⇒ kein Push

            // Echte Inhalts-Änderung (neues PBI) ⇒ Write + Version 2.
            await new JsonCoreRepository(root).SaveAsync(core with
            { Items = [.. core.Items, Item("PBI-2", "pbi", "Neues Paket")] });
            var (_, v3) = await BacklogDocumentProjection.RunAsync(root, DateTime.UnixEpoch.AddDays(2));
            Assert.Equal(2, v3);
            Assert.Contains("PBI-2", File.ReadAllText(path));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Render_gruppiert_je_Feature_und_zeigt_Prio_Klaerung_und_Fallbacks()
    {
        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
        [
            Item("FC-1", "feature", "Besuchs-Feature"),
            Item("PBI-1", "pbi", "Besuchsübersicht bereitstellen",
                meta: new() { ["featureId"] = "FC-1", ["priority"] = "high" }),   // Speicher-Form; Anzeige deutsch
            Item("PBI-2", "pbi", "Unklares PBI", status: "needs_clarify",
                meta: new() { ["featureId"] = "FC-1" }),
            Item("PBI-3", "pbi", "Heimatloses PBI"),
        ], [], [], []);

        var md = BacklogDocumentProjection.Render(core, version: 2, new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc));

        Assert.Contains("# Product Backlog — p", md);
        Assert.Contains("Version: 2", md);
        Assert.Contains("PBIs: 3", md);
        Assert.Contains("## Besuchs-Feature (FC-1)", md);
        Assert.Contains("## Ohne Feature-Zuordnung", md);
        Assert.Contains("| PBI-1 | Besuchsübersicht bereitstellen |", md);
        Assert.Contains("| hoch | — |", md);                       // Prio gesetzt, Schätzung ehrlich „—"
        Assert.Contains("| Klärung offen |", md);                  // Blocker-Achse als Klartext
        Assert.Contains("Fingerabdruck:", md);                     // In-Sync-/Refresh-Anker
    }
}
