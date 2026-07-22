using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record L4CompletionHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<L4CompletionHumanDecision> Decisions);

public sealed record L4CompletionHumanDecision(
    [property: JsonPropertyName("proposalItemId")] string ProposalItemId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedProposalJson")] string? EditedProposalJson,
    [property: JsonPropertyName("reason")] string? Reason);

public static class L4CompletionReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditOperation = "editOperation";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditProblem = "editProblem";
    public const string FieldEditSuggestedResolution = "editSuggestedResolution";
    public const string FieldEditWhyItMatters = "editWhyItMatters";
    public const string FieldEditEvidenceState = "editEvidenceState";
    public const string FieldEditRequiresHumanDecision = "editRequiresHumanDecision";
    public const string FieldReason = "reason";

    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
        { "accept", "edit", "reject", "revise" };
    private static readonly HashSet<string> Operations = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADD_OPEN_DECISION", "ADD_DISK_POINT", "MARK_NEEDS_BREAKDOWN", "REVISE_REQUIREMENT",
        "SPLIT_REQUIREMENT", "LINK_RELATED_ITEMS", "DEFER", "NO_CHANGE"
    };
    private static readonly HashSet<string> EvidenceStates = new(StringComparer.OrdinalIgnoreCase)
        { "stated", "derived", "weakly_inferred", "not_stated" };

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = Proposal autorisieren · edit = bearbeitete Fassung uebernehmen · reject = verwerfen · revise = mit Feedback zurueck an L4 Completion"),
        new ReviewFieldSpec(FieldEditOperation, "Edit: Operation", ReviewInputType.Dropdown,
            ["", "ADD_OPEN_DECISION", "ADD_DISK_POINT", "MARK_NEEDS_BREAKDOWN", "REVISE_REQUIREMENT", "SPLIT_REQUIREMENT", "LINK_RELATED_ITEMS", "DEFER", "NO_CHANGE"], Required: false,
            Help: "Leer lassen = Original-Operation behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Titel behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditProblem, "Edit: Problem", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Problem behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditSuggestedResolution, "Edit: vorgeschlagene Loesung / Klaerung", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Vorschlag behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditWhyItMatters, "Edit: Warum wichtig", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer lassen = Original-Begruendung behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditEvidenceState, "Edit: Evidence State", ReviewInputType.Dropdown,
            ["", "stated", "derived", "weakly_inferred", "not_stated"], Required: false,
            Help: "Leer lassen = Original-Evidence-State behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditRequiresHumanDecision, "Edit: Requires Human Decision", ReviewInputType.Dropdown,
            ["", "true", "false"], Required: false,
            Help: "Leer lassen = Original-Wert behalten.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldReason, "Begruendung / Feedback", ReviewInputType.FreeText, [], Required: false,
            Help: "Fuer jede Entscheidung erforderlich. Bei revise ist dies die Anweisung an den Agenten.")
    ];

    public static ReviewSession BuildSession(
        string runId,
        L4AdequacyReport adequacy,
        L4CompletionProposalDocument proposals,
        CanonicalRequirementsBaseline baseline,
        string scope)
    {
        var items = proposals.Items
            .Where(p => Include(p, scope))
            .Select(p => BuildItem(p, adequacy, baseline))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"l4-completion-{runId}",
            Title = "L4 Completion — Human Review",
            Subtitle = scope switch
            {
                "all" => $"{items.Count} Completion-Proposals sichtbar. Alle sind editierbar.",
                _ => $"{items.Count} entscheidungsrelevante Completion-Proposals. Projektwahrheit entsteht erst nach Review + Apply."
            },
            Help = BuildHelp(),
            FieldSchema = Schema(),
            Items = items
        };
    }

    private static ReviewHelp BuildHelp() => new(
        Title: "L4 Completion Review",
        Summary: "Hier entscheidest du, welche vom L4-Agenten erkannten Luecken, offenen Entscheidungen und DISK-Punkte in den weiteren Requirements-Prozess duerfen.",
        Sections:
        [
            new ReviewHelpSection(
                "Ziel dieses Schritts",
                """
                Der L4 Completion Agent hat den konsolidierten Requirements-Stand kapitelweise bewertet und daraus Proposals erzeugt.

                Diese Proposals sind noch keine Projektwahrheit. Du autorisierst, bearbeitest, verwirfst oder gibst sie zur Ueberarbeitung zurueck.

                Erst ein spaeterer Apply-Schritt uebernimmt akzeptierte oder editierte Proposals deterministisch in den naechsten L4-Stand.
                """),
            new ReviewHelpSection(
                "Entscheidung",
                """
                accept:
                Das Proposal ist fachlich sinnvoll und darf spaeter unveraendert angewandt werden. Edit-Felder musst du nicht ausfuellen.

                edit:
                Die Richtung stimmt, aber einzelne Felder sollen anders lauten. Erst dann erscheinen die Edit-Felder. Leere Edit-Felder behalten den Originalwert.

                reject:
                Das Proposal ist falsch, irrelevant oder soll nicht weiterverfolgt werden.

                revise:
                Der Agent soll den Punkt spaeter mit deinem Feedback ueberarbeiten. Schreibe die Anweisung in Begruendung / Feedback.
                """),
            new ReviewHelpSection(
                "Edit-Felder",
                """
                Edit-Felder sind nur sichtbar, wenn Entscheidung = edit gewaehlt ist.

                Du musst nicht alles ausfuellen. Aendere nur, was wirklich anders sein soll:
                - Operation: Art des Proposals, z. B. ADD_OPEN_DECISION oder ADD_DISK_POINT.
                - Titel: kurze Bezeichnung des Punktes.
                - Problem: welche Luecke oder Unklarheit besteht.
                - Vorgeschlagene Loesung / Klaerung: was als naechstes geklaert, entschieden oder zerlegt werden soll.
                - Warum wichtig: Auswirkung auf Projektstart, Planung, Umsetzung oder Abnahme.
                - Evidence State: wie stark der Punkt belegt ist.
                - Requires Human Decision: ob eine menschliche Entscheidung zwingend ist.
                """),
            new ReviewHelpSection(
                "Evidence State",
                """
                stated:
                Direkt belegt.

                derived:
                Aus vorhandenen Requirements/Entscheidungen fachlich abgeleitet.

                weakly_inferred:
                Schwach ableitbar, unsicher.

                not_stated:
                Wurde nicht gesagt. Das ist typisch fuer DISK-Punkte.
                """),
            new ReviewHelpSection(
                "DISK-Punkte",
                """
                DISK bedeutet Diskussionspunkt.

                Ein DISK-Punkt wurde nicht als Projektwahrheit gesagt, kann aber als Requirements-Engineering-Vorschlag wichtig sein.

                Beispiel: Verfuegbarkeit/Wiederanlauf wurde nicht genannt, kann aber fuer Projektstart und Abnahme relevant sein.

                DISK-Punkte duerfen nicht direkt in IssuePlanning oder GitHub laufen. Sie brauchen eine bewusste menschliche Entscheidung.
                """),
            new ReviewHelpSection(
                "Auswirkungen",
                """
                Nach Fertig schreibt die UI human-decisions.json.

                Diese Datei dokumentiert deine Entscheidungen und ist Input fuer den spaeteren l4-completion-apply.

                accept/edit kann spaeter zu neuen offenen Entscheidungen, Breakdown-Markierungen oder DISK-Eintraegen im L4-Stand fuehren.

                reject entfernt den Vorschlag aus dem weiteren Pfad.

                revise ist Feedback fuer einen spaeteren Agenten-Revisionslauf.
                """)
        ]);

    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        if (string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        if (!string.Equals(decision, "edit", StringComparison.OrdinalIgnoreCase)) return true;

        return HasValidEditOverrides(item);
    }

    public static L4CompletionHumanDecisionsFile Apply(
        string runId,
        ReviewSession session,
        L4CompletionProposalDocument proposals)
    {
        var originals = proposals.Items.ToDictionary(p => p.ProposalItemId, StringComparer.Ordinal);
        var decisions = session.Items.Select(it =>
        {
            string? V(string key) => it.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(it.ItemId, out var original);
            return new L4CompletionHumanDecision(
                ProposalItemId: it.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedProposalJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedProposal(it, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                    : null,
                Reason: V(FieldReason));
        }).ToList();
        return new L4CompletionHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, L4CompletionHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byProposal = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.ProposalItemId))
            .GroupBy(d => d.ProposalItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byProposal.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedProposalJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<L4CompletionProposalItem>(decision.EditedProposalJson, Json);
                    if (edited is not null) SetChangedProposalFields(item, edited);
                }
                catch
                {
                    Set(item, FieldEditSuggestedResolution, decision.EditedProposalJson);
                }
            }
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(
        string resolverKey,
        L4AdequacyReport adequacy,
        L4CompletionProposalDocument proposals,
        CanonicalRequirementsBaseline baseline)
    {
        if (resolverKey.StartsWith("proposal:", StringComparison.Ordinal))
        {
            var id = resolverKey["proposal:".Length..];
            var proposal = proposals.Items.FirstOrDefault(p => string.Equals(p.ProposalItemId, id, StringComparison.Ordinal));
            return proposal is null ? $"(Proposal {id} nicht gefunden)" : JsonSerializer.Serialize(proposal, Json);
        }
        if (resolverKey.StartsWith("finding:", StringComparison.Ordinal))
        {
            var id = resolverKey["finding:".Length..];
            var finding = adequacy.Findings.FirstOrDefault(f => string.Equals(f.FindingId, id, StringComparison.Ordinal));
            return finding is null ? $"(Finding {id} nicht gefunden)" : JsonSerializer.Serialize(finding, Json);
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var req = baseline.Requirements.FirstOrDefault(r => string.Equals(r.RequirementId, id, StringComparison.Ordinal));
            return req is null ? $"(Requirement {id} nicht gefunden)" : JsonSerializer.Serialize(req, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static ReviewItem BuildItem(
        L4CompletionProposalItem proposal,
        L4AdequacyReport adequacy,
        CanonicalRequirementsBaseline baseline)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Operation", proposal.Operation),
            new(proposal.RequiresHumanDecision ? ReviewNoteKind.Warning : ReviewNoteKind.Info,
                "Human Review",
                proposal.RequiresHumanDecision
                    ? "Dieses Proposal braucht menschliche Autorisierung, bevor es Projektzustand werden darf."
                    : "Keine Human-Decision-Pflicht laut Agent; bleibt im Kontrollreview editierbar."),
            new(ReviewNoteKind.Info, "Evidence State", proposal.EvidenceState),
            new(ReviewNoteKind.Warning, "Problem", proposal.Problem),
            new(ReviewNoteKind.Suggestion, "Suggested Resolution", proposal.SuggestedResolution),
            new(ReviewNoteKind.Reason, "Why it matters", proposal.WhyItMatters)
        };

        if (proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add(new ReviewNote(
                ReviewNoteKind.Warning,
                "DISK",
                "Diskussionspunkt: wurde nicht als Projektwahrheit gesagt. Nur bei Human Acceptance darf daraus ein weiterverarbeitbares Item werden."));
        }

        var findingSummary = proposal.SourceFindingIds
            .Select(id => adequacy.Findings.FirstOrDefault(f => string.Equals(f.FindingId, id, StringComparison.Ordinal)))
            .Where(f => f is not null)
            .Select(f => $"{f!.FindingId} [{f.Adequacy}] {f.Section}: {f.Summary}")
            .ToArray();
        if (findingSummary.Length > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Adequacy Findings", string.Join("\n\n", findingSummary)));

        var reqSummary = proposal.SourceRequirementIds
            .Select(id => baseline.Requirements.FirstOrDefault(r => string.Equals(r.RequirementId, id, StringComparison.Ordinal)))
            .Where(r => r is not null)
            .Select(r => $"{r!.RequirementId}: {r.Title}\n{Truncate(r.Text, 420)}")
            .ToArray();
        if (reqSummary.Length > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Source Requirements", string.Join("\n\n", reqSummary)));

        if (proposal.Metadata.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Metadata", JsonSerializer.Serialize(proposal.Metadata, Json)));

        var context = new List<ContextBlock>
        {
            new(ContextBlockKind.Generic, "Proposal JSON", $"proposal:{proposal.ProposalItemId}")
        };
        foreach (var findingId in proposal.SourceFindingIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Finding {findingId}", $"finding:{findingId}"));
        foreach (var requirementId in proposal.SourceRequirementIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Requirement {requirementId}", $"requirement:{requirementId}"));

        var item = new ReviewItem
        {
            ItemId = proposal.ProposalItemId,
            Summary = $"{proposal.Title}\n\n{proposal.Problem}",
            Badge = proposal.Operation,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, SuggestedDecision(proposal)),
                new ReviewFieldValue(FieldEditOperation, ""),
                new ReviewFieldValue(FieldEditTitle, ""),
                new ReviewFieldValue(FieldEditProblem, ""),
                new ReviewFieldValue(FieldEditSuggestedResolution, ""),
                new ReviewFieldValue(FieldEditWhyItMatters, ""),
                new ReviewFieldValue(FieldEditEvidenceState, ""),
                new ReviewFieldValue(FieldEditRequiresHumanDecision, ""),
                new ReviewFieldValue(FieldReason, SuggestedReason(proposal))
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static bool Include(L4CompletionProposalItem proposal, string scope)
        => scope switch
        {
            "all" => true,
            _ => proposal.RequiresHumanDecision
                 || proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase)
                 || proposal.Operation.Equals("ADD_OPEN_DECISION", StringComparison.OrdinalIgnoreCase)
                 || proposal.Operation.Equals("MARK_NEEDS_BREAKDOWN", StringComparison.OrdinalIgnoreCase)
        };

    private static string SuggestedDecision(L4CompletionProposalItem proposal)
        => proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase)
            ? "revise"
            : "accept";

    private static string SuggestedReason(L4CompletionProposalItem proposal)
        => proposal.Operation.Equals("ADD_DISK_POINT", StringComparison.OrdinalIgnoreCase)
            ? "auto: DISK ist Open-World und muss bewusst angenommen, editiert oder abgelehnt werden."
            : "auto: Completion-Proposal aus Adequacy-Feedback; bitte fachlich pruefen.";

    private static string FieldOf(ReviewItem item, string key) =>
        item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";

    private static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    private static void SetChangedProposalFields(ReviewItem item, L4CompletionProposalItem proposal)
    {
        Set(item, FieldEditOperation, proposal.Operation);
        Set(item, FieldEditTitle, proposal.Title);
        Set(item, FieldEditProblem, proposal.Problem);
        Set(item, FieldEditSuggestedResolution, proposal.SuggestedResolution);
        Set(item, FieldEditWhyItMatters, proposal.WhyItMatters);
        Set(item, FieldEditEvidenceState, proposal.EvidenceState);
        Set(item, FieldEditRequiresHumanDecision, proposal.RequiresHumanDecision ? "true" : "false");
    }

    private static bool TryBuildEditedProposal(
        ReviewItem item,
        L4CompletionProposalItem? original,
        out L4CompletionProposalItem? proposal)
    {
        proposal = null;
        var operation = OverrideOrOriginal(FieldOf(item, FieldEditOperation), original?.Operation);
        var evidenceState = OverrideOrOriginal(FieldOf(item, FieldEditEvidenceState), original?.EvidenceState);
        if (string.IsNullOrWhiteSpace(operation) || !Operations.Contains(operation)) return false;
        if (string.IsNullOrWhiteSpace(evidenceState) || !EvidenceStates.Contains(evidenceState)) return false;

        var humanDecisionRaw = OverrideOrOriginal(FieldOf(item, FieldEditRequiresHumanDecision), original?.RequiresHumanDecision.ToString().ToLowerInvariant());
        if (!bool.TryParse(humanDecisionRaw, out var humanDecision)) return false;

        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original?.Title);
        var problem = OverrideOrOriginal(FieldOf(item, FieldEditProblem), original?.Problem);
        var suggestedResolution = OverrideOrOriginal(FieldOf(item, FieldEditSuggestedResolution), original?.SuggestedResolution);
        var whyItMatters = OverrideOrOriginal(FieldOf(item, FieldEditWhyItMatters), original?.WhyItMatters);
        if (original is null
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldEditOperation))
            && string.IsNullOrWhiteSpace(title)
            && string.IsNullOrWhiteSpace(problem)
            && string.IsNullOrWhiteSpace(suggestedResolution)
            && string.IsNullOrWhiteSpace(whyItMatters)
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldEditEvidenceState))
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldEditRequiresHumanDecision)))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(title)
            || string.IsNullOrWhiteSpace(problem)
            || string.IsNullOrWhiteSpace(suggestedResolution)
            || string.IsNullOrWhiteSpace(whyItMatters))
            return false;

        proposal = new L4CompletionProposalItem(
            ProposalItemId: item.ItemId,
            Operation: operation,
            Category: original?.Category ?? "",
            Title: title,
            Problem: problem,
            SuggestedResolution: suggestedResolution,
            WhyItMatters: whyItMatters,
            EvidenceState: evidenceState,
            SourceRequirementIds: original?.SourceRequirementIds ?? [],
            SourceFindingIds: original?.SourceFindingIds ?? [],
            RequiresHumanDecision: humanDecision,
            Metadata: original?.Metadata ?? new Dictionary<string, string>(StringComparer.Ordinal));
        return true;
    }

    private static bool HasValidEditOverrides(ReviewItem item)
    {
        var operation = FieldOf(item, FieldEditOperation);
        var evidenceState = FieldOf(item, FieldEditEvidenceState);
        var requiresHumanDecision = FieldOf(item, FieldEditRequiresHumanDecision);
        if (!string.IsNullOrWhiteSpace(operation) && !Operations.Contains(operation)) return false;
        if (!string.IsNullOrWhiteSpace(evidenceState) && !EvidenceStates.Contains(evidenceState)) return false;
        if (!string.IsNullOrWhiteSpace(requiresHumanDecision) && !bool.TryParse(requiresHumanDecision, out _)) return false;

        return !string.IsNullOrWhiteSpace(operation)
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditTitle))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditProblem))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditSuggestedResolution))
               || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditWhyItMatters))
               || !string.IsNullOrWhiteSpace(evidenceState)
               || !string.IsNullOrWhiteSpace(requiresHumanDecision);
    }

    private static string OverrideOrOriginal(string? overrideValue, string? originalValue)
        => string.IsNullOrWhiteSpace(overrideValue) ? originalValue ?? "" : overrideValue.Trim();

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
