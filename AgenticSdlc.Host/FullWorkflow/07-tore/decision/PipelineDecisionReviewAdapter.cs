using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.HumanReview;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

public sealed record PipelineDecisionDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("resolutions")] IReadOnlyList<PipelineDecisionResolution> Resolutions);

// R-14 G1-b (= E0.8): das EDIT-faehige Review des operativen decision-gate. Anders als die anderen Gates wird hier
// nicht ein Vorschlag bestaetigt — die AUFLOESUNG ENTSTEHT am Gate: je offener Entscheidung waehlt der Mensch den
// Ausgang (behalten/uebernehmen/verfeinern) und liefert bei Bedarf den neuen Text (Prefill = Meeting-Aussage).
// E0-Endstand: leerer Default (aktive Entscheidung, keine Durchwink-Falle) · Wirkungs-Klartext an jeder Option ·
// vertagen ist ERLAUBT und frei (P2a: nur Ablehnen braeuchte Begruendung — hier gibt es kein Ablehnen) ·
// Blast-Radius (blockierte PBIs) · gruppiertes Glossar · strukturierte Hilfe.
public static class PipelineDecisionReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldStatement = "newStatement";
    public const string FieldReason = "reason";

    public const string ChoiceKeep = "keep";
    public const string ChoiceAdopt = "adopt";
    public const string ChoiceRefine = "refine";
    public const string ChoiceDefer = "defer";

    private static readonly HashSet<string> Choices = new(StringComparer.OrdinalIgnoreCase) { ChoiceKeep, ChoiceAdopt, ChoiceRefine, ChoiceDefer };
    private static readonly HashSet<string> NeedsStatement = new(StringComparer.OrdinalIgnoreCase) { ChoiceAdopt, ChoiceRefine };

    private const string GOut = "Ausgänge — was deine Wahl bewirkt";
    private const string GTerms = "Begriffe";

    public static ReviewSession BuildSession(string runId, PipelineDecisionReviewRequest request)
    {
        return new ReviewSession
        {
            SessionId = $"decision-gate-{runId}",
            Title = "Offene Entscheidungen — Widersprüche auflösen (Tor 2)",
            Subtitle = request.Decisions.Count == 0
                ? "Keine offenen Entscheidungen."
                : $"{request.Decisions.Count} offene Entscheidung(en). Du löst auf oder vertagst — blockierte PBIs bleiben bis zur Auflösung geschützt.",
            Glossary =
            [
                new ReviewGlossaryEntry("Original behalten", "Der Widerspruch wird abgewiesen — die bestehende Anforderung bleibt unverändert gültig. WIRKT: Entscheidung → resolved, blockierte PBIs → active.", GOut),
                new ReviewGlossaryEntry("Neues übernehmen", "Die Meeting-Aussage gewinnt — die BEDEUTUNG KIPPT (z. B. 14 → 7 Tage). WIRKT: alte Anforderung → superseded (fällt aus allen Aktiv-Sichten, bleibt als Historie mit ihren Belegen), NEUE Anforderungs-ID mit deinem Text + supersedes-Kante; PBIs wechseln auf die neue (Deckungs-Swap) und werden zur Inhalts-Angleichung markiert.", GOut),
                new ReviewGlossaryEntry("Verfeinern", "Dieselbe Anforderung, nur SCHÄRFER gesagt — die Bedeutung bleibt (z. B. „14 Tage“ → „14 Tage, DSGVO-konform“). WIRKT: neue VERSION derselben ID (alte Fassung in der Historie), PBIs werden zur Angleichung markiert.", GOut),
                new ReviewGlossaryEntry("Vertagen", "Jetzt nicht entscheidbar (z. B. Stakeholder nötig). WIRKT: nichts — die Entscheidung bleibt offen und sichtbar geparkt, der Block hält, beim nächsten Lauf wird sie wieder vorgelegt.", GOut),
                new ReviewGlossaryEntry("offene Entscheidung (DEC)", "Ein erfasster Widerspruch zwischen Meeting und Projektwahrheit, der eine menschliche Entscheidung braucht.", GTerms),
                new ReviewGlossaryEntry("offene Frage (ohne Ziel)", "Eine im Meeting GESTELLTE Frage (9g) — kein Widerspruch, kein Ziel-Requirement. Auflösen = „Original behalten“ mit Pflicht-Begründung (was gilt jetzt / welche Anforderung beantwortet sie); Übernehmen/Verfeinern sind hier nicht möglich.", GTerms),
                new ReviewGlossaryEntry("blockiertes PBI", "Arbeit, die auf der umstrittenen Anforderung baut — wird bis zur Auflösung nicht nach GitHub gebracht (kein Leak).", GTerms),
                new ReviewGlossaryEntry("superseded", "Status einer abgelösten Anforderung: nicht mehr gültig, bleibt als Historie erhalten.", GTerms)
            ],
            Help = BuildHelp(),
            // E0-Muster: deklarierter Experiment-Bulk mit Bestätigung. „Alle vertagen" ist der EINZIG mögliche Bulk —
            // Auflösen braucht je Widerspruch eine echte inhaltliche Wahl (kein accept-all an einem Wahrheits-Konflikt).
            BulkAction = new ReviewBulkAction(
                "Alle vertagen (Stakeholder nötig)",
                [new ReviewFieldValue(FieldDecision, ChoiceDefer)],
                "Alle noch offenen Widersprüche VERTAGEN? Sie bleiben sichtbar geparkt, die blockierten PBIs bleiben geschützt, " +
                "und beim nächsten Lauf werden sie wieder vorgelegt. Bereits getroffene Entscheidungen bleiben unberührt."),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
                    [ChoiceKeep, ChoiceAdopt, ChoiceRefine, ChoiceDefer], Required: true,
                    Help: "Wie wird der Widerspruch aufgelöst? Vertagen ist erlaubt — dann bleibt alles geschützt geparkt.",
                    Options:
                    [
                        new(ChoiceKeep, "Original behalten — Widerspruch abweisen"),
                        new(ChoiceAdopt, "Neues übernehmen — alte Anforderung wird abgelöst"),
                        new(ChoiceRefine, "Verfeinern — geklärte Fassung formulieren"),
                        new(ChoiceDefer, "Vertagen — bleibt offen und geparkt")
                    ]),
                new ReviewFieldSpec(FieldStatement, "Neuer Anforderungs-Text (Pflicht bei Übernehmen/Verfeinern)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Vorbefüllt mit der Meeting-Aussage — editiere sie zur endgültigen Formulierung.",
                    VisibleWhen: new ReviewFieldVisibility(FieldDecision, [ChoiceAdopt, ChoiceRefine])),
                new ReviewFieldSpec(FieldReason, "Begründung (nur Audit)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Optional: warum so entschieden? Landet als Beleg an der Entscheidung.")
            ],
            Items = request.Decisions.Select(BuildItem).ToList()
        };
    }

    // Aufloesen braucht eine vollstaendige Wahl; bei Übernehmen/Verfeinern zusaetzlich den neuen Text.
    // Vertagen ist ohne weitere Pflichten gueltig (P2a: defer ist frei).
    public static bool Resolved(ReviewItem item)
    {
        var choice = FieldOf(item, FieldDecision);
        if (!Choices.Contains(choice)) return false;

        // 9g: die ITEM-Optionen sind die eine Quelle der Palette (FieldOptions, wie am ingest-Gate).
        // Generische Regel: eine Wahl ausserhalb der Item-Palette ist ungueltig. Ziellose Frage-DECs
        // (Palette ohne „Übernehmen") verlangen bei „geklärt" die Antwort als Pflicht-Begründung (P2a).
        if (item.FieldOptions.TryGetValue(FieldDecision, out var allowed) && allowed.Count > 0)
        {
            if (!allowed.Any(o => string.Equals(o.Value, choice, StringComparison.OrdinalIgnoreCase))) return false;
            var questionPalette = !allowed.Any(o => string.Equals(o.Value, ChoiceAdopt, StringComparison.OrdinalIgnoreCase));
            if (questionPalette && string.Equals(choice, ChoiceKeep, StringComparison.OrdinalIgnoreCase))
                return FieldOf(item, FieldReason) is { Length: > 0 };
        }
        return !NeedsStatement.Contains(choice) || FieldOf(item, FieldStatement) is { Length: > 0 };
    }

    public static PipelineDecisionDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it =>
        {
            var choice = FieldOf(it, FieldDecision);
            return choice.ToLowerInvariant() switch
            {
                ChoiceKeep => new PipelineDecisionResolution(it.ItemId, DecisionStage.ActionResolve, DecisionOutcome.KeepOriginal, null, ReasonOf(it)),
                ChoiceAdopt => new PipelineDecisionResolution(it.ItemId, DecisionStage.ActionResolve, DecisionOutcome.AdoptNew, FieldOf(it, FieldStatement), ReasonOf(it)),
                ChoiceRefine => new PipelineDecisionResolution(it.ItemId, DecisionStage.ActionResolve, DecisionOutcome.Refine, FieldOf(it, FieldStatement), ReasonOf(it)),
                _ => new PipelineDecisionResolution(it.ItemId, DecisionStage.ActionDefer, null, null, ReasonOf(it)),
            };
        }).ToList());

    public static void MergeExistingDecisions(ReviewSession session, PipelineDecisionDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Resolutions, r => r.DecisionId, (item, r) =>
        {
            Set(item, FieldDecision, ChoiceOf(r));
            Set(item, FieldStatement, r.NewStatement);
            Set(item, FieldReason, r.Reason);
        }, Resolved);

    // Entscheid-Datei -> Port-Response (Resume liest die Datei; DECs ohne Eintrag werden sicher VERTAGT).
    public static PipelineDecisionReviewResponse ToResponse(PipelineDecisionDecisionsFile file, PipelineDecisionReviewRequest request)
    {
        var byId = file.Resolutions.GroupBy(r => r.DecisionId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var resolutions = request.Decisions
            .Select(d => byId.GetValueOrDefault(d.DecisionId)
                         ?? new PipelineDecisionResolution(d.DecisionId, DecisionStage.ActionDefer, null, null, null))
            .ToList();
        return new PipelineDecisionReviewResponse(resolutions, file.Reviewer);
    }

    private static ReviewItem BuildItem(PipelineDecisionItemView v)
    {
        // E0-Muster: die Gegenüberstellung als Noten-PAAR (Vorher = Core, Dagegen = Meeting) — der Kern des Konflikts
        // steht direkt untereinander, bevor irgendetwas entschieden wird.
        // 9g: ziellose DECs (Meeting-Fragen) haben eine KLEINERE Options-Palette — kein Ziel, nichts abzuloesen.
        // Umsetzung ueber ReviewItem.FieldOptions (per-Item-Optionen, dasselbe Muster wie die Op-Arten am
        // ingest-Gate) — die Palette selbst ist die eine Quelle; Resolved() leitet die Regeln daraus ab.
        var targetless = v.TargetRequirementId is not { Length: > 0 };
        var notes = targetless
            ? new List<ReviewNote>
            {
                new(ReviewNoteKind.Suggestion, "Die offene Frage", v.DecisionText),
                new(ReviewNoteKind.Info, "So löst du sie auf",
                    "„Original behalten“ bedeutet hier: Frage ist GEKLÄRT/ERLEDIGT — die Begründung ist Pflicht und nennt, " +
                    "WAS jetzt gilt bzw. WELCHE Anforderung die Antwort trägt (z. B. „beantwortet durch REQ-81“). " +
                    "„Übernehmen“/„Verfeinern“ sind hier nicht möglich (kein Ziel). Vertagen hält sie sichtbar geparkt.")
            }
            : new List<ReviewNote>
            {
                new(ReviewNoteKind.Info, "Bestehende Wahrheit (Core)", $"{v.TargetRequirementId}: {v.TargetRequirementText}"),
                new(ReviewNoteKind.Suggestion, "Meeting sagt dagegen", v.ProposedStatement),
                new(ReviewNoteKind.Warning, "Ändert die Projektwahrheit",
                    "Übernehmen/Verfeinern mutiert Anforderungen (Ablösung bzw. neue Version) — die Inhalts-Angleichung der PBIs folgt im selben Lauf am nächsten Gate (pbi-update).")
            };
        if (v.Origin is { Length: > 0 })
            notes.Add(new ReviewNote(ReviewNoteKind.Info, targetless ? "Woher stammt diese offene Frage?" : "Woher stammt dieser Widerspruch?", v.Origin));
        if (v.BlockedPbis.Count > 0)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Blockierte PBIs (bis zur Auflösung geschützt)", string.Join(", ", v.BlockedPbis)));

        // E0-Befund des Autors (04.08.): der Einfluss muss ANKLICKBAR sein — Ziel-Anforderung und jedes
        // blockierte PBI als Kontext-Block (Details rendert der Runner aus dem live geladenen Core).
        var context = new List<ContextBlock>();
        if (v.TargetRequirementId is { Length: > 0 })
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Ziel-Anforderung {v.TargetRequirementId} ansehen", $"item:{v.TargetRequirementId}"));
        context.AddRange(v.BlockedPbis.Select(p => new ContextBlock(ContextBlockKind.Reference, $"{p} ansehen (betroffenes PBI)", $"item:{p}")));

        var item = new ReviewItem
        {
            ItemId = v.DecisionId,
            Summary = $"{v.DecisionId}: {v.DecisionText}",
            Badge = targetless ? "Offene Frage" : "Widerspruch",
            Notes = notes,
            ContextBlocks = context,
            // Default bewusst LEER — jede Auflösung ist eine aktive Entscheidung (E0-Politik, keine Durchwink-Falle).
            // 9g (saubere Form): ziellose DECs bekommen PER-ITEM-Optionen (ReviewItem.FieldOptions —
            // dasselbe Muster wie die Op-Arten am ingest-Gate). Übernehmen/Verfeinern EXISTIEREN hier gar nicht
            // als Wahl; die Optionen selbst sind die einzige Quelle der Item-Palette (kein Marker-Feld).
            FieldOptions = targetless
                ? new Dictionary<string, IReadOnlyList<ReviewOption>>
                {
                    [FieldDecision] =
                    [
                        new(ChoiceKeep, "Geklärt/erledigt — Begründung nennt die Antwort bzw. die beantwortende Anforderung (Pflicht)"),
                        new(ChoiceDefer, "Vertagen — bleibt offen und sichtbar geparkt")
                    ]
                }
                : new Dictionary<string, IReadOnlyList<ReviewOption>>(),
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, ""),
                new ReviewFieldValue(FieldStatement, v.ProposedStatement),   // Prefill = Meeting-Aussage
                new ReviewFieldValue(FieldReason, "")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static ReviewHelp BuildHelp() => new(
        Title: "Widersprüche auflösen (Tor 2)",
        Summary: "Ein Meeting hat der Projektwahrheit widersprochen. Hier entscheidest DU den Ausgang — nichts wird geraten, vertagen ist erlaubt.",
        Sections:
        [
            new ReviewHelpSection(
                "Was du hier tust",
                """
                • Je Widerspruch: bestehende Anforderung und Meeting-Aussage vergleichen.
                • Original behalten · Neues übernehmen · Verfeinern — oder Vertagen (Stakeholder nötig).
                • Bei Übernehmen/Verfeinern formulierst du den endgültigen Text (vorbefüllt mit der Meeting-Aussage).
                """),
            new ReviewHelpSection(
                "Verfeinern oder Übernehmen? — der genaue Unterschied",
                """
                • Daumenregel: Bedeutung BLEIBT (nur schärfer gesagt) → Verfeinern. Bedeutung KIPPT (Widerspruch bestätigt sich als Richtungswechsel) → Übernehmen.
                • Verfeinern = neue VERSION derselben ID: die Versionskette erzählt die Biografie EINER Wahrheit — sie darf präziser werden, aber sich nie selbst widersprechen.
                • Übernehmen = NEUE ID + supersedes-Kante: der Richtungswechsel steht als abfragbares Faktum im Graphen; die alte Anforderung behält IHRE Belege (die belegen ja die alte Aussage!), die neue bekommt ihre eigenen. So lügt die Beleg-Kette nie.
                • Die alte ID fällt bei Übernehmen aus allen Aktiv-Sichten (superseded), bleibt aber als Historie klickbar erhalten — nichts wird gelöscht.
                • Mechanisch ist BEIDES sicher (Relationen/Blockaden/Angleichung laufen korrekt) — die Wahl entscheidet, was die Historie später ehrlich erzählt.
                """),
            new ReviewHelpSection(
                "Woher kommen diese Widersprüche?",
                """
                • Heute aus genau EINER Stelle: dem Ingest-Gate. Dort hast DU eine Meeting-Aussage als echten Konflikt (CONTRADICT) bestätigt — die Wahrheit wurde dabei NICHT geändert, nur der Konflikt festgehalten und geschützt geparkt.
                • Die Herkunft steht an jedem Widerspruch („Woher stammt dieser Widerspruch?": Lauf + Meeting-Item).
                • Künftig kommt eine zweite Quelle dazu: der manuelle „→ Entscheidung"-Knopf für unauflösbare Klärungsfälle.
                """),
            new ReviewHelpSection(
                "Warum das sicher ist",
                """
                • Blockierte PBIs bleiben bis zur Auflösung gehalten — nichts Umstrittenes erreicht GitHub.
                • Ablösungen löschen nichts: alte Fassungen bleiben als Historie/superseded erhalten.
                • Vertagte Entscheidungen parken sichtbar und werden beim nächsten Lauf wieder vorgelegt.
                """),
            new ReviewHelpSection(
                "Wie es weitergeht (nach Fertig)",
                """
                • Deine Entscheide landen in decision-gate-decisions.json; „pipeline-full resume" wendet sie an.
                • Die Struktur passt sich sofort automatisch an (Deckungs-Swap, Entblocken, Historie).
                • Die INHALTS-Angleichung der betroffenen PBIs schlägt der Angleichungs-Agent im selben Lauf am
                  nächsten Gate (pbi-update) vor — dort entscheidest du erneut.
                • Danach Forward: entblockte Arbeit geht regulär nach GitHub (Update-Zyklus am bestehenden Issue).
                """),
            new ReviewHelpSection(
                "Alle vertagen (Experiment/Stakeholder)",
                "Der Sammel-Button vertagt alle offenen Widersprüche auf einmal — bewusst, wenn die Stakeholder-Antwort " +
                "fehlt. Nichts geht verloren: der Parkplatz ist sichtbar, der Block hält, die Wiedervorlage kommt automatisch.")
        ]);

    // Klartext-Kontext fuer die item:-Referenzen (Core wird vom Runner LIVE + read-only geladen): was ist das
    // Item HEUTE — damit der Mensch den Einfluss seiner Wahl direkt sieht (Vorher-Blick; das Nachher beschreibt
    // der Wirkungs-Text der gewaehlten Option).
    public static string RenderItemContext(string key, Delta.ProjectStateDocument core)
    {
        if (!key.StartsWith("item:", StringComparison.Ordinal)) return $"(Unbekannter Kontext: {key})";
        var id = key["item:".Length..];
        var it = core.Items.FirstOrDefault(x => string.Equals(x.ItemId, id, StringComparison.Ordinal));
        if (it is null) return $"({id} ist nicht im Core — evtl. Sandbox/anderer Stand)";

        if (string.Equals(it.ItemType, "pbi", StringComparison.OrdinalIgnoreCase) && it.Pbi is not null)
        {
            var covers = core.Relations
                .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal) && string.Equals(r.FromId, id, StringComparison.Ordinal))
                .Select(r => r.ToId).OrderBy(x => x, StringComparer.Ordinal).ToList();
            var aks = it.Pbi.AcceptanceCriteria.Count == 0
                ? "(keine)"
                : string.Join("\n", it.Pbi.AcceptanceCriteria.Select(a => $"• {a}"));
            return $"""
                    {id} — „{it.Pbi.Title}" (Status: {it.Status}, Version {it.Version})

                    Ziel: {it.Pbi.Goal ?? "(keins erfasst)"}
                    Deckt Anforderungen: {(covers.Count == 0 ? "(keine)" : string.Join(", ", covers))}

                    Akzeptanzkriterien:
                    {aks}
                    """;
        }

        var covering = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal) && string.Equals(r.ToId, id, StringComparison.Ordinal))
            .Select(r => r.FromId).OrderBy(x => x, StringComparer.Ordinal).ToList();
        return $"""
                {id} (Status: {it.Status}, Version {it.Version})

                „{it.Text}"

                Gedeckt durch: {(covering.Count == 0 ? "(kein PBI)" : string.Join(", ", covering))}
                """;
    }

    private static string ChoiceOf(PipelineDecisionResolution r)
        => string.Equals(r.Action, DecisionStage.ActionResolve, StringComparison.OrdinalIgnoreCase)
            ? r.Outcome switch
            {
                DecisionOutcome.KeepOriginal => ChoiceKeep,
                DecisionOutcome.AdoptNew => ChoiceAdopt,
                DecisionOutcome.Refine => ChoiceRefine,
                _ => ChoiceDefer,
            }
            : ChoiceDefer;

    private static string? ReasonOf(ReviewItem it) => FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null;
    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key);
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value);
}
