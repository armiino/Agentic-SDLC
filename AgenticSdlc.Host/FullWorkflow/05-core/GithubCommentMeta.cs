using System.Globalization;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// C2d ① (09.08.2026, c2d-plan §3-2) — das KOMMENTAR-Gedächtnis als High-Water-Mark in der Wahrheit selbst
/// (KEIN eigener Store, keine zweite Wahrheit): `lastProcessedCommentId` je Issue wohnt dort, wo das Issue
/// schon erinnert ist — an der implemented_by_issue-Mapping-Relation (PBI-gemappte Issues) bzw. am
/// adoptierenden Item (<see cref="GithubOriginMeta"/>-Claim). Gestempelt wird NUR am gated Apply, und
/// Ablehnung stempelt AUCH (§11-Prinzip); das Ablehnungs-WISSEN trägt weiterhin R-35.
/// DOKUMENTIERTE GRENZE: ein Issue OHNE Mapping und OHNE adoptierendes Item (alles abgelehnt, nichts
/// geprägt) hat keinen Core-Anker — seine Kommentare erscheinen beim nächsten Pull ERNEUT, dann mit der
/// R-35-Note „schon einmal abgelehnt". Bewusst so (Leitregel KEIN Auto-Skip): lieber erinnertes
/// Wieder-Vorlegen als ein Anker-Store neben der Wahrheit.
/// </summary>
public static class GithubCommentMeta
{
    public const string Key = "lastProcessedCommentId";

    /// <summary>Metadata-Schlüssel an INCOMING-Delta-Items aus dem Kommentar-Destillat: der höchste tragende
    /// Kommentar. Der gated Apply liest ihn und stempelt <see cref="Key"/> am Core-Anker (IngestionApplyExec).</summary>
    public const string AnchorKey = "githubCommentAnchor";

    /// <summary>issueNumber → letzter verarbeiteter Kommentar (null = nie gestempelt ⇒ alles ist neu).
    /// Mapping-Relation hat Vorrang vor Item-Claim (dieselbe Präzedenz wie der Ernte-Detektor).</summary>
    public static long? LastProcessedFor(ProjectStateDocument core, int issueNumber)
    {
        var fromMapping = MappingRelationsFor(core, issueNumber)
            .Select(r => Parse(r.Metadata.GetValueOrDefault(Key)))
            .FirstOrDefault(v => v is not null);
        if (fromMapping is not null) return fromMapping;

        return GithubOriginMeta.ClaimsByIssue(core).TryGetValue(issueNumber, out var claimant)
            ? Parse(claimant.Metadata.GetValueOrDefault(Key))
            : null;
    }

    /// <summary>Filtert einen Kommentar-Snapshot auf das NEUE seit dem Anker (je Issue eigener Stand).</summary>
    public static IReadOnlyList<T> NewSince<T>(ProjectStateDocument core, IReadOnlyList<T> comments,
        Func<T, int> issueOf, Func<T, long> commentIdOf)
        => comments
            .Where(c => LastProcessedFor(core, issueOf(c)) is not { } mark || commentIdOf(c) > mark)
            .ToList();

    /// <summary>Stempelt den Anker am Core-Anker des Issues (Mapping-Relation, sonst adoptierendes Item).
    /// Monoton — ein älterer Stempel überschreibt nie einen neueren. Stamped=false ⇒ kein Anker vorhanden
    /// (dokumentierte Grenze oben) — der Aufrufer meldet das LAUT statt still zu verlieren.</summary>
    public static (ProjectStateDocument Core, bool Stamped) Stamp(ProjectStateDocument core, int issueNumber, long lastCommentId)
    {
        var relations = core.Relations.ToList();
        var stamped = false;
        for (var i = 0; i < relations.Count; i++)
        {
            if (!IsMappingFor(relations[i], issueNumber)) continue;
            if (Parse(relations[i].Metadata.GetValueOrDefault(Key)) is { } existing && existing >= lastCommentId) return (core, true);
            var meta = new Dictionary<string, string>(relations[i].Metadata, StringComparer.Ordinal)
            { [Key] = lastCommentId.ToString(CultureInfo.InvariantCulture) };
            relations[i] = relations[i] with { Metadata = meta };
            stamped = true;
            break;
        }
        if (stamped)
            return (core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Relations = relations }, true);

        if (GithubOriginMeta.ClaimsByIssue(core).TryGetValue(issueNumber, out var claimant))
        {
            if (Parse(claimant.Metadata.GetValueOrDefault(Key)) is { } existing && existing >= lastCommentId) return (core, true);
            var meta = new Dictionary<string, string>(claimant.Metadata, StringComparer.Ordinal)
            { [Key] = lastCommentId.ToString(CultureInfo.InvariantCulture) };
            var items = core.Items
                .Select(i => string.Equals(i.ItemId, claimant.ItemId, StringComparison.Ordinal) ? i with { Metadata = meta } : i)
                .ToList();
            return (core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items }, true);
        }

        return (core, false);
    }

    private static IEnumerable<ProjectStateRelation> MappingRelationsFor(ProjectStateDocument core, int issueNumber)
        => core.Relations.Where(r => IsMappingFor(r, issueNumber));

    private static bool IsMappingFor(ProjectStateRelation r, int issueNumber)
        => string.Equals(r.RelationType, CoreGithubMapping.RelationType, StringComparison.Ordinal)
           && string.Equals(r.ToId, CoreGithubMapping.IssueRef(issueNumber), StringComparison.Ordinal);

    private static long? Parse(string? raw)
        => long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
}
