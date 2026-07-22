using System.Text.RegularExpressions;

using AgenticSdlc.Host.FullWorkflow.Tore.Github;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

internal static partial class GithubIssueCandidateMatcher
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "der", "die", "das", "den", "dem", "des", "und", "oder", "mit", "fuer", "für", "ohne", "eine", "einer",
        "einem", "einen", "ein", "im", "in", "am", "an", "auf", "von", "zu", "zur", "zum", "als", "pro", "nach",
        "bereitstellen", "umsetzen", "konkretisieren", "klaeren", "klären", "vorbereiten", "muessen", "müssen",
        "werden", "fachlich", "teilweise", "soll"
    };

    public static IReadOnlyList<GithubIssueCandidateMatch> Find(
        IssuePlanItem item,
        IReadOnlyList<GithubIssueSnapshot> issues,
        int limit = 5)
    {
        var itemTokens = Tokens($"{item.Title} {item.Description} {string.Join(' ', item.Labels)}");
        return issues
            .Select(issue => Score(item, itemTokens, issue))
            .Where(match => match.Score >= 4)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Issue.IssueNumber)
            .Take(Math.Clamp(limit, 1, 20))
            .ToArray();
    }

    public static GithubIssueCandidateMatch? Best(IssuePlanItem item, IReadOnlyList<GithubIssueSnapshot> issues)
        => Find(item, issues, limit: 1).FirstOrDefault();

    public static bool IsStrong(GithubIssueCandidateMatch match)
        => match.Score >= 10
           || match.SourceRequirementIds.Count >= 2
           || match.OverlappingTerms.Count >= 2;

    private static GithubIssueCandidateMatch Score(IssuePlanItem item, HashSet<string> itemTokens, GithubIssueSnapshot issue)
    {
        var issueText = $"{issue.Title} {issue.Body} {string.Join(' ', issue.Labels)}";
        var issueTokens = Tokens(issueText);
        var overlap = itemTokens.Intersect(issueTokens, StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        var sourceIds = item.SourceRequirementIds
            .Where(id => issueText.Contains(id, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var score = overlap.Length + sourceIds.Length * 6;
        if (Normalize(issue.Title).Contains(Normalize(item.Title), StringComparison.OrdinalIgnoreCase)
            || Normalize(item.Title).Contains(Normalize(issue.Title), StringComparison.OrdinalIgnoreCase))
            score += 8;

        return new GithubIssueCandidateMatch(issue, score, overlap, sourceIds);
    }

    private static HashSet<string> Tokens(string text)
        => WordRegex().Matches(Normalize(text))
            .Select(m => m.Value)
            .Where(t => t.Length >= 4 && !StopWords.Contains(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string Normalize(string value)
        => (value ?? string.Empty)
            .Replace("[TEST]", "", StringComparison.OrdinalIgnoreCase)
            .Replace("ä", "ae", StringComparison.OrdinalIgnoreCase)
            .Replace("ö", "oe", StringComparison.OrdinalIgnoreCase)
            .Replace("ü", "ue", StringComparison.OrdinalIgnoreCase)
            .Replace("ß", "ss", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

    [GeneratedRegex("[a-z0-9][a-z0-9-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();
}

internal sealed record GithubIssueCandidateMatch(
    GithubIssueSnapshot Issue,
    int Score,
    IReadOnlyList<string> OverlappingTerms,
    IReadOnlyList<string> SourceRequirementIds);
