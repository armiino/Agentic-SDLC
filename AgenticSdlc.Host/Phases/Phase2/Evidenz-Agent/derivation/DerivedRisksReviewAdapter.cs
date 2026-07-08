using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// I-d — projiziert die abgeleiteten Risiken + ihre I-c-Verdikte in eine domänen-agnostische
/// <see cref="ReviewSession"/> und wendet die Autor-Entscheidungen an. Reuse des generischen
/// <c>AgenticSdlc.HumanReview</c>-UI (Präzedenz: <c>LedgerAdjudicationReviewAdapter</c>). Bewusst schlank: EIN
/// Entscheidungsfeld je Risiko (approve/reject/needs_revision) + optionaler Grund; die Anker-Requirements sind
/// lazy nachladbarer Kontext. Der Mensch ist hier die tragende Instanz (Inferenz = schwache Maschinen-Garantie).
/// </summary>
public static class DerivedRisksReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reviewReason";

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["approve", "reject", "needs_revision"], Required: true,
            Help: "approve = als abgeleitetes Risiko übernehmen · reject = verwerfen · needs_revision = Maker-Überarbeitung"),
        new ReviewFieldSpec(FieldReason, "Begründung (optional)", ReviewInputType.FreeText, [], Required: false),
    ];

    /// <summary>Baut die Review-Session. <paramref name="verdictsById"/> = I-c-Verdikte je itemId (für Badge/Notiz +
    /// Default-Vorschlag).</summary>
    public static ReviewSession BuildSession(
        ArtifactDocument derived,
        IReadOnlyDictionary<string, InferenceVerdict> verdictsById)
    {
        var items = derived.Items.Select(r => BuildItem(r, verdictsById.GetValueOrDefault(r.ItemId))).ToList();
        return new ReviewSession
        {
            SessionId = $"derived-risks-{derived.ArtifactId}",
            Title = "Abgeleitete Risiken — Human Review",
            Subtitle = $"{items.Count} abgeleitete Risiken (origin=derived), verankert an Requirements. Inferenz → der Mensch entscheidet.",
            FieldSchema = Schema(),
            Items = items
        };
    }

    private static ReviewItem BuildItem(ArtifactItem r, InferenceVerdict? verdict)
    {
        var notes = new List<ReviewNote>();
        if (verdict is not null)
        {
            var kind = verdict.Verdict switch
            {
                InferenceVerdictKind.Supported => ReviewNoteKind.Info,
                InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated => ReviewNoteKind.Warning,
                _ => ReviewNoteKind.Suggestion
            };
            notes.Add(new(kind, $"I-c-Verdikt: {verdict.Verdict}", verdict.Rationale));
        }
        if (r.Assumptions is { Count: > 0 })
            notes.Add(new(ReviewNoteKind.Suggestion, "Annahmen", string.Join(" · ", r.Assumptions)));
        if (!string.IsNullOrWhiteSpace(r.DerivationRationale))
            notes.Add(new(ReviewNoteKind.Reason, "Ableitung", r.DerivationRationale!));

        // Jede Anker-Anforderung als lazy nachladbarer Kontextblock (resolverKey = die REQ-id).
        var ctx = r.SourceArtifactItemIds
            .Select(id => new ContextBlock(ContextBlockKind.Reference, $"Anforderung {id}", id))
            .ToList();

        // Default-Vorschlag aus dem I-c-Verdikt (der Mensch kann ändern) — needs_revision bei unklar/geflaggt.
        var suggested = verdict?.Verdict switch
        {
            InferenceVerdictKind.Supported => "approve",
            InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated => "reject",
            _ => "needs_revision"
        };

        return new ReviewItem
        {
            ItemId = r.ItemId,
            Summary = r.Text,
            Badge = verdict?.Verdict.ToString() ?? "no-verdict",
            Notes = notes,
            ContextBlocks = ctx,
            FieldValues = [new ReviewFieldValue(FieldDecision, suggested)]
        };
    }

    /// <summary>Löst einen Kontextblock auf: resolverKey = REQ-id → der Anforderungstext aus der Baseline.</summary>
    public static string ResolveContext(string resolverKey, IReadOnlyDictionary<string, ArtifactItem> baselineById)
        => baselineById.TryGetValue(resolverKey, out var req)
            ? $"**{req.ItemId}**\n\n{req.Text}"
            : $"(Anforderung {resolverKey} nicht in der Baseline gefunden)";

    public static bool Resolved(ReviewItem it) =>
        !string.IsNullOrWhiteSpace(it.FieldValues.FirstOrDefault(f => f.FieldKey == FieldDecision)?.Value);

    /// <summary>Wendet die Entscheidungen an: liefert die APPROVED-Items (für approved-derived-risks.json) +
    /// das vollständige Entscheidungs-Protokoll (auch reject/needs_revision — Audit, Reviewer §11.4).</summary>
    public static (List<ArtifactItem> Approved, List<object> Decisions) Apply(
        ReviewSession session, IReadOnlyDictionary<string, ArtifactItem> derivedById)
    {
        var approved = new List<ArtifactItem>();
        var decisions = new List<object>();
        foreach (var it in session.Items)
        {
            var decision = it.FieldValues.FirstOrDefault(f => f.FieldKey == FieldDecision)?.Value ?? "";
            var reason = it.FieldValues.FirstOrDefault(f => f.FieldKey == FieldReason)?.Value;
            decisions.Add(new { itemId = it.ItemId, decision, reason });
            if (string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase) && derivedById.TryGetValue(it.ItemId, out var item))
                approved.Add(item);
        }
        return (approved, decisions);
    }
}
