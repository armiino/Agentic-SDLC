using AgenticSdlc.HumanReview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

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

    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase) { "apply", "skip" };

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
            Title = "L4 Re-Clarify — Cluster-Korrekturen",
            Subtitle = review.Operations.Count == 0
                ? $"ReviewAgent={review.Verdict}: keine Korrektur vorgeschlagen. Coverage-Gate={(gate.Pass ? "pass" : "fail")}."
                : $"{review.Operations.Count} vorgeschlagene Korrekturen (ReviewAgent={review.Verdict}). apply = uebernehmen, skip = verwerfen.",
            Help = BuildHelp(review),
            FieldSchema = Schema(),
            Items = items
        };
    }

    public static bool Resolved(ReviewItem item)
        => Decisions.Contains(FieldOf(item, FieldDecision));

    public static ClusterHumanDecisionsFile Apply(string runId, ReviewSession session)
    {
        var decisions = session.Items.Select(item => new ClusterHumanDecision(
            OpId: item.ItemId,
            Decision: FieldOf(item, FieldDecision),
            Reason: FieldOf(item, FieldReason) is { Length: > 0 } r ? r : null)).ToList();
        return new ClusterHumanDecisionsFile(runId, "human (review-ui)", decisions);
    }

    public static void MergeExistingDecisions(ReviewSession session, ClusterHumanDecisionsFile? file)
    {
        if (file is null) return;
        var byOp = file.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.OpId))
            .GroupBy(d => d.OpId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        foreach (var item in session.Items)
        {
            if (!byOp.TryGetValue(item.ItemId, out var decision)) continue;
            Set(item, FieldDecision, decision.Decision);
            Set(item, FieldReason, decision.Reason);
            item.Resolved = Resolved(item);
        }
    }

    public static string ResolveContext(string resolverKey, CanonicalRequirementsBaseline baseline, FeatureClusterSet clusters, ReClarifyGateReport gate, ClusterReviewReport review)
    {
        _ = gate;
        if (resolverKey.StartsWith("op:", StringComparison.Ordinal))
        {
            var id = resolverKey["op:".Length..];
            var op = review.Operations.FirstOrDefault(o => string.Equals(o.OpId, id, StringComparison.Ordinal));
            return op is null ? $"(Operation {id} nicht gefunden)" : JsonSerializer.Serialize(op, Json);
        }
        if (resolverKey.StartsWith("cluster:", StringComparison.Ordinal))
        {
            var id = resolverKey["cluster:".Length..];
            var c = clusters.Clusters.FirstOrDefault(x => string.Equals(x.ClusterId, id, StringComparison.Ordinal));
            return c is null ? $"(Cluster {id} nicht gefunden)" : JsonSerializer.Serialize(c, Json);
        }
        if (resolverKey.StartsWith("requirement:", StringComparison.Ordinal))
        {
            var id = resolverKey["requirement:".Length..];
            var r = baseline.Requirements.FirstOrDefault(x => string.Equals(x.RequirementId, id, StringComparison.Ordinal));
            return r is null ? $"(Requirement {id} nicht gefunden)" : JsonSerializer.Serialize(r, Json);
        }
        return $"(Unbekannter Kontext: {resolverKey})";
    }

    private static IReadOnlyList<ReviewFieldSpec> Schema() =>
    [
        new ReviewFieldSpec(FieldDecision, "Entscheidung", ReviewInputType.Dropdown,
            ["apply", "skip"], Required: true,
            Help: "apply = Korrektur uebernehmen (wird beim Apply deterministisch ausgefuehrt) · skip = verwerfen"),
        new ReviewFieldSpec(FieldReason, "Begruendung (optional)", ReviewInputType.MultiLine, [], Required: false,
            Help: "Optionale Notiz, warum uebernommen/verworfen.")
    ];

    private static ReviewHelp BuildHelp(ClusterReviewReport review) => new(
        Title: "L4 Re-Clarify Cluster-Korrekturen",
        Summary: "Der ReviewAgent hat konkrete Korrekturen an den Feature-Clustern vorgeschlagen. Du entscheidest je Vorschlag apply/skip.",
        Sections:
        [
            new ReviewHelpSection(
                "Was du hier tust",
                """
                Ein Cluster-Agent hat Requirements zu Feature-Clustern gruppiert. Ein zweiter Agent (ReviewAgent)
                hat die Gruppierung geprueft und schlaegt konkrete Fixes als OPERATIONEN vor (z.B. ein Querschnitt-
                Requirement einem Feature zufuegen, ein falsch verschmolzenes Feature ausgliedern).

                Du nimmst jede Operation an (apply) oder verwirfst sie (skip). Der Apply fuehrt die akzeptierten
                Operationen deterministisch aus und prueft danach erneut, dass kein Requirement verloren geht (Coverage).
                """),
            new ReviewHelpSection(
                "Operationen",
                """
                add_crosscutting:    Eine Querschnitt-Regel (Rollen, Validierung, Datenschutz) wird einem Feature-Cluster als crossCutting zugeordnet.
                remove_crosscutting: Ein falscher crossCutting-Verweis wird entfernt.
                move_core:           Ein Kern-Requirement wandert in den richtigen Cluster.
                new_cluster:         Ein faelschlich verschmolzenes Feature wird ausgegliedert.
                merge_clusters:      Zwei kuenstlich getrennte Cluster werden zusammengefuehrt.
                """),
            new ReviewHelpSection(
                "ReviewAgent-Gesamturteil",
                $"verdict={review.Verdict}. {review.Summary}"),
            new ReviewHelpSection(
                "Auswirkungen",
                "Nach Fertig schreibt die UI human-decisions.json. l4-re-clarify-apply materialisiert die akzeptierten "
                + "Cluster (mit erneutem Coverage-Check) als Basis fuer den spaeteren CLARIFY/CUT-Schritt -> Product Backlog Items.")
        ]);

    private static ReviewItem BuildItem(ClusterOperation op, IReadOnlyDictionary<string, CanonicalRequirement> byReq, IReadOnlyDictionary<string, FeatureCluster> byCluster, ClusterReviewReport review)
    {
        var notes = new List<ReviewNote>
        {
            new(ReviewNoteKind.Suggestion, "Vorgeschlagene Korrektur", Describe(op, byReq, byCluster)),
            new(ReviewNoteKind.Reason, "Begruendung ReviewAgent", op.Rationale ?? "(keine)")
        };
        if (!string.IsNullOrWhiteSpace(op.RequirementId) && byReq.TryGetValue(op.RequirementId!, out var req))
            notes.Add(new ReviewNote(ReviewNoteKind.Info, $"Requirement {op.RequirementId}", $"{req.Title}\n{Truncate(req.Text, 320)}"));

        var context = new List<ContextBlock> { new(ContextBlockKind.Generic, "Operation JSON", $"op:{op.OpId}") };
        foreach (var cid in new[] { op.FromClusterId, op.ToClusterId }.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
            context.Add(new ContextBlock(ContextBlockKind.Generic, $"Cluster {cid}", $"cluster:{cid}"));
        if (!string.IsNullOrWhiteSpace(op.RequirementId))
            context.Add(new ContextBlock(ContextBlockKind.Reference, $"Requirement {op.RequirementId}", $"requirement:{op.RequirementId}"));

        var item = new ReviewItem
        {
            ItemId = op.OpId,
            Summary = Describe(op, byReq, byCluster),
            Badge = op.Kind,
            Notes = notes,
            ContextBlocks = context,
            FieldValues =
            [
                new ReviewFieldValue(FieldDecision, "apply"),
                new ReviewFieldValue(FieldReason, "")
            ]
        };
        item.Resolved = Resolved(item);
        return item;
    }

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

    private static string FieldOf(ReviewItem item, string key)
        => item.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value?.Trim() ?? "";

    private static void Set(ReviewItem item, string key, string? value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    private static string Truncate(string value, int max)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= max ? normalized : normalized[..max] + "...";
    }
}
