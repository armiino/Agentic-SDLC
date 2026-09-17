using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

/// <summary>
/// C2d ① Baustein 3 (09.08.2026, c2d-plan §1–§3) — das KOMMENTAR-DESTILLAT: Kommentare sind Diskussionsraum/
/// Evidenz (§10-Flächen-Modell). Der deterministische Collector sammelt je Issue die NEUEN Kommentare seit dem
/// Anker (<see cref="GithubCommentMeta"/>) samt Core-Kontext; der Destillat-Agent (InboundAgent-Schwester,
/// A2+-Zaum: erkunden + genau einmal speichern) deutet sie zu Vorschlägen — er sagt nie „ich ändere das Issue",
/// sondern „daraus ergibt sich vermutlich diese Core-Änderung". Wahrheits-Drafts fahren über den bestehenden
/// <see cref="GithubInboundDeltaBuilder"/> an Tor 1; clarify_answer ist der EXPLIZITE SONDERFALL (§3-4) in die
/// C4-Klärungs-Bahn. Die Wahrheit entsteht ausschließlich an den bestehenden Gates.
/// </summary>
public sealed record GithubCommentFind(
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("title")] string Title,
    // Core-Kontext: gemapptes PBI (Vorrang) ODER adoptierendes Item — null = Issue ohne Core-Anker.
    [property: JsonPropertyName("coreItemId")] string? CoreItemId,
    [property: JsonPropertyName("coreText")] string? CoreText,
    [property: JsonPropertyName("needsClarify")] bool NeedsClarify,
    [property: JsonPropertyName("comments")] IReadOnlyList<GithubIssueCommentSnapshot> Comments);

public static class GithubCommentDisposition
{
    public const string Requirement = GithubInboundDisposition.Requirement;
    public const string Architecture = GithubInboundDisposition.Architecture;
    public const string OpenQuestion = GithubInboundDisposition.OpenQuestion;
    public const string ClarifyAnswer = "clarify_answer";
    public const string Noise = GithubInboundDisposition.Noise;

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        { Requirement, Architecture, OpenQuestion, ClarifyAnswer, Noise };
}

/// <summary>Ein Destillat: commentIds = die tragenden Kommentare (Beleg-Pflicht); targetPbiId nur bei
/// clarify_answer (das needs_clarify-PBI, dessen Frage beantwortet wird).</summary>
public sealed record GithubCommentDraft(
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("commentIds")] IReadOnlyList<long> CommentIds,
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("draftText")] string DraftText,
    [property: JsonPropertyName("rationale")] string Rationale,
    [property: JsonPropertyName("targetPbiId")] string? TargetPbiId = null);

public static class GithubCommentDistill
{
    /// <summary>Deterministischer Collector (LLM-frei): NEUE Kommentare seit Anker, gruppiert je Issue,
    /// mit Core-Kontext. NiC-Issues werden NIE destilliert (§9-Opt-out gilt auch für den Diskussionsraum).</summary>
    public static IReadOnlyList<GithubCommentFind> Collect(
        ProjectStateDocument core,
        IReadOnlyList<GithubIssueSnapshot> issues,
        IReadOnlyList<GithubIssueCommentSnapshot> comments)
    {
        var issueByNumber = issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var mappingByIssue = CoreGithubMapping.CurrentMappings(core)
            .GroupBy(m => m.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var claims = GithubOriginMeta.ClaimsByIssue(core);
        var coreById = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        return GithubCommentMeta.NewSince(core, comments, c => c.IssueNumber, c => c.CommentId)
            // Echo-Schutz: die EIGENEN Vermerk-Kommentare des Systems sind NIE Ernte-Beute — sonst
            // destilliert der Agent die dort zitierten Wahrheits-Sätze als „neue" Aussagen zurück.
            .Where(c => !GithubCommentVermerk.IsSystemVermerk(c.Body))
            .GroupBy(c => c.IssueNumber)
            .Where(g => issueByNumber.TryGetValue(g.Key, out var issue) && !IsNic(issue))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var issue = issueByNumber[g.Key];
                var coreItemId = mappingByIssue.TryGetValue(g.Key, out var m) ? m.PbiId
                    : claims.TryGetValue(g.Key, out var claimant) ? claimant.ItemId : null;
                var coreItem = coreItemId is not null ? coreById.GetValueOrDefault(coreItemId) : null;
                return new GithubCommentFind(g.Key, issue.Title, coreItemId,
                    coreItem?.Text,
                    coreItem?.ReadStatus().Blocker == Blocker.NeedsClarify,
                    g.OrderBy(c => c.CommentId).ToList());
            })
            .ToList();
    }

    private static bool IsNic(GithubIssueSnapshot issue)
        => issue.Title.TrimStart().StartsWith(GithubInboundDetect.NicTitlePrefix, StringComparison.OrdinalIgnoreCase)
           || issue.Labels.Any(l => string.Equals(l, GithubInboundDetect.NicLabel, StringComparison.OrdinalIgnoreCase));

    public static async Task<IReadOnlyList<GithubCommentDraft>> DraftAsync(
        Func<IReadOnlyList<AITool>, AIAgent> factory, IReadOnlyList<GithubCommentFind> finds, CancellationToken ct = default)
    {
        if (finds.Count == 0) return [];
        var tools = new GithubCommentDistillTools(finds);
        var agent = factory(tools.Build());
        await agent.RunAsync([new ChatMessage(ChatRole.User, GithubCommentDistillAgent.Task)], cancellationToken: ct).ConfigureAwait(false);
        if (!tools.Saved)
            throw new InvalidOperationException("COMMENT_DRAFTS_MISSING: Agent hat save_comment_drafts nicht aufgerufen (LAUT statt leerem Ergebnis).");
        return tools.SavedDrafts!;
    }

    /// <summary>Geteilte Konvertierung (Zwei-Bahnen: Zwischenbahn-CLI UND --from-github-Harvest):
    /// Wahrheits-Destillate → Inbound-Drafts fürs EINE Delta (CommentAnchor = höchster tragender Kommentar).</summary>
    public static IReadOnlyList<GithubInboundDraft> ToInboundDrafts(IReadOnlyList<GithubCommentDraft> drafts)
        => drafts
            .Where(d => d.Disposition is GithubInboundDisposition.Requirement
                or GithubInboundDisposition.Architecture or GithubInboundDisposition.OpenQuestion)
            .Select(d => new GithubInboundDraft(d.IssueNumber, d.Disposition, d.DraftText, d.Rationale,
                CommentAnchor: d.CommentIds.DefaultIfEmpty(0).Max()))
            .ToList();

    /// <summary>Geteilte Konvertierung: Kommentar-Funde → synthetische Inbound-Funde (Delta-Metadata-Kontext).</summary>
    public static IReadOnlyList<GithubInboundFind> ToSyntheticFinds(IReadOnlyList<GithubCommentFind> finds)
        => finds
            .Select(f => new GithubInboundFind(GithubInboundCategory.CommentDistill, f.IssueNumber, f.Title,
                f.CoreItemId is not null && f.CoreItemId.StartsWith("PBI-", StringComparison.Ordinal) ? f.CoreItemId : null,
                [$"Kommentar-Destillat ({f.Comments.Count} neue Kommentare)"], null,
                AdoptedItemId: f.CoreItemId is not null && !f.CoreItemId.StartsWith("PBI-", StringComparison.Ordinal) ? f.CoreItemId : null))
            .ToList();

    /// <summary>§3-4 Adapter-Naht: clarify_answer-Drafts → C4-Antworten (die BESTEHENDE Klärungs-Bahn baut
    /// daraus Angleichungs-Vorschläge fürs pbi-update-Gate). AnswerRef = `gh-comment:&lt;issue&gt;#&lt;maxCommentId&gt;`.</summary>
    public static IReadOnlyList<PbiUpdate.ClarifySweepAnswer> ToClarifyAnswers(IReadOnlyList<GithubCommentDraft> drafts)
        => drafts
            .Where(d => d.Disposition == GithubCommentDisposition.ClarifyAnswer && !string.IsNullOrWhiteSpace(d.TargetPbiId))
            .Select(d => new PbiUpdate.ClarifySweepAnswer(d.TargetPbiId!, d.DraftText,
                Quelle: $"github-comment gh#{d.IssueNumber}",
                AnswerRef: $"gh-comment:{d.IssueNumber}#{d.CommentIds.DefaultIfEmpty(0).Max()}"))
            .ToList();
}

internal sealed class GithubCommentDistillTools(IReadOnlyList<GithubCommentFind> finds)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;
    private IReadOnlyList<GithubCommentDraft>? _saved;

    public bool Saved => _saved is not null;
    public IReadOnlyList<GithubCommentDraft>? SavedDrafts => _saved;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetFinds, "get_comment_finds",
            "Die neuen Kommentare je Issue seit dem letzten Anker: issueNumber, titel, coreItemId/coreText "
            + "(das zugehoerige Core-Item; needsClarify=true heisst: es wartet auf eine Klaerungs-Antwort), "
            + "comments (commentId, author, body)."),
        AIFunctionFactory.Create(SaveDrafts, "save_comment_drafts",
            "Speichert die Destillate: je Fund MINDESTENS EIN Draft {issueNumber, commentIds (die tragenden "
            + "Kommentare!), disposition (requirement|architecture|open_question|clarify_answer|noise), "
            + "draftText (praezise deutsche Aussage; bei noise leer erlaubt), rationale, targetPbiId (NUR bei "
            + "clarify_answer: das needs_clarify-PBI)}. Reines Geplauder = noise. Genau einmal aufrufen."),
    ];

    private string GetFinds() => JsonSerializer.Serialize(finds, Json);

    private string SaveDrafts(IReadOnlyList<GithubCommentDraft> drafts)
    {
        if (_saved is not null) return "FEHLER: bereits gespeichert (save_comment_drafts ist einmalig).";
        var findByIssue = finds.ToDictionary(f => f.IssueNumber);
        var errors = new List<string>();
        foreach (var d in drafts)
        {
            if (!findByIssue.TryGetValue(d.IssueNumber, out var find))
            { errors.Add($"Draft fuer unbekanntes Issue #{d.IssueNumber}."); continue; }
            if (!GithubCommentDisposition.All.Contains(d.Disposition))
                errors.Add($"#{d.IssueNumber}: unbekannte disposition '{d.Disposition}'.");
            if (d.Disposition != GithubCommentDisposition.Noise && string.IsNullOrWhiteSpace(d.DraftText))
                errors.Add($"#{d.IssueNumber}: draftText fehlt (nur bei noise erlaubt).");
            if (d.Disposition == GithubCommentDisposition.ClarifyAnswer && string.IsNullOrWhiteSpace(d.TargetPbiId))
                errors.Add($"#{d.IssueNumber}: clarify_answer braucht targetPbiId.");
            var known = find.Comments.Select(c => c.CommentId).ToHashSet();
            if (d.CommentIds.Count == 0 || d.CommentIds.Any(id => !known.Contains(id)))
                errors.Add($"#{d.IssueNumber}: commentIds muessen die tragenden Kommentare des Funds sein (Beleg-Pflicht).");
        }
        foreach (var missing in findByIssue.Keys.Except(drafts.Select(d => d.IssueNumber)))
            errors.Add($"Fund #{missing} hat KEINEN Draft (jeder Fund braucht mindestens einen; Rest = noise).");
        if (errors.Count > 0) return "FEHLER:\n- " + string.Join("\n- ", errors);
        _saved = drafts;
        return $"OK: {drafts.Count} Destillate gespeichert.";
    }
}

public static class GithubCommentDistillAgent
{
    public const string Task = """
        Destilliere die Issue-Diskussionen: get_comment_finds -> je Fund MINDESTENS EIN Draft mit disposition
        (requirement|architecture|open_question|clarify_answer|noise), den tragenden commentIds und praezisem
        deutschen draftText (belegtreu, nichts erfinden; bei clarify_answer: targetPbiId = das needs_clarify-PBI).
        Du aenderst NIE das Issue und NIE die Wahrheit — du formulierst nur Vorschlaege.
        save_comment_drafts GENAU EINMAL.
        """;
}
