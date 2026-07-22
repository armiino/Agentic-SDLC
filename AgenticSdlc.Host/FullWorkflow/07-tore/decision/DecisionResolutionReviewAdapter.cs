using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// HumanReview der Decision-Auflösungen (apply/skip je Op). Der Mensch autorisiert die Auflösung (die Stakeholder-
// Antwort ist der Beleg). Generische HumanReview-UI, wie ingest-review / github-forward-review.
public static class DecisionResolutionReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static ReviewSession BuildSession(string runId, DecisionResolutionPlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op, byId)).ToList();
        return new ReviewSession
        {
            SessionId = $"decision-{runId}",
            Title = "Decision-Ingestion — Open Decisions auflösen",
            Subtitle = plan.Operations.Count == 0 ? "Keine Auflösungen." : $"{plan.Operations.Count} Auflösungen. apply = übernehmen, skip = verwerfen.",
            Help = new ReviewHelp("Decision-Ingestion (Tor 2)",
                "Eine offene Decision wird durch eine Stakeholder-Antwort aufgelöst; betroffene PBIs werden entblockt.",
                [
                    new ReviewHelpSection("Outcomes",
                        "KEEP_ORIGINAL (alte Aussage gewinnt → PBI active) · ADOPT_NEW (neue Aussage gewinnt, Requirement-Swap → PBI needs_clarify) · " +
                        "REFINE (beide zusammengeführt → PBI needs_clarify)."),
                    new ReviewHelpSection("Auswirkung",
                        "Nach Fertig → human-decisions.json. decision-apply setzt DEC=resolved (contradicts bleibt als contradicts_resolved erhalten), " +
                        "entblockt PBIs, und schreibt ein github-sync-Delta → Tor 3 bringt die entblockte Arbeit nach GitHub.")
                ]),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "apply = Auflösung übernehmen · skip = verwerfen"),
                new ReviewFieldSpec(FieldReason, "Begründung (optional)", ReviewInputType.MultiLine, [], Required: false, Help: "Optionale Notiz.")
            ],
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static DecisionResolutionDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new DecisionReviewDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, DecisionResolutionDecisionsFile? file)
    {
        if (file is null) return;
        var byOp = file.Decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        foreach (var item in session.Items)
            if (byOp.TryGetValue(item.ItemId, out var d)) { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); item.Resolved = Resolved(item); }
    }

    public static string ResolveContext(string key, DecisionResolutionPlanDocument plan, ProjectStateDocument core)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx) && idx >= 0 && idx < plan.Operations.Count)
            return JsonSerializer.Serialize(plan.Operations[idx], Json);
        if (key.StartsWith("item:", StringComparison.Ordinal))
        {
            var it = core.Items.FirstOrDefault(x => string.Equals(x.ItemId, key["item:".Length..], StringComparison.Ordinal));
            return it is null ? "(unbekannt)" : JsonSerializer.Serialize(new { it.ItemId, it.ItemType, it.Status, it.Text }, Json);
        }
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewItem BuildItem(string opId, int idx, DecisionResolutionOp op, IReadOnlyDictionary<string, ProjectStateItem> byId)
    {
        var summary = $"{op.Outcome} {op.DecisionId} → Ziel {op.TargetRequirementId}, {op.AffectedPbis.Count} PBI(s)";
        var notes = new List<ReviewNote> { new(ReviewNoteKind.Reason, "Begründung", op.Rationale) };
        if (op.NewStatement is { Length: > 0 })
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Neue Aussage", op.NewStatement));
        if (op.AffectedPbis.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Betroffene PBIs", string.Join(", ", op.AffectedPbis)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "Operation JSON", $"op:{idx}"),
            new(ContextBlockKind.Reference, $"Decision {op.DecisionId}", $"item:{op.DecisionId}"),
            new(ContextBlockKind.Reference, $"Ziel {op.TargetRequirementId}", $"item:{op.TargetRequirementId}")
        };

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = op.Outcome,
            Notes = notes,
            ContextBlocks = context,
            FieldValues = [new ReviewFieldValue(FieldDecision, "apply"), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
