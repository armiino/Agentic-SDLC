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
        new(FieldRepairStatus, "Repair: Status", ReviewInputType.Dropdown,
            ["decided", "open", "rejected", "uncertain", "required"], Required: false,
            Help: "nur bei apply_repair: gewählten Status in den finalen Claim übernehmen"),
        new(FieldRepairModality, "Repair: Modalität", ReviewInputType.Dropdown,
            ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"], Required: false,
            Help: "nur bei apply_repair: gewählte Modalität in den finalen Claim übernehmen"),
        new(FieldRepairTimeScope, "Repair: Zeitbezug", ReviewInputType.Dropdown,
            ["mvp", "later_possible", "mvp_or_later_unclear"], Required: false,
            Help: "nur bei apply_repair: gewählten Zeitbezug in den finalen Claim übernehmen"),
        new(FieldRepairScope, "Repair: Geltungsbereich", ReviewInputType.FreeText, [], Required: false,
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
            Help = BuildHelp(),
            FieldSchema = Schema(referenceOptions),
            Items = items,
        };
    }

    /// <summary>i-Button-Inhalt (R-5/Autor-Wunsch 2026-07-23): Felder, Werte UND Wirkung beim Apply —
    /// muss synchron bleiben mit <see cref="AdjudicationCompletenessGate.Project"/> und `01-ledger/README.md`.</summary>
    private static ReviewHelp BuildHelp() => new(
        "Ledger-Adjudikation",
        "Du bist die letzte Instanz vor der freigegebenen Evidenzschicht (consumable.json). Alles, was du hier "
        + "annimmst, wird Faktenbasis für Baselines → Core → Backlog → GitHub-Issues. Was du ablehnst, verlässt "
        + "den Produktpfad endgültig (bleibt aber im Audit nachvollziehbar).",
        [
            new ReviewHelpSection("Die zwei Item-Typen",
                "review_required_claim (RR::…): ein extrahierter Claim, dessen Facetten-Validierung nicht 'grounded' war "
                + "(partial/overstated/unsupported) — der Validator-Befund steht im System-Vorschlag.\n"
                + "unit_signal (US::…): eine Transkript-Aussage, die der Ledger NICHT abdeckt (missing_claim) oder die das "
                + "System nicht sicher zuordnen konnte (needs_human) bzw. als Zusatz-Evidenz vorschlägt (attach)."),
            new ReviewHelpSection("Aktion — was beim Apply WIRKLICH passiert",
                "accept_gap: Claim wird AS-IS in den consumable übernommen (bei unit_signal: neuer Claim aus der Aussage, "
                + "Facetten noch offen → facetStatus=pending, danach ledger-adjudicate-refine).\n"
                + "apply_repair (nur RR): Claim wird MIT deinen Repair-Feldern (unten) korrigiert übernommen.\n"
                + "promote_to_claim (nur unit_signal): die Aussage wird ein EIGENER neuer Claim (facetStatus=pending → refine).\n"
                + "attach_evidence (nur unit_signal, braucht Referenz-Ziel): das Transkript-Zitat wird als zusätzliche Evidenz "
                + "an den Ziel-Claim gehängt — KEIN neuer Claim.\n"
                + "merge_existing / mark_covered_by (brauchen Referenz-Ziel): NUR Audit-Eintrag 'gehört zu X' / 'ist durch X "
                + "abgedeckt' — der consumable ändert sich NICHT.\n"
                + "reject: Claim/Aussage kommt NICHT in den consumable — endgültig raus aus dem Produktpfad (Audit bleibt).\n"
                + "defer: keine Entscheidung — zählt als pending und blockiert den sauberen Abschluss (Ziel: pending=0)."),
            new ReviewHelpSection("Repair: Status (nur bei apply_repair) — Entscheidungsstand des Claims",
                "decided: im Gespräch entschieden · open: bewusst offen · rejected: im Gespräch verworfen · "
                + "uncertain: unklar/widersprüchlich · required: EXTERN verpflichtend (Gesetz/Auflage).\n"
                + "Achtung Rasierklingen-Regel: status=required verlangt Modalität must oder must_not — sonst schlägt das "
                + "Ledger-Gate fehl (INCONSISTENT_REQUIRED_MODALITY)."),
            new ReviewHelpSection("Repair: Modalität (nur bei apply_repair) — Verbindlichkeit",
                "must: harte Pflicht · must_not: Verbot · must_clarify: MUSS noch geklärt werden · "
                + "must_consider: muss berücksichtigt/abgewogen werden · must_note: muss festgehalten werden · "
                + "desired: gewünscht, nicht verpflichtend · optional: nice-to-have.\n"
                + "Wirkung (belegt): steht dem Baseline-Maker als Claim-Kontext im Prompt UND der Contract-Checker (C3) "
                + "verbietet dem Artefakt harte Formulierungen ('muss/entschieden'), wenn die Facette WEICH ist "
                + "(desired/optional/must_clarify/must_consider/must_note). Entscheidend ist also vor allem die Seite "
                + "hart↔weich; Feinunterschiede INNERHALB 'weich' haben derzeit keinen maschinellen Konsumenten (Doku/Audit)."),
            new ReviewHelpSection("Repair: Zeitbezug (nur bei apply_repair)",
                "mvp: gehört in den MVP · later_possible: später möglich/geplant · mvp_or_later_unclear: Zuordnung unklar.\n"
                + "Wirkung (belegt): weiche Zeitwerte (later_possible/unclear) verbieten dem Artefakt MVP-Behauptungen "
                + "(Contract-Checker C3). Einen direkten deterministischen Backlog-Schnitt-Konsumenten gibt es derzeit "
                + "NICHT — weiterer Einfluss läuft über den generierten Artefakt-Text."),
            new ReviewHelpSection("Repair: Geltungsbereich (nur bei apply_repair)",
                "Freitext: FÜR WEN/WO gilt der Claim (z. B. 'Pflegekräfte', 'gesamte Einrichtung'). "
                + "Leer lassen = Geltungsbereich des Claims bleibt unverändert."),
            new ReviewHelpSection("Referenz-Ziel",
                "Pflicht bei attach_evidence, merge_existing, mark_covered_by: die ID des existierenden Ziel-Claims "
                + "(Autocomplete: 'id — Proposition'). Ohne gültiges Ziel gilt die Zeile als nicht entschieden."),
            new ReviewHelpSection("Vorbelegung & System-Vorschlag",
                "Repair-Felder sind mit dem Validator-Vorschlag vorbelegt (observed → suggested) — du kannst jeden Wert "
                + "übersteuern. Der System-Vorschlag ist nie bindend: DU entscheidest."),
            new ReviewHelpSection("Woher kommen die Items & Vorschläge?",
                "RR-Items + Repair-Vorschläge: aus der Facetten-Validierung (step-03) — ein Prüf-LLM bewertet jeden "
                + "kanonischen Claim einzeln gegen das Transkript (grounded/partial/overstated/unsupported) und schlägt "
                + "Facetten-Korrekturen vor (observed → suggested). US-Items: aus der Unused-Pipeline (step-01c/01d) — "
                + "deterministisch segmentierte, nicht verwendete Transkript-Aussagen, die ein Vergleichs-LLM als "
                + "fehlend/unklar/anhängbar einstuft.\n"
                + "Vorschläge sind IMMER Modell-Urteile — die Mechanik (Gates/Traces) garantiert nur, dass nichts "
                + "unbilanziert verloren geht."),
            new ReviewHelpSection("Was die Adjudikation NICHT ändert (Grenzen)",
                "merge_existing / mark_covered_by sind reine Audit-Einträge — sie verändern den consumable NICHT.\n"
                + "Die Adjudikation bestimmt, WAS Fakt ist — nicht die spätere FORMULIERUNG: das Paraphrasieren "
                + "übernehmen die Folgestufen; deren Treue prüfen eigene Gates (Fidelity/Checker).\n"
                + "Drei Wege laufen bewusst an dir vorbei (sonst müsstest du alle Units einzeln reviewen): "
                + "grounded-Claims (direkt approved), als noise triagierte Units (step-01c), already_covered mit "
                + "gültiger Referenz (seit R-1 referenz-erzwungen). Alle drei sind in den Run-Artefakten auditierbar."),
            new ReviewHelpSection("Was nach 'Fertig' passiert",
                "ledger-adjudicate-apply schreibt: adjudicated-ledger.json (Audit ALLER Entscheidungen), consumable.json "
                + "(finale Claim-Menge = approved + deine Annahmen − rejects) und gate.json (hartes Vollständigkeits-Gate — "
                + "unentschiedene Zeilen oder fehlende Referenz-Ziele = Fehler). Bei neuen Claims (accept_gap aus Unit / "
                + "promote_to_claim): ledger-adjudicate-refine vergibt die fehlenden Facetten. Der consumable ist danach "
                + "der Input für 02-baselines (recipe)."),
        ]);

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
