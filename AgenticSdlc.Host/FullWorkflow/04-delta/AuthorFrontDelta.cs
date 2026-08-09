using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

/// <summary>
/// 3c Autor-Front (09.08.2026, w2-freeze-checkliste) — die DRITTE Evidenz-Front: der Autor diktiert
/// Wahrheits-Kandidaten im Chat; sie werden WÖRTLICH (Quelle `author via steward-chat`) zum Delta im
/// Meeting-Ketten-Vertrag. Danach arbeitet die UNVERÄNDERTE Kette (Tor 1 prägt, Placement, Drafting,
/// Forward) — der Steward prägt NIE selbst. K13: in-process-Builder (GithubInboundDeltaBuilder-Muster);
/// Mächtigkeit = GitHub-Front (requirement + architecture; Fragen = Schritt-4-Adapter, geteilte Naht).
/// </summary>
public sealed record AuthorStatement(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("rationale")] string? Rationale = null);

public static class AuthorFrontDeltaBuilder
{
    public static readonly IReadOnlySet<string> Dispositions =
        new HashSet<string>(StringComparer.Ordinal) { "requirement", "architecture" };

    /// <summary>Validierung LAUT: leerer Text / unbekannte Disposition ⇒ Fehlerliste statt Delta.</summary>
    public static (ProjectStateDocument? Delta, IReadOnlyList<string> Errors) Build(
        IReadOnlyList<AuthorStatement> statements, string sessionName)
    {
        var errors = new List<string>();
        for (var i = 0; i < statements.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(statements[i].Text)) errors.Add($"#{i + 1}: leerer Text.");
            if (!Dispositions.Contains(statements[i].Disposition))
                errors.Add($"#{i + 1}: disposition '{statements[i].Disposition}' (erlaubt: requirement|architecture).");
        }
        if (statements.Count == 0) errors.Add("keine Aussagen.");
        if (errors.Count > 0) return (null, errors);

        var sourceId = $"author-chat:{sessionName}";
        var items = statements.Select((st, i) =>
            new ProjectStateItem(
                $"AF-{i + 1}", st.Disposition, st.Text.Trim(), "AuthorFront", "author_front", 1,
                sessionName, sourceId, "author-statement", null, null, [], [],
                new Dictionary<string, string>
                {
                    ["quelle"] = "author via steward-chat",
                    ["sessionName"] = sessionName,
                    ["rationale"] = st.Rationale ?? "",
                }).WithStatus(CoreStatus.From("baseline"))).ToList();

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
