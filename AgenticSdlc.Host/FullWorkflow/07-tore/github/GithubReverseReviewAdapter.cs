using AgenticSdlc.HumanReview;
using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Reverse-Plans. Zentraler Punkt (E4): bei PBI_DONE VERIFIZIERT der Mensch, dass die Arbeit
// wirklich fertig ist — apply setzt dann done, skip verwirft. Generische HumanReview-UI, wie github-forward-review.
//
// E0.5 (02.08.): Klartext statt roher apply/skip- und Kind-Keys; ehrliche Wirkungs-Kennzeichnung je Op-Art
// (PBI_DONE WIRKT, MAPPING_SYNC = Housekeeping, FLAG_REOPENED = nur Hinweis); PBI-Titel + „geschlossen“-Marker aus
// dem Core/Snapshot (via ReverseReviewContext), damit man beim Lesen weiss, WAS man als fertig bestaetigt und dass es
// um ein geschlossenes Issue geht; deklarierte Experiment-Bulk-Linie „Alle uebernehmen“ (nicht Default). Die Apply-/
// Merge-/Resolved-Logik ist UNVERAENDERT — nur Anzeige + Datenanbindung.
public static class GithubReverseReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    // Klartext-Optionen fuers Entscheidungs-Dropdown (Value = interner Key bleibt englisch; Label = Klartext).
    private static readonly IReadOnlyList<ReviewOption> BaseDecisionOptions =
        [new("apply", "✓ Übernehmen"), new("skip", "Nicht übernehmen")];

    public static ReviewSession BuildSession(string runId, GithubReversePlanDocument plan, ReverseReviewContext? context = null)
    {
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op, context)).ToList();
        return new ReviewSession
        {
            SessionId = $"github-reverse-{runId}",
            Title = "GitHub-Rückmeldung — Core aktualisieren (verifiziert)",
            Subtitle = plan.Operations.Count == 0
                ? "Keine Rückmeldungen aus GitHub."
                : $"{plan.Operations.Count} Vorschläge aus geschlossenen/geänderten Issues. Du bestätigst, was in den Core übernommen wird.",
            Glossary =
            [
                new ReviewGlossaryEntry("geschlossenes Issue", "Ein GitHub-Issue, das jemand auf „closed“ gesetzt hat. Das ist nur ein SIGNAL — es ändert den Core nicht automatisch."),
                new ReviewGlossaryEntry("PBI abschließen (Normalfall)", "Issue geschlossen, PBI noch aktiv → Vorschlag, das PBI im Core auf fertig (done) zu setzen. Wirkt nur nach deiner Bestätigung (E4). Schließt die Verknüpfung gleich mit."),
                new ReviewGlossaryEntry("Verknüpfung aufräumen (Ausnahme)", "Das PBI ist im Core schon abgeschlossen, nur die interne Issue↔PBI-Verknüpfung hängt noch auf „offen“. Wird hier geschlossen — keine PBI-Änderung. Taucht im sauberen Ablauf normal nicht auf."),
                new ReviewGlossaryEntry("Wieder geöffnet – prüfen (Ausnahme)", "Ein Issue ist wieder offen, obwohl das PBI/die Verknüpfung abgeschlossen ist. Nur ein Hinweis auf eine Abweichung — kein automatischer Change."),
                new ReviewGlossaryEntry("fertig (done)", "Der Core-Status, der bedeutet: die Arbeit an diesem PBI ist abgeschlossen. Entsteht AUSSCHLIESSLICH über ein hier bestätigtes „PBI abschließen“ — nie aus GitHub allein.")
            ],
            Help = new ReviewHelp("GitHub-Rückmeldung in den Core",
                "GitHub ist Projektion, nie Quelle. Ein geschlossenes Issue ist nur ein Vorschlag — der Core ändert sich erst durch deine Freigabe hier.",
                [
                    new ReviewHelpSection("Verifikation (E4)",
                        "Ein „PBI abschließen“ heißt NICHT „Issue zu = fertig“. Übernimm es NUR, wenn du bestätigst, dass die " +
                        "Arbeit wirklich abgeschlossen ist. Sonst „Nicht übernehmen“. Der Status „fertig“ entsteht ausschließlich über diese Freigabe."),
                    new ReviewHelpSection("Die drei Arten von Rückmeldungen",
                        "• PBI abschließen (Normalfall): setzt das PBI auf fertig (done) und schließt die Verknüpfung mit. Das verifizierst du.\n" +
                        "• Verknüpfung aufräumen (Ausnahme): PBI war schon fertig, nur die Verknüpfung hängt noch offen — wird geschlossen, keine PBI-Änderung.\n" +
                        "• Wieder geöffnet – prüfen (Ausnahme): ein abgeschlossenes Issue ist wieder offen — nur Hinweis, kein automatischer Change."),
                    new ReviewHelpSection("Alle übernehmen (Experiment)",
                        "Der Sammel-Button übernimmt alle noch offenen Vorschläge auf einmal — bewusst OHNE Einzelprüfung. Das kürzt die " +
                        "E4-Verifikation ab und ist deshalb ein deklarierter Experiment-Modus (wie accept-all/replay), NICHT der Normalweg.")
                ]),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "Übernehmen (bei „PBI abschließen“ = fertig verifiziert) oder nicht übernehmen.",
                    Options: BaseDecisionOptions),
                new ReviewFieldSpec(FieldReason, "Begründung (nur Audit)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Optionale Notiz — wird protokolliert, ändert aber nichts am Core.")
            ],
            // Deklarierte Experiment-Bulk-Linie: alle noch offenen Vorschläge auf apply setzen (kein Default, Confirm-Dialog).
            // Das ist die MANUELLE Vorstufe der späteren Steward-Auto-Policy (s. plan-project-steward §5.1 / E-E).
            BulkAction = new ReviewBulkAction(
                "✓ Alle übernehmen (Experiment — ohne Einzelprüfung)",
                [new ReviewFieldValue(FieldDecision, "apply")],
                "Alle noch offenen Vorschläge OHNE Einzelprüfung übernehmen? Ein „PBI abschließen“ setzt sein PBI auf fertig. " +
                "Das kürzt die E4-Verifikation ab (Experiment-Modus). Bereits gesetzte Entscheidungen bleiben unberührt."),
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item) => Decisions.Contains(FieldOf(item, FieldDecision));

    public static GithubReverseDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new GithubReverseDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, GithubReverseDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) => { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); }, Resolved);

    public static string ResolveContext(string key, GithubReversePlanDocument plan, ReverseReviewContext? context = null)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx) && idx >= 0 && idx < plan.Operations.Count)
            return RenderContext(plan.Operations[idx], context);
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewItem BuildItem(string opId, int idx, GithubReverseOp op, ReverseReviewContext? context)
    {
        var pbiTitle = PbiTitle(op, context);
        var issueState = IssueStateLabel(op, context);
        var title = pbiTitle is null ? op.PbiId : $"„{pbiTitle}“ ({op.PbiId})";

        var summary = op.Kind switch
        {
            GithubReverseKind.PbiDone => $"Fertig? {title} · Issue #{op.IssueNumber} {issueState} — {PbiStatusLabel(op.CurrentPbiStatus)} → fertig",
            GithubReverseKind.MappingSyncClosed => $"Aufräumen: {title} · Issue #{op.IssueNumber} {issueState} — nur Verknüpfung schließen",
            GithubReverseKind.FlagReopened => $"Wieder offen: {title} · Issue #{op.IssueNumber} {issueState} — bitte prüfen",
            _ => $"{op.Kind} {op.PbiId}"
        };

        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Wirkung", KindEffect(op.Kind)),
            new(ReviewNoteKind.Reason, "Warum vorgeschlagen", op.Rationale)
        };
        if (context is not null && context.IssuesByNumber.TryGetValue(op.IssueNumber, out var issue))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "GitHub-Issue", $"#{op.IssueNumber} „{issue.Title}“ · Status: {StateLabel(issue.State)}"));
        if (op.RequiresVerification)
            notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Verifikation nötig (E4)",
                "„Fertig“ nur übernehmen, wenn die Arbeit wirklich abgeschlossen ist — nicht bloß weil das Issue zu ist."));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = KindBadge(op.Kind),
            Notes = notes,
            // Per-Item-Klartext fürs Dropdown je Op-Art (überschreibt die Basis-Options nur für dieses Item).
            FieldOptions = new Dictionary<string, IReadOnlyList<ReviewOption>> { [FieldDecision] = DecisionOptions(op.Kind) },
            ContextBlocks = [new ContextBlock(ContextBlockKind.Generic, "Details anzeigen", $"op:{idx}")],
            // Default bewusst NICHT apply bei verifikationspflichtigen Ops — der Mensch muss aktiv entscheiden.
            FieldValues = [new ReviewFieldValue(FieldDecision, ""), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // ---- Klartext-Helfer (reine Anzeige) ---------------------------------------------------------------

    private static string KindBadge(string kind) => kind switch
    {
        GithubReverseKind.PbiDone => "PBI abschließen",
        GithubReverseKind.MappingSyncClosed => "Verknüpfung aufräumen",
        GithubReverseKind.FlagReopened => "Wieder geöffnet – prüfen",
        _ => kind
    };

    private static string KindEffect(string kind) => kind switch
    {
        GithubReverseKind.PbiDone => "Setzt das PBI im Core auf fertig (done).",
        GithubReverseKind.MappingSyncClosed => "Räumt nur die interne Issue↔PBI-Verknüpfung auf — keine PBI-Änderung.",
        GithubReverseKind.FlagReopened => "Issue ist erneut offen; nur ein Hinweis auf eine Abweichung — kein automatischer Change.",
        _ => "—"
    };

    private static IReadOnlyList<ReviewOption> DecisionOptions(string kind) => kind switch
    {
        GithubReverseKind.PbiDone => [new("apply", "✓ Als fertig bestätigen"), new("skip", "Nicht übernehmen (offen lassen)")],
        GithubReverseKind.MappingSyncClosed => [new("apply", "✓ Aufräumen übernehmen"), new("skip", "Nicht übernehmen")],
        GithubReverseKind.FlagReopened => [new("apply", "✓ Als gesehen markieren"), new("skip", "Ignorieren")],
        _ => BaseDecisionOptions
    };

    private static string? PbiTitle(GithubReverseOp op, ReverseReviewContext? context)
        => context is not null && context.PbiTitles.TryGetValue(op.PbiId, out var t) && !string.IsNullOrWhiteSpace(t) ? t : null;

    // „geschlossen“/„wieder offen“: bevorzugt der echte Snapshot-Zustand; sonst deterministisch aus der Op-Art abgeleitet
    // (PBI_DONE/MAPPING entstehen nur bei geschlossenem Issue, FLAG nur bei wieder geöffnetem — s. GithubReverseSeed).
    private static string IssueStateLabel(GithubReverseOp op, ReverseReviewContext? context)
    {
        if (context is not null && context.IssuesByNumber.TryGetValue(op.IssueNumber, out var issue))
            return StateLabel(issue.State);
        return op.Kind == GithubReverseKind.FlagReopened ? "(wieder offen)" : "(geschlossen)";
    }

    private static string StateLabel(string state)
        => state.Equals("closed", StringComparison.OrdinalIgnoreCase) ? "(geschlossen)"
         : state.Equals("open", StringComparison.OrdinalIgnoreCase) ? "(offen)"
         : $"({state})";

    private static string PbiStatusLabel(string status) => status switch
    {
        "active" => "aktiv/in Arbeit",
        "baseline" => "Baseline",
        "accepted" => "angenommen",
        "needs_clarify" => "Klärung nötig",
        "open_decision" => "offene Entscheidung",
        "blocked_by_decision" => "durch Entscheidung blockiert",
        "done" => "fertig",
        "superseded" => "abgelöst",
        "resolved" => "aufgelöst",
        "" => "—",
        _ => status
    };

    // Lesbarer Detail-Kontext (statt rohem Operation-JSON). renderContextCards rendert Zeilen mit „:“ als Überschrift,
    // „>> “ als Fokus-Karte, sonst als Karte.
    private static string RenderContext(GithubReverseOp op, ReverseReviewContext? context)
    {
        var sb = new StringBuilder();
        sb.Append("Art:\n");
        sb.Append($">> {KindBadge(op.Kind)} — {KindEffect(op.Kind)}\n");

        sb.Append("PBI:\n");
        var title = PbiTitle(op, context);
        sb.Append(title is null ? $"{op.PbiId}\n" : $"{op.PbiId} — {title}\n");
        if (op.Kind == GithubReverseKind.PbiDone)
            sb.Append($"Aktueller Status: {PbiStatusLabel(op.CurrentPbiStatus)}  →  Vorschlag: fertig (done)\n");
        else
            sb.Append($"Aktueller Status: {PbiStatusLabel(op.CurrentPbiStatus)}\n");

        sb.Append("GitHub-Issue:\n");
        if (context is not null && context.IssuesByNumber.TryGetValue(op.IssueNumber, out var issue))
        {
            sb.Append($"#{op.IssueNumber} — {issue.Title}\n");
            sb.Append($"Status: {StateLabel(issue.State)}\n");
            if (!string.IsNullOrWhiteSpace(issue.Url)) sb.Append($"{issue.Url}\n");
        }
        else
        {
            sb.Append($"#{op.IssueNumber} {IssueStateLabel(op, context)}\n");
        }

        sb.Append("Warum vorgeschlagen:\n");
        sb.Append(op.Rationale);
        return sb.ToString();
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}

// E0.5 — optionaler Anzeige-Kontext: PBI-Titel aus dem Core + Issue-Titel/-Zustand aus dem Snapshot. Rein additiv;
// null = das Gate funktioniert wie zuvor (nur IDs). Kein Einfluss auf Entscheidung/Apply.
public sealed record ReverseReviewContext(
    IReadOnlyDictionary<string, string> PbiTitles,
    IReadOnlyDictionary<int, GithubIssueSnapshot> IssuesByNumber);
