using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record GithubActionHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<GithubActionHumanDecision> Decisions);

public sealed record GithubActionHumanDecision(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedActionJson")] string? EditedActionJson,
    [property: JsonPropertyName("reason")] string? Reason);

public static class GithubReconciliationReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditOperation = "editOperation";
    public const string FieldEditTargetIssueNumber = "editTargetIssueNumber";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditBody = "editBody";
    public const string FieldEditAcceptanceCriteria = "editAcceptanceCriteria";
    public const string FieldEditLabels = "editLabels";
    public const string FieldEditRequiresHumanReview = "editRequiresHumanReview";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "accept", "edit", "reject", "revise" };
    private static readonly HashSet<string> Operations = new(StringComparer.OrdinalIgnoreCase) { "CREATE", "UPDATE", "LINK", "REOPEN", "NO_CHANGE", "NEEDS_REVIEW" };

    public static ReviewSession BuildSession(string runId, GithubReconciliationInput input, GithubActionPlanDocument plan, GithubActionPlanGateReport gate, string scope)
    {
        var warningIds = gate.Warnings.Where(w => !string.IsNullOrWhiteSpace(w.ActionId)).Select(w => w.ActionId!).ToHashSet(StringComparer.Ordinal);
        var items = plan.Actions
            .Where(action => Include(action, warningIds, scope))
            .Select(action => BuildItem(action, input, gate))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"github-reconciliation-{runId}",
            Title = "GitHub Reconciliation — Human Review",
            Subtitle = scope == "needs-human"
                ? $"{items.Count} reviewpflichtige GitHubActionPlanItems. Es wird noch nichts nach GitHub geschrieben."
                : $"{items.Count} GitHubActionPlanItems sichtbar. Alle sind editierbar.",
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
        if (!decision.Equals("edit", StringComparison.OrdinalIgnoreCase)) return true;
        return HasValidEditOverrides(item);
    }

    public static GithubActionHumanDecisionsFile Apply(string runId, ReviewSession session, GithubActionPlanDocument plan)
    {
        var originals = plan.Actions.ToDictionary(a => a.ActionId, StringComparer.Ordinal);
        var decisions = session.Items.Select(item =>
        {
            string? V(string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(item.ItemId, out var original);
            return new GithubActionHumanDecision(
                ActionId: item.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedActionJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedAction(item, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                        : null,
                Reason: V(FieldReason));
        }).ToList();
        return new GithubActionHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, GithubActionHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byAction = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.ActionId))
            .GroupBy(d => d.ActionId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byAction.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedActionJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<GithubActionPlanItem>(decision.EditedActionJson, Json);
                    if (edited is not null) SetEditedFields(item, edited);
                }
                catch
                {
                    Set(item, FieldEditBody, decision.EditedActionJson);
                }
            }
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, GithubReconciliationInput input, GithubActionPlanDocument plan, GithubActionPlanGateReport gate)
    {
        if (resolverKey.StartsWith("action:", StringComparison.Ordinal))
        {
            var id = resolverKey["action:".Length..];
            var action = plan.Actions.FirstOrDefault(a => string.Equals(a.ActionId, id, StringComparison.Ordinal));
            return action is null ? $"(Action {id} nicht gefunden)" : JsonSerializer.Serialize(action, Json);
        }
        if (resolverKey.StartsWith("issueplan:", StringComparison.Ordinal))
        {
            var id = resolverKey["issueplan:".Length..];
            var item = input.AcceptedIssuePlan.Items.FirstOrDefault(i => string.Equals(i.IssuePlanId, id, StringComparison.Ordinal));
            return item is null ? $"(IssuePlanItem {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("gate:", StringComparison.Ordinal))
        {
            var id = resolverKey["gate:".Length..];
            var issues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.ActionId, id, StringComparison.Ordinal)).ToArray();
            return JsonSerializer.Serialize(issues, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["accept", "edit", "reject", "revise"], true,
            Help: "accept = Action freigeben · edit = Felder aendern · reject = entfernen · revise = mit Feedback zurueck an Reconciliation"),
        new(FieldEditOperation, "Edit: Operation", ReviewInputType.Dropdown, ["", "CREATE", "UPDATE", "LINK", "REOPEN", "NO_CHANGE", "NEEDS_REVIEW"], false,
            Help: "Leer lassen = Original-Operation behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditTargetIssueNumber, "Edit: Target Issue Number", ReviewInputType.FreeText, [], false,
            Help: "Nur fuer UPDATE/LINK/REOPEN. Leer lassen = Originalwert behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], false, Help: "Leer lassen = Original-Titel behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditBody, "Edit: Body", ReviewInputType.MultiLine, [], false, Help: "Leer lassen = Original-Body behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditAcceptanceCriteria, "Edit: Acceptance Criteria", ReviewInputType.MultiLine, [], false,
            Help: "Eine Zeile pro Kriterium. Leer lassen = Original-Kriterien behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditLabels, "Edit: Labels", ReviewInputType.FreeText, [], false,
            Help: "Kommagetrennt. Leer lassen = Original-Labels behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldEditRequiresHumanReview, "Edit: Requires Human Review", ReviewInputType.Dropdown, ["", "true", "false"], false,
            Help: "Leer lassen = Original-Wert behalten.", VisibleWhen: OnlyOnEdit),
        new(FieldReason, "Begruendung / Feedback", ReviewInputType.MultiLine, [], false,
            Help: "Warum akzeptiert/geaendert/verworfen/revidiert wird. Bei revise ist dies die Anweisung an Reconciliation.")
    ];

    private static ReviewHelp BuildHelp() => new(
        "GitHub Reconciliation Review",
        "Hier pruefst du den plan-only GitHubActionPlan. Dieser Schritt schreibt noch nichts nach GitHub.",
        [
            new("Ziel", "Der Reconciliation-Agent hat AcceptedIssuePlanItems gegen vorhandene Issues/Mappings eingeordnet. Du autorisierst nur den Plan, nicht den GitHub-Write."),
            new("Operationen", "CREATE erzeugt spaeter ein neues Issue. UPDATE/LINK/REOPEN beziehen sich auf targetIssueNumber. NO_CHANGE dokumentiert bewusst keine Aktion. NEEDS_REVIEW bleibt klaerungsbeduerftig."),
            new("Edit", "Edit-Felder erscheinen nur bei Entscheidung = edit. Leere Felder behalten den Originalwert."),
            new("Auswirkung", "Nach Fertig entsteht human-decisions.json. Ein spaeterer Apply erzeugt accepted-github-action-plan.json.")
        ]);

    private static ReviewItem BuildItem(GithubActionPlanItem action, GithubReconciliationInput input, GithubActionPlanGateReport gate)
    {
        var gateIssues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.ActionId, action.ActionId, StringComparison.Ordinal)).ToArray();
        var source = input.AcceptedIssuePlan.Items.FirstOrDefault(i => string.Equals(i.IssuePlanId, action.IssuePlanId, StringComparison.Ordinal));
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Operation", action.Operation),
            new(action.RequiresHumanReview ? ReviewNoteKind.Warning : ReviewNoteKind.Info, "Human Review",
                action.RequiresHumanReview ? "Diese Action wurde als reviewpflichtig markiert." : "Nicht explizit reviewpflichtig, aber editierbar."),
            new(ReviewNoteKind.Reason, "Reason", action.Reason),
            new(ReviewNoteKind.Info, "IssuePlanItem", source is null ? "(nicht gefunden)" : $"{source.IssuePlanId}: {source.Title}\n{Truncate(source.Description, 420)}"),
            new(ReviewNoteKind.Info, "Acceptance Criteria", action.AcceptanceCriteria.Count == 0 ? "(keine)" : string.Join("\n", action.AcceptanceCriteria.Select(c => "- " + c)))
        };
        if (action.TargetIssueNumber is not null)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Target Issue", "#" + action.TargetIssueNumber.Value));
        foreach (var issue in gateIssues)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, issue.Code, issue.Message));
        if (action.Metadata.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Metadata", JsonSerializer.Serialize(action.Metadata, Json)));

        var item = new ReviewItem
        {
            ItemId = action.ActionId,
            Summary = $"{action.Title}\n\n{Truncate(action.Body, 700)}",
            Badge = action.Operation,
            Notes = notes,
            ContextBlocks =
            [
                new(ContextBlockKind.Generic, "Action JSON", $"action:{action.ActionId}"),
                new(ContextBlockKind.Generic, "Gate Issues", $"gate:{action.ActionId}"),
                new(ContextBlockKind.Reference, $"IssuePlan {action.IssuePlanId}", $"issueplan:{action.IssuePlanId}")
            ],
            FieldValues =
            [
                new(FieldDecision, "accept"),
                new(FieldEditOperation, ""),
                new(FieldEditTargetIssueNumber, ""),
                new(FieldEditTitle, ""),
                new(FieldEditBody, ""),
                new(FieldEditAcceptanceCriteria, ""),
                new(FieldEditLabels, ""),
                new(FieldEditRequiresHumanReview, ""),
                new(FieldReason, SuggestedReason(action, gateIssues))
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static bool Include(GithubActionPlanItem action, HashSet<string> warningIds, string scope)
        => scope switch
        {
            "needs-human" => action.RequiresHumanReview || action.Operation.Equals("NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase) || warningIds.Contains(action.ActionId),
            _ => true
        };

    private static string SuggestedReason(GithubActionPlanItem action, IReadOnlyList<GithubActionPlanGateIssue> gateIssues)
    {
        if (gateIssues.Count > 0) return "auto: Gate-Hinweis vorhanden; bitte fachlich pruefen.";
        if (action.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase)) return "auto: bewusst keine GitHub-Aktion.";
        if (action.Operation.Equals("NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase)) return "auto: vor GitHub-Aktion klaerungsbeduerftig.";
        return "auto: GitHubActionPlanItem aus Reconciliation; bitte fachlich pruefen.";
    }

    private static bool TryBuildEditedAction(ReviewItem item, GithubActionPlanItem? original, out GithubActionPlanItem? edited)
    {
        edited = null;
        var operation = OverrideOrOriginal(FieldOf(item, FieldEditOperation), original?.Operation);
        if (string.IsNullOrWhiteSpace(operation) || !Operations.Contains(operation)) return false;
        var targetRaw = OverrideOrOriginal(FieldOf(item, FieldEditTargetIssueNumber), original?.TargetIssueNumber?.ToString() ?? "");
        int? target = null;
        if (!string.IsNullOrWhiteSpace(targetRaw))
        {
            if (!int.TryParse(targetRaw, out var parsed)) return false;
            target = parsed;
        }
        var reviewRaw = OverrideOrOriginal(FieldOf(item, FieldEditRequiresHumanReview), original?.RequiresHumanReview.ToString().ToLowerInvariant());
        if (!bool.TryParse(reviewRaw, out var requiresHumanReview)) return false;
        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original?.Title);
        var body = OverrideOrOriginal(FieldOf(item, FieldEditBody), original?.Body);
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body)) return false;

        edited = new GithubActionPlanItem(
            ActionId: item.ItemId,
            Operation: operation,
            IssuePlanId: original?.IssuePlanId ?? "",
            SourceRequirementIds: original?.SourceRequirementIds ?? [],
            TargetIssueNumber: target,
            Title: title,
            Body: body,
            AcceptanceCriteria: OverrideList(FieldOf(item, FieldEditAcceptanceCriteria), original?.AcceptanceCriteria ?? [], splitLines: true),
            Labels: OverrideList(FieldOf(item, FieldEditLabels), original?.Labels ?? [], splitLines: false),
            Reason: original?.Reason ?? "",
            RequiresHumanReview: requiresHumanReview,
            Metadata: original?.Metadata ?? new Dictionary<string, object?>(StringComparer.Ordinal));
        return HasValidEditOverrides(item);
    }

    private static bool HasValidEditOverrides(ReviewItem item)
    {
        var operation = FieldOf(item, FieldEditOperation);
        var target = FieldOf(item, FieldEditTargetIssueNumber);
        var review = FieldOf(item, FieldEditRequiresHumanReview);
        if (!string.IsNullOrWhiteSpace(operation) && !Operations.Contains(operation)) return false;
        if (!string.IsNullOrWhiteSpace(target) && !int.TryParse(target, out _)) return false;
        if (!string.IsNullOrWhiteSpace(review) && !bool.TryParse(review, out _)) return false;
        return !string.IsNullOrWhiteSpace(operation)
               || !string.IsNullOrWhiteSpace(target)
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditTitle))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditBody))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditAcceptanceCriteria))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditLabels))
               || !string.IsNullOrWhiteSpace(review);
    }

    private static void SetEditedFields(ReviewItem item, GithubActionPlanItem edited)
    {
        Set(item, FieldEditOperation, edited.Operation);
        Set(item, FieldEditTargetIssueNumber, edited.TargetIssueNumber?.ToString() ?? "");
        Set(item, FieldEditTitle, edited.Title);
        Set(item, FieldEditBody, edited.Body);
        Set(item, FieldEditAcceptanceCriteria, string.Join('\n', edited.AcceptanceCriteria));
        Set(item, FieldEditLabels, string.Join(", ", edited.Labels));
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

    private static string FieldOf(ReviewItem item, string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";
    private static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }
    private static string OverrideOrOriginal(string? overrideValue, string? originalValue)
        => string.IsNullOrWhiteSpace(overrideValue) ? originalValue ?? "" : overrideValue.Trim();
    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
