using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Inc 1c-2: die arbeitsfaehigen Views auf den Core (deterministische Queries, kein LLM). Der Core bleibt die
// vollstaendige Wahrheit mit Historie; Agenten/Workflows bekommen gezielte Ausschnitte (plan-core-ingestion §3):
//   active-backlog · affected-items (Blast-Radius) · archive · github-sync.

public sealed record ActiveBacklogView(
    IReadOnlyList<ProjectStateItem> Features,
    IReadOnlyList<ProjectStateItem> Pbis,
    IReadOnlyList<ProjectStateItem> Requirements,
    IReadOnlyList<ProjectStateItem> OpenDecisions);

public sealed record ArchiveView(IReadOnlyList<ProjectStateItem> Items);

public sealed record GithubSyncEntry(
    string PbiId,
    string Title,
    string Status,
    string? Readiness,
    IReadOnlyList<string> CoveredRequirementIds,
    bool BlockedByOpenDecision,
    string? GithubIssue,
    // T3.1: operationaler Zustand des gemappten Issues (open/closed) aus der Core-Relation, sonst null.
    // Relevant fuer Dedup/Drift im naechsten Forward-Lauf (geschlossenes Issue bei aktivem PBI = Drift).
    string? GithubIssueStatus = null,
    // E0.1c/R-23: Akzeptanzkriterien + Statement (Story-Form) aus dem Core-Payload — landen im
    // deterministischen Issue-Body.
    IReadOnlyList<string>? AcceptanceCriteria = null,
    string? Statement = null,
    // ③ A3 (06.08.): die bestätigten Rahmen des PBIs ("ARCH-x — Text", via constrained_by) — werden im
    // Issue-Body als Sektion "Technische Rahmenbedingungen" sichtbar. Optional: Alt-Snapshots bleiben lesbar.
    IReadOnlyList<string>? Constraints = null,
    // A4/E-R2 (06.08.): umgesetzte Architektur-Arbeit ("ARCH-x - Text", via covers->architecture) -
    // die work-Rolle des PBIs, im Body als eigene Zeile (CoveredRequirementIds bleibt REIN req).
    IReadOnlyList<string>? CoveredArchitecture = null);

public sealed record GithubSyncView(IReadOnlyList<GithubSyncEntry> Entries);

// Blast-Radius eines Deltas: die direkt getouchten Items (seedIds) + 1 Hop ueber den Core-Graphen
// (betroffene PBIs/Features/Decisions). done/superseded/retired kommen nur rein, wenn sie 1 Hop von seed
// entfernt sind (Referenz/Konflikt) — sonst bleiben sie im Core, aber nicht in dieser View.
public sealed record AffectedItemsView(
    DateTime GeneratedUtc,
    IReadOnlyList<string> DirectIds,
    IReadOnlyList<ProjectStateItem> Requirements,
    IReadOnlyList<ProjectStateItem> Features,
    IReadOnlyList<ProjectStateItem> Pbis,
    IReadOnlyList<ProjectStateItem> OpenDecisions,
    IReadOnlyList<ProjectStateRelation> Relations,
    // ③ A3 (06.08., byAspect): die Rahmen-Seite der Betroffenheit — arch-Items im Radius (inkl. 2. Hop
    // PBI→constrained_by→ARCH, damit „welche Rahmen berührt diese Änderung?" direkt ablesbar ist).
    IReadOnlyList<ProjectStateItem>? Architecture = null);

public static class CoreViews
{
    // §5-S4: Archived-Set entfernt — IsArchived fragt jetzt CoreStatus.IsArchived (Superseded ODER Done); retired war ohnehin tot.

    public static ActiveBacklogView ActiveBacklog(ProjectStateDocument core)
    {
        var active = core.Items.Where(i => !IsArchived(i)).ToList();
        return new ActiveBacklogView(
            Features: active.Where(i => Is(i, "feature")).ToList(),
            Pbis: active.Where(i => Is(i, "pbi")).ToList(),
            Requirements: active.Where(i => Is(i, "requirement")).ToList(),
            OpenDecisions: active.Where(i => Is(i, "decision") && IsOpenDecision(i)).ToList());
    }

    public static ArchiveView Archive(ProjectStateDocument core)
        => new(core.Items.Where(IsArchived).ToList());

    public static GithubSyncView GithubSync(ProjectStateDocument core)
    {
        var contradictedReqs = core.Relations
            .Where(r => string.Equals(r.RelationType, "contradicts", StringComparison.Ordinal))
            .Select(r => r.ToId).ToHashSet(StringComparer.Ordinal);

        var coversByPbi = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ToId).ToList(), StringComparer.Ordinal);

        // ③ A3: die Rahmen je PBI (lebende constrained_by-Kanten; Text aus dem Core — Beleg, kein freier Text).
        var textById = core.Items.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
        var archIds = core.Items.Where(i => Is(i, "architecture")).Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var constraintsByPbi = core.Relations
            .Where(r => string.Equals(r.RelationType, ConstraintSwap.Relation, StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key,
                g => g.Select(r => $"{r.ToId} — {textById.GetValueOrDefault(r.ToId, "(unbekannt)")}")
                      .OrderBy(x => x, StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        // T3.1: das PBI<->Issue-Mapping kommt jetzt aus der Core-Relation implemented_by_issue (persistent),
        // nicht mehr nur aus einem Run-Artefakt. Fallback auf die alte metadata["githubIssue"] bleibt.
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        var entries = core.Items
            .Where(i => Is(i, "pbi") && !IsArchived(i))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(p =>
            {
                // A4/E-R2: covers kann jetzt auch arch-Ziele tragen - die Requirements-Zeile bleibt REIN req,
                // die Architektur-Arbeit wird eigene Sicht (kein ARCH-Id-Rauschen in req-Konsumenten).
                var coveredAll = coversByPbi.TryGetValue(p.ItemId, out var c) ? c : [];
                var covered = coveredAll.Where(id => !archIds.Contains(id)).ToList();
                var coveredArch = coveredAll.Where(archIds.Contains)
                    .Select(id => $"{id} — {textById.GetValueOrDefault(id, "(unbekannt)")}")
                    .OrderBy(x => x, StringComparer.Ordinal).ToList();
                var blocked = covered.Any(contradictedReqs.Contains);
                var mapping = mappingByPbi.GetValueOrDefault(p.ItemId);
                return new GithubSyncEntry(
                    PbiId: p.ItemId,
                    Title: p.Pbi?.Title ?? p.Text,
                    Status: p.Status,
                    Readiness: p.Pbi?.Readiness,
                    CoveredRequirementIds: covered,
                    BlockedByOpenDecision: blocked,
                    GithubIssue: mapping is not null ? CoreGithubMapping.IssueRef(mapping.IssueNumber) : p.Metadata.GetValueOrDefault("githubIssue"),
                    GithubIssueStatus: mapping?.OperationalStatus,
                    AcceptanceCriteria: p.Pbi?.AcceptanceCriteria ?? [],
                    Statement: p.Pbi?.Goal,
                    Constraints: constraintsByPbi.GetValueOrDefault(p.ItemId),
                    CoveredArchitecture: coveredArch.Count == 0 ? null : coveredArch);
            })
            .ToList();
        return new GithubSyncView(entries);
    }

    public static AffectedItemsView AffectedItems(ProjectStateDocument core, IReadOnlySet<string> seedIds)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var affected = new HashSet<string>(seedIds.Where(byId.ContainsKey), StringComparer.Ordinal);

        // 1 Hop ueber den Graphen: die andere Seite jeder Relation, die ein seed-Item beruehrt.
        foreach (var r in core.Relations)
        {
            if (seedIds.Contains(r.FromId) && byId.ContainsKey(r.ToId)) affected.Add(r.ToId);
            if (seedIds.Contains(r.ToId) && byId.ContainsKey(r.FromId)) affected.Add(r.FromId);
        }

        // ③ A3 (2. Hop, NUR Rahmen-Kanten): von jedem betroffenen PBI zu seinen constrained_by-ARCHs —
        // die Rahmen-Seite der Betroffenheit, ohne den Radius allgemein aufzublasen.
        foreach (var r in core.Relations.Where(r => string.Equals(r.RelationType, ConstraintSwap.Relation, StringComparison.Ordinal)))
            if (affected.Contains(r.FromId) && byId.ContainsKey(r.ToId)) affected.Add(r.ToId);

        var items = affected.Select(id => byId[id]).ToList();
        var relations = core.Relations
            .Where(r => affected.Contains(r.FromId) || affected.Contains(r.ToId))
            .ToList();

        return new AffectedItemsView(
            GeneratedUtc: DateTime.UtcNow,
            DirectIds: seedIds.Where(byId.ContainsKey).OrderBy(x => x, StringComparer.Ordinal).ToList(),
            Requirements: items.Where(i => Is(i, "requirement")).ToList(),
            Features: items.Where(i => Is(i, "feature")).ToList(),
            Pbis: items.Where(i => Is(i, "pbi")).ToList(),
            OpenDecisions: items.Where(i => Is(i, "decision")).ToList(),
            Relations: relations,
            Architecture: items.Where(i => Is(i, "architecture")).ToList());
    }

    // §5-S4: über die zentrale Lese-Naht (typisierte Achsen; retired entfällt — war tot). Verhalten gleich.
    private static bool IsArchived(ProjectStateItem i) => i.ReadStatus().IsArchived;
    private static bool IsOpenDecision(ProjectStateItem i) => i.ReadStatus().IsOpenDecision;
    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
}
