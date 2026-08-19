using AgenticSdlc.HumanReview;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Adr;

public sealed record AdrItemDecision(
    [property: JsonPropertyName("draft")] AdrDraft Draft);

public sealed record AdrDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("decisions")] IReadOnlyList<AdrItemDecision> Decisions);

/// <summary>
/// R-11 A5/U5 (06.08.) — die ADR-Abnahme-UI (Adjudikations-/U2v2-Vorbild): Felder VORBELEGT mit dem
/// Agent-Entwurf (Korrektur-Modus, dokumentierte Ausnahme wie U2 — ein begründeter Vorschlag wird
/// korrigiert, nicht aus dem Nichts entschieden), Related als <see cref="ReviewInputType.ReferenceList"/>
/// (rechte Liste = Wahrheits-Katalog, Details/Chips wie U2v2), lazy Datei-VORSCHAU je Entwurf.
/// freigeben=nein = VERTAGEN (Scan legt erneut vor). Die Gate-Abnahme IST die Entscheidung → accepted.
/// </summary>
public static class AdrReviewAdapter
{
    public const string FieldFreigeben = "freigeben";
    public const string FieldTitel = "titel";
    public const string FieldKontext = "kontext";
    public const string FieldEntscheidung = "entscheidung";
    public const string FieldKonsequenzen = "konsequenzen";
    public const string FieldAlternativen = "alternativen";
    public const string FieldRelated = "verwandte";
    public const string Ja = "ja";
    public const string Nein = "nein";

    public static ReviewSession BuildSession(string runId, AdrReviewRequest request)
        => new()
        {
            SessionId = $"adr-{runId}",
            Title = "ADR-Abnahme — Architektur-Entscheidungen dokumentieren",
            Subtitle = $"{request.Items.Count} Entwurf/Entwürfe. Der Agent hat formuliert (vorbelegt) — du editierst oder nimmst an. "
                + "'freigeben=nein' = vertagen (kommt wieder). Fertig ist JEDERZEIT erlaubt — nicht entschiedene Entwürfe gelten als vertagt (Teil-Abnahme).",
            // Teil-Abnahme als DEKLARIERTE Eigenschaft (Server-Erlaubnis + Fertig-Knopf aus EINER Quelle):
            // die Datei-Semantik „weggelassen = vertagt" trägt das sicher; der Scan legt erneut vor.
            AllowPartialFinish = true,
            Notes =
            [
                new ReviewNote(ReviewNoteKind.Info, "Wirkung deiner Entscheide",
                    "freigeben=ja → ADR-Datei in docs/adr/ (Status accepted — deine Abnahme IST die Entscheidung) + Nummer im Core-Item\n" +
                    "freigeben=nein → vertagt, das design-Item wird beim nächsten arch-aktiven Lauf erneut vorgelegt\n" +
                    "Index (docs/adr/README.md) und Architektur-Übersicht (docs/architecture.md) werden automatisch regeneriert")
            ],
            // Sammel-Aktion wie in den anderen UIs (bewusster Ein-Klick-Akt mit Confirm; setzt NUR offene Items).
            BulkAction = new ReviewBulkAction(
                Label: "✓ Alle freigeben (wie vorgeschlagen)",
                Set: [new ReviewFieldValue(FieldFreigeben, Ja)],
                Confirm: "{n} offene Entwürfe freigeben (Status accepted, Dateien werden beim Apply geschrieben)?"),
            Glossary =
            [
                new ReviewGlossaryEntry("ADR", "Architecture Decision Record — EINE Markdown-Datei je Entscheidung (Kontext · Entscheidung · Konsequenzen). Projektion aus dem Core, nie von Hand editiert.", "Begriffe"),
                new ReviewGlossaryEntry("superseded", "Wird die Entscheidung später abgelöst (②-Bahn), bekommt das alte ADR automatisch 'superseded by ADR-NNNN' — editiert wird nie.", "Begriffe")
            ],
            FieldSchema =
            [
                // U5-Feinschliff (Autor 06.08.): jedes Feld erklaert sich beim Hover (Help = title-Attribut).
                new ReviewFieldSpec(FieldFreigeben, "freigeben?", ReviewInputType.Dropdown, [Ja, Nein], Required: true,
                    Help: "ja = die ADR-Datei wird mit deiner Abnahme geschrieben (Status accepted). nein = vertagen — das Item kommt beim naechsten arch-aktiven Lauf wieder.",
                    Options: [new(Ja, "ja — ADR-Datei wird geschrieben (accepted)"), new(Nein, "nein — vertagen")]),
                new ReviewFieldSpec(FieldTitel, "Titel", ReviewInputType.FreeText, [], Required: false,
                    Help: "Praegnanter Entscheidungs-Titel (Substantiv-Stil, kein ganzer Satz) — wird Ueberschrift und Dateiname des ADR."),
                new ReviewFieldSpec(FieldKontext, "Kontext", ReviewInputType.MultiLine, [], Required: false,
                    Help: "WARUM stand die Entscheidung an? Ausgangslage, Problem, treibende Anforderungen — aus dem Meeting-Material, nichts erfinden."),
                new ReviewFieldSpec(FieldEntscheidung, "Entscheidung", ReviewInputType.MultiLine, [], Required: false,
                    Help: "WAS wurde entschieden — praezise und aktiv formuliert („Wir nutzen …“). Die getroffene Entscheidung, keine neue."),
                new ReviewFieldSpec(FieldKonsequenzen, "Konsequenzen", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Was folgt aus der Entscheidung — Positives UND Einschraenkendes (z. B. welche Arbeit gebunden wird, was geprueft werden muss)."),
                new ReviewFieldSpec(FieldAlternativen, "Betrachtete Alternativen (optional)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Nur wenn Alternativen im Material erkennbar diskutiert wurden — nie erfinden. Leer = Abschnitt entfaellt in der Datei."),
                new ReviewFieldSpec(FieldRelated, "Verwandte Core-Items", ReviewInputType.ReferenceList, [], Required: false,
                    Help: "Core-Items (Anforderungen/Architektur), die das ADR nennen soll — z. B. die Anforderungs-Sicht derselben Festlegung. Rechts aus dem Katalog hinzufuegen; Klick auf ein Item zeigt Details.",
                    Options: (request.Truth ?? []).Select(t => new ReviewOption(t.Id, $"{t.Id} — {t.Title}")).ToList(),
                    CatalogTitle: "Core-Items")
            ],
            Items = request.Items.Select(BuildItem).ToList()
        };

    private static ReviewItem BuildItem(AdrItemView v)
    {
        var item = new ReviewItem
        {
            ItemId = v.Draft.ItemId,
            Summary = v.ItemText,
            Badge = "design",
            Notes = [new ReviewNote(ReviewNoteKind.Suggestion, "System-Vorschlag",
                $"„{v.Draft.Title}“ — Felder unten sind vorbelegt; du kannst jedes editieren.")],
            // Lazy Datei-Vorschau (U5: „Body-Vorschau wie Forward-CREATE", E0.2-Muster).
            ContextBlocks = [new ContextBlock(ContextBlockKind.Excerpt, "Vorschau der ADR-Datei", $"vorschau:{v.Draft.ItemId}")],
            FieldValues =
            [
                // E0-Leere-Default (06.08., Autor): der ENTSCHEID startet leer — Prosa ist vorbelegt (Korrektur-
                // Modus), die Freigabe ist ein bewusster Akt je Item ODER der Sammel-Knopf unten.
                new ReviewFieldValue(FieldFreigeben, ""),
                new ReviewFieldValue(FieldTitel, v.Draft.Title),
                new ReviewFieldValue(FieldKontext, v.Draft.Context),
                new ReviewFieldValue(FieldEntscheidung, v.Draft.Decision),
                new ReviewFieldValue(FieldKonsequenzen, v.Draft.Consequences),
                new ReviewFieldValue(FieldAlternativen, v.Draft.Alternatives ?? ""),
                new ReviewFieldValue(FieldRelated, string.Join(Environment.NewLine, v.Draft.RelatedIds ?? []))
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    public static bool Resolved(ReviewItem item) => Field(item, FieldFreigeben) is Ja or Nein;

    public static AdrDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, session.Items
            .Where(i => Field(i, FieldFreigeben) == Ja)
            .Select(i => new AdrItemDecision(new AdrDraft(
                i.ItemId,
                Field(i, FieldTitel) ?? "",
                Field(i, FieldKontext) ?? "",
                Field(i, FieldEntscheidung) ?? "",
                Field(i, FieldKonsequenzen) ?? "",
                ArchClassify.ArchClassifyReviewAdapter.ParseTargets(Field(i, FieldRelated)),
                string.IsNullOrWhiteSpace(Field(i, FieldAlternativen)) ? null : Field(i, FieldAlternativen))))
            .ToList());

    public static void MergeExistingDecisions(ReviewSession session, AdrDecisionsFile? file)
    {
        if (file is null) return;
        var byId = file.Decisions.ToDictionary(d => d.Draft.ItemId, StringComparer.Ordinal);
        foreach (var item in session.Items)
        {
            if (!byId.TryGetValue(item.ItemId, out var d)) continue;
            Set(item, FieldFreigeben, Ja);
            Set(item, FieldTitel, d.Draft.Title);
            Set(item, FieldKontext, d.Draft.Context);
            Set(item, FieldEntscheidung, d.Draft.Decision);
            Set(item, FieldKonsequenzen, d.Draft.Consequences);
            Set(item, FieldAlternativen, d.Draft.Alternatives ?? "");
            Set(item, FieldRelated, string.Join(Environment.NewLine, d.Draft.RelatedIds ?? []));
            item.Resolved = Resolved(item);
        }
    }

    /// <summary>Die Response fürs Gate aus der Entscheid-Datei (weggelassene Items = vertagt).</summary>
    public static AdrReviewResponse ToResponse(AdrDecisionsFile file, string reviewer)
        => new(file.Decisions.Select(d => d.Draft).ToList(), reviewer);

    /// <summary>Lazy Vorschau: die gerenderte Datei aus der Anfrage (Finalize hat sie erzeugt).</summary>
    public static string ResolvePreview(string key, AdrReviewRequest request)
        => key.StartsWith("vorschau:", StringComparison.Ordinal)
           && request.Items.FirstOrDefault(i => i.Draft.ItemId == key["vorschau:".Length..]) is { } v
            ? v.Preview
            : $"(Unbekannter Kontext: {key})";

    /// <summary>U5-Feinschliff (Autor 06.08.): Detail eines Wahrheits-Items (req/arch) fuers rechte Panel —
    /// Klick auf den Katalog zeigt Volltext, Art/Status/Rollen und die Wirkungs-Relationen. Aus dem Core, read-only.</summary>
    public static ReviewReferenceDetails? BuildTruthReference(Delta.ProjectStateDocument core, string id)
    {
        var item = core.Items.FirstOrDefault(i => i.ItemId == id
            && (string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
             || string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase)));
        if (item is null) return null;

        var isArch = string.Equals(item.ItemType, "architecture", StringComparison.OrdinalIgnoreCase);
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Art", isArch ? "Architektur" : "Anforderung"),
            new(ReviewNoteKind.Info, "Status", item.Status)
        };
        if (isArch && item.Architecture?.Roles is { Count: > 0 } roles)
            notes.Add(new(ReviewNoteKind.Info, "Rollen", string.Join(" + ", roles)
                + (item.Architecture.AdrId is null ? "" : $" · ADR: {item.Architecture.AdrId}")));

        return new ReviewReferenceDetails(id, $"{id} — {Truncate(item.Text, 90)}", item.Text, notes,
            [new ContextBlock(ContextBlockKind.Reference, "Core-Relationen", "relationen")]);
    }

    /// <summary>Lazy Relationen-Kontext des Wahrheits-Items (deckende PBIs · gebundene PBIs · Supersede-Kette).</summary>
    public static string ResolveTruthReferenceContext(Delta.ProjectStateDocument core, string id, string key)
    {
        if (key != "relationen") return $"(Unbekannter Kontext: {key})";
        // Nur die KONZEPT-Relationen (core-relationen-konzept.md) — Evidenz-Verdrahtung (z. B.
        // evidenced_by_ledger_claim) ist Provenienz-Detail und hilft bei der ADR-Abnahme nicht.
        var shown = new HashSet<string>(StringComparer.Ordinal)
        { "part_of_feature", "covers", "constrained_by", "constrained_by_superseded", "supersedes", "contradicts", "contradicts_resolved", "implemented_by_issue" };
        var titleById = core.Items.ToDictionary(i => i.ItemId, i => i.Pbi?.Title ?? i.Text, StringComparer.Ordinal);
        var lines = core.Relations
            .Where(r => shown.Contains(r.RelationType))
            .Where(r => r.FromId == id || r.ToId == id)
            .Select(r => r.FromId == id
                ? $"- {r.RelationType} → {r.ToId} — {Truncate(titleById.GetValueOrDefault(r.ToId, "?"), 80)}"
                : $"- {r.FromId} — {Truncate(titleById.GetValueOrDefault(r.FromId, "?"), 80)} —{r.RelationType}→ dieses Item")
            .OrderBy(x => x, StringComparer.Ordinal).ToList();
        return lines.Count == 0 ? "(keine Konzept-Relationen an diesem Item)" : string.Join(Environment.NewLine, lines);
    }

    private static string Truncate(string v, int max)
    {
        var t = string.Join(' ', (v ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return t.Length <= max ? t : t[..max] + "…";
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
