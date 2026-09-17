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

    // E0.4: EINE Label-Quelle für Dropdown-Optionen UND den System-Vorschlag (kein Drift). Value = Enum (intern),
    // Label = kurzer Klartext; die hart/weich-Einordnung (die einzige, die C3 unterscheidet) steht in der Feld-Hilfe.
    private static readonly ReviewOption[] StatusOptions =
    [
        new("decided", "entschieden"),
        new("open", "offen"),
        new("rejected", "verworfen"),
        new("uncertain", "unklar / widersprüchlich"),
        new("required", "extern vorgeschrieben"),
    ];
    private static readonly ReviewOption[] ModalityOptions =
    [
        new("must", "MUSS"),
        new("must_not", "DARF NICHT"),
        new("must_clarify", "muss erst GEKLÄRT werden"),
        new("must_consider", "muss ABGEWOGEN werden"),
        new("must_note", "muss FESTGEHALTEN werden"),
        new("desired", "gewünscht"),
        new("optional", "optional"),
    ];
    private static readonly ReviewOption[] TimeScopeOptions =
    [
        new("mvp", "MVP"),
        new("later_possible", "später möglich / geplant"),
        new("mvp_or_later_unclear", "unklar"),
    ];

    // Gültige Aktionen je Item-Typ (E0.4-Filter): RR = review_required_claim, US = unit_signal/coverage_miss. WIRKUNG oben,
    // AUDIT unten. mark_covered_by ist mit merge_existing zusammengeführt (am consumable identisch) — nicht mehr angeboten.
    private static readonly ReviewOption[] RrActionOptions =
    [
        new(AdjudicationActions.AcceptGap, "Unverändert übernehmen"),
        new(AdjudicationActions.ApplyRepair, "Korrektur übernehmen"),
        new(AdjudicationActions.MergeExisting, "Gehört zu / abgedeckt durch bestehenden (Audit)"),
        new(AdjudicationActions.Reject, "Verwerfen (Audit) — Begründung Pflicht"),
        new(AdjudicationActions.Defer, "Offen lassen (Audit)"),
    ];
    private static readonly ReviewOption[] UsActionOptions =
    [
        new(AdjudicationActions.PromoteToClaim, "Als neuen Claim übernehmen"),
        new(AdjudicationActions.AttachEvidence, "Als Beleg an bestehenden Claim anhängen"),
        new(AdjudicationActions.MergeExisting, "Gehört zu / abgedeckt durch bestehenden (Audit)"),
        new(AdjudicationActions.Reject, "Verwerfen (Audit) — Begründung Pflicht"),
        new(AdjudicationActions.Defer, "Offen lassen (Audit)"),
    ];

    private static bool IsReviewRequired(AdjudicationItem a) =>
        string.Equals(a.ItemType, "review_required_claim", StringComparison.OrdinalIgnoreCase);

    // E0.9-Konsistenz (03.08.): Badge in Klartext statt rohem itemType — Voraussetzung dafür, dass das Glossar
    // (wie an allen Gates) auf die ANGEZEIGTEN Labels keyen kann.
    private static string BadgeLabel(string itemType) => itemType?.Trim().ToLowerInvariant() switch
    {
        "review_required_claim" => "Claim zur Prüfung",
        "unit_signal" => "Transkript-Signal",
        "coverage_miss" => "Abdeckungs-Lücke",
        _ => itemType ?? "?",
    };

    private const string GTypes = "Item-Typen (Badge)";
    private const string GActions = "Aktionen — Wirkung vs. Audit";
    private const string GFacets = "Facetten (Claim-Eigenschaften)";
    private const string GTerms = "Begriffe";

    private static IReadOnlyList<ReviewGlossaryEntry> Glossary() =>
    [
        new("Claim zur Prüfung", "Ein extrahierter Claim, dessen Facetten-Validierung nicht sicher war (teilweise belegt / überzogen / nicht belegt) — der Validator-Befund steht im System-Vorschlag.", GTypes),
        new("Transkript-Signal", "Eine Transkript-Aussage, die der Ledger nicht abdeckt oder nicht sicher zuordnen konnte — das Vergleichs-Verdikt steht im System-Vorschlag.", GTypes),
        new("Abdeckungs-Lücke", "Eine vom Vergleich gemeldete Stelle, an der der Ledger eine Aussage des Transkripts nicht abdeckt.", GTypes),
        new("Unverändert übernehmen", "WIRKT: der Claim geht unverändert in die finale Claim-Menge (consumable).", GActions),
        new("Korrektur übernehmen", "WIRKT: der Claim geht mit deinen korrigierten Facetten in die finale Claim-Menge.", GActions),
        new("Als neuen Claim übernehmen", "WIRKT: aus der Transkript-Aussage wird ein neuer Claim (Facetten vergibt der Refine-Schritt).", GActions),
        new("Als Beleg an bestehenden Claim anhängen", "WIRKT: die Aussage wird Zusatz-Evidenz am gewählten bestehenden Claim.", GActions),
        new("Gehört zu / abgedeckt durch bestehenden (Audit)", "Nur Audit: vermerkt die Zuordnung im Protokoll — die finale Claim-Menge ändert sich nicht.", GActions),
        new("Verwerfen (Audit)", "Nur Audit: das Item kommt nicht in die finale Claim-Menge. Begründung ist Pflicht — sie ist die einzige Spur der Ablehnung.", GActions),
        new("Offen lassen (Audit)", "Nur Audit: bleibt als unentschieden protokolliert — blockiert das Gate NICHT.", GActions),
        new("Verbindlichkeit", "Wie verpflichtend die Anforderung ist (MUSS · DARF NICHT · gewünscht · optional · Klärungs-/Abwägungs-/Notiz-Pflicht).", GFacets),
        new("Zeitbezug", "Wann es gelten soll: MVP · später möglich/geplant · unklar.", GFacets),
        new("Entscheidungsstand", "Wie entschieden die Sache ist: entschieden · offen · verworfen · unklar/widersprüchlich · extern vorgeschrieben.", GFacets),
        new("Geltungsbereich", "Für wen/was der Claim gilt (z. B. App, Rolle, Bereich).", GFacets),
        new("Claim", "Eine belegte Einzel-Aussage aus dem Transkript — die kleinste Einheit der Evidenzschicht.", GTerms),
        new("consumable", "Die finale Claim-Menge dieses Laufs (freigegeben + deine Annahmen − Verworfenes) — der Input für die Baselines.", GTerms),
        new("System-Vorschlag", "Was Validator bzw. Vergleichs-LLM für dieses Item empfehlen — der Ein-Klick-Button übernimmt genau das.", GTerms)
    ];

    // US-Vorschlag (compare_classification): das Verdikt des Vergleichs-LLM in Klartext statt des rohen Enum-Werts.
    private static readonly IReadOnlyDictionary<string, string> VerdictLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["missing_claim"] = "Fehlt als eigener Claim",
        ["attach_as_evidence"] = "Zusatz-Beleg für einen bestehenden Claim",
        ["already_covered_indirectly"] = "Schon durch einen bestehenden Claim abgedeckt",
        ["needs_human"] = "Unklar — bitte selbst entscheiden",
    };

    // Verdikte, für die es ein sinnvolles Ziel (relatedCandidate) gibt → Referenz-Ziel vorbelegen.
    private static bool VerdictWantsTarget(string? verdict) =>
        string.Equals(verdict, "attach_as_evidence", StringComparison.OrdinalIgnoreCase)
        || string.Equals(verdict, "already_covered_indirectly", StringComparison.OrdinalIgnoreCase);

    /// <summary>Klartext-Titel einer Facette (auch für die rechte Claim-Detail-Ansicht wiederverwendet).</summary>
    public static string FacetTitle(string? facet) => NormalizeFacet(facet) switch
    {
        "modality" => "Verbindlichkeit",
        "timescope" => "Zeitbezug",
        "status" => "Entscheidungsstand",
        "scope" => "Geltungsbereich",
        _ => facet ?? "?",
    };

    /// <summary>Klartext-Label eines Facetten-Werts — dieselbe Quelle wie die Dropdowns (auch rechte Detail-Ansicht).</summary>
    public static string FacetValueLabel(string? facet, string? value)
    {
        if (string.IsNullOrEmpty(value)) return "?";
        var opts = NormalizeFacet(facet) switch
        {
            "modality" => ModalityOptions,
            "timescope" => TimeScopeOptions,
            "status" => StatusOptions,
            _ => null,
        };
        return opts?.FirstOrDefault(o => string.Equals(o.Value, value, StringComparison.OrdinalIgnoreCase))?.Label ?? value;
    }

    /// <summary>Validierungs-Verdikt eines Claims in Klartext (rechte Detail-Ansicht).</summary>
    public static string ValidationVerdictLabel(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "grounded" => "belegt",
        "partial" => "teilweise belegt",
        "overstated" => "überzogen formuliert",
        "unsupported" => "nicht belegt",
        _ => verdict ?? "?",
    };

    /// <summary>Claim-Status (Validierungs-Ergebnis) in Klartext (rechte Detail-Ansicht).</summary>
    public static string ClaimStatusLabel(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "approved" => "freigegeben",
        "review_required" => "zur Prüfung markiert (Validierung unsicher)",
        _ => status ?? "?",
    };

    private static IReadOnlyList<ReviewFieldSpec> Schema(IReadOnlyList<ReviewOption>? referenceOptions)
    {
        // E0.4: Repair-Felder nur bei apply_repair zeigen, Referenz-Ziel nur bei Verknüpfen — der Rest ist Ballast je Zeile.
        var onlyRepair = new ReviewFieldVisibility(FieldAction, [AdjudicationActions.ApplyRepair]);
        var onlyReference = new ReviewFieldVisibility(FieldAction,
            [AdjudicationActions.AttachEvidence, AdjudicationActions.MergeExisting, AdjudicationActions.MarkCoveredBy]);
        return
        [
            // Aktion: Labels + Reihenfolge kommen PER ITEM (RR- vs US-Set, s. ReviewItem.FieldOptions in BuildItem) — je Item
            // nur die GÜLTIGEN Aktionen. AllowedValues bleibt die volle Menge (Validierung). WIRKUNG oben / AUDIT unten.
            new(FieldAction, "Aktion", ReviewInputType.Dropdown, AdjudicationActions.All, Required: true,
                Help: "Was mit dem Item passiert. OBEN ändert die Evidenzschicht (consumable): Übernehmen · Korrektur · neuer Claim · "
                      + "Beleg anhängen. UNTEN = nur Audit, KEIN consumable-Effekt (Vermerk, warum das Item KEIN neuer Claim wird). "
                      + "Offen-lassen bleibt pending und wird NICHT automatisch aufgelöst."),

            // Repair-Facetten — Klartext-Labels (Wert bleibt intern das Enum), hart↔weich sichtbar; die harte/weiche Grenze
            // ist das einzige, was der Contract-Checker C3 unterscheidet, die Fein-Werte sind LLM-Kontext + Audit.
            new(FieldRepairStatus, "Entscheidungsstand (Status)", ReviewInputType.Dropdown,
                ["decided", "open", "rejected", "uncertain", "required"], Required: false,
                Help: "Wie steht es um den Claim? Der Wert extern-vorgeschrieben verlangt Modalität MUSS oder DARF NICHT (sonst Gate-Fehler).",
                Options: StatusOptions,
                VisibleWhen: onlyRepair),
            new(FieldRepairModality, "Verbindlichkeit (Modalität)", ReviewInputType.Dropdown,
                ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"], Required: false,
                Help: "Wie verbindlich ist der Claim? Formt die Wortwahl der Folge-Artefakte; hart (MUSS/DARF NICHT) vs. weich wird geprüft — weiche Facette + harte Artefakt-Sprache löst eine Warnung aus.",
                Options: ModalityOptions,
                VisibleWhen: onlyRepair),
            new(FieldRepairTimeScope, "Zeitbezug (MVP?)", ReviewInputType.Dropdown,
                ["mvp", "later_possible", "mvp_or_later_unclear"], Required: false,
                Help: "Gehört der Claim in den MVP oder später? Weiche Werte (später/unklar) verbieten dem Artefakt MVP-Behauptungen (sonst Warnung); kein automatischer Backlog-Schnitt.",
                Options: TimeScopeOptions,
                VisibleWhen: onlyRepair),
            new(FieldRepairScope, "Geltungsbereich", ReviewInputType.FreeText, [], Required: false,
                Help: "FÜR WEN/WO gilt der Claim (z. B. Pflegekräfte, gesamte Einrichtung). Leer lassen = Geltungsbereich unverändert.",
                VisibleWhen: onlyRepair),

            new(FieldReason, "Begründung (nur Audit)", ReviewInputType.FreeText, [], Required: false,
                Help: "PFLICHT beim Verwerfen (warum kommt das Item nicht in die Claim-Menge?) — sonst optional. "
                      + "Nur Audit: landet im adjudicated-ledger.json, hat keinen automatischen Konsumenten."),
            new(FieldTarget, "Referenz-Ziel", ReviewInputType.FreeText, [], Required: false,
                Help: "bei Verknüpfen (anhängen / gehört-zu / abgedeckt): Claim-ID tippen oder aus der Liste wählen (bei unit_signal mit Systemvorschlag vorbelegt)",
                Options: referenceOptions,
                VisibleWhen: onlyReference),
        ];
    }

    /// <param name="referenceOptions">Katalog vorhandener Claims (Value=id, Label="id — proposition") für das
    /// Referenz-Ziel-Autocomplete. null = einfaches Freitextfeld (wenn kein validated Ledger geladen).</param>
    public static ReviewSession BuildSession(AdjudicationQueue queue, IReadOnlyList<ReviewOption>? referenceOptions = null,
        IReadOnlyDictionary<string, string>? candidateToCanonical = null)
    {
        var items = queue.Items.Select(a => BuildItem(a, candidateToCanonical)).ToList();
        var sub = $"validated={queue.SourceValidatedRunId ?? "?"}"
                  + (queue.SourceUnitRunId is { } u ? $" · units={u}" : "")
                  + $" · {items.Count} Items";
        return new ReviewSession
        {
            SessionId = queue.SourceValidatedRunId ?? "adjudication",
            Title = "Ledger-Adjudikation",
            Subtitle = sub,
            Help = BuildHelp(),
            Glossary = Glossary(),
            // Sammel-Aktion: je offenem Item den EIGENEN System-Vorschlag übernehmen (RR: Korrektur · US: Verdikt-Aktion);
            // Items ohne Vorschlag (needs_human) bleiben offen; bereits Entschiedenes wird nicht überschrieben.
            BulkAction = new ReviewBulkAction(
                "✓ Alle Vorschläge übernehmen (Experiment — ohne Einzelprüfung)",
                [],
                "Für {n} offene Items den jeweiligen System-Vorschlag OHNE Einzelprüfung übernehmen? (needs_human bleibt offen; bereits Entschiedenes bleibt unberührt.)",
                ApplyItemQuickActions: true),
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
                "• Claim zur Prüfung (RR::…) — ein extrahierter Claim, dessen Facetten-Validierung nicht sicher war "
                + "(teilweise belegt / überzogen / nicht belegt). Der Validator-Befund steht im System-Vorschlag.\n"
                + "• Transkript-Signal (US::…) — eine Transkript-Aussage, die der Ledger NICHT abdeckt oder die das "
                + "System nicht sicher zuordnen konnte bzw. als Zusatz-Evidenz vorschlägt."),
            new ReviewHelpSection("Aktion — was WIRKT vs. was nur Audit ist",
                "WIRKT (ändert die Evidenzschicht / consumable):\n"
                + "• Unverändert übernehmen — Claim/Aussage kommt as-is rein (bei US: neuer Claim, Facetten danach automatisch via Refine).\n"
                + "• Korrektur übernehmen (nur RR) — Claim kommt MIT deiner Facetten-Korrektur (Felder unten) rein.\n"
                + "• Als neuen Claim übernehmen (nur US) — die Aussage wird ein eigener neuer Claim.\n"
                + "• Als Beleg an bestehenden Claim anhängen (nur US, braucht Referenz-Ziel) — das Zitat stärkt einen bestehenden Claim.\n"
                + "\n"
                + "Nur AUDIT (kein consumable-Effekt, nur Vermerk im Protokoll):\n"
                + "• Gehört zu / abgedeckt durch bestehenden (braucht Referenz-Ziel) — redundant mit einem bestehenden Claim.\n"
                + "• Verwerfen (Begründung Pflicht) — kommt nicht in den consumable.\n"
                + "• Offen lassen — bleibt unentschieden protokolliert; blockiert das Gate NICHT.\n"
                + "\n"
                + "Je Item werden nur die für seinen Typ (RR/US) gültigen Aktionen angeboten. Der Ein-Klick-Button "
                + "übernimmt den System-Vorschlag (Felder vorbelegt)."),
            new ReviewHelpSection("Korrektur-Feld: Entscheidungsstand (nur bei Korrektur übernehmen)",
                "• entschieden — im Gespräch entschieden\n"
                + "• offen — bewusst offengelassen\n"
                + "• verworfen — im Gespräch verworfen\n"
                + "• unklar / widersprüchlich\n"
                + "• extern vorgeschrieben — verpflichtend durch Gesetz/Auflage\n"
                + "\n"
                + "Achtung: extern vorgeschrieben verlangt die Verbindlichkeit MUSS oder DARF NICHT — sonst schlägt das Ledger-Gate fehl."),
            new ReviewHelpSection("Korrektur-Feld: Verbindlichkeit (nur bei Korrektur übernehmen)",
                "• MUSS — harte Pflicht\n"
                + "• DARF NICHT — Verbot\n"
                + "• muss erst GEKLÄRT / ABGEWOGEN / FESTGEHALTEN werden — weiche Pflicht-Stufen\n"
                + "• gewünscht — nicht verpflichtend\n"
                + "• optional — nice-to-have\n"
                + "\n"
                + "Wirkung (belegt): steht dem Baseline-Maker als Claim-Kontext im Prompt UND der Contract-Checker (C3) "
                + "verbietet dem Artefakt harte Formulierungen ('muss/entschieden'), wenn die Facette WEICH ist. "
                + "Entscheidend ist vor allem die Seite hart↔weich; Feinunterschiede innerhalb 'weich' sind derzeit Doku/Audit."),
            new ReviewHelpSection("Korrektur-Feld: Zeitbezug (nur bei Korrektur übernehmen)",
                "• MVP — gehört in den MVP\n"
                + "• später möglich / geplant\n"
                + "• unklar — Zuordnung offen\n"
                + "\n"
                + "Wirkung (belegt): weiche Zeitwerte verbieten dem Artefakt MVP-Behauptungen (Contract-Checker C3). "
                + "Einen direkten deterministischen Backlog-Schnitt-Konsumenten gibt es derzeit NICHT."),
            new ReviewHelpSection("Korrektur-Feld: Geltungsbereich (nur bei Korrektur übernehmen)",
                "Freitext: FÜR WEN/WO gilt der Claim (z. B. 'Pflegekräfte', 'gesamte Einrichtung'). "
                + "Leer lassen = bleibt unverändert."),
            new ReviewHelpSection("Referenz-Ziel",
                "Pflicht bei Beleg-anhängen / Gehört-zu: die ID des existierenden Ziel-Claims "
                + "(Autocomplete: 'id — Proposition'). Ohne gültiges Ziel gilt die Zeile als nicht entschieden."),
            new ReviewHelpSection("Vorbelegung & System-Vorschlag",
                "Korrektur-Felder sind mit dem Validator-Vorschlag vorbelegt — du kannst jeden Wert übersteuern. "
                + "Der System-Vorschlag ist nie bindend: DU entscheidest."),
            new ReviewHelpSection("Woher kommen die Items & Vorschläge?",
                "• Claims zur Prüfung + Korrektur-Vorschläge: aus der Facetten-Validierung (step-03) — ein Prüf-LLM bewertet "
                + "jeden Claim gegen das Transkript (belegt / teilweise belegt / überzogen / nicht belegt).\n"
                + "• Transkript-Signale: aus der Unused-Pipeline (step-01c/01d) — nicht verwendete Transkript-Aussagen, "
                + "die ein Vergleichs-LLM als fehlend/unklar/anhängbar einstuft.\n"
                + "\n"
                + "Vorschläge sind IMMER Modell-Urteile — die Mechanik (Gates/Traces) garantiert nur, dass nichts "
                + "unbilanziert verloren geht."),
            new ReviewHelpSection("Was die Adjudikation NICHT ändert (Grenzen)",
                "• Gehört-zu / abgedeckt ist reiner Audit-Eintrag — verändert den consumable NICHT.\n"
                + "• Die Adjudikation bestimmt, WAS Fakt ist — nicht die spätere FORMULIERUNG (das prüfen eigene Gates).\n"
                + "• Drei Wege laufen bewusst an dir vorbei: voll belegte Claims (direkt freigegeben), als Rauschen "
                + "aussortierte Aussagen, bereits-Abgedecktes mit gültiger Referenz — alle in den Run-Artefakten auditierbar."),
            new ReviewHelpSection("Alle Vorschläge übernehmen (Experiment)",
                "Der Sammel-Button übernimmt je offenem Item dessen EIGENEN System-Vorschlag — bewusst OHNE Einzelprüfung. "
                + "Deklarierter Experiment-Modus (wie accept-all/replay), NICHT der Normalweg. Items ohne Vorschlag "
                + "(Unklar — bitte selbst entscheiden) bleiben offen; bereits Entschiedenes bleibt unberührt."),
            new ReviewHelpSection("Was nach 'Fertig' passiert",
                "• adjudicated-ledger.json — Audit ALLER Entscheidungen\n"
                + "• consumable.json — finale Claim-Menge (freigegeben + deine Annahmen − Verworfenes)\n"
                + "• gate.json — hartes Vollständigkeits-Gate (unentschiedene Zeilen / fehlende Referenz-Ziele = Fehler)\n"
                + "\n"
                + "Bei NEUEN Claims vergibt der automatische Refine-Schritt die fehlenden Facetten. "
                + "Der consumable ist danach der Input für 02-baselines."),
        ]);

    private static ReviewItem BuildItem(AdjudicationItem a, IReadOnlyDictionary<string, string>? candidateToCanonical)
    {
        // System-Vorschlag + Grund als hervorgehobene Notes (nicht mehr als Fließtext in der Summary).
        var notes = new List<ReviewNote>();
        var suggestions = EffectiveSuggestions(a).ToList();
        var hasFacetRepairs = suggestions.Any(s => string.Equals(s.Kind, "facet_repair", StringComparison.OrdinalIgnoreCase));
        if (suggestions.Count > 0)
        {
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
            Badge = BadgeLabel(a.ItemType),
            Summary = a.Proposition,
            Notes = notes,
            ContextBlocks = ctx,
            // E0.4: nur die für DIESEN Item-Typ gültigen Aktionen anbieten (RR vs US) — verhindert typ-fremde Picks.
            FieldOptions = new Dictionary<string, IReadOnlyList<ReviewOption>>(StringComparer.Ordinal)
            {
                [FieldAction] = IsReviewRequired(a) ? RrActionOptions : UsActionOptions,
            },
            // E0.4: Ein-Klick „Vorschlag übernehmen" — RR: Facetten-Korrektur (apply_repair) · US: die zum Verdikt passende
            // Aktion. Repair-Felder bzw. Referenz-Ziel sind vorbelegt; VisibleWhen klappt sie nach dem Klick auf.
            QuickActions = BuildQuickActions(a, hasFacetRepairs),
            FieldValues =
            [
                new(FieldAction, a.Action),
                ..RepairFieldValues(a),
                new(FieldReason, a.ActionReason),
                // E0.4: Referenz-Ziel bei attach/covered-Verdikten VORBELEGEN — der vorgeschlagene Candidate wird auf den
                // kanonischen Claim gemappt (Candidate-IDs stehen nach Canonicalization nicht 1:1 im validated Ledger).
                new(FieldTarget, SuggestedTarget(a, candidateToCanonical)),
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
        // E0.9-P2a: Verwerfen braucht ein begründetes Warum (Audit-Symmetrie); defer bleibt bewusst frei
        // (Vertagen ist kein Widerspruch, Zwangs-Begründung dort wäre Tippzwang ohne Audit-Wert).
        if (string.Equals(action, AdjudicationActions.Reject, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(FieldOf(it, FieldReason))) return false;
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

    /// <summary>Vorbelegtes Referenz-Ziel: bereits gewählt → das; sonst bei attach/covered-Verdikt der vorgeschlagene
    /// Candidate (relatedCandidateIds[0] = a.ClaimId), auf den kanonischen Claim gemappt (matcht den Referenz-Katalog).</summary>
    private static string? SuggestedTarget(AdjudicationItem a, IReadOnlyDictionary<string, string>? candidateToCanonical)
    {
        if (!string.IsNullOrWhiteSpace(a.ReferenceTarget)) return a.ReferenceTarget;   // Mensch hat schon gewählt
        if (!VerdictWantsTarget(Verdict(a)) || a.ClaimId is not { } cand) return null;
        return candidateToCanonical is not null && candidateToCanonical.TryGetValue(cand, out var canonical) ? canonical : cand;
    }

    /// <summary>Das compare_classification-Verdikt eines US-Items (missing_claim/attach_as_evidence/…), sonst null.</summary>
    private static string? Verdict(AdjudicationItem a) => EffectiveSuggestions(a)
        .FirstOrDefault(s => string.Equals(s.Kind, "compare_classification", StringComparison.OrdinalIgnoreCase))?.Classification;

    /// <summary>Die zum US-Verdikt passende Aktion für den „Vorschlag übernehmen"-Button (needs_human = keine).</summary>
    private static string? VerdictAction(string? verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "missing_claim" => AdjudicationActions.PromoteToClaim,
        "attach_as_evidence" => AdjudicationActions.AttachEvidence,
        "already_covered_indirectly" => AdjudicationActions.MergeExisting,
        _ => null,
    };

    /// <summary>Ein-Klick-Buttons: RR → „Korrektur übernehmen" (apply_repair); US → „Vorschlag übernehmen" (Verdikt-Aktion).</summary>
    private static IReadOnlyList<ReviewQuickAction> BuildQuickActions(AdjudicationItem a, bool hasFacetRepairs)
    {
        if (hasFacetRepairs)
            return [new ReviewQuickAction("✓ Korrektur übernehmen", FieldAction, AdjudicationActions.ApplyRepair)];
        return VerdictAction(Verdict(a)) is { } action
            ? [new ReviewQuickAction("✓ Vorschlag übernehmen", FieldAction, action)]
            : [];
    }

    private static string FormatSuggestion(AdjudicationSuggestion s)
    {
        // E0.4: Klartext statt roher Enum-Werte — dieselben Labels wie die Dropdowns (z. B. „Verbindlichkeit: MUSS … → muss ABGEWOGEN …").
        if (s.Facet is not null)
            return $"{FacetTitle(s.Facet)}: {FacetValueLabel(s.Facet, s.Observed)} → {FacetValueLabel(s.Facet, s.Suggested)}";
        return s.Classification is { } v ? VerdictLabels.GetValueOrDefault(v, v) : "(ohne Detail)";
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
