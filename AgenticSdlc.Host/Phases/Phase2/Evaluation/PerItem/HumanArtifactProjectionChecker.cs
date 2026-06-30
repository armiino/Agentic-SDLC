using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Deterministisch orientierter Checker fuer ArtifactClaims -> Human Markdown Projektion.</summary>
public static partial class HumanArtifactProjectionChecker
{
    public static HumanArtifactProjectionReport Check(
        IReadOnlyList<GeneratedArtifactClaim> claims,
        string markdown)
    {
        var sourceIds = claims
            .SelectMany(c => c.SourceClaimIds)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var markdownRefs = SourceRefRegex()
            .Matches(markdown)
            .Select(m => m.Groups[1].Value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var markdownLines = markdown
            .Split('\n')
            .Select((line, index) => new MarkdownLine(index + 1, line.TrimEnd()))
            .Where(l => !string.IsNullOrWhiteSpace(l.Text))
            .ToList();

        var bulletLines = markdownLines
            .Where(l => l.Text.TrimStart().StartsWith("- ", StringComparison.Ordinal))
            .ToList();

        var unreferencedBullets = bulletLines
            .Where(l => !SourceRefRegex().IsMatch(l.Text))
            .Select(l => new ProjectionLineIssue(l.LineNumber, l.Text, "unreferenced_bullet"))
            .ToList();

        var unknownRefs = markdownRefs
            .Where(id => !sourceIds.Contains(id, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var missingRefs = sourceIds
            .Where(id => !markdownRefs.Contains(id, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var claimResults = claims.Select(c =>
        {
            var lines = markdownLines
                .Where(l => c.SourceClaimIds.Any(id => l.Text.Contains($"[{id}]", StringComparison.OrdinalIgnoreCase)))
                .ToList();
            var statusTerms = StatusTermsFor(c);
            var textTokenOverlap = MaxTokenOverlap(c.Text, lines.Select(l => StripRefs(l.Text)));
            var hasStatusSignal = statusTerms.Count == 0 || lines.Any(l => statusTerms.Any(t => l.Text.Contains(t, StringComparison.OrdinalIgnoreCase)));

            var verdict = lines.Count == 0 ? "projected_missing"
                : textTokenOverlap >= 0.55 && hasStatusSignal ? "projected_paraphrased_ok"
                : textTokenOverlap >= 0.35 ? "projected_weak"
                : "projected_changed";

            return new ProjectionClaimResult(
                c.ArtifactClaimId,
                c.Text,
                c.SourceClaimIds,
                lines.Select(l => l.LineNumber).ToList(),
                lines.Select(l => l.Text).ToList(),
                Math.Round(textTokenOverlap, 4),
                hasStatusSignal,
                verdict);
        }).ToList();

        return new HumanArtifactProjectionReport(
            claims.Count,
            sourceIds,
            markdownRefs,
            missingRefs,
            unknownRefs,
            unreferencedBullets,
            claimResults,
            new
            {
                sourceRefCoverage = Math.Round((sourceIds.Count - missingRefs.Count) / (double)Math.Max(1, sourceIds.Count), 4),
                unknownRefCount = unknownRefs.Count,
                unreferencedBulletCount = unreferencedBullets.Count,
                projectedOk = claimResults.Count(r => r.Verdict == "projected_paraphrased_ok"),
                projectedWeak = claimResults.Count(r => r.Verdict == "projected_weak"),
                projectedMissing = claimResults.Count(r => r.Verdict == "projected_missing"),
                projectedChanged = claimResults.Count(r => r.Verdict == "projected_changed")
            });
    }

    private static IReadOnlyList<string> StatusTermsFor(GeneratedArtifactClaim claim)
    {
        var values = string.Join(' ', claim.Status, claim.Modality, claim.Scope, claim.TimeScope ?? "").ToLowerInvariant();
        var terms = new List<string>();
        if (values.Contains("undecided") || values.Contains("open")) terms.AddRange(["offen", "nicht entschieden", "ungeklärt", "geklaert"]);
        if (values.Contains("not_mvp") || values.Contains("later")) terms.AddRange(["nicht für das mvp", "nicht im mvp", "später", "spaetere"]);
        if (values.Contains("uncertain")) terms.AddRange(["unsicher", "unklar", "keine feste"]);
        if (values.Contains("must") || values.Contains("required")) terms.AddRange(["muss", "erforderlich", "verpflichtend"]);
        return terms.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static double MaxTokenOverlap(string claimText, IEnumerable<string> lines)
    {
        var claimTokens = Tokens(claimText).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (claimTokens.Count == 0) return 0;
        return lines
            .Select(line => Tokens(line).ToHashSet(StringComparer.OrdinalIgnoreCase))
            .DefaultIfEmpty([])
            .Max(lineTokens => claimTokens.Count(t => lineTokens.Contains(t)) / (double)claimTokens.Count);
    }

    private static IEnumerable<string> Tokens(string text)
        => WordRegex()
            .Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(t => t.Length >= 4)
            .Except(["dass", "oder", "eine", "einen", "einer", "sind", "wird", "werden", "muss", "müssen", "fuer", "für"]);

    private static string StripRefs(string text) => SourceRefRegex().Replace(text, "").Trim();

    [GeneratedRegex(@"\[(SC-[A-Z0-9-]+)\]")]
    private static partial Regex SourceRefRegex();

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex WordRegex();

    private sealed record MarkdownLine(int LineNumber, string Text);
}

public sealed record HumanArtifactProjectionReport(
    int ArtifactClaims,
    IReadOnlyList<string> ExpectedSourceClaimIds,
    IReadOnlyList<string> VisibleMarkdownRefs,
    IReadOnlyList<string> MissingRefs,
    IReadOnlyList<string> UnknownRefs,
    IReadOnlyList<ProjectionLineIssue> UnreferencedBullets,
    IReadOnlyList<ProjectionClaimResult> ClaimResults,
    object Metrics);

public sealed record ProjectionLineIssue(int LineNumber, string Text, string Code);

public sealed record ProjectionClaimResult(
    string ArtifactClaimId,
    string ClaimText,
    IReadOnlyList<string> SourceClaimIds,
    IReadOnlyList<int> MarkdownLineNumbers,
    IReadOnlyList<string> MarkdownLines,
    double TokenOverlap,
    bool HasStatusSignal,
    string Verdict);
