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
    string? Statement = null);

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
    IReadOnlyList<ProjectStateRelation> Relations);

public static class CoreViews
{
    private static readonly HashSet<string> Archived = new(StringComparer.OrdinalIgnoreCase) { "done", "superseded", "retired" };

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

        // T3.1: das PBI<->Issue-Mapping kommt jetzt aus der Core-Relation implemented_by_issue (persistent),
        // nicht mehr nur aus einem Run-Artefakt. Fallback auf die alte metadata["githubIssue"] bleibt.
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        var entries = core.Items
            .Where(i => Is(i, "pbi") && !IsArchived(i))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(p =>
            {
                var covered = coversByPbi.TryGetValue(p.ItemId, out var c) ? c : [];
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
                    Statement: p.Pbi?.Goal);
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
            Relations: relations);
    }

    private static bool IsArchived(ProjectStateItem i) => Archived.Contains(i.Status);
    private static bool IsOpenDecision(ProjectStateItem i) => string.Equals(i.Status, "open_decision", StringComparison.OrdinalIgnoreCase);
    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
}
