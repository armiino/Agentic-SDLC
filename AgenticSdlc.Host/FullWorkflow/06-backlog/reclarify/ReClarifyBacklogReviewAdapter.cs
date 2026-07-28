using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record BacklogHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<BacklogHumanDecision> Decisions);

public sealed record BacklogHumanDecision(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("editedPbiJson")] string? EditedPbiJson,
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die Product Backlog Items in die generische HumanReview-UI (per-Item, wie IssuePlanning).
// Der Mensch gibt PBIs frei, passt Akzeptanzkriterien/MVP/Prioritaet/Readiness an, oder verwirft/revidiert.
// Die vom Agenten aufgeworfenen offenen Entscheidungen sind sichtbar (blockierende hervorgehoben).
public static class ReClarifyBacklogReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldEditTitle = "editTitle";
    public const string FieldEditStatement = "editStatement";
    public const string FieldEditAcceptance = "editAcceptanceCriteria";
    public const string FieldEditMvp = "editMvp";
    public const string FieldEditPriority = "editPriorityRank";
    public const string FieldEditReadiness = "editReadiness";
    public const string FieldReason = "reason";

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly ReviewFieldVisibility OnlyOnEdit = new(FieldDecision, ["edit"]);
    // "revise" wird seit E0.1a nicht mehr angeboten (toter Pfad: Apply behandelte es wie reject);
    // bleibt hier toleriert, damit alte human-decisions.json/Replays weiter als entschieden gelten.
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "accept", "edit", "reject", "revise" };

    public static ReviewSession BuildSession(string runId, CanonicalRequirementsBaseline baseline, ProductBacklogDocument backlog, ReClarifyGateReport gate, string scope)
    {
        var byReq = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var items = backlog.Items
            .Where(p => Include(p, scope))
            .Select(p => BuildItem(p, byReq, gate))
            .ToList();

        return new ReviewSession
        {
            SessionId = $"l4-re-clarify-backlog-{runId}",
            Title = "L4 Re-Clarify — Product Backlog Review",
            Subtitle = scope == "blocked"
                ? $"{items.Count} PBIs mit blockierenden offenen Entscheidungen. Gate={(gate.Pass ? "pass" : "fail")}."
                : $"{items.Count} Product Backlog Items — je Item: akzeptieren, anpassen oder verwerfen. Gate={(gate.Pass ? "pass" : "fail")}.",
            Help = BuildHelp(),
            Notes = SessionNotes(),
            Glossary = Glossary(),
            BulkAction = new ReviewBulkAction(
                Label: "Accept all",
                Set:
                [
                    new ReviewFieldValue(FieldDecision, "accept"),
                    new ReviewFieldValue(FieldReason, "Sammel-Freigabe durch Reviewer (accept-all im UI).")
                ],
                Confirm: "{n} PBIs ohne Entscheid auf 'Akzeptieren' setzen? Bereits getroffene Entscheide bleiben unberuehrt; die Sammel-Freigabe wird als Begruendung protokolliert."),
            FieldSchema = Schema(),
            Items = items
        };
    }

    // E0.1g: die Wirkungskette IM UI — was nach „Fertig" mit jedem Entscheid passiert.
    private static IReadOnlyList<ReviewNote> SessionNotes() =>
    [
        new ReviewNote(ReviewNoteKind.Info, "Was bewirkt dein Entscheid?",
            "Akzeptieren ⇒ das PBI wird nach Apply + DoR-Gate Teil der Projekt-Wahrheit (Core) und beim GitHub-Sync ein Issue.\n"
            + "Anpassen ⇒ deine Fassung ersetzt das PBI (Version+1). Wirksamkeit: Readiness (steuert Forward-HOLD) > Titel/Statement (bis ins Issue) > Akzeptanzkriterien (Core + Issue-Body) > MVP/Rang (nur Doku).\n"
            + "Verwerfen ⇒ das PBI entfaellt; seine Requirements sind erstmal wieder ohne Verwendung (DoR-Gate meldet die Luecke, uncovered-requirements.json listet sie).\n"
            + "Begruendung ⇒ reines Audit-Protokoll (human-decisions.json + Core-History) — keine Anweisung ans System.")
    ];

    // E0.1m: Fach-Begriffe in Klartext — UI zeigt Tooltips + Glossar in der Hilfe. Wirkungs-Ehrlichkeit
    // (Autor-Frage 27.07.): NUR blockierende Fragen haben eine automatische Wirkung (Forward-HOLD);
    // alle anderen Begriffe sind Einordnung fuer den Menschen — jeder Eintrag sagt das explizit.
    private static IReadOnlyList<ReviewGlossaryEntry> Glossary() =>
    [
        new("backlog_ready", "DoR erfuellt — kann geplant werden, nichts steht im Weg."),
        new("ready_with_nonblocking_questions", "Umsetzbar — die offenen Fragen laufen nur als Doku mit, keine blockiert den Scope; Forward/Issue laufen normal."),
        new("blocked_by_decision", "WIRKT: der GitHub-Forward parkt das PBI (HOLD), bis die blockierende Frage geklaert ist."),
        new("BLOCKIERT den Scope", "WIRKT: diese Frage trifft den PBI-Kern — readiness wird blocked_by_decision, der GitHub-Forward parkt das PBI."),
        new("Team-Default moeglich", "Das Team kann selbst mit einem sinnvollen Default entscheiden. Nur Einordnung — keine automatische Wirkung."),
        new("Stakeholder muss entscheiden", "Braucht eine fachliche Entscheidung der Stakeholder. Nur Einordnung — keine automatische Wirkung."),
        new("teilweise geklaert", "Es gibt Teilbeleg im Transkript, aber keine vollstaendige Antwort. Nur Einordnung — keine automatische Wirkung."),
        new("Default vorgeschlagen", "Der Agent schlaegt einen Default vor — noch nicht autorisiert. Nur Einordnung — keine automatische Wirkung."),
        new("required_for_mvp", "Voraussetzung fuer das MVP. Nur Doku — kein Konsument in der Kette."),
        new("out_of_scope", "Bewusst ausserhalb des Scopes. ACHTUNG: nur Doku — das PBI wird trotzdem ein Issue, nichts filtert nach MVP."),
        new("undecided", "MVP-Einordnung noch offen. Nur Doku — kein Konsument in der Kette.")
    ];

    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        // E0.1b/i: Begruendung ist nur beim Verwerfen Pflicht (Audit-Gewicht); bei accept/edit ist der
        // explizite Entscheid selbst die Autorisierung — kein Tippzwang, keine Prefill-Falle mehr.
        if (string.Equals(decision, "reject", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        if (!string.Equals(decision, "edit", StringComparison.OrdinalIgnoreCase)) return true;
        return HasValidEditOverrides(item);
    }

    public static BacklogHumanDecisionsFile Apply(string runId, ReviewSession session, ProductBacklogDocument backlog)
    {
        var originals = backlog.Items.ToDictionary(i => i.PbiId, StringComparer.Ordinal);
        var decisions = session.Items.Select(item =>
        {
            string? V(string key) => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value;
            originals.TryGetValue(item.ItemId, out var original);
            return new BacklogHumanDecision(
                PbiId: item.ItemId,
                Decision: V(FieldDecision) ?? "",
                EditedPbiJson: string.Equals(V(FieldDecision), "edit", StringComparison.OrdinalIgnoreCase)
                    && TryBuildEditedPbi(item, original, out var edited)
                        ? JsonSerializer.Serialize(edited, Json)
                        : null,
                Reason: V(FieldReason));
        }).ToList();
        return new BacklogHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, BacklogHumanDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.PbiId, (item, decision) =>
        {
            Set(item, FieldDecision, decision.Decision);
            if (!string.IsNullOrWhiteSpace(decision.EditedPbiJson))
            {
                try
                {
                    var edited = JsonSerializer.Deserialize<ProductBacklogItem>(decision.EditedPbiJson, Json);
                    if (edited is not null) SetEditedFields(item, edited);
                }
                catch { Set(item, FieldEditStatement, decision.EditedPbiJson); }
            }
            Set(item, FieldReason, decision.Reason);
        }, Resolved);

    // E0.1f: Drilldowns loesen zu LESBAREM Text auf, nicht zu Roh-JSON (Vorbild: Schritt-1-Adjudikation).
    public static string ResolveContext(string resolverKey, CanonicalRequirementsBaseline baseline, ProductBacklogDocument backlog, ReClarifyGateReport gate)
    {
        if (resolverKey.StartsWith("pbi:", StringComparison.Ordinal))
        {
            var id = resolverKey["pbi:".Length..];
            var p = backlog.Items.FirstOrDefault(x => string.Equals(x.PbiId, id, StringComparison.Ordinal));
            return p is null ? $"(PBI {id} nicht gefunden)" : DescribePbi(p);
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var r = baseline.Requirements.FirstOrDefault(x => string.Equals(x.RequirementId, id, StringComparison.Ordinal));
            return r is null
                ? $"(Requirement {id} nicht gefunden)"
                : $"{r.RequirementId} — {r.Title}\n\n{r.Text}\n\nStatus: {r.Status} · Version {r.Version}\n"
                  + $"Herkunft: {r.OriginSummary}\nQuellen: {(r.SourceItemIds.Count == 0 ? "-" : string.Join(", ", r.SourceItemIds))}";
        }
        if (resolverKey.StartsWith("gate:", StringComparison.Ordinal))
        {
            var id = resolverKey["gate:".Length..];
            var issues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.SubjectId, id, StringComparison.Ordinal)).ToArray();
            return issues.Length == 0 ? "(keine Gate-Meldungen)" : string.Join("\n", issues.Select(i => $"- {i.Code}: {i.Message}"));
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static string DescribePbi(ProductBacklogItem p)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{p.PbiId} — {p.Title}  (v{p.Version}, {p.Type})");
        if (!string.IsNullOrWhiteSpace(p.Goal)) sb.AppendLine().AppendLine(p.Goal);
        sb.AppendLine();
        sb.AppendLine($"MVP: {p.Mvp ?? "-"} · Rank: {p.PriorityRank?.ToString() ?? "-"} · Readiness: {p.Readiness ?? "-"}");
        AppendList(sb, "Akzeptanzkriterien", p.AcceptanceCriteria);
        AppendList(sb, "In Scope", p.Scope.InScope);
        AppendList(sb, "Out of Scope", p.Scope.OutOfScope);
        AppendList(sb, "Offene Entscheidungen", p.OpenDecisions.Select(DescribeOpenDecision).ToList());
        AppendList(sb, "Abhaengigkeiten", p.Dependencies);
        sb.AppendLine($"Requirements: {(p.RequirementIds.Count == 0 ? "-" : string.Join(", ", p.RequirementIds))}");
        return sb.ToString();
    }

    private static void AppendList(System.Text.StringBuilder sb, string title, IReadOnlyList<string> lines)
    {
        if (lines.Count == 0) return;
        sb.AppendLine($"{title}:");
        foreach (var line in lines) sb.AppendLine($"- {line}");
    }

    // E0.1-Nachschärfung (Autor 27.07.): Klartext statt "[kind/resolution/BLOCKS]" — die Enum-Werte
    // bleiben in den Artefakten englisch, nur die Anzeige uebersetzt.
    private static string DescribeOpenDecisionShort(PbiOpenDecision o)
        => $"{KindLabel(o.Kind)}{(o.BlocksScope ? " · BLOCKIERT den Scope" : "")}: {o.Question}"
           + (string.IsNullOrWhiteSpace(o.ProposedResolution) ? "" : $" — Vorschlag: {o.ProposedResolution}");

    private static string DescribeOpenDecision(PbiOpenDecision o)
        => $"{KindLabel(o.Kind)} · {ResolutionLabel(o.Resolution)}{(o.BlocksScope ? " · BLOCKIERT den Scope" : "")}: {o.Question}"
           + (string.IsNullOrWhiteSpace(o.ProposedResolution) ? "" : $" — Vorschlag: {o.ProposedResolution}");

    private static string KindLabel(string kind) => kind switch
    {
        "engineering_default" => "Team-Default moeglich",
        "stakeholder_decision" => "Stakeholder muss entscheiden",
        _ => kind
    };

    private static string ResolutionLabel(string resolution) => resolution switch
    {
        "resolved" => "geklaert",
        "partial" => "teilweise geklaert",
        "proposed_default" => "Default vorgeschlagen",
        "open_decision" => "ungeklaert",
        "not_applicable" => "nicht relevant",
        _ => resolution
    };

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["accept", "edit", "reject"], Required: true,
            Help: "Was mit diesem PBI passiert — Details im Banner 'Was bewirkt dein Entscheid?'.",
            Options:
            [
                new ReviewOption("accept", "Akzeptieren — wird Wahrheit (Core) + GitHub-Issue"),
                new ReviewOption("edit", "Anpassen — Aenderungen werden uebernommen"),
                new ReviewOption("reject", "Verwerfen — PBI entfaellt, Requirements erstmal wieder nicht in Verwendung")
            ]),
        new ReviewFieldSpec(FieldEditTitle, "Edit: Titel", ReviewInputType.FreeText, [], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditStatement, "Edit: Statement", ReviewInputType.MultiLine, [], Required: false,
            Help: "Als <Rolle> will ich ... Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditAcceptance, "Edit: Akzeptanzkriterien", ReviewInputType.MultiLine, [], Required: false,
            Help: "Eine Zeile pro Kriterium. Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditMvp, "Edit: MVP", ReviewInputType.Dropdown,
            ["", "mvp", "required_for_mvp", "later", "out_of_scope", "undecided"], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditPriority, "Edit: priorityRank", ReviewInputType.FreeText, [], Required: false,
            Help: "Ganzzahl. Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldEditReadiness, "Edit: Readiness", ReviewInputType.Dropdown,
            ["", "backlog_ready", "ready_with_nonblocking_questions", "blocked_by_decision"], Required: false,
            Help: "Leer = Original.", VisibleWhen: OnlyOnEdit),
        new ReviewFieldSpec(FieldReason, "Begruendung (Audit-Protokoll)", ReviewInputType.MultiLine, [], Required: false,
            Help: "Warum du so entscheidest. Landet als Beleg in human-decisions.json + Core-History — "
                + "keine Anweisung ans System, niemand liest sie maschinell.")
    ];

    private static ReviewHelp BuildHelp() => new(
        Title: "L4 Re-Clarify Product Backlog Review",
        Summary: "Du gibst die aus den Feature-Clustern geschnittenen PBIs frei, bevor sie Projekt-Wahrheit werden und Issues entstehen.",
        Sections:
        [
            new ReviewHelpSection("Ziel",
                "Der Clarify-Agent hat jedes Feature in PBIs geschnitten (Akzeptanzkriterien + explizite offene Entscheidungen). "
                + "Du bestaetigst/korrigierst. Offene Entscheidungen sind sichtbar; blockierende sind hervorgehoben."),
            new ReviewHelpSection("Entscheidung",
                "Akzeptieren = PBI ist gut. Anpassen = Titel/Statement/Kriterien/MVP/Prioritaet/Readiness "
                + "aendern (nur ausgefuellte Felder ueberschreiben das Original). Verwerfen = PBI raus — Begruendung ist "
                + "dann Pflicht, und seine Requirements sind erstmal wieder ohne Verwendung."),
            new ReviewHelpSection("Offene Entscheidungen — Herkunft und Verbleib",
                "Sie entstehen beim Schneiden: jede Annahme ohne Transkript-Beleg wird als offene Frage markiert statt als "
                + "Fakt erfunden. Sie bleiben am PBI dokumentiert und werden (noch) NICHT zu eigenen Decision-Items. "
                + "Nicht-blockierende Fragen laufen nur als Doku mit; blockierende setzen readiness=blocked_by_decision — "
                + "der GitHub-Forward parkt das PBI dann, bis entschieden ist."),
            new ReviewHelpSection("Was passiert nach Fertig",
                "Die UI schreibt human-decisions.json. Der Apply materialisiert die uebernommenen/geaenderten PBIs als "
                + "ProductBacklogView, setzt die Traceability deterministisch neu und faehrt das DoR-Gate erneut. Danach "
                + "hebt core-seed-backlog die PBIs als Projekt-Wahrheit in den Core; der GitHub-Sync erzeugt daraus Issues.")
        ]);

    private static ReviewItem BuildItem(ProductBacklogItem p, IReadOnlyDictionary<string, CanonicalRequirement> byReq, ReClarifyGateReport gate)
    {
        var reqs = p.RequirementIds.Select(id => byReq.TryGetValue(id, out var r) ? $"{id}: {r.Title}" : $"{id}: (unbekannt)").ToArray();
        // Anzeige-Leitsatz (E0.9): nur was wirkt (BLOCKIERT), die Handlung informiert (wer entscheidet,
        // Vorschlag) oder warnt — die volle Klassifikation (resolution/evidence) bleibt im Drilldown/Artefakt.
        var open = p.OpenDecisions.Select(o => "- " + DescribeOpenDecisionShort(o)).ToArray();
        var gateIssues = gate.Errors.Concat(gate.Warnings).Where(i => string.Equals(i.SubjectId, p.PbiId, StringComparison.Ordinal)).ToArray();

        var notes = new List<ReviewNote>
        {
            // Readiness steht schon als Badge, type ist immer "pbi" — hier nur, was zusaetzlich traegt.
            new(ReviewNoteKind.Info, "Einordnung", $"MVP={p.Mvp ?? "-"}; Rang={p.PriorityRank?.ToString() ?? "-"}"),
            new(ReviewNoteKind.Reason, "Statement", p.Goal ?? "(kein Statement)"),
            new(ReviewNoteKind.Info, $"Akzeptanzkriterien ({p.AcceptanceCriteria.Count})", p.AcceptanceCriteria.Count == 0 ? "(keine)" : string.Join("\n", p.AcceptanceCriteria.Select(c => "- " + c))),
            new(p.OpenDecisions.Any(o => o.BlocksScope) ? ReviewNoteKind.Warning : ReviewNoteKind.Info,
                $"Offene Entscheidungen ({p.OpenDecisions.Count})",
                open.Length == 0
                    ? "(keine)"
                    : string.Join("\n", open)
                      + "\nHerkunft: vom Clarify-Agent als unbelegte Annahme markiert, statt sie als Fakt zu erfinden."
                      + "\nVerbleib: bleibt am PBI dokumentiert, wird (noch) kein eigenes Decision-Item; nicht-blockierende Fragen laufen nur mit — blockierende parken das PBI im GitHub-Forward."),
            new(ReviewNoteKind.Info, $"Abgedeckte Requirements ({reqs.Length})",
                (reqs.Length == 0 ? "(keine)" : string.Join("\n", reqs.Select(r => "- " + r)))
                + "\nDiese Deckung wird beim Core-Seed zur covers-Relation (Rueckverfolgbarkeit bis ins Issue)."
                + "\nBei Verwerfen sind diese Requirements erstmal wieder ohne Verwendung — das DoR-Gate meldet die Luecke.")
        };
        foreach (var gi in gateIssues)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, gi.Code, gi.Message));

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "PBI im Detail", $"pbi:{p.PbiId}") };
        foreach (var id in p.RequirementIds)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Requirement {id}", $"requirement:{id}"));

        var item = new ReviewItem
        {
            ItemId = p.PbiId,
            Summary = $"{p.Title}\n\n{Truncate(p.Goal ?? "", 500)}",
            Badge = p.Readiness ?? p.Type,
            Notes = notes,
            ContextBlocks = context,
            // E0.1b: KEIN Vorentscheid, KEIN Reason-Prefill — jedes Item startet offen; der Mensch
            // entscheidet aktiv (Sammel-Freigabe nur ueber den bestaetigungspflichtigen Bulk-Button).
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, ""),
                new ReviewFieldValue(FieldEditTitle, ""),
                new ReviewFieldValue(FieldEditStatement, ""),
                new ReviewFieldValue(FieldEditAcceptance, ""),
                new ReviewFieldValue(FieldEditMvp, ""),
                new ReviewFieldValue(FieldEditPriority, ""),
                new ReviewFieldValue(FieldEditReadiness, ""),
                new ReviewFieldValue(FieldReason, "")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    private static bool Include(ProductBacklogItem p, string scope)
        => scope switch
        {
            "blocked" => p.OpenDecisions.Any(o => o.BlocksScope) || string.Equals(p.Readiness, "blocked_by_decision", StringComparison.OrdinalIgnoreCase),
            _ => true
        };

    private static bool TryBuildEditedPbi(ReviewItem item, ProductBacklogItem? original, out ProductBacklogItem? edited)
    {
        edited = null;
        if (original is null) return false;
        var title = OverrideOrOriginal(FieldOf(item, FieldEditTitle), original.Title);
        if (string.IsNullOrWhiteSpace(title)) return false;
        int? rank = original.PriorityRank;
        var rankRaw = FieldOf(item, FieldEditPriority);
        if (!string.IsNullOrWhiteSpace(rankRaw) && int.TryParse(rankRaw, out var parsed)) rank = parsed;

        edited = new ProductBacklogItem(
            PbiId: original.PbiId,
            IdentityKey: original.IdentityKey,
            Version: original.Version + 1,
            Type: original.Type,
            Title: title,
            RequirementIds: original.RequirementIds)
        {
            Goal = OverrideOrOriginal(FieldOf(item, FieldEditStatement), original.Goal),
            Scope = original.Scope,
            AcceptanceCriteria = OverrideList(FieldOf(item, FieldEditAcceptance), original.AcceptanceCriteria),
            OpenDecisions = original.OpenDecisions,
            Mvp = OverrideOrOriginal(FieldOf(item, FieldEditMvp), original.Mvp),
            PriorityRank = rank,
            Dependencies = original.Dependencies,
            Readiness = OverrideOrOriginal(FieldOf(item, FieldEditReadiness), original.Readiness),
            Traceability = original.Traceability,
            Provenance = original.Provenance
        };
        return HasValidEditOverrides(item);
    }

    private static bool HasValidEditOverrides(ReviewItem item)
        => !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditTitle))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditStatement))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditAcceptance))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditMvp))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditPriority))
        || !string.IsNullOrWhiteSpace(FieldOf(item, FieldEditReadiness));

    private static void SetEditedFields(ReviewItem item, ProductBacklogItem edited)
    {
        Set(item, FieldEditTitle, edited.Title);
        Set(item, FieldEditStatement, edited.Goal);
        Set(item, FieldEditAcceptance, string.Join('\n', edited.AcceptanceCriteria));
        Set(item, FieldEditMvp, edited.Mvp);
        Set(item, FieldEditPriority, edited.PriorityRank?.ToString());
        Set(item, FieldEditReadiness, edited.Readiness);
    }

    private static IReadOnlyList<string> OverrideList(string raw, IReadOnlyList<string> original)
    {
        if (string.IsNullOrWhiteSpace(raw)) return original;
        return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static string? OverrideOrOriginal(string? overrideValue, string? originalValue)
        => string.IsNullOrWhiteSpace(overrideValue) ? originalValue : overrideValue.Trim();

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
