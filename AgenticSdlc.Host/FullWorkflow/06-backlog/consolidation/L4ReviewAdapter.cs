using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record L4HumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<L4HumanDecision> Decisions);

public sealed record L4HumanDecision(
    [property: JsonPropertyName("operationId")] string OperationId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedOperationJson")] string? EditedOperationJson,
    [property: JsonPropertyName("reason")] string? Reason);

public static class L4ReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditedOperationJson = "editedOperationJson";
    public const string FieldReason = "reason";

    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
        { "accept", "edit", "reject", "revise" };

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = Operation freigeben · edit = bearbeitete Operation übernehmen · reject = Operation verwerfen · revise = an L4-Agenten zurückgeben"),
        new ReviewFieldSpec(FieldEditedOperationJson, "Bearbeitete Operation JSON (nur bei edit)", ReviewInputType.FreeText, [], Required: false),
        new ReviewFieldSpec(FieldReason, "Begründung / Feedback", ReviewInputType.FreeText, [], Required: false,
            Help: "Für jede Entscheidung erforderlich. Bei revise ist dies die Anweisung an den Agenten.")
    ];

    public static ReviewSession BuildSession(
        string runId,
        ConsolidationPlan plan,
        ProjectStateDocument state,
        string scope)
    {
        var items = plan.Operations
            .Where(op => Include(op, scope))
            .Select(op => BuildItem(op, state, plan))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"l4-{runId}",
            Title = "L4 ConsolidationPlan — Human Review",
            Subtitle = scope switch
            {
                "all" => $"{items.Count} Planoperationen sichtbar. KEEP ist vorakzeptiert, bleibt aber editierbar.",
                "changes" => $"{items.Count} nicht-triviale Operationen zur Kontrolle: DEPRECATE/LINK/MERGE/SPLIT/REVISE/ADD.",
                _ => $"{items.Count} semantische Operationen zur Autorisierung. Projektwahrheit entsteht erst nach Review + Apply."
            },
            FieldSchema = Schema(),
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        if (string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        if (!string.Equals(decision, "edit", StringComparison.OrdinalIgnoreCase)) return true;

        var json = FieldOf(item, FieldEditedOperationJson);
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            _ = JsonSerializer.Deserialize<ConsolidationOperation>(json, Json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static L4HumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(it =>
        {
            string? V(string key) => it.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            return new L4HumanDecision(
                OperationId: it.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedOperationJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    ? V(FieldEditedOperationJson)
                    : null,
                Reason: V(FieldReason));
        }).ToList();
        return new L4HumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, L4HumanDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OperationId, (item, decision) =>
        {
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldEditedOperationJson, decision.EditedOperationJson);
            Set(item, FieldReason, decision.Reason);
        }, Resolved);

    public static string ResolveContext(string resolverKey, ProjectStateDocument state, ConsolidationPlan plan)
    {
        if (resolverKey.StartsWith("operation:", StringComparison.Ordinal))
        {
            var opId = resolverKey["operation:".Length..];
            var op = plan.Operations.FirstOrDefault(o => string.Equals(o.OperationId, opId, StringComparison.Ordinal));
            return op is null ? $"(Operation {opId} nicht gefunden)" : JsonSerializer.Serialize(op, Json);
        }
        if (resolverKey.StartsWith("item:", StringComparison.Ordinal))
        {
            var itemId = resolverKey["item:".Length..];
            var item = state.Items.FirstOrDefault(i => string.Equals(i.ItemId, itemId, StringComparison.Ordinal));
            if (item is null) return $"(Item {itemId} nicht gefunden)";
            var relations = state.Relations
                .Where(r => string.Equals(r.FromId, itemId, StringComparison.Ordinal) || string.Equals(r.ToId, itemId, StringComparison.Ordinal))
                .ToArray();
            var provenance = state.Provenance.FirstOrDefault(p => string.Equals(p.ItemId, itemId, StringComparison.Ordinal));
            return JsonSerializer.Serialize(new { item, relations, provenance }, Json);
        }
        if (resolverKey.StartsWith("target:", StringComparison.Ordinal))
        {
            var targetId = resolverKey["target:".Length..];
            var producer = FindTargetProducer(plan, targetId);
            if (producer is null) return $"(Target {targetId} nicht gefunden)";
            var target = producer.Targets.First(t => string.Equals(t.RequirementId, targetId, StringComparison.Ordinal));
            var sourceItems = producer.SourceItemIds
                .Select(id => state.Items.FirstOrDefault(i => string.Equals(i.ItemId, id, StringComparison.Ordinal)))
                .Where(i => i is not null)
                .ToArray();
            return JsonSerializer.Serialize(new { target, producedByOperation = producer.OperationId, sourceItems }, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static ReviewItem BuildItem(ConsolidationOperation op, ProjectStateDocument state, ConsolidationPlan plan)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Operation", op.Operation),
            new(op.RequiresHumanReview ? ReviewNoteKind.Warning : ReviewNoteKind.Info,
                "Human Review", op.RequiresHumanReview ? "Diese Operation veraendert Projektwahrheit und braucht Autorisierung." : "Keine Human-Review-Pflicht laut Gate-Regel."),
            new(ReviewNoteKind.Reason, "Rationale", op.Rationale ?? "(keine Rationale)")
        };

        if (op.Targets.Count > 0)
        {
            notes.Add(new(ReviewNoteKind.Info, "Targets",
                string.Join("\n\n", op.Targets.Select(t => $"{t.RequirementId} [{t.Status}]\n{t.Text}"))));
        }

        var sourceSummary = op.SourceItemIds
            .Select(id => state.Items.FirstOrDefault(i => string.Equals(i.ItemId, id, StringComparison.Ordinal)))
            .Where(i => i is not null)
            .Select(i => $"{i!.ItemId}: {Truncate(i.Text, 420)}")
            .ToArray();
        if (sourceSummary.Length > 0)
        {
            notes.Add(new ReviewNote(
                ReviewNoteKind.Info,
                op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase) ? "Verknuepfte Source-Items" : "Source-Items",
                string.Join("\n\n", sourceSummary)));
        }

        var replacementTargetIds = ExtractStringArray(op.Metadata, "replacedBy");
        if (replacementTargetIds.Count > 0)
        {
            var replacements = replacementTargetIds.Select(targetId =>
            {
                var producer = FindTargetProducer(plan, targetId);
                var target = producer?.Targets.FirstOrDefault(t => string.Equals(t.RequirementId, targetId, StringComparison.Ordinal));
                return target is null
                    ? $"{targetId}: (Target nicht im Plan gefunden)"
                    : $"{targetId} aus {producer!.OperationId} ({string.Join(", ", producer.SourceItemIds)})\n{target.Text}";
            }).ToArray();
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Ersetzt durch", string.Join("\n\n", replacements)));
        }

        if (op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add(new ReviewNote(
                ReviewNoteKind.Suggestion,
                "Was bedeutet LINK?",
                "LINK erzeugt kein neues Requirement und verwirft nichts. Es markiert eine fachliche Beziehung/Clusterung zwischen bestehenden Items, damit L4/Issue-Planning diese Items gemeinsam betrachten kann."));
        }

        if (op.Metadata.Count > 0)
            notes.Add(new(ReviewNoteKind.Info, "Metadata", JsonSerializer.Serialize(op.Metadata, Json)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "Operation JSON", $"operation:{op.OperationId}")
        };
        foreach (var sourceItemId in op.SourceItemIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Source {sourceItemId}", $"item:{sourceItemId}"));
        foreach (var targetId in replacementTargetIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Replacement {targetId}", $"target:{targetId}"));

        var item = new ReviewItem
        {
            ItemId = op.OperationId,
            Summary = $"{op.Operation}: {string.Join(", ", op.SourceItemIds)}"
                      + (sourceSummary.Length > 0 ? "\n\n" + string.Join("\n", sourceSummary) : ""),
            Badge = op.Operation,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, SuggestedDecision(op)),
                new ReviewFieldValue(FieldReason, SuggestedReason(op))
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static bool Include(ConsolidationOperation op, string scope)
        => scope switch
        {
            "all" => true,
            "changes" => !op.Operation.Equals("KEEP", StringComparison.OrdinalIgnoreCase),
            _ => op.RequiresHumanReview
        };

    private static string SuggestedDecision(ConsolidationOperation op)
        => op.Operation.Equals("KEEP", StringComparison.OrdinalIgnoreCase)
            || op.Operation.Equals("LINK", StringComparison.OrdinalIgnoreCase)
                ? "accept"
                : op.RequiresHumanReview ? "accept" : "revise";

    private static string SuggestedReason(ConsolidationOperation op)
        => op.RequiresHumanReview
            ? "auto: Gate-pflichtige semantische Operation; bitte fachlich prüfen."
            : "auto: Kontrolloperation ohne Gate-pflichtige Semantik; im Review editierbar.";

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }

    private static ConsolidationOperation? FindTargetProducer(ConsolidationPlan plan, string targetRequirementId)
        => plan.Operations.FirstOrDefault(op =>
            op.Targets.Any(t => string.Equals(t.RequirementId, targetRequirementId, StringComparison.Ordinal)));

    private static IReadOnlyList<string> ExtractStringArray(IReadOnlyDictionary<string, object?> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var raw) || raw is null) return [];
        if (raw is string s) return [s];
        if (raw is IEnumerable<string> strings) return strings.ToArray();
        if (raw is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return [element.GetString() ?? ""];
            if (element.ValueKind == JsonValueKind.Array)
                return element.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString() ?? "")
                    .Where(sv => !string.IsNullOrWhiteSpace(sv))
                    .ToArray();
        }
        return [];
    }
}
