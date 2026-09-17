using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

/// <summary>
/// C2b (08.08.2026, c2-inbound-plan §2/§5) — der InboundAgent-FALLBACK (K9: bewusst A2+, mehr Spielraum als
/// A1-Knoten, aber aufgaben-gebunden): deutet die deterministischen Ernte-Funde (F1-Freitext/Diffs, F2-Neu-
/// Issues) und draftet je Fund GENAU EINEN Vorschlag mit Disposition. Haus-Muster wie PbiAlignTools:
/// erkunden (get_inbound_finds) + genau einmal speichern (save_inbound_drafts) — der Agent schreibt NIE
/// selbst; die Drafts werden deterministisch zum Delta-Vertrag der Meeting-Kette gebaut (Tore unverändert).
/// </summary>
public sealed record GithubInboundDraft(
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("draftText")] string DraftText,
    [property: JsonPropertyName("rationale")] string Rationale,
    // C2d §3-2: stammt der Draft aus Kommentaren, traegt er den hoechsten tragenden Kommentar als Anker —
    // der gated Apply stempelt damit das Kommentar-Gedaechtnis (GithubCommentMeta). null = Body-/Issue-Draft.
    [property: JsonPropertyName("commentAnchor"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? CommentAnchor = null);

public static class GithubInboundDisposition
{
    public const string Requirement = "requirement";
    public const string Architecture = "architecture";
    public const string OpenQuestion = "open_question";
    public const string Noise = "noise";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        { Requirement, Architecture, OpenQuestion, Noise };
}

internal sealed class GithubInboundDraftTools(IReadOnlyList<GithubInboundFind> finds)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;
    private IReadOnlyList<GithubInboundDraft>? _saved;

    public bool Saved => _saved is not null;
    public IReadOnlyList<GithubInboundDraft>? SavedDrafts => _saved;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetFinds, "get_inbound_finds",
            "Die Ernte-Funde: issueNumber, kategorie (F1=Edit an gemapptem Issue [pbiId gesetzt], F2=Neu-Issue, "
            + "F7=Edit an bereits adoptiertem Issue [adoptedItemId gesetzt]), "
            + "titel, details (deterministischer Diff), statement/akzeptanzkriterien/freitext aus dem geparsten Body."),
        AIFunctionFactory.Create(SaveDrafts, "save_inbound_drafts",
            "Speichert je Fund GENAU EINEN Draft: issueNumber, disposition (requirement|architecture|open_question|noise), "
            + "draftText (die praezise deutsche Aussage fuer die Wahrheit; bei noise leer erlaubt), rationale. Genau einmal aufrufen."),
    ];

    private string GetFinds()
        => JsonSerializer.Serialize(finds.Select(f => new
        {
            issueNumber = f.IssueNumber,
            kategorie = f.Category,
            titel = f.Title,
            pbiId = f.PbiId,
            adoptedItemId = f.AdoptedItemId,
            details = f.Details,
            statement = f.Parsed?.Statement,
            akzeptanzkriterien = f.Parsed?.AcceptanceCriteria,
            freitext = f.Parsed?.FreeText,
        }), Json);

    private string SaveDrafts(IReadOnlyList<GithubInboundDraft> drafts)
    {
        if (_saved is not null) return "FEHLER: bereits gespeichert (save_inbound_drafts ist einmalig).";
        var expected = finds.Select(f => f.IssueNumber).ToHashSet();
        var errors = new List<string>();
        foreach (var d in drafts)
        {
            if (!expected.Contains(d.IssueNumber)) errors.Add($"Draft fuer unbekanntes Issue #{d.IssueNumber}.");
            if (!GithubInboundDisposition.All.Contains(d.Disposition))
                errors.Add($"#{d.IssueNumber}: unbekannte disposition '{d.Disposition}' (erlaubt: requirement|architecture|open_question|noise).");
            if (!string.Equals(d.Disposition, GithubInboundDisposition.Noise, StringComparison.Ordinal) && string.IsNullOrWhiteSpace(d.DraftText))
                errors.Add($"#{d.IssueNumber}: draftText fehlt (nur bei noise erlaubt).");
        }
        foreach (var missing in expected.Except(drafts.Select(d => d.IssueNumber)))
            errors.Add($"Fund #{missing} hat KEINEN Draft (jeder Fund braucht genau einen; nicht Verwertbares = noise).");
        if (drafts.GroupBy(d => d.IssueNumber).Any(g => g.Count() > 1)) errors.Add("Mehrere Drafts fuer dasselbe Issue.");
        if (errors.Count > 0) return "FEHLER:\n- " + string.Join("\n- ", errors);
        _saved = drafts;
        return $"OK: {drafts.Count} Drafts gespeichert.";
    }
}

public static class GithubInboundDraftAgent
{
    public const string Task = """
        Deute die Ernte-Funde von GitHub: get_inbound_finds -> je Fund GENAU EIN Draft mit disposition
        (requirement|architecture|open_question|noise) und praezisem deutschen draftText (belegtreu, nichts
        erfinden). save_inbound_drafts GENAU EINMAL.
        """;

    public static async Task<IReadOnlyList<GithubInboundDraft>> DraftAsync(
        Func<IReadOnlyList<AITool>, AIAgent> factory, IReadOnlyList<GithubInboundFind> finds, CancellationToken ct = default)
    {
        if (finds.Count == 0) return [];
        var tools = new GithubInboundDraftTools(finds);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, Task)], cancellationToken: ct).ConfigureAwait(false);
        if (!tools.Saved)
            throw new InvalidOperationException("INBOUND_DRAFTS_MISSING: Agent hat save_inbound_drafts nicht aufgerufen (LAUT statt leerem Delta).");
        return tools.SavedDrafts!;
    }
}

/// <summary>
/// Deterministischer Bau des Delta-Vertrags (§2: DERSELBE Vertrag wie die Meeting-Kette — die Tore bleiben
/// unverändert). requirement/architecture/open_question-Drafts werden Delta-Items (die Disposition IST der
/// itemType — Fragen fahren seit 9i auf der 9g-Schiene zum DEC-Topf, F3); die Evidenz (Issue-Nr/URL/
/// harvestedHashes) reist als <see cref="GithubOriginMeta"/> in Item-Metadata + sources/provenance und wird
/// beim gated Apply ins Core-Item übernommen (Ernte-Gedächtnis + deterministischer Forward-Link, 9m).
/// noise → nur Report.
/// </summary>
public static class GithubInboundDeltaBuilder
{
    public static ProjectStateDocument Build(
        IReadOnlyList<GithubInboundDraft> drafts,
        IReadOnlyList<GithubInboundFind> finds,
        IReadOnlyList<GithubIssueSnapshot> issues,
        string runId)
    {
        var findByIssue = finds.GroupBy(f => f.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var issueByNumber = issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());

        var items = new List<ProjectStateItem>();
        var sources = new List<ProjectStateSource>();
        var provenance = new List<ProjectStateProvenance>();

        var perIssueCount = new Dictionary<int, int>();
        foreach (var d in drafts.Where(d => d.Disposition is GithubInboundDisposition.Requirement
                         or GithubInboundDisposition.Architecture or GithubInboundDisposition.OpenQuestion)
                     .OrderBy(d => d.IssueNumber))
        {
            if (!issueByNumber.TryGetValue(d.IssueNumber, out var issue)) continue;   // Draft ohne Issue = unmöglich (Save validiert)
            var find = findByIssue.GetValueOrDefault(d.IssueNumber);
            // C2d: eine Diskussion kann MEHRERE Aussagen tragen — eindeutige IDs per Suffix (GH-20, GH-20-2, …).
            var nth = perIssueCount[d.IssueNumber] = perIssueCount.GetValueOrDefault(d.IssueNumber) + 1;
            var itemId = nth == 1 ? $"GH-{d.IssueNumber}" : $"GH-{d.IssueNumber}-{nth}";
            var sourceId = $"github-issue:{d.IssueNumber}";

            var meta = new Dictionary<string, string>
            {
                // §9/§11 + 9i/9m: der gated Apply übernimmt GENAU diese Herkunfts-Felder (GithubOriginMeta.CarryOver)
                // ins Core-Item — Ernte-Gedächtnis + deterministische Forward-Link-Quelle.
                [GithubOriginMeta.IssueNumber] = d.IssueNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [GithubOriginMeta.IssueUrl] = issue.Url ?? "",
                [GithubOriginMeta.HarvestedTitleHash] = GithubProjectionHash.Compute(issue.Title),
                [GithubOriginMeta.HarvestedBodyHash] = GithubProjectionHash.Compute(issue.Body),
                ["inboundCategory"] = find?.Category ?? "",
                ["mappedPbiId"] = find?.PbiId ?? "",
                ["rationale"] = d.Rationale,
            };
            // C2d §3-2: Kommentar-Anker reist mit — der gated Apply stempelt daraus das Gedächtnis.
            if (d.CommentAnchor is { } anchor)
                meta[GithubCommentMeta.AnchorKey] = anchor.ToString(System.Globalization.CultureInfo.InvariantCulture);

            // §5-Statusmodell: die typisierten Achsen sind Pflicht (der laute ReadStatus-Guard wirft sonst
            // beim Serialisieren) — Delta-Items tragen wie in der Meeting-Kette den Status "baseline".
            items.Add(new ProjectStateItem(
                itemId, d.Disposition, d.DraftText.Trim(), "GithubInbound", "github_inbound", 1,
                runId, sourceId, "github-issue", null, null, [], [],
                meta).WithStatus(CoreStatus.From("baseline")));
            if (nth == 1)   // C2d: mehrere Drafts je Issue teilen sich EINE Quelle (kein Duplikat-Source).
                sources.Add(new ProjectStateSource(sourceId, "github-issue", issue.Url ?? $"gh#{d.IssueNumber}", runId,
                    $"GitHub-Issue #{d.IssueNumber}: {issue.Title}"));
            provenance.Add(new ProjectStateProvenance(itemId,
                [new ProjectStateProvenanceLink("github-issue", $"gh#{d.IssueNumber}", "harvested_from", sourceId,
                    new Dictionary<string, string>())]));
        }

        return new ProjectStateDocument("agentic-sdlc", ProjectStateDocument.CurrentSchemaVersion, DateTime.UtcNow,
            sources, items, [], provenance, []);
    }
}
