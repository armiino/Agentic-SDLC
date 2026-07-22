using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record BacklogHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<BacklogHumanDecision> Decisions);

public sealed record BacklogHumanDecision(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedPbiJson")] string? EditedPbiJson,
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die Product Backlog Items in die generische HumanReview-UI (per-Item, wie IssuePlanning).
// Der Mensch gibt PBIs frei, passt Akzeptanzkriterien/MVP/Prioritaet/Readiness an, oder verwirft/revidiert.
// Die vom Agenten aufgeworfenen offenen Entscheidungen sind sichtbar (blockierende hervorgehoben).
public static class ReClarifyBacklogReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditStatement = "editStatement";
    public const string FieldEditAcceptance = "editAcceptanceCriteria";
    public const string FieldEditMvp = "editMvp";
    public const string FieldEditPriority = "editPriorityRank";
    public const string FieldEditReadiness = "editReadiness";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "accept", "edit", "reject", "revise" };

    public static ReviewSession BuildSession(string runId, CanonicalRequirementsBaseline baseline, ProductBacklogDocument backlog, ReClarifyGateReport gate, string scope)
    {
        var byReq = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var items = backlog.Items
            .Where(p => Include(p, scope))
            .Select(p => BuildItem(p, byReq, gate))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"l4-re-clarify-backlog-{runId}",
            Title = "L4 Re-Clarify — Product Backlog Review",
            Subtitle = scope == "blocked"
                ? $"{items.Count} PBIs mit blockierenden offenen Entscheidungen. Gate={(gate.Pass ? "pass" : "fail")}."
                : $"{items.Count} Product Backlog Items. accept/edit/reject/revise. Gate={(gate.Pass ? "pass" : "fail")}.",
            Help = BuildHelp(),
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
        return HasValidEditOverrides(item);
    }

    public static BacklogHumanDecisionsFile Apply(string runId, ReviewSession session, ProductBacklogDocument backlog)
    {
        var originals = backlog.Items.ToDictionary(i => i.PbiId, StringComparer.Ordinal);
        var decisions = session.Items.Select(item =>
        {
            string? V(string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(item.ItemId, out var original);
            return new BacklogHumanDecision(
                PbiId: item.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedPbiJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedPbi(item, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                        : null,
                Reason: V(FieldReason));
        }).ToList();
        return new BacklogHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, BacklogHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byItem = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.PbiId))
            .GroupBy(d => d.PbiId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byItem.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedPbiJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<ProductBacklogItem>(decision.EditedPbiJson, Json);
                    if (edited is not null) SetEditedFields(item, edited);
                }
                catch { Set(item, FieldEditStatement, decision.EditedPbiJson); }
            }
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, CanonicalRequirementsBaseline baseline, ProductBacklogDocument backlog, ReClarifyGateReport gate)
    {
        if (resolverKey.StartsWith("pbi:", StringComparison.Ordinal))
        {
            var id = resolverKey["pbi:".Length..];
            var p = backlog.Items.FirstOrDefault(x => string.Equals(x.PbiId, id, StringComparison.Ordinal));
            return p is null ? $"(PBI {id} nicht gefunden)" : JsonSerializer.Serialize(p, Json);
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var r = baseline.Requirements.FirstOrDefault(x => string.Equals(x.RequirementId, id, StringComparison.Ordinal));
            return r is null ? $"(Requirement {id} nicht gefunden)" : JsonSerializer.Serialize(r, Json);
        }
        if (resolverKey.StartsWith("gate:", StringComparison.Ordinal))
        {
            var id = resolverKey["gate:".Length..];
            var issues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.SubjectId, id, StringComparison.Ordinal)).ToArray();
            return JsonSerializer.Serialize(issues, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = PBI freigeben · edit = anpassen · reject = verwerfen · revise = mit Feedback zurueck an Clarify-Agent"),
        new ReviewFieldSpec(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditStatement, "Edit: Statement", ReviewInputType.MultiLine, [], Required: false,
            Help: "Als <Rolle> will ich ... Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditAcceptance, "Edit: Akzeptanzkriterien", ReviewInputType.MultiLine, [], Required: false,
            Help: "Eine Zeile pro Kriterium. Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditMvp, "Edit: MVP", ReviewInputType.Dropdown,
            ["", "mvp", "required_for_mvp", "later", "out_of_scope", "undecided"], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditPriority, "Edit: priorityRank", ReviewInputType.FreeText, [], Required: false,
            Help: "Ganzzahl. Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditReadiness, "Edit: Readiness", ReviewInputType.Dropdown,
            ["", "backlog_ready", "ready_with_nonblocking_questions", "blocked_by_decision"], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldReason, "Begruendung / Feedback", ReviewInputType.MultiLine, [], Required: false,
            Help: "Warum. Bei revise = Anweisung an den Clarify-Agent.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "L4 Re-Clarify Product Backlog Review",
        Summary: "Du gibst die aus den Feature-Clustern geschnittenen PBIs frei, bevor daraus Issues geplant werden.",
        Sections:
        [
            new ReviewHelpSection("Ziel",
                "Der Clarify-Agent hat jedes Feature in PBIs geschnitten (Akzeptanzkriterien + explizite offene Entscheidungen). "
                + "Du bestaetigst/korrigierst. Offene Entscheidungen sind sichtbar; blockierende sind hervorgehoben."),
            new ReviewHelpSection("Entscheidung",
                "accept = PBI ist gut. edit = Titel/Statement/Kriterien/MVP/Prioritaet/Readiness anpassen. "
                + "reject = PBI raus. revise = Clarify-Agent soll neu schneiden (Feedback in Begruendung)."),
            new ReviewHelpSection("Auswirkungen",
                "Nach Fertig schreibt die UI human-decisions.json. l4-re-clarify-backlog-apply materialisiert die "
                + "akzeptierten/edited PBIs als ProductBacklogView (mit erneutem DoR-Gate).")
        ]);

    private static ReviewItem BuildItem(ProductBacklogItem p, IReadOnlyDictionary<string, CanonicalRequirement> byReq, ReClarifyGateReport gate)
    {
        var reqs = p.RequirementIds.Select(id => byReq.TryGetValue(id, out var r) ? $"{id}: {r.Title}" : $"{id}: (unbekannt)").ToArray();
        var open = p.OpenDecisions.Select(o => $"[{o.Kind}/{o.Resolution}{(o.BlocksScope ? "/BLOCKS" : "")}] {o.Question}").ToArray();
        var gateIssues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.SubjectId, p.PbiId, StringComparison.Ordinal)).ToArray();

        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Type / MVP / Readiness", $"{p.Type} · {p.Mvp ?? "-"} · {p.Readiness ?? "-"} · rank={p.PriorityRank?.ToString() ?? "-"}"),
            new(ReviewNoteKind.Reason, "Statement", p.Goal ?? "(kein Statement)"),
            new(ReviewNoteKind.Info, $"Akzeptanzkriterien ({p.AcceptanceCriteria.Count})", p.AcceptanceCriteria.Count == 0 ? "(keine)" : string.Join("\n", p.AcceptanceCriteria.Select(c => "- " + c))),
            new(p.OpenDecisions.Any(o => o.BlocksScope) ? ReviewNoteKind.Warning : ReviewNoteKind.Suggestion,
                $"Offene Entscheidungen ({p.OpenDecisions.Count})", open.Length == 0 ? "(keine)" : string.Join("\n", open.Select(o => "- " + o))),
            new(ReviewNoteKind.Info, "Requirements", string.Join("\n", reqs))
        };
        foreach (var gi in gateIssues)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, gi.Code, gi.Message));

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "PBI JSON", $"pbi:{p.PbiId}") };
        foreach (var id in p.RequirementIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Requirement {id}", $"requirement:{id}"));

        var item = new ReviewItem
        {
            ItemId = p.PbiId,
            Summary = $"{p.Title}\n\n{Truncate(p.Goal ?? "", 500)}",
            Badge = p.Readiness ?? p.Type,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, "accept"),
                new ReviewFieldValue(FieldEditTitle, ""),
                new ReviewFieldValue(FieldEditStatement, ""),
                new ReviewFieldValue(FieldEditAcceptance, ""),
                new ReviewFieldValue(FieldEditMvp, ""),
                new ReviewFieldValue(FieldEditPriority, ""),
                new ReviewFieldValue(FieldEditReadiness, ""),
                new ReviewFieldValue(FieldReason, "auto: PBI aus L4ReClarifyBacklogAgent; bitte fachlich pruefen.")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static bool Include(ProductBacklogItem p, string scope)
        => scope switch
        {
            "blocked" => p.OpenDecisions.Any(o => o.BlocksScope) || string.Equals(p.Readiness, "blocked_by_decision", StringComparison.OrdinalIgnoreCase),
            _ => true
        };

    private static bool TryBuildEditedPbi(ReviewItem item, ProductBacklogItem? original, out ProductBacklogItem? edited)
    {
        edited = null;
        if (original is null) return false;
        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original.Title);
        if (string.IsNullOrWhiteSpace(title)) return false;
        int? rank = original.PriorityRank;
        var rankRaw = FieldOf(item, FieldEditPriority);
        if (!string.IsNullOrWhiteSpace(rankRaw) && int.TryParse(rankRaw, out var parsed)) rank = parsed;

        edited = new ProductBacklogItem(
            PbiId: original.PbiId,
            IdentityKey: original.IdentityKey,
            Version: original.Version + 1,
            Type: original.Type,
            Title: title,
            RequirementIds: original.RequirementIds)
        {
            Goal = OverrideOrOriginal(FieldOf(item, FieldEditStatement), original.Goal),
            Scope = original.Scope,
            AcceptanceCriteria = OverrideList(FieldOf(item, FieldEditAcceptance), original.AcceptanceCriteria),
            OpenDecisions = original.OpenDecisions,
            Mvp = OverrideOrOriginal(FieldOf(item, FieldEditMvp), original.Mvp),
            PriorityRank = rank,
            Dependencies = original.Dependencies,
            Readiness = OverrideOrOriginal(FieldOf(item, FieldEditReadiness), original.Readiness),
            Traceability = original.Traceability,
            Provenance = original.Provenance
        };
        return HasValidEditOverrides(item);
    }

    private static bool HasValidEditOverrides(ReviewItem item)
        => !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditTitle))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditStatement))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditAcceptance))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditMvp))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditPriority))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditReadiness));

    private static void SetEditedFields(ReviewItem item, ProductBacklogItem edited)
    {
        Set(item, FieldEditTitle, edited.Title);
        Set(item, FieldEditStatement, edited.Goal);
        Set(item, FieldEditAcceptance, string.Join('\n', edited.AcceptanceCriteria));
        Set(item, FieldEditMvp, edited.Mvp);
        Set(item, FieldEditPriority, edited.PriorityRank?.ToString());
        Set(item, FieldEditReadiness, edited.Readiness);
    }

    private static IReadOnlyList<string> OverrideList(string raw, IReadOnlyList<string> original)
    {
        if (string.IsNullOrWhiteSpace(raw)) return original;
        return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static string? OverrideOrOriginal(string? overrideValue, string? originalValue)
        => string.IsNullOrWhiteSpace(overrideValue) ? originalValue : overrideValue.Trim();

    private static string FieldOf(ReviewItem item, string key)
        => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";

    private static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
