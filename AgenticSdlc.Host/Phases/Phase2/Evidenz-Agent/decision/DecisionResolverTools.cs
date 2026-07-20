using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// T2.2 — Tools des Decision-Resolver-Agenten (der 4. agentische Knoten): er deutet eine freie Stakeholder-Antwort
// in eine strukturierte Auflösung je offener Decision (Outcome + optional neue Aussage). Die eigentliche State-
// Mutation bleibt deterministisch (T2.1): save_resolutions erzeugt nur den DecisionResolutionInput, den die
// bewährte Derivation → Gate → Apply-Kette verarbeitet. Kein Write, kein Core-Zugriff jenseits Lesen.
internal sealed class DecisionResolverTools(ProjectStateDocument core, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, ProjectStateItem> _byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
    private IReadOnlyList<DecisionResolutionRequest>? _saved;

    public bool Saved => _saved is not null;
    public DecisionResolutionInput SavedInput => new(_saved ?? []);

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetOpenDecisions, "get_open_decisions",
            "Die offenen Decisions (id, text, widersprochenes Requirement) — nur diese kannst du auflösen."),
        AIFunctionFactory.Create(GetDecisionContext, "get_decision_context",
            "Kontext einer Decision: DEC-Text, widersprochenes Requirement (Text), betroffene PBIs — Basis fürs Urteil."),
        AIFunctionFactory.Create(SaveResolutions, "save_resolutions",
            "Speichert je adressierter Decision GENAU EINE Auflösung: decisionId + outcome (KEEP_ORIGINAL|ADOPT_NEW|REFINE) + newStatement (bei ADOPT_NEW/REFINE) + rationale. Genau einmal."),
    ];

    private string GetOpenDecisions()
    {
        var rows = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(i.Status, DecisionStatus.Open, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new { decisionId = i.ItemId, text = i.Text, targetRequirementId = TargetOf(i), })
            .ToArray();
        run.AppendEvent(new { type = "DECISION_TOOL_LIST_OPEN", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetDecisionContext(string decisionId)
    {
        var id = (decisionId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var dec) || !string.Equals(dec.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
            return $"UNKNOWN_DECISION: {id}";
        var target = TargetOf(dec);
        var targetText = target is not null && _byId.TryGetValue(target, out var t) ? t.Text : null;
        var affected = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                        && (i.Pbi?.OpenDecisionRefs.Contains(id) ?? false))
            .Select(i => i.ItemId).ToArray();
        run.AppendEvent(new { type = "DECISION_TOOL_CONTEXT", runId = run.RunId, decisionId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { decisionId = id, decisionText = dec.Text, targetRequirementId = target, targetRequirementText = targetText, blockedPbis = affected }, Json);
    }

    private string SaveResolutions(DecisionResolutionRequest[] resolutions)
    {
        if (Saved) return "ALREADY_SAVED: save_resolutions darf nur einmal aufgerufen werden.";
        _saved = (resolutions ?? []).ToList();
        run.AppendEvent(new { type = "DECISION_TOOL_SAVE", runId = run.RunId, resolutions = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, resolutions = _saved.Count }, Json);
    }

    private string? TargetOf(ProjectStateItem dec)
        => core.Relations.FirstOrDefault(r => string.Equals(r.RelationType, DecisionRelations.Contradicts, StringComparison.Ordinal)
               && string.Equals(r.FromId, dec.ItemId, StringComparison.Ordinal))?.ToId
           ?? dec.Metadata.GetValueOrDefault("targetEntityId");
}
