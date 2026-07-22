using AgenticSdlc.Host.FullWorkflow.Backlog;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.6 — der DETERMINISTISCHE Gegenspieler zum GithubForwardAgent fuer den unmapped-Fall: reiner Keyword-Overlap
// (GithubIssueQueries.Search) statt semantischer Zuordnung. Ueber Schwelle -> LINK, sonst CREATE. Erzeugt dasselbe
// GithubForwardOp-Schema wie der Agent (inkl. searchedQueries/searchEvidence -> gate-valide) -> 1:1 vergleichbar.
// Zweck: zeigen, wo dieser billige Matcher beim Dedup-Recall an semantischem Drift (Paraphrase, entfernte IDs)
// scheitert und der Agent noetig ist — und wo er reicht (klare Treffer / echte Neuanlagen).
public static class GithubForwardDeterministicMatcher
{
    public static GithubForwardOp Match(
        string pbiId,
        string title,
        IReadOnlyList<string> requirementTexts,
        IReadOnlyList<GithubIssueSnapshot> issues,
        int threshold)
    {
        var query = string.Join(' ', new[] { title }.Concat(requirementTexts).Where(s => !string.IsNullOrWhiteSpace(s)));
        var hits = GithubIssueQueries.Search(issues, query, "all", 5);
        var top = hits.FirstOrDefault();

        if (top is not null && top.Score >= threshold)
        {
            return new GithubForwardOp(
                GithubForwardKind.Link, pbiId, top.IssueNumber, null, null, null, null, null,
                Anchor: $"deterministic overlap score={top.Score}: {string.Join(", ", top.OverlappingTerms)}",
                Rationale: $"Keyword-Overlap >= {threshold} mit #{top.IssueNumber}.", Origin: "deterministic");
        }

        var searched = Tokenize(query).Take(8).ToList();
        return new GithubForwardOp(
            GithubForwardKind.CreateIssue, pbiId, null, title, BuildBody(pbiId, requirementTexts),
            Labels: [], SearchedQueries: searched,
            SearchEvidence: $"deterministic: kein Treffer mit Overlap >= {threshold} (bester={top?.Score ?? 0}).",
            Anchor: null, Rationale: "Kein hinreichender Keyword-Overlap — Neuanlage.", Origin: "deterministic");
    }

    private static string BuildBody(string pbiId, IReadOnlyList<string> requirementTexts)
        => $"PBI {pbiId}\n\n" + (requirementTexts.Count == 0 ? "-" : string.Join("\n- ", new[] { "Requirements:" }.Concat(requirementTexts)));

    private static IReadOnlyList<string> Tokenize(string query)
        => query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}
