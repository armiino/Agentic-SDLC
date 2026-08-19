using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.ArchClassify;

public sealed record ArchClassifyItemDecision(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName("rationale")] string Rationale,
    // ① (06.08.): die vom Menschen bestätigten/korrigierten Ziel-PBIs (constraint-Wirkung). Optional: Alt-Dateien lesbar.
    [property: JsonPropertyName("targetPbiIds")] IReadOnlyList<string>? TargetPbiIds = null);

public sealed record ArchClassifyDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("decisions")] IReadOnlyList<ArchClassifyItemDecision> Decisions);

/// <summary>
/// R-11 U2v2 (06.08., Autor-Abnahme des Endbilds) — die Review-UI der Rollen-Klassifikation nach dem
/// Adjudikations-Vorbild: DREI ja/nein-Dropdowns mit KURZEN Labels (Wirkung im Glossar/Options/Wirkungs-
/// Banner, nicht in die Labels gestopft) + <see cref="ReviewInputType.ReferenceList"/>-Ziel-Feld: die
/// Ziel-PBIs sind CHIPS in der Card (Klick = PBI-Details rechts, × = entfernen), die rechte Liste zeigt
/// ALLE aktiven PBIs (Suche, Detail-Panel mit AKs/REQs/ARCHs, „Als Ziel hinzufügen"). Felder sind mit dem
/// AGENT-VORSCHLAG VORBELEGT (Korrektur-Modus, dokumentierte Ausnahme von der Leere-Defaults-Regel).
/// Alle Rollen auf „nein" = VERTAGEN (idempotenter Scan legt erneut vor).
/// </summary>
public static class ArchClassifyReviewAdapter
{
    public const string FieldConstraint = "rolle_constraint";
    public const string FieldWork = "rolle_work";
    public const string FieldDesign = "rolle_design";
    public const string FieldRationale = "begruendung";
    public const string FieldTargets = "ziel_pbis";
    // 1f-① (19.08., Block-E-Nebenfund „Durchwink-Falle"): die Rollen-Vorbelegung (Korrektur-Modus) machte
    // jedes Item SOFORT resolved — „Fertig" ging ohne einen einzigen Klick. E0-Endform nach decision-gate-
    // Präzedenz: ÜBERNEHMEN ist ein expliziter Akt (leeres Pflichtfeld) + deklarierter Bulk mit Bestätigung.
    public const string FieldUebernehmen = "uebernehmen";
    public const string Ja = "ja";
    public const string Nein = "nein";

    public static ReviewSession BuildSession(string runId, ArchClassifyReviewRequest request)
    {
        return new ReviewSession
        {
            SessionId = $"arch-classify-{runId}",
            Title = "Architektur — Konsum-Rollen bestätigen/korrigieren",
            Subtitle = $"{request.Items.Count} Architektur-Item(s). Der Agent hat Rollen + Ziel-PBIs vorgeschlagen (vorbelegt) — du korrigierst je Rolle und ÜBERNIMMST je Item (oder alle per Sammel-Knopf). Nicht Übernommenes = vertagt (kommt wieder); alle Rollen 'nein' = ebenfalls vertagt.",
            // 1f-①: der EINZIG mögliche Bulk — Rollen variieren je Item (kommen aus der Vorbelegung), der
            // Bulk bestätigt sie nur. E0-Muster: deklariert + bestätigungspflichtig (wie decision-gate).
            BulkAction = new ReviewBulkAction(
                "✓ Alle wie vorgeschlagen übernehmen",
                [new ReviewFieldValue(FieldUebernehmen, Ja)],
                "{n} Items mit den VORGESCHLAGENEN (bzw. von dir korrigierten) Rollen übernehmen? Der Apply schreibt die Rollen in die Wahrheit; bereits übernommene Items bleiben unberührt."),
            // U2v2: der Wirkungs-Banner (E0-Muster) trägt die Wirkung — eine Zeile je Rolle (Autor 06.08.);
            // die Rollen-Begriffe bekommen darin (und auf den Feld-Labels) den Glossar-Tooltip.
            Notes =
            [
                new ReviewNote(ReviewNoteKind.Info, "Wirkung deiner Entscheide",
                    "constraint → der Rahmen erscheint als „Technische Rahmenbedingung“ in den Issues der Ziel-PBIs (Relation constrained_by)\n" +
                    "work → es wird ein eigenes technisches PBI mit Issue vorgeschlagen (A4)\n" +
                    "design → es wird ein ADR-Dokument erzeugt (A5)")
            ],
            Glossary =
            [
                new ReviewGlossaryEntry("constraint", "Der Fakt schränkt andere Arbeit ein. WIRKT: erscheint als „Technische Rahmenbedingung“ in den GitHub-Issues der betroffenen PBIs (A3).", "Rollen"),
                new ReviewGlossaryEntry("work", "Aus dem Fakt folgt echte technische Arbeit. WIRKT: es wird ein eigenes technisches PBI mit GitHub-Issue vorgeschlagen (A4).", "Rollen"),
                new ReviewGlossaryEntry("design", "Eine begründbare Architektur-Entscheidung. WIRKT: es wird ein ADR-Dokument erzeugt (A5).", "Rollen"),
                new ReviewGlossaryEntry("Rollen kombinieren", "Ein Item darf mehrere Rollen tragen — eine Technologie-Festlegung ist z. B. oft design UND constraint zugleich.", "Begriffe"),
                new ReviewGlossaryEntry("vertagen", "Alle drei Rollen auf 'nein': das Item bleibt unklassifiziert und wird beim nächsten arch-aktiven Lauf erneut vorgelegt. WIRKT: nichts — kein Konsum ohne Rollen.", "Begriffe")
            ],
            FieldSchema =
            [
                // U2v2: kurze Labels — die Wirkung steht im Glossar (Tooltip), in den Options und im Banner.
                new ReviewFieldSpec(FieldConstraint, "constraint?", ReviewInputType.Dropdown,
                    [Ja, Nein], Required: true,
                    Options: [new(Ja, "ja — schränkt andere Arbeit ein"), new(Nein, "nein")]),
                new ReviewFieldSpec(FieldWork, "work?", ReviewInputType.Dropdown,
                    [Ja, Nein], Required: true,
                    Options: [new(Ja, "ja — daraus folgt ein technisches PBI"), new(Nein, "nein")]),
                new ReviewFieldSpec(FieldDesign, "design?", ReviewInputType.Dropdown,
                    [Ja, Nein], Required: true,
                    Options: [new(Ja, "ja — begründbare Architektur-Entscheidung"), new(Nein, "nein")]),
                // ①/U2v2: das Ziel-Feld des gebündelten Wirkungs-Gates als CHIPS (nur sichtbar bei constraint=ja,
                // E0.9). Die Options speisen zugleich die rechte PBI-Liste (Suche + Details + hinzufügen/entfernen).
                new ReviewFieldSpec(FieldTargets, "Ziel-PBIs", ReviewInputType.ReferenceList, [], Required: false,
                    Help: "rechts aus der PBI-Liste hinzufügen",
                    Options: (request.ActivePbis ?? []).Select(p => new ReviewOption(p.Id, $"{p.Id} — {p.Title}")).ToList(),
                    VisibleWhen: new ReviewFieldVisibility(FieldConstraint, [Ja]),
                    CatalogTitle: "Aktive PBIs"),
                new ReviewFieldSpec(FieldRationale, "Begründung (Audit; vorbelegt vom Agenten)", ReviewInputType.MultiLine, [], Required: false),
                // 1f-①: der Entscheid selbst — startet LEER (E0: keine Durchwink-Falle); die Rollen oben
                // sind nur der korrigierbare Vorschlag.
                new ReviewFieldSpec(FieldUebernehmen, "übernehmen?", ReviewInputType.Dropdown, [Ja], Required: true,
                    Help: "Der bewusste Akt: Rollen (wie vorgeschlagen oder korrigiert) in die Wahrheit übernehmen. Leer lassen = vertagen (kommt wieder).",
                    Options: [new(Ja, "✓ übernehmen — Rollen werden Wahrheit (Apply)")])
            ],
            Items = request.Items.Select(BuildItem).ToList()
        };
    }

    private static ReviewItem BuildItem(ArchClassifyItemView v)
    {
        // Autor-Feedback 06.08. (an den anderen UIs orientiert): System-Vorschlag + Begründung als
        // sichtbare Notiz-Cards (Muster IngestionReviewAdapter „Wirkung"/„Warum vorgeschlagen").
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Suggestion, "System-Vorschlag",
                $"Rollen: {string.Join(" + ", v.ProposedRoles)} — die Dropdowns unten sind damit vorbelegt; du kannst jede Rolle umentscheiden."),
            new(ReviewNoteKind.Reason, "Warum vorgeschlagen",
                string.IsNullOrWhiteSpace(v.Rationale) ? "(keine Begründung)" : v.Rationale)
        };
        // U2v2: bei constraint zeigen die CHIPS die betroffenen PBIs (mit Titel, inspizierbar) — keine Extra-
        // Note. Nur bei work-OHNE-constraint ist das Ziel-Feld unsichtbar: dann als reine Anzeige-Note.
        if (Targets(v).Count > 0 && !v.ProposedRoles.Contains(ArchRoles.Constraint, StringComparer.Ordinal))
            notes.Add(new(ReviewNoteKind.Info, "Gehört fachlich zu (Anzeige, keine Wirkung)",
                string.Join(Environment.NewLine, Targets(v).Select(t => $"{t.Id} — {(string.IsNullOrWhiteSpace(t.Title) ? "(unbekannt/inaktiv)" : t.Title)}"))));

        var item = new ReviewItem
        {
            ItemId = v.ItemId,
            Summary = v.Text,
            Badge = string.Join("+", v.ProposedRoles),
            Notes = notes,
            FieldValues =
            [
                new ReviewFieldValue(FieldConstraint, v.ProposedRoles.Contains(ArchRoles.Constraint, StringComparer.Ordinal) ? Ja : Nein),
                new ReviewFieldValue(FieldWork, v.ProposedRoles.Contains(ArchRoles.Work, StringComparer.Ordinal) ? Ja : Nein),
                new ReviewFieldValue(FieldDesign, v.ProposedRoles.Contains(ArchRoles.Design, StringComparer.Ordinal) ? Ja : Nein),
                new ReviewFieldValue(FieldTargets, string.Join(Environment.NewLine, Targets(v).Select(t => t.Id))),
                new ReviewFieldValue(FieldRationale, v.Rationale),
                new ReviewFieldValue(FieldUebernehmen, "")   // 1f-①: der Entscheid startet LEER
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static IReadOnlyList<ArchPbiOption> Targets(ArchClassifyItemView v) => v.ProposedTargets ?? [];

    /// <summary>Zeilen/Kommata → PBI-ID-Liste (getrimmt, dedupliziert; leere Eingabe = keine Ziele).</summary>
    public static IReadOnlyList<string> ParseTargets(string? raw)
        => string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Distinct(StringComparer.Ordinal).ToList();

    /// <summary>1f-①: aufgelöst = Rollen beantwortet UND explizit übernommen — die Vorbelegung allein
    /// entscheidet NICHTS mehr (E0: kein Durchwinken; Übernehmen ist der eine bewusste Akt je Item).</summary>
    public static bool Resolved(ReviewItem item)
        => Field(item, FieldConstraint) is Ja or Nein
        && Field(item, FieldWork) is Ja or Nein
        && Field(item, FieldDesign) is Ja or Nein
        && Field(item, FieldUebernehmen) == Ja;

    public static ArchClassifyDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = new List<ArchClassifyItemDecision>();
        foreach (var item in session.Items)
        {
            if (Field(item, FieldUebernehmen) != Ja) continue;            // 1f-①: unbestätigt = vertagt
            var roles = new List<string>();
            if (Field(item, FieldConstraint) == Ja) roles.Add(ArchRoles.Constraint);
            if (Field(item, FieldWork) == Ja) roles.Add(ArchRoles.Work);
            if (Field(item, FieldDesign) == Ja) roles.Add(ArchRoles.Design);
            if (roles.Count == 0) continue;                               // alle nein = vertagt (Scan legt erneut vor)
            decisions.Add(new(item.ItemId, roles, Field(item, FieldRationale) ?? "",
                ParseTargets(Field(item, FieldTargets))));
        }
        return new ArchClassifyDecisionsFile(runId, decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, ArchClassifyDecisionsFile? file)
    {
        if (file is null) return;
        var byId = file.Decisions.ToDictionary(d => d.ItemId, StringComparer.Ordinal);
        foreach (var item in session.Items)
        {
            if (!byId.TryGetValue(item.ItemId, out var d)) continue;
            Set(item, FieldConstraint, d.Roles.Contains(ArchRoles.Constraint, StringComparer.Ordinal) ? Ja : Nein);
            Set(item, FieldWork, d.Roles.Contains(ArchRoles.Work, StringComparer.Ordinal) ? Ja : Nein);
            Set(item, FieldDesign, d.Roles.Contains(ArchRoles.Design, StringComparer.Ordinal) ? Ja : Nein);
            if (!string.IsNullOrWhiteSpace(d.Rationale)) Set(item, FieldRationale, d.Rationale);
            // ① Targets: null = Alt-Datei ohne Ziel-Wissen -> Vorbelegung NICHT löschen; eine (auch leere)
            // Liste ist ein echter Human-Stand (Apply schreibt immer eine Liste) und gewinnt.
            if (d.TargetPbiIds is not null) Set(item, FieldTargets, string.Join(Environment.NewLine, d.TargetPbiIds));
            // 1f-①: ein Eintrag in der Entscheid-Datei IST eine frühere Übernahme (Apply schreibt nur Bestätigtes).
            Set(item, FieldUebernehmen, Ja);
            item.Resolved = Resolved(item);
        }
    }

    /// <summary>Die Response fürs Gate aus der Entscheid-Datei (weggelassene Items = vertagt).</summary>
    public static ArchClassifyReviewResponse ToResponse(ArchClassifyDecisionsFile file, string reviewer)
        => new(file.Decisions.Select(d => new ArchClassifyProposal(d.ItemId, d.Roles, d.Rationale, d.TargetPbiIds)).ToList(), reviewer);

    /// <summary>U2v2: das PBI-Detail fürs rechte Referenz-Panel — Titel, Ziel/Goal, Feature-Zugehörigkeit,
    /// und lazy Kontext-Blöcke (AKs · zugehörige REQs · bestehende Rahmen). Alles aus dem Core ableitbar.</summary>
    public static ReviewReferenceDetails? BuildPbiReference(ProjectStateDocument core, string pbiId)
    {
        var pbi = core.Items.FirstOrDefault(i => i.ItemId == pbiId
            && string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase));
        if (pbi is null) return null;

        var featureById = core.Items
            .Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(i => i.ItemId, i => i.Feature?.Label ?? i.Text, StringComparer.Ordinal);
        var feature = core.Relations
            .Where(r => r.RelationType == "part_of_feature" && r.FromId == pbiId)
            .Select(r => featureById.TryGetValue(r.ToId, out var l) ? l : r.ToId)
            .FirstOrDefault();

        var notes = new List<ReviewNote> { new(ReviewNoteKind.Info, "Feature", feature ?? "(ohne Feature)") };
        if (pbi.ReadStatus().Validity != Validity.Active)
            notes.Add(new(ReviewNoteKind.Warning, "Status", "Dieses PBI ist NICHT aktiv — als Ziel wird es beim Apply verworfen."));

        return new ReviewReferenceDetails(
            pbiId,
            $"{pbiId} — {pbi.Pbi?.Title ?? pbi.Text}",
            pbi.Pbi?.Goal ?? pbi.Text,
            notes,
            [
                new ContextBlock(ContextBlockKind.Excerpt, "Akzeptanzkriterien", "aks"),
                new ContextBlock(ContextBlockKind.Reference, "Zugehörige REQs", "reqs"),
                new ContextBlock(ContextBlockKind.Reference, "Bestehende Rahmen (ARCHs)", "archs")
            ]);
    }

    /// <summary>U2v2: die lazy Kontext-Blöcke des PBI-Details (aks | reqs | archs).</summary>
    public static string ResolvePbiReferenceContext(ProjectStateDocument core, string pbiId, string key)
    {
        var pbi = core.Items.FirstOrDefault(i => i.ItemId == pbiId);
        if (pbi is null) return $"(PBI '{pbiId}' nicht im Core)";
        var textById = core.Items.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
        switch (key)
        {
            case "aks":
                var aks = pbi.Pbi?.AcceptanceCriteria ?? [];
                return aks.Count == 0 ? "(keine Akzeptanzkriterien hinterlegt)"
                    : string.Join(Environment.NewLine, aks.Select(a => $"• {a}"));
            case "reqs":
                var reqs = core.Relations
                    .Where(r => r.RelationType == "covers" && r.FromId == pbiId)
                    .Select(r => $"• {r.ToId} — {(textById.TryGetValue(r.ToId, out var t) ? t : "(unbekannt)")}")
                    .ToList();
                return reqs.Count == 0 ? "(keine covers-Relationen — REQ-Zuordnung entsteht am pbi-Gate)"
                    : string.Join(Environment.NewLine, reqs);
            case "archs":
                var archs = core.Relations
                    .Where(r => r.RelationType == "constrained_by" && r.FromId == pbiId)
                    .Select(r => $"• {r.ToId} — {(textById.TryGetValue(r.ToId, out var t) ? t : "(unbekannt)")}")
                    .ToList();
                return archs.Count == 0 ? "(noch keine Rahmenbedingungen an diesem PBI)"
                    : string.Join(Environment.NewLine, archs);
            default:
                return $"(Unbekannter Kontext: {key})";
        }
    }

    private static string? Field(ReviewItem item, string key)
        => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;

    private static void Set(ReviewItem item, string key, string value)
    {
        var existing = item.FieldValues.FirstOrDefault(f => f.FieldKey == key);
        if (existing is not null) item.FieldValues.Remove(existing);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }
}
