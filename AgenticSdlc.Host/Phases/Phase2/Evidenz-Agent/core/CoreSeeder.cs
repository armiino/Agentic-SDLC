using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Einmaliger Seed: hebt einen bestehenden ProjectState-Rebuild als Ausgangs-WAHRHEIT in den Core.
// Deterministisch, kein LLM: setzt je Item einen identityKey (Retrieval-Anker) + leere Historie und
// stellt Version>=1 sicher. Relations/Provenance/Proposals/Sources bleiben unveraendert erhalten.
public static class CoreSeeder
{
    public sealed record Report(int Total, int Requirements, int Architecture, int Other, int WithIdentityKey);

    public static (ProjectStateDocument Core, Report Report) Seed(ProjectStateDocument source)
    {
        var items = source.Items.Select(SeedItem).ToList();
        var core = source with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Items = items
        };

        var report = new Report(
            Total: items.Count,
            Requirements: items.Count(i => Is(i, "requirement")),
            Architecture: items.Count(i => Is(i, "architecture")),
            Other: items.Count(i => !Is(i, "requirement") && !Is(i, "architecture")),
            WithIdentityKey: items.Count(i => !string.IsNullOrEmpty(i.IdentityKey)));

        return (core, report);
    }

    private static ProjectStateItem SeedItem(ProjectStateItem item) => item with
    {
        Version = item.Version <= 0 ? 1 : item.Version,
        IdentityKey = string.IsNullOrWhiteSpace(item.IdentityKey) ? IdentityKey.From(item.Text) : item.IdentityKey,
        History = item.History ?? []
    };

    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
}
