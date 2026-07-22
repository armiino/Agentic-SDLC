using System.Text;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Der erste Adapter der generischen Human-Review-Schicht (<see cref="AgenticSdlc.HumanReview"/>):
/// projiziert eine <see cref="AdjudicationQueue"/> in eine domaenen-agnostische <see cref="ReviewSession"/>
/// und wieder zurueck. Enthaelt die einzige adjudikations-spezifische Logik (Feldschema, gate-lite
/// „resolved", Kontext-Aufloesung). Der Server/das Frontend kennen davon nichts.
/// </summary>
public static class LedgerAdjudicationReviewAdapter
{
    public const string FieldAction = "action";
    public const string FieldReason = "actionReason";
    public const string FieldTarget = "referenceTarget";
    public const string FieldRepairStatus = "repair.status";
    public const string FieldRepairModality = "repair.modality";
    public const string FieldRepairTimeScope = "repair.timeScope";
    public const string FieldRepairScope = "repair.scope";

    /// <summary>Transkript-Herkunft eines Atomic-Unit (für die Evidenz-Auflösung von unit_signal-Items).</summary>
    public sealed record UnitRef(string Speaker, int TurnIndex, string Text);

    private static IReadOnlyList<ReviewFieldSpec> Schema(IReadOnlyList<ReviewOption>? referenceOptions) =>
    [
        new(FieldAction, "Aktion", ReviewInputType.Dropdown, AdjudicationActions.All, Required: true,
            Help: "unit_signal: promote_to_claim (eigener Claim) | attach_evidence (an Ziel-Claim hängen) | mark_covered_by | reject | defer · "
                  + "review_required: apply_repair | accept_gap | merge_existing | reject | defer"),
        new(FieldRepairStatus, "Repair Status", ReviewInputType.Dropdown,
            ["decided", "open", "rejected", "uncertain", "required"], Required: false,
            Help: "nur bei apply_repair: gewählten Status in den finalen Claim übernehmen"),
        new(FieldRepairModality, "Repair Modality", ReviewInputType.Dropdown,
            ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"], Required: false,
            Help: "nur bei apply_repair: gewählte Modalität in den finalen Claim übernehmen"),
        new(FieldRepairTimeScope, "Repair TimeScope", ReviewInputType.Dropdown,
            ["mvp", "later_possible", "mvp_or_later_unclear"], Required: false,
            Help: "nur bei apply_repair: gewählten Zeitbezug in den finalen Claim übernehmen"),
        new(FieldRepairScope, "Repair Scope", ReviewInputType.FreeText, [], Required: false,
            Help: "nur bei apply_repair: optionalen Scope-Repair übernehmen"),
        new(FieldReason, "Begründung", ReviewInputType.FreeText, [], Required: false,
            Help: "kurze Begründung der Entscheidung (optional)"),
        new(FieldTarget, "Referenz-Ziel", ReviewInputType.FreeText, [], Required: false,
            Help: "bei attach_evidence / merge_existing / mark_covered_by: Claim-ID tippen oder aus der Liste wählen (bei unit_signal mit Systemvorschlag vorbelegt)",
            Options: referenceOptions),
    ];

    /// <param name="referenceOptions">Katalog vorhandener Claims (Value=id, Label="id — proposition") für das
    /// Referenz-Ziel-Autocomplete. null = einfaches Freitextfeld (wenn kein validated Ledger geladen).</param>
    public static ReviewSession BuildSession(AdjudicationQueue queue, IReadOnlyList<ReviewOption>? referenceOptions = null)
    {
        var items = queue.Items.Select(BuildItem).ToList();
        var sub = $"validated={queue.SourceValidatedRunId ?? "?"}"
                  + (queue.SourceUnitRunId is { } u ? $" · units={u}" : "")
                  + $" · {items.Count} Items";
        return new ReviewSession
        {
            SessionId = queue.SourceValidatedRunId ?? "adjudication",
            Title = "Ledger-Adjudikation",
            Subtitle = sub,
            FieldSchema = Schema(referenceOptions),
            Items = items,
        };
    }

    private static ReviewItem BuildItem(AdjudicationItem a)
    {
        // System-Vorschlag + Grund als hervorgehobene Notes (nicht mehr als Fließtext in der Summary).
        var notes = new List<ReviewNote>();
        var suggestions = EffectiveSuggestions(a).ToList();
        if (suggestions.Count > 0)
        {
            var hasFacetRepairs = suggestions.Any(s => string.Equals(s.Kind, "facet_repair", StringComparison.OrdinalIgnoreCase));
            var text = string.Join("\n", suggestions.Select(FormatSuggestion));
            var label = hasFacetRepairs && suggestions.Count > 1
                ? $"System-Vorschlag · facet_repair ({suggestions.Count})"
                : $"System-Vorschlag · {suggestions[0].Kind}";
            notes.Add(new(ReviewNoteKind.Suggestion, label, text));
        }
        // Grund nur zeigen, wenn er nicht wortgleich zur Proposition ist (bei unit_signal oft identisch).
        if (!string.IsNullOrWhiteSpace(a.Reason)
            && !string.Equals(a.Reason.Trim(), a.Proposition.Trim(), StringComparison.Ordinal))
            notes.Add(new(ReviewNoteKind.Reason, "Grund", a.Reason));

        var ctx = new List<ContextBlock>();
        // Evidenz-Block: review_required trägt Quotes; unit_signal trägt den Unit-Bezug (lazy aufgelöst).
        if (a.EvidenceRefs.Count > 0 || a.UnitId is not null)
        {
            var label = a.UnitId is not null ? "Evidenz & Bezug öffnen" : $"Evidenz öffnen ({a.EvidenceRefs.Count})";
            ctx.Add(new(ContextBlockKind.Quote, label, "evidence"));
            ctx.Add(new(ContextBlockKind.Excerpt, "Transkript-Kontext öffnen", "transcript"));
        }

        return new ReviewItem
        {
            ItemId = a.ItemId,
            Badge = a.ItemType,
            Summary = a.Proposition,
            Notes = notes,
            ContextBlocks = ctx,
            FieldValues =
            [
                new(FieldAction, a.Action),
                ..RepairFieldValues(a),
                new(FieldReason, a.ActionReason),
                // KEIN Pre-Fill aus claimId: unit_signal-claimIds sind Candidate-IDs (nach Canonicalization nicht
                // im validated Ledger). Der Autor wählt das kanonische Ziel über das A5-Autocomplete (referenceTarget).
                new(FieldTarget, a.ReferenceTarget),
            ],
            Resolved = false, // wird gleich neu berechnet
        };
    }

    /// <summary>gate-lite: eine Zeile ist „entschieden", wenn sie eine gueltige Aktion hat und (bei
    /// merge/mark) ein referenceTarget. Spiegelt <c>AdjudicationCompletenessGate</c> fuer die Live-Anzeige;
    /// das echte Gate bleibt die harte Instanz beim apply.</summary>
    public static bool Resolved(ReviewItem it)
    {
        var action = FieldOf(it, FieldAction);
        if (!AdjudicationActions.IsValid(action)) return false;
        var needsTarget = string.Equals(action, AdjudicationActions.MergeExisting, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(action, AdjudicationActions.MarkCoveredBy, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(action, AdjudicationActions.AttachEvidence, StringComparison.OrdinalIgnoreCase);
        return !needsTarget || !string.IsNullOrWhiteSpace(FieldOf(it, FieldTarget));
    }

    /// <summary>Traegt die im UI gesetzten Feldwerte zurueck in das urspruengliche Queue-Item (fuer Autosave).</summary>
    public static AdjudicationItem MergeInto(AdjudicationItem original, ReviewItem reviewed) => original with
    {
        Action = Empty(FieldOf(reviewed, FieldAction)),
        ActionReason = Empty(FieldOf(reviewed, FieldReason)),
        ReferenceTarget = Empty(FieldOf(reviewed, FieldTarget)),
        RepairStatus = Empty(FieldOf(reviewed, FieldRepairStatus)),
        RepairModality = Empty(FieldOf(reviewed, FieldRepairModality)),
        RepairTimeScope = Empty(FieldOf(reviewed, FieldRepairTimeScope)),
        RepairScope = Empty(FieldOf(reviewed, FieldRepairScope)),
    };

    private static IEnumerable<AdjudicationSuggestion> EffectiveSuggestions(AdjudicationItem item)
    {
        if (item.SystemSuggestions is { Count: > 0 }) return item.SystemSuggestions;
        return item.SystemSuggestion is null ? [] : [item.SystemSuggestion];
    }

    private static string FormatSuggestion(AdjudicationSuggestion s)
    {
        if (s.Facet is not null)
            return $"{s.Facet}: {s.Observed ?? "?"} -> {s.Suggested ?? "?"}";
        return s.Classification ?? "(ohne Detail)";
    }

    private static ReviewFieldValue[] RepairFieldValues(AdjudicationItem item)
    {
        var suggestions = EffectiveSuggestions(item)
            .Where(s => string.Equals(s.Kind, "facet_repair", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (suggestions.Count == 0) return [];

        var values = new List<ReviewFieldValue>();
        AddRepairValue(values, suggestions, "status", FieldRepairStatus, item.RepairStatus);
        AddRepairValue(values, suggestions, "modality", FieldRepairModality, item.RepairModality);
        AddRepairValue(values, suggestions, "timescope", FieldRepairTimeScope, item.RepairTimeScope);
        AddRepairValue(values, suggestions, "scope", FieldRepairScope, item.RepairScope);
        return values.ToArray();
    }

    private static void AddRepairValue(
        List<ReviewFieldValue> values,
        IReadOnlyList<AdjudicationSuggestion> suggestions,
        string facet,
        string fieldKey,
        string? selected)
    {
        var suggestion = suggestions.FirstOrDefault(s => string.Equals(NormalizeFacet(s.Facet), facet, StringComparison.Ordinal));
        if (suggestion is null) return;
        values.Add(new ReviewFieldValue(fieldKey, string.IsNullOrWhiteSpace(selected) ? suggestion.Suggested : selected));
    }

    private static string NormalizeFacet(string? facet)
        => string.Equals(facet, "timeScope", StringComparison.OrdinalIgnoreCase)
            ? "timescope"
            : (facet ?? "").Trim().ToLowerInvariant();

    /// <summary>
    /// Löst den Evidenz-Block eines Items auf. Für <c>review_required</c> sind die EvidenceRefs bereits
    /// Transkript-Quotes. Für <c>unit_signal</c> ist die echte Evidenz der Atomic-Unit-Text (aus
    /// <paramref name="units"/>), und die EvidenceRefs sind bezogene Claim-IDs → über <paramref name="claimProps"/>
    /// zu lesbaren Propositionen aufgelöst (sonst sähe der Autor nur nackte IDs).
    /// </summary>
    public static string ResolveContext(
        AdjudicationQueue queue, string itemId, string blockKey,
        IReadOnlyDictionary<string, string> claimProps,
        IReadOnlyDictionary<string, string> candidateToCanonicalClaimIds,
        IReadOnlyDictionary<string, IReadOnlyList<string>> claimSourceUnitIds,
        IReadOnlyDictionary<string, UnitRef> units)
    {
        var item = queue.Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item is null) return "(Item nicht gefunden)";
        if (blockKey == "transcript") return ResolveTranscriptContext(item, claimSourceUnitIds, units);
        if (blockKey != "evidence") return "(unbekannter Kontextblock)";

        if (item.UnitId is { } uid)
        {
            var sb = new StringBuilder();
            if (units.TryGetValue(uid, out var u))
                sb.AppendLine($"Transkript-Evidenz (Unit {uid}):")
                  .AppendLine($"  {u.Speaker} (Turn {u.TurnIndex}): {u.Text}");
            else
                sb.AppendLine($"(Unit {uid} nicht in step-00 gefunden)");

            var related = item.EvidenceRefs.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (related.Count > 0)
            {
                sb.AppendLine().AppendLine("Bezogener Claim (Kandidat für Referenz-Ziel):");
                foreach (var id in related)
                    AppendRelatedClaim(sb, id, claimProps, candidateToCanonicalClaimIds);
            }
            return sb.ToString().TrimEnd();
        }

        return item.EvidenceRefs.Count == 0
            ? "(keine Evidenz hinterlegt)"
            : string.Join("\n\n", item.EvidenceRefs.Select((q, n) => $"[{n + 1}] {q}"));
    }

    private static void AppendRelatedClaim(
        StringBuilder sb,
        string id,
        IReadOnlyDictionary<string, string> claimProps,
        IReadOnlyDictionary<string, string> candidateToCanonicalClaimIds)
    {
        sb.Append("  ").Append(id).Append(" — ")
          .AppendLine(claimProps.TryGetValue(id, out var p) ? p : "(Proposition nicht gefunden)");

        if (!candidateToCanonicalClaimIds.TryGetValue(id, out var canonicalId)
            || string.Equals(canonicalId, id, StringComparison.Ordinal))
            return;

        sb.Append("    -> kanonischer Zielclaim: ").Append(canonicalId);
        if (claimProps.TryGetValue(canonicalId, out var canonicalProp) && !string.IsNullOrWhiteSpace(canonicalProp))
            sb.Append(" — ").Append(canonicalProp);
        sb.AppendLine();
        sb.AppendLine("       Als referenceTarget diesen kanonischen Claim verwenden.");
    }

    private static string ResolveTranscriptContext(
        AdjudicationItem item,
        IReadOnlyDictionary<string, IReadOnlyList<string>> claimSourceUnitIds,
        IReadOnlyDictionary<string, UnitRef> units)
    {
        if (units.Count == 0) return "(kein Transkript-/Unit-Kontext gefunden)";

        var highlightIds = new HashSet<string>(StringComparer.Ordinal);
        if (item.UnitId is { } uid) highlightIds.Add(uid);
        if (item.ClaimId is { } claimId && claimSourceUnitIds.TryGetValue(claimId, out var sourceIds))
            foreach (var id in sourceIds)
                if (!string.IsNullOrWhiteSpace(id)) highlightIds.Add(id);

        // Fallback fuer alte/non-unit Runs oder Claims ohne sourceUnitIds: Quotes best-effort im Unit-Text suchen.
        if (highlightIds.Count == 0)
        {
            foreach (var quote in item.EvidenceRefs.Where(q => !string.IsNullOrWhiteSpace(q)))
            {
                var needle = NormalizeQuoteForMatch(quote);
                if (!IsUsefulNeedle(needle)) continue;
                foreach (var (id, u) in units)
                {
                    var text = NormalizeForMatch(u.Text);
                    if (IsUsefulNeedle(text) && (text.Contains(needle, StringComparison.Ordinal) || needle.Contains(text, StringComparison.Ordinal)))
                        highlightIds.Add(id);
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("Transkript-Kontext");
        sb.AppendLine("Marker >> = sourceUnitIds dieses Claims bzw. relevante Evidence/Unit fuer dieses Review-Item.");
        sb.AppendLine();

        foreach (var (id, u) in units.OrderBy(kv => kv.Value.TurnIndex).ThenBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var marker = highlightIds.Contains(id) ? ">> " : "   ";
            sb.Append(marker)
              .Append(id)
              .Append(" | ")
              .Append(u.Speaker)
              .Append(" (Turn ")
              .Append(u.TurnIndex)
              .Append("): ")
              .AppendLine(u.Text);
        }

        return sb.ToString().TrimEnd();
    }

    private static string NormalizeQuoteForMatch(string text)
    {
        var s = NormalizeForMatch(text);
        var colon = s.IndexOf(':', StringComparison.Ordinal);
        if (colon >= 0 && colon < 24) s = s[(colon + 1)..].Trim();
        s = s.Replace("...", " ", StringComparison.Ordinal)
             .Replace("…", " ", StringComparison.Ordinal)
             .Replace("[", " ", StringComparison.Ordinal)
             .Replace("]", " ", StringComparison.Ordinal);
        while (s.Contains("  ", StringComparison.Ordinal)) s = s.Replace("  ", " ", StringComparison.Ordinal);
        return s.Trim();
    }

    private static string NormalizeForMatch(string text)
    {
        var s = text.Trim().Trim('"', '\'', '“', '”', '„');
        while (s.Contains("  ", StringComparison.Ordinal)) s = s.Replace("  ", " ", StringComparison.Ordinal);
        return s.ToLowerInvariant();
    }

    private static bool IsUsefulNeedle(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length < 12) return false;

        var tokens = normalized
            .Split([' ', '\t', '.', ',', ';', ':', '!', '?', '-', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 3)
            .ToList();
        if (tokens.Count < 2) return false;

        var generic = new HashSet<string>(StringComparer.Ordinal)
        {
            "genau", "okay", "stimmt", "richtig", "gut", "punkt", "dann", "also"
        };
        return tokens.Any(t => !generic.Contains(t));
    }

    private static string? FieldOf(ReviewItem it, string key) => it.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
    private static string? Empty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
