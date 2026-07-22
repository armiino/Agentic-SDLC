using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

public sealed record PbiUpdateDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<PbiUpdateDecision> Decisions);

public sealed record PbiUpdateDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die PBI-Operationen in die generische HumanReview-UI: 1 Item je Operation (opId = op-<index>),
// Entscheidung apply/skip. Der Mensch autorisiert die Backlog-Aenderungen.
public static class PbiUpdateReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };

    public static ReviewSession BuildSession(string runId, PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        // R5: byId-Dictionary entfernt — BuildItem nutzte es nie (Test-Fund Log #24); core bleibt in der
        // Signatur (einheitliche Adapter-Form; kuenftige Kontext-Anreicherung moeglich).
        _ = core;
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", op)).ToList();
        return new ReviewSession
        {
            SessionId = $"pbi-update-{runId}",
            Title = "Incrementeller PBI-Update — Backlog-Aenderungen",
            Subtitle = plan.Operations.Count == 0 ? "Keine PBI-Aenderungen." : $"{plan.Operations.Count} PBI-Operationen. apply = uebernehmen, skip = verwerfen.",
            Help = new ReviewHelp("Incrementeller PBI-Update",
                "Ein neues Meeting hat den Core geaendert. Hier werden NUR die betroffenen PBIs aktualisiert (stabile IDs).",
                [
                    new ReviewHelpSection("Operationen",
                        "NEW_PBI (neues PBI) · EXTEND_PBI (Requirement zu PBI) · MARK_CHANGED (needs_clarify) · " +
                        "BLOCK_PBI (blocked_by_decision) · SUPERSEDE_PBI (Requirement-Swap)."),
                    new ReviewHelpSection("Auswirkung",
                        "Nach Fertig -> human-decisions.json. pbi-update-apply fuehrt die akzeptierten Operationen deterministisch aus " +
                        "(Merge mehrerer Ursachen pro PBI, stabile IDs) und schreibt ein github-sync-Delta.")
                ]),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "apply = Aenderung uebernehmen · skip = verwerfen"),
                new ReviewFieldSpec(FieldReason, "Begruendung (optional)", ReviewInputType.MultiLine, [], Required: false, Help: "Optionale Notiz.")
            ],
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static PbiUpdateDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new PbiUpdateDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, PbiUpdateDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) => { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); }, Resolved);

    public static string ResolveContext(string key, PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx) && idx >= 0 && idx < plan.Operations.Count)
            return JsonSerializer.Serialize(plan.Operations[idx], JsonFiles.Json);
        if (key.StartsWith("pbi:", StringComparison.Ordinal))
        {
            var it = core.Items.FirstOrDefault(x => string.Equals(x.ItemId, key["pbi:".Length..], StringComparison.Ordinal));
            return it is null ? "(unbekannt)" : JsonSerializer.Serialize(new { it.ItemId, it.Status, it.Pbi }, JsonFiles.Json);
        }
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewItem BuildItem(string opId, PbiStateChangeOperation op)
    {
        var idx = opId["op-".Length..];
        var target = op.PbiId ?? op.FeatureId;
        var summary = op.Kind switch
        {
            PbiUpdateKind.NewPbi => $"NEW_PBI in {op.FeatureId} fuer {op.RequirementId}",
            PbiUpdateKind.ExtendPbi => $"EXTEND {op.PbiId} um {op.RequirementId}",
            PbiUpdateKind.MarkChanged => $"MARK_CHANGED {op.PbiId} ({op.RequirementId} verfeinert)",
            PbiUpdateKind.BlockPbi => $"BLOCK {op.PbiId} (Decision {op.OpenDecisionRef})",
            PbiUpdateKind.SupersedePbi => $"SUPERSEDE in {op.PbiId}: {op.RequirementId} -> {op.ReplacementRequirementId}",
            _ => $"{op.Kind} {op.PbiId}"
        };
        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Operation JSON", $"op:{idx}") };
        if (op.PbiId is not null) context.Add(new ContextBlock(ContextBlockKind.Reference, $"PBI {op.PbiId}", $"pbi:{op.PbiId}"));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = op.Kind,
            Notes = [new ReviewNote(ReviewNoteKind.Reason, "Begruendung", op.Rationale)],
            ContextBlocks = context,
            FieldValues = [new ReviewFieldValue(FieldDecision, "apply"), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
