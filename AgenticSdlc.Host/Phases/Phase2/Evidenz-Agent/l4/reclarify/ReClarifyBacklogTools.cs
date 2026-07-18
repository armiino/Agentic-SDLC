using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// MAKER-Tools des Clarify-Agenten: pro Feature-Cluster den vollen Kontext (core + crossCutting mit Text)
// erkunden, PBIs pruefen und speichern. Der Agent verhoert jedes Feature (Zweck/Daten/Regeln/Fehler/Abnahme)
// und schneidet es in wertorientierte PBIs mit Akzeptanzkriterien + expliziten offenen Entscheidungen.
internal sealed class ReClarifyBacklogTools(FeatureClusterSet clusters, CanonicalRequirementsBaseline baseline, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, CanonicalRequirement> _byReq = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
    private readonly Dictionary<string, FeatureCluster> _byCluster = clusters.Clusters.ToDictionary(c => c.ClusterId, StringComparer.Ordinal);
    private IReadOnlyList<ProductBacklogItem>? _saved;
    private int _checkRounds;

    public bool Saved => _saved is not null;
    public IReadOnlyList<ProductBacklogItem>? SavedItems => _saved;
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListClusters, "list_clusters",
            "Listet die Feature-Cluster (clusterId, label, core-/crossCutting-Anzahl)."),
        AIFunctionFactory.Create(GetFeatureContext, "get_feature_context",
            "Liefert fuer EINEN Cluster den vollen Kontext: core- und crossCutting-Requirements mit Titel und Text."),
        AIFunctionFactory.Create(GetRequirement, "get_requirement",
            "Liest ein einzelnes Requirement vollstaendig."),
        AIFunctionFactory.Create(CheckPbis, "check_pbis",
            "Prueft die PBIs deterministisch (Coverage der Cluster-Cores, keine stille Luecke). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(SavePbis, "save_pbis",
            "Speichert die finalen Product Backlog Items. Genau einmal am Ende aufrufen."),
    ];

    private string ListClusters()
    {
        var rows = clusters.Clusters
            .Select(c => new { c.ClusterId, c.IdentityKey, c.Label, core = c.CoreRequirementIds.Count, crossCutting = c.CrossCuttingRequirementIds.Count })
            .ToArray();
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_TOOL_LIST", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetFeatureContext(string clusterId)
    {
        var id = (clusterId ?? string.Empty).Trim();
        if (!_byCluster.TryGetValue(id, out var c)) return $"UNKNOWN_CLUSTER: {id}";
        var ctx = new
        {
            c.ClusterId,
            c.IdentityKey,
            c.Label,
            c.Rationale,
            core = c.CoreRequirementIds.Select(Detail).ToArray(),
            crossCutting = c.CrossCuttingRequirementIds.Select(Detail).ToArray()
        };
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_TOOL_CONTEXT", runId = run.RunId, clusterId = id, core = c.CoreRequirementIds.Count, crossCutting = c.CrossCuttingRequirementIds.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(ctx, Json);
    }

    private string GetRequirement(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_byReq.TryGetValue(id, out var r)) return $"UNKNOWN_REQUIREMENT: {id}";
        return JsonSerializer.Serialize(r, Json);
    }

    private string CheckPbis(ProductBacklogItem[] items)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var doc = BuildDoc(items ?? []);
        var report = ReClarifyBacklogGate.Check(clusters, baseline, doc);
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_TOOL_CHECK", runId = run.RunId, round, pbis = doc.Items.Count, pass = report.Pass, errors = report.Errors.Count, warnings = report.Warnings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SavePbis(ProductBacklogItem[] items)
    {
        if (Saved) return "ALREADY_SAVED: save_pbis darf nur einmal aufgerufen werden.";
        _saved = (items ?? []).ToList();
        run.AppendEvent(new { type = "RE_CLARIFY_BACKLOG_TOOL_SAVE", runId = run.RunId, pbis = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, pbis = _saved.Count }, Json);
    }

    private object Detail(string reqId)
        => _byReq.TryGetValue(reqId, out var r)
            ? new { requirementId = reqId, r.Title, r.Status, text = r.Text }
            : new { requirementId = reqId, Title = "(unbekannt)", Status = "", text = "" };

    private ProductBacklogDocument BuildDoc(IReadOnlyList<ProductBacklogItem> items)
        => new(
            SchemaVersion: ProductBacklogDocument.CurrentSchemaVersion,
            BacklogId: $"product-backlog-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: baseline.ProjectId,
            BaselineId: baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourcePath: clusters.SourceBaselinePath,
            Items: items.ToList());
}
