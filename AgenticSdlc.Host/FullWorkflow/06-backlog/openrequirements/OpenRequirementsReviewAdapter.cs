using AgenticSdlc.HumanReview;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class OpenRequirementsReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldClarificationType = "clarificationType";
    public const string FieldPriority = "priority";
    public const string FieldIssueTitle = "issueTitle";
    public const string FieldQuestion = "question";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
    {
        "create_clarification_issue", "defer", "out_of_scope", "promote_to_delivery_planning",
        "merge_with_existing", "keep_open_decision", "break_down_required"
    };
    private static readonly HashSet<string> ClarificationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "decision", "breakdown", "scope", "privacy", "security", "architecture", "ux", "accessibility", "data", "process"
    };
    private static readonly HashSet<string> Priorities = new(StringComparer.OrdinalIgnoreCase) { "high", "medium", "low" };

    public static ReviewSession BuildSession(
        string runId,
        OperationalizationAuditDocument audit,
        RequirementsReadinessReport? readiness,
        string scope)
    {
        var readinessById = readiness?.Items.ToDictionary(i => i.RequirementId, StringComparer.Ordinal)
                            ?? new Dictionary<string, RequirementReadinessItem>(StringComparer.Ordinal);
        var items = audit.Requirements
            .Where(r => Include(r, scope))
            .Select(r =>
            {
                readinessById.TryGetValue(r.RequirementId, out var readinessItem);
                return BuildItem(r, readinessItem);
            })
            .ToArray();

        return new ReviewSession
        {
            SessionId = $"open-requirements-{runId}",
            Title = "Open Requirements — Human Review",
            Subtitle = $"{items.Length} Requirements sind nicht im Delivery-IssuePlanning gelandet. Entscheide, wie sie als Klaerungsarbeit weiterlaufen.",
            Help = BuildHelp(),
            FieldSchema = Schema(),
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        if (!ClarificationTypes.Contains(FieldOf(item, FieldClarificationType))) return false;
        if (!Priorities.Contains(FieldOf(item, FieldPriority))) return false;
        if (string.IsNullOrWhiteSpace(FieldOf(item, FieldIssueTitle))) return false;
        if (string.IsNullOrWhiteSpace(FieldOf(item, FieldQuestion))) return false;
        return !string.IsNullOrWhiteSpace(FieldOf(item, FieldReason));
    }

    public static OpenRequirementHumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(item => new OpenRequirementHumanDecision(
            RequirementId: item.ItemId,
            Decision: FieldOf(item, FieldDecision),
            ClarificationType: FieldOf(item, FieldClarificationType),
            Priority: FieldOf(item, FieldPriority),
            IssueTitle: FieldOf(item, FieldIssueTitle),
            Question: FieldOf(item, FieldQuestion),
            Reason: FieldOf(item, FieldReason))).ToArray();
        return new OpenRequirementHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, OpenRequirementHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byRequirement = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.RequirementId))
            .GroupBy(d => d.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byRequirement.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldClarificationType, decision.ClarificationType);
            Set(item, FieldPriority, decision.Priority);
            Set(item, FieldIssueTitle, decision.IssueTitle);
            Set(item, FieldQuestion, decision.Question);
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, OperationalizationAuditDocument audit, RequirementsReadinessReport? readiness)
    {
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var item = audit.Requirements.FirstOrDefault(r => r.RequirementId.Equals(id, StringComparison.Ordinal));
            return item is null ? $"(Requirement {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("readiness:", StringComparison.Ordinal))
        {
            var id = resolverKey["readiness:".Length..];
            var item = readiness?.Items.FirstOrDefault(r => r.RequirementId.Equals(id, StringComparison.Ordinal));
            return item is null ? $"(Readiness {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["create_clarification_issue", "defer", "out_of_scope", "promote_to_delivery_planning", "merge_with_existing", "keep_open_decision", "break_down_required"], true,
            Help: "create_clarification_issue = Klaerungs-Issue planen · defer = spaeter · out_of_scope = aus Scope · promote = in Delivery-IssuePlanning geben · merge = mit anderem Punkt verbinden · keep_open_decision = nur im State offen halten · break_down_required = zuerst zerlegen"),
        new(FieldClarificationType, "Klaerungstyp", ReviewInputType.Dropdown,
            ["decision", "breakdown", "scope", "privacy", "security", "architecture", "ux", "accessibility", "data", "process"], true,
            Help: "Fachliche Kategorie fuer das spaetere Klaerungs-Issue."),
        new(FieldPriority, "Prioritaet", ReviewInputType.Dropdown, ["high", "medium", "low"], true,
            Help: "Prioritaet der Klaerungsarbeit."),
        new(FieldIssueTitle, "Klaerungs-Issue Titel", ReviewInputType.FreeText, [], true,
            Help: "Titel fuer ein spaeteres Klaerungs-/Discovery-Issue."),
        new(FieldQuestion, "Klaerungsfrage", ReviewInputType.FreeText, [], true,
            Help: "Welche konkrete Frage muss beantwortet werden, damit das Requirement weiter operationalisiert werden kann?"),
        new(FieldReason, "Begruendung / Review-Notiz", ReviewInputType.FreeText, [], true,
            Help: "Warum diese Entscheidung getroffen wurde.")
    ];

    private static ReviewHelp BuildHelp() => new(
        "Open Requirements Review",
        "Hier entscheidest du, wie Requirements weiterlaufen, die nicht direkt in Delivery-IssuePlanning freigegeben wurden.",
        [
            new("Ziel", "Offene Requirements werden nicht ignoriert. Sie werden als Klaerungsarbeit, deferred/out-of-scope oder als erneut zu planende Delivery-Arbeit klassifiziert."),
            new("Klaerungs-Issue", "Default fuer needs_decision und needs_breakdown. Daraus entsteht spaeter ein Issue, das eine Entscheidung oder Zerlegung herbeifuehrt, nicht sofort Implementierung."),
            new("Promote", "Nur verwenden, wenn du sicher bist, dass das Requirement bereits umsetzbar ist und in Delivery-IssuePlanning gehoert."),
            new("Apply", "Nach Fertig schreibt Apply ein accepted-open-requirement-decisions.json und ein clarification-planning-input.json.")
        ]);

    private static ReviewItem BuildItem(OperationalizationRequirementAudit item, RequirementReadinessItem? readiness)
    {
        var suggestion = Suggest(item, readiness);
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Readiness", item.Readiness ?? "(unbekannt)"),
            new(ReviewNoteKind.Suggestion, "Vorschlag", suggestion.Decision),
            new(ReviewNoteKind.Reason, "Grund", string.Join("\n", readiness?.Reasons.Select(r => $"- {r.Code}: {r.Message}") ?? []))
        };
        foreach (var finding in item.FindingCodes)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Finding", finding));

        var reviewItem = new ReviewItem
        {
            ItemId = item.RequirementId,
            Summary = $"{item.Title}\n\nStatus: {item.Status ?? "-"}\nReadiness: {item.Readiness ?? "-"}",
            Badge = item.Readiness ?? "open",
            Notes = notes,
            ContextBlocks =
            [
                new(ContextBlockKind.Generic, "Operationalization Audit Item", $"requirement:{item.RequirementId}"),
                new(ContextBlockKind.Generic, "Readiness Details", $"readiness:{item.RequirementId}")
            ],
            FieldValues =
            [
                new(FieldDecision, suggestion.Decision),
                new(FieldClarificationType, suggestion.ClarificationType),
                new(FieldPriority, suggestion.Priority),
                new(FieldIssueTitle, suggestion.IssueTitle),
                new(FieldQuestion, suggestion.Question),
                new(FieldReason, suggestion.Reason)
            ]
        };
        reviewItem.Resolved = Resolved(reviewItem);
        return reviewItem;
    }

    private static OpenRequirementSuggestion Suggest(OperationalizationRequirementAudit item, RequirementReadinessItem? readiness)
    {
        var type = SuggestType(item, readiness);
        var decision = item.Readiness switch
        {
            "deferred_or_optional" => "defer",
            "needs_breakdown" => "create_clarification_issue",
            "needs_decision" => "create_clarification_issue",
            _ => "keep_open_decision"
        };
        if (decision == "create_clarification_issue" && type == "breakdown")
            decision = "break_down_required";
        var title = decision is "defer"
            ? $"Zurueckstellen: {item.Title}"
            : $"Klaerung: {item.Title}";
        var question = item.Readiness switch
        {
            "needs_breakdown" => $"Wie muss '{item.Title}' in konkrete, testbare Requirements oder Arbeitspakete zerlegt werden?",
            "deferred_or_optional" => $"Soll '{item.Title}' im aktuellen Projektumfang bleiben, spaeter geplant oder verworfen werden?",
            _ => $"Welche fachliche Entscheidung ist noetig, damit '{item.Title}' operationalisiert werden kann?"
        };
        return new OpenRequirementSuggestion(decision, type, item.Readiness is "needs_decision" ? "high" : "medium", title, question,
            $"auto: aus Readiness={item.Readiness ?? "unknown"} fuer Open-Requirements-Review vorgeschlagen.");
    }

    private static string SuggestType(OperationalizationRequirementAudit item, RequirementReadinessItem? readiness)
    {
        var text = $"{item.Title} {string.Join(' ', readiness?.Reasons.Select(r => r.Message) ?? [])}".ToLowerInvariant();
        if (item.Readiness == "needs_breakdown") return "breakdown";
        if (text.Contains("datenschutz") || text.Contains("rechtsgrundlage") || text.Contains("bewohnerdaten")) return "privacy";
        if (text.Contains("sicherheit")) return "security";
        if (text.Contains("getx") || text.Contains("firestore") || text.Contains("android studio")) return "architecture";
        if (text.Contains("barriere")) return "accessibility";
        if (text.Contains("scope")) return "scope";
        return "decision";
    }

    private static bool Include(OperationalizationRequirementAudit item, string scope)
        => scope switch
        {
            "all" => true,
            "needs-breakdown" => string.Equals(item.Readiness, "needs_breakdown", StringComparison.OrdinalIgnoreCase),
            "deferred" => string.Equals(item.Readiness, "deferred_or_optional", StringComparison.OrdinalIgnoreCase),
            _ => !item.InIssuePlanningInput
        };

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private sealed record OpenRequirementSuggestion(string Decision, string ClarificationType, string Priority, string IssueTitle, string Question, string Reason);
}
