using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// HumanReview des Reverse-Plans. Zentraler Punkt (E4): bei PBI_DONE VERIFIZIERT der Mensch, dass die Arbeit
// wirklich fertig ist — apply setzt dann done, skip verwirft. Generische HumanReview-UI, wie github-forward-review.
public static class GithubReverseReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static ReviewSession BuildSession(string runId, GithubReversePlanDocument plan)
    {
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op)).ToList();
        return new ReviewSession
        {
            SessionId = $"github-reverse-{runId}",
            Title = "GitHub Reverse-Feedback — verifizierter StateChange",
            Subtitle = plan.Operations.Count == 0 ? "Keine Reverse-Vorschlaege." : $"{plan.Operations.Count} Vorschlaege. apply = uebernehmen, skip = verwerfen.",
            Help = new ReviewHelp("GitHub Reverse-Feedback-Ingestion",
                "GitHub-Zustand wird NIE automatisch Wahrheit. Ein geschlossenes Issue ist nur ein Vorschlag.",
                [
                    new ReviewHelpSection("Verifikation (E4)",
                        "PBI_DONE bedeutet NICHT 'Issue zu = fertig'. Setze apply NUR, wenn du bestaetigst, dass die " +
                        "Arbeit wirklich abgeschlossen ist. Sonst skip. `done` entsteht ausschliesslich ueber diese Freigabe."),
                    new ReviewHelpSection("Operationen",
                        "PBI_DONE (Status -> done, verifikationspflichtig) · MAPPING_SYNC_CLOSED (nur Mapping-Status) · " +
                        "FLAG_REOPENED (Drift, kein Auto-Change).")
                ]),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "apply = uebernehmen (bei PBI_DONE = fertig verifiziert) · skip = verwerfen"),
                new ReviewFieldSpec(FieldReason, "Begruendung (optional)", ReviewInputType.MultiLine, [], Required: false, Help: "Optionale Notiz.")
            ],
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static GithubReverseDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new GithubReverseDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, GithubReverseDecisionsFile? file)
    {
        if (file is null) return;
        var byOp = file.Decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        foreach (var item in session.Items)
            if (byOp.TryGetValue(item.ItemId, out var d)) { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); item.Resolved = Resolved(item); }
    }

    public static string ResolveContext(string key, GithubReversePlanDocument plan)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx) && idx >= 0 && idx < plan.Operations.Count)
            return JsonSerializer.Serialize(plan.Operations[idx], Json);
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewItem BuildItem(string opId, int idx, GithubReverseOp op)
    {
        var summary = op.Kind switch
        {
            GithubReverseKind.PbiDone => $"PBI_DONE? {op.PbiId} (#{op.IssueNumber}) — {op.CurrentPbiStatus} -> done, VERIFIZIEREN",
            GithubReverseKind.MappingSyncClosed => $"MAPPING_SYNC_CLOSED {op.PbiId} (#{op.IssueNumber})",
            GithubReverseKind.FlagReopened => $"FLAG_REOPENED {op.PbiId} (#{op.IssueNumber})",
            _ => $"{op.Kind} {op.PbiId}"
        };
        var notes = new List<ReviewNote> { new(ReviewNoteKind.Reason, "Begruendung", op.Rationale) };
        if (op.RequiresVerification)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Verifikation noetig (E4)", "apply NUR, wenn die Arbeit wirklich fertig ist."));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = op.Kind,
            Notes = notes,
            ContextBlocks = [new ContextBlock(ContextBlockKind.Generic, "Operation JSON", $"op:{idx}")],
            // Default bewusst NICHT apply bei verifikationspflichtigen Ops — der Mensch muss aktiv entscheiden.
            FieldValues = [new ReviewFieldValue(FieldDecision, ""), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string FieldOf(ReviewItem item, string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";
    private static void Set(ReviewItem item, string key, string? value) { item.FieldValues.RemoveAll(f => f.FieldKey == key); item.FieldValues.Add(new ReviewFieldValue(key, value)); }
}
