using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using System.Text;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Core;

public sealed record IngestionHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<IngestionHumanDecision> Decisions);

public sealed record IngestionHumanDecision(
    [property: JsonPropertyName("incomingItemId")] string IncomingItemId,
    [property: JsonPropertyName("decision")] string Decision,   // apply | skip
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die vom Resolver vorgeschlagenen Ingestion-OPERATIONEN in die generische HumanReview-UI:
// ein Item je Operation, Entscheidung apply/skip. Der Mensch autorisiert die Veraenderung der Wahrheit.
// Der IngestionApply fuehrt die akzeptierten Operationen deterministisch aus (Upsert-by-Identity).
//
// E0.7 (03.08.): Klartext statt roher apply/skip- und Kind-Keys; ehrlich ABGESTUFTE Wirkungs-Note (NEW/REFINE/
// SUPERSEDE/CONTRADICT ändern die Core-Wahrheit, RESTATE/ALREADY_DECIDED nur Provenance); leerer Default (aktive
// Entscheidung, keine Durchwink-Falle); deklarierte Experiment-Bulk-Linie; lesbarer Eingehend↔Core-Kontext statt
// rohem JSON. Apply-/Merge-Logik unverändert. E0.9-P2a (03.08.): skip verlangt eine Begründung (Resolved).
public static class IngestionReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";

    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private static readonly IReadOnlyList<ReviewOption> BaseDecisionOptions =
        [new("apply", "✓ Übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")];

    private const string GKinds = "Operationen — was mit der Anforderung passiert";
    private const string GTerms = "Begriffe";

    public static ReviewSession BuildSession(
        string runId,
        StateChangePlanDocument plan,
        ProjectStateDocument meetingDelta,
        ProjectStateDocument core)
    {
        var incomingById = meetingDelta.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var coreById = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        // Blast-Radius: welche PBIs decken welches Requirement ab (covers-Relation, ToId=Requirement, FromId=PBI).
        var pbisByReq = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal))
            .GroupBy(r => r.ToId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(r => r.FromId).Distinct(StringComparer.Ordinal).ToList(), StringComparer.Ordinal);
        var items = plan.Operations.Select(op => BuildItem(op, incomingById, coreById, pbisByReq)).ToList();

        return new ReviewSession
        {
            SessionId = $"ingestion-{runId}",
            Title = "Neue Meeting-Anforderungen — Core aktualisieren",
            Subtitle = plan.Operations.Count == 0
                ? "Keine Änderungen vorgeschlagen."
                : $"{plan.Operations.Count} vorgeschlagene Änderungen an der Projektwahrheit. Du bestätigst, was in den Core übernommen wird.",
            Glossary =
            [
                new ReviewGlossaryEntry("Neu (NEW / NEW_RELATED)", "Eine neue Anforderung — nichts Passendes im Core (NEW) bzw. fachlich neu im selben Feature (NEW_RELATED). WIRKT: legt eine neue REQ-ID an.", GKinds),
                new ReviewGlossaryEntry("Verfeinern (REFINE)", "Dieselbe Anforderung, konkretisiert. WIRKT: neue Version; die alte Fassung bleibt in der Historie.", GKinds),
                new ReviewGlossaryEntry("Ersetzen (SUPERSEDE)", "Eine alte Anforderung wird abgelöst. WIRKT: alt → superseded, neue REQ + supersedes-Relation.", GKinds),
                new ReviewGlossaryEntry("Widerspruch (CONTRADICT)", "Die Meldung widerspricht dem Core. WIRKT: legt eine offene Entscheidung (DEC) an — blockiert, bis ein Mensch entscheidet.", GKinds),
                new ReviewGlossaryEntry("Wiederholung (RESTATE)", "Schon bekannt/identisch. Nur ein Beleg (Provenance) an der bestehenden Anforderung — KEINE Wahrheitsänderung.", GKinds),
                new ReviewGlossaryEntry("Bereits entschieden (ALREADY_DECIDED)", "Schon als offene Entscheidung erfasst. No-op — nur Provenance-Vermerk, KEINE Wahrheitsänderung.", GKinds),
                new ReviewGlossaryEntry("superseded", "Der Status einer abgelösten Anforderung: nicht mehr gültig, bleibt aber als Historie erhalten.", GTerms),
                new ReviewGlossaryEntry("offene Entscheidung (DEC)", "Ein ungelöster Widerspruch/Klärungspunkt, der eine menschliche Entscheidung braucht (Tor 2).", GTerms)
            ],
            Help = BuildHelp(),
            FieldSchema = Schema(),
            BulkAction = new ReviewBulkAction(
                "✓ Alle übernehmen (Experiment — ohne Einzelprüfung)",
                [new ReviewFieldValue(FieldDecision, "apply")],
                "Alle noch offenen Änderungen OHNE Einzelprüfung in den Core übernehmen? Das kürzt die bewusste Autorisierung " +
                "jeder Wahrheits-Mutation ab (Experiment-Modus, wie accept-all/replay). Bereits gesetzte Entscheidungen bleiben unberührt."),
            Items = items
        };
    }

    // E0.9-P2a: Ablehnen braucht ein begründetes Warum (Audit-Symmetrie; ein apply erklärt sich durch den
    // Vorschlag selbst, ein skip nie) — Muster wie GithubForwardReviewAdapter.Resolved.
    public static bool Resolved(ReviewItem item)
        => ReviewFields.ResolvedRequiringReason(item, Decisions, FieldDecision, FieldReason, "skip"); // Basis-W2

    public static IngestionHumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(item => new IngestionHumanDecision(
            IncomingItemId: item.ItemId,
            Decision: FieldOf(item, FieldDecision),
            Reason: FieldOf(item, FieldReason) is { Length: > 0 } r ? r : null)).ToList();
        return new IngestionHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, IngestionHumanDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.IncomingItemId, (item, decision) =>
        {
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldReason, decision.Reason);
        }, Resolved);

    public static string ResolveContext(string resolverKey, StateChangePlanDocument plan, ProjectStateDocument meetingDelta, ProjectStateDocument core)
    {
        var incomingById = meetingDelta.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var coreById = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        if (resolverKey.StartsWith("op:", StringComparison.Ordinal))
        {
            var id = resolverKey["op:".Length..];
            var op = plan.Operations.FirstOrDefault(o => string.Equals(o.IncomingItemId, id, StringComparison.Ordinal));
            return op is null ? $"(Operation {id} nicht gefunden)" : RenderContext(op, incomingById, coreById);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["apply", "skip"], Required: true,
            Help: "Änderung in den Core übernehmen (deterministischer Apply) oder verwerfen.",
            Options: BaseDecisionOptions),
        new ReviewFieldSpec(FieldReason, "Begründung (nur Audit)", ReviewInputType.MultiLine, [], Required: false,
            Help: "PFLICHT beim Überspringen (warum weichst du vom Vorschlag ab?) — sonst optional. Landet als Beleg in human-decisions.json.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "Neue Meeting-Anforderungen in den Core",
        Summary: "Ein neues Meeting wurde gegen die Projektwahrheit (Core) aufgelöst. Du entscheidest je Änderung übernehmen/verwerfen.",
        Sections:
        [
            new ReviewHelpSection(
                "Was du hier tust",
                """
                Ein Resolver-Agent hat jede eingehende Anforderung des neuen Meetings gegen den Core geprüft und eine
                Operation vorgeschlagen. Du autorisierst jede (übernehmen) oder verwirfst sie (verwerfen) — beim Verwerfen ist eine kurze Begründung Pflicht (sie ist die einzige Spur der Ablehnung). Der Apply führt
                die akzeptierten Operationen deterministisch aus — die Wahrheit wird nie still überschrieben (alte Fassungen
                bleiben in der Historie, Widersprüche werden zu offenen Entscheidungen).
                """),
            new ReviewHelpSection(
                "Was WIRKT und was nur Beleg ist",
                """
                • Neu / Verfeinern / Ersetzen / Widerspruch: ändern die Core-Wahrheit (neue REQ, neue Version, Ablösung, offene Entscheidung).
                • Wiederholung / Bereits entschieden: nur Beleg (Provenance) — keine Wahrheitsänderung.
                Genau diese Abstufung steht als „Wirkung" an jedem Vorschlag.
                """),
            new ReviewHelpSection(
                "Alle übernehmen (Experiment)",
                "Der Sammel-Button übernimmt alle offenen Änderungen auf einmal — bewusst OHNE Einzelprüfung. Deklarierter " +
                "Experiment-Modus (wie accept-all/replay), NICHT der Normalweg an einem Wahrheits-Gate.")
        ]);

    private static ReviewItem BuildItem(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> incomingById, IReadOnlyDictionary<string, ProjectStateItem> coreById, IReadOnlyDictionary<string, IReadOnlyList<string>> pbisByReq)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Wirkung", KindEffect(op.Kind)),
            new(ReviewNoteKind.Reason, "Warum vorgeschlagen", string.IsNullOrWhiteSpace(op.Rationale) ? "(keine)" : op.Rationale)
        };
        if (IsTruthChanging(op.Kind))
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Ändert die Projektwahrheit",
                "Übernehmen mutiert den Core — nur bestätigen, wenn die Änderung stimmt."));
        var blast = BlastRadiusNote(op, coreById, pbisByReq);
        if (blast is not null) notes.Add(blast);

        var item = new ReviewItem
        {
            ItemId = op.IncomingItemId,
            Summary = Describe(op, incomingById, coreById),
            Badge = KindBadge(op.Kind),
            Notes = notes,
            FieldOptions = new Dictionary<string, IReadOnlyList<ReviewOption>> { [FieldDecision] = DecisionOptions(op.Kind) },
            ContextBlocks = [new ContextBlock(ContextBlockKind.Generic, "Eingehend → Core", $"op:{op.IncomingItemId}")],
            // Default bewusst LEER — jede Wahrheits-Mutation aktiv autorisieren (keine Durchwink-Falle).
            FieldValues = [new ReviewFieldValue(FieldDecision, ""), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // Blast-Radius: welche PBIs die Ziel-Anforderung abdecken → werden am NÄCHSTEN Gate (pbi-update) behandelt.
    // Ehrlicher Hinweis, KEINE Entscheidung hier (E0.9-Leitsatz). Bei SUPERSEDE zusätzlich der R-34-Hinweis
    // (die alte covers-Relation bleibt bis pbi-update bestehen).
    private static ReviewNote? BlastRadiusNote(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> coreById, IReadOnlyDictionary<string, IReadOnlyList<string>> pbisByReq)
    {
        if (op.Kind is StateChangeKind.New or StateChangeKind.NewRelated)
            return new ReviewNote(ReviewNoteKind.Info, "Nachgelagert",
                "Keine bestehende PBI-Abdeckung — wird am nächsten Gate (pbi-update) als neues/erweitertes PBI platziert.");

        if (string.IsNullOrWhiteSpace(op.TargetEntityId) || !pbisByReq.TryGetValue(op.TargetEntityId!, out var pbiIds) || pbiIds.Count == 0)
            return null;

        var labels = string.Join(", ", pbiIds.Select(id =>
            coreById.TryGetValue(id, out var p) ? $"{id} („{Truncate(p.Text, 50)}“)" : id));

        return op.Kind switch
        {
            StateChangeKind.Supersede => new ReviewNote(ReviewNoteKind.Warning, "Nachgelagert betroffen",
                $"{labels} decken die abzulösende Anforderung noch ab. Bis pbi-update läuft, bleibt die Verknüpfung bestehen; " +
                "das Ersetzen der PBIs wird am nächsten Gate (pbi-update) behandelt."),
            StateChangeKind.Contradict => new ReviewNote(ReviewNoteKind.Info, "Nachgelagert betroffen",
                $"{labels} decken diese Anforderung ab und werden am nächsten Gate (pbi-update) als blockiert markiert."),
            StateChangeKind.Refine => new ReviewNote(ReviewNoteKind.Info, "Nachgelagert betroffen",
                $"{labels} decken diese Anforderung ab und werden am nächsten Gate (pbi-update) angepasst."),
            // RESTATE/ALREADY_DECIDED: keine Wahrheitsänderung → die PBIs sind nicht betroffen (keine Note).
            _ => null
        };
    }

    // ---- Klartext-Helfer (reine Anzeige) ---------------------------------------------------------------

    private static bool IsTruthChanging(string kind) => kind switch
    {
        StateChangeKind.New or StateChangeKind.NewRelated or StateChangeKind.Refine
            or StateChangeKind.Supersede or StateChangeKind.Contradict => true,
        _ => false
    };

    private static string KindBadge(string kind) => kind switch
    {
        StateChangeKind.Restate => "Wiederholung",
        StateChangeKind.Refine => "Verfeinern",
        StateChangeKind.New => "Neu",
        StateChangeKind.NewRelated => "Neu im Feature",
        StateChangeKind.Supersede => "Ersetzen",
        StateChangeKind.Contradict => "Widerspruch",
        StateChangeKind.AlreadyDecided => "Bereits entschieden",
        _ => kind
    };

    private static string KindEffect(string kind) => kind switch
    {
        StateChangeKind.New => "Legt eine neue Anforderung im Core an (neue REQ-ID).",
        StateChangeKind.NewRelated => "Legt eine neue Anforderung im selben Feature an (neue REQ-ID).",
        StateChangeKind.Refine => "Erzeugt eine neue Version der bestehenden Anforderung — die alte bleibt in der Historie.",
        StateChangeKind.Supersede => "Löst die alte Anforderung ab (alt → superseded) und legt die neue an.",
        StateChangeKind.Contradict => "Legt eine offene Entscheidung (DEC) an — blockiert, bis ein Mensch entscheidet.",
        StateChangeKind.Restate => "Nur Beleg/Provenance an der bestehenden Anforderung — keine Wahrheitsänderung.",
        StateChangeKind.AlreadyDecided => "No-op — nur ein Provenance-Vermerk, keine Wahrheitsänderung.",
        _ => "—"
    };

    private static IReadOnlyList<ReviewOption> DecisionOptions(string kind) => kind switch
    {
        StateChangeKind.New or StateChangeKind.NewRelated => [new("apply", "✓ Neu anlegen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        StateChangeKind.Refine => [new("apply", "✓ Verfeinerung übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        StateChangeKind.Supersede => [new("apply", "✓ Ersetzen übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        StateChangeKind.Contradict => [new("apply", "✓ Als Widerspruch erfassen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        StateChangeKind.Restate => [new("apply", "✓ Beleg übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        StateChangeKind.AlreadyDecided => [new("apply", "✓ Vermerk übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        _ => BaseDecisionOptions
    };

    private static string Describe(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> incomingById, IReadOnlyDictionary<string, ProjectStateItem> coreById)
    {
        var inc = incomingById.TryGetValue(op.IncomingItemId, out var i) ? OneLine(i.Text) : OneLine(op.Statement);
        string Tgt() => op.TargetEntityId is not null && coreById.TryGetValue(op.TargetEntityId, out var t)
            ? $"{op.TargetEntityId} ({Truncate(t.Text, 60)})" : op.TargetEntityId ?? "?";
        return op.Kind switch
        {
            StateChangeKind.Restate => $"Wiederholung: {Truncate(inc, 90)} — schon bekannt als {Tgt()}",
            StateChangeKind.Refine => $"Verfeinern: {Tgt()} → {Truncate(inc, 90)}",
            StateChangeKind.NewRelated => $"Neu im Feature {op.FeatureKey}: {Truncate(inc, 90)}",
            StateChangeKind.New => $"Neu: {Truncate(inc, 100)}",
            StateChangeKind.Supersede => $"Ersetzen: {Tgt()} → {Truncate(inc, 90)}",
            StateChangeKind.Contradict => $"Widerspruch zu {Tgt()}: {Truncate(inc, 80)}",
            StateChangeKind.AlreadyDecided => $"Bereits entschieden: {Tgt()}",
            _ => $"{op.Kind}: {Truncate(inc, 80)}"
        };
    }

    // Lesbarer Eingehend↔Core-Kontext (statt rohem Operation-JSON). renderContextCards: Zeile mit „:" = Überschrift,
    // „>> " = Fokus-Karte, sonst Karte.
    private static string RenderContext(StateChangeOperation op, IReadOnlyDictionary<string, ProjectStateItem> incomingById, IReadOnlyDictionary<string, ProjectStateItem> coreById)
    {
        var sb = new StringBuilder();
        sb.Append($"{KindBadge(op.Kind)}:\n");
        sb.Append($">> {KindEffect(op.Kind)}\n");

        var incText = incomingById.TryGetValue(op.IncomingItemId, out var inc) ? OneLine(inc.Text) : OneLine(op.Statement);
        sb.Append("Eingehend (neu aus dem Meeting):\n");
        sb.Append($"{incText}\n");
        if (!string.IsNullOrWhiteSpace(op.FeatureKey))
            sb.Append($"Feature: {op.FeatureKey}\n");

        if (!string.IsNullOrWhiteSpace(op.TargetEntityId) && coreById.TryGetValue(op.TargetEntityId!, out var tgt))
        {
            sb.Append($"Bestehend im Core ({op.TargetEntityId}):\n");
            sb.Append($"{OneLine(tgt.Text)}\n");
        }

        sb.Append("Warum vorgeschlagen:\n");
        sb.Append(string.IsNullOrWhiteSpace(op.Rationale) ? "(keine Begründung)" : OneLine(op.Rationale));
        return sb.ToString();
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max) => ReviewFields.TruncateOneLine(value, max); // Basis-W2
    private static string OneLine(string value) => ReviewFields.OneLine(value); // Basis-W2
}
