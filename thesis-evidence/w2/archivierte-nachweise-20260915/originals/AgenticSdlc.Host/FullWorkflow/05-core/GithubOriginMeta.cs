using System.Globalization;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// 9i/9m (09.08.2026, w2-freeze-checkliste Schritt 4) — die EINE Schlüssel-Quelle der GitHub-HERKUNFT an
/// Wahrheits-Items: der Inbound-Delta-Bau schreibt diese Metadata-Felder ans Incoming, der gated Ingest-Apply
/// trägt sie beim PRÄGEN ins Core-Item über (CarryOver) — damit ist die Wahrheit selbst das Gedächtnis (K12):
/// die Ernte überspringt verarbeitete Issues (ClaimsByIssue), der Forward linkt adoptierte PBIs deterministisch
/// aufs Ursprungs-Issue statt per Such-Match (AdoptedIssueByPbi, schließt 9m: kein Duplikat-Issue mehr).
/// Bewusst NUR am Item-Neu-Prägen (NEW/SUPERSEDE/CONTRADICT/OPEN_QUESTION) — REFINE/RESTATE auf gemappte
/// PBI-Issues sind über die implemented_by_issue-Relation bereits erinnert.
/// </summary>
public static class GithubOriginMeta
{
    public const string IssueNumber = "githubIssueNumber";
    public const string IssueUrl = "githubIssueUrl";
    public const string HarvestedTitleHash = "harvestedTitleHash";
    public const string HarvestedBodyHash = "harvestedBodyHash";

    private static readonly string[] Keys = [IssueNumber, IssueUrl, HarvestedTitleHash, HarvestedBodyHash];

    /// <summary>Trägt die GitHub-Herkunft eines Incomings in die Metadata eines entstehenden Core-Items
    /// über. No-op für Incomings ohne Herkunft (Meeting-/Autor-Bahn) — die geteilte Naht bleibt bahn-neutral.</summary>
    public static void CarryOver(ProjectStateItem incoming, Dictionary<string, string> targetMeta)
    {
        foreach (var key in Keys)
            if (incoming.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                targetMeta[key] = value;
    }

    public static int? IssueNumberOf(ProjectStateItem item)
        => item.Metadata.TryGetValue(IssueNumber, out var raw)
           && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    /// <summary>Ernte-Gedächtnis: issueNumber → das Core-Item, das dieses Issue adoptiert hat (REQ/ARCH/DEC).
    /// Bei mehreren Anspruchsnehmern gewinnt deterministisch die kleinste ItemId (stabil, kein stilles Raten).</summary>
    public static IReadOnlyDictionary<int, ProjectStateItem> ClaimsByIssue(ProjectStateDocument core)
        => core.Items
            .Select(i => (Item: i, Issue: IssueNumberOf(i)))
            .Where(t => t.Issue is not null)
            .GroupBy(t => t.Issue!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Item.ItemId, StringComparer.Ordinal).First().Item);

    /// <summary>9m: pbiId → Ursprungs-Issue-Nummer, abgeleitet aus der bestätigten Deckung (covers → Item mit
    /// GitHub-Herkunft). NUR bei EINDEUTIGKEIT (genau eine distinkte Issue-Nummer unter den gedeckten Items) —
    /// mehrdeutige Fälle bleiben bewusst dem bestehenden Such-Match überlassen (kein stilles Raten).</summary>
    public static IReadOnlyDictionary<string, int> AdoptedIssueByPbi(ProjectStateDocument core)
    {
        var issueByItem = core.Items
            .Select(i => (i.ItemId, Issue: IssueNumberOf(i)))
            .Where(t => t.Issue is not null)
            .ToDictionary(t => t.ItemId, t => t.Issue!.Value, StringComparer.Ordinal);

        return core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal)
                        && issueByItem.ContainsKey(r.ToId))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .Select(g => (PbiId: g.Key, Issues: g.Select(r => issueByItem[r.ToId]).Distinct().ToList()))
            .Where(t => t.Issues.Count == 1)
            .ToDictionary(t => t.PbiId, t => t.Issues[0], StringComparer.Ordinal);
    }
}
