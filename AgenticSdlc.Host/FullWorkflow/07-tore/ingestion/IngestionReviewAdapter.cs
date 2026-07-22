using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

public sealed record IngestionHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<IngestionHumanDecision> Decisions);

public sealed record IngestionHumanDecision(
    [property: JsonPropertyName("incomingItemId")] string IncomingItemId,
    [property: JsonPropertyName("decision")] string Decision,   // apply | skip
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die vom Resolver vorgeschlagenen Ingestion-OPERATIONEN in die generische HumanReview-UI:
// ein Item je Operation, Entscheidung apply/skip. Der Mensch autorisiert die Veraenderung der Wahrheit.
// Der IngestionApply fuehrt die akzeptierten Operationen deterministisch aus (Upsert-by-Identity).
public static class IngestionReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };

    public static ReviewSession BuildSession(
        string runId,
        StateChangePlanDocument plan,
        ProjectStateDocument meetingDelta,
        ProjectStateDocument core)
    {
        var incomingById = meetingDelta.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var coreById = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var items = plan.Operations.Select(op => BuildItem(op, incomingById, coreById)).ToList();

        return new ReviewSession
        {
            SessionId = $"ingestion-{runId}",
            Title = "Requirement-Ingestion — Core aktualisieren",
            Subtitle = plan.Operations.Count == 0
                ? "Keine Operationen vorgeschlagen."
                : $"{plan.Operations.Count} vorgeschlagene Aenderungen an der Projektwahrheit. apply = uebernehmen, skip = verwerfen.",
            Help = BuildHelp(),
            FieldSchema = Schema(),
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static IngestionHumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(item => new IngestionHumanDecision(
            IncomingItemId: item.ItemId,
            Decision: FieldOf(item, FieldDecision),
            Reason: FieldOf(item, FieldReason) is { Length: > 0 } r ? r : null)).ToList();
        return new IngestionHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, IngestionHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byIncoming = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.IncomingItemId))
            .GroupBy(d => d.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byIncoming.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, StateChangePlanDocument plan, ProjectStateDocument meetingDelta, ProjectStateDocument core)
    {
        if (resolverKey.StartsWith("op:", StringComparison.Ordinal))
        {
            var id = resolverKey["op:".Length..];
            var op = plan.Operations.FirstOrDefault(o => string.Equals(o.IncomingItemId, id, StringComparison.Ordinal));
            return op is null ? $"(Operation {id} nicht gefunden)" : JsonSerializer.Serialize(op, Json);
        }
        if (resolverKey.StartsWith("incoming:", StringComparison.Ordinal))
        {
            var id = resolverKey["incoming:".Length..];
            var it = meetingDelta.Items.FirstOrDefault(x => string.Equals(x.ItemId, id, StringComparison.Ordinal));
            return it is null ? $"(Incoming {id} nicht gefunden)" : JsonSerializer.Serialize(it, Json);
        }
        if (resolverKey.StartsWith("entity:", StringComparison.Ordinal))
        {
            var id = resolverKey["entity:".Length..];
            var it = core.Items.FirstOrDefault(x => string.Equals(x.ItemId, id, StringComparison.Ordinal));
            return it is null ? $"(Entitaet {id} nicht gefunden)" : JsonSerializer.Serialize(it, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["apply", "skip"], Required: true,
            Help: "apply = Aenderung in den Core uebernehmen (deterministischer Apply) · skip = verwerfen"),
        new ReviewFieldSpec(FieldReason, "Begruendung (optional)", ReviewInputType.MultiLine, [], Required: false,
            Help: "Optionale Notiz, warum uebernommen/verworfen.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "Requirement-Ingestion",
        Summary: "Ein neues Meeting wurde gegen die Projektwahrheit (Core) aufgeloest. Du entscheidest je Aenderung apply/skip.",
        Sections:
        [
            new ReviewHelpSection(
                "Was du hier tust",
                """
                Ein Resolver-Agent hat jedes eingehende Requirement des neuen Meetings gegen den Core geprueft und
                eine Operation vorgeschlagen: bekannt (RESTATE), verfeinert (REFINE), fachlich neu im selben Feature
                (NEW_RELATED), neu (NEW), Ersatz (SUPERSEDE) oder Widerspruch (CONTRADICT).

                Du autorisierst jede Operation (apply) oder verwirfst sie (skip). Der Apply fuehrt die akzeptierten
                Operationen deterministisch aus: REFINE erzeugt eine neue Version (alte bleibt in der Historie),
                NEW/NEW_RELATED praegt eine neue stabile Core-ID, CONTRADICT legt eine offene Entscheidung an -
                die Wahrheit wird nie still ueberschrieben.
                """),
            new ReviewHelpSection(
                "Operationen",
                """
                RESTATE:     identisch zu bestehender Anforderung -> nur Beleg/Provenienz.
                REFINE:      konkretisiert dieselbe Anforderung -> neue Version.
                NEW_RELATED: fachlich neu, gleiches Feature (z.B. "bearbeiten" neben "anzeigen").
                NEW:         nichts Passendes im Core -> neue Anforderung.
                SUPERSEDE:   ersetzt eine alte Anforderung (alte wird superseded).
                CONTRADICT:  widerspricht dem Core -> Open Decision fuer den Menschen.
                """),
            new ReviewHelpSection(
                "Auswirkungen",
                "Nach Fertig schreibt die UI human-decisions.json. ingest-apply fuehrt die akzeptierten Operationen "
                + "deterministisch in den Core aus (state/core/project-state.json) und schreibt ein Delta + affected-view.")
        ]);

    private static ReviewItem BuildItem(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> incomingById, IReadOnlyDictionary<string, ProjectStateItem> coreById)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Suggestion, "Vorgeschlagene Operation", Describe(op, coreById)),
            new(ReviewNoteKind.Reason, "Begruendung Resolver", string.IsNullOrWhiteSpace(op.Rationale) ? "(keine)" : op.Rationale)
        };
        if (incomingById.TryGetValue(op.IncomingItemId, out var inc))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Eingehend {op.IncomingItemId}", Truncate(inc.Text, 320)));
        if (!string.IsNullOrWhiteSpace(op.TargetEntityId) && coreById.TryGetValue(op.TargetEntityId!, out var tgt))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Core-Entitaet {op.TargetEntityId}", Truncate(tgt.Text, 320)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "Operation JSON", $"op:{op.IncomingItemId}"),
            new(ContextBlockKind.Reference, $"Eingehend {op.IncomingItemId}", $"incoming:{op.IncomingItemId}")
        };
        if (!string.IsNullOrWhiteSpace(op.TargetEntityId))
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Core-Entitaet {op.TargetEntityId}", $"entity:{op.TargetEntityId}"));

        var item = new ReviewItem
        {
            ItemId = op.IncomingItemId,
            Summary = Describe(op, coreById),
            Badge = op.Kind,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, "apply"),
                new ReviewFieldValue(FieldReason, "")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string Describe(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> coreById)
    {
        string Tgt() => op.TargetEntityId is not null && coreById.TryGetValue(op.TargetEntityId, out var t)
            ? $"{op.TargetEntityId} ({Truncate(t.Text, 60)})" : op.TargetEntityId ?? "?";
        return op.Kind switch
        {
            StateChangeKind.Restate => $"RESTATE: bekannt = {Tgt()}",
            StateChangeKind.Refine => $"REFINE: verfeinert {Tgt()}",
            StateChangeKind.NewRelated => $"NEW_RELATED: neu im Feature '{op.FeatureKey}' — {Truncate(op.Statement, 80)}",
            StateChangeKind.New => $"NEW: {Truncate(op.Statement, 90)}",
            StateChangeKind.Supersede => $"SUPERSEDE: ersetzt {Tgt()}",
            StateChangeKind.Contradict => $"CONTRADICT: widerspricht {Tgt()}",
            StateChangeKind.AlreadyDecided => $"ALREADY_DECIDED: schon offene Entscheidung {Tgt()}",
            _ => $"{op.Kind}: {Truncate(op.Statement, 80)}"
        };
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
