using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// Authored-Relations-Nachschlag (L4/L3-Relationen aus dem ProjectState) auf CAN-REQ-Ebene.
// Best-effort: laedt den ProjectState ueber baseline.SourceProjectStatePath; faellt sonst leer aus.
internal sealed class RelationLookup
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _map;
    private RelationLookup(IReadOnlyDictionary<string, IReadOnlyList<string>> map) => _map = map;

    public static readonly RelationLookup Empty =
        new(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal));

    public bool HasData => _map.Count > 0;
    public IReadOnlyList<string> Related(string canReqId) => _map.TryGetValue(canReqId, out var v) ? v : [];

    public static async Task<RelationLookup> BuildAsync(CanonicalRequirementsBaseline baseline, string repoRoot, CancellationToken ct = default)
    {
        var path = baseline.SourceProjectStatePath;
        if (string.IsNullOrWhiteSpace(path)) return Empty;
        var full = Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);
        if (!File.Exists(full)) return Empty;

        JsonProjectStateRepository repo;
        try { repo = await JsonProjectStateRepository.LoadAsync(full, ct).ConfigureAwait(false); }
        catch { return Empty; }

        var itemToCan = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var r in baseline.Requirements)
            foreach (var sid in r.SourceItemIds)
                itemToCan[sid] = r.RequirementId;

        var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var r in baseline.Requirements)
        {
            var related = new HashSet<string>(StringComparer.Ordinal);
            foreach (var sid in r.SourceItemIds)
            {
                foreach (var rel in await repo.GetRelationsAsync(sid, ct).ConfigureAwait(false))
                {
                    var other = string.Equals(rel.FromId, sid, StringComparison.Ordinal) ? rel.ToId : rel.FromId;
                    if (itemToCan.TryGetValue(other, out var canId) && !string.Equals(canId, r.RequirementId, StringComparison.Ordinal))
                        related.Add(canId);
                }
            }
            if (related.Count > 0)
                map[r.RequirementId] = related.OrderBy(x => x, StringComparer.Ordinal).ToList();
        }
        return new RelationLookup(map);
    }
}

// MAKER-Tools: der Cluster-Agent erkundet Requirements + authored Relationen und speichert Cluster.
internal sealed class ReClarifyClusterTools(CanonicalRequirementsBaseline baseline, RelationLookup relations, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, CanonicalRequirement> _byId =
        baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
    private IReadOnlyList<FeatureCluster>? _saved;
    private int _checkRounds;

    public bool Saved => _saved is not null;
    public IReadOnlyList<FeatureCluster>? SavedClusters => _saved;
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListRequirements, "list_requirements",
            "Listet die kanonischen Requirements der Baseline (id, title, status, sourceItemIds)."),
        AIFunctionFactory.Create(SearchRequirements, "search_requirements",
            "Sucht in Requirements nach Stichworten (id/title/text)."),
        AIFunctionFactory.Create(GetRequirement, "get_requirement",
            "Liest ein Requirement vollstaendig (Text, Status, Quellen, Metadaten)."),
        AIFunctionFactory.Create(GetRelatedRequirements, "get_related_requirements",
            "Liefert die AUTHORED verwandten Requirements (L3/L4-Relationen) fuer ein Requirement."),
        AIFunctionFactory.Create(CheckClusters, "check_clusters",
            "Prueft Cluster deterministisch (Coverage: jedes aktive Requirement genau einmal core). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(SaveClusters, "save_clusters",
            "Speichert die finalen Feature-Cluster. Genau einmal am Ende aufrufen."),
    ];

    private string ListRequirements(int limit = 100)
    {
        var rows = baseline.Requirements
            .OrderBy(r => r.RequirementId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 300))
            .Select(r => new { r.RequirementId, r.Title, r.Status, sourceItemIds = r.SourceItemIds, text = Truncate(r.Text, 300) })
            .ToArray();
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_LIST", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchRequirements(string query, int limit = 30)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return "[]";
        var rows = baseline.Requirements
            .Select(r => new { r, score = terms.Count(t => r.RequirementId.Contains(t, StringComparison.OrdinalIgnoreCase) || r.Title.Contains(t, StringComparison.OrdinalIgnoreCase) || r.Text.Contains(t, StringComparison.OrdinalIgnoreCase)) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score).ThenBy(x => x.r.RequirementId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => new { x.r.RequirementId, x.r.Title, x.r.Status, text = Truncate(x.r.Text, 400) })
            .ToArray();
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_SEARCH", runId = run.RunId, query, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetRequirement(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var r)) return $"UNKNOWN_REQUIREMENT: {id}";
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_GET", runId = run.RunId, requirementId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(r, Json);
    }

    private string GetRelatedRequirements(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_byId.ContainsKey(id)) return $"UNKNOWN_REQUIREMENT: {id}";
        var related = relations.Related(id)
            .Select(rid => new { requirementId = rid, title = _byId.TryGetValue(rid, out var rr) ? rr.Title : rid })
            .ToArray();
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_RELATED", runId = run.RunId, requirementId = id, returned = related.Length, hasData = relations.HasData, timestampUtc = DateTime.UtcNow });
        return related.Length == 0
            ? (relations.HasData ? "[]  (keine authored Relationen fuer dieses Requirement)" : "[]  (keine ProjectState-Relationen verfuegbar)")
            : JsonSerializer.Serialize(related, Json);
    }

    private string CheckClusters(FeatureCluster[] clusters)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var report = ReClarifyClusterGate.Check(baseline, clusters ?? []);
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_CHECK", runId = run.RunId, round, clusters = clusters?.Length ?? 0, pass = report.Pass, errors = report.Errors.Count, warnings = report.Warnings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SaveClusters(FeatureCluster[] clusters)
    {
        if (Saved) return "ALREADY_SAVED: save_clusters darf nur einmal aufgerufen werden.";
        _saved = (clusters ?? []).ToList();
        run.AppendEvent(new { type = "RE_CLARIFY_TOOL_SAVE", runId = run.RunId, clusters = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, clusters = _saved.Count }, Json);
    }

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}

// CHECKER-Tools: der zweite Agent kritisiert die vorgeschlagenen Cluster und speichert ein Review.
internal sealed class ReClarifyClusterReviewTools(CanonicalRequirementsBaseline baseline, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, CanonicalRequirement> _byId =
        baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
    private ClusterReviewReport? _saved;

    public bool Saved => _saved is not null;
    public ClusterReviewReport? SavedReview => _saved;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetRequirement, "get_requirement",
            "Liest ein Requirement vollstaendig, um die Cluster-Zuordnung fachlich zu pruefen."),
        AIFunctionFactory.Create(SaveReview, "save_review",
            "Speichert Review-Feedback: findings (Diagnose) UND operations (konkrete, anwendbare Fixes). Genau einmal am Ende."),
    ];

    private string GetRequirement(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var r)) return $"UNKNOWN_REQUIREMENT: {id}";
        return JsonSerializer.Serialize(r, Json);
    }

    private string SaveReview(string verdict, string summary, ClusterReviewFinding[] findings, ClusterOperation[] operations)
    {
        if (Saved) return "ALREADY_SAVED: save_review darf nur einmal aufgerufen werden.";
        _saved = new ClusterReviewReport(
            SchemaVersion: ClusterReviewReport.CurrentSchemaVersion,
            ReviewId: $"cluster-review-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            Verdict: string.IsNullOrWhiteSpace(verdict) ? "revise" : verdict.Trim(),
            Summary: summary ?? string.Empty,
            Findings: (findings ?? []).ToList())
        {
            Operations = (operations ?? []).ToList()
        };
        run.AppendEvent(new { type = "RE_CLARIFY_REVIEW_SAVE", runId = run.RunId, verdict = _saved.Verdict, findings = _saved.Findings.Count, operations = _saved.Operations.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, verdict = _saved.Verdict, findings = _saved.Findings.Count, operations = _saved.Operations.Count }, Json);
    }
}
