using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record ClarificationPlanningHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<ClarificationPlanningHumanDecision> Decisions);

public sealed record ClarificationPlanningHumanDecision(
    [property: JsonPropertyName("clarificationPlanId")] string ClarificationPlanId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedClarificationPlanItemJson")] string? EditedClarificationPlanItemJson,
    [property: JsonPropertyName("reason")] string? Reason);

public static class ClarificationPlanningReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditOperation = "editOperation";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditQuestion = "editQuestion";
    public const string FieldEditDescription = "editDescription";
    public const string FieldEditAcceptanceCriteria = "editAcceptanceCriteria";
    public const string FieldEditLabels = "editLabels";
    public const string FieldEditPriority = "editPriority";
    public const string FieldEditRequiresHumanReview = "editRequiresHumanReview";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
        { "accept", "edit", "reject", "revise" };
    private static readonly HashSet<string> Operations = new(StringComparer.OrdinalIgnoreCase)
        { "CREATE_CLARIFICATION_ISSUE", "CREATE_BREAKDOWN_ISSUE", "MERGE_WITH_EXISTING_CLARIFICATION", "DEFER", "NO_ACTION" };

    public static ReviewSession BuildSession(
        string runId,
        ClarificationPlanningInput input,
        ClarificationPlanDocument plan,
        ClarificationPlanGateReport gate,
        string scope)
    {
        var warningIds = gate.Warnings
            .Where(w => !string.IsNullOrWhiteSpace(w.ClarificationPlanId))
            .Select(w => w.ClarificationPlanId!)
            .ToHashSet(StringComparer.Ordinal);
        var items = plan.Items
            .Where(item => Include(item, warningIds, scope))
            .Select(item => BuildItem(item, input, gate))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"clarification-agent-{runId}",
            Title = "Clarification Planning — Human Review",
            Subtitle = scope switch
            {
                "warnings" => $"{items.Count} ClarificationPlanItems mit Gate-Warnungen.",
                "needs-human" => $"{items.Count} reviewpflichtige ClarificationPlanItems.",
                _ => $"{items.Count} ClarificationPlanItems sichtbar. Klaerungs-/Breakdown-Arbeit wird hier autorisiert."
            },
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

    public static ClarificationPlanningHumanDecisionsFile Apply(string runId, ReviewSession session, ClarificationPlanDocument plan)
    {
        var originals = plan.Items.ToDictionary(i => i.ClarificationPlanId, StringComparer.Ordinal);
        var decisions = session.Items.Select(item =>
        {
            string? V(string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(item.ItemId, out var original);
            return new ClarificationPlanningHumanDecision(
                ClarificationPlanId: item.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedClarificationPlanItemJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedPlanItem(item, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                        : null,
                Reason: V(FieldReason));
        }).ToList();
        return new ClarificationPlanningHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, ClarificationPlanningHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byItem = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.ClarificationPlanId))
            .GroupBy(d => d.ClarificationPlanId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byItem.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedClarificationPlanItemJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<ClarificationPlanItem>(decision.EditedClarificationPlanItemJson, Json);
                    if (edited is not null) SetEditedFields(item, edited);
                }
                catch
                {
                    Set(item, FieldEditDescription, decision.EditedClarificationPlanItemJson);
                }
            }
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, ClarificationPlanningInput input, ClarificationPlanDocument plan, ClarificationPlanGateReport gate)
    {
        if (resolverKey.StartsWith("plan:", StringComparison.Ordinal))
        {
            var id = resolverKey["plan:".Length..];
            var item = plan.Items.FirstOrDefault(i => string.Equals(i.ClarificationPlanId, id, StringComparison.Ordinal));
            return item is null ? $"(ClarificationPlanItem {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("clarification:", StringComparison.Ordinal))
        {
            var id = resolverKey["clarification:".Length..];
            var item = input.Items.FirstOrDefault(i => string.Equals(i.ClarificationId, id, StringComparison.Ordinal));
            return item is null ? $"(ClarificationItem {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("gate:", StringComparison.Ordinal))
        {
            var id = resolverKey["gate:".Length..];
            var issues = gate.Errors.Concat(gate.Warnings)
                .Where(i => string.Equals(i.ClarificationPlanId, id, StringComparison.Ordinal)
                            || i.SourceClarificationIds.Contains(id, StringComparer.Ordinal)
                            || i.SourceRequirementIds.Contains(id, StringComparer.Ordinal))
                .ToArray();
            return JsonSerializer.Serialize(issues, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = Planitem freigeben · edit = Felder aendern · reject = entfernen · revise = mit Feedback zurueck an Agent"),
        new ReviewFieldSpec(FieldEditOperation, "Edit: Operation", ReviewInputType.Dropdown,
            ["", "CREATE_CLARIFICATION_ISSUE", "CREATE_BREAKDOWN_ISSUE", "MERGE_WITH_EXISTING_CLARIFICATION", "DEFER", "NO_ACTION"], Required: false,
            Help: "Leer lassen = Original-Operation behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Titel behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditQuestion, "Edit: Klaerungsfrage", ReviewInputType.MultiLine, [], Required: false,
            Help: "Leer lassen = Original-Frage behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditDescription, "Edit: Beschreibung", ReviewInputType.MultiLine, [], Required: false,
            Help: "Leer lassen = Original-Beschreibung behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditAcceptanceCriteria, "Edit: Acceptance Criteria", ReviewInputType.MultiLine, [], Required: false,
            Help: "Eine Zeile pro Kriterium. Leer lassen = Original-Kriterien behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditLabels, "Edit: Labels", ReviewInputType.FreeText, [], Required: false,
            Help: "Kommagetrennt. Leer lassen = Original-Labels behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditPriority, "Edit: Priority", ReviewInputType.Dropdown,
            ["", "high", "medium", "low"], Required: false,
            Help: "Leer lassen = Original-Priority behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditRequiresHumanReview, "Edit: Requires Human Review", ReviewInputType.Dropdown,
            ["", "true", "false"], Required: false,
            Help: "Leer lassen = Original-Wert behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldReason, "Begruendung / Feedback", ReviewInputType.MultiLine, [], Required: false,
            Help: "Warum akzeptiert/geaendert/verworfen/revidiert wird. Bei revise ist dies die Anweisung an den Agenten.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "Clarification Planning Review",
        Summary: "Hier pruefst du den plan-only Schnitt fuer offene Requirements, bevor daraus Klaerungs-/Breakdown-Arbeit wird.",
        Sections:
        [
            new ReviewHelpSection(
                "Ziel dieses Schritts",
                """
                Der ClarificationPlanningAgent hat offene Requirements aus dem Operationalization Audit in Klaerungs- oder
                Breakdown-Planitems ueberfuehrt.

                Dieser Plan erzeugt noch keine GitHub-Issues. Er beschreibt nur, welche offenen Punkte als Klaerungsarbeit,
                Breakdown-Arbeit, Defer oder No-Action weiterlaufen sollen.
                """),
            new ReviewHelpSection(
                "Entscheidung",
                """
                accept:
                Das Planitem ist fachlich sinnvoll und darf spaeter in den akzeptierten ClarificationPlan.

                edit:
                Der Schnitt ist grundsaetzlich richtig, aber Operation, Titel, Frage, Beschreibung, Kriterien, Labels oder
                Priority sollen angepasst werden. Edit-Felder erscheinen erst bei dieser Auswahl.

                reject:
                Das Planitem soll nicht weitergegeben werden.

                revise:
                Der Agent soll den Punkt spaeter ueberarbeiten. Schreibe das notwendige Feedback in Begruendung / Feedback.
                """),
            new ReviewHelpSection(
                "Operationen",
                """
                CREATE_CLARIFICATION_ISSUE:
                Spaeter soll daraus wahrscheinlich ein Klaerungsissue entstehen.

                CREATE_BREAKDOWN_ISSUE:
                Spaeter soll daraus wahrscheinlich ein Zerlegungs-/Discovery-Issue entstehen.

                MERGE_WITH_EXISTING_CLARIFICATION:
                Mehrere offene Punkte werden bewusst in einer Klaerung gebuendelt.

                DEFER:
                Der Punkt bleibt bewusst fuer spaeter offen.

                NO_ACTION:
                Keine operative Klaerungsarbeit noetig.
                """),
            new ReviewHelpSection(
                "Auswirkungen",
                """
                Nach Fertig schreibt die UI human-decisions.json in den Clarification-Agent-Run.

                `clarification-agent-apply` erzeugt daraus einen akzeptierten ClarificationPlan.
                Erst ein spaeterer GitHub-Reconciliation/Write-Schritt darf daraus GitHub-Aktionen vorbereiten.
                """)
        ]);

    private static ReviewItem BuildItem(ClarificationPlanItem item, ClarificationPlanningInput input, ClarificationPlanGateReport gate)
    {
        var gateIssues = gate.Errors.Concat(gate.Warnings)
            .Where(i => string.Equals(i.ClarificationPlanId, item.ClarificationPlanId, StringComparison.Ordinal))
            .ToArray();
        var sourceItems = item.SourceClarificationIds
            .Select(id => input.Items.FirstOrDefault(i => string.Equals(i.ClarificationId, id, StringComparison.Ordinal)))
            .Where(i => i is not null)
            .Select(i => $"{i!.ClarificationId} -> {i.SourceRequirementId}: {i.RequirementTitle}\nFrage: {i.Question}\nRationale: {i.Rationale}")
            .ToArray();

        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Operation", item.Operation),
            new(ReviewNoteKind.Info, "Priority", item.Priority),
            new(item.RequiresHumanReview ? ReviewNoteKind.Warning : ReviewNoteKind.Info,
                "Human Review",
                item.RequiresHumanReview ? "Dieses Planitem wurde als reviewpflichtig markiert." : "Nicht explizit reviewpflichtig, aber editierbar."),
            new(ReviewNoteKind.Reason, "Klaerungsfrage", item.Question),
            new(ReviewNoteKind.Info, "Source Items", string.Join("\n\n", sourceItems)),
            new(ReviewNoteKind.Info, "Acceptance Criteria", item.AcceptanceCriteria.Count == 0 ? "(keine)" : string.Join("\n", item.AcceptanceCriteria.Select(c => "- " + c)))
        };
        foreach (var gateIssue in gateIssues)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, gateIssue.Code, gateIssue.Message));
        if (item.Metadata.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Metadata", JsonSerializer.Serialize(item.Metadata, Json)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "ClarificationPlanItem JSON", $"plan:{item.ClarificationPlanId}"),
            new(ContextBlockKind.Generic, "Gate Issues", $"gate:{item.ClarificationPlanId}")
        };
        foreach (var clarificationId in item.SourceClarificationIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Clarification {clarificationId}", $"clarification:{clarificationId}"));

        var reviewItem = new ReviewItem
        {
            ItemId = item.ClarificationPlanId,
            Summary = $"{item.Title}\n\n{Truncate(item.Description, 800)}",
            Badge = item.Operation,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, "accept"),
                new ReviewFieldValue(FieldEditOperation, ""),
                new ReviewFieldValue(FieldEditTitle, ""),
                new ReviewFieldValue(FieldEditQuestion, ""),
                new ReviewFieldValue(FieldEditDescription, ""),
                new ReviewFieldValue(FieldEditAcceptanceCriteria, ""),
                new ReviewFieldValue(FieldEditLabels, ""),
                new ReviewFieldValue(FieldEditPriority, ""),
                new ReviewFieldValue(FieldEditRequiresHumanReview, ""),
                new ReviewFieldValue(FieldReason, SuggestedReason(item, gateIssues))
            ]
        };
        reviewItem.Resolved = Resolved(reviewItem);
        return reviewItem;
    }

    private static bool Include(ClarificationPlanItem item, HashSet<string> warningIds, string scope)
        => scope switch
        {
            "needs-human" => item.RequiresHumanReview || warningIds.Contains(item.ClarificationPlanId),
            "warnings" => warningIds.Contains(item.ClarificationPlanId),
            _ => true
        };

    private static string SuggestedReason(ClarificationPlanItem item, IReadOnlyList<ClarificationPlanGateIssue> gateIssues)
    {
        if (gateIssues.Count > 0) return "auto: Gate-Warnung vorhanden; Buendelung/Schnitt bitte fachlich pruefen.";
        if (item.Operation.Equals("DEFER", StringComparison.OrdinalIgnoreCase)) return "auto: Agent plant bewusstes Defer; fachlich pruefen.";
        if (item.Operation.Equals("NO_ACTION", StringComparison.OrdinalIgnoreCase)) return "auto: Agent plant keine weitere Aktion; fachlich pruefen.";
        return "auto: ClarificationPlanItem aus ClarificationPlanningAgent; bitte fachlich pruefen.";
    }

    private static bool TryBuildEditedPlanItem(ReviewItem item, ClarificationPlanItem? original, out ClarificationPlanItem? edited)
    {
        edited = null;
        var operation = OverrideOrOriginal(FieldOf(item, FieldEditOperation), original?.Operation);
        if (string.IsNullOrWhiteSpace(operation) || !Operations.Contains(operation)) return false;
        var requiresHumanReviewRaw = OverrideOrOriginal(FieldOf(item, FieldEditRequiresHumanReview), original?.RequiresHumanReview.ToString().ToLowerInvariant());
        if (!bool.TryParse(requiresHumanReviewRaw, out var requiresHumanReview)) return false;

        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original?.Title);
        var question = OverrideOrOriginal(FieldOf(item, FieldEditQuestion), original?.Question);
        var description = OverrideOrOriginal(FieldOf(item, FieldEditDescription), original?.Description);
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(description)) return false;

        edited = new ClarificationPlanItem(
            ClarificationPlanId: item.ItemId,
            Operation: operation,
            Title: title,
            Question: question,
            Description: description,
            SourceClarificationIds: original?.SourceClarificationIds ?? [],
            SourceRequirementIds: original?.SourceRequirementIds ?? [],
            AcceptanceCriteria: OverrideList(FieldOf(item, FieldEditAcceptanceCriteria), original?.AcceptanceCriteria ?? [], splitLines: true),
            Labels: OverrideList(FieldOf(item, FieldEditLabels), original?.Labels ?? [], splitLines: false),
            Priority: OverrideOrOriginal(FieldOf(item, FieldEditPriority), original?.Priority),
            RequiresHumanReview: requiresHumanReview,
            Metadata: original?.Metadata ?? new Dictionary<string, object?>(StringComparer.Ordinal));
        return HasValidEditOverrides(item);
    }

    private static bool HasValidEditOverrides(ReviewItem item)
    {
        var operation = FieldOf(item, FieldEditOperation);
        var requiresHumanReview = FieldOf(item, FieldEditRequiresHumanReview);
        if (!string.IsNullOrWhiteSpace(operation) && !Operations.Contains(operation)) return false;
        if (!string.IsNullOrWhiteSpace(requiresHumanReview) && !bool.TryParse(requiresHumanReview, out _)) return false;

        return !string.IsNullOrWhiteSpace(operation)
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditTitle))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditQuestion))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditDescription))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditAcceptanceCriteria))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditLabels))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditPriority))
               || !string.IsNullOrWhiteSpace(requiresHumanReview);
    }

    private static void SetEditedFields(ReviewItem item, ClarificationPlanItem edited)
    {
        Set(item, FieldEditOperation, edited.Operation);
        Set(item, FieldEditTitle, edited.Title);
        Set(item, FieldEditQuestion, edited.Question);
        Set(item, FieldEditDescription, edited.Description);
        Set(item, FieldEditAcceptanceCriteria, string.Join('\n', edited.AcceptanceCriteria));
        Set(item, FieldEditLabels, string.Join(", ", edited.Labels));
        Set(item, FieldEditPriority, edited.Priority);
        Set(item, FieldEditRequiresHumanReview, edited.RequiresHumanReview ? "true" : "false");
    }

    private static IReadOnlyList<string> OverrideList(string raw, IReadOnlyList<string> original, bool splitLines)
    {
        if (string.IsNullOrWhiteSpace(raw)) return original;
        var parts = splitLines
            ? raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static string OverrideOrOriginal(string? overrideValue, string? originalValue)
        => string.IsNullOrWhiteSpace(overrideValue) ? originalValue ?? "" : overrideValue.Trim();

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
