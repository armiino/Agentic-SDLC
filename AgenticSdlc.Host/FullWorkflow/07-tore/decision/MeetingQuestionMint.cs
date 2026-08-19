using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// 9g: die EINE Quelle der Frage→DEC-Semantik (Zwei-Bahnen-Regel) — eine im Meeting AUSGESPROCHENE offene Frage
// wird zur ganz normalen offenen Entscheidung im vorhandenen DEC-Topf (Parkplatz, decision-gate, Kangal), statt
// im open-questions-Artefakt zu versanden. Dritte DEC-Herkunft neben Ingest-CONTRADICT und D2-Klaerungsantrag;
// wie beim D2-Mint BEWUSST OHNE Relation (kein Widerspruch, kein Ziel — die Frage steht frei) und ohne PBI-Block.
// Aufrufer: IngestionApply (Betriebs-Loop, Mensch autorisiert am ingest-Gate) + Core-Bootstrap (deterministisch,
// gedeckt durch das Adjudikations-Gate, das die Frage-Claims freigab).
public static class MeetingQuestionMint
{
    // 1d DEC-Origin (18.08., Block-E-Fund ①): die Frage-DEC trägt die ECHTE Bahn ihrer Herkunft — vorher
    // prägte die geteilte Naht IMMER die Meeting-Herkunft (DEC-002 kam aus dem Autor-Diktat und log damit).
    // Die Herkunfts-Achse ist W2-Messmaterial; abgeleitet aus dem Origin des Delta-Items (eine Quelle je Bahn).
    public const string Origin = "MEETING_OPEN_QUESTION";
    public const string OriginAuthor = "AUTHOR_OPEN_QUESTION";
    public const string OriginGithub = "GITHUB_OPEN_QUESTION";

    internal static string QuestionOrigin(ProjectStateItem incoming) => incoming.Origin switch
    {
        "AuthorFront" => OriginAuthor,      // AuthorFrontDelta-Bahn (Diktat)
        "GithubInbound" => OriginGithub,    // GithubInboundDrafting-Bahn (Ernte)
        _ => Origin,                        // Meeting-Kette (Default — die namensgebende Bahn)
    };

    /// <summary>Item-Fabrik: EIN DEC-Item aus einer offenen Frage (I7: sourceRunId = Ausloeser-Lauf;
    /// Delta-Herkunft via ingestedFrom/ingestedFromRun bzw. -Session; Beleg-Kette via claimIds).</summary>
    public static ProjectStateItem NewDecision(
        string decId, string questionText, ProjectStateItem incoming, IReadOnlyList<string> claimIds, string sourceRun)
    {
        var meta = new Dictionary<string, string>(StringComparer.Ordinal) { ["ingestedFrom"] = incoming.ItemId };
        // 1d (Block-E-Fund ①, zweiter Teil): der SourceRunId eines Diktat-Items ist der SESSION-Slug, keine
        // RunId — ehrlich benennen statt als Lauf ausgeben; echte Läufe bleiben ingestedFromRun.
        if (!string.IsNullOrWhiteSpace(incoming.SourceRunId))
            meta[string.Equals(incoming.Origin, "AuthorFront", StringComparison.Ordinal)
                ? "ingestedFromSession" : "ingestedFromRun"] = incoming.SourceRunId;
        // 9i: kam die Frage aus einem GitHub-Issue, traegt die DEC die Herkunft (Ernte-Gedaechtnis; W4-Anker
        // fuer „DEC aufgeloest -> Issue schliessen"). No-op fuer Meeting-/Autor-Fragen — geteilte Naht bleibt neutral.
        GithubOriginMeta.CarryOver(incoming, meta);

        return new ProjectStateItem(
            ItemId: decId, ItemType: "decision", Text: questionText, Origin: QuestionOrigin(incoming), Stage: null, Version: 1,
            SourceRunId: sourceRun, SourceArtifactId: incoming.SourceArtifactId,
            SourceArtifactType: incoming.SourceArtifactType, SourceDecisionId: null, SourceCandidateId: null,
            SourceClaimIds: claimIds, SourceArtifactItemIds: [], Metadata: meta,
            IdentityKey: IdentityKey.From(questionText), History: []).WithStatus(CoreStatus.From("open_decision"));
    }

    /// <summary>Dokument-Ebene (Bootstrap-Bahn): praegt fuer jedes open_question-Item eine DEC. Idempotent per
    /// IdentityKey — eine bereits als DEC bekannte Frage (offen ODER aufgeloest) wird uebersprungen und benannt.</summary>
    public static (ProjectStateDocument Core, IReadOnlyList<string> Minted, IReadOnlyList<string> SkippedKnown) Mint(
        ProjectStateDocument core, IReadOnlyList<ProjectStateItem> questionItems, string sourceRun)
    {
        var knownDecKeys = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.IdentityKey)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.Ordinal);

        var items = core.Items.ToList();
        var nextDec = MaxDecSuffix(items.Select(i => i.ItemId)) + 1;
        var minted = new List<string>();
        var skipped = new List<string>();

        foreach (var q in questionItems)
        {
            if (knownDecKeys.Contains(IdentityKey.From(q.Text))) { skipped.Add($"{q.ItemId}: Frage bereits als DEC bekannt"); continue; }
            var decId = $"DEC-{nextDec++:D3}";
            items.Add(NewDecision(decId, q.Text, q, q.SourceClaimIds, sourceRun));
            knownDecKeys.Add(IdentityKey.From(q.Text));
            minted.Add(decId);
        }

        if (minted.Count == 0) return (core, minted, skipped);
        return (core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items }, minted, skipped);
    }

    private static int MaxDecSuffix(IEnumerable<string> ids)
        => ids.Where(id => id.StartsWith("DEC-", StringComparison.Ordinal))
            .Select(id => id["DEC-".Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse).DefaultIfEmpty(0).Max();
}
