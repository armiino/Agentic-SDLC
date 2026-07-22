using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

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
