using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// Einziges live-geteiltes GitHub-Modell (17 tor3-Dateien + decision-Test).
// Die reconciliation-only Modelle (GithubReconciliationInput, GithubIssueMapping, GithubActionPlan*)
// liegen seit Move #7 in research/github-reconciliation/GithubReconciliationSharedModels.cs.
public sealed record GithubIssueSnapshot(
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string? Body,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("milestone")] string? Milestone,
    [property: JsonPropertyName("updatedUtc")] DateTime? UpdatedUtc);

// C2d ① (09.08.2026, c2d-plan §3-1): EIN Kommentar im eigenen comments-snapshot — Kommentare sind
// Diskussionsraum/Evidenz (§10-Flächen-Modell), nie Wahrheit; eigenes Artefakt, weil Frische/Pagination/
// Verarbeitung anders ticken als beim Issue-Body. CommentId = GitHubs stabile ID (Anker-Basis).
public sealed record GithubIssueCommentSnapshot(
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("commentId")] long CommentId,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("createdUtc")] DateTime? CreatedUtc,
    [property: JsonPropertyName("body")] string Body);
