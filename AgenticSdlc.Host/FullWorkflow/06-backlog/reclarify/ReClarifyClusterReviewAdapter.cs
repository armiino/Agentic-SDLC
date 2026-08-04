using AgenticSdlc.HumanReview;
using System.Text;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record ClusterHumanDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<ClusterHumanDecision> Decisions);

public sealed record ClusterHumanDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,   // apply | skip
    [property: JsonPropertyName("reason")] string? Reason);

// Projiziert die vom ReviewAgent vorgeschlagenen OPERATIONEN als anwendbare Vorschlaege in die generische
// HumanReview-UI: ein Item pro Operation, Entscheidung apply/skip. So liefert die UI echten Mehrwert -
// der Mensch nimmt konkrete Fixes an oder verwirft sie, statt IDs von Hand zu tippen. Der spaetere
// ReClarifyClusterApply fuehrt die akzeptierten Operationen deterministisch aus + prueft Coverage neu.
public static class ReClarifyClusterReviewAdapter
{
    public const string FieldDecision = "decision";
    public const string FieldReason = "reason";

    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };

    // Klartext-Basis-Optionen fuers Entscheidungs-Dropdown (interner Value bleibt apply/skip; Label = Klartext).
    private static readonly IReadOnlyList<ReviewOption> BaseDecisionOptions =
        [new("apply", "✓ Übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")];

    private const string GKinds = "Korrektur-Arten";
    private const string GTerms = "Grundbegriffe";

    public static ReviewSession BuildSession(
        string runId,
        CanonicalRequirementsBaseline baseline,
        FeatureClusterSet clusters,
        ReClarifyGateReport gate,
        ClusterReviewReport review,
        string scope)
    {
        _ = scope;
        var byReq = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var byCluster = clusters.Clusters.ToDictionary(c => c.ClusterId, StringComparer.Ordinal);
        var items = review.Operations.Select(op => BuildItem(op, byReq, byCluster, review)).ToList();

        return new ReviewSession
        {
            SessionId = $"l4-re-clarify-{runId}",
            Title = "Feature-Cluster — Korrekturen prüfen",
            Subtitle = review.Operations.Count == 0
                ? $"Kein Korrekturbedarf ({VerdictLabel(review.Verdict)}). Abdeckung={(gate.Pass ? "vollständig" : "LÜCKE")}."
                : $"{review.Operations.Count} vorgeschlagene Korrekturen an der Feature-Gruppierung. Du bestätigst, was übernommen wird.",
            Glossary =
            [
                new ReviewGlossaryEntry("Verschieben (move_core)", "Ein core-Requirement gehört in einen anderen Cluster.", GKinds),
                new ReviewGlossaryEntry("Abspalten (new_cluster)", "Ein Cluster ist zu breit — ein Teil wird ein eigenes Feature.", GKinds),
                new ReviewGlossaryEntry("Zusammenlegen (merge_clusters)", "Zwei Cluster sind eigentlich ein Feature.", GKinds),
                new ReviewGlossaryEntry("Regel anheften / lösen (crosscutting)", "Eine Querschnitt-Regel wird einem Feature als mit-betreffend zugeordnet bzw. die Zuordnung entfernt. Lösen entfernt NUR die Zusatz-Markierung — das Requirement bleibt in seinem Kern-Feature (core), wird also nicht lose.", GKinds),
                new ReviewGlossaryEntry("Feature-Cluster", "Eine Gruppe von Requirements, die zusammen ein fachliches Feature bilden. Wird später in 1..n PBIs geschnitten.", GTerms),
                new ReviewGlossaryEntry("core", "Die Requirements, die ein Feature ausmachen — jedes aktive Requirement ist in genau EINEM Cluster core.", GTerms),
                new ReviewGlossaryEntry("cross-cutting (Querschnitt)", "Eine Regel (Rollen/Rechte, Validierung, Datenschutz), die ein Feature MIT-betrifft, aber Kern eines anderen Clusters ist. Darf in mehreren Clustern auftauchen.", GTerms),
                new ReviewGlossaryEntry("Abdeckung (Coverage)", "Prüfung, dass jedes Requirement in genau einem Cluster core ist — nichts geht verloren, nichts doppelt.", GTerms)
            ],
            Help = BuildHelp(review),
            FieldSchema = Schema(),
            // Deklarierte Experiment-Bulk-Linie: alle noch offenen Vorschläge auf apply (kein Default, Confirm-Dialog).
            BulkAction = new ReviewBulkAction(
                "✓ Alle akzeptieren (Experiment — ohne Einzelprüfung)",
                [new ReviewFieldValue(FieldDecision, "apply")],
                "Alle noch offenen Korrekturen OHNE Einzelprüfung übernehmen? Das kürzt die bewusste Absegnung der Feature-Struktur ab (Experiment-Modus). Bereits gesetzte Entscheidungen bleiben unberührt."),
            Items = items
        };
    }

    // E0.9-P2a: Ablehnen braucht ein begründetes Warum (Muster: GithubForwardReviewAdapter.Resolved).
    public static bool Resolved(ReviewItem item)
        => ReviewFields.ResolvedRequiringReason(item, Decisions, FieldDecision, FieldReason, "skip"); // Basis-W2

    public static ClusterHumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(item => new ClusterHumanDecision(
            OpId: item.ItemId,
            Decision: FieldOf(item, FieldDecision),
            Reason: FieldOf(item, FieldReason) is { Length: > 0 } r ? r : null)).ToList();
        return new ClusterHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, ClusterHumanDecisionsFile? file)
        => ReviewMerge.ByItemId(session, file?.Decisions, d => d.OpId, (item, decision) =>
        {
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldReason, decision.Reason);
        }, Resolved);

    public static string ResolveContext(string resolverKey, CanonicalRequirementsBaseline baseline, FeatureClusterSet clusters, ReClarifyGateReport gate, ClusterReviewReport review)
    {
        _ = gate;
        var byReq = baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
        var byCluster = clusters.Clusters.ToDictionary(c => c.ClusterId, StringComparer.Ordinal);

        if (resolverKey.StartsWith("op:", StringComparison.Ordinal))
        {
            var id = resolverKey["op:".Length..];
            var op = review.Operations.FirstOrDefault(o => string.Equals(o.OpId, id, StringComparison.Ordinal));
            return op is null ? $"(Operation {id} nicht gefunden)" : RenderDiff(op, byCluster, byReq);
        }
        if (resolverKey.StartsWith("cluster:", StringComparison.Ordinal))
        {
            var id = resolverKey["cluster:".Length..];
            return byCluster.TryGetValue(id, out var c) ? RenderCluster(c, byReq) : $"(Cluster {id} nicht gefunden)";
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            if (!byReq.TryGetValue(id, out var r)) return $"(Requirement {id} nicht gefunden)";
            return $"{id}:\n>> {r.Title}\n{Truncate(r.Text, 600)}";
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    // Vorher → Nachher als lesbare Karten (renderContextCards: Zeile mit ":" = Überschrift, „>> " = Fokus-Karte, sonst Karte).
    // Spiegelt EXAKT die deterministische Apply-Semantik (ReClarifyClusterApply.TryApply) wider.
    private static string RenderDiff(ClusterOperation op, IReadOnlyDictionary<string, FeatureCluster> byCluster, IReadOnlyDictionary<string, CanonicalRequirement> byReq)
    {
        string CluName(string? id) => id is not null && byCluster.TryGetValue(id, out var c) ? $"{id} ({c.Label})" : id ?? "?";
        IReadOnlyList<string> Core(string? id) => id is not null && byCluster.TryGetValue(id, out var c) ? c.CoreRequirementIds : [];
        IReadOnlyList<string> Cross(string? id) => id is not null && byCluster.TryGetValue(id, out var c) ? c.CrossCuttingRequirementIds : [];
        // Vollständiger Requirement-Text (nur Whitespace normalisiert, NICHT abgeschnitten — der Diff ist die Detail-Ansicht).
        string ReqLine(string id, string prefix = "") => byReq.TryGetValue(id, out var r) ? $"{prefix}{id} · {OneLine(r.Text)}" : $"{prefix}{id}";
        var sb = new StringBuilder();

        // Ein Requirement-Block: jede Zeile = eine Karte (id · Titel). Zusätze via prefix „+ " (neu) / „− " (entfällt).
        void Block(string cluHead, IEnumerable<string> ids, string prefix = "")
        {
            sb.Append(cluHead + ":\n");
            var list = ids.ToList();
            if (list.Count == 0) { sb.Append("(keine)\n"); return; }
            foreach (var id in list) sb.Append(ReqLine(id, prefix) + "\n");
        }

        switch (op.Kind)
        {
            case "move_core":
            {
                var req = op.RequirementId!;
                sb.Append("Verschieben:\n");
                sb.Append(">> " + ReqLine(req) + "\n");
                sb.Append("Vorher:\n");
                Block($"{CluName(op.FromClusterId)} — core", Core(op.FromClusterId));
                Block($"{CluName(op.ToClusterId)} — core", Core(op.ToClusterId));
                sb.Append("Nachher:\n");
                Block($"{CluName(op.FromClusterId)} — core (ohne das Requirement)", Core(op.FromClusterId).Where(r => r != req));
                Block($"{CluName(op.ToClusterId)} — core (Ziel)", Core(op.ToClusterId));
                sb.Append(ReqLine(req, "+ ") + "\n");
                break;
            }
            case "add_crosscutting":
            {
                var req = op.RequirementId!;
                sb.Append("Regel an ein Feature anheften:\n");
                sb.Append(">> " + ReqLine(req) + "\n");
                sb.Append($"Vorher — {CluName(op.ToClusterId)}:\n");
                Block("Querschnitt-Regeln", Cross(op.ToClusterId));
                sb.Append($"Nachher — {CluName(op.ToClusterId)}:\n");
                Block("Querschnitt-Regeln", Cross(op.ToClusterId));
                sb.Append(ReqLine(req, "+ ") + "\n");
                break;
            }
            case "remove_crosscutting":
            {
                var req = op.RequirementId!;
                sb.Append("Querschnitt-Zuordnung lösen:\n");
                sb.Append(">> " + ReqLine(req) + "\n");
                sb.Append($"Vorher — {CluName(op.FromClusterId)}:\n");
                Block("Querschnitt-Regeln", Cross(op.FromClusterId));
                sb.Append($"Nachher — {CluName(op.FromClusterId)}:\n");
                Block("Querschnitt-Regeln", Cross(op.FromClusterId).Where(r => r != req));
                break;
            }
            case "merge_clusters":
            {
                sb.Append($"Zusammenlegen — {CluName(op.FromClusterId)} → {CluName(op.ToClusterId)}:\n");
                sb.Append("Vorher:\n");
                Block($"{CluName(op.FromClusterId)} — core", Core(op.FromClusterId));
                Block($"{CluName(op.ToClusterId)} — core", Core(op.ToClusterId));
                sb.Append("Nachher:\n");
                Block($"{CluName(op.ToClusterId)} — core (behält seine)", Core(op.ToClusterId));
                foreach (var id in Core(op.FromClusterId).Where(r => !Core(op.ToClusterId).Contains(r)))
                    sb.Append(ReqLine(id, "+ ") + "\n");
                sb.Append($">> {CluName(op.FromClusterId)} — wird aufgelöst\n");
                break;
            }
            case "new_cluster":
            {
                var moved = op.CoreRequirementIds;
                var sources = byCluster.Values.Where(c => c.CoreRequirementIds.Any(moved.Contains)).ToList();
                sb.Append($"Abspalten — neues Feature {op.NewClusterId} ({op.Label}):\n");
                sb.Append("Vorher:\n");
                foreach (var c in sources) Block($"{CluName(c.ClusterId)} — core", c.CoreRequirementIds);
                if (sources.Count == 0) sb.Append("(Quell-Cluster der Requirements nicht gefunden)\n");
                sb.Append("Nachher:\n");
                foreach (var c in sources) Block($"{CluName(c.ClusterId)} — core (ohne abgespaltene)", c.CoreRequirementIds.Where(r => !moved.Contains(r)));
                Block($">> {op.NewClusterId} ({op.Label}) — core [neu]", moved, "+ ");
                break;
            }
            default:
                sb.Append($"{op.Kind} ({op.OpId})\n");
                break;
        }
        return sb.ToString();
    }

    private static string RenderCluster(FeatureCluster c, IReadOnlyDictionary<string, CanonicalRequirement> byReq)
    {
        _ = byReq;
        var sb = new StringBuilder();
        sb.Append($"{c.ClusterId} ({c.Label}):\n");
        sb.Append($"core: {(c.CoreRequirementIds.Count == 0 ? "—" : string.Join(", ", c.CoreRequirementIds))}\n");
        sb.Append($"Querschnitt-Regeln: {(c.CrossCuttingRequirementIds.Count == 0 ? "—" : string.Join(", ", c.CrossCuttingRequirementIds))}\n");
        if (!string.IsNullOrWhiteSpace(c.Rationale)) sb.Append($"Begründung: {c.Rationale}\n");
        return sb.ToString();
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["apply", "skip"], Required: true,
            Help: "Korrektur übernehmen (wird beim Apply deterministisch ausgeführt) oder verwerfen.",
            Options: BaseDecisionOptions),
        new ReviewFieldSpec(FieldReason, "Begründung (nur Audit)", ReviewInputType.MultiLine, [], Required: false,
            Help: "PFLICHT beim Nicht-Übernehmen (warum weichst du von der Korrektur ab?) — sonst optional.")
    ];

    private static ReviewHelp BuildHelp(ClusterReviewReport review) => new(
        Title: "Feature-Cluster — Korrekturen prüfen",
        Summary: "Ein Prüf-Agent hat konkrete Korrekturen an der Feature-Gruppierung vorgeschlagen. Du entscheidest je Vorschlag: übernehmen oder verwerfen.",
        Sections:
        [
            new ReviewHelpSection(
                "Was du hier tust",
                """
                Ein Cluster-Agent hat Requirements zu Feature-Clustern gruppiert. Ein zweiter Agent (ReviewAgent)
                hat die Gruppierung geprueft und schlaegt konkrete Fixes als OPERATIONEN vor (z.B. ein Querschnitt-
                Requirement einem Feature zufuegen, ein falsch verschmolzenes Feature ausgliedern).

                Du nimmst jede Korrektur an (übernehmen) oder verwirfst sie — beim Verwerfen ist eine kurze Begründung
                Pflicht. Der Apply führt die übernommenen Korrekturen deterministisch aus und prüft danach erneut,
                dass kein Requirement verloren geht (Abdeckung). Die 5 Korrektur-Arten erklärt das Glossar unten.
                """),
            new ReviewHelpSection(
                "Gesamturteil des Prüf-Agenten",
                $"{VerdictLabel(review.Verdict)}. {review.Summary}"),
            new ReviewHelpSection(
                "Was deine Entscheidung bewirkt",
                """
                Dieses Gate schreibt noch NICHTS in den Core. Es formt die Feature-Gruppierung, aus der danach die
                PBIs geschnitten und (erst am späteren Seed, nach einem weiteren Gate) in den Core übernommen werden.
                Zwei Stufen von Wirkung:
                • Verschieben / Abspalten / Zusammenlegen: bestimmt, in WELCHEM Feature ein Requirement/PBI im Core landet.
                • Regel anheften / lösen (cross-cutting): beeinflusst, wie die PBIs geschnitten werden — wird aber NICHT
                  als eigene Core-Beziehung gespeichert (nur als Notiz am Feature).

                Keine Sorge bei „Regel lösen": das entfernt nur die Zusatz-Markierung „berührt dieses Feature auch".
                Das Requirement bleibt in seinem KERN-Feature (core) — es wird dadurch nicht lose. Die Coverage-Prüfung
                garantiert, dass jedes Requirement genau einmal core ist.
                """),
            new ReviewHelpSection(
                "Alle akzeptieren (Experiment)",
                "Der Sammel-Button übernimmt alle offenen Korrekturen auf einmal — bewusst OHNE Einzelprüfung. Das ist ein "
                + "deklarierter Experiment-Modus (wie accept-all/replay), NICHT der Normalweg.")
        ]);

    private static ReviewItem BuildItem(ClusterOperation op, IReadOnlyDictionary<string, CanonicalRequirement> byReq, IReadOnlyDictionary<string, FeatureCluster> byCluster, ClusterReviewReport review)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Info, "Wirkung", KindEffect(op.Kind)),
            new(ReviewNoteKind.Suggestion, "Vorgeschlagene Korrektur", Describe(op, byReq, byCluster)),
            new(ReviewNoteKind.Reason, "Warum vorgeschlagen", op.Rationale ?? "(keine)")
        };
        if (!string.IsNullOrWhiteSpace(op.RequirementId) && byReq.TryGetValue(op.RequirementId!, out var req))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Requirement {op.RequirementId}", OneLine(req.Text)));

        // Nur EIN Kontext-Block: der Vorher→Nachher-Diff (er zeigt die betroffenen Requirements mit Titeln).
        // Das Ziel-Requirement steht zusätzlich als Inline-Note (voller Text) — kein doppelter Requirement-Block mehr.
        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Vorher → Nachher", $"op:{op.OpId}") };

        var item = new ReviewItem
        {
            ItemId = op.OpId,
            Summary = Describe(op, byReq, byCluster),
            Badge = KindBadge(op.Kind),
            Notes = notes,
            // Per-Item-Klartext fürs Dropdown je Op-Art (überschreibt die Basis-Options nur für dieses Item).
            FieldOptions = new Dictionary<string, IReadOnlyList<ReviewOption>> { [FieldDecision] = DecisionOptions(op.Kind) },
            ContextBlocks = context,
            // Default bewusst LEER — der Mensch soll die Feature-Struktur aktiv absegnen (wie am Reverse-Gate).
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, ""),
                new ReviewFieldValue(FieldReason, "")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

    // ---- Klartext-Helfer (reine Anzeige) ---------------------------------------------------------------

    private static string VerdictLabel(string verdict) => verdict?.Trim().ToLowerInvariant() switch
    {
        "approve" => "keine Einwände",
        "revise" => "Korrekturen empfohlen",
        "skipped" => "übersprungen (Gate nicht bestanden)",   // R-33 S2: Review läuft nur bei bestandenem Gate
        _ => verdict ?? "?",
    };

    private static string KindBadge(string kind) => kind switch
    {
        "move_core" => "Verschieben",
        "new_cluster" => "Abspalten",
        "merge_clusters" => "Zusammenlegen",
        "add_crosscutting" => "Regel anheften",
        "remove_crosscutting" => "Regel lösen",
        _ => kind
    };

    // Ehrliche, ABGESTUFTE Wirkung (R-33): move/merge/split formen die Core-Feature-Struktur; cross-cutting formt nur
    // das PBI-Schneiden und wird NICHT als Core-Relation gespeichert.
    private static string KindEffect(string kind) => kind switch
    {
        "move_core" => "Verschiebt dieses Requirement in ein anderes Feature — es (und die daraus geschnittenen PBIs) landet im Core unter dem Ziel-Feature.",
        "merge_clusters" => "Legt zwei Feature-Gruppen zu EINER zusammen — alle ihre Requirements landen im Core unter demselben Feature (die andere Gruppe verschwindet).",
        "new_cluster" => "Spaltet ein eigenes Feature ab — die abgespaltenen Requirements bilden im Core ein NEUES Feature (getrennt vom bisherigen).",
        "add_crosscutting" or "remove_crosscutting"
            => "Beeinflusst, wie die PBIs geschnitten werden — wird aber NICHT als eigene Core-Beziehung gespeichert (nur als Notiz am Feature).",
        _ => "—"
    };

    private static IReadOnlyList<ReviewOption> DecisionOptions(string kind) => kind switch
    {
        "move_core" => [new("apply", "✓ Verschiebung übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        "new_cluster" => [new("apply", "✓ Abspaltung übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        "merge_clusters" => [new("apply", "✓ Zusammenlegen übernehmen"), new("skip", "Nicht übernehmen (Begründung Pflicht)")],
        "add_crosscutting" => [new("apply", "✓ Regel anheften"), new("skip", "Nicht anheften (Begründung Pflicht)")],
        "remove_crosscutting" => [new("apply", "✓ Regel lösen"), new("skip", "Nicht lösen (Begründung Pflicht)")],
        _ => BaseDecisionOptions
    };

    private static string Describe(ClusterOperation op, IReadOnlyDictionary<string, CanonicalRequirement> byReq, IReadOnlyDictionary<string, FeatureCluster> byCluster)
    {
        string ReqLabel(string? id) => id is not null && byReq.TryGetValue(id, out var r) ? $"{id} ({Truncate(r.Title, 60)})" : id ?? "?";
        string CluLabel(string? id) => id is not null && byCluster.TryGetValue(id, out var c) ? $"{id} ({c.Label})" : id ?? "?";
        return op.Kind switch
        {
            "add_crosscutting" => $"crossCutting {ReqLabel(op.RequirementId)} zu Cluster {CluLabel(op.ToClusterId)} hinzufuegen",
            "remove_crosscutting" => $"crossCutting {ReqLabel(op.RequirementId)} aus Cluster {CluLabel(op.FromClusterId)} entfernen",
            "move_core" => $"core {ReqLabel(op.RequirementId)} von {CluLabel(op.FromClusterId)} nach {CluLabel(op.ToClusterId)} verschieben",
            "new_cluster" => $"neuen Cluster {op.NewClusterId} '{op.Label}' bilden aus core: {string.Join(", ", op.CoreRequirementIds)}",
            "merge_clusters" => $"Cluster {CluLabel(op.FromClusterId)} in {CluLabel(op.ToClusterId)} zusammenfuehren",
            _ => $"{op.Kind} ({op.OpId})"
        };
    }

    private static string FieldOf(ReviewItem item, string key) => ReviewFields.Of(item, key); // Basis-W1

    private static void Set(ReviewItem item, string key, string? value) => ReviewFields.Set(item, key, value); // Basis-W1

    private static string Truncate(string value, int max) => ReviewFields.TruncateOneLine(value, max); // Basis-W2
    private static string OneLine(string value) => ReviewFields.OneLine(value); // Basis-W2
}
