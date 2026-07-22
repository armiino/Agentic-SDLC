using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record IssuePlanningHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<IssuePlanningHumanDecision> Decisions);

public sealed record IssuePlanningHumanDecision(
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedIssuePlanItemJson")] string? EditedIssuePlanItemJson,
    [property: JsonPropertyName("reason")] string? Reason);

public static class IssuePlanningReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditOperation = "editOperation";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditDescription = "editDescription";
    public const string FieldEditAcceptanceCriteria = "editAcceptanceCriteria";
    public const string FieldEditLabels = "editLabels";
    public const string FieldEditDependencies = "editDependencies";
    public const string FieldEditRequiresHumanReview = "editRequiresHumanReview";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
        { "accept", "edit", "reject", "revise" };
    private static readonly HashSet<string> Operations = new(StringComparer.OrdinalIgnoreCase)
        { "CREATE", "LINK", "NO_CHANGE", "NEEDS_REVIEW" };

    public static ReviewSession BuildSession(
        string runId,
        IssuePlanningInput input,
        IssuePlanDocument plan,
        IssuePlanGateReport gate,
        string scope)
    {
        var warningIds = gate.Warnings
            .Where(w => !string.IsNullOrWhiteSpace(w.IssuePlanId))
            .Select(w => w.IssuePlanId!)
            .ToHashSet(StringComparer.Ordinal);
        var items = plan.Items
            .Where(item => Include(item, warningIds, scope))
            .Select(item => BuildItem(item, input, gate))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"l4-issuplanning-{runId}",
            Title = "L4 IssuePlanning — Human Review",
            Subtitle = scope switch
            {
                "needs-human" => $"{items.Count} reviewpflichtige IssuePlanItems. Der GitHub-Agent bekommt spaeter nur akzeptierte/gepruefte Items.",
                _ => $"{items.Count} IssuePlanItems sichtbar. CREATE, NEEDS_REVIEW und NO_CHANGE bleiben editierbar."
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

    public static IssuePlanningHumanDecisionsFile Apply(string runId, ReviewSession session, IssuePlanDocument plan)
    {
        var originals = plan.Items.ToDictionary(i => i.IssuePlanId, StringComparer.Ordinal);
        var decisions = session.Items.Select(item =>
        {
            string? V(string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(item.ItemId, out var original);
            return new IssuePlanningHumanDecision(
                IssuePlanId: item.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedIssuePlanItemJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedIssuePlanItem(item, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                        : null,
                Reason: V(FieldReason));
        }).ToList();
        return new IssuePlanningHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, IssuePlanningHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byItem = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.IssuePlanId))
            .GroupBy(d => d.IssuePlanId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byItem.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedIssuePlanItemJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<IssuePlanItem>(decision.EditedIssuePlanItemJson, Json);
                    if (edited is not null) SetEditedFields(item, edited);
                }
                catch
                {
                    Set(item, FieldEditDescription, decision.EditedIssuePlanItemJson);
                }
            }
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, IssuePlanningInput input, IssuePlanDocument plan, IssuePlanGateReport gate)
    {
        if (resolverKey.StartsWith("plan:", StringComparison.Ordinal))
        {
            var id = resolverKey["plan:".Length..];
            var item = plan.Items.FirstOrDefault(i => string.Equals(i.IssuePlanId, id, StringComparison.Ordinal));
            return item is null ? $"(IssuePlanItem {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var item = input.Items.FirstOrDefault(i => string.Equals(i.RequirementId, id, StringComparison.Ordinal));
            return item is null ? $"(Requirement {id} nicht gefunden)" : JsonSerializer.Serialize(item, Json);
        }
        if (resolverKey.StartsWith("gate:", StringComparison.Ordinal))
        {
            var id = resolverKey["gate:".Length..];
            var issues = gate.Errors.Concat(gate.Warnings)
                .Where(i => string.Equals(i.IssuePlanId, id, StringComparison.Ordinal)
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
            Help: "accept = IssuePlanItem freigeben · edit = einzelne Felder aendern · reject = aus Plan entfernen · revise = mit Feedback zurueck an Planner"),
        new ReviewFieldSpec(FieldEditOperation, "Edit: Operation", ReviewInputType.Dropdown,
            ["", "CREATE", "LINK", "NO_CHANGE", "NEEDS_REVIEW"], Required: false,
            Help: "Leer lassen = Original-Operation behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Titel behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditDescription, "Edit: Beschreibung", ReviewInputType.MultiLine, [], Required: false,
            Help: "Leer lassen = Original-Beschreibung behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditAcceptanceCriteria, "Edit: Acceptance Criteria", ReviewInputType.MultiLine, [], Required: false,
            Help: "Eine Zeile pro Kriterium. Leer lassen = Original-Kriterien behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditLabels, "Edit: Labels", ReviewInputType.FreeText, [], Required: false,
            Help: "Kommagetrennt. Leer lassen = Original-Labels behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditDependencies, "Edit: Dependencies", ReviewInputType.FreeText, [], Required: false,
            Help: "Kommagetrennt. Leer lassen = Original-Dependencies behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditRequiresHumanReview, "Edit: Requires Human Review", ReviewInputType.Dropdown,
            ["", "true", "false"], Required: false,
            Help: "Leer lassen = Original-Wert behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldReason, "Begruendung / Feedback", ReviewInputType.MultiLine, [], Required: false,
            Help: "Warum akzeptiert/geaendert/verworfen/revidiert wird. Bei revise ist dies die Anweisung an den Planner.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "L4 IssuePlanning Review",
        Summary: "Hier pruefst du den plan-only Issue-Schnitt, bevor ein spaeterer GitHub-Agent daraus operative Aktionen ableiten darf.",
        Sections:
        [
            new ReviewHelpSection(
                "Ziel dieses Schritts",
                """
                Der L4IssuePlanningAgent hat aus Readiness-freigegebenen Requirements einen IssuePlan erzeugt.

                Dieser Plan erzeugt noch keine GitHub-Issues. Er beschreibt nur, welche Arbeitseinheiten spaeter erstellt,
                geprueft, verlinkt oder bewusst nicht umgesetzt werden sollen.
                """),
            new ReviewHelpSection(
                "Entscheidung",
                """
                accept:
                Das Planitem ist fachlich sinnvoll und darf spaeter in den akzeptierten IssuePlan.

                edit:
                Der Schnitt ist grundsaetzlich richtig, aber Titel, Beschreibung, Operation, Kriterien, Labels oder Dependencies
                sollen angepasst werden. Edit-Felder erscheinen erst bei dieser Auswahl.

                reject:
                Das Planitem soll nicht weitergegeben werden.

                revise:
                Der Planner soll den Punkt spaeter ueberarbeiten. Schreibe das notwendige Feedback in Begruendung / Feedback.
                """),
            new ReviewHelpSection(
                "Operationen",
                """
                CREATE:
                Spaeter soll daraus wahrscheinlich ein neues GitHub-Issue entstehen.

                NEEDS_REVIEW:
                Der Punkt ist wichtig, aber fuer direkte Issue-Erstellung noch zu unklar.

                NO_CHANGE:
                Das Requirement ist bewusst abgedeckt, erzeugt aber keine operative Arbeitseinheit.

                LINK:
                Spaeter soll mit einem bestehenden Issue verbunden werden. Dieser Modus wird erst mit GitHub-Reconciliation
                wirklich stark.
                """),
            new ReviewHelpSection(
                "Edit-Felder",
                """
                Edit-Felder sind optional und nur sichtbar, wenn Entscheidung = edit gewaehlt ist.

                Leere Felder behalten den Originalwert.
                Acceptance Criteria werden zeilenweise gepflegt.
                Labels und Dependencies werden kommagetrennt gepflegt.
                """),
            new ReviewHelpSection(
                "Auswirkungen",
                """
                Nach Fertig schreibt die UI human-decisions.json in den IssuePlanning-Run.

                Der spaetere l4-issuplanning-apply erzeugt daraus einen akzeptierten IssuePlan.
                Erst ein spaeterer GitHub-Agent darf diesen akzeptierten Plan gegen bestehende Issues abgleichen oder Writes vorbereiten.
                """)
        ]);

    private static ReviewItem BuildItem(IssuePlanItem item, IssuePlanningInput input, IssuePlanGateReport gate)
    {
        var gateIssues = gate.Errors.Concat(gate.Warnings)
            .Where(i => string.Equals(i.IssuePlanId, item.IssuePlanId, StringComparison.Ordinal))
            .ToArray();
        var sourceRequirements = item.SourceRequirementIds
            .Select(id => input.Items.FirstOrDefault(i => string.Equals(i.RequirementId, id, StringComparison.Ordinal)))
            .Where(i => i is not null)
            .Select(i => $"{i!.RequirementId}: {i.Title}\n{Truncate(i.Text, 420)}")
            .ToArray();

        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Operation", item.Operation),
            new(item.RequiresHumanReview ? ReviewNoteKind.Warning : ReviewNoteKind.Info,
                "Human Review",
                item.RequiresHumanReview
                    ? "Dieses Planitem wurde vom Agenten als reviewpflichtig markiert."
                    : "Nicht explizit reviewpflichtig, aber im Kontrollreview editierbar."),
            new(ReviewNoteKind.Reason, "Rationale", item.Rationale ?? "(keine Rationale)"),
            new(ReviewNoteKind.Info, "Source Requirements", string.Join("\n\n", sourceRequirements)),
            new(ReviewNoteKind.Info, "Acceptance Criteria", item.AcceptanceCriteria.Count == 0 ? "(keine)" : string.Join("\n", item.AcceptanceCriteria.Select(c => "- " + c)))
        };
        foreach (var gateIssue in gateIssues)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, gateIssue.Code, gateIssue.Message));
        if (item.Metadata.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Metadata", JsonSerializer.Serialize(item.Metadata, Json)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "IssuePlanItem JSON", $"plan:{item.IssuePlanId}"),
            new(ContextBlockKind.Generic, "Gate Issues", $"gate:{item.IssuePlanId}")
        };
        foreach (var requirementId in item.SourceRequirementIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Requirement {requirementId}", $"requirement:{requirementId}"));

        var reviewItem = new ReviewItem
        {
            ItemId = item.IssuePlanId,
            Summary = $"{item.Title}\n\n{Truncate(item.Description, 700)}",
            Badge = item.Operation,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, SuggestedDecision(item, gateIssues)),
                new ReviewFieldValue(FieldEditOperation, ""),
                new ReviewFieldValue(FieldEditTitle, ""),
                new ReviewFieldValue(FieldEditDescription, ""),
                new ReviewFieldValue(FieldEditAcceptanceCriteria, ""),
                new ReviewFieldValue(FieldEditLabels, ""),
                new ReviewFieldValue(FieldEditDependencies, ""),
                new ReviewFieldValue(FieldEditRequiresHumanReview, ""),
                new ReviewFieldValue(FieldReason, SuggestedReason(item, gateIssues))
            ]
        };
        reviewItem.Resolved = Resolved(reviewItem);
        return reviewItem;
    }

    private static bool Include(IssuePlanItem item, HashSet<string> warningIds, string scope)
        => scope switch
        {
            "needs-human" => item.RequiresHumanReview
                             || item.Operation.Equals("NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase)
                             || warningIds.Contains(item.IssuePlanId),
            _ => true
        };

    private static string SuggestedDecision(IssuePlanItem item, IReadOnlyList<IssuePlanGateIssue> gateIssues)
        => "accept";

    private static string SuggestedReason(IssuePlanItem item, IReadOnlyList<IssuePlanGateIssue> gateIssues)
    {
        if (gateIssues.Count > 0) return "auto: Gate-Warnung vorhanden; bitte Split/Schnitt fachlich pruefen.";
        if (item.Operation.Equals("NEEDS_REVIEW", StringComparison.OrdinalIgnoreCase))
            return "auto: Agent markiert den Punkt als klaerungsbeduerftig; als Plan-Operation ist NEEDS_REVIEW zulaessig.";
        if (item.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase))
            return "auto: Requirement wird bewusst nicht als operative Arbeitseinheit geplant.";
        return "auto: IssuePlanItem aus L4IssuePlanningAgent; bitte fachlich pruefen.";
    }

    private static bool TryBuildEditedIssuePlanItem(ReviewItem item, IssuePlanItem? original, out IssuePlanItem? edited)
    {
        edited = null;
        var operation = OverrideOrOriginal(FieldOf(item, FieldEditOperation), original?.Operation);
        if (string.IsNullOrWhiteSpace(operation) || !Operations.Contains(operation)) return false;
        var requiresHumanReviewRaw = OverrideOrOriginal(FieldOf(item, FieldEditRequiresHumanReview), original?.RequiresHumanReview.ToString().ToLowerInvariant());
        if (!bool.TryParse(requiresHumanReviewRaw, out var requiresHumanReview)) return false;

        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original?.Title);
        var description = OverrideOrOriginal(FieldOf(item, FieldEditDescription), original?.Description);
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)) return false;

        edited = new IssuePlanItem(
            IssuePlanId: item.ItemId,
            Operation: operation,
            Title: title,
            Description: description,
            SourceRequirementIds: original?.SourceRequirementIds ?? [],
            AcceptanceCriteria: OverrideList(FieldOf(item, FieldEditAcceptanceCriteria), original?.AcceptanceCriteria ?? [], splitLines: true),
            Labels: OverrideList(FieldOf(item, FieldEditLabels), original?.Labels ?? [], splitLines: false),
            Dependencies: OverrideList(FieldOf(item, FieldEditDependencies), original?.Dependencies ?? [], splitLines: false),
            Rationale: original?.Rationale,
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
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditDescription))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditAcceptanceCriteria))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditLabels))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditDependencies))
               || !string.IsNullOrWhiteSpace(requiresHumanReview);
    }

    private static void SetEditedFields(ReviewItem item, IssuePlanItem edited)
    {
        Set(item, FieldEditOperation, edited.Operation);
        Set(item, FieldEditTitle, edited.Title);
        Set(item, FieldEditDescription, edited.Description);
        Set(item, FieldEditAcceptanceCriteria, string.Join('\n', edited.AcceptanceCriteria));
        Set(item, FieldEditLabels, string.Join(", ", edited.Labels));
        Set(item, FieldEditDependencies, string.Join(", ", edited.Dependencies));
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

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
