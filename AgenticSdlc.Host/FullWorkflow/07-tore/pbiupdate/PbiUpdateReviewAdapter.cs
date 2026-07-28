using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

public sealed record PbiUpdateDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<PbiUpdateDecision> Decisions);

public sealed record PbiUpdateDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
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
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private const int TextPreviewChars = 500;

    public static ReviewSession BuildSession(string runId, PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op, byId)).ToList();
        return new ReviewSession
        {
            SessionId = $"pbi-update-{runId}",
            Title = "Incrementeller PBI-Update — Backlog-Aenderungen",
            Subtitle = plan.Operations.Count == 0
                ? "Keine PBI-Aenderungen."
                : $"{plan.Operations.Count} Backlog-Operationen aus dem letzten Meeting — je Op: uebernehmen oder ueberspringen.",
            Help = BuildHelp(),
            Notes = SessionNotes(),
            Glossary = Glossary(),
            BulkAction = new ReviewBulkAction(
                Label: "Accept all",
                Set:
                [
                    new ReviewFieldValue(FieldDecision, "apply"),
                    new ReviewFieldValue(FieldReason, "Sammel-Freigabe durch Reviewer (accept-all im UI).")
                ],
                Confirm: "{n} Operationen ohne Entscheid auf 'Uebernehmen' setzen? ACHTUNG: apply aendert die "
                    + "Projektwahrheit (Core) und speist danach den GitHub-Sync. Bereits getroffene Entscheide bleiben unberuehrt."),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "Was mit dieser Aenderung passiert — Details im Banner 'Was bewirkt dein Entscheid?'.",
                    Options:
                    [
                        new ReviewOption("apply", "Uebernehmen — aendert die Projektwahrheit (Core)"),
                        new ReviewOption("skip", "Ueberspringen — Aenderung verwerfen (Begruendung Pflicht)")
                    ]),
                new ReviewFieldSpec(FieldReason, "Begruendung (Audit-Protokoll)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Pflicht beim Ueberspringen (warum weicht der Mensch vom Vorschlag ab?). Landet als Beleg in "
                        + "human-decisions.json — keine Anweisung ans System.")
            ],
            Items = items
        };
    }

    // E0.3: Items starten offen; Begruendung nur beim skip Pflicht (Abweichung vom Vorschlag dokumentieren).
    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        if (string.Equals(decision, "skip", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        return true;
    }

    public static PbiUpdateDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new PbiUpdateDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, PbiUpdateDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) => { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); }, Resolved);

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

    private static ReviewHelp BuildHelp() => new(
        "Incrementeller PBI-Update",
        "Ein neues Meeting hat die Projektwahrheit geaendert. Hier autorisierst du, wie die betroffenen Backlog-Items "
        + "(PBIs) darauf reagieren — bevor daraus ein GitHub-Sync entsteht.",
        [
            new ReviewHelpSection("Woher kommen diese Operationen?",
                "Sie werden regelbasiert aus dem Meeting-Delta abgeleitet: ein verfeinertes Requirement -> MARK_CHANGED, "
                + "ein Widerspruch -> BLOCK_PBI, ein Ersatz -> SUPERSEDE_PBI, ein neues Requirement -> NEW_PBI/EXTEND_PBI. "
                + "Kein LLM entscheidet hier den Op-Typ."),
            new ReviewHelpSection("Was passiert nach Fertig",
                "Die UI schreibt human-decisions.json. pbi-update-apply fuehrt die uebernommenen Operationen deterministisch "
                + "aus, aendert den Core (Status/Deckung der PBIs) und schreibt ein github-sync-Delta -> naechstes Gate (Forward)."),
            new ReviewHelpSection("Wichtig: needs_clarify wird hier nur GESETZT, nicht aufgeloest",
                "MARK_CHANGED und SUPERSEDE markieren ein PBI als 'geaendert, aber inhaltlich noch ungeklaert' — Titel und "
                + "Akzeptanzkriterien sind dann NICHT automatisch an die neue Anforderung angeglichen. Ein Schritt, der das "
                + "tut, fehlt aktuell in der Kette (R-26-C). Bis dahin: bewusst uebernehmen und die Klaerung im Blick behalten.")
        ]);

    // E0.3e: die Wirkungskette IM UI — in Sprache, die auch Projektfremde verstehen.
    private static IReadOnlyList<ReviewNote> SessionNotes() =>
    [
        new ReviewNote(ReviewNoteKind.Info, "Was bewirkt dein Entscheid?",
            "Uebernehmen ⇒ die Aenderung wird Teil der Projektwahrheit (Core) und fliesst danach in den GitHub-Sync. "
            + "Was genau: neues Backlog-Item anlegen, Anforderung zuordnen, Item als geaendert/blockiert markieren oder "
            + "eine Anforderung ersetzen.\n"
            + "Ueberspringen ⇒ die Aenderung wird nicht uebernommen, der Core bleibt wie er ist. Bitte kurz begruenden.\n"
            + "Markierungen (needs_clarify, blocked_by_decision) ⇒ das Item ist dann vorgemerkt, aber inhaltlich noch "
            + "offen — die eigentliche Klaerung ist ein spaeterer Schritt.")
    ];

    // E0.3e: Fach-Begriffe in Klartext mit Wirkungs-Ehrlichkeit (Muster wie Forward-Review).
    private static IReadOnlyList<ReviewGlossaryEntry> Glossary() =>
    [
        new("NEW_PBI", "WIRKT: legt ein neues Backlog-Item (PBI) fuer eine neue Anforderung an."),
        new("EXTEND_PBI", "WIRKT: ordnet einem bestehenden PBI zusaetzlich eine Anforderung zu."),
        new("MARK_CHANGED", "WIRKT: markiert das PBI als 'geaendert, ungeklaert' (needs_clarify), weil seine Anforderung verfeinert wurde."),
        new("BLOCK_PBI", "WIRKT: setzt das PBI auf 'blockiert' (blocked_by_decision) — es wartet auf eine offene Entscheidung."),
        new("SUPERSEDE_PBI", "WIRKT: tauscht eine abgeloeste Anforderung gegen ihren Ersatz aus (und markiert das PBI als ungeklaert)."),
        new("needs_clarify", "Interner PBI-Zustand: geaendert, aber Inhalt noch nicht an die neue Anforderung angeglichen."),
        new("blocked_by_decision", "Interner PBI-Zustand: wartet auf eine Entscheidung — es entsteht (noch) kein GitHub-Issue."),
        new("verfeinert", "Die Anforderung wurde inhaltlich praezisiert — eine neue Fassung ersetzt die alte (Vorher/Nachher unten).")
    ];

    private static ReviewItem BuildItem(string opId, int idx, PbiStateChangeOperation op,
        IReadOnlyDictionary<string, ProjectStateItem> byId)
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
            _ => $"{op.Kind} {op.PbiId}"
        };

        var notes = new List<ReviewNote>();

        // Betroffenes PBI (aktueller Inhalt) — bei allen ausser NEW_PBI.
        if (pbi is not null)
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Betroffenes PBI ({pbi.ItemId}, {pbi.Status})",
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
                notes.Add(NeedsClarifyNote(op.RequirementId));
                break;
            }
            case PbiUpdateKind.SupersedePbi:
            {
                var repl = op.ReplacementRequirementId is not null ? byId.GetValueOrDefault(op.ReplacementRequirementId) : null;
                notes.Add(new ReviewNote(ReviewNoteKind.Info,
                    $"Anforderungs-Ersatz ({op.RequirementId} → {op.ReplacementRequirementId})",
                    $"− {Truncate(req?.Text ?? op.RequirementId, TextPreviewChars)}\n+ {Truncate(repl?.Text ?? op.ReplacementRequirementId ?? "?", TextPreviewChars)}"));
                notes.Add(NeedsClarifyNote(op.ReplacementRequirementId ?? op.RequirementId));
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
            case PbiUpdateKind.ExtendPbi:
                notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, $"Zusaetzliche Anforderung {op.RequirementId}", ReqBody(req)));
                break;
        }

        // E0.9-Leitsatz: die Platzierungs-Begruendung ist NUR bei NEW/EXTEND handlungsleitend — dort entscheidet
        // ein Agent, WOHIN eine neue Anforderung gehoert (eigenes PBI vs. Erweiterung), und begruendet es. Bei den
        // deterministischen Kinds (MARK/BLOCK/SUPERSEDE) ist die Rationale ein Regel-Leersatz -> nicht zeigen.
        if (PbiUpdateKind.Placement.Contains(op.Kind) && !string.IsNullOrWhiteSpace(op.Rationale))
            notes.Add(new ReviewNote(ReviewNoteKind.Reason, "Warum diese Zuordnung? (Vorschlag des Platzierungs-Agenten)", op.Rationale));

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Operation im Detail", $"op:{idx}") };
        if (op.PbiId is not null) context.Add(new ContextBlock(ContextBlockKind.Reference, $"PBI {op.PbiId} komplett", $"pbi:{op.PbiId}"));
        context.Add(new ContextBlock(ContextBlockKind.Reference, $"Anforderung {op.RequirementId}", $"requirement:{op.RequirementId}"));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = op.Kind,
            Notes = notes,
            ContextBlocks = context,
            // E0.3c: kein Vorentscheid — der Mensch entscheidet aktiv (Accept all nur ueber den Bestaetigungs-Button).
            FieldValues = [new ReviewFieldValue(FieldDecision, ""), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // Der ehrliche R-26-Hinweis am Item — verbindet dieses Gate mit der offenen Klaerungs-Luecke.
    private static ReviewNote NeedsClarifyNote(string reqId) => new(ReviewNoteKind.Warning,
        "Danach: needs_clarify",
        "Uebernehmen setzt das PBI auf 'geaendert, ungeklaert'. Titel und Akzeptanzkriterien sind dann NICHT an die "
        + $"neue Fassung von {reqId} angeglichen — dieser Angleichungs-Schritt fehlt aktuell in der Kette (R-26-C). "
        + "Bis dahin bleibt die inhaltliche Klaerung eine offene Aufgabe.");

    private static string DescribeOp(PbiStateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> byId)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{op.Kind}");
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
        sb.AppendLine($"{p.ItemId} — {p.Pbi?.Title ?? p.Text}  ({p.Status})");
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
        sb.AppendLine($"{r.ItemId} ({r.Status}, v{r.Version})");
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

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max] + " …";

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
