using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

/// <summary>
/// 3c Autor-Front (09.08.2026, w2-freeze-checkliste) — die DRITTE Evidenz-Front: der Autor diktiert
/// Wahrheits-Kandidaten im Chat; sie werden WÖRTLICH (Quelle `author via steward-chat`) zum Delta im
/// Meeting-Ketten-Vertrag. Danach arbeitet die UNVERÄNDERTE Kette (Tor 1 prägt, Placement, Drafting,
/// Forward) — der Steward prägt NIE selbst. K13: in-process-Builder (GithubInboundDeltaBuilder-Muster);
/// Mächtigkeit = GitHub-Front (requirement + architecture + question — 9i: diktierte offene Fragen fahren
/// als itemType open_question auf der 9g-Schiene zum DEC-Topf, ingest-Gate entscheidet).
/// </summary>
public sealed record AuthorStatement(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("rationale")] string? Rationale = null,
    // C2d §3-7 (Herkunfts-Treue im Korrektur-Pfad): stammt der Anstoß aus einem GitHub-Issue (reject am
    // Gate + Neu-Diktat), reist die Issue-Nummer mit — deterministischer Forward-Link, Ernte-Gedächtnis
    // und Abschluss-Vermerk bleiben intakt. null = reines Autor-Thema.
    [property: JsonPropertyName("githubIssueNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? GithubIssueNumber = null);

public static class AuthorFrontDeltaBuilder
{
    public static readonly IReadOnlySet<string> Dispositions =
        new HashSet<string>(StringComparer.Ordinal) { "requirement", "architecture", "question" };

    // 9i: "question" ist Autor-Sprache — im Delta-Vertrag heißt der Typ open_question (9g-Schiene).
    private static string ItemTypeOf(string disposition)
        => string.Equals(disposition, "question", StringComparison.Ordinal) ? "open_question" : disposition;

    /// <summary>Validierung LAUT: leerer Text / unbekannte Disposition ⇒ Fehlerliste statt Delta.
    /// githubOrigin (§3-7, optional): löst eine Issue-Nummer zu vollen Herkunfts-Metadata auf (Url/Hashes
    /// aus dem Snapshot) — der Aufrufer (Steward) liefert die Naht, die Delta-Schicht bleibt github-frei.</summary>
    public static (ProjectStateDocument? Delta, IReadOnlyList<string> Errors) Build(
        IReadOnlyList<AuthorStatement> statements, string sessionName,
        Func<int, IReadOnlyDictionary<string, string>?>? githubOrigin = null)
    {
        var errors = new List<string>();
        for (var i = 0; i < statements.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(statements[i].Text)) errors.Add($"#{i + 1}: leerer Text.");
            if (!Dispositions.Contains(statements[i].Disposition))
                errors.Add($"#{i + 1}: disposition '{statements[i].Disposition}' (erlaubt: requirement|architecture|question).");
        }
        if (statements.Count == 0) errors.Add("keine Aussagen.");
        if (errors.Count > 0) return (null, errors);

        var sourceId = $"author-chat:{sessionName}";
        var items = statements.Select((st, i) =>
        {
            var meta = new Dictionary<string, string>
            {
                ["quelle"] = "author via steward-chat",
                ["sessionName"] = sessionName,
                ["rationale"] = st.Rationale ?? "",
            };
            // §3-7: Issue-Herkunft ans Item — CarryOver am gated Apply trägt sie dann in die Wahrheit.
            if (st.GithubIssueNumber is { } issue)
            {
                meta["githubIssueNumber"] = issue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (githubOrigin?.Invoke(issue) is { } resolved)
                    foreach (var (k, v) in resolved) meta[k] = v;
            }
            return new ProjectStateItem(
                $"AF-{i + 1}", ItemTypeOf(st.Disposition), st.Text.Trim(), "AuthorFront", "author_front", 1,
                sessionName, sourceId, "author-statement", null, null, [], [],
                meta).WithStatus(CoreStatus.From("baseline"));
        }).ToList();

        var delta = new ProjectStateDocument("agentic-sdlc", ProjectStateDocument.CurrentSchemaVersion, DateTime.UtcNow,
            [new ProjectStateSource(sourceId, "author-statement", $"steward-chat/{sessionName}", null,
                "Wörtliches Autor-Diktat (Autor-Front)")],
            items, [],
            items.Select(i => new ProjectStateProvenance(i.ItemId,
                [new ProjectStateProvenanceLink("author", sessionName, "dictated_by", sourceId,
                    new Dictionary<string, string>())])).ToList(),
            []);
        return (delta, []);
    }
}
