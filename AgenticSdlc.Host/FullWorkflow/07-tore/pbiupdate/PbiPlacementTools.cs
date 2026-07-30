using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Tools des Platzierungs-Agenten (der EINZIGE agentische Teil von 1c-3): entscheidet je NEUEM Requirement
// EXTEND_PBI (welches bestehende PBI) vs NEW_PBI (in welchem Feature). Beleg ueber Requirement-Overlap/Feature.
internal sealed class PbiPlacementTools(
    IReadOnlyList<PbiUpdateDerivation.UnplacedRequirement> unplaced,
    ProjectStateDocument core,
    RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly Dictionary<string, ProjectStateItem> _byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
    private IReadOnlyList<PbiStateChangeOperation>? _saved;

    public bool Saved => _saved is not null;
    public IReadOnlyList<PbiStateChangeOperation>? SavedPlacements => _saved;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetUnplaced, "get_unplaced_requirements",
            "Die neuen Requirements, die noch keinem PBI zugeordnet sind (requirementId, text, featureHint)."),
        AIFunctionFactory.Create(ListFeatures, "list_features",
            "Bestehende Features (featureId, label) als moegliche Ziele fuer NEW_PBI."),
        AIFunctionFactory.Create(GetFeaturePbis, "get_feature_pbis",
            "Die PBIs eines Features (pbiId, title, coveredRequirementIds) - Kandidaten fuer EXTEND_PBI."),
        AIFunctionFactory.Create(GetPbi, "get_pbi",
            "Liest ein PBI (Titel, goal, acceptanceCriteria, linkedRequirementIds)."),
        AIFunctionFactory.Create(SavePlacements, "save_placements",
            "Speichert je unplaced Requirement GENAU EINE Platzierung: kind=EXTEND_PBI (mit pbiId, bestehendes PBI) "
            + "ODER NEW_PBI (mit featureId, bestehendes Feature) ODER NEW_FEATURE (mit proposedFeatureLabel, wenn KEIN "
            + "bestehendes Feature fachlich passt). Genau einmal."),
    ];

    private string GetUnplaced()
    {
        var rows = unplaced.Select(u => new { requirementId = u.RequirementId, text = Truncate(u.Text, 300), featureHint = u.FeatureHint }).ToArray();
        run.AppendEvent(new { type = "PBI_PLACE_UNPLACED", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string ListFeatures()
    {
        var rows = core.Items.Where(i => Is(i, "feature"))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new { featureId = i.ItemId, label = i.Feature?.Label ?? i.Text }).ToArray();
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetFeaturePbis(string featureId)
    {
        var pbiIds = core.Relations
            .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                        && string.Equals(r.ToId, featureId, StringComparison.Ordinal)
                        && _byId.TryGetValue(r.FromId, out var it) && Is(it, "pbi"))
            .Select(r => r.FromId).Distinct(StringComparer.Ordinal).ToList();
        var covers = core.Relations.Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Select(x => x.ToId).ToList(), StringComparer.Ordinal);
        var rows = pbiIds.Where(_byId.ContainsKey).Select(id => new
        {
            pbiId = id,
            title = _byId[id].Pbi?.Title ?? _byId[id].Text,
            coveredRequirementIds = covers.TryGetValue(id, out var c) ? c : []
        }).ToArray();
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetPbi(string pbiId)
    {
        var id = (pbiId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var it) || it.Pbi is null) return $"UNKNOWN_PBI: {id}";
        return JsonSerializer.Serialize(new { it.ItemId, it.Status, it.Pbi }, Json);
    }

    private string SavePlacements(PbiStateChangeOperation[] placements)
    {
        if (Saved) return "ALREADY_SAVED: save_placements darf nur einmal aufgerufen werden.";
        _saved = (placements ?? []).ToList();
        run.AppendEvent(new { type = "PBI_PLACE_SAVE", runId = run.RunId, placements = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, placements = _saved.Count }, Json);
    }

    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string value, int max)
    {
        var t = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return t.Length <= max ? t : t[..max] + "...";
    }
}
