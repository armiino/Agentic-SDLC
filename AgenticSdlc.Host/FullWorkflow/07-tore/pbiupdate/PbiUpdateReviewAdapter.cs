using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

public sealed record PbiUpdateDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<PbiUpdateDecision> Decisions,
    // R-26-C (A1, additiv): Entscheidungen zu den Angleichungs-Vorschlägen (accept/edit/skip je PBI). Alte
    // Dateien ohne dieses Feld => null => keine Angleichung (Inhalts-Mutation NUR mit expliziter Freigabe).
    [property: JsonPropertyName("alignmentDecisions")] IReadOnlyList<PbiAlignmentDecision>? AlignmentDecisions = null);

public sealed record PbiUpdateDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason,
    // B1: bei NEW_PBI die (evtl. vom Menschen geänderte) Feature-Zuordnung. null/leer = Vorschlag des Agenten.
    // Alte Dateien ohne dieses Feld => null => keine Korrektur. Der Apply liest es und schreibt die Op um.
    [property: JsonPropertyName("featureId")] string? FeatureId = null);

// R-26-C: Entscheidung des Menschen zum Angleichungs-Vorschlag eines PBI. accept = Vorschlag übernehmen,
// edit = mit den editierten Feldern übernehmen, skip = nicht angleichen (PBI bleibt needs_clarify).
public sealed record PbiAlignmentDecision(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedTitle")] string? EditedTitle,
    [property: JsonPropertyName("editedStatement")] string? EditedStatement,
    [property: JsonPropertyName("editedAcceptanceCriteria")] IReadOnlyList<string>? EditedAcceptanceCriteria,
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die PBI-Operationen in die generische HumanReview-UI: 1 Item je Operation (opId = op-<index>),
// Entscheidung apply/skip. Der Mensch autorisiert die Backlog-Aenderungen (= Wahrheits-Mutation im Core).
//
// E0.3 (27.07., R-20-Fix): der kontext-aermste Screen der Kette wird angereichert — IDs werden zu Titeln/
// Texten aufgeloest, "verfeinert"/SUPERSEDE zeigen Vorher/Nachher aus dem Core (History bzw. Ersatz-Requirement),
// Glossar + Wirkungs-Banner erklaeren die 5 Op-Typen und ihre Core-Wirkung, Default leer, Drilldowns lesbar.
// E0.9-Leitsatz: gezeigt wird, was WIRKT/die Handlung informiert — nicht die deterministische Roh-Rationale.
// Folge-Logik (Entscheid-Format apply/skip, Apply, Gate) unveraendert.
public static class PbiUpdateReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    // R-26-C: die Angleichung wird DIREKT am zugehoerigen MARK_CHANGED/SUPERSEDE-Op-Item gezeigt (gleicher
    // PBI-Kontext, bessere UX) — zwei getrennte Entscheidungen (Struktur + Inhalt) in EINEM Item. Versteckte
    // Traeger-Felder steuern die Sichtbarkeit + tragen die pbiId fuer den Apply.
    public const string FieldHasAlign = "hasAlign";       // "yes" => dieses Op-Item traegt eine Angleichung
    public const string FieldAlignPbiId = "alignPbiId";   // pbiId der Angleichung (fuer den Apply)
    public const string FieldAlignDecision = "alignDecision";
    public const string FieldAlignTitle = "alignTitle";
    public const string FieldAlignStatement = "alignStatement";
    public const string FieldAlignAcceptance = "alignAcceptance";
    // B1 (Fall-C-Bündelung, Lösungsweg B): editierbare Feature-Zuordnung eines NEW_PBI. BEWUSST der Feldschlüssel
    // "referenceTarget" — daran ist die generische Review-UI fest verdrahtet: die rechte Kontext-Leiste zeigt die
    // Optionen dieses Feldes als Katalog (hier die Feature-Landkarte), und ein Klick auf einen Eintrag setzt genau
    // dieses Feld am aktiven Item. So bekommt B1 Landkarte + Klick-zu-Zuweisen OHNE Änderung an der geteilten UI.
    public const string FieldFeature = "referenceTarget";
    public const string FieldKind = "opKind";             // Hidden-Träger: op.Kind für die VisibleWhen-Bedingung
    // B2: Sentinel-Präfix für Optionen, die auf ein im selben Plan VORGESCHLAGENES neues Feature zeigen (existiert
    // noch nicht im Core → kein echter featureId). Wert = "proposed:<Label>". Feature-IDs sind "FC-nn" → kollisionsfrei.
    public const string ProposedPrefix = "proposed:";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip", "to_decision" };   // R-14 D2: dritter Weg
    private static readonly HashSet<string> AlignDecisions = new(StringComparer.OrdinalIgnoreCase) { "accept", "edit", "skip" };
    private static readonly ReviewFieldVisibility OnlyHasAlign = new(FieldHasAlign, ["yes"]);
    private static readonly ReviewFieldVisibility OnlyAlignEdit = new(FieldAlignDecision, ["edit"]);
    private static readonly ReviewFieldVisibility OnlyNewPbi = new(FieldKind, [PbiUpdateKind.NewPbi]);
    private const int TextPreviewChars = 500;

    public static ReviewSession BuildSession(string runId, PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        // R-26-C: jede Angleichung an ihr ERSTES passendes MARK_CHANGED/SUPERSEDE-Op (gleicher PBI) haengen,
        // damit Struktur + Inhaltsangleichung im selben Item stehen. (Angleichungen entstehen nur fuer diese
        // Op-Arten; ein PBI mit mehreren Ops zeigt die Angleichung genau einmal, am ersten Op.)
        var opAlign = MapAlignmentsToOps(plan);
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op, byId, opAlign.GetValueOrDefault(i))).ToList();

        // B1/B2: die bestehenden Features + die im Plan vorgeschlagenen NEUEN Features als Katalog. Speist die rechte
        // Feature-Landkarte (über den referenceTarget-Feldschlüssel) UND das editierbare Feature-Dropdown je NEW_PBI.
        var featureOptions = FeatureOptions(plan, core);

        var alignCount = opAlign.Count;
        return new ReviewSession
        {
            SessionId = $"pbi-update-{runId}",
            Title = "Incrementeller PBI-Update — Backlog-Aenderungen",
            Subtitle = plan.Operations.Count == 0 && alignCount == 0
                ? "Keine PBI-Aenderungen."
                : $"{plan.Operations.Count} Struktur-Operationen"
                  + (alignCount > 0 ? $" + {alignCount} Inhalts-Angleichung(en)" : "")
                  + " aus dem letzten Meeting.",
            Help = BuildHelp(),
            Notes = SessionNotes(),
            Glossary = Glossary(),
            BulkAction = new ReviewBulkAction(
                Label: "✓ Alle übernehmen (Experiment — ohne Einzelprüfung)",
                Set:
                [
                    new ReviewFieldValue(FieldDecision, "apply"),
                    new ReviewFieldValue(FieldAlignDecision, "accept"),
                    new ReviewFieldValue(FieldReason, "Sammel-Freigabe durch Reviewer (accept-all im UI).")
                ],
                Confirm: "{n} offene Punkte uebernehmen (Struktur-Ops + Angleichungen)? ACHTUNG: aendert die "
                    + "Projektwahrheit (Core) und speist danach den GitHub-Sync. Bereits getroffene Entscheide bleiben unberuehrt."),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldHasAlign, "", ReviewInputType.Hidden, [], Required: false),
                new ReviewFieldSpec(FieldAlignPbiId, "", ReviewInputType.Hidden, [], Required: false),
                new ReviewFieldSpec(FieldKind, "", ReviewInputType.Hidden, [], Required: false),  // B1: Träger für VisibleWhen NEW_PBI
                // B1: nur bei NEW_PBI sichtbar. Der Platzierungs-Agent schlägt ein bestehendes Feature vor; der Mensch
                // kann es hier (oder per Klick in der Feature-Landkarte rechts) auf ein anderes bestehendes Feature
                // korrigieren. Der Apply schreibt die Op auf das gewählte Feature um. (B2 später: neue In-Plan-Features.)
                new ReviewFieldSpec(FieldFeature, "Feature-Zuordnung", ReviewInputType.Dropdown, [], Required: false,
                    Help: "Unter welchem bestehenden Feature dieses neue PBI angelegt wird. Änderbar — wähle ein anderes "
                        + "Feature aus der Liste oder klicke es in der Feature-Landkarte (rechts) an.",
                    Options: featureOptions, VisibleWhen: OnlyNewPbi),
                new ReviewFieldSpec(FieldDecision, "Struktur-Entscheidung", ReviewInputType.Dropdown, ["apply", "skip", "to_decision"], Required: true,
                    Help: "Ob dieses PBI von der Aenderung betroffen ist — Details im Banner 'Was bewirkt dein Entscheid?'. "
                        + "'→ Entscheidung' = das ist NICHT deine Entscheidung (Stakeholder noetig): praegt eine offene Entscheidung, das PBI wird geschuetzt geblockt, das decision-gate legt sie beim naechsten Lauf vor.",
                    Options:
                    [
                        new ReviewOption("apply", "Uebernehmen — aendert die Projektwahrheit (Core)"),
                        new ReviewOption("skip", "Ueberspringen — Aenderung verwerfen (Begruendung Pflicht)"),
                        new ReviewOption("to_decision", "→ Entscheidung noetig (Stakeholder) — praegt offene Entscheidung, PBI wird geblockt (Begruendung Pflicht = die Frage)")
                    ]),
                new ReviewFieldSpec(FieldAlignDecision, "Inhaltliche Angleichung", ReviewInputType.Dropdown, ["accept", "edit", "skip"], Required: true,
                    Help: "Wie der angepasste PBI-Inhalt in die Wahrheit uebernommen wird (eigene Entscheidung, unabhaengig von der Struktur).",
                    Options:
                    [
                        new ReviewOption("accept", "Uebernehmen — Vorschlag wird PBI-Inhalt, PBI wird geklaert (active)"),
                        new ReviewOption("edit", "Anpassen — deine Fassung wird uebernommen"),
                        new ReviewOption("skip", "Nicht angleichen — PBI bleibt ungeklaert (needs_clarify), Begruendung Pflicht")
                    ],
                    VisibleWhen: OnlyHasAlign),
                new ReviewFieldSpec(FieldAlignTitle, "Titel (angeglichen)", ReviewInputType.FreeText, [], Required: false,
                    Help: "Angepasster PBI-Titel. Leer = Vorschlag behalten.", VisibleWhen: OnlyAlignEdit),
                new ReviewFieldSpec(FieldAlignStatement, "Statement (angeglichen)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Als <Rolle> will ich ... Leer = Vorschlag behalten.", VisibleWhen: OnlyAlignEdit),
                new ReviewFieldSpec(FieldAlignAcceptance, "Akzeptanzkriterien (angeglichen)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Eine Zeile pro Kriterium. Leer = Vorschlag behalten.", VisibleWhen: OnlyAlignEdit),
                new ReviewFieldSpec(FieldReason, "Begruendung (Audit-Protokoll)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Pflicht beim Ueberspringen/Nicht-angleichen. Landet als Beleg in human-decisions.json — keine Anweisung ans System.")
            ],
            Items = items
        };
    }

    // E0.3/R-26-C: Items starten offen; Begruendung nur beim skip Pflicht. Traegt ein Op-Item eine Angleichung,
    // braucht es BEIDE Entscheidungen (Struktur + Inhalt) — getrennt, nie automatisch voneinander abgeleitet.
    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        // Begruendung Pflicht bei skip (P2a) UND to_decision (R-14 D2: die Begruendung WIRD der Entscheidungs-Text).
        if ((string.Equals(decision, "skip", StringComparison.OrdinalIgnoreCase)
             || string.Equals(decision, "to_decision", StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        if (HasAlign(item))
        {
            var ad = FieldOf(item, FieldAlignDecision);
            if (!AlignDecisions.Contains(ad)) return false;
            if (string.Equals(ad, "skip", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        }
        return true;
    }

    public static PbiUpdateDecisionsFile Apply(string runId, ReviewSession session)
    {
        var ops = session.Items
            .Select(it => new PbiUpdateDecision(it.ItemId, FieldOf(it, FieldDecision), NullIfEmpty(FieldOf(it, FieldReason)),
                // B1: die (evtl. geänderte) Feature-Zuordnung NUR bei NEW_PBI mitschreiben — sonst irrelevant.
                FeatureId: string.Equals(FieldOf(it, FieldKind), PbiUpdateKind.NewPbi, StringComparison.Ordinal)
                    ? NullIfEmpty(FieldOf(it, FieldFeature)) : null))
            .ToList();
        var aligns = session.Items.Where(HasAlign)
            .Select(it => new PbiAlignmentDecision(
                FieldOf(it, FieldAlignPbiId),
                FieldOf(it, FieldAlignDecision),
                NullIfEmpty(FieldOf(it, FieldAlignTitle)),
                NullIfEmpty(FieldOf(it, FieldAlignStatement)),
                SplitLines(FieldOf(it, FieldAlignAcceptance)),
                NullIfEmpty(FieldOf(it, FieldReason))))
            .ToList();
        return new(runId, "human (review-ui)", ops, aligns.Count > 0 ? aligns : null);
    }

    // R-14 D2: aus den Review-Entscheiden die „→ Entscheidung"-Antraege extrahieren (op-<i> → Op des Plans).
    // Die PFLICHT-Begruendung ist die Stakeholder-Frage; ohne sie (defensiv) kein Antrag.
    public static IReadOnlyList<Decision.PbiDecisionRequest> DecisionRequestsFrom(
        PbiStateChangePlanDocument plan, IReadOnlyList<PbiUpdateDecision> decisions)
        => decisions
            .Where(d => string.Equals(d.Decision, "to_decision", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(d.Reason))
            .Select(d => d.OpId.StartsWith("op-", StringComparison.Ordinal) && int.TryParse(d.OpId["op-".Length..], out var i)
                         && i >= 0 && i < plan.Operations.Count ? (Op: plan.Operations[i], d.Reason) : (Op: null!, d.Reason))
            .Where(x => x.Op is not null)
            .Select(x => new Decision.PbiDecisionRequest(x.Op.RequirementId, x.Op.PbiId, x.Reason!))
            .ToList();

    public static void MergeExistingDecisions(ReviewSession session, PbiUpdateDecisionsFile? file)
    {
        ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) =>
            {
                Set(item, FieldDecision, d.Decision);
                Set(item, FieldReason, d.Reason);
                // B1: nur eine explizit gesetzte Feature-Korrektur zurückspielen; sonst bleibt der Vorschlag (Default).
                if (!string.IsNullOrWhiteSpace(d.FeatureId)) Set(item, FieldFeature, d.FeatureId);
            }, Resolved);
        // Angleichungs-Entscheidungen per pbiId -> das Op-Item, das diese Angleichung traegt (alignPbiId).
        foreach (var ad in file?.AlignmentDecisions ?? [])
        {
            var item = session.Items.FirstOrDefault(it => string.Equals(FieldOf(it, FieldAlignPbiId), ad.PbiId, StringComparison.Ordinal));
            if (item is null) continue;
            Set(item, FieldAlignDecision, ad.Decision);
            Set(item, FieldAlignTitle, ad.EditedTitle);
            Set(item, FieldAlignStatement, ad.EditedStatement);
            Set(item, FieldAlignAcceptance, ad.EditedAcceptanceCriteria is { Count: > 0 } c ? string.Join('\n', c) : null);
            if (!string.IsNullOrWhiteSpace(ad.Reason)) Set(item, FieldReason, ad.Reason);
            item.Resolved = Resolved(item);
        }
    }

    private static bool HasAlign(ReviewItem item) => string.Equals(FieldOf(item, FieldHasAlign), "yes", StringComparison.Ordinal);
    private static string? NullIfEmpty(string v) => string.IsNullOrWhiteSpace(v) ? null : v;

    // Ordnet jede Angleichung ihrem ersten passenden MARK_CHANGED/SUPERSEDE/EXTEND_PBI-Op zu (gleicher PBI).
    // O3a: EXTEND_PBI traegt jetzt ebenfalls einen Draft (extend-Modus), muss also als Angleichungsblock erscheinen.
    private static IReadOnlyDictionary<int, PbiAlignment> MapAlignmentsToOps(PbiStateChangePlanDocument plan)
    {
        var map = new Dictionary<int, PbiAlignment>();
        foreach (var a in plan.Alignments ?? [])
        {
            for (var i = 0; i < plan.Operations.Count; i++)
            {
                var op = plan.Operations[i];
                if (map.ContainsKey(i)) continue;
                // align/extend: an das bestehende PBI-Op (gleiche PbiId). O3b create: an das NEW_PBI-Op mit
                // passender Ziel-Requirement (das PBI existiert noch nicht, daher kein PbiId-Match).
                var matchesExisting = a.PbiId is not null
                    && string.Equals(op.PbiId, a.PbiId, StringComparison.Ordinal)
                    && (op.Kind == PbiUpdateKind.MarkChanged || op.Kind == PbiUpdateKind.SupersedePbi || op.Kind == PbiUpdateKind.ExtendPbi);
                // O3b: NEW_PBI (create im bestehenden Feature) · O4b: NEW_FEATURE (create im NEUEN Feature) —
                // beide tragen den PBI-Draft, damit der Mensch ihn vor Freigabe sieht (Gate-Sinn).
                var matchesCreate = a.PbiId is null && a.TargetRequirementId is not null
                    && (op.Kind == PbiUpdateKind.NewPbi || op.Kind == PbiUpdateKind.NewFeature)
                    && string.Equals(op.RequirementId, a.TargetRequirementId, StringComparison.Ordinal);
                if (matchesExisting || matchesCreate) { map[i] = a; break; }
            }
        }
        return map;
    }
    private static IReadOnlyList<string>? SplitLines(string v)
        => string.IsNullOrWhiteSpace(v) ? null
           : v.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(l => l.Length > 0).ToArray();

    // E0.3d: Drilldowns loesen zu LESBAREM Text auf statt Roh-JSON.
    public static string ResolveContext(string key, PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx)
            && idx >= 0 && idx < plan.Operations.Count)
            return DescribeOp(plan.Operations[idx], byId);
        if (key.StartsWith("pbi:", StringComparison.Ordinal))
            return byId.TryGetValue(key["pbi:".Length..], out var p) ? DescribePbi(p) : "(PBI nicht im Core gefunden)";
        if (key.StartsWith("requirement:", StringComparison.Ordinal))
            return byId.TryGetValue(key["requirement:".Length..], out var r) ? DescribeRequirement(r) : "(Anforderung nicht im Core gefunden)";
        return $"(Unbekannter Kontext: {key})";
    }

    // B1/B2: Auswahl-Katalog. Bestehende Features (Value=featureId, Label "FC-xx — Label") + die im selben Plan
    // vorgeschlagenen NEUEN Features (Value="proposed:<Label>", Label "🆕 NEU: <Label>", dedup per Label). Die
    // generische UI zerlegt das Label (id — proposition) für die rechte Feature-Landkarte.
    private static IReadOnlyList<ReviewOption> FeatureOptions(PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var existing = core.Items.Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .Select(f => new ReviewOption(f.ItemId, $"{f.ItemId} — {f.Feature?.Label ?? f.Text}"));
        // B2: die NEW_FEATURE-Ops dieses Plans als wählbare Ziele — so kann der Mensch ein NEW_PBI in ein gerade
        // vorgeschlagenes neues Feature umhängen (die eigentliche Kopplung).
        var proposed = plan.Operations
            .Where(o => string.Equals(o.Kind, PbiUpdateKind.NewFeature, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(o.ProposedFeatureLabel))
            .Select(o => o.ProposedFeatureLabel!.Trim())
            .Distinct(StringComparer.Ordinal)
            .Select(label => new ReviewOption(ProposedPrefix + label, $"🆕 NEU: {label}"));
        return existing.Concat(proposed).ToList();
    }

    // B1/B2: Detailkarte in der rechten Landkarte. Bestehendes Feature -> seine PBIs (über part_of_feature).
    // Vorgeschlagenes neues Feature ("proposed:<Label>") -> die geplanten Anforderungen aus dem Plan. Rein
    // informativ (Lese-Landkarte, hilft der Feature-Wahl); löst KEINE Wahrheits-Mutation aus.
    public static ReviewReferenceDetails? ResolveReference(string reference, ProjectStateDocument core, PbiStateChangePlanDocument? plan = null)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        // B2: vorgeschlagenes neues Feature — existiert noch nicht im Core, Details kommen aus dem Plan.
        if (reference.StartsWith(ProposedPrefix, StringComparison.Ordinal))
        {
            if (plan is null) return null;
            var label = reference[ProposedPrefix.Length..];
            var reqs = plan.Operations
                .Where(o => string.Equals(o.Kind, PbiUpdateKind.NewFeature, StringComparison.Ordinal)
                            && string.Equals(o.ProposedFeatureLabel?.Trim(), label, StringComparison.Ordinal))
                .Select(o => byId.TryGetValue(o.RequirementId, out var r) ? r.Text : o.RequirementId)
                .ToList();
            var pnotes = new List<ReviewNote>
            {
                new(ReviewNoteKind.Suggestion, $"🆕 Neues Feature „{label}“", "Wird beim Übernehmen neu angelegt (Fall C).")
            };
            foreach (var t in reqs) pnotes.Add(new ReviewNote(ReviewNoteKind.Info, "Geplante Anforderung", Truncate(t, TextPreviewChars)));
            return new ReviewReferenceDetails(reference, $"🆕 {label}", $"Neues Feature, {reqs.Count} geplante Anforderung(en).", pnotes, []);
        }
        var featureId = reference;
        if (!byId.TryGetValue(featureId, out var feat) || !string.Equals(feat.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            return null;
        // part_of_feature: pbi -> feature (FromId = PBI, ToId = Feature).
        var pbiIds = core.Relations
            .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                        && string.Equals(r.ToId, featureId, StringComparison.Ordinal))
            .Select(r => r.FromId)
            .Where(id => byId.TryGetValue(id, out var it) && string.Equals(it.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal).ToList();
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, $"Feature {feat.ItemId} ({StatusLabel(feat.Status)})", feat.Feature?.Label ?? feat.Text)
        };
        if (pbiIds.Count == 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "PBIs in diesem Feature", "(noch keine)"));
        else
            foreach (var pid in pbiIds)
                notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, $"{pid} ({StatusLabel(byId[pid].Status)})",
                    byId[pid].Pbi?.Title ?? byId[pid].Text));
        return new ReviewReferenceDetails(featureId, feat.Feature?.Label ?? feat.Text,
            $"{pbiIds.Count} PBI(s) in diesem Feature.", notes, []);
    }

    private static ReviewHelp BuildHelp() => new(
        "Backlog an neue Anforderungen anpassen",
        "Ein neues Meeting hat die Projektwahrheit geaendert. Hier autorisierst du, wie die betroffenen Backlog-Items "
        + "(PBIs) darauf reagieren — bevor daraus ein GitHub-Sync entsteht.",
        [
            new ReviewHelpSection("Zwei getrennte Entscheidungen je Item",
                "• Struktur — was mit dem PBI passiert (übernehmen/überspringen): anlegen, erweitern, als geändert markieren, "
                + "blockieren, Anforderung ersetzen oder neues Feature. Die Wirkung-Zeile sagt es je Item konkret.\n"
                + "• Inhaltliche Angleichung (falls angeboten) — der vorgeschlagene neue Titel/Statement/Akzeptanzkriterien. "
                + "Übernehmen klärt das PBI (geändert · ungeklärt → aktiv); bearbeiten oder ablehnen geht auch.\n"
                + "\n"
                + "Beide Entscheidungen sind unabhängig voneinander."),
            new ReviewHelpSection("Woher kommen die Operationen?",
                "Sie werden regelbasiert aus dem Meeting-Delta abgeleitet (kein LLM entscheidet den Op-Typ). Nur die "
                + "Platzierung (neues PBI vs. Erweiterung vs. neues Feature) und der Angleichungs-Vorschlag kommen von einem Agenten."),
            new ReviewHelpSection("Alle übernehmen (Experiment)",
                "Der Sammel-Button setzt alle noch offenen Struktur-Entscheidungen auf Übernehmen — bewusst OHNE Einzelprüfung. "
                + "Deklarierter Experiment-Modus (wie accept-all/replay), NICHT der Normalweg. Bereits gesetzte Entscheidungen bleiben unberührt."),
            new ReviewHelpSection("Was passiert nach Fertig",
                "Die UI schreibt human-decisions.json. pbi-update-apply fuehrt die uebernommenen Operationen deterministisch "
                + "aus, aendert den Core (Status/Deckung der PBIs) und schreibt ein github-sync-Delta -> naechstes Gate (Forward).")
        ]);

    // E0.3e: die Wirkungskette IM UI — in Sprache, die auch Projektfremde verstehen.
    private static IReadOnlyList<ReviewNote> SessionNotes() =>
    [
        new ReviewNote(ReviewNoteKind.Info, "Was bewirkt dein Entscheid?",
            "Uebernehmen ⇒ die Aenderung wird Teil der Projektwahrheit (Core) und fliesst danach in den GitHub-Sync. "
            + "Was genau: neues Backlog-Item anlegen, Anforderung zuordnen, Item als geaendert/blockiert markieren oder "
            + "eine Anforderung ersetzen.\n"
            + "Ueberspringen ⇒ die Aenderung wird nicht uebernommen, der Core bleibt wie er ist. Bitte kurz begruenden.\n"
            + "Angleichung (ANGLEICHUNG) ⇒ hier wird der PBI-INHALT (Titel/Statement/Akzeptanzkriterien) an die "
            + "geaenderte Anforderung angepasst. Uebernehmen klaert das PBI (geändert · ungeklärt → aktiv); Nicht-angleichen "
            + "laesst es ungeklaert.")
    ];

    // E0.3e: Fach-Begriffe in Klartext mit Wirkungs-Ehrlichkeit (Muster wie Forward-Review).
    private const string GOps = "Operationen — was mit dem PBI passiert";
    private const string GStates = "PBI-Zustände";
    private const string GMisc = "Weiteres";

    private static IReadOnlyList<ReviewGlossaryEntry> Glossary() =>
    [
        new("Neues PBI", "Legt ein neues Backlog-Item (PBI) fuer eine neue Anforderung an.", GOps),
        new("PBI erweitern", "Ordnet einem bestehenden PBI zusaetzlich eine Anforderung zu.", GOps),
        new("Als geändert markieren", "Markiert das PBI als geaendert · ungeklaert, weil seine Anforderung verfeinert wurde — die Angleichung klaert Titel/Kriterien.", GOps),
        new("Blockieren", "Setzt das PBI auf blockiert · Entscheidung — es wartet auf eine offene Entscheidung, es entsteht kein Issue.", GOps),
        new("Anforderung ersetzen", "Tauscht eine abgeloeste Anforderung gegen ihren Ersatz aus (und markiert das PBI als geaendert · ungeklaert).", GOps),
        new("Neues Feature", "Kein bestehendes Feature passt — legt ein neues Feature + erstes PBI an.", GOps),
        new("geändert · ungeklärt", "PBI-Zustand: geaendert, aber Titel/Kriterien noch nicht an die neue Anforderung angeglichen (intern: needs_clarify).", GStates),
        new("blockiert · Entscheidung", "PBI-Zustand: wartet auf eine Entscheidung — es entsteht (noch) kein GitHub-Issue (intern: blocked_by_decision).", GStates),
        new("verfeinert", "Die Anforderung wurde inhaltlich praezisiert — eine neue Fassung ersetzt die alte (Vorher/Nachher unten).", GMisc)
    ];

    // ---- Klartext-Helfer (E0.3-Retrofit 03.08.; nur Anzeige) ------------------------------------------
    private static string KindBadge(string kind) => kind switch
    {
        PbiUpdateKind.NewPbi => "Neues PBI",
        PbiUpdateKind.ExtendPbi => "PBI erweitern",
        PbiUpdateKind.MarkChanged => "Als geändert markieren",
        PbiUpdateKind.BlockPbi => "Blockieren",
        PbiUpdateKind.SupersedePbi => "Anforderung ersetzen",
        PbiUpdateKind.NewFeature => "Neues Feature",
        _ => kind
    };

    private static string KindEffect(string kind) => kind switch
    {
        PbiUpdateKind.NewPbi => "Legt ein neues Backlog-Item (PBI) im Core an.",
        PbiUpdateKind.ExtendPbi => "Ordnet einem bestehenden PBI zusätzlich eine Anforderung zu (PBI wird geändert · ungeklärt).",
        PbiUpdateKind.MarkChanged => "Markiert das PBI als geändert · ungeklärt — die Angleichung unten klärt Titel/Kriterien.",
        PbiUpdateKind.BlockPbi => "Setzt das PBI auf blockiert (wartet auf eine offene Entscheidung) — es entsteht kein Issue.",
        PbiUpdateKind.SupersedePbi => "Tauscht die abgelöste Anforderung gegen ihren Ersatz (PBI wird geändert · ungeklärt).",
        PbiUpdateKind.NewFeature => "Legt ein NEUES Feature + erstes PBI im Core an.",
        _ => "—"
    };

    private static string StatusLabel(string status) => status switch
    {
        "active" => "aktiv",
        "needs_clarify" => "geändert · ungeklärt",
        "blocked_by_decision" => "blockiert · Entscheidung",
        "done" => "fertig",
        "superseded" => "abgelöst",
        "baseline" => "Baseline",
        "accepted" => "angenommen",
        "open_decision" => "offene Entscheidung",
        "resolved" => "aufgelöst",
        _ => status
    };

    private static readonly IReadOnlyList<ReviewOption> SkipOption = [new("skip", "Überspringen")];
    private static IReadOnlyList<ReviewOption> DecisionOptions(string kind) => kind switch
    {
        PbiUpdateKind.NewPbi => [new("apply", "✓ PBI anlegen"), .. SkipOption],
        PbiUpdateKind.ExtendPbi => [new("apply", "✓ Erweiterung übernehmen"), .. SkipOption],
        PbiUpdateKind.MarkChanged => [new("apply", "✓ Änderung übernehmen"), .. SkipOption],
        PbiUpdateKind.BlockPbi => [new("apply", "✓ Blockieren übernehmen"), .. SkipOption],
        PbiUpdateKind.SupersedePbi => [new("apply", "✓ Ersetzen übernehmen"), .. SkipOption],
        PbiUpdateKind.NewFeature => [new("apply", "✓ Feature + PBI anlegen"), .. SkipOption],
        _ => [new("apply", "✓ Übernehmen"), .. SkipOption]
    };

    private static ReviewItem BuildItem(string opId, int idx, PbiStateChangeOperation op,
        IReadOnlyDictionary<string, ProjectStateItem> byId, PbiAlignment? align)
    {
        var pbi = op.PbiId is not null ? byId.GetValueOrDefault(op.PbiId) : null;
        var req = byId.GetValueOrDefault(op.RequirementId);
        var reqShort = Truncate(req?.Text ?? op.RequirementId, 90);

        // E0.3f: Summary mit Text statt nackter IDs.
        var summary = op.Kind switch
        {
            PbiUpdateKind.NewPbi => $"NEU: Backlog-Item fuer „{reqShort}“",
            PbiUpdateKind.ExtendPbi => $"{PbiTitle(pbi, op.PbiId)}: deckt zusaetzlich „{reqShort}“ ab",
            PbiUpdateKind.MarkChanged => $"{PbiTitle(pbi, op.PbiId)}: Anforderung wurde verfeinert",
            PbiUpdateKind.BlockPbi => $"{PbiTitle(pbi, op.PbiId)}: blockiert durch offene Entscheidung",
            PbiUpdateKind.SupersedePbi => $"{PbiTitle(pbi, op.PbiId)}: Anforderung wird ersetzt",
            PbiUpdateKind.NewFeature => $"NEU: Feature + Backlog-Item für „{reqShort}“",
            _ => $"{KindBadge(op.Kind)} {op.PbiId}"
        };

        var notes = new List<ReviewNote> { new(ReviewNoteKind.Info, "Wirkung", KindEffect(op.Kind)) };

        // Betroffenes PBI (aktueller Inhalt) — bei allen ausser NEW_PBI.
        if (pbi is not null)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Betroffenes PBI ({pbi.ItemId}, {StatusLabel(pbi.Status)})",
                $"{pbi.Pbi?.Title ?? pbi.Text}"
                + (string.IsNullOrWhiteSpace(pbi.Pbi?.Goal) ? "" : $"\n{pbi.Pbi!.Goal}")));

        // Die Anforderung(en) im Klartext, mit Vorher/Nachher wo es das Kind hergibt.
        switch (op.Kind)
        {
            case PbiUpdateKind.MarkChanged:
            {
                var prev = PreviousText(req);
                notes.Add(prev is null
                    ? new ReviewNote(ReviewNoteKind.Suggestion, $"Anforderung {op.RequirementId} (neue Fassung)", ReqBody(req))
                    : new ReviewNote(ReviewNoteKind.Info, $"Anforderung {op.RequirementId} — Vorher/Nachher (− alt · + neu)",
                        $"− {Truncate(prev, TextPreviewChars)}\n+ {ReqBody(req)}"));
                if (align is null) notes.Add(NeedsClarifyNote(op.RequirementId)); // ohne Angleichungs-Vorschlag: Lücke sichtbar
                break;
            }
            case PbiUpdateKind.SupersedePbi:
            {
                var repl = op.ReplacementRequirementId is not null ? byId.GetValueOrDefault(op.ReplacementRequirementId) : null;
                notes.Add(new ReviewNote(ReviewNoteKind.Info,
                    $"Anforderungs-Ersatz ({op.RequirementId} → {op.ReplacementRequirementId})",
                    $"− {Truncate(req?.Text ?? op.RequirementId, TextPreviewChars)}\n+ {Truncate(repl?.Text ?? op.ReplacementRequirementId ?? "?", TextPreviewChars)}"));
                if (align is null) notes.Add(NeedsClarifyNote(op.ReplacementRequirementId ?? op.RequirementId));
                break;
            }
            case PbiUpdateKind.BlockPbi:
            {
                var dec = op.OpenDecisionRef is not null ? byId.GetValueOrDefault(op.OpenDecisionRef) : null;
                notes.Add(new ReviewNote(ReviewNoteKind.Warning,
                    $"Blockierende Entscheidung ({op.OpenDecisionRef})",
                    dec?.Text is { Length: > 0 } t ? t : "(Entscheidungs-Text nicht im Core gefunden)"));
                break;
            }
            case PbiUpdateKind.NewPbi:
            {
                var feat = op.FeatureId is not null ? byId.GetValueOrDefault(op.FeatureId) : null;
                notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, $"Neue Anforderung {op.RequirementId}", ReqBody(req)));
                notes.Add(new ReviewNote(ReviewNoteKind.Info, "Einordnung",
                    $"Feature: {feat?.Feature?.Label ?? feat?.Text ?? op.FeatureId ?? "-"}"));
                break;
            }
            case PbiUpdateKind.NewFeature:  // O4: kein bestehendes Feature passt -> neues Feature + erstes PBI
                notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, $"Neue Anforderung {op.RequirementId}", ReqBody(req)));
                notes.Add(new ReviewNote(ReviewNoteKind.Info, "Einordnung",
                    $"NEUES Feature wird angelegt: {op.ProposedFeatureLabel ?? "-"}"));
                break;
            case PbiUpdateKind.ExtendPbi:
                notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, $"Zusaetzliche Anforderung {op.RequirementId}", ReqBody(req)));
                break;
        }

        // E0.9-Leitsatz: die Platzierungs-Begruendung ist NUR bei NEW/EXTEND handlungsleitend — dort entscheidet
        // ein Agent, WOHIN eine neue Anforderung gehoert (eigenes PBI vs. Erweiterung), und begruendet es. Bei den
        // deterministischen Kinds (MARK/BLOCK/SUPERSEDE) ist die Rationale ein Regel-Leersatz -> nicht zeigen.
        if (PbiUpdateKind.Placement.Contains(op.Kind) && !string.IsNullOrWhiteSpace(op.Rationale))
            notes.Add(new ReviewNote(ReviewNoteKind.Reason, "Warum diese Zuordnung? (Vorschlag des Platzierungs-Agenten)", op.Rationale));

        // R-26-C / O3a: Angleichung an einem BESTEHENDEN PBI — AKTUELL + ANGEGLICHEN nebeneinander.
        if (align is not null && pbi is not null)
        {
            notes.Add(new ReviewNote(ReviewNoteKind.Reason, "Inhaltliche Angleichung — warum",
                align.Rationale + $"\nAusgeloest durch: {(align.TriggerRequirementIds.Count == 0 ? "-" : string.Join(", ", align.TriggerRequirementIds))}"));
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "PBI-Inhalt AKTUELL",
                PbiContentBlock(pbi.Pbi?.Title ?? pbi.Text, pbi.Pbi?.Goal, pbi.Pbi?.AcceptanceCriteria)));
            notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, "PBI-Inhalt ANGEGLICHEN (Vorschlag)",
                PbiContentBlock(align.ProposedTitle, align.ProposedStatement, align.ProposedAcceptanceCriteria)));
        }
        // O3b create (NEW_PBI): es gibt noch KEIN aktuelles PBI — aber der vorgeschlagene Inhalt MUSS sichtbar sein.
        // Sonst uebernaehme der Mensch bei Default "accept" ungesehenen Inhalt in die Core-Wahrheit (Gate-Sinn).
        else if (align is not null)
        {
            notes.Add(new ReviewNote(ReviewNoteKind.Reason, "Neues PBI — warum",
                align.Rationale + $"\nAusgeloest durch: {(align.TriggerRequirementIds.Count == 0 ? "-" : string.Join(", ", align.TriggerRequirementIds))}"));
            notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, "Neues PBI — so wuerde es entstehen (Vorschlag)",
                PbiContentBlock(align.ProposedTitle, align.ProposedStatement, align.ProposedAcceptanceCriteria)));
        }

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Operation im Detail", $"op:{idx}") };
        if (op.PbiId is not null) context.Add(new ContextBlock(ContextBlockKind.Reference, $"PBI {op.PbiId} komplett", $"pbi:{op.PbiId}"));
        context.Add(new ContextBlock(ContextBlockKind.Reference, $"Anforderung {op.RequirementId}", $"requirement:{op.RequirementId}"));

        // E0.3c: kein Vorentscheid — der Mensch entscheidet aktiv (Accept all nur ueber den Bestaetigungs-Button).
        var fields = new List<ReviewFieldValue>
        {
            new(FieldDecision, ""),
            new(FieldReason, ""),
            new(FieldHasAlign, align is not null ? "yes" : ""),
            // O3b: DraftKey = bestehende PbiId (align/extend) ODER Ziel-Requirement (create) — einheitlicher Match.
            new(FieldAlignPbiId, align?.DraftKey ?? ""),
            new(FieldKind, op.Kind),   // B1: Träger für VisibleWhen (Feature-Feld nur bei NEW_PBI)
            // B1: vorbelegt mit dem Vorschlag des Platzierungs-Agenten; nur bei NEW_PBI sichtbar/relevant.
            new(FieldFeature, op.Kind == PbiUpdateKind.NewPbi ? op.FeatureId ?? "" : "")
        };
        if (align is not null)
        {
            // Autor-Wunsch: die Angleichung startet auf "Uebernehmen" (der Vorschlag ist der erwartete Normalfall).
            // Die STRUKTUR-Entscheidung hat weiter KEINEN Default (Durchwink-Schutz) — beide bleiben getrennt.
            fields.Add(new ReviewFieldValue(FieldAlignDecision, "accept"));
            fields.Add(new ReviewFieldValue(FieldAlignTitle, align.ProposedTitle ?? ""));
            fields.Add(new ReviewFieldValue(FieldAlignStatement, align.ProposedStatement ?? ""));
            fields.Add(new ReviewFieldValue(FieldAlignAcceptance, align.ProposedAcceptanceCriteria is { Count: > 0 } c ? string.Join('\n', c) : ""));
        }

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = align is not null ? summary + "  ·  + Inhalts-Angleichung" : summary,
            Badge = KindBadge(op.Kind),
            Notes = notes,
            FieldOptions = new Dictionary<string, IReadOnlyList<ReviewOption>> { [FieldDecision] = DecisionOptions(op.Kind) },
            ContextBlocks = context,
            FieldValues = fields
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // Zeigt die drei PBI-Module klar getrennt (Titel / Statement / Akzeptanzkriterien) — die UI hebt die
    // "Label:"-Zeilen hervor, sodass alt und neu Modul-fuer-Modul vergleichbar sind.
    private static string PbiContentBlock(string? title, string? statement, IReadOnlyList<string>? acceptance)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Titel: ").Append(string.IsNullOrWhiteSpace(title) ? "(unveraendert)" : title);
        sb.Append("\nStatement: ").Append(string.IsNullOrWhiteSpace(statement) ? "(unveraendert)" : statement);
        sb.Append("\nAkzeptanzkriterien:");
        if (acceptance is { Count: > 0 } ac)
            foreach (var c in ac) sb.Append("\n- ").Append(c);
        else
            sb.Append(" (unveraendert)");
        return sb.ToString();
    }

    // Der ehrliche R-26-Hinweis am Item — verbindet dieses Gate mit der offenen Klaerungs-Luecke.
    private static ReviewNote NeedsClarifyNote(string reqId) => new(ReviewNoteKind.Warning,
        "Danach: geändert · ungeklärt",
        "Für diese Operation liegt KEIN Angleichungs-Vorschlag vor. Uebernehmen setzt das PBI auf 'geaendert · ungeklaert'; "
        + $"Titel und Akzeptanzkriterien sind dann (noch) nicht an die neue Fassung von {reqId} angeglichen — die inhaltliche "
        + "Klaerung bleibt eine offene Aufgabe.");

    private static string DescribeOp(PbiStateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> byId)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{KindBadge(op.Kind)}");
        if (op.PbiId is not null) sb.AppendLine($"Ziel-PBI: {PbiTitle(byId.GetValueOrDefault(op.PbiId), op.PbiId)}");
        if (op.FeatureId is not null) sb.AppendLine($"Feature: {byId.GetValueOrDefault(op.FeatureId)?.Feature?.Label ?? op.FeatureId}");
        sb.AppendLine($"Anforderung: {op.RequirementId} — {byId.GetValueOrDefault(op.RequirementId)?.Text ?? "(nicht im Core)"}");
        if (op.ReplacementRequirementId is not null)
            sb.AppendLine($"Ersatz: {op.ReplacementRequirementId} — {byId.GetValueOrDefault(op.ReplacementRequirementId)?.Text ?? "(nicht im Core)"}");
        if (op.OpenDecisionRef is not null)
            sb.AppendLine($"Offene Entscheidung: {op.OpenDecisionRef} — {byId.GetValueOrDefault(op.OpenDecisionRef)?.Text ?? "(nicht im Core)"}");
        sb.AppendLine($"Regel-Begruendung: {op.Rationale}");
        return sb.ToString();
    }

    private static string DescribePbi(ProjectStateItem p)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{p.ItemId} — {p.Pbi?.Title ?? p.Text}  ({StatusLabel(p.Status)})");
        if (!string.IsNullOrWhiteSpace(p.Pbi?.Goal)) sb.AppendLine().AppendLine(p.Pbi!.Goal);
        if (p.Pbi?.AcceptanceCriteria is { Count: > 0 } ac)
        {
            sb.AppendLine().AppendLine("Akzeptanzkriterien:");
            foreach (var c in ac) sb.AppendLine($"- {c}");
        }
        return sb.ToString();
    }

    private static string DescribeRequirement(ProjectStateItem r)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{r.ItemId} ({StatusLabel(r.Status)}, v{r.Version})");
        sb.AppendLine().AppendLine(r.Text);
        var prev = PreviousText(r);
        if (prev is not null) sb.AppendLine().AppendLine("Fruehere Fassung:").AppendLine(prev);
        return sb.ToString();
    }

    private static string PbiTitle(ProjectStateItem? pbi, string? fallbackId)
        => pbi?.Pbi?.Title ?? pbi?.Text ?? fallbackId ?? "(PBI)";

    private static string ReqBody(ProjectStateItem? req) => Truncate(req?.Text ?? "(Anforderung nicht im Core gefunden)", TextPreviewChars);

    // Letzte fruehere Fassung aus der Ingestion-Historie, wenn sie sich vom aktuellen Text unterscheidet.
    private static string? PreviousText(ProjectStateItem? item)
    {
        var prev = item?.History?.LastOrDefault()?.Text;
        return string.IsNullOrWhiteSpace(prev) || string.Equals(prev, item!.Text, StringComparison.Ordinal) ? null : prev;
    }

    private static string Truncate(string value, int max) => ReviewFields.TruncateRaw(value, max); // Basis-W2

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
