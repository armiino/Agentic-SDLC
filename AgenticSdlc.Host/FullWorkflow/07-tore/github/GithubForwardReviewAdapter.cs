using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// HumanReview des Forward-Plans (apply/skip je Op). Der Mensch autorisiert den PLAN — der GitHub-Write selbst
// passiert erst im gated Apply (T3.4). Generische HumanReview-UI, wie pbi-update-review / ingest-review.
// E0.2 a–g (27.07.): UPDATE zeigt Vorher/Nachher (Snapshot vs. Vorschlag), CREATE zeigt den Body, Default leer
// (Begruendung bei skip Pflicht), lesbare Drilldowns, Glossar + Wirkungs-Banner, Titel in den Summaries,
// Accept-all mit Warntext. Folge-Logik (Entscheid-Format, Apply, Gate) unveraendert.
public static class GithubForwardReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };
    private const int BodyPreviewChars = 700;

    public static ReviewSession BuildSession(string runId, GithubForwardPlanDocument plan,
        IReadOnlyDictionary<int, GithubIssueSnapshot>? issuesByNumber = null)
    {
        var items = plan.Operations.Select((op, i) => BuildItem($"op-{i}", i, op, issuesByNumber)).ToList();
        return new ReviewSession
        {
            SessionId = $"github-forward-{runId}",
            Title = "GitHub Forward-Reconciliation — Delta gegen GitHub",
            Subtitle = plan.Operations.Count == 0
                ? "Keine Operationen."
                : $"{plan.Operations.Count} Operationen — je Op: ausfuehren oder ueberspringen. Kein GitHub-Write vor dem gated Apply.",
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
                Confirm: "{n} Operationen ohne Entscheid auf 'Ausfuehren' setzen? ACHTUNG: der gated Apply fuehrt "
                    + "diese Ops gegen GitHub aus (extern!), sobald execute aktiv ist. Bereits getroffene Entscheide "
                    + "bleiben unberuehrt."),
            FieldSchema =
            [
                new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown, ["apply", "skip"], Required: true,
                    Help: "Was mit dieser Operation passiert — Details im Banner 'Was bewirkt dein Entscheid?'.",
                    Options:
                    [
                        new ReviewOption("apply", "Ausfuehren — Op wird beim gated Apply gegen GitHub ausgefuehrt"),
                        new ReviewOption("skip", "Ueberspringen — Op wird nicht ausgefuehrt (Begruendung Pflicht)")
                    ]),
                new ReviewFieldSpec(FieldReason, "Begruendung (Audit-Protokoll)", ReviewInputType.MultiLine, [], Required: false,
                    Help: "Pflicht beim Ueberspringen (warum weicht der Mensch vom Plan ab?). Landet als Beleg in "
                        + "human-decisions.json — keine Anweisung ans System.")
            ],
            Items = items
        };
    }

    // E0.2c: Items starten offen; Begruendung nur beim skip Pflicht (Abweichung vom Plan dokumentieren) —
    // beim apply ist der explizite Entscheid selbst die Autorisierung.
    public static bool Resolved(ReviewItem item)
    {
        var decision = FieldOf(item, FieldDecision);
        if (!Decisions.Contains(decision)) return false;
        if (string.Equals(decision, "skip", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(FieldOf(item, FieldReason))) return false;
        return true;
    }

    public static GithubForwardDecisionsFile Apply(string runId, ReviewSession session)
        => new(runId, "human (review-ui)", session.Items.Select(it => new GithubForwardDecision(
            it.ItemId, FieldOf(it, FieldDecision), FieldOf(it, FieldReason) is { Length: > 0 } r ? r : null)).ToList());

    public static void MergeExistingDecisions(ReviewSession session, GithubForwardDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId,
            (item, d) => { Set(item, FieldDecision, d.Decision); Set(item, FieldReason, d.Reason); }, Resolved);

    // E0.2d: Drilldowns loesen zu LESBAREM Text auf (Muster: DescribePbi aus dem Backlog-Review).
    public static string ResolveContext(string key, GithubForwardPlanDocument plan,
        IReadOnlyDictionary<int, GithubIssueSnapshot>? issuesByNumber = null)
    {
        if (key.StartsWith("op:", StringComparison.Ordinal) && int.TryParse(key["op:".Length..], out var idx)
            && idx >= 0 && idx < plan.Operations.Count)
            return DescribeOp(plan.Operations[idx]);
        if (key.StartsWith("issue:", StringComparison.Ordinal) && int.TryParse(key["issue:".Length..], out var nr))
            return issuesByNumber is not null && issuesByNumber.TryGetValue(nr, out var issue)
                ? DescribeIssue(issue)
                : $"(Issue #{nr} nicht im Snapshot dieses Laufs)";
        return $"(Unbekannter Kontext: {key})";
    }

    private static ReviewHelp BuildHelp() => new(
        "GitHub Forward-Reconciliation",
        "Der github-sync-Delta wird gegen GitHub abgeglichen. Du autorisierst den PLAN — geschrieben wird erst im gated Apply.",
        [
            new ReviewHelpSection("Woher kommt der Plan?",
                "Deterministische Ops (origin=deterministic) entstehen regelbasiert aus Core-Mapping + Issue-Snapshot; "
                + "agentische Ops (origin=agent) aus der Suche des Forward-Agenten (LINK oder CREATE, mit Such-Evidenz — "
                + "das Gate lehnt CREATE ohne Suche ab)."),
            new ReviewHelpSection("Was passiert nach Fertig",
                "Die UI schreibt human-decisions.json. Der gated Apply (T3.4) fuehrt NUR die ausfuehren-Ops aus: "
                + "CREATE/UPDATE/COMMENT schreiben nach GitHub (EXTERN — einziges Gate mit Aussenwirkung!), LINK schreibt "
                + "nur das Mapping (implemented_by_issue) in den Core. Default ist Dry-Run; real geschrieben wird nur mit "
                + "expliziter execute-Policy."),
            new ReviewHelpSection("Vorher/Nachher bei UPDATE",
                "'Issue AKTUELL' kommt aus dem Snapshot des Laufs, 'VORSCHLAG NEU' ist der Body aus dem Plan — genau das, "
                + "was beim Ausfuehren im Issue stehen wuerde. Vergleiche beide, bevor du ausfuehrst (R-23).")
        ]);

    // E0.2e: die Wirkungskette IM UI — in Sprache, die auch Projektfremde verstehen (Autor-Nachschärfung).
    private static IReadOnlyList<ReviewNote> SessionNotes() =>
    [
        new ReviewNote(ReviewNoteKind.Info, "Was bewirkt dein Entscheid?",
            "Ausfuehren ⇒ die Aenderung wird im naechsten Schritt wirklich gemacht (Issue anlegen, aendern oder "
            + "kommentieren). Solange der Testmodus (Dry-Run) aktiv ist, wird dabei noch nichts nach GitHub geschrieben.\n"
            + "Ueberspringen ⇒ es passiert nichts, alles bleibt wie es ist. Bitte kurz begruenden, warum.\n"
            + "Hinweis-Zeilen (FLAG_DRIFT, HOLD_…, NO_CHANGE) ⇒ nur Information — dort passiert unabhaengig vom "
            + "Entscheid nie etwas.")
    ];

    // E0.2e: Fach-Begriffe in Klartext (Wirkungs-Ehrlichkeit wie im Backlog-Review).
    private static IReadOnlyList<ReviewGlossaryEntry> Glossary() =>
    [
        new("CREATE_ISSUE", "WIRKT: legt ein neues GitHub-Issue an (nur nach ausgefuehrter Duplikat-Suche zulaessig)."),
        new("UPDATE_ISSUE", "WIRKT: patcht Titel/Body des bestehenden Issues — Vorher/Nachher am Item pruefen!"),
        new("COMMENT", "WIRKT: schreibt einen Kommentar ans bestehende Issue."),
        new("LINK", "WIRKT nur im Core: ordnet dem PBI ein bestehendes Issue zu (Mapping implemented_by_issue) — kein GitHub-Write."),
        new("NO_CHANGE", "Keine Aktion — PBI und Issue sind synchron."),
        new("FLAG_DRIFT", "Nur Marker, keine Aktion: Issue-Zustand passt nicht zum PBI (z. B. geschlossen bei aktivem PBI) — manuell pruefen."),
        new("HOLD_BLOCKED", "Nur Marker, keine Aktion: PBI wartet auf eine blockierende Entscheidung — bewusst KEIN Issue."),
        new("HOLD_CLARIFY", "Nur Marker, keine Aktion: neues, noch unklares PBI — geparkt statt Auto-CREATE."),
        new("deterministic", "Regelbasiert aus Core-Mapping + Snapshot erzeugt — kein LLM beteiligt."),
        new("agent", "Vom Forward-Agenten vorgeschlagen (Suche + Evidenz) — deshalb dieses Review."),
        new("Sync-Metadaten", "Fusszeile im Issue-Body: aus welchem internen Projekt-Zustand das Issue erzeugt wurde — Beleg fuer die Nachvollziehbarkeit, kein Arbeitsauftrag."),
        new("needs_clarify", "Interner PBI-Zustand: Inhalt noch unklar — im Projekt-Core geparkt, bis er geschaerft ist."),
        new("blocked_by_decision", "Interner PBI-Zustand: eine offene Entscheidung blockiert — deshalb wird kein Issue angelegt (HOLD)."),
        new("backlog_ready", "Interner PBI-Zustand: geklaert und planbar.")
    ];

    private static ReviewItem BuildItem(string opId, int idx, GithubForwardOp op,
        IReadOnlyDictionary<int, GithubIssueSnapshot>? issuesByNumber)
    {
        GithubIssueSnapshot? issue = null;
        if (op.TargetIssueNumber is int nr && issuesByNumber is not null) issuesByNumber.TryGetValue(nr, out issue);

        // E0.2f: Titel statt nackter IDs in der Summary.
        var issueRef = op.TargetIssueNumber is null ? "" : $" -> #{op.TargetIssueNumber}"
            + (issue is null ? "" : $" „{Truncate(issue.Title, 80)}“");
        var summary = op.Kind switch
        {
            GithubForwardKind.CreateIssue => $"CREATE_ISSUE fuer {op.PbiId}: {op.Title}",
            GithubForwardKind.UpdateIssue => $"UPDATE_ISSUE {op.PbiId}{issueRef}",
            GithubForwardKind.Comment => $"COMMENT {op.PbiId}{issueRef}",
            GithubForwardKind.Link => $"LINK {op.PbiId}{issueRef}",
            GithubForwardKind.FlagDrift => $"FLAG_DRIFT {op.PbiId}{issueRef}",
            GithubForwardKind.HoldBlocked => $"HOLD_BLOCKED {op.PbiId} (blockiert)",
            GithubForwardKind.HoldClarify => $"HOLD_CLARIFY {op.PbiId} (neu, unklar — geparkt)",
            GithubForwardKind.NoChange => $"NO_CHANGE {op.PbiId}",
            _ => $"{op.Kind} {op.PbiId}"
        };

        var notes = new List<ReviewNote> { new(ReviewNoteKind.Reason, "Begruendung", op.Rationale) };
        // E0.2a: bei UPDATE zaehlt die AENDERUNG — Diff statt zweier Volltexte (Autor-Nachschärfung:
        // „ich sehe nicht, welche Aenderung greifen soll"). Volltexte bleiben in den Drilldowns.
        if (op.Kind == GithubForwardKind.UpdateIssue)
        {
            // R-26-Beleg aus E0.2: needs_clarify heisst "PBI geaendert, Inhalt noch NICHT angeglichen" —
            // ein Update wuerde diesen ungeklaerten Stand nach GitHub spiegeln (z. B. alter Titel vs. neue
            // Requirements). Der Aufloese-Schritt fehlt in der Kette (R-26-C-TODO); bis dahin warnt das Review.
            if (string.Equals(Token(op.Body, "Status"), "needs_clarify", StringComparison.Ordinal))
                notes.Add(new ReviewNote(ReviewNoteKind.Warning, "PBI ungeklaert (needs_clarify)",
                    "Dieses PBI ist als 'geaendert, aber inhaltlich ungeklaert' markiert — Titel/Kriterien sind noch "
                    + "nicht an die neuen Requirements angeglichen. Das Update wuerde einen ungeklaerten Stand nach "
                    + "GitHub spiegeln. Empfehlung: ueberspringen (Begruendung: erst klaeren), dann nach der Klaerung syncen."));
            if (issue is null)
            {
                notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Issue AKTUELL",
                    "(Snapshot nicht verfuegbar — was sich aendert, kann nicht berechnet werden; Drilldown/GitHub pruefen!)"));
                notes.Add(new ReviewNote(ReviewNoteKind.Info, "VORSCHLAG NEU (wuerde so im Issue stehen)",
                    $"{op.Title ?? "(Titel unveraendert)"}\n{Truncate(op.Body ?? "(kein Body im Plan)", BodyPreviewChars)}"));
            }
            else
            {
                notes.Add(BuildUpdateDiff(issue, op));
                // R-30: der Apply PATCHt das labels-Feld — GitHub ERSETZT damit die komplette Label-Liste.
                if (op.Labels is { Count: > 0 }
                    && !op.Labels.OrderBy(x => x).SequenceEqual(issue.Labels.OrderBy(x => x), StringComparer.OrdinalIgnoreCase))
                    notes.Add(new ReviewNote(ReviewNoteKind.Warning, "Labels wuerden ERSETZT (R-30)",
                        $"bisher: {(issue.Labels.Count == 0 ? "(keine)" : string.Join(", ", issue.Labels))} → neu: {string.Join(", ", op.Labels)}"
                        + "\nGitHub ersetzt beim Update die KOMPLETTE Label-Liste — bestehende Labels gehen verloren. Im Zweifel: ueberspringen."));
            }
        }
        // E0.2b: CREATE zeigt den vorgeschlagenen Body.
        if (op.Kind == GithubForwardKind.CreateIssue && !string.IsNullOrWhiteSpace(op.Body))
            notes.Add(new ReviewNote(ReviewNoteKind.Suggestion, "Vorgeschlagener Issue-Body",
                Truncate(op.Body, BodyPreviewChars)));

        if (op.SearchedQueries is { Count: > 0 })
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Suche (Rev-3)", $"{string.Join(" | ", op.SearchedQueries)} — {op.SearchEvidence}"));
        if (!string.IsNullOrWhiteSpace(op.Anchor))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, "Anker", op.Anchor));

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Operation im Detail", $"op:{idx}") };
        if (op.TargetIssueNumber is int target)
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Issue #{target} komplett", $"issue:{target}"));

        var item = new ReviewItem
        {
            ItemId = opId,
            Summary = summary,
            Badge = $"{op.Kind} · {op.Origin}",
            Notes = notes,
            ContextBlocks = context,
            // E0.2c: KEIN Vorentscheid — der Mensch entscheidet aktiv (Accept all nur ueber den Bestaetigungs-Button).
            FieldValues = [new ReviewFieldValue(FieldDecision, ""), new ReviewFieldValue(FieldReason, "")]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // SEMANTISCHER Diff aktuelles Issue vs. Vorschlag (Autor-Nachschärfung: Zeilen-Diff zeigte nur
    // Format-Rauschen, weil der Body komplett neu geschrieben wird). Verglichen wird, was fachlich
    // zaehlt: Titel, Requirement-Deckung, Status/Readiness — Gleiches wird NICHT angezeigt.
    private static ReviewNote BuildUpdateDiff(GithubIssueSnapshot issue, GithubForwardOp op)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(op.Title) && !string.Equals(op.Title.Trim(), issue.Title.Trim(), StringComparison.Ordinal))
        {
            lines.Add($"− Titel bisher: {Truncate(issue.Title, 200)}");
            lines.Add($"+ Titel neu: {Truncate(op.Title, 200)}");
        }
        if (op.Body is not null)
        {
            var oldReqs = ReqIds(issue.Body);
            var newReqs = ReqIds(op.Body);
            foreach (var id in newReqs.Except(oldReqs, StringComparer.Ordinal))
                lines.Add($"+ deckt jetzt zusaetzlich {id} ab");
            foreach (var id in oldReqs.Except(newReqs, StringComparer.Ordinal))
                lines.Add($"− Deckung {id} entfaellt");

            var (oldStatus, newStatus) = (Token(issue.Body, "Status"), Token(op.Body, "Status"));
            if (oldStatus is not null && newStatus is not null && !string.Equals(oldStatus, newStatus, StringComparison.Ordinal))
                lines.Add($"Status: {oldStatus} → {newStatus}");
            var (oldReady, newReady) = (Token(issue.Body, "Readiness"), Token(op.Body, "Readiness"));
            if (oldReady is not null && newReady is not null && !string.Equals(oldReady, newReady, StringComparison.Ordinal))
                lines.Add($"Readiness: {oldReady} → {newReady}");
        }
        if (lines.Count == 0)
            return new ReviewNote(ReviewNoteKind.Info, $"Was aendert sich an Issue #{issue.IssueNumber}?",
                "Keine fachliche Aenderung erkennbar (Titel, Requirement-Deckung, Status und Readiness bleiben gleich)."
                + (BodyEquals(issue.Body, op.Body) ? "" : "\nDer Body-Text wird lediglich neu aus dem aktuellen Core-Stand geschrieben (Format) — Volltexte ueber die Buttons unten."));
        if (!BodyEquals(issue.Body, op.Body))
            lines.Add("Der Body-Text wird komplett neu aus dem aktuellen Core-Stand geschrieben — Volltexte ueber die Buttons unten.");
        return new ReviewNote(ReviewNoteKind.Info, $"Was aendert sich an Issue #{issue.IssueNumber}? (− faellt weg · + kommt neu)",
            string.Join("\n", lines));
    }

    private static readonly System.Text.RegularExpressions.Regex ReqIdRx =
        new(@"\b(?:CAN-)?REQ-\d+\b", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static IReadOnlyList<string> ReqIds(string? text)
        => ReqIdRx.Matches(text ?? "").Select(m => m.Value).Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();

    private static string? Token(string? text, string key)
    {
        var m = System.Text.RegularExpressions.Regex.Match(text ?? "", key + @"[:\s]+([a-z_]+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static bool BodyEquals(string? a, string? b)
        => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.Ordinal);

    private static string DescribeOp(GithubForwardOp op)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{op.Kind} fuer {op.PbiId}{(op.TargetIssueNumber is null ? "" : $" -> Issue #{op.TargetIssueNumber}")}  (origin: {op.Origin})");
        if (!string.IsNullOrWhiteSpace(op.Title)) sb.AppendLine($"Titel: {op.Title}");
        if (op.Labels is { Count: > 0 }) sb.AppendLine($"Labels: {string.Join(", ", op.Labels)}");
        sb.AppendLine($"Begruendung: {op.Rationale}");
        if (!string.IsNullOrWhiteSpace(op.Anchor)) sb.AppendLine($"Anker: {op.Anchor}");
        if (op.SearchedQueries is { Count: > 0 })
        {
            sb.AppendLine($"Such-Queries: {string.Join(" | ", op.SearchedQueries)}");
            if (!string.IsNullOrWhiteSpace(op.SearchEvidence)) sb.AppendLine($"Such-Evidenz: {op.SearchEvidence}");
        }
        if (!string.IsNullOrWhiteSpace(op.Body))
        {
            sb.AppendLine();
            sb.AppendLine("Body (vollstaendig):");
            sb.AppendLine(op.Body);
        }
        return sb.ToString();
    }

    private static string DescribeIssue(GithubIssueSnapshot issue)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Issue #{issue.IssueNumber} ({issue.State}) — {issue.Title}");
        if (issue.Labels.Count > 0) sb.AppendLine($"Labels: {string.Join(", ", issue.Labels)}");
        if (!string.IsNullOrWhiteSpace(issue.Milestone)) sb.AppendLine($"Milestone: {issue.Milestone}");
        if (issue.UpdatedUtc is not null) sb.AppendLine($"Zuletzt geaendert: {issue.UpdatedUtc:u}");
        if (!string.IsNullOrWhiteSpace(issue.Url)) sb.AppendLine(issue.Url);
        sb.AppendLine();
        sb.AppendLine(issue.Body is { Length: > 0 } ? issue.Body : "(Body leer)");
        return sb.ToString();
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max] + " …";

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1
    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1
}
