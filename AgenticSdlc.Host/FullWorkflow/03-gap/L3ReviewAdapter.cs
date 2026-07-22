using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// Projiziert das L3-Human-Review-Paket (die Kandidaten, die eine menschliche Entscheidung brauchen — alles außer
/// SUPPORTED_ANCHORED) in eine domänen-agnostische <see cref="ReviewSession"/> und wandelt die Entscheidungen zurück in
/// die <c>human-decisions.json</c>, die <c>l3-apply</c> (accept/edit/reject) und <c>l3-revise</c> (revise) bereits
/// konsumieren. Reuse des generischen <c>AgenticSdlc.HumanReview</c>-UI (Präzedenz: <c>DerivedRisksReviewAdapter</c>,
/// <c>LedgerAdjudicationReviewAdapter</c>). Der Mensch ist hier die tragende Instanz (open-world = un-beweisbar).
/// </summary>
public static class L3ReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEdited = "editedText";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
        { "accept", "edit", "reject", "revise" };

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = als Projekt-Item autorisieren · edit = bearbeitete Fassung übernehmen · reject = verwerfen · revise = mit Feedback zurück an den Agenten"),
        new ReviewFieldSpec(FieldEdited, "Bearbeiteter Text (nur bei edit)", ReviewInputType.FreeText, [], Required: false),
        new ReviewFieldSpec(FieldReason, "Begründung / Feedback", ReviewInputType.FreeText, [], Required: false,
            Help: "Für jede Entscheidung erforderlich. Bei revise ist dies die Anweisung an den Agenten."),
    ];

    /// <summary>
    /// Baut die Session aus den gerouteten Kandidaten. Default ist <paramref name="includeSupportedAnchored"/> false:
    /// nur echte Review-Faelle. Mit true wird L3 zur Kontrollschicht ueber alle Kandidaten; SupportedAnchored sind dann
    /// vorbefuellt und resolved, bleiben aber editierbar.
    /// </summary>
    public static ReviewSession BuildSession(string runId, IReadOnlyList<L3RoutedCandidate> routed, bool includeSupportedAnchored = false)
    {
        var items = routed
            .Where(r => includeSupportedAnchored || r.Class != L3Class.SupportedAnchored)
            .Select(BuildItem)
            .ToList();
        return new ReviewSession
        {
            SessionId = $"l3-{runId}",
            Title = includeSupportedAnchored
                ? "L3 Open-World-Ableitung — Kontrollreview"
                : "L3 Open-World-Ableitung — Human Review",
            Subtitle = includeSupportedAnchored
                ? $"{items.Count} Kandidaten sichtbar. SupportedAnchored sind vorab akzeptiert/resolved, aber editierbar."
                : $"{items.Count} Kandidaten zur Autorisierung. Open-World ist maschinell nicht beweisbar → der Mensch entscheidet.",
            FieldSchema = Schema(),
            Items = items
        };
    }

    private static ReviewItem BuildItem(L3RoutedCandidate r)
    {
        var notes = new List<ReviewNote>
        {
            new(r.Class switch
            {
                L3Class.Contradicted => ReviewNoteKind.Warning,
                L3Class.Unreferenced => ReviewNoteKind.Suggestion,
                _ => ReviewNoteKind.Info
            }, $"Klasse: {r.Class}", ClassNote(r))
        };
        foreach (var a in r.Anchors)
            notes.Add(new(a.Verdict is InferenceVerdictKind.Supported ? ReviewNoteKind.Info : ReviewNoteKind.Warning,
                $"Anker {a.ItemId} ({a.Relation})", $"{(a.Exists ? a.Verdict?.ToString() ?? "—" : "UNKNOWN (nicht in der Umwelt)")}: {a.Rationale}"));
        if (r.Candidate.Rationale is { Length: > 0 } rat) notes.Add(new(ReviewNoteKind.Reason, "Begründung (Agent)", rat));
        if (r.Candidate.ImpactIfMissing is { Length: > 0 } impact) notes.Add(new(ReviewNoteKind.Warning, "Impact wenn ungeklärt", impact));
        notes.Add(new(ReviewNoteKind.Info, "L3-Metadaten",
            $"intent={r.Candidate.Intent ?? "?"}; gapCategory={r.Candidate.GapCategory ?? "?"}; requiresHumanDecision={r.Candidate.RequiresHumanDecision?.ToString() ?? "null"}"));
        if (r.Candidate.BasedOn is { Count: > 0 } basedOn)
            notes.Add(new(ReviewNoteKind.Info, "BasedOn (Recherche-Hinweise)", string.Join(", ", basedOn)));
        if (r.Candidate.Assumptions is { Count: > 0 } assumptions)
            notes.Add(new(ReviewNoteKind.Warning, "Annahmen", string.Join("\n", assumptions.Select(a => $"- {a}"))));
        if (r.NoAnchorReason is { Length: > 0 } nar) notes.Add(new(ReviewNoteKind.Suggestion, "Kein Anker", nar));

        // Existierende Anker und basedOn-Items als lazy nachladbarer Umwelt-Kontext (resolverKey = die Item-id).
        var ctx = r.Anchors.Where(a => a.Exists)
            .Select(a => new ContextBlock(ContextBlockKind.Reference, $"Anker {a.ItemId}", a.ItemId))
            .ToList();
        var seenContextIds = ctx.Select(c => c.ResolverKey).ToHashSet(StringComparer.Ordinal);
        foreach (var id in r.Candidate.BasedOn ?? [])
            if (seenContextIds.Add(id))
                ctx.Add(new ContextBlock(ContextBlockKind.Reference, $"BasedOn {id}", id));

        // Default-Vorschlag aus der Klasse (der Mensch kann ändern).
        var suggested = r.Class switch
        {
            L3Class.Contradicted => "reject",
            L3Class.WeakOrUncertain => "revise",
            _ => "accept"   // Unreferenced: plausibler Open-World-Vorschlag → Autorisierung anbieten
        };
        var reason = r.Class == L3Class.SupportedAnchored
            ? "auto: SupportedAnchored durch L3-Resolve/Judge; im Kontrollreview editierbar."
            : null;

        var item = new ReviewItem
        {
            ItemId = r.Candidate.CandidateId,
            Summary = r.Candidate.Text,
            Badge = r.Class.ToString(),
            Notes = notes,
            ContextBlocks = ctx,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, suggested),
                new ReviewFieldValue(FieldReason, reason)
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static string ClassNote(L3RoutedCandidate r) => r.Class switch
    {
        L3Class.Contradicted => "Widerspruch zu einem bestehenden Item — Konfliktentscheidung (reject ODER später Change Request), nicht automatisch verwerfen.",
        L3Class.Unreferenced => "Kein tragfähiger Anker — echter Open-World-Vorschlag. Nur mit menschlicher Autorisierung übernehmen.",
        _ => "Schwach/unsicher gestützt — entscheide accept/edit/reject oder revise (Feedback an den Agenten)."
    };

    /// <summary>Löst einen Umwelt-Kontextblock auf: resolverKey = Item-id → der Text aus der Umwelt.</summary>
    public static string ResolveContext(string resolverKey, IReadOnlyDictionary<string, ArtifactItem> envById)
        => envById.TryGetValue(resolverKey, out var it) ? $"**{it.ItemId}**\n\n{it.Text}" : $"(Item {resolverKey} nicht in der Umwelt)";

    public static bool Resolved(ReviewItem it)
    {
        var decision = FieldOf(it, FieldDecision);
        if (!Decisions.Contains(decision)) return false;

        var reason = FieldOf(it, FieldReason);
        if (string.IsNullOrWhiteSpace(reason)) return false;

        return !string.Equals(decision, "edit", StringComparison.OrdinalIgnoreCase)
               || !string.IsNullOrWhiteSpace(FieldOf(it, FieldEdited));
    }

    /// <summary>Wandelt die Session-Entscheidungen in die von apply/revise konsumierte <see cref="HumanDecisionsFile"/>.</summary>
    public static HumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(it =>
        {
            string? V(string k) => it.FieldValues.FirstOrDefault(f => f.FieldKey == k)?.Value;
            var decision = V(FieldDecision) ?? "";
            var edited = string.Equals(decision, "edit", StringComparison.OrdinalIgnoreCase) ? V(FieldEdited) : null;
            return new HumanDecision(it.ItemId, decision, edited, FinalAnchorIds: null, Reason: V(FieldReason));
        }).ToList();
        return new HumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    /// <summary>
    /// Re-Launch-Unterstuetzung: traegt eine vorhandene human-decisions.json zurueck in die ReviewSession ein, damit die
    /// UI eine unterbrochene Sitzung exakt dort fortsetzt, wo der Mensch aufgehoert hat.
    /// </summary>
    public static void MergeExistingDecisions(ReviewSession session, HumanDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.CandidateId, (item, decision) =>
        {
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldEdited, decision.EditedText);
            Set(item, FieldReason, decision.Reason);
        }, Resolved);

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
