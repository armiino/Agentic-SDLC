using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

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

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject", "revise"], Required: true,
            Help: "accept = als Projekt-Item autorisieren · edit = bearbeitete Fassung übernehmen · reject = verwerfen · revise = mit Feedback zurück an den Agenten"),
        new ReviewFieldSpec(FieldEdited, "Bearbeiteter Text (nur bei edit)", ReviewInputType.FreeText, [], Required: false),
        new ReviewFieldSpec(FieldReason, "Begründung / Feedback (bei revise: Anweisung an den Agenten)", ReviewInputType.FreeText, [], Required: false),
    ];

    /// <summary>Baut die Session aus den gerouteten Kandidaten (nur die, die der Mensch entscheiden muss).</summary>
    public static ReviewSession BuildSession(string runId, IReadOnlyList<L3RoutedCandidate> routed)
    {
        var items = routed.Where(r => r.Class != L3Class.SupportedAnchored).Select(BuildItem).ToList();
        return new ReviewSession
        {
            SessionId = $"l3-{runId}",
            Title = "L3 Open-World-Ableitung — Human Review",
            Subtitle = $"{items.Count} Kandidaten zur Autorisierung. Open-World ist maschinell nicht beweisbar → der Mensch entscheidet.",
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
        if (r.NoAnchorReason is { Length: > 0 } nar) notes.Add(new(ReviewNoteKind.Suggestion, "Kein Anker", nar));

        // Existierende Anker als lazy nachladbarer Umwelt-Kontext (resolverKey = die Item-id).
        var ctx = r.Anchors.Where(a => a.Exists)
            .Select(a => new ContextBlock(ContextBlockKind.Reference, $"Umwelt-Item {a.ItemId}", a.ItemId))
            .ToList();

        // Default-Vorschlag aus der Klasse (der Mensch kann ändern).
        var suggested = r.Class switch
        {
            L3Class.Contradicted => "reject",
            L3Class.WeakOrUncertain => "revise",
            _ => "accept"   // Unreferenced: plausibler Open-World-Vorschlag → Autorisierung anbieten
        };

        return new ReviewItem
        {
            ItemId = r.Candidate.CandidateId,
            Summary = r.Candidate.Text,
            Badge = r.Class.ToString(),
            Notes = notes,
            ContextBlocks = ctx,
            FieldValues = [new ReviewFieldValue(FieldDecision, suggested)]
        };
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

    public static bool Resolved(ReviewItem it) =>
        !string.IsNullOrWhiteSpace(it.FieldValues.FirstOrDefault(f => f.FieldKey == FieldDecision)?.Value);

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
}
