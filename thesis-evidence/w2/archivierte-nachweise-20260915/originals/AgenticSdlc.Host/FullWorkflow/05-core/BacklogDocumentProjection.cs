using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Slice S (Autor-Idee 21.08., „das ganze PBI als echte lebende Tabelle"): das BACKLOG als
/// deterministische Core-Projektion — `docs/backlog.md`, je Feature eine Tabelle. Muster identisch
/// zum Anforderungsdokument (Version/Stand/Fingerprint, GENERIERT-Prinzip); publiziert wird es über
/// die Doc-Publish-Liste (Teil 1), gerendert FRISCH beim Seed („lebend" = garantiert Core-Stand).
/// Die Spalten Prio/Schätzung lesen die Payload-Felder aus Teil 2 — bis dahin ehrlich „—".
/// </summary>
public static class BacklogDocumentProjection
{
    public const string RelPath = "docs/backlog.md";

    public static async Task<(string Path, int Version)> RunAsync(string repoRoot, DateTime? utcNow = null)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Core fehlt (state/core) — kein Backlog zu projizieren.");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var target = Path.Combine(repoRoot, RelPath);
        var previous = File.Exists(target) ? File.ReadAllText(target) : null;
        var version = RequirementsDocumentProjection.NextVersion(previous);
        var content = Render(core, version, utcNow ?? DateTime.UtcNow);
        // R-72: nur echte Inhalts-Änderungen schreiben (geteilte SameBody-Wache) — kein Kopfzeilen-Push,
        // wenn eine fremde Wahrheits-Änderung (z. B. DEC-Schließung) den Backlog-Inhalt gar nicht berührt.
        if (previous is not null && RequirementsDocumentProjection.SameBody(previous, content))
            return (target, version - 1);
        await File.WriteAllTextAsync(target, content).ConfigureAwait(false);
        return (target, version);
    }

    public static string Render(ProjectStateDocument core, int version, DateTime generatedUtc)
    {
        var sb = new System.Text.StringBuilder();
        var pbis = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Validity == Validity.Active)
            .ToList();
        var features = core.Items
            .Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(i => i.ItemId, i => i.Pbi?.Title ?? i.Feature?.Label ?? i.Text, StringComparer.Ordinal);
        var coversByPbi = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(r => r.ToId).OrderBy(x => x, StringComparer.Ordinal)), StringComparer.Ordinal);
        var issueByPbi = CoreGithubMapping.ByPbi(core);

        sb.Append($"# Product Backlog — {core.ProjectId}\n\n");
        sb.Append($"> Version: {version} · Stand: {generatedUtc:yyyy-MM-dd HH:mm} UTC · PBIs: {pbis.Count} · Fingerabdruck: {RequirementsDocumentProjection.Fingerprint(core)}\n");
        sb.Append("> Projektion aus der Projektwahrheit (Core) — GENERIERT, nie von Hand pflegen.\n\n");

        foreach (var group in pbis
                     .GroupBy(p => p.Metadata.GetValueOrDefault("featureId") ?? "")
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var featureTitle = group.Key.Length > 0 && features.TryGetValue(group.Key, out var t)
                ? $"{t} ({group.Key})" : "Ohne Feature-Zuordnung";
            sb.Append($"## {featureTitle}\n\n");
            sb.Append("| PBI | Titel | Status | Klärung/Blocker | Prio | Schätzung | Requirements | Issue |\n");
            sb.Append("| --- | --- | --- | --- | --- | --- | --- | --- |\n");
            foreach (var p in group.OrderBy(x => x.ItemId, StringComparer.Ordinal))
            {
                var st = p.ReadStatus();
                var blocker = st.Blocker switch
                {
                    Blocker.NeedsClarify => "Klärung offen",
                    Blocker.BlockedByDecision => "blockiert (DEC)",
                    _ => "—",
                };
                // Teil 2: gespeichert englisch (PbiFields-Regel), angezeigt deutsch.
                var prioRaw = p.Metadata.GetValueOrDefault(PbiFields.MetaPriority);
                var prio = Cell(prioRaw is null ? null : PbiFields.PriorityDe(prioRaw));
                var estimate = Cell(p.Metadata.GetValueOrDefault(PbiFields.MetaEstimate));
                var issue = issueByPbi.TryGetValue(p.ItemId, out var m) ? $"#{m.IssueNumber}" : "—";
                var titel = (p.Pbi?.Title ?? p.Text).Replace("|", "\\|");
                sb.Append($"| {p.ItemId} | {titel} | {p.Status} | {blocker} | {prio} | {estimate} | {coversByPbi.GetValueOrDefault(p.ItemId, "—")} | {issue} |\n");
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static string Cell(string? v) => string.IsNullOrWhiteSpace(v) ? "—" : v;
}
