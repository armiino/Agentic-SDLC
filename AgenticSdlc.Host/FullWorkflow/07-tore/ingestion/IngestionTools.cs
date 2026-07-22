using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// MAKER-Tools der Requirement-Ingestion: der Resolver erkundet eingehende Items + den Core (Retrieval Stufe 0)
// und speichert einen StateChangePlan. Beleg-Pflicht: MATCH/REFINE/SUPERSEDE/CONTRADICT nennen targetEntityId.
internal sealed class IngestionTools(
    ProjectStateDocument meetingDelta,
    ProjectStateDocument core,
    ICandidateRetriever retriever,
    RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly Dictionary<string, ProjectStateItem> _coreById = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
    private IReadOnlyList<StateChangeOperation>? _saved;
    private int _checkRounds;

    public bool Saved => _saved is not null;
    public IReadOnlyList<StateChangeOperation>? SavedOperations => _saved;
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetIncomingItems, "get_incoming_items",
            "Listet die eingehenden Requirement-Items dieses Meetings (incomingItemId, text, sourceClaimIds), die aufgeloest werden muessen."),
        AIFunctionFactory.Create(ListCoreRequirements, "list_core_requirements",
            "Listet die bestehenden Requirement-Entitaeten des Core als Kandidaten (entityId, identityKey, status, text)."),
        AIFunctionFactory.Create(GetCoreEntity, "get_core_entity",
            "Liest eine Core-Entitaet vollstaendig (Text, Status, Herkunft, Historie)."),
        AIFunctionFactory.Create(ListOpenDecisions, "list_open_decisions",
            "Listet offene Entscheidungen (DEC-*, status=open_decision) mit dem widersprochenen Ziel. Pruefen, BEVOR du CONTRADICT vorschlaegst - ist der Widerspruch schon erfasst, nutze ALREADY_DECIDED."),
        AIFunctionFactory.Create(SearchCore, "search_core",
            "Sucht in den Core-Requirements nach Stichworten (entityId/text)."),
        AIFunctionFactory.Create(CheckPlan, "check_state_change_plan",
            "Prueft die Operationen deterministisch (Coverage/Ziele/Belege). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(SavePlan, "save_state_change_plan",
            "Speichert die finalen Operationen (genau eine je eingehendem Requirement). Genau einmal am Ende aufrufen."),
    ];

    private IReadOnlyList<ProjectStateItem> Incoming() =>
        meetingDelta.Items.Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)).ToList();

    private string GetIncomingItems()
    {
        var rows = Incoming()
            .Select(i => new { incomingItemId = i.ItemId, text = Truncate(i.Text, 500), origin = i.Origin, sourceClaimIds = i.SourceClaimIds })
            .ToArray();
        run.AppendEvent(new { type = "INGEST_TOOL_INCOMING", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string ListCoreRequirements(int limit = 300)
    {
        var rows = retriever.GetCandidates(string.Empty, core)
            .Take(Math.Clamp(limit, 1, 500))
            .Select(i => new { entityId = i.ItemId, identityKey = i.IdentityKey, i.Status, text = Truncate(i.Text, 300) })
            .ToArray();
        run.AppendEvent(new { type = "INGEST_TOOL_LIST", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetCoreEntity(string entityId)
    {
        var id = (entityId ?? string.Empty).Trim();
        if (!_coreById.TryGetValue(id, out var item)) return $"UNKNOWN_ENTITY: {id}";
        run.AppendEvent(new { type = "INGEST_TOOL_GET", runId = run.RunId, entityId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(item, Json);
    }

    private string ListOpenDecisions()
    {
        var rows = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(i.Status, "open_decision", StringComparison.OrdinalIgnoreCase))
            .Select(i => new { entityId = i.ItemId, contradicts = i.Metadata.GetValueOrDefault("targetEntityId"), text = Truncate(i.Text, 240) })
            .ToArray();
        run.AppendEvent(new { type = "INGEST_TOOL_DECISIONS", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchCore(string query, int limit = 30)
    {
        var terms = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return "[]";
        var rows = core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .Select(i => new { i, score = terms.Count(t => i.ItemId.Contains(t, StringComparison.OrdinalIgnoreCase) || i.Text.Contains(t, StringComparison.OrdinalIgnoreCase)) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score).ThenBy(x => x.i.ItemId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 80))
            .Select(x => new { entityId = x.i.ItemId, x.i.Status, text = Truncate(x.i.Text, 400) })
            .ToArray();
        run.AppendEvent(new { type = "INGEST_TOOL_SEARCH", runId = run.RunId, query, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string CheckPlan(StateChangeOperation[] operations)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, "check", DateTime.UtcNow, string.Empty, (operations ?? []).ToList());
        var report = IngestionGate.Check(meetingDelta, core, plan);
        run.AppendEvent(new { type = "INGEST_TOOL_CHECK", runId = run.RunId, round, operations = operations?.Length ?? 0, pass = report.Pass, errors = report.Errors.Count, warnings = report.Warnings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SavePlan(StateChangeOperation[] operations)
    {
        if (Saved) return "ALREADY_SAVED: save_state_change_plan darf nur einmal aufgerufen werden.";
        _saved = (operations ?? []).ToList();
        run.AppendEvent(new { type = "INGEST_TOOL_SAVE", runId = run.RunId, operations = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, operations = _saved.Count }, Json);
    }

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }
}
