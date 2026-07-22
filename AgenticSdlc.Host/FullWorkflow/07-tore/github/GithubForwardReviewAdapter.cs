using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Forward-Plans (apply/skip je Op). Der Mensch autorisiert den PLAN — der GitHub-Write selbst
// passiert erst im gated Apply (T3.4). Generische HumanReview-UI, wie pbi-update-review / ingest-review.
public static class GithubForwardReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static ReviewSession BuildSession(string runId, GithubForwardPlanDocument plan)
    {
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op)).ToList();
        return new ReviewSession
        {
            SessionId = $"github-forward-{runId}",
            Title = "GitHub Forward-Reconciliation — Delta gegen GitHub",
            Subtitle = plan.Operations.Count == 0 ? "Keine Operationen." : $"{plan.Operations.Count} Operationen. apply = autorisieren, skip = verwerfen.",
            Help = new ReviewHelp("GitHub Forward-Reconciliation",
                "Der github-sync-Delta wird gegen GitHub abgeglichen. Kein Write vor dieser Freigabe.",
                [
                    new ReviewHelpSection("Operationen",
                        "CREATE_ISSUE (neu, nur nach ausgefuehrter Suche) · UPDATE_ISSUE (Patch-Vorschlag) · COMMENT · " +
                        "LINK (bestehendes Issue zuordnen) · NO_CHANGE · FLAG_DRIFT (manuell pruefen) · HOLD_BLOCKED (wartet auf Tor 2)."),
                    new ReviewHelpSection("Auswirkung",
                        "Nach Fertig -> human-decisions.json. Der gated Apply (T3.4) fuehrt NUR die akzeptierten Ops aus " +
                        "und schreibt das Mapping (implemented_by_issue) in den Core zurueck.")
                ]),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "apply = Op autorisieren · skip = verwerfen"),
                new ReviewFieldSpec(FieldReason, "Begruendung (optional)", ReviewInputType.MultiLine, [], Required: false, Help: "Optionale Notiz.")
            ],
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static GithubForwardDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new GithubForwardDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, GithubForwardDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) => { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); }, Resolved);

    public static string ResolveContext(string key, GithubForwardPlanDocument plan)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx) && idx >= 0 && idx < plan.Operations.Count)
            return JsonSerializer.Serialize(plan.Operations[idx], Json);
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewItem BuildItem(string opId, int idx, GithubForwardOp op)
    {
        var target = op.TargetIssueNumber is null ? "" : $" -> #{op.TargetIssueNumber}";
        var summary = op.Kind switch
        {
            GithubForwardKind.CreateIssue => $"CREATE_ISSUE fuer {op.PbiId}: {op.Title}",
            GithubForwardKind.UpdateIssue => $"UPDATE_ISSUE {op.PbiId}{target}",
            GithubForwardKind.Comment => $"COMMENT {op.PbiId}{target}",
            GithubForwardKind.Link => $"LINK {op.PbiId}{target}",
            GithubForwardKind.FlagDrift => $"FLAG_DRIFT {op.PbiId}{target}",
            GithubForwardKind.HoldBlocked => $"HOLD_BLOCKED {op.PbiId} (blockiert)",
            GithubForwardKind.NoChange => $"NO_CHANGE {op.PbiId}",
            _ => $"{op.Kind} {op.PbiId}"
        };
        var notes = new List<ReviewNote> { new(ReviewNoteKind.Reason, "Begruendung", op.Rationale) };
        if (op.SearchedQueries is { Count: > 0 })
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Suche (Rev-3)", $"{string.Join(" | ", op.SearchedQueries)} — {op.SearchEvidence}"));
        if (!string.IsNullOrWhiteSpace(op.Anchor))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Anker", op.Anchor));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = $"{op.Kind} · {op.Origin}",
            Notes = notes,
            ContextBlocks = [new ContextBlock(ContextBlockKind.Generic, "Operation JSON", $"op:{idx}")],
            FieldValues = [new ReviewFieldValue(FieldDecision, "apply"), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
